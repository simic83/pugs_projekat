import { createContext, useMemo } from "react";

export const AppContext = createContext({});

export function AppProvider({ children }) {
  const value = useMemo(() => ({}), []);

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

