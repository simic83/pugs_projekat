import { apiRequest } from "./httpClient.js";

export const checklistApi = {
  getChecklistItems(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/checklist-items`);
  },

  createChecklistItem(tripPlanId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/checklist-items`, {
      method: "POST",
      body: data,
    });
  },

  updateChecklistItem(tripPlanId, checklistItemId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/checklist-items/${checklistItemId}`, {
      method: "PUT",
      body: data,
    });
  },

  deleteChecklistItem(tripPlanId, checklistItemId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/checklist-items/${checklistItemId}`, {
      method: "DELETE",
    });
  },
};
