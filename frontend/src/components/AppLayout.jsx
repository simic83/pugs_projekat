import { LogOut, Luggage, Plane, ShieldCheck, UserRound } from "lucide-react";
import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export function AppLayout() {
  const { isAdmin, logout, user } = useAuth();
  const navigate = useNavigate();
  const displayName = user?.name ?? user?.email ?? "Korisnik";

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="app-layout">
      <header className="app-topbar">
        <Link className="app-brand" to="/">
          <span className="brand-mark" aria-hidden="true">
            <Plane />
          </span>
          <span className="brand-text">
            <span className="brand-title">Travel Planner</span>
            <span className="brand-subtitle">Studentski planer putovanja</span>
          </span>
        </Link>

        <nav aria-label="Glavna navigacija" className="app-nav">
          <NavLink className={({ isActive }) => `app-nav-link${isActive ? " is-active" : ""}`} end to="/">
            <Luggage className="nav-icon" aria-hidden="true" />
            Moji planovi
          </NavLink>
          {isAdmin ? (
            <NavLink className={({ isActive }) => `app-nav-link${isActive ? " is-active" : ""}`} to="/admin">
              <ShieldCheck className="nav-icon" aria-hidden="true" />
              Admin
            </NavLink>
          ) : null}
        </nav>

        <div className="app-user">
          <span className="user-name" title={displayName}>
            <UserRound className="user-icon" aria-hidden="true" />
            <span>{displayName}</span>
          </span>
          <button className="btn btn-secondary" onClick={handleLogout} type="button">
            <LogOut className="btn-icon" aria-hidden="true" />
            Logout
          </button>
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
