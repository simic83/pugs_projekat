import { useCallback, useEffect, useMemo, useState } from "react";
import { QRCodeSVG } from "qrcode.react";
import { activitiesApi } from "../api/activitiesApi.js";
import { budgetApi } from "../api/budgetApi.js";
import { checklistApi } from "../api/checklistApi.js";
import { destinationsApi } from "../api/destinationsApi.js";
import { notesApi } from "../api/notesApi.js";
import { sharingApi } from "../api/sharingApi.js";
import { tripPlansApi } from "../api/tripPlansApi.js";
import { ActivityCalendar, buildActivityCalendarDays, groupActivitiesByDate } from "../components/ActivityCalendar.jsx";
import { EXPENSE_CATEGORIES } from "../models/budget.js";
import { SHARE_ACCESS_LEVEL_OPTIONS, SHARE_ACCESS_LEVELS } from "../models/sharing.js";

const emptyTripPlanForm = {
  title: "",
  description: "",
  startDate: "",
  endDate: "",
  plannedBudget: "0",
  notes: "",
};

const emptyDestinationForm = {
  name: "",
  location: "",
  arrivalDate: "",
  departureDate: "",
  description: "",
};

const emptyActivityForm = {
  title: "",
  activityDate: "",
  activityTime: "",
  location: "",
  description: "",
  estimatedCost: "0",
  status: 0,
};

const emptyExpenseForm = {
  title: "",
  category: 0,
  amount: "0",
  expenseDate: "",
  description: "",
};

const emptyChecklistForm = {
  title: "",
};

const emptyNoteForm = {
  title: "",
  content: "",
};

const activityStatuses = [
  { value: 0, label: "Planned" },
  { value: 1, label: "Reserved" },
  { value: 2, label: "Completed" },
  { value: 3, label: "Cancelled" },
];

export function TripPlansPage() {
  const [tripPlans, setTripPlans] = useState([]);
  const [selectedTripPlanId, setSelectedTripPlanId] = useState(null);
  const [selectedTripPlan, setSelectedTripPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [checklistItems, setChecklistItems] = useState([]);
  const [notes, setNotes] = useState([]);
  const [shares, setShares] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [tripPlanForm, setTripPlanForm] = useState(emptyTripPlanForm);
  const [selectedTripPlanForm, setSelectedTripPlanForm] = useState(emptyTripPlanForm);
  const [destinationForm, setDestinationForm] = useState(emptyDestinationForm);
  const [activityForm, setActivityForm] = useState(emptyActivityForm);
  const [expenseForm, setExpenseForm] = useState(emptyExpenseForm);
  const [checklistForm, setChecklistForm] = useState(emptyChecklistForm);
  const [noteForm, setNoteForm] = useState(emptyNoteForm);
  const [shareAccessLevel, setShareAccessLevel] = useState(SHARE_ACCESS_LEVELS.VIEW);
  const [shareExpiresAt, setShareExpiresAt] = useState("");
  const [generatedShareLink, setGeneratedShareLink] = useState("");
  const [visibleShareQrId, setVisibleShareQrId] = useState(null);
  const [activityViewMode, setActivityViewMode] = useState("list");
  const [editingDestinationId, setEditingDestinationId] = useState(null);
  const [editingActivityId, setEditingActivityId] = useState(null);
  const [editingExpenseId, setEditingExpenseId] = useState(null);
  const [editingChecklistItemId, setEditingChecklistItemId] = useState(null);
  const [editingNoteId, setEditingNoteId] = useState(null);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const loadTripPlans = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const plans = (await tripPlansApi.getAll()) ?? [];
      setTripPlans(plans);
      setSelectedTripPlanId((currentId) => {
        if (currentId && plans.some((plan) => plan.id === currentId)) {
          return currentId;
        }

        return plans[0]?.id ?? null;
      });
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadTripPlanDetails = useCallback(async (tripPlanId) => {
    if (!tripPlanId) {
      setSelectedTripPlan(null);
      setDestinations([]);
      setActivities([]);
      setExpenses([]);
      setChecklistItems([]);
      setNotes([]);
      setShares([]);
      setBudgetSummary(null);
      setSelectedTripPlanForm(emptyTripPlanForm);
      setEditingDestinationId(null);
      setEditingActivityId(null);
      setEditingExpenseId(null);
      setEditingChecklistItemId(null);
      setEditingNoteId(null);
      setDestinationForm(emptyDestinationForm);
      setActivityForm(emptyActivityForm);
      setExpenseForm(emptyExpenseForm);
      setChecklistForm(emptyChecklistForm);
      setNoteForm(emptyNoteForm);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      return;
    }

    setError("");

    try {
      const [
        tripPlan,
        tripDestinations,
        tripActivities,
        tripExpenses,
        tripBudgetSummary,
        tripChecklistItems,
        tripNotes,
        tripShares,
      ] =
        await Promise.all([
          tripPlansApi.getById(tripPlanId),
          destinationsApi.getByTripPlanId(tripPlanId),
          activitiesApi.getByTripPlanId(tripPlanId),
          budgetApi.getExpenses(tripPlanId),
          budgetApi.getBudgetSummary(tripPlanId),
          checklistApi.getChecklistItems(tripPlanId),
          notesApi.getNotes(tripPlanId),
          sharingApi.getShares(tripPlanId),
        ]);

      setSelectedTripPlan(tripPlan);
      setSelectedTripPlanForm(toTripPlanForm(tripPlan));
      setDestinations(tripDestinations ?? []);
      setActivities(tripActivities ?? []);
      setExpenses(tripExpenses ?? []);
      setBudgetSummary(tripBudgetSummary);
      setChecklistItems(tripChecklistItems ?? []);
      setNotes(tripNotes ?? []);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  }, []);

  useEffect(() => {
    void loadTripPlans();
  }, [loadTripPlans]);

  useEffect(() => {
    void loadTripPlanDetails(selectedTripPlanId);
  }, [loadTripPlanDetails, selectedTripPlanId]);

  const sortedDestinations = useMemo(
    () => [...destinations].sort((first, second) => compareDates(first.arrivalDate, second.arrivalDate)),
    [destinations],
  );

  const activityGroups = useMemo(() => groupActivitiesByDate(activities), [activities]);

  const activityCalendarDays = useMemo(
    () => buildActivityCalendarDays(selectedTripPlan?.startDate, selectedTripPlan?.endDate, activities),
    [activities, selectedTripPlan?.endDate, selectedTripPlan?.startDate],
  );

  const updateTripPlanField = (event) => {
    updateForm(setTripPlanForm, event);
  };

  const updateSelectedTripPlanField = (event) => {
    updateForm(setSelectedTripPlanForm, event);
  };

  const updateDestinationField = (event) => {
    updateForm(setDestinationForm, event);
  };

  const updateActivityField = (event) => {
    updateForm(setActivityForm, event);
  };

  const updateExpenseField = (event) => {
    updateForm(setExpenseForm, event);
  };

  const updateChecklistField = (event) => {
    updateForm(setChecklistForm, event);
  };

  const updateNoteField = (event) => {
    updateForm(setNoteForm, event);
  };

  const selectTripPlan = (tripPlanId) => {
    setSelectedTripPlanId(tripPlanId);
    setMessage("");
    setEditingDestinationId(null);
    setEditingActivityId(null);
    setEditingExpenseId(null);
    setEditingChecklistItemId(null);
    setEditingNoteId(null);
    setDestinationForm(emptyDestinationForm);
    setActivityForm(emptyActivityForm);
    setExpenseForm(emptyExpenseForm);
    setChecklistForm(emptyChecklistForm);
    setNoteForm(emptyNoteForm);
    setGeneratedShareLink("");
    setVisibleShareQrId(null);
    setActivityViewMode("list");
  };

  const submitTripPlan = async (event) => {
    event.preventDefault();
    setError("");

    try {
      const created = await tripPlansApi.create({
        title: tripPlanForm.title,
        description: tripPlanForm.description || null,
        startDate: toDateTime(tripPlanForm.startDate),
        endDate: toDateTime(tripPlanForm.endDate),
        plannedBudget: Number(tripPlanForm.plannedBudget || 0),
        notes: tripPlanForm.notes || null,
      });

      setTripPlanForm(emptyTripPlanForm);
      await loadTripPlans();
      setSelectedTripPlanId(created.id);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteTripPlan = async (tripPlanId) => {
    setError("");
    setMessage("");

    try {
      await tripPlansApi.remove(tripPlanId);
      setSelectedTripPlanId(null);
      setSelectedTripPlan(null);
      setExpenses([]);
      setChecklistItems([]);
      setNotes([]);
      setShares([]);
      setBudgetSummary(null);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      await loadTripPlans();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const updateSelectedTripPlan = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await tripPlansApi.update(selectedTripPlanId, {
        title: selectedTripPlanForm.title,
        description: selectedTripPlanForm.description || null,
        startDate: toDateTime(selectedTripPlanForm.startDate),
        endDate: toDateTime(selectedTripPlanForm.endDate),
        plannedBudget: Number(selectedTripPlanForm.plannedBudget || 0),
        notes: selectedTripPlanForm.notes || null,
      });

      await loadTripPlanDetails(selectedTripPlanId);
      await loadTripPlans();
      setMessage("Osnovni podaci plana su sacuvani.");
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const resetSelectedTripPlanForm = () => {
    setSelectedTripPlanForm(toTripPlanForm(selectedTripPlan));
    setMessage("");
  };

  const submitDestination = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        name: destinationForm.name,
        location: destinationForm.location || null,
        arrivalDate: toDateTime(destinationForm.arrivalDate),
        departureDate: toDateTime(destinationForm.departureDate),
        description: destinationForm.description || null,
      };

      if (editingDestinationId) {
        await destinationsApi.update(selectedTripPlanId, editingDestinationId, payload);
      } else {
        await destinationsApi.create(selectedTripPlanId, payload);
      }

      cancelDestinationEdit();
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editDestination = (destination) => {
    setEditingDestinationId(destination.id);
    setDestinationForm({
      name: destination.name ?? "",
      location: destination.location ?? "",
      arrivalDate: toDateInputValue(destination.arrivalDate),
      departureDate: toDateInputValue(destination.departureDate),
      description: destination.description ?? "",
    });
  };

  const cancelDestinationEdit = () => {
    setEditingDestinationId(null);
    setDestinationForm(emptyDestinationForm);
  };

  const deleteDestination = async (destinationId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await destinationsApi.remove(selectedTripPlanId, destinationId);
      if (editingDestinationId === destinationId) {
        cancelDestinationEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitActivity = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        title: activityForm.title,
        activityDate: toDateTime(activityForm.activityDate),
        activityTime: activityForm.activityTime ? `${activityForm.activityTime}:00` : null,
        location: activityForm.location || null,
        description: activityForm.description || null,
        estimatedCost: Number(activityForm.estimatedCost || 0),
        status: Number(activityForm.status),
      };

      if (editingActivityId) {
        await activitiesApi.update(selectedTripPlanId, editingActivityId, payload);
      } else {
        await activitiesApi.create(selectedTripPlanId, payload);
      }

      cancelActivityEdit();
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editActivity = (activity) => {
    setEditingActivityId(activity.id);
    setActivityForm({
      title: activity.title ?? "",
      activityDate: toDateInputValue(activity.activityDate),
      activityTime: toTimeInputValue(activity.activityTime),
      location: activity.location ?? "",
      description: activity.description ?? "",
      estimatedCost: String(activity.estimatedCost ?? 0),
      status: Number(activity.status ?? 0),
    });
  };

  const cancelActivityEdit = () => {
    setEditingActivityId(null);
    setActivityForm(emptyActivityForm);
  };

  const deleteActivity = async (activityId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await activitiesApi.remove(selectedTripPlanId, activityId);
      if (editingActivityId === activityId) {
        cancelActivityEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitExpense = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: expenseForm.title,
        category: Number(expenseForm.category),
        amount: Number(expenseForm.amount || 0),
        expenseDate: toDateTime(expenseForm.expenseDate),
        description: expenseForm.description || null,
      };

      if (editingExpenseId) {
        await budgetApi.updateExpense(selectedTripPlanId, editingExpenseId, payload);
      } else {
        await budgetApi.createExpense(selectedTripPlanId, payload);
      }

      cancelExpenseEdit();
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editExpense = (expense) => {
    setEditingExpenseId(expense.id);
    setExpenseForm({
      title: expense.title ?? "",
      category: Number(expense.category ?? 0),
      amount: String(expense.amount ?? 0),
      expenseDate: toDateInputValue(expense.expenseDate),
      description: expense.description ?? "",
    });
  };

  const cancelExpenseEdit = () => {
    setEditingExpenseId(null);
    setExpenseForm(emptyExpenseForm);
  };

  const deleteExpense = async (expenseId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await budgetApi.deleteExpense(selectedTripPlanId, expenseId);
      if (editingExpenseId === expenseId) {
        cancelExpenseEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitChecklistItem = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: checklistForm.title,
      };

      if (editingChecklistItemId) {
        const checklistItem = checklistItems.find((item) => item.id === editingChecklistItemId);
        await checklistApi.updateChecklistItem(selectedTripPlanId, editingChecklistItemId, {
          ...payload,
          isCompleted: Boolean(checklistItem?.isCompleted),
        });
      } else {
        await checklistApi.createChecklistItem(selectedTripPlanId, payload);
      }

      cancelChecklistEdit();
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editChecklistItem = (checklistItem) => {
    setEditingChecklistItemId(checklistItem.id);
    setChecklistForm({
      title: checklistItem.title ?? "",
    });
  };

  const cancelChecklistEdit = () => {
    setEditingChecklistItemId(null);
    setChecklistForm(emptyChecklistForm);
  };

  const toggleChecklistItem = async (checklistItem, isCompleted) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await checklistApi.updateChecklistItem(selectedTripPlanId, checklistItem.id, {
        title: checklistItem.title,
        isCompleted,
      });

      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteChecklistItem = async (checklistItemId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await checklistApi.deleteChecklistItem(selectedTripPlanId, checklistItemId);
      if (editingChecklistItemId === checklistItemId) {
        cancelChecklistEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitNote = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: noteForm.title,
        content: noteForm.content || null,
      };

      if (editingNoteId) {
        await notesApi.updateNote(selectedTripPlanId, editingNoteId, {
          title: payload.title,
          content: payload.content,
        });
      } else {
        await notesApi.createNote(selectedTripPlanId, payload);
      }

      cancelNoteEdit();
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editNote = (note) => {
    setEditingNoteId(note.id);
    setNoteForm({
      title: note.title ?? "",
      content: note.content ?? "",
    });
  };

  const cancelNoteEdit = () => {
    setEditingNoteId(null);
    setNoteForm(emptyNoteForm);
  };

  const deleteNote = async (noteId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await notesApi.deleteNote(selectedTripPlanId, noteId);
      if (editingNoteId === noteId) {
        cancelNoteEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const createShare = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const created = await sharingApi.createShare(selectedTripPlanId, {
        accessLevel: Number(shareAccessLevel),
        expiresAt: shareExpiresAt ? toExpirationDateTime(shareExpiresAt) : null,
      });

      setShareExpiresAt("");
      setGeneratedShareLink(buildSharedTripPlanLink(created.token));
      setVisibleShareQrId(created.id ?? null);
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const revokeShare = async (shareId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await sharingApi.revokeShare(selectedTripPlanId, shareId);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <div className="page-stack">
      <header className="page-header">
        <div>
          <h1 className="page-title">Moji planovi</h1>
          <p className="page-subtitle">Pregled putovanja, troskova, aktivnosti i deljenih linkova.</p>
        </div>
      </header>

      {error ? <p className="alert alert-error">{error}</p> : null}
      {message ? <p className="alert alert-success">{message}</p> : null}

      <section className="trips-layout">
        <aside className="sidebar-column">
          <section className="form-card">
            <div className="section-header">
              <div>
                <h2 className="section-title">Novi plan</h2>
                <p className="section-subtitle">Osnovni podaci za putovanje.</p>
              </div>
            </div>

            <form className="form-grid" onSubmit={submitTripPlan}>
              <label className="field">
                <span className="field-label">Naziv plana</span>
                <input
                  className="input"
                  name="title"
                  onChange={updateTripPlanField}
                  placeholder="Letovanje u Grckoj"
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
                  placeholder="Kratak opis plana"
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
                  placeholder="Smestaj, prevoz, vazne napomene"
                  rows={3}
                  value={tripPlanForm.notes}
                />
              </label>

              <button className="btn btn-primary" type="submit">
                Dodaj plan
              </button>
            </form>
          </section>

          <section className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title">Planovi</h2>
                <p className="section-subtitle">Ukupno: {tripPlans.length}</p>
              </div>
              {isLoading ? <span className="badge badge-muted">Ucitava se...</span> : null}
            </div>

            <div className="trip-list">
              {tripPlans.map((tripPlan) => (
                <article
                  className={`trip-card${tripPlan.id === selectedTripPlanId ? " is-active" : ""}`}
                  key={tripPlan.id}
                >
                  <div className="trip-card-header">
                    <h3 className="trip-title">{tripPlan.title}</h3>
                    <span className="badge">{formatMoney(tripPlan.plannedBudget)}</span>
                  </div>

                  {tripPlan.description ? <p className="trip-description">{tripPlan.description}</p> : null}

                  <div className="meta-grid">
                    <span className="meta-item">
                      <span className="meta-label">Datumi</span>
                      <span className="meta-value">{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</span>
                    </span>
                    <span className="meta-item">
                      <span className="meta-label">Budzet</span>
                      <span className="meta-value">{formatMoney(tripPlan.plannedBudget)}</span>
                    </span>
                  </div>

                  <div className="card-actions">
                    <button className="btn btn-primary btn-small" onClick={() => selectTripPlan(tripPlan.id)} type="button">
                      Otvori detalje
                    </button>
                    <button
                      className="btn btn-danger-soft btn-small"
                      onClick={() => deleteTripPlan(tripPlan.id)}
                      type="button"
                    >
                      Obrisi
                    </button>
                  </div>
                </article>
              ))}

              {!isLoading && tripPlans.length === 0 ? <div className="empty-state">Nema sacuvanih planova.</div> : null}
            </div>
          </section>
        </aside>

        <section className="details-column">
          {selectedTripPlan ? (
            <>
              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Osnovni podaci</h2>
                    <p className="section-subtitle">Naziv, period, budzet i napomene za izabrani plan.</p>
                  </div>
                  <button className="btn btn-danger-soft" onClick={() => deleteTripPlan(selectedTripPlan.id)} type="button">
                    Obrisi plan
                  </button>
                </div>

                <div className="detail-heading">
                  <div>
                    <h3 className="detail-title">{selectedTripPlan.title}</h3>
                    <p className="section-subtitle">
                      {formatDateRange(selectedTripPlan.startDate, selectedTripPlan.endDate)}
                    </p>
                  </div>
                </div>

                <div className="overview-grid">
                  <span className="stat-tile">
                    <span className="stat-label">Planirani budzet</span>
                    <span className="stat-value">
                      {formatMoney(budgetSummary?.plannedBudget ?? selectedTripPlan.plannedBudget)}
                    </span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">Ukupno potroseno</span>
                    <span className="stat-value">{formatMoney(budgetSummary?.totalExpenses)}</span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">Preostalo</span>
                    <span className="stat-value">
                      {formatMoney(budgetSummary?.remainingBudget ?? selectedTripPlan.plannedBudget)}
                    </span>
                  </span>
                </div>

                {selectedTripPlan.description ? <p className="list-item-description">{selectedTripPlan.description}</p> : null}
                {selectedTripPlan.notes ? <p className="note-box">{selectedTripPlan.notes}</p> : null}

                <div className="form-panel">
                  <h3 className="subsection-title">Izmena osnovnih podataka</h3>
                  <form className="form-grid" onSubmit={updateSelectedTripPlan}>
                    <label className="field">
                      <span className="field-label">Naziv plana</span>
                      <input
                        className="input"
                        name="title"
                        onChange={updateSelectedTripPlanField}
                        required
                        value={selectedTripPlanForm.title}
                      />
                    </label>

                    <label className="field">
                      <span className="field-label">Opis</span>
                      <textarea
                        className="textarea"
                        name="description"
                        onChange={updateSelectedTripPlanField}
                        rows={3}
                        value={selectedTripPlanForm.description}
                      />
                    </label>

                    <div className="form-row">
                      <label className="field">
                        <span className="field-label">Pocetak</span>
                        <input
                          className="input"
                          name="startDate"
                          onChange={updateSelectedTripPlanField}
                          required
                          type="date"
                          value={selectedTripPlanForm.startDate}
                        />
                      </label>
                      <label className="field">
                        <span className="field-label">Kraj</span>
                        <input
                          className="input"
                          name="endDate"
                          onChange={updateSelectedTripPlanField}
                          required
                          type="date"
                          value={selectedTripPlanForm.endDate}
                        />
                      </label>
                    </div>

                    <label className="field">
                      <span className="field-label">Planirani budzet</span>
                      <input
                        className="input"
                        min="0"
                        name="plannedBudget"
                        onChange={updateSelectedTripPlanField}
                        step="0.01"
                        type="number"
                        value={selectedTripPlanForm.plannedBudget}
                      />
                    </label>

                    <label className="field">
                      <span className="field-label">Napomene</span>
                      <textarea
                        className="textarea"
                        name="notes"
                        onChange={updateSelectedTripPlanField}
                        rows={3}
                        value={selectedTripPlanForm.notes}
                      />
                    </label>

                    <div className="button-row">
                      <button className="btn btn-primary" type="submit">
                        Sacuvaj osnovne podatke
                      </button>
                      <button className="btn btn-secondary" onClick={resetSelectedTripPlanForm} type="button">
                        Vrati vrednosti
                      </button>
                    </div>
                  </form>
                </div>
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Destinacije</h2>
                    <p className="section-subtitle">Gradovi i mesta koja ulaze u plan.</p>
                  </div>
                  <span className="badge">{destinations.length}</span>
                </div>

                <form className="form-grid" onSubmit={submitDestination}>
                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">Naziv</span>
                      <input
                        className="input"
                        name="name"
                        onChange={updateDestinationField}
                        placeholder="Atina"
                        required
                        value={destinationForm.name}
                      />
                    </label>
                    <label className="field">
                      <span className="field-label">Lokacija</span>
                      <input
                        className="input"
                        name="location"
                        onChange={updateDestinationField}
                        placeholder="Grcka"
                        value={destinationForm.location}
                      />
                    </label>
                  </div>

                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">Datum dolaska</span>
                      <input
                        className="input"
                        name="arrivalDate"
                        onChange={updateDestinationField}
                        required
                        type="date"
                        value={destinationForm.arrivalDate}
                      />
                    </label>
                    <label className="field">
                      <span className="field-label">Datum odlaska</span>
                      <input
                        className="input"
                        name="departureDate"
                        onChange={updateDestinationField}
                        required
                        type="date"
                        value={destinationForm.departureDate}
                      />
                    </label>
                  </div>

                  <label className="field">
                    <span className="field-label">Opis</span>
                    <textarea
                      className="textarea"
                      name="description"
                      onChange={updateDestinationField}
                      placeholder="Smestaj, prevoz ili kratka napomena"
                      rows={3}
                      value={destinationForm.description}
                    />
                  </label>

                  <div className="button-row">
                    <button className="btn btn-primary" type="submit">
                      {editingDestinationId ? "Sacuvaj destinaciju" : "Dodaj destinaciju"}
                    </button>
                    {editingDestinationId ? (
                      <button className="btn btn-secondary" onClick={cancelDestinationEdit} type="button">
                        Odustani
                      </button>
                    ) : null}
                  </div>
                </form>

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
                      <div className="list-item-actions">
                        <button className="btn btn-secondary btn-small" onClick={() => editDestination(destination)} type="button">
                          Izmeni
                        </button>
                        <button
                          className="btn btn-danger-soft btn-small"
                          onClick={() => deleteDestination(destination.id)}
                          type="button"
                        >
                          Obrisi
                        </button>
                      </div>
                    </article>
                  ))}
                  {destinations.length === 0 ? <div className="empty-state">Nema dodatih destinacija.</div> : null}
                </div>
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Aktivnosti</h2>
                    <p className="section-subtitle">Sortirano po datumima.</p>
                  </div>
                  <span className="badge">{activities.length}</span>
                </div>

                <form className="form-grid" onSubmit={submitActivity}>
                  <label className="field">
                    <span className="field-label">Naziv aktivnosti</span>
                    <input
                      className="input"
                      name="title"
                      onChange={updateActivityField}
                      placeholder="Obilazak muzeja"
                      required
                      value={activityForm.title}
                    />
                  </label>

                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">Datum</span>
                      <input
                        className="input"
                        name="activityDate"
                        onChange={updateActivityField}
                        required
                        type="date"
                        value={activityForm.activityDate}
                      />
                    </label>
                    <label className="field">
                      <span className="field-label">Vreme</span>
                      <input
                        className="input"
                        name="activityTime"
                        onChange={updateActivityField}
                        type="time"
                        value={activityForm.activityTime}
                      />
                    </label>
                  </div>

                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">Lokacija</span>
                      <input
                        className="input"
                        name="location"
                        onChange={updateActivityField}
                        placeholder="Centar grada"
                        value={activityForm.location}
                      />
                    </label>
                    <label className="field">
                      <span className="field-label">Status</span>
                      <select className="select" name="status" onChange={updateActivityField} value={activityForm.status}>
                        {activityStatuses.map((status) => (
                          <option key={status.value} value={status.value}>
                            {status.label}
                          </option>
                        ))}
                      </select>
                    </label>
                  </div>

                  <label className="field">
                    <span className="field-label">Procenjeni trosak</span>
                    <input
                      className="input"
                      min="0"
                      name="estimatedCost"
                      onChange={updateActivityField}
                      step="0.01"
                      type="number"
                      value={activityForm.estimatedCost}
                    />
                  </label>

                  <label className="field">
                    <span className="field-label">Opis</span>
                    <textarea
                      className="textarea"
                      name="description"
                      onChange={updateActivityField}
                      placeholder="Kratka napomena za aktivnost"
                      rows={3}
                      value={activityForm.description}
                    />
                  </label>

                  <div className="button-row">
                    <button className="btn btn-primary" type="submit">
                      {editingActivityId ? "Sacuvaj aktivnost" : "Dodaj aktivnost"}
                    </button>
                    {editingActivityId ? (
                      <button className="btn btn-secondary" onClick={cancelActivityEdit} type="button">
                        Odustani
                      </button>
                    ) : null}
                  </div>
                </form>

                <div className="activity-view-toggle" aria-label="Prikaz aktivnosti" role="group">
                  <button
                    aria-pressed={activityViewMode === "list"}
                    className={`btn btn-small btn-toggle${activityViewMode === "list" ? " is-active" : ""}`}
                    onClick={() => setActivityViewMode("list")}
                    type="button"
                  >
                    Lista
                  </button>
                  <button
                    aria-pressed={activityViewMode === "calendar"}
                    className={`btn btn-small btn-toggle${activityViewMode === "calendar" ? " is-active" : ""}`}
                    onClick={() => setActivityViewMode("calendar")}
                    type="button"
                  >
                    Kalendar
                  </button>
                </div>

                {activityViewMode === "list" ? (
                  <div className="item-list">
                    {activityGroups.map(([dateKey, groupActivities]) => (
                      <div className="activity-day" key={dateKey}>
                        <h3 className="activity-day-title">
                          {dateKey === "no-date" ? "Bez datuma" : formatDate(groupActivities[0]?.activityDate)}
                        </h3>
                        {groupActivities.map((activity) => (
                          <article className="list-item" key={activity.id}>
                            <div className="list-item-main">
                              <span className="list-item-title">{activity.title}</span>
                              <p className="muted">
                                {activity.activityTime ? `${String(activity.activityTime).slice(0, 5)} - ` : ""}
                                {activity.location || "Bez lokacije"}
                              </p>
                              <p className="muted">
                                <span className={`badge ${getStatusClass(activity.status)}`}>
                                  {getStatusLabel(activity.status)}
                                </span>{" "}
                                {formatMoney(activity.estimatedCost)}
                              </p>
                              {activity.description ? <p className="list-item-description">{activity.description}</p> : null}
                            </div>
                            <div className="list-item-actions">
                              <button className="btn btn-secondary btn-small" onClick={() => editActivity(activity)} type="button">
                                Izmeni
                              </button>
                              <button
                                className="btn btn-danger-soft btn-small"
                                onClick={() => deleteActivity(activity.id)}
                                type="button"
                              >
                                Obrisi
                              </button>
                            </div>
                          </article>
                        ))}
                      </div>
                    ))}
                    {activities.length === 0 ? <div className="empty-state">Nema dodatih aktivnosti.</div> : null}
                  </div>
                ) : (
                  <ActivityCalendar days={activityCalendarDays} emptyMessage="Nema dodatih aktivnosti." />
                )}
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Troskovi i budzet</h2>
                    <p className="section-subtitle">Transport, smestaj, hrana, karte i ostalo.</p>
                  </div>
                  <span className="badge">{expenses.length}</span>
                </div>

                <div className="overview-grid">
                  <span className="stat-tile">
                    <span className="stat-label">Planirani budzet</span>
                    <span className="stat-value">
                      {formatMoney(budgetSummary?.plannedBudget ?? selectedTripPlan.plannedBudget)}
                    </span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">Ukupno potroseno</span>
                    <span className="stat-value">{formatMoney(budgetSummary?.totalExpenses)}</span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">Preostali budzet</span>
                    <span className="stat-value">
                      {formatMoney(budgetSummary?.remainingBudget ?? selectedTripPlan.plannedBudget)}
                    </span>
                  </span>
                </div>

                <form className="form-grid" onSubmit={submitExpense}>
                  <label className="field">
                    <span className="field-label">Naziv troska</span>
                    <input
                      className="input"
                      name="title"
                      onChange={updateExpenseField}
                      placeholder="Avionska karta"
                      required
                      value={expenseForm.title}
                    />
                  </label>

                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">Kategorija</span>
                      <select
                        className="select"
                        name="category"
                        onChange={updateExpenseField}
                        required
                        value={expenseForm.category}
                      >
                        {EXPENSE_CATEGORIES.map((category) => (
                          <option key={category.value} value={category.value}>
                            {category.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="field">
                      <span className="field-label">Iznos</span>
                      <input
                        className="input"
                        min="0"
                        name="amount"
                        onChange={updateExpenseField}
                        required
                        step="0.01"
                        type="number"
                        value={expenseForm.amount}
                      />
                    </label>
                  </div>

                  <label className="field">
                    <span className="field-label">Datum</span>
                    <input
                      className="input"
                      name="expenseDate"
                      onChange={updateExpenseField}
                      required
                      type="date"
                      value={expenseForm.expenseDate}
                    />
                  </label>

                  <label className="field">
                    <span className="field-label">Opis</span>
                    <textarea
                      className="textarea"
                      name="description"
                      onChange={updateExpenseField}
                      placeholder="Napomena za trosak"
                      rows={3}
                      value={expenseForm.description}
                    />
                  </label>

                  <div className="button-row">
                    <button className="btn btn-primary" type="submit">
                      {editingExpenseId ? "Sacuvaj trosak" : "Dodaj trosak"}
                    </button>
                    {editingExpenseId ? (
                      <button className="btn btn-secondary" onClick={cancelExpenseEdit} type="button">
                        Odustani
                      </button>
                    ) : null}
                  </div>
                </form>

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
                      <div className="list-item-actions">
                        <button className="btn btn-secondary btn-small" onClick={() => editExpense(expense)} type="button">
                          Izmeni
                        </button>
                        <button
                          className="btn btn-danger-soft btn-small"
                          onClick={() => deleteExpense(expense.id)}
                          type="button"
                        >
                          Obrisi
                        </button>
                      </div>
                    </article>
                  ))}
                  {expenses.length === 0 ? <div className="empty-state">Nema dodatih troskova.</div> : null}
                </div>
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Checklist</h2>
                    <p className="section-subtitle">Stavke za pakovanje i pripremu.</p>
                  </div>
                  <span className="badge">{checklistItems.length}</span>
                </div>

                <form className="form-grid" onSubmit={submitChecklistItem}>
                  <label className="field">
                    <span className="field-label">Nova stavka</span>
                    <input
                      className="input"
                      maxLength={150}
                      name="title"
                      onChange={updateChecklistField}
                      placeholder="pasos, karta, punjac, putno osiguranje"
                      required
                      value={checklistForm.title}
                    />
                  </label>

                  <div className="button-row">
                    <button className="btn btn-primary" type="submit">
                      {editingChecklistItemId ? "Sacuvaj stavku" : "Dodaj stavku"}
                    </button>
                    {editingChecklistItemId ? (
                      <button className="btn btn-secondary" onClick={cancelChecklistEdit} type="button">
                        Odustani
                      </button>
                    ) : null}
                  </div>
                </form>

                <div className="item-list">
                  {checklistItems.map((checklistItem) => (
                    <article className="list-item checklist-item" key={checklistItem.id}>
                      <label className="checklist-label">
                        <input
                          checked={Boolean(checklistItem.isCompleted)}
                          onChange={(event) => toggleChecklistItem(checklistItem, event.target.checked)}
                          type="checkbox"
                        />
                        <span className={`checklist-title${checklistItem.isCompleted ? " is-done" : ""}`}>
                          {checklistItem.title}
                        </span>
                      </label>
                      <div className="list-item-actions">
                        <button className="btn btn-secondary btn-small" onClick={() => editChecklistItem(checklistItem)} type="button">
                          Izmeni
                        </button>
                        <button
                          className="btn btn-danger-soft btn-small"
                          onClick={() => deleteChecklistItem(checklistItem.id)}
                          type="button"
                        >
                          Obrisi
                        </button>
                      </div>
                    </article>
                  ))}
                  {checklistItems.length === 0 ? <div className="empty-state">Checklist je prazan.</div> : null}
                </div>
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Beleske</h2>
                    <p className="section-subtitle">Kratke napomene vezane za izabrani plan.</p>
                  </div>
                  <span className="badge">{notes.length}</span>
                </div>

                <form className="form-grid" onSubmit={submitNote}>
                  <label className="field">
                    <span className="field-label">Naslov</span>
                    <input
                      className="input"
                      maxLength={150}
                      name="title"
                      onChange={updateNoteField}
                      placeholder="Rezervacije, ideje, bitni kontakti"
                      required
                      value={noteForm.title}
                    />
                  </label>

                  <label className="field">
                    <span className="field-label">Sadrzaj</span>
                    <textarea
                      className="textarea"
                      name="content"
                      onChange={updateNoteField}
                      placeholder="Tekst beleske"
                      rows={3}
                      value={noteForm.content}
                    />
                  </label>

                  <div className="button-row">
                    <button className="btn btn-primary" type="submit">
                      {editingNoteId ? "Sacuvaj belesku" : "Dodaj belesku"}
                    </button>
                    {editingNoteId ? (
                      <button className="btn btn-secondary" onClick={cancelNoteEdit} type="button">
                        Odustani
                      </button>
                    ) : null}
                  </div>
                </form>

                <div className="item-list">
                  {notes.map((note) => (
                    <article className="list-item" key={note.id}>
                      <div className="list-item-main">
                        <span className="list-item-title">{note.title}</span>
                        <p className="muted">
                          {note.updatedAt ? `Izmenjeno: ${formatDateTime(note.updatedAt)}` : `Kreirano: ${formatDateTime(note.createdAt)}`}
                        </p>
                        {note.content ? <p className="list-item-description">{note.content}</p> : null}
                      </div>
                      <div className="list-item-actions">
                        <button className="btn btn-secondary btn-small" onClick={() => editNote(note)} type="button">
                          Izmeni
                        </button>
                        <button className="btn btn-danger-soft btn-small" onClick={() => deleteNote(note.id)} type="button">
                          Obrisi
                        </button>
                      </div>
                    </article>
                  ))}
                  {notes.length === 0 ? <div className="empty-state">Nema dodatih beleski.</div> : null}
                </div>
              </section>

              <section className="section-card">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">Deljenje</h2>
                    <p className="section-subtitle">Kreiranje i opoziv javnih linkova.</p>
                  </div>
                  <span className="badge">{shares.length}</span>
                </div>

                <div className="access-help">
                  <strong>VIEW</strong>: samo pregled. <strong>EDIT</strong>: moze izmeniti osnovne podatke plana.
                </div>

                <form className="form-grid" onSubmit={createShare}>
                  <div className="form-row">
                    <label className="field">
                      <span className="field-label">AccessLevel</span>
                      <select
                        className="select"
                        onChange={(event) => setShareAccessLevel(Number(event.target.value))}
                        value={shareAccessLevel}
                      >
                        {SHARE_ACCESS_LEVEL_OPTIONS.map((accessLevel) => (
                          <option key={accessLevel.value} value={accessLevel.value}>
                            {accessLevel.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="field">
                      <span className="field-label">Datum isteka</span>
                      <input
                        className="input"
                        min={todayDateInput()}
                        onChange={(event) => setShareExpiresAt(event.target.value)}
                        type="date"
                        value={shareExpiresAt}
                      />
                    </label>
                  </div>

                  <button className="btn btn-primary" type="submit">
                    Kreiraj link
                  </button>
                </form>

                {generatedShareLink ? (
                  <div className="generated-share">
                    <p className="link-box">
                      Novi link:{" "}
                      <a href={generatedShareLink} rel="noreferrer" target="_blank">
                        {generatedShareLink}
                      </a>
                    </p>
                    <ShareQrCode value={generatedShareLink} />
                  </div>
                ) : null}

                <div className="item-list">
                  {shares.map((share) => {
                    const shareLink = buildSharedTripPlanLink(share.token);
                    const isQrVisible = visibleShareQrId === share.id;

                    return (
                      <article className="list-item" key={share.id}>
                        <div className="list-item-main">
                          <span className="list-item-title">{getShareAccessLevelLabel(share.accessLevel)}</span>
                          <span className={`badge ${share.isRevoked ? "badge-danger" : "badge-success"}`}>
                            {share.isRevoked ? "Revoked" : "Active"}
                          </span>
                          <p className="muted">Created: {formatDateTime(share.createdAt)}</p>
                          {share.expiresAt ? <p className="muted">Expires: {formatDateTime(share.expiresAt)}</p> : null}
                          <p className="share-token">{share.token}</p>
                          {!share.isRevoked ? (
                            <a className="breakable" href={shareLink} rel="noreferrer" target="_blank">
                              {shareLink}
                            </a>
                          ) : null}
                          {!share.isRevoked && isQrVisible ? <ShareQrCode value={shareLink} /> : null}
                        </div>
                        {!share.isRevoked ? (
                          <div className="list-item-actions">
                            <button
                              className="btn btn-secondary btn-small"
                              onClick={() => setVisibleShareQrId(isQrVisible ? null : share.id)}
                              type="button"
                            >
                              {isQrVisible ? "Sakrij QR" : "Prikazi QR"}
                            </button>
                            <button className="btn btn-danger-soft btn-small" onClick={() => revokeShare(share.id)} type="button">
                              Opozovi
                            </button>
                          </div>
                        ) : null}
                      </article>
                    );
                  })}
                  {shares.length === 0 ? <div className="empty-state">Nema kreiranih share tokena.</div> : null}
                </div>
              </section>
            </>
          ) : (
            <div className="empty-state">Kreiraj novi plan ili otvori detalje postojeceg plana.</div>
          )}
        </section>
      </section>
    </div>
  );
}

function ShareQrCode({ value }) {
  return (
    <div className="share-qr-card">
      <div className="share-qr-code">
        <QRCodeSVG includeMargin level="M" size={132} title="QR kod za deljeni plan" value={value} />
      </div>
      <p>Skeniraj QR kod za otvaranje deljenog plana.</p>
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

function updateForm(setForm, event) {
  const { name, value } = event.target;
  setForm((currentForm) => ({
    ...currentForm,
    [name]: value,
  }));
}

function toDateTime(dateValue) {
  return dateValue ? `${dateValue}T00:00:00` : null;
}

function toExpirationDateTime(dateValue) {
  return dateValue ? `${dateValue}T23:59:59` : null;
}

function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 10);
}

function toTimeInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 5);
}

function todayDateInput() {
  return new Date().toISOString().slice(0, 10);
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

function getStatusLabel(value) {
  const numericValue = Number(value);
  return activityStatuses.find((status) => status.value === numericValue)?.label ?? "Planned";
}

function getStatusClass(value) {
  const numericValue = Number(value);

  if (numericValue === 1) {
    return "status-reserved";
  }

  if (numericValue === 2) {
    return "status-completed";
  }

  if (numericValue === 3) {
    return "status-cancelled";
  }

  return "status-planned";
}

function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}

function getShareAccessLevelLabel(value) {
  return Number(value) === SHARE_ACCESS_LEVELS.EDIT ? "EDIT" : "VIEW";
}

function buildSharedTripPlanLink(token) {
  return `${window.location.origin}/shared/${token}`;
}
