import { Route, Routes } from "react-router-dom";
import { AppLayout } from "../components/AppLayout.jsx";
import { LoginPage } from "../pages/LoginPage.jsx";
import { RegisterPage } from "../pages/RegisterPage.jsx";
import { SharedTripPlanPage } from "../pages/SharedTripPlanPage.jsx";
import { TripPlansPage } from "../pages/TripPlansPage.jsx";
import { ProtectedRoute } from "./ProtectedRoute.jsx";

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<TripPlansPage />} />
        </Route>
      </Route>
      <Route path="/shared/:token" element={<SharedTripPlanPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  );
}
