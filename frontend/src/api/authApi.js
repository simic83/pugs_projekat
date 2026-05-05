import { apiRequest } from "./httpClient.js";

export const authApi = {
  register(request) {
    return apiRequest("/api/users", {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  login(request) {
    return apiRequest("/api/sessions", {
      method: "POST",
      body: request,
      skipAuth: true,
    });
  },

  getCurrentUser(token) {
    return apiRequest("/api/users/me", {
      token,
    });
  },
};
