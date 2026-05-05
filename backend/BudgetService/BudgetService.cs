using System.Fabric;
using BudgetService.Configuration;
using BudgetService.Data;
using BudgetService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;

namespace BudgetService
{
    internal sealed class BudgetService : StatefulService, IBudgetService
    {
        private readonly IBudgetRepository repository;

        public BudgetService(StatefulServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            repository = new BudgetRepository(settings.DefaultConnection);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<List<ExpenseDto>> GetExpensesAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                if (!await UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return new List<ExpenseDto>();
                }

                var expenses = await repository.GetExpensesByTripPlanIdAsync(tripPlanId);
                return expenses.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Expense list failed", exception);
                return new List<ExpenseDto>();
            }
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(Guid tripPlanId, Guid expenseId, Guid userId)
        {
            try
            {
                if (!await UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return null;
                }

                var expense = await repository.GetExpenseByIdAsync(expenseId);
                return expense is null || expense.TripPlanId != tripPlanId ? null : ToDto(expense);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Expense lookup failed", exception);
                return null;
            }
        }

        public async Task<ExpenseDto?> CreateExpenseAsync(Guid tripPlanId, Guid userId, CreateExpenseRequestDto request)
        {
            if (!IsValidCreateRequest(tripPlanId, request))
            {
                return null;
            }

            try
            {
                if (!await UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return null;
                }

                var now = DateTime.UtcNow;
                var category = request.Category.GetValueOrDefault();
                var expense = new ExpenseModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = request.Title.Trim(),
                    Category = category.ToString(),
                    Amount = request.Amount,
                    ExpenseDate = request.ExpenseDate.Date,
                    Description = NormalizeOptionalText(request.Description),
                    CreatedAt = now
                };

                var created = await repository.CreateExpenseAsync(expense);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Expense create failed", exception);
                return null;
            }
        }

        public async Task<ExpenseDto?> UpdateExpenseAsync(
            Guid tripPlanId,
            Guid expenseId,
            Guid userId,
            UpdateExpenseRequestDto request)
        {
            if (!IsValidUpdateRequest(request))
            {
                return null;
            }

            try
            {
                if (!await UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return null;
                }

                var expense = await repository.GetExpenseByIdAsync(expenseId);
                if (expense is null || expense.TripPlanId != tripPlanId)
                {
                    return null;
                }

                expense.Title = request.Title.Trim();
                expense.Category = request.Category.GetValueOrDefault().ToString();
                expense.Amount = request.Amount;
                expense.ExpenseDate = request.ExpenseDate.Date;
                expense.Description = NormalizeOptionalText(request.Description);
                expense.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateExpenseAsync(expense);
                return updated ? ToDto(expense) : null;
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Expense update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteExpenseAsync(Guid tripPlanId, Guid expenseId, Guid userId)
        {
            try
            {
                if (!await UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return Failure("Trip plan was not found.");
                }

                var expense = await repository.GetExpenseByIdAsync(expenseId);
                if (expense is null || expense.TripPlanId != tripPlanId)
                {
                    return Failure("Expense was not found.");
                }

                var deleted = await repository.DeleteExpenseAsync(expenseId);
                return deleted ? Success("Expense deleted.") : Failure("Expense was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Expense delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<BudgetSummaryDto?> GetBudgetSummaryAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var plannedBudget = await repository.GetPlannedBudgetForOwnerAsync(tripPlanId, userId);
                if (plannedBudget is null)
                {
                    return null;
                }

                var totalExpenses = await repository.GetTotalByTripPlanIdAsync(tripPlanId);
                return new BudgetSummaryDto
                {
                    TripPlanId = tripPlanId,
                    PlannedBudget = plannedBudget.Value,
                    TotalExpenses = totalExpenses,
                    RemainingBudget = plannedBudget.Value - totalExpenses
                };
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Budget summary failed", exception);
                return null;
            }
        }

        private async Task<bool> UserOwnsTripPlanAsync(Guid tripPlanId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || userId == Guid.Empty)
            {
                return false;
            }

            var plannedBudget = await repository.GetPlannedBudgetForOwnerAsync(tripPlanId, userId);
            return plannedBudget is not null;
        }

        private static bool IsValidCreateRequest(Guid tripPlanId, CreateExpenseRequestDto request)
        {
            return tripPlanId != Guid.Empty
                && (request.TripPlanId == Guid.Empty || request.TripPlanId == tripPlanId)
                && IsValidExpenseFields(request.Title, request.Category, request.Amount, request.ExpenseDate);
        }

        private static bool IsValidUpdateRequest(UpdateExpenseRequestDto request)
        {
            return IsValidExpenseFields(request.Title, request.Category, request.Amount, request.ExpenseDate);
        }

        private static bool IsValidExpenseFields(
            string title,
            ExpenseCategory? category,
            decimal amount,
            DateTime expenseDate)
        {
            return !string.IsNullOrWhiteSpace(title)
                && category.HasValue
                && Enum.IsDefined(typeof(ExpenseCategory), category.Value)
                && amount >= 0
                && expenseDate != default;
        }

        private static ExpenseDto ToDto(ExpenseModel expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                TripPlanId = expense.TripPlanId,
                Title = expense.Title,
                Category = ParseCategory(expense.Category),
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }

        private static ExpenseCategory ParseCategory(string category)
        {
            return Enum.TryParse<ExpenseCategory>(category, ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(ExpenseCategory), parsed)
                    ? parsed
                    : ExpenseCategory.Other;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static OperationResultDto Success(string message)
        {
            return new OperationResultDto
            {
                Succeeded = true,
                Message = message
            };
        }

        private static OperationResultDto Failure(string message)
        {
            return new OperationResultDto
            {
                Succeeded = false,
                Message = message
            };
        }

        private void LogDatabaseError(string message, Exception exception)
        {
            ServiceEventSource.Current.ServiceMessage(Context, "{0}: {1}", message, exception.Message);
        }
    }
}
