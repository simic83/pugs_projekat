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

  createSharedDestination(token, request) {
    return apiRequest(`/api/shares/${token}/destinations`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedDestination(token, destinationId, request) {
    return apiRequest(`/api/shares/${token}/destinations/${destinationId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedDestination(token, destinationId) {
    return apiRequest(`/api/shares/${token}/destinations/${destinationId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },

  createSharedActivity(token, request) {
    return apiRequest(`/api/shares/${token}/activities`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedActivity(token, activityId, request) {
    return apiRequest(`/api/shares/${token}/activities/${activityId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedActivity(token, activityId) {
    return apiRequest(`/api/shares/${token}/activities/${activityId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },

  createSharedExpense(token, request) {
    return apiRequest(`/api/shares/${token}/expenses`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedExpense(token, expenseId, request) {
    return apiRequest(`/api/shares/${token}/expenses/${expenseId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedExpense(token, expenseId) {
    return apiRequest(`/api/shares/${token}/expenses/${expenseId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },

  createSharedChecklistItem(token, request) {
    return apiRequest(`/api/shares/${token}/checklist-items`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedChecklistItem(token, checklistItemId, request) {
    return apiRequest(`/api/shares/${token}/checklist-items/${checklistItemId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedChecklistItem(token, checklistItemId) {
    return apiRequest(`/api/shares/${token}/checklist-items/${checklistItemId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },

  createSharedNote(token, request) {
    return apiRequest(`/api/shares/${token}/notes`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedNote(token, noteId, request) {
    return apiRequest(`/api/shares/${token}/notes/${noteId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedNote(token, noteId) {
    return apiRequest(`/api/shares/${token}/notes/${noteId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },

  createSharedReminder(token, request) {
    return apiRequest(`/api/shares/${token}/reminders`, {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  updateSharedReminder(token, reminderId, request) {
    return apiRequest(`/api/shares/${token}/reminders/${reminderId}`, {
      method: "PUT",
      body: request,
      skipAuth: true,
    });
  },

  deleteSharedReminder(token, reminderId) {
    return apiRequest(`/api/shares/${token}/reminders/${reminderId}`, {
      method: "DELETE",
      skipAuth: true,
    });
  },
};
