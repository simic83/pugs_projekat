namespace TravelPlanner.Contracts.DTOs.Budget;

public sealed class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid TripPlanId { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
