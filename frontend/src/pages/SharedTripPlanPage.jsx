import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { sharingApi } from "../api/sharingApi.js";
import { ActivityCalendar, buildActivityCalendarDays } from "../components/ActivityCalendar.jsx";
import { EXPENSE_CATEGORIES } from "../models/budget.js";
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
      setError("Link nije validan, istekao je ili je opozvan.");
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
    } catch {
      setSharedTripPlan(null);
      setError("Link nije validan, istekao je ili je opozvan.");
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

  const tripPlan = sharedTripPlan?.tripPlan;
  const destinations = sharedTripPlan?.destinations ?? [];
  const activities = sharedTripPlan?.activities ?? [];
  const expenses = sharedTripPlan?.expenses ?? [];
  const budgetSummary = sharedTripPlan?.budgetSummary;
  const checklistItems = sharedTripPlan?.checklistItems ?? [];
  const noteItems = Array.isArray(sharedTripPlan?.notes) ? sharedTripPlan.notes : [];
  const hasNotesSection = Array.isArray(sharedTripPlan?.notes);
  const accessLevel = sharedTripPlan?.accessLevel ?? sharedTripPlan?.share?.accessLevel;
  const canEdit = Number(accessLevel) === SHARE_ACCESS_LEVELS.EDIT;
  const plannedBudget = budgetSummary?.plannedBudget ?? tripPlan?.plannedBudget ?? 0;
  const totalExpenses = budgetSummary?.totalExpenses ?? expenses.reduce((total, expense) => total + Number(expense.amount ?? 0), 0);
  const remainingBudget = budgetSummary?.remainingBudget ?? plannedBudget - totalExpenses;

  const sortedDestinations = useMemo(
    () => [...destinations].sort((first, second) => compareDates(first.arrivalDate, second.arrivalDate)),
    [destinations],
  );

  const activityCalendarDays = useMemo(
    () => buildActivityCalendarDays(tripPlan?.startDate, tripPlan?.endDate, activities),
    [activities, tripPlan?.endDate, tripPlan?.startDate],
  );

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

  return (
    <div className="public-layout">
      <header className="public-topbar">
        <Link className="public-brand" to="/login">
          <span className="brand-mark">TP</span>
          <span className="brand-text">
            <span className="brand-title">Travel Planner</span>
            <span className="brand-subtitle">Deljeni plan putovanja</span>
          </span>
        </Link>
        <Link className="btn btn-secondary" to="/login">
          Login
        </Link>
      </header>

      <main className="public-main page-stack">
        {isLoading ? <div className="empty-state">Ucitavanje deljenog plana...</div> : null}
        {error ? <p className="alert alert-error">{error}</p> : null}
        {message ? <p className="alert alert-success">{message}</p> : null}

        {tripPlan ? (
          <>
            <header className="page-header">
              <div>
                <h1 className="page-title">{tripPlan.title}</h1>
                <p className="page-subtitle">{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</p>
              </div>
              <span className="badge">{canEdit ? "EDIT pristup" : "VIEW pristup"}</span>
            </header>

            <section className="public-grid">
              <div className="page-stack">
                <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Osnovni podaci</h2>
                    <p className="section-subtitle">
                      {canEdit ? "Osnovni podaci se mogu izmeniti preko ovog linka." : "Link je samo za pregled."}
                    </p>
                  </div>
                </div>

                {canEdit ? (
                  <form className="form-grid" onSubmit={updateSharedTripPlan}>
                    <label className="field">
                      <span className="field-label">Naziv plana</span>
                      <input
                        className="input"
                        name="title"
                        onChange={updateTripPlanField}
                        required
                        value={tripPlanForm.title}
                      />
                    </label>

                    <label className="field">
                      <span className="field-label">Opis</span>
                      <textarea
                        className="textarea"
                        name="description"
                        onChange={updateTripPlanField}
                        rows={3}
                        value={tripPlanForm.description}
                      />
                    </label>

                    <div className="form-row">
                      <label className="field">
                        <span className="field-label">Pocetak</span>
                        <input
                          className="input"
                          name="startDate"
                          onChange={updateTripPlanField}
                          required
                          type="date"
                          value={tripPlanForm.startDate}
                        />
                      </label>
                      <label className="field">
                        <span className="field-label">Kraj</span>
                        <input
                          className="input"
                          name="endDate"
                          onChange={updateTripPlanField}
                          required
                          type="date"
                          value={tripPlanForm.endDate}
                        />
                      </label>
                    </div>

                    <label className="field">
                      <span className="field-label">Planirani budzet</span>
                      <input
                        className="input"
                        min="0"
                        name="plannedBudget"
                        onChange={updateTripPlanField}
                        step="0.01"
                        type="number"
                        value={tripPlanForm.plannedBudget}
                      />
                    </label>

                    <label className="field">
                      <span className="field-label">Napomene</span>
                      <textarea
                        className="textarea"
                        name="notes"
                        onChange={updateTripPlanField}
                        rows={3}
                        value={tripPlanForm.notes}
                      />
                    </label>

                    <button className="btn btn-primary" type="submit">
                      Sacuvaj izmene
                    </button>
                  </form>
                ) : (
                  <div className="item-list">
                    {tripPlan.description ? <p className="list-item-description">{tripPlan.description}</p> : null}
                    {tripPlan.notes ? <p className="note-box">{tripPlan.notes}</p> : null}
                    <div className="public-summary">
                      <span className="stat-tile">
                        <span className="stat-label">Planirani budzet</span>
                        <span className="stat-value">{formatMoney(tripPlan.plannedBudget)}</span>
                      </span>
                      <span className="stat-tile">
                        <span className="stat-label">Period</span>
                        <span className="stat-value">{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</span>
                      </span>
                    </div>
                  </div>
                )}
              </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title">Troskovi i budzet</h2>
                      <p className="section-subtitle">Planirani budzet, ukupni troskovi i preostali iznos.</p>
                    </div>
                    <span className="badge">{expenses.length}</span>
                  </div>

                  <div className="overview-grid">
                    <span className="stat-tile">
                      <span className="stat-label">Planirani budzet</span>
                      <span className="stat-value">{formatMoney(plannedBudget)}</span>
                    </span>
                    <span className="stat-tile">
                      <span className="stat-label">Ukupno potroseno</span>
                      <span className="stat-value">{formatMoney(totalExpenses)}</span>
                    </span>
                    <span className="stat-tile">
                      <span className="stat-label">Preostali budzet</span>
                      <span className="stat-value">{formatMoney(remainingBudget)}</span>
                    </span>
                  </div>

                  <div className="item-list">
                    {expenses.map((expense) => (
                      <article className="list-item" key={expense.id}>
                        <div className="list-item-main">
                          <span className="list-item-title">{expense.title}</span>
                          <p className="muted">
                            {getExpenseCategoryLabel(expense.category)} - {formatDate(expense.expenseDate)}
                          </p>
                          <p className="muted">{formatMoney(expense.amount)}</p>
                          {expense.description ? <p className="list-item-description">{expense.description}</p> : null}
                        </div>
                      </article>
                    ))}
                    {expenses.length === 0 ? <div className="empty-state">Nema unetih troskova.</div> : null}
                  </div>
                </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title">Checklist</h2>
                      <p className="section-subtitle">Stavke za pripremu i njihov status.</p>
                    </div>
                    <span className="badge">{checklistItems.length}</span>
                  </div>

                  <div className="item-list">
                    {checklistItems.map((checklistItem) => (
                      <article className="list-item checklist-item" key={checklistItem.id}>
                        <label className="checklist-label">
                          <input checked={Boolean(checklistItem.isCompleted)} disabled readOnly type="checkbox" />
                          <span className={`checklist-title${checklistItem.isCompleted ? " is-done" : ""}`}>
                            {checklistItem.title}
                          </span>
                        </label>
                        <span className={`badge${checklistItem.isCompleted ? " badge-success" : " badge-muted"}`}>
                          {checklistItem.isCompleted ? "Zavrseno" : "Nije zavrseno"}
                        </span>
                      </article>
                    ))}
                    {checklistItems.length === 0 ? <div className="empty-state">Nema checklist stavki.</div> : null}
                  </div>
                </section>
              </div>

              <div className="page-stack">
                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title">Destinacije</h2>
                      <p className="section-subtitle">Mesta iz deljenog plana.</p>
                    </div>
                    <span className="badge">{destinations.length}</span>
                  </div>

                  <div className="item-list">
                    {sortedDestinations.map((destination) => (
                      <article className="list-item" key={destination.id}>
                        <div className="list-item-main">
                          <span className="list-item-title">{destination.name}</span>
                          <p className="muted">
                            {destination.location ? `${destination.location} - ` : ""}
                            {formatDateRange(destination.arrivalDate, destination.departureDate)}
                          </p>
                          {destination.description ? (
                            <p className="list-item-description">{destination.description}</p>
                          ) : null}
                        </div>
                      </article>
                    ))}
                    {destinations.length === 0 ? <div className="empty-state">Nema destinacija za prikaz.</div> : null}
                  </div>
                </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title">Aktivnosti</h2>
                      <p className="section-subtitle">Kalendarski prikaz aktivnosti po danima.</p>
                    </div>
                    <span className="badge">{activities.length}</span>
                  </div>

                  <ActivityCalendar days={activityCalendarDays} />
                </section>

                {hasNotesSection ? (
                  <section className="section-card">
                    <div className="section-header">
                      <div>
                        <h2 className="section-title">Beleske</h2>
                        <p className="section-subtitle">Notes stavke vezane za deljeni plan.</p>
                      </div>
                      <span className="badge">{noteItems.length}</span>
                    </div>

                    <div className="item-list">
                      {noteItems.map((note) => (
                        <article className="list-item" key={note.id}>
                          <div className="list-item-main">
                            <span className="list-item-title">{note.title}</span>
                            <p className="muted">
                              {note.updatedAt
                                ? `Izmenjeno: ${formatDateTime(note.updatedAt)}`
                                : `Kreirano: ${formatDateTime(note.createdAt)}`}
                            </p>
                            {note.content ? <p className="list-item-description">{note.content}</p> : null}
                          </div>
                        </article>
                      ))}
                      {noteItems.length === 0 ? <div className="empty-state">Nema beleski.</div> : null}
                    </div>
                  </section>
                ) : null}
              </div>
            </section>
          </>
        ) : null}
      </main>
    </div>
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
  return dateValue ? `${dateValue}T00:00:00` : null;
}

function compareDates(firstValue, secondValue) {
  const firstTime = firstValue ? new Date(firstValue).getTime() : Number.MAX_SAFE_INTEGER;
  const secondTime = secondValue ? new Date(secondValue).getTime() : Number.MAX_SAFE_INTEGER;

  return firstTime - secondTime;
}

function formatDate(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleDateString();
}

function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString();
}

function formatDateRange(startDate, endDate) {
  const formattedStartDate = formatDate(startDate);
  const formattedEndDate = formatDate(endDate);

  if (formattedStartDate && formattedEndDate) {
    return `${formattedStartDate} - ${formattedEndDate}`;
  }

  return formattedStartDate || formattedEndDate || "Bez datuma";
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}
