import { apiRequest } from "./httpClient.js";

export const authApi = {
  register(request) {
    return apiRequest("/api/users", {
      method: "POST",
      body: request,
    });
  },

  login(request) {
    return apiRequest("/api/sessions", {
      method: "POST",
      body: request,
    });
  },

  getCurrentUser(token) {
    return apiRequest("/api/users/me", {
      token,
    });
  },
};
