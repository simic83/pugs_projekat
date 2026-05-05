import { apiRequest } from "./httpClient.js";

export const budgetApi = {
  getExpenses(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/expenses`);
  },

  getExpense(tripPlanId, expenseId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/expenses/${expenseId}`);
  },

  createExpense(tripPlanId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/expenses`, {
      method: "POST",
      body: data,
    });
  },

  updateExpense(tripPlanId, expenseId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/expenses/${expenseId}`, {
      method: "PUT",
      body: data,
    });
  },

  deleteExpense(tripPlanId, expenseId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/expenses/${expenseId}`, {
      method: "DELETE",
    });
  },

  getBudgetSummary(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/budget`);
  },
};
