import { apiRequest } from "./httpClient.js";

export const sharingApi = {
  getShares(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/shares`);
  },

  createShare(tripPlanId, request) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/shares`, {
      method: "POST",
      body: request,
    });
  },

  revokeShare(tripPlanId, shareId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/shares/${shareId}`, {
      method: "DELETE",
    });
  },

  getSharedTripPlan(token) {
    return apiRequest(`/api/shares/${token}/trip-plan`, {
      skipAuth: true,
    });
  },

  updateSharedTripPlan(token, request) {
    return apiRequest(`/api/shares/${token}/trip-plan`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },
};
