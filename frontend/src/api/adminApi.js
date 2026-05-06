import { apiRequest } from "./httpClient.js";

export const adminApi = {
  getUsers() {
    return apiRequest("/api/users");
  },

  changeUserRole(userId, role) {
    return apiRequest(`/api/users/${userId}/role`, {
      method: "PUT",
      body: { role },
    });
  },

  deleteUser(userId) {
    return apiRequest(`/api/users/${userId}`, {
      method: "DELETE",
    });
  },

  getAllTripPlans() {
    return apiRequest("/api/admin/trip-plans");
  },

  deleteTripPlan(tripPlanId) {
    return apiRequest(`/api/admin/trip-plans/${tripPlanId}`, {
      method: "DELETE",
    });
  },
};
