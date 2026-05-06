import { apiRequest } from "./httpClient.js";

export const notesApi = {
  getNotes(tripPlanId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/notes`);
  },

  createNote(tripPlanId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/notes`, {
      method: "POST",
      body: data,
    });
  },

  updateNote(tripPlanId, noteId, data) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/notes/${noteId}`, {
      method: "PUT",
      body: data,
    });
  },

  deleteNote(tripPlanId, noteId) {
    return apiRequest(`/api/trip-plans/${tripPlanId}/notes/${noteId}`, {
      method: "DELETE",
    });
  },
};
