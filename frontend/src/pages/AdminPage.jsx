import { MapPinned, RefreshCw, Save, Trash2, Users } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useApp } from "../context/AppContext.jsx";
import { useAuth } from "../context/AuthContext.jsx";
import { USER_ROLES, getRoleNames } from "../models/auth.js";

export function AdminPage() {
  const { adminService } = useApp();
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [tripPlans, setTripPlans] = useState([]);
  const [roleDrafts, setRoleDrafts] = useState({});
  const [isLoading, setIsLoading] = useState(true);
  const [savingUserId, setSavingUserId] = useState("");
  const [deletingUserId, setDeletingUserId] = useState("");
  const [deletingTripPlanId, setDeletingTripPlanId] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const loadAdminData = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const { users: userModels, tripPlans: tripPlanModels } = await adminService.getDashboard();
      setUsers(userModels);
      setTripPlans(tripPlanModels);
      setRoleDrafts(
        Object.fromEntries(userModels.map((user) => [user.id, getPrimaryRole(user)])),
      );
    } catch (loadError) {
      setError(loadError.message || "Admin podaci nisu ucitani.");
    } finally {
      setIsLoading(false);
    }
  }, [adminService]);

  useEffect(() => {
    void loadAdminData();
  }, [loadAdminData]);

  const updateRoleDraft = (userId, role) => {
    setRoleDrafts((current) => ({
      ...current,
      [userId]: role,
    }));
  };

  const saveUserRole = async (userId) => {
    setSavingUserId(userId);
    setError("");
    setMessage("");

    try {
      await adminService.changeUserRole(userId, roleDrafts[userId] ?? USER_ROLES.USER);
      setMessage("Uloga korisnika je sacuvana.");
      await loadAdminData();
    } catch (saveError) {
      setError(saveError.message || "Uloga korisnika nije sacuvana.");
    } finally {
      setSavingUserId("");
    }
  };

  const deleteUser = async (selectedUser) => {
    if (selectedUser.id === currentUser?.id) {
      setError("Ne mozete obrisati sopstveni admin nalog.");
      setMessage("");
      return;
    }

    const confirmed = window.confirm(`Obrisati korisnika "${selectedUser.email}" i sve njegove planove?`);
    if (!confirmed) {
      return;
    }

    setDeletingUserId(selectedUser.id);
    setError("");
    setMessage("");

    try {
      await adminService.deleteUser(selectedUser.id);
      setUsers((current) => current.filter((user) => user.id !== selectedUser.id));
      setMessage("Korisnik je obrisan.");
      await loadAdminData();
    } catch (deleteError) {
      setError(deleteError.message || "Korisnik nije obrisan.");
    } finally {
      setDeletingUserId("");
    }
  };

  const deleteTripPlan = async (tripPlan) => {
    const confirmed = window.confirm(`Obrisati plan "${tripPlan.title}"?`);
    if (!confirmed) {
      return;
    }

    setDeletingTripPlanId(tripPlan.id);
    setError("");
    setMessage("");

    try {
      await adminService.deleteTripPlan(tripPlan.id);
      setTripPlans((current) => current.filter((currentTripPlan) => currentTripPlan.id !== tripPlan.id));
      setMessage("Plan putovanja je obrisan.");
    } catch (deleteError) {
      setError(deleteError.message || "Plan putovanja nije obrisan.");
    } finally {
      setDeletingTripPlanId("");
    }
  };

  return (
    <div className="page-stack admin-page">
      <header className="page-header">
        <div>
          <h1 className="page-title">Admin</h1>
          <p className="page-subtitle">Pregled korisnika i planova putovanja u sistemu.</p>
        </div>
        <button className="btn btn-secondary" disabled={isLoading} onClick={loadAdminData} type="button">
          <RefreshCw className="btn-icon" aria-hidden="true" />
          Osvezi
        </button>
      </header>

      {error ? <p className="alert alert-error">{error}</p> : null}
      {message ? <p className="alert alert-success">{message}</p> : null}

      <section className="section-card">
        <div className="section-header">
          <div>
            <h2 className="section-title section-title-row">
              <Users className="section-title-icon" aria-hidden="true" />
              Korisnici
            </h2>
            <p className="section-subtitle">Osnovni podaci i promena uloge User/Admin.</p>
          </div>
        </div>

        {isLoading ? (
          <p className="empty-state">Ucitavanje korisnika...</p>
        ) : users.length === 0 ? (
          <p className="empty-state">Nema korisnika.</p>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Ime</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Kreiran</th>
                  <th>Promena role</th>
                  <th>Akcije</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => {
                  const roleNames = getRoleNames(user.roles);

                  return (
                    <tr key={user.id}>
                      <td>{user.name || "-"}</td>
                      <td>{user.email}</td>
                      <td>
                        <span className="role-badges">
                          {(roleNames.length > 0 ? roleNames : ["Bez role"]).map((roleName) => (
                            <span className="badge" key={roleName}>
                              {roleName}
                            </span>
                          ))}
                        </span>
                      </td>
                      <td>{formatDate(user.createdAtUtc)}</td>
                      <td>
                        <div className="admin-role-form">
                          <select
                            className="select"
                            onChange={(event) => updateRoleDraft(user.id, event.target.value)}
                            value={roleDrafts[user.id] ?? getPrimaryRole(user)}
                          >
                            <option value={USER_ROLES.USER}>User</option>
                            <option value={USER_ROLES.ADMIN}>Admin</option>
                          </select>
                          <button
                            className="btn btn-primary btn-small"
                            disabled={savingUserId === user.id}
                            onClick={() => saveUserRole(user.id)}
                            type="button"
                          >
                            <Save className="btn-icon" aria-hidden="true" />
                            Sacuvaj
                          </button>
                        </div>
                      </td>
                      <td>
                        <button
                          className="btn btn-danger-soft btn-small"
                          disabled={deletingUserId === user.id || user.id === currentUser?.id}
                          onClick={() => deleteUser(user)}
                          title={user.id === currentUser?.id ? "Ne mozete obrisati svoj nalog" : "Obrisi korisnika"}
                          type="button"
                        >
                          <Trash2 className="btn-icon" aria-hidden="true" />
                          Obrisi
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="section-card">
        <div className="section-header">
          <div>
            <h2 className="section-title section-title-row">
              <MapPinned className="section-title-icon" aria-hidden="true" />
              Planovi putovanja
            </h2>
            <p className="section-subtitle">Svi planovi sa vlasnikom i osnovnim podacima.</p>
          </div>
        </div>

        {isLoading ? (
          <p className="empty-state">Ucitavanje planova...</p>
        ) : tripPlans.length === 0 ? (
          <p className="empty-state">Nema planova putovanja.</p>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Naziv</th>
                  <th>Vlasnik</th>
                  <th>Datumi</th>
                  <th>Budzet</th>
                  <th>Kreiran</th>
                  <th>Akcije</th>
                </tr>
              </thead>
              <tbody>
                {tripPlans.map((tripPlan) => (
                  <tr key={tripPlan.id}>
                    <td>
                      <span className="admin-title-cell">{tripPlan.title}</span>
                      {tripPlan.description ? <span className="admin-muted-cell">{tripPlan.description}</span> : null}
                    </td>
                    <td>
                      <span className="admin-title-cell">
                        {tripPlan.ownerName || tripPlan.ownerEmail || "Nepoznat korisnik"}
                      </span>
                      <span className="admin-muted-cell">{tripPlan.ownerEmail || tripPlan.ownerUserId}</span>
                    </td>
                    <td>{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</td>
                    <td>{formatMoney(tripPlan.plannedBudget)}</td>
                    <td>{formatDate(tripPlan.createdAtUtc)}</td>
                    <td>
                      <button
                        className="btn btn-danger-soft btn-small"
                        disabled={deletingTripPlanId === tripPlan.id}
                        onClick={() => deleteTripPlan(tripPlan)}
                        type="button"
                      >
                        <Trash2 className="btn-icon" aria-hidden="true" />
                        Obrisi
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}

function getPrimaryRole(user) {
  const roleNames = getRoleNames(user?.roles);
  return roleNames.includes(USER_ROLES.ADMIN) ? USER_ROLES.ADMIN : USER_ROLES.USER;
}

function formatDate(value) {
  if (!value) {
    return "-";
  }

  return new Date(value).toLocaleDateString();
}

function formatDateRange(startDate, endDate) {
  return `${formatDate(startDate)} - ${formatDate(endDate)}`;
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}
