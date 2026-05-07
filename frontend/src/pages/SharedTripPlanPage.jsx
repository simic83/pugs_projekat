import {
  Calendar,
  CalendarDays,
  Info,
  List,
  ListChecks,
  LogIn,
  MapPinned,
  NotebookText,
  Pencil,
  PiggyBank,
  Plane,
  Plus,
  Receipt,
  Save,
  Trash2,
  Wallet,
  X,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ActivityCalendar } from "../components/ActivityCalendar.jsx";
import { FormFieldError } from "../components/trips/FormFieldError.jsx";
import { RemindersSection } from "../components/trips/RemindersSection.jsx";
import { useApp } from "../context/AppContext.jsx";
import { EXPENSE_CATEGORIES, createExpenseFormModel, createExpenseRequestModel } from "../models/budget.js";
import {
  createChecklistItemFormModel,
  createChecklistItemRequestModel,
  createChecklistItemUpdateRequestModel,
} from "../models/checklist.js";
import { createNoteFormModel, createNoteRequestModel } from "../models/notes.js";
import {
  createReminderFormModel,
  createReminderRequestModel,
  createReminderUpdateRequestModel,
} from "../models/reminders.js";
import { SHARE_ACCESS_LEVELS } from "../models/sharing.js";
import {
  ACTIVITY_STATUS,
  ACTIVITY_STATUSES,
  createActivityFormModel,
  createActivityRequestModel,
  createDestinationFormModel,
  createDestinationRequestModel,
  createTripPlanFormModel,
  createTripPlanRequestModel,
} from "../models/tripPlan.js";
import {
  hasValidationErrors,
  validateActivity,
  validateChecklistItem,
  validateDestination,
  validateExpense,
  validateNote,
  validateReminder,
  validateTripPlan,
} from "../utils/validation.js";

const emptyFormErrors = {
  tripPlan: {},
  destination: {},
  activity: {},
  expense: {},
  checklist: {},
  note: {},
  reminder: {},
};

export function SharedTripPlanPage() {
  const { sharedTripPlanService } = useApp();
  const { token } = useParams();
  const [sharedTripPlan, setSharedTripPlan] = useState(null);
  const [tripPlanForm, setTripPlanForm] = useState(createTripPlanFormModel);
  const [destinationForm, setDestinationForm] = useState(createDestinationFormModel);
  const [activityForm, setActivityForm] = useState(createActivityFormModel);
  const [expenseForm, setExpenseForm] = useState(createExpenseFormModel);
  const [checklistForm, setChecklistForm] = useState(createChecklistItemFormModel);
  const [noteForm, setNoteForm] = useState(createNoteFormModel);
  const [reminderForm, setReminderForm] = useState(createReminderFormModel);
  const [activityViewMode, setActivityViewMode] = useState("list");
  const [editingDestinationId, setEditingDestinationId] = useState(null);
  const [editingActivityId, setEditingActivityId] = useState(null);
  const [editingExpenseId, setEditingExpenseId] = useState(null);
  const [editingChecklistItemId, setEditingChecklistItemId] = useState(null);
  const [editingNoteId, setEditingNoteId] = useState(null);
  const [editingReminderId, setEditingReminderId] = useState(null);
  const [error, setError] = useState("");
  const [formErrors, setFormErrors] = useState(emptyFormErrors);
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const refreshSharedTripPlan = useCallback(async () => {
    const data = await sharedTripPlanService.getByToken(token);
    setSharedTripPlan(data);
    setTripPlanForm(createTripPlanFormModel(data.tripPlan));
    return data;
  }, [sharedTripPlanService, token]);

  const loadSharedTripPlan = useCallback(async () => {
    if (!token) {
      setError("Link nije validan, istekao je ili je opozvan.");
      setIsLoading(false);
      return;
    }

    setError("");
    setMessage("");
    setIsLoading(true);
    setActivityViewMode("list");
    setFormErrors(emptyFormErrors);
    resetEditState(
      setEditingDestinationId,
      setEditingActivityId,
      setEditingExpenseId,
      setEditingChecklistItemId,
      setEditingNoteId,
      setEditingReminderId,
      setDestinationForm,
      setActivityForm,
      setExpenseForm,
      setChecklistForm,
      setNoteForm,
      setReminderForm,
    );

    try {
      await refreshSharedTripPlan();
    } catch {
      setSharedTripPlan(null);
      setError("Link nije validan, istekao je ili je opozvan.");
    } finally {
      setIsLoading(false);
    }
  }, [refreshSharedTripPlan, token]);

  useEffect(() => {
    void loadSharedTripPlan();
  }, [loadSharedTripPlan]);

  const tripPlan = sharedTripPlan?.tripPlan;
  const destinations = sharedTripPlan?.destinations ?? [];
  const activities = sharedTripPlan?.activities ?? [];
  const expenses = sharedTripPlan?.expenses ?? [];
  const budgetSummary = sharedTripPlan?.budgetSummary;
  const checklistItems = sharedTripPlan?.checklistItems ?? [];
  const noteItems = Array.isArray(sharedTripPlan?.notes) ? sharedTripPlan.notes : [];
  const reminderItems = Array.isArray(sharedTripPlan?.reminders) ? sharedTripPlan.reminders : [];
  const hasNotesSection = Array.isArray(sharedTripPlan?.notes);
  const accessLevel = sharedTripPlan?.accessLevel ?? sharedTripPlan?.share?.accessLevel;
  const canEdit = Number(accessLevel) === SHARE_ACCESS_LEVELS.EDIT;
  const DestinationSubmitIcon = editingDestinationId ? Save : Plus;
  const ActivitySubmitIcon = editingActivityId ? Save : Plus;
  const ExpenseSubmitIcon = editingExpenseId ? Save : Plus;
  const ChecklistSubmitIcon = editingChecklistItemId ? Save : Plus;
  const NoteSubmitIcon = editingNoteId ? Save : Plus;
  const plannedBudget = budgetSummary?.plannedBudget ?? tripPlan?.plannedBudget ?? 0;
  const totalExpenses =
    budgetSummary?.totalExpenses ?? expenses.reduce((total, expense) => total + Number(expense.amount ?? 0), 0);
  const remainingBudget = budgetSummary?.remainingBudget ?? plannedBudget - totalExpenses;

  const sortedDestinations = useMemo(
    () => [...destinations].sort((first, second) => compareDates(first.arrivalDate, second.arrivalDate)),
    [destinations],
  );

  const sortedActivities = useMemo(
    () => [...activities].sort(compareActivities),
    [activities],
  );

  const sortedReminders = useMemo(
    () => [...reminderItems].sort(compareReminders),
    [reminderItems],
  );

  const setFormValidationErrors = (formName, validationErrors) => {
    setFormErrors((currentErrors) => ({
      ...currentErrors,
      [formName]: validationErrors,
    }));
  };

  const clearFormErrors = (formName) => {
    setFormValidationErrors(formName, {});
  };

  const clearFormFieldError = (formName, fieldName) => {
    setFormErrors((currentErrors) => ({
      ...currentErrors,
      [formName]: {
        ...currentErrors[formName],
        [fieldName]: "",
      },
    }));
  };

  const updateSharedFormField = (formName, setForm, event) => {
    clearFormFieldError(formName, event.target.name);
    updateForm(setForm, event);
  };

  const updateSharedReminderField = (event) => {
    const { checked, name, type, value } = event.target;
    clearFormFieldError("reminder", name);
    setReminderForm((currentForm) => ({
      ...currentForm,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const stopInvalidSubmit = (formName, validationErrors) => {
    if (hasValidationErrors(validationErrors)) {
      setFormValidationErrors(formName, validationErrors);
      setError("");
      setMessage("");
      return true;
    }

    clearFormErrors(formName);
    return false;
  };

  const runSharedEdit = async (editAction, successMessage) => {
    if (!token || !canEdit) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await editAction();
      await refreshSharedTripPlan();
      setMessage(successMessage);
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const updateSharedTripPlan = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("tripPlan", validateTripPlan(tripPlanForm))) {
      return;
    }

    await runSharedEdit(async () => {
      await sharedTripPlanService.updateTripPlan(token, createTripPlanRequestModel(tripPlanForm));
    }, "Plan je izmenjen.");
  };

  const submitDestination = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("destination", validateDestination(destinationForm))) {
      return;
    }

    await runSharedEdit(async () => {
      const payload = createDestinationRequestModel(destinationForm);
      await sharedTripPlanService.saveDestination(token, editingDestinationId, payload);

      cancelDestinationEdit();
    }, editingDestinationId ? "Destinacija je izmenjena." : "Destinacija je dodata.");
  };

  const editDestination = (destination) => {
    if (!canEdit) {
      return;
    }

    setEditingDestinationId(destination.id);
    clearFormErrors("destination");
    setDestinationForm(createDestinationFormModel(destination));
  };

  const cancelDestinationEdit = () => {
    setEditingDestinationId(null);
    setDestinationForm(createDestinationFormModel());
    clearFormErrors("destination");
  };

  const deleteDestination = async (destinationId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteDestination(token, destinationId);
      if (editingDestinationId === destinationId) {
        cancelDestinationEdit();
      }
    }, "Destinacija je obrisana.");
  };

  const submitActivity = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("activity", validateActivity(activityForm))) {
      return;
    }

    await runSharedEdit(async () => {
      const payload = createActivityRequestModel(activityForm);
      await sharedTripPlanService.saveActivity(token, editingActivityId, payload);

      cancelActivityEdit();
    }, editingActivityId ? "Aktivnost je izmenjena." : "Aktivnost je dodata.");
  };

  const editActivity = (activity) => {
    if (!canEdit) {
      return;
    }

    setEditingActivityId(activity.id);
    clearFormErrors("activity");
    setActivityForm(createActivityFormModel(activity));
  };

  const cancelActivityEdit = () => {
    setEditingActivityId(null);
    setActivityForm(createActivityFormModel());
    clearFormErrors("activity");
  };

  const deleteActivity = async (activityId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteActivity(token, activityId);
      if (editingActivityId === activityId) {
        cancelActivityEdit();
      }
    }, "Aktivnost je obrisana.");
  };

  const submitExpense = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("expense", validateExpense(expenseForm))) {
      return;
    }

    await runSharedEdit(async () => {
      const payload = createExpenseRequestModel(expenseForm, tripPlan?.id);

      if (editingExpenseId) {
        await sharedTripPlanService.saveExpense(token, editingExpenseId, {
          title: payload.title,
          category: payload.category,
          amount: payload.amount,
          expenseDate: payload.expenseDate,
          description: payload.description,
        });
      } else {
        await sharedTripPlanService.saveExpense(token, null, payload);
      }

      cancelExpenseEdit();
    }, editingExpenseId ? "Trosak je izmenjen." : "Trosak je dodat.");
  };

  const editExpense = (expense) => {
    if (!canEdit) {
      return;
    }

    setEditingExpenseId(expense.id);
    clearFormErrors("expense");
    setExpenseForm(createExpenseFormModel(expense));
  };

  const cancelExpenseEdit = () => {
    setEditingExpenseId(null);
    setExpenseForm(createExpenseFormModel());
    clearFormErrors("expense");
  };

  const deleteExpense = async (expenseId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteExpense(token, expenseId);
      if (editingExpenseId === expenseId) {
        cancelExpenseEdit();
      }
    }, "Trosak je obrisan.");
  };

  const submitChecklistItem = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("checklist", validateChecklistItem(checklistForm))) {
      return;
    }

    await runSharedEdit(async () => {
      if (editingChecklistItemId) {
        const checklistItem = checklistItems.find((item) => item.id === editingChecklistItemId);
        await sharedTripPlanService.saveChecklistItem(token, editingChecklistItemId, createChecklistItemUpdateRequestModel({
          title: checklistForm.title,
          isCompleted: Boolean(checklistItem?.isCompleted),
        }));
      } else {
        await sharedTripPlanService.saveChecklistItem(
          token,
          null,
          createChecklistItemRequestModel(checklistForm, tripPlan?.id),
        );
      }

      cancelChecklistEdit();
    }, editingChecklistItemId ? "Checklist stavka je izmenjena." : "Checklist stavka je dodata.");
  };

  const editChecklistItem = (checklistItem) => {
    if (!canEdit) {
      return;
    }

    setEditingChecklistItemId(checklistItem.id);
    clearFormErrors("checklist");
    setChecklistForm(createChecklistItemFormModel(checklistItem));
  };

  const cancelChecklistEdit = () => {
    setEditingChecklistItemId(null);
    setChecklistForm(createChecklistItemFormModel());
    clearFormErrors("checklist");
  };

  const toggleChecklistItem = async (checklistItem, isCompleted) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.saveChecklistItem(token, checklistItem.id, createChecklistItemUpdateRequestModel({
        title: checklistItem.title,
        isCompleted,
      }));
    }, "Checklist stavka je izmenjena.");
  };

  const deleteChecklistItem = async (checklistItemId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteChecklistItem(token, checklistItemId);
      if (editingChecklistItemId === checklistItemId) {
        cancelChecklistEdit();
      }
    }, "Checklist stavka je obrisana.");
  };

  const submitNote = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("note", validateNote(noteForm))) {
      return;
    }

    await runSharedEdit(async () => {
      const payload = createNoteRequestModel(noteForm, tripPlan?.id);

      if (editingNoteId) {
        await sharedTripPlanService.saveNote(token, editingNoteId, {
          title: payload.title,
          content: payload.content,
        });
      } else {
        await sharedTripPlanService.saveNote(token, null, payload);
      }

      cancelNoteEdit();
    }, editingNoteId ? "Beleska je izmenjena." : "Beleska je dodata.");
  };

  const editNote = (note) => {
    if (!canEdit) {
      return;
    }

    setEditingNoteId(note.id);
    clearFormErrors("note");
    setNoteForm(createNoteFormModel(note));
  };

  const cancelNoteEdit = () => {
    setEditingNoteId(null);
    setNoteForm(createNoteFormModel());
    clearFormErrors("note");
  };

  const deleteNote = async (noteId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteNote(token, noteId);
      if (editingNoteId === noteId) {
        cancelNoteEdit();
      }
    }, "Beleska je obrisana.");
  };

  const submitReminder = async (event) => {
    event.preventDefault();

    if (stopInvalidSubmit("reminder", validateReminder(reminderForm))) {
      return;
    }

    await runSharedEdit(async () => {
      const payload = createReminderRequestModel(reminderForm, tripPlan?.id);

      if (editingReminderId) {
        await sharedTripPlanService.saveReminder(token, editingReminderId, createReminderUpdateRequestModel({
          title: payload.title,
          description: payload.description,
          reminderAt: payload.reminderAt,
          isCompleted: Boolean(reminderForm.isCompleted),
        }));
      } else {
        const created = await sharedTripPlanService.saveReminder(token, null, payload);
        if (reminderForm.isCompleted && created?.id) {
          await sharedTripPlanService.saveReminder(token, created.id, createReminderUpdateRequestModel({
            title: payload.title,
            description: payload.description,
            reminderAt: payload.reminderAt,
            isCompleted: true,
          }));
        }
      }

      cancelReminderEdit();
    }, editingReminderId ? "Podsjetnik je izmenjen." : "Podsjetnik je dodat.");
  };

  const editReminder = (reminder) => {
    if (!canEdit) {
      return;
    }

    setEditingReminderId(reminder.id);
    clearFormErrors("reminder");
    setReminderForm(createReminderFormModel(reminder));
  };

  const cancelReminderEdit = () => {
    setEditingReminderId(null);
    setReminderForm(createReminderFormModel());
    clearFormErrors("reminder");
  };

  const toggleReminder = async (reminder, isCompleted) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.saveReminder(token, reminder.id, createReminderUpdateRequestModel({
        title: reminder.title,
        description: reminder.description || null,
        reminderAt: reminder.reminderAt,
        isCompleted,
      }));
    }, "Podsjetnik je izmenjen.");
  };

  const deleteReminder = async (reminderId) => {
    await runSharedEdit(async () => {
      await sharedTripPlanService.deleteReminder(token, reminderId);
      if (editingReminderId === reminderId) {
        cancelReminderEdit();
      }
    }, "Podsjetnik je obrisan.");
  };

  return (
    <div className="public-layout">
      <header className="public-topbar">
        <Link className="public-brand" to="/login">
          <span className="brand-mark" aria-hidden="true">
            <Plane />
          </span>
          <span className="brand-text">
            <span className="brand-title">Travel Planner</span>
            <span className="brand-subtitle">Deljeni plan putovanja</span>
          </span>
        </Link>
        <Link className="btn btn-secondary" to="/login">
          <LogIn className="btn-icon" aria-hidden="true" />
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
                      <h2 className="section-title section-title-row">
                        <Info className="section-title-icon" aria-hidden="true" />
                        Osnovni podaci
                      </h2>
                      <p className="section-subtitle">
                        {canEdit ? "Osnovni podaci se mogu izmeniti preko ovog linka." : "Link je samo za pregled."}
                      </p>
                    </div>
                  </div>

                  {canEdit ? (
                    <form className="form-grid" noValidate onSubmit={updateSharedTripPlan}>
                      <label className="field">
                        <span className="field-label">Naziv plana</span>
                        <input
                          className={`input${formErrors.tripPlan.title ? " input-error" : ""}`}
                          name="title"
                          onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                          required
                          value={tripPlanForm.title}
                        />
                        <FormFieldError message={formErrors.tripPlan.title} />
                      </label>

                      <label className="field">
                        <span className="field-label">Opis</span>
                        <textarea
                          className="textarea"
                          name="description"
                          onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                          rows={3}
                          value={tripPlanForm.description}
                        />
                      </label>

                      <div className="form-row">
                        <label className="field">
                          <span className="field-label">Pocetak</span>
                          <input
                            className={`input${formErrors.tripPlan.startDate ? " input-error" : ""}`}
                            name="startDate"
                            onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                            required
                            type="date"
                            value={tripPlanForm.startDate}
                          />
                          <FormFieldError message={formErrors.tripPlan.startDate} />
                        </label>
                        <label className="field">
                          <span className="field-label">Kraj</span>
                          <input
                            className={`input${formErrors.tripPlan.endDate ? " input-error" : ""}`}
                            name="endDate"
                            onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                            required
                            type="date"
                            value={tripPlanForm.endDate}
                          />
                          <FormFieldError message={formErrors.tripPlan.endDate} />
                        </label>
                      </div>

                      <label className="field">
                        <span className="field-label">Planirani budzet</span>
                        <input
                          className={`input${formErrors.tripPlan.plannedBudget ? " input-error" : ""}`}
                          min="0"
                          name="plannedBudget"
                          onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                          step="0.01"
                          type="number"
                          value={tripPlanForm.plannedBudget}
                        />
                        <FormFieldError message={formErrors.tripPlan.plannedBudget} />
                      </label>

                      <label className="field">
                        <span className="field-label">Napomene</span>
                        <textarea
                          className="textarea"
                          name="notes"
                          onChange={(event) => updateSharedFormField("tripPlan", setTripPlanForm, event)}
                          rows={3}
                          value={tripPlanForm.notes}
                        />
                      </label>

                      <button className="btn btn-primary" type="submit">
                        <Save className="btn-icon" aria-hidden="true" />
                        Sacuvaj izmene
                      </button>
                    </form>
                  ) : (
                    <TripPlanReadOnly tripPlan={tripPlan} />
                  )}
                </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title section-title-row">
                        <Wallet className="section-title-icon" aria-hidden="true" />
                        Troskovi i budzet
                      </h2>
                      <p className="section-subtitle">Planirani budzet, ukupni troskovi i preostali iznos.</p>
                    </div>
                    <span className="badge">{expenses.length}</span>
                  </div>

                  <div className="overview-grid">
                    <span className="stat-tile">
                      <span className="stat-label">
                        <Wallet className="stat-icon" aria-hidden="true" />
                        Planirani budzet
                      </span>
                      <span className="stat-value">{formatMoney(plannedBudget)}</span>
                    </span>
                    <span className="stat-tile">
                      <span className="stat-label">
                        <Receipt className="stat-icon" aria-hidden="true" />
                        Ukupno potroseno
                      </span>
                      <span className="stat-value">{formatMoney(totalExpenses)}</span>
                    </span>
                    <span className="stat-tile">
                      <span className="stat-label">
                        <PiggyBank className="stat-icon" aria-hidden="true" />
                        Preostali budzet
                      </span>
                      <span className="stat-value">{formatMoney(remainingBudget)}</span>
                    </span>
                  </div>

                  {canEdit ? (
                    <form className="form-grid" noValidate onSubmit={submitExpense}>
                      <label className="field">
                        <span className="field-label">Naziv troska</span>
                        <input
                          className={`input${formErrors.expense.title ? " input-error" : ""}`}
                          name="title"
                          onChange={(event) => updateSharedFormField("expense", setExpenseForm, event)}
                          required
                          value={expenseForm.title}
                        />
                        <FormFieldError message={formErrors.expense.title} />
                      </label>

                      <div className="form-row">
                        <label className="field">
                          <span className="field-label">Kategorija</span>
                          <select
                            className={`select${formErrors.expense.category ? " input-error" : ""}`}
                            name="category"
                            onChange={(event) => updateSharedFormField("expense", setExpenseForm, event)}
                            required
                            value={expenseForm.category}
                          >
                            {EXPENSE_CATEGORIES.map((category) => (
                              <option key={category.value} value={category.value}>
                                {category.label}
                              </option>
                            ))}
                          </select>
                          <FormFieldError message={formErrors.expense.category} />
                        </label>
                        <label className="field">
                          <span className="field-label">Iznos</span>
                          <input
                            className={`input${formErrors.expense.amount ? " input-error" : ""}`}
                            min="0"
                            name="amount"
                            onChange={(event) => updateSharedFormField("expense", setExpenseForm, event)}
                            required
                            step="0.01"
                            type="number"
                            value={expenseForm.amount}
                          />
                          <FormFieldError message={formErrors.expense.amount} />
                        </label>
                      </div>

                      <label className="field">
                        <span className="field-label">Datum</span>
                        <input
                          className={`input${formErrors.expense.expenseDate ? " input-error" : ""}`}
                          name="expenseDate"
                          onChange={(event) => updateSharedFormField("expense", setExpenseForm, event)}
                          required
                          type="date"
                          value={expenseForm.expenseDate}
                        />
                        <FormFieldError message={formErrors.expense.expenseDate} />
                      </label>

                      <label className="field">
                        <span className="field-label">Opis</span>
                        <textarea
                          className="textarea"
                          name="description"
                          onChange={(event) => updateSharedFormField("expense", setExpenseForm, event)}
                          rows={3}
                          value={expenseForm.description}
                        />
                      </label>

                      <div className="button-row">
                        <button className="btn btn-primary" type="submit">
                          <ExpenseSubmitIcon className="btn-icon" aria-hidden="true" />
                          {editingExpenseId ? "Sacuvaj trosak" : "Dodaj trosak"}
                        </button>
                        {editingExpenseId ? (
                          <button className="btn btn-secondary" onClick={cancelExpenseEdit} type="button">
                            <X className="btn-icon" aria-hidden="true" />
                            Odustani
                          </button>
                        ) : null}
                      </div>
                    </form>
                  ) : null}

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
                        {canEdit ? (
                          <div className="list-item-actions">
                            <button className="btn btn-secondary btn-small" onClick={() => editExpense(expense)} type="button">
                              <Pencil className="btn-icon" aria-hidden="true" />
                              Izmeni
                            </button>
                            <button className="btn btn-danger-soft btn-small" onClick={() => deleteExpense(expense.id)} type="button">
                              <Trash2 className="btn-icon" aria-hidden="true" />
                              Obrisi
                            </button>
                          </div>
                        ) : null}
                      </article>
                    ))}
                    {expenses.length === 0 ? <div className="empty-state">Nema unetih troskova.</div> : null}
                  </div>
                </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title section-title-row">
                        <ListChecks className="section-title-icon" aria-hidden="true" />
                        Checklist
                      </h2>
                      <p className="section-subtitle">Stavke za pripremu i njihov status.</p>
                    </div>
                    <span className="badge">{checklistItems.length}</span>
                  </div>

                  {canEdit ? (
                    <form className="form-grid" noValidate onSubmit={submitChecklistItem}>
                      <label className="field">
                        <span className="field-label">Stavka</span>
                        <input
                          className={`input${formErrors.checklist.title ? " input-error" : ""}`}
                          maxLength={150}
                          name="title"
                          onChange={(event) => updateSharedFormField("checklist", setChecklistForm, event)}
                          required
                          value={checklistForm.title}
                        />
                        <FormFieldError message={formErrors.checklist.title} />
                      </label>

                      <div className="button-row">
                        <button className="btn btn-primary" type="submit">
                          <ChecklistSubmitIcon className="btn-icon" aria-hidden="true" />
                          {editingChecklistItemId ? "Sacuvaj stavku" : "Dodaj stavku"}
                        </button>
                        {editingChecklistItemId ? (
                          <button className="btn btn-secondary" onClick={cancelChecklistEdit} type="button">
                            <X className="btn-icon" aria-hidden="true" />
                            Odustani
                          </button>
                        ) : null}
                      </div>
                    </form>
                  ) : null}

                  <div className="item-list">
                    {checklistItems.map((checklistItem) => (
                      <article className="list-item checklist-item" key={checklistItem.id}>
                        <label className="checklist-label">
                          <input
                            checked={Boolean(checklistItem.isCompleted)}
                            disabled={!canEdit}
                            onChange={(event) => toggleChecklistItem(checklistItem, event.target.checked)}
                            readOnly={!canEdit}
                            type="checkbox"
                          />
                          <span className={`checklist-title${checklistItem.isCompleted ? " is-done" : ""}`}>
                            {checklistItem.title}
                          </span>
                        </label>
                        {canEdit ? (
                          <div className="list-item-actions">
                            <button
                              className="btn btn-secondary btn-small"
                              onClick={() => editChecklistItem(checklistItem)}
                              type="button"
                            >
                              <Pencil className="btn-icon" aria-hidden="true" />
                              Izmeni
                            </button>
                            <button
                              className="btn btn-danger-soft btn-small"
                              onClick={() => deleteChecklistItem(checklistItem.id)}
                              type="button"
                            >
                              <Trash2 className="btn-icon" aria-hidden="true" />
                              Obrisi
                            </button>
                          </div>
                        ) : (
                          <span className={`badge${checklistItem.isCompleted ? " badge-success" : " badge-muted"}`}>
                            {checklistItem.isCompleted ? "Zavrseno" : "Nije zavrseno"}
                          </span>
                        )}
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
                      <h2 className="section-title section-title-row">
                        <MapPinned className="section-title-icon" aria-hidden="true" />
                        Destinacije
                      </h2>
                      <p className="section-subtitle">Mesta iz deljenog plana.</p>
                    </div>
                    <span className="badge">{destinations.length}</span>
                  </div>

                  {canEdit ? (
                    <form className="form-grid" noValidate onSubmit={submitDestination}>
                      <label className="field">
                        <span className="field-label">Naziv destinacije</span>
                        <input
                          className={`input${formErrors.destination.name ? " input-error" : ""}`}
                          name="name"
                          onChange={(event) => updateSharedFormField("destination", setDestinationForm, event)}
                          required
                          value={destinationForm.name}
                        />
                        <FormFieldError message={formErrors.destination.name} />
                      </label>

                      <label className="field">
                        <span className="field-label">Lokacija</span>
                        <input
                          className="input"
                          name="location"
                          onChange={(event) => updateSharedFormField("destination", setDestinationForm, event)}
                          value={destinationForm.location}
                        />
                      </label>

                      <div className="form-row">
                        <label className="field">
                          <span className="field-label">Dolazak</span>
                          <input
                            className={`input${formErrors.destination.arrivalDate ? " input-error" : ""}`}
                            name="arrivalDate"
                            onChange={(event) => updateSharedFormField("destination", setDestinationForm, event)}
                            required
                            type="date"
                            value={destinationForm.arrivalDate}
                          />
                          <FormFieldError message={formErrors.destination.arrivalDate} />
                        </label>
                        <label className="field">
                          <span className="field-label">Odlazak</span>
                          <input
                            className={`input${formErrors.destination.departureDate ? " input-error" : ""}`}
                            name="departureDate"
                            onChange={(event) => updateSharedFormField("destination", setDestinationForm, event)}
                            required
                            type="date"
                            value={destinationForm.departureDate}
                          />
                          <FormFieldError message={formErrors.destination.departureDate} />
                        </label>
                      </div>

                      <label className="field">
                        <span className="field-label">Opis</span>
                        <textarea
                          className="textarea"
                          name="description"
                          onChange={(event) => updateSharedFormField("destination", setDestinationForm, event)}
                          rows={3}
                          value={destinationForm.description}
                        />
                      </label>

                      <div className="button-row">
                        <button className="btn btn-primary" type="submit">
                          <DestinationSubmitIcon className="btn-icon" aria-hidden="true" />
                          {editingDestinationId ? "Sacuvaj destinaciju" : "Dodaj destinaciju"}
                        </button>
                        {editingDestinationId ? (
                          <button className="btn btn-secondary" onClick={cancelDestinationEdit} type="button">
                            <X className="btn-icon" aria-hidden="true" />
                            Odustani
                          </button>
                        ) : null}
                      </div>
                    </form>
                  ) : null}

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
                        {canEdit ? (
                          <div className="list-item-actions">
                            <button
                              className="btn btn-secondary btn-small"
                              onClick={() => editDestination(destination)}
                              type="button"
                            >
                              <Pencil className="btn-icon" aria-hidden="true" />
                              Izmeni
                            </button>
                            <button
                              className="btn btn-danger-soft btn-small"
                              onClick={() => deleteDestination(destination.id)}
                              type="button"
                            >
                              <Trash2 className="btn-icon" aria-hidden="true" />
                              Obrisi
                            </button>
                          </div>
                        ) : null}
                      </article>
                    ))}
                    {destinations.length === 0 ? <div className="empty-state">Nema destinacija za prikaz.</div> : null}
                  </div>
                </section>

                <section className="section-card">
                  <div className="section-header">
                    <div>
                      <h2 className="section-title section-title-row">
                        <CalendarDays className="section-title-icon" aria-hidden="true" />
                        Aktivnosti
                      </h2>
                      <p className="section-subtitle">Lista ili kalendarski prikaz aktivnosti po danima.</p>
                    </div>
                    <span className="badge">{activities.length}</span>
                  </div>

                  {canEdit ? (
                    <form className="form-grid" noValidate onSubmit={submitActivity}>
                      <label className="field">
                        <span className="field-label">Naziv aktivnosti</span>
                        <input
                          className={`input${formErrors.activity.title ? " input-error" : ""}`}
                          name="title"
                          onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                          required
                          value={activityForm.title}
                        />
                        <FormFieldError message={formErrors.activity.title} />
                      </label>

                      <div className="form-row">
                        <label className="field">
                          <span className="field-label">Datum</span>
                          <input
                            className={`input${formErrors.activity.activityDate ? " input-error" : ""}`}
                            name="activityDate"
                            onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                            required
                            type="date"
                            value={activityForm.activityDate}
                          />
                          <FormFieldError message={formErrors.activity.activityDate} />
                        </label>
                        <label className="field">
                          <span className="field-label">Vreme</span>
                          <input
                            className="input"
                            name="activityTime"
                            onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                            type="time"
                            value={activityForm.activityTime}
                          />
                        </label>
                      </div>

                      <label className="field">
                        <span className="field-label">Lokacija</span>
                        <input
                          className="input"
                          name="location"
                          onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                          value={activityForm.location}
                        />
                      </label>

                      <label className="field">
                        <span className="field-label">Opis</span>
                        <textarea
                          className="textarea"
                          name="description"
                          onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                          rows={3}
                          value={activityForm.description}
                        />
                      </label>

                      <div className="form-row">
                        <label className="field">
                          <span className="field-label">Procena troska</span>
                          <input
                            className={`input${formErrors.activity.estimatedCost ? " input-error" : ""}`}
                            min="0"
                            name="estimatedCost"
                            onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                            step="0.01"
                            type="number"
                            value={activityForm.estimatedCost}
                          />
                          <FormFieldError message={formErrors.activity.estimatedCost} />
                        </label>
                        <label className="field">
                          <span className="field-label">Status</span>
                          <select
                            className={`select${formErrors.activity.status ? " input-error" : ""}`}
                            name="status"
                            onChange={(event) => updateSharedFormField("activity", setActivityForm, event)}
                            value={activityForm.status}
                          >
                            {ACTIVITY_STATUSES.map((status) => (
                              <option key={status.value} value={status.value}>
                                {status.label}
                              </option>
                            ))}
                          </select>
                          <FormFieldError message={formErrors.activity.status} />
                        </label>
                      </div>

                      <div className="button-row">
                        <button className="btn btn-primary" type="submit">
                          <ActivitySubmitIcon className="btn-icon" aria-hidden="true" />
                          {editingActivityId ? "Sacuvaj aktivnost" : "Dodaj aktivnost"}
                        </button>
                        {editingActivityId ? (
                          <button className="btn btn-secondary" onClick={cancelActivityEdit} type="button">
                            <X className="btn-icon" aria-hidden="true" />
                            Odustani
                          </button>
                        ) : null}
                      </div>
                    </form>
                  ) : null}

                  <div className="activity-view-toggle" aria-label="Prikaz aktivnosti" role="group">
                    <button
                      aria-pressed={activityViewMode === "list"}
                      className={`btn btn-small btn-toggle${activityViewMode === "list" ? " is-active" : ""}`}
                      onClick={() => setActivityViewMode("list")}
                      type="button"
                    >
                      <List className="btn-icon" aria-hidden="true" />
                      Lista
                    </button>
                    <button
                      aria-pressed={activityViewMode === "calendar"}
                      className={`btn btn-small btn-toggle${activityViewMode === "calendar" ? " is-active" : ""}`}
                      onClick={() => setActivityViewMode("calendar")}
                      type="button"
                    >
                      <Calendar className="btn-icon" aria-hidden="true" />
                      Kalendar
                    </button>
                  </div>

                  {activityViewMode === "list" ? (
                    <div className="item-list">
                      {sortedActivities.map((activity) => (
                        <article className="list-item" key={activity.id}>
                          <div className="list-item-main">
                            <span className="list-item-title">{activity.title}</span>
                            <p className="muted">
                              {formatDate(activity.activityDate)}
                              {activity.activityTime ? ` - ${String(activity.activityTime).slice(0, 5)}` : ""}
                            </p>
                            <span className={`badge ${getStatusClass(activity.status)}`}>{getStatusLabel(activity.status)}</span>
                            {activity.location ? <p className="muted">{activity.location}</p> : null}
                            {activity.estimatedCost ? <p className="muted">{formatMoney(activity.estimatedCost)}</p> : null}
                            {activity.description ? <p className="list-item-description">{activity.description}</p> : null}
                          </div>
                          {canEdit ? (
                            <div className="list-item-actions">
                              <button className="btn btn-secondary btn-small" onClick={() => editActivity(activity)} type="button">
                                <Pencil className="btn-icon" aria-hidden="true" />
                                Izmeni
                              </button>
                              <button className="btn btn-danger-soft btn-small" onClick={() => deleteActivity(activity.id)} type="button">
                                <Trash2 className="btn-icon" aria-hidden="true" />
                                Obrisi
                              </button>
                            </div>
                          ) : null}
                        </article>
                      ))}
                      {activities.length === 0 ? <div className="empty-state">Nema aktivnosti za prikaz.</div> : null}
                    </div>
                  ) : (
                    <ActivityCalendar activities={activities} initialDate={tripPlan?.startDate} />
                  )}
                </section>

                <RemindersSection
                  canEdit={canEdit}
                  editingReminderId={editingReminderId}
                  errors={formErrors.reminder}
                  form={reminderForm}
                  onCancelEdit={cancelReminderEdit}
                  onChange={updateSharedReminderField}
                  onDelete={deleteReminder}
                  onEdit={editReminder}
                  onSubmit={submitReminder}
                  onToggle={toggleReminder}
                  reminders={sortedReminders}
                />

                {hasNotesSection ? (
                  <section className="section-card">
                    <div className="section-header">
                      <div>
                        <h2 className="section-title section-title-row">
                          <NotebookText className="section-title-icon" aria-hidden="true" />
                          Beleske
                        </h2>
                        <p className="section-subtitle">Notes stavke vezane za deljeni plan.</p>
                      </div>
                      <span className="badge">{noteItems.length}</span>
                    </div>

                    {canEdit ? (
                      <form className="form-grid" noValidate onSubmit={submitNote}>
                        <label className="field">
                          <span className="field-label">Naslov</span>
                          <input
                            className={`input${formErrors.note.title ? " input-error" : ""}`}
                            maxLength={150}
                            name="title"
                            onChange={(event) => updateSharedFormField("note", setNoteForm, event)}
                            required
                            value={noteForm.title}
                          />
                          <FormFieldError message={formErrors.note.title} />
                        </label>

                        <label className="field">
                          <span className="field-label">Sadrzaj</span>
                          <textarea
                            className="textarea"
                            name="content"
                            onChange={(event) => updateSharedFormField("note", setNoteForm, event)}
                            rows={3}
                            value={noteForm.content}
                          />
                        </label>

                        <div className="button-row">
                          <button className="btn btn-primary" type="submit">
                            <NoteSubmitIcon className="btn-icon" aria-hidden="true" />
                            {editingNoteId ? "Sacuvaj belesku" : "Dodaj belesku"}
                          </button>
                          {editingNoteId ? (
                            <button className="btn btn-secondary" onClick={cancelNoteEdit} type="button">
                              <X className="btn-icon" aria-hidden="true" />
                              Odustani
                            </button>
                          ) : null}
                        </div>
                      </form>
                    ) : null}

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
                          {canEdit ? (
                            <div className="list-item-actions">
                              <button className="btn btn-secondary btn-small" onClick={() => editNote(note)} type="button">
                                <Pencil className="btn-icon" aria-hidden="true" />
                                Izmeni
                              </button>
                              <button className="btn btn-danger-soft btn-small" onClick={() => deleteNote(note.id)} type="button">
                                <Trash2 className="btn-icon" aria-hidden="true" />
                                Obrisi
                              </button>
                            </div>
                          ) : null}
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

function TripPlanReadOnly({ tripPlan }) {
  return (
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
  );
}

function resetEditState(
  setEditingDestinationId,
  setEditingActivityId,
  setEditingExpenseId,
  setEditingChecklistItemId,
  setEditingNoteId,
  setEditingReminderId,
  setDestinationForm,
  setActivityForm,
  setExpenseForm,
  setChecklistForm,
  setNoteForm,
  setReminderForm,
) {
  setEditingDestinationId(null);
  setEditingActivityId(null);
  setEditingExpenseId(null);
  setEditingChecklistItemId(null);
  setEditingNoteId(null);
  setEditingReminderId(null);
  setDestinationForm(createDestinationFormModel());
  setActivityForm(createActivityFormModel());
  setExpenseForm(createExpenseFormModel());
  setChecklistForm(createChecklistItemFormModel());
  setNoteForm(createNoteFormModel());
  setReminderForm(createReminderFormModel());
}

function updateForm(setForm, event) {
  const { name, value } = event.target;
  setForm((currentForm) => ({
    ...currentForm,
    [name]: value,
  }));
}

function compareActivities(first, second) {
  const dateResult = compareDates(first.activityDate, second.activityDate);
  if (dateResult !== 0) {
    return dateResult;
  }

  return String(first.activityTime ?? "").localeCompare(String(second.activityTime ?? ""));
}

function compareReminders(first, second) {
  if (Boolean(first.isCompleted) !== Boolean(second.isCompleted)) {
    return first.isCompleted ? 1 : -1;
  }

  return compareDates(first.reminderAt, second.reminderAt);
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
  return ACTIVITY_STATUSES.find((status) => status.value === numericValue)?.label ?? "Planned";
}

function getStatusClass(value) {
  const numericValue = Number(value);

  if (numericValue === ACTIVITY_STATUS.RESERVED) {
    return "status-reserved";
  }

  if (numericValue === ACTIVITY_STATUS.COMPLETED) {
    return "status-completed";
  }

  if (numericValue === ACTIVITY_STATUS.CANCELLED) {
    return "status-cancelled";
  }

  return "status-planned";
}

function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}

function getRequestErrorMessage(requestError) {
  return requestError?.message || "Izmena nije uspela.";
}
