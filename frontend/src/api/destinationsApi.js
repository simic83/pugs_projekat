import { apiRequest } from "./httpClient.js";

export const destinationsApi = {
  getByTripPlanId(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/destinations`);
  },

  create(tripPlanId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/destinations`, {
      method: "POST",
      body: request,
    });
  },

  update(tripPlanId, destinationId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/destinations/${destinationId}`, {
      method: "PUT",
      body: request,
    });
  },

  remove(tripPlanId, destinationId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/destinations/${destinationId}`, {
      method: "DELETE",
    });
  },
};
