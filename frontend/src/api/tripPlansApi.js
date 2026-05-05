import { apiRequest } from "./httpClient.js";

export const tripPlansApi = {
  getAll() {
    return apiRequest("/api/trip-plans");
  },

  getById(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}`);
  },

  create(request) {
    return apiRequest("/api/trip-plans", {
      method: "POST",
      body: request,
    });
  },

  update(tripPlanId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}`, {
      method: "PUT",
      body: request,
    });
  },

  remove(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}`, {
      method: "DELETE",
    });
  },
};
