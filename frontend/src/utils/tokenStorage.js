const TOKEN_STORAGE_KEY = "travelPlanner.authToken";

export const tokenStorage = {
  getToken() {
    return window.localStorage.getItem(TOKEN_STORAGE_KEY);
  },

  setToken(token) {
    window.localStorage.setItem(TOKEN_STORAGE_KEY, token);
  },

  clearToken() {
    window.localStorage.removeItem(TOKEN_STORAGE_KEY);
  },
};
