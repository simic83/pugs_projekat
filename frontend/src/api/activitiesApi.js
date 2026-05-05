import { apiRequest } from "./httpClient.js";

export const activitiesApi = {
  getByTripPlanId(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/activities`);
  },

  create(tripPlanId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/activities`, {
      method: "POST",
      body: request,
    });
  },

  update(tripPlanId, activityId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/activities/${activityId}`, {
      method: "PUT",
      body: request,
    });
  },

  remove(tripPlanId, activityId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/activities/${activityId}`, {
      method: "DELETE",
    });
  },
};
