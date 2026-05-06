import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export function ProtectedRoute({ children, requireAdmin = false }) {
  const { isAdmin, isLoading, token, user } = useAuth();
  const location = useLocation();

  if (isLoading || (token && !user)) {
    return null;
  }

  if (!token) {
    return <Navigate replace state={{ from: location }} to="/login" />;
  }

  if (requireAdmin && !isAdmin) {
    return <Navigate replace to="/" />;
  }

  return children ?? <Outlet />;
}
