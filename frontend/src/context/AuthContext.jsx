import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { authApi } from "../api/authApi.js";
import { USER_ROLES, createAuthResponseModel, createUserModel, getRoleNames, userHasRole } from "../models/auth.js";
import { tokenStorage } from "../utils/tokenStorage.js";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => tokenStorage.getToken());
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  const persistAuth = useCallback((response) => {
    const authResponse = createAuthResponseModel(response);

    if (!authResponse.accessToken) {
      throw new Error("Authentication response did not include a token.");
    }

    tokenStorage.setToken(authResponse.accessToken);
    setToken(authResponse.accessToken);
    setUser(authResponse.user);
  }, []);

  const login = useCallback(
    async (credentials) => {
      const response = await authApi.login(credentials);
      persistAuth(response);
      return response;
    },
    [persistAuth],
  );

  const register = useCallback(
    async (request) => {
      return authApi.register(request);
    },
    [],
  );

  const logout = useCallback(() => {
    tokenStorage.clearToken();
    setToken(null);
    setUser(null);
  }, []);

  const loadCurrentUser = useCallback(async () => {
    if (!token) {
      setUser(null);
      return null;
    }

    setIsLoading(true);

    try {
      const currentUser = createUserModel(await authApi.getCurrentUser(token));
      setUser(currentUser);
      return currentUser;
    } catch (error) {
      logout();
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, [logout, token]);

  useEffect(() => {
    if (token) {
      void loadCurrentUser().catch(() => {});
    }
  }, [loadCurrentUser, token]);

  const value = useMemo(() => {
    const roles = getRoleNames(user?.roles);

    return {
      user,
      token,
      roles,
      isAdmin: userHasRole(user, USER_ROLES.ADMIN),
      isLoading,
      login,
      register,
      logout,
      loadCurrentUser,
    };
  }, [isLoading, loadCurrentUser, login, logout, register, token, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
