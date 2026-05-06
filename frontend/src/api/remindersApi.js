import { apiRequest } from "./httpClient.js";

export const remindersApi = {
  getReminders(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/reminders`);
  },

  createReminder(tripPlanId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/reminders`, {
      method: "POST",
      body: data,
    });
  },

  updateReminder(tripPlanId, reminderId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/reminders/${reminderId}`, {
      method: "PUT",
      body: data,
    });
  },

  deleteReminder(tripPlanId, reminderId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/reminders/${reminderId}`, {
      method: "DELETE",
    });
  },
};
