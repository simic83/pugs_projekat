import { useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { sharingApi } from "../api/sharingApi.js";
import { SHARE_ACCESS_LEVELS } from "../models/sharing.js";

const emptyTripPlanForm = {
  title: "",
  description: "",
  startDate: "",
  endDate: "",
  plannedBudget: "0",
  notes: "",
};

export function SharedTripPlanPage() {
  const { token } = useParams();
  const [sharedTripPlan, setSharedTripPlan] = useState(null);
  const [tripPlanForm, setTripPlanForm] = useState(emptyTripPlanForm);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const loadSharedTripPlan = useCallback(async () => {
    if (!token) {
      setError("Share token is missing.");
      setIsLoading(false);
      return;
    }

    setError("");
    setMessage("");
    setIsLoading(true);

    try {
      const data = await sharingApi.getSharedTripPlan(token);
      setSharedTripPlan(data);
      setTripPlanForm(toTripPlanForm(data?.tripPlan));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadSharedTripPlan();
  }, [loadSharedTripPlan]);

  const updateTripPlanField = (event) => {
    const { name, value } = event.target;
    setTripPlanForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));
  };

  const updateSharedTripPlan = async (event) => {
    event.preventDefault();
    if (!token || !canEdit) {
      return;
    }

    setError("");
    setMessage("");

    try {
      const updated = await sharingApi.updateSharedTripPlan(token, {
        title: tripPlanForm.title,
        description: tripPlanForm.description || null,
        startDate: toDateTime(tripPlanForm.startDate),
        endDate: toDateTime(tripPlanForm.endDate),
        plannedBudget: Number(tripPlanForm.plannedBudget || 0),
        notes: tripPlanForm.notes || null,
      });

      setSharedTripPlan(updated);
      setTripPlanForm(toTripPlanForm(updated?.tripPlan));
      setMessage("Plan je izmenjen.");
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const tripPlan = sharedTripPlan?.tripPlan;
  const destinations = sharedTripPlan?.destinations ?? [];
  const activities = sharedTripPlan?.activities ?? [];
  const canEdit = Number(sharedTripPlan?.share?.accessLevel) === SHARE_ACCESS_LEVELS.EDIT;

  return (
    <main style={styles.page}>
      <header style={styles.header}>
        <div>
          <h1 style={styles.title}>{tripPlan?.title ?? "Shared trip plan"}</h1>
          {tripPlan ? <p style={styles.subtitle}>{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</p> : null}
        </div>
        <Link style={styles.secondaryButton} to="/login">
          Login
        </Link>
      </header>

      {isLoading ? <p style={styles.muted}>Loading...</p> : null}
      {error ? <p style={styles.error}>{error}</p> : null}
      {message ? <p style={styles.success}>{message}</p> : null}

      {tripPlan ? (
        <section style={styles.layout}>
          <section style={styles.panel}>
            <h2 style={styles.sectionTitle}>Plan</h2>
            {canEdit ? (
              <form onSubmit={updateSharedTripPlan} style={styles.form}>
                <input
                  name="title"
                  onChange={updateTripPlanField}
                  required
                  style={styles.input}
                  value={tripPlanForm.title}
                />
                <textarea
                  name="description"
                  onChange={updateTripPlanField}
                  placeholder="Description"
                  rows={3}
                  style={styles.input}
                  value={tripPlanForm.description}
                />
                <div style={styles.twoColumns}>
                  <input
                    name="startDate"
                    onChange={updateTripPlanField}
                    required
                    style={styles.input}
                    type="date"
                    value={tripPlanForm.startDate}
                  />
                  <input
                    name="endDate"
                    onChange={updateTripPlanField}
                    required
                    style={styles.input}
                    type="date"
                    value={tripPlanForm.endDate}
                  />
                </div>
                <input
                  min="0"
                  name="plannedBudget"
                  onChange={updateTripPlanField}
                  step="0.01"
                  style={styles.input}
                  type="number"
                  value={tripPlanForm.plannedBudget}
                />
                <textarea
                  name="notes"
                  onChange={updateTripPlanField}
                  placeholder="Notes"
                  rows={3}
                  style={styles.input}
                  value={tripPlanForm.notes}
                />
                <button style={styles.primaryButton} type="submit">
                  Sacuvaj izmene
                </button>
              </form>
            ) : (
              <div style={styles.list}>
                {tripPlan.description ? <p>{tripPlan.description}</p> : null}
                {tripPlan.notes ? <p style={styles.note}>{tripPlan.notes}</p> : null}
                <p style={styles.muted}>Planned budget: {formatMoney(tripPlan.plannedBudget)}</p>
              </div>
            )}
          </section>

          <section style={styles.panel}>
            <h2 style={styles.sectionTitle}>Destinations</h2>
            <div style={styles.list}>
              {destinations.map((destination) => (
                <div key={destination.id} style={styles.rowItem}>
                  <strong>{destination.name}</strong>
                  <p style={styles.muted}>
                    {destination.location ? `${destination.location} - ` : ""}
                    {formatDateRange(destination.arrivalDate, destination.departureDate)}
                  </p>
                </div>
              ))}
              {destinations.length === 0 ? <p style={styles.muted}>No destinations.</p> : null}
            </div>
          </section>

          <section style={styles.panel}>
            <h2 style={styles.sectionTitle}>Activities</h2>
            <div style={styles.list}>
              {activities.map((activity) => (
                <div key={activity.id} style={styles.rowItem}>
                  <strong>{activity.title}</strong>
                  <p style={styles.muted}>
                    {formatDate(activity.activityDate)}
                    {activity.activityTime ? ` ${String(activity.activityTime).slice(0, 5)}` : ""}
                    {activity.location ? ` - ${activity.location}` : ""}
                  </p>
                  <p style={styles.muted}>{formatMoney(activity.estimatedCost)}</p>
                </div>
              ))}
              {activities.length === 0 ? <p style={styles.muted}>No activities.</p> : null}
            </div>
          </section>
        </section>
      ) : null}
    </main>
  );
}

function toTripPlanForm(tripPlan) {
  if (!tripPlan) {
    return emptyTripPlanForm;
  }

  return {
    title: tripPlan.title ?? "",
    description: tripPlan.description ?? "",
    startDate: toDateInputValue(tripPlan.startDate),
    endDate: toDateInputValue(tripPlan.endDate),
    plannedBudget: String(tripPlan.plannedBudget ?? 0),
    notes: tripPlan.notes ?? "",
  };
}

function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 10);
}

function toDateTime(dateValue) {
  return `${dateValue}T00:00:00`;
}

function formatDate(value) {
  if (!value) {
    return "";
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

const styles = {
  page: {
    minHeight: "100vh",
    padding: "24px",
    background: "#f7f7f8",
    color: "#202027",
  },
  header: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "16px",
    marginBottom: "18px",
  },
  title: {
    margin: 0,
    fontSize: "30px",
  },
  subtitle: {
    margin: "4px 0 0",
    color: "#60606b",
  },
  layout: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 300px), 1fr))",
    gap: "18px",
  },
  panel: {
    display: "grid",
    alignContent: "start",
    gap: "14px",
    padding: "18px",
    border: "1px solid #dedee3",
    borderRadius: "8px",
    background: "#ffffff",
  },
  sectionTitle: {
    margin: 0,
    fontSize: "18px",
  },
  form: {
    display: "grid",
    gap: "10px",
  },
  twoColumns: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 160px), 1fr))",
    gap: "10px",
  },
  input: {
    width: "100%",
    minHeight: "38px",
    boxSizing: "border-box",
    padding: "8px 10px",
    border: "1px solid #c9c9d1",
    borderRadius: "6px",
    font: "inherit",
  },
  primaryButton: {
    minHeight: "40px",
    padding: "0 14px",
    border: 0,
    borderRadius: "6px",
    background: "#1769aa",
    color: "#ffffff",
    cursor: "pointer",
    font: "inherit",
    fontWeight: 700,
  },
  secondaryButton: {
    minHeight: "38px",
    boxSizing: "border-box",
    padding: "8px 14px",
    border: "1px solid #b9bbc6",
    borderRadius: "6px",
    background: "#ffffff",
    color: "#202027",
    cursor: "pointer",
    font: "inherit",
    textDecoration: "none",
  },
  list: {
    display: "grid",
    gap: "10px",
  },
  rowItem: {
    display: "grid",
    gap: "4px",
    padding: "12px",
    border: "1px solid #dedee3",
    borderRadius: "6px",
  },
  note: {
    padding: "10px",
    borderRadius: "6px",
    background: "#f3f4f6",
  },
  muted: {
    margin: 0,
    color: "#60606b",
    fontSize: "14px",
  },
  error: {
    margin: "0 0 16px",
    padding: "10px 12px",
    border: "1px solid #f0b8b2",
    borderRadius: "6px",
    background: "#fff5f4",
    color: "#b42318",
  },
  success: {
    margin: "0 0 16px",
    padding: "10px 12px",
    border: "1px solid #b7d7ee",
    borderRadius: "6px",
    background: "#eef6fd",
    color: "#1769aa",
  },
};
