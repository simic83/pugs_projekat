import { useEffect, useMemo, useState } from "react";
import {
  BellRing,
  CalendarDays,
  FileText,
  Info,
  ListChecks,
  MapPinned,
  NotebookText,
  Plus,
  Share2,
  Trash2,
  Wallet,
} from "lucide-react";
import { Link } from "react-router-dom";
import { groupActivitiesByDate } from "../components/ActivityCalendar.jsx";
import { ActivitiesSection } from "../components/trips/ActivitiesSection.jsx";
import { ChecklistSection } from "../components/trips/ChecklistSection.jsx";
import { DestinationsSection } from "../components/trips/DestinationsSection.jsx";
import { EmptyState } from "../components/trips/EmptyState.jsx";
import { ExpensesSection } from "../components/trips/ExpensesSection.jsx";
import { NotesSection } from "../components/trips/NotesSection.jsx";
import { RemindersSection } from "../components/trips/RemindersSection.jsx";
import { SharingSection } from "../components/trips/SharingSection.jsx";
import { TripPlanDetails } from "../components/trips/TripPlanDetails.jsx";
import { TripPlanForm } from "../components/trips/TripPlanForm.jsx";
import { TripPlanList } from "../components/trips/TripPlanList.jsx";
import {
  buildSharedTripPlanLink,
  compareDates,
  formatDateRange,
  formatMoney,
} from "../components/trips/tripDisplayUtils.js";
import { useApp } from "../context/AppContext.jsx";
import { createExpenseFormModel, createExpenseRequestModel } from "../models/budget.js";
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
import { SHARE_ACCESS_LEVELS, createShareRequestModel } from "../models/sharing.js";
import {
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
  validateShare,
  validateTripPlan,
} from "../utils/validation.js";

const emptyFormErrors = {
  tripPlan: {},
  selectedTripPlan: {},
  destination: {},
  activity: {},
  expense: {},
  checklist: {},
  note: {},
  reminder: {},
  share: {},
};

const workspaceTabs = [
  { id: "overview", label: "Pregled", Icon: Info },
  { id: "route", label: "Ruta", Icon: MapPinned },
  { id: "schedule", label: "Raspored", Icon: CalendarDays },
  { id: "budget", label: "Budzet", Icon: Wallet },
  { id: "prep", label: "Priprema", Icon: ListChecks },
  { id: "notes", label: "Beleske", Icon: NotebookText },
  { id: "sharing", label: "Deljenje", Icon: Share2 },
];

export function TripPlansPage() {
  const {
    tripPlans,
    selectedTripPlanId,
    selectedTripPlan,
    destinations,
    activities,
    expenses,
    checklistItems,
    notes,
    reminders,
    shares,
    budgetSummary,
    error,
    message,
    isLoading,
    setError,
    setMessage,
    selectTripPlan: selectTripPlanFromContext,
    createTripPlan,
    updateTripPlan,
    deleteTripPlan: removeTripPlan,
    saveDestination,
    deleteDestination: removeDestination,
    saveActivity,
    deleteActivity: removeActivity,
    saveExpense,
    deleteExpense: removeExpense,
    saveChecklistItem,
    deleteChecklistItem: removeChecklistItem,
    saveNote,
    deleteNote: removeNote,
    saveReminder,
    deleteReminder: removeReminder,
    createShareToken,
    revokeShareToken,
  } = useApp();
  const [tripPlanForm, setTripPlanForm] = useState(createTripPlanFormModel);
  const [selectedTripPlanForm, setSelectedTripPlanForm] = useState(createTripPlanFormModel);
  const [destinationForm, setDestinationForm] = useState(createDestinationFormModel);
  const [activityForm, setActivityForm] = useState(createActivityFormModel);
  const [expenseForm, setExpenseForm] = useState(createExpenseFormModel);
  const [checklistForm, setChecklistForm] = useState(createChecklistItemFormModel);
  const [noteForm, setNoteForm] = useState(createNoteFormModel);
  const [reminderForm, setReminderForm] = useState(createReminderFormModel);
  const [shareAccessLevel, setShareAccessLevel] = useState(SHARE_ACCESS_LEVELS.VIEW);
  const [shareExpiresAt, setShareExpiresAt] = useState("");
  const [generatedShareLink, setGeneratedShareLink] = useState("");
  const [visibleShareQrId, setVisibleShareQrId] = useState(null);
  const [activityViewMode, setActivityViewMode] = useState("list");
  const [activeTripTab, setActiveTripTab] = useState("overview");
  const [isCreatePanelOpen, setIsCreatePanelOpen] = useState(false);
  const [editingDestinationId, setEditingDestinationId] = useState(null);
  const [editingActivityId, setEditingActivityId] = useState(null);
  const [editingExpenseId, setEditingExpenseId] = useState(null);
  const [editingChecklistItemId, setEditingChecklistItemId] = useState(null);
  const [editingNoteId, setEditingNoteId] = useState(null);
  const [editingReminderId, setEditingReminderId] = useState(null);
  const [formErrors, setFormErrors] = useState(emptyFormErrors);

  const resetTripPlanUiState = () => {
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
    setGeneratedShareLink("");
    setVisibleShareQrId(null);
    setActivityViewMode("list");
    setFormErrors(emptyFormErrors);
  };

  useEffect(() => {
    setSelectedTripPlanForm(createTripPlanFormModel(selectedTripPlan));

    if (!selectedTripPlan) {
      resetTripPlanUiState();
    }
  }, [selectedTripPlan]);

  useEffect(() => {
    if (!isLoading && tripPlans.length === 0) {
      setIsCreatePanelOpen(true);
    }
  }, [isLoading, tripPlans.length]);

  const sortedDestinations = useMemo(
    () => [...destinations].sort((first, second) => compareDates(first.arrivalDate, second.arrivalDate)),
    [destinations],
  );

  const activityGroups = useMemo(() => groupActivitiesByDate(activities), [activities]);

  const tabCounts = useMemo(
    () => ({
      route: sortedDestinations.length,
      schedule: activities.length,
      budget: expenses.length,
      prep: reminders.length + checklistItems.length,
      notes: notes.length,
      sharing: shares.length,
    }),
    [
      activities.length,
      checklistItems.length,
      expenses.length,
      notes.length,
      reminders.length,
      shares.length,
      sortedDestinations.length,
    ],
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

  const updateTripPlanField = (event) => {
    clearFormFieldError("tripPlan", event.target.name);
    updateForm(setTripPlanForm, event);
  };

  const updateSelectedTripPlanField = (event) => {
    clearFormFieldError("selectedTripPlan", event.target.name);
    updateForm(setSelectedTripPlanForm, event);
  };

  const updateDestinationField = (event) => {
    clearFormFieldError("destination", event.target.name);
    updateForm(setDestinationForm, event);
  };

  const updateActivityField = (event) => {
    clearFormFieldError("activity", event.target.name);
    updateForm(setActivityForm, event);
  };

  const updateExpenseField = (event) => {
    clearFormFieldError("expense", event.target.name);
    updateForm(setExpenseForm, event);
  };

  const updateChecklistField = (event) => {
    clearFormFieldError("checklist", event.target.name);
    updateForm(setChecklistForm, event);
  };

  const updateNoteField = (event) => {
    clearFormFieldError("note", event.target.name);
    updateForm(setNoteForm, event);
  };

  const updateReminderField = (event) => {
    const { checked, name, type, value } = event.target;
    clearFormFieldError("reminder", name);
    setReminderForm((currentForm) => ({
      ...currentForm,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const updateShareAccessLevel = (accessLevel) => {
    clearFormFieldError("share", "accessLevel");
    setShareAccessLevel(accessLevel);
  };

  const updateShareExpiresAt = (expiresAt) => {
    clearFormFieldError("share", "expiresAt");
    setShareExpiresAt(expiresAt);
  };

  const selectTripPlan = (tripPlanId) => {
    selectTripPlanFromContext(tripPlanId);
    setActiveTripTab("overview");
    resetTripPlanUiState();
  };

  const submitTripPlan = async (event) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (stopInvalidSubmit("tripPlan", validateTripPlan(tripPlanForm))) {
      return;
    }

    try {
      await createTripPlan(createTripPlanRequestModel(tripPlanForm));
      setTripPlanForm(createTripPlanFormModel());
      setActiveTripTab("overview");
      setIsCreatePanelOpen(false);
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const deleteTripPlan = async (tripPlanId) => {
    setError("");
    setMessage("");

    try {
      await removeTripPlan(tripPlanId);
      resetTripPlanUiState();
      setActiveTripTab("overview");
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const updateSelectedTripPlan = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("selectedTripPlan", validateTripPlan(selectedTripPlanForm))) {
      return;
    }

    try {
      await updateTripPlan(selectedTripPlanId, createTripPlanRequestModel(selectedTripPlanForm));
      setMessage("Osnovni podaci plana su sacuvani.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const resetSelectedTripPlanForm = () => {
    setSelectedTripPlanForm(createTripPlanFormModel(selectedTripPlan));
    clearFormErrors("selectedTripPlan");
    setMessage("");
  };

  const submitDestination = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("destination", validateDestination(destinationForm, selectedTripPlan))) {
      return;
    }

    try {
      const payload = createDestinationRequestModel(destinationForm);

      await saveDestination(selectedTripPlanId, editingDestinationId, payload);
      cancelDestinationEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editDestination = (destination) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await removeDestination(selectedTripPlanId, destinationId);
      if (editingDestinationId === destinationId) {
        cancelDestinationEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const submitActivity = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("activity", validateActivity(activityForm, selectedTripPlan))) {
      return;
    }

    try {
      const payload = createActivityRequestModel(activityForm);

      await saveActivity(selectedTripPlanId, editingActivityId, payload);
      cancelActivityEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editActivity = (activity) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await removeActivity(selectedTripPlanId, activityId);
      if (editingActivityId === activityId) {
        cancelActivityEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const submitExpense = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("expense", validateExpense(expenseForm))) {
      return;
    }

    try {
      const payload = createExpenseRequestModel(expenseForm, selectedTripPlanId);

      await saveExpense(selectedTripPlanId, editingExpenseId, payload);
      cancelExpenseEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editExpense = (expense) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await removeExpense(selectedTripPlanId, expenseId);
      if (editingExpenseId === expenseId) {
        cancelExpenseEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const submitChecklistItem = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("checklist", validateChecklistItem(checklistForm))) {
      return;
    }

    try {
      const payload = createChecklistItemRequestModel(checklistForm, selectedTripPlanId);

      if (editingChecklistItemId) {
        const checklistItem = checklistItems.find((item) => item.id === editingChecklistItemId);
        await saveChecklistItem(selectedTripPlanId, editingChecklistItemId, createChecklistItemUpdateRequestModel({
          ...payload,
          isCompleted: Boolean(checklistItem?.isCompleted),
        }));
      } else {
        await saveChecklistItem(selectedTripPlanId, null, payload);
      }

      cancelChecklistEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editChecklistItem = (checklistItem) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await saveChecklistItem(selectedTripPlanId, checklistItem.id, createChecklistItemUpdateRequestModel({
        title: checklistItem.title,
        isCompleted,
      }));
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const deleteChecklistItem = async (checklistItemId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await removeChecklistItem(selectedTripPlanId, checklistItemId);
      if (editingChecklistItemId === checklistItemId) {
        cancelChecklistEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const submitNote = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("note", validateNote(noteForm))) {
      return;
    }

    try {
      const payload = createNoteRequestModel(noteForm, selectedTripPlanId);

      if (editingNoteId) {
        await saveNote(selectedTripPlanId, editingNoteId, {
          title: payload.title,
          content: payload.content,
        });
      } else {
        await saveNote(selectedTripPlanId, null, payload);
      }

      cancelNoteEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editNote = (note) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await removeNote(selectedTripPlanId, noteId);
      if (editingNoteId === noteId) {
        cancelNoteEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const submitReminder = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("reminder", validateReminder(reminderForm))) {
      return;
    }

    try {
      const payload = createReminderRequestModel(reminderForm, selectedTripPlanId);

      if (editingReminderId) {
        await saveReminder(selectedTripPlanId, editingReminderId, createReminderUpdateRequestModel({
          title: payload.title,
          description: payload.description,
          reminderAt: payload.reminderAt,
          isCompleted: Boolean(reminderForm.isCompleted),
        }));
      } else {
        const created = await saveReminder(selectedTripPlanId, null, payload);
        if (reminderForm.isCompleted && created?.id) {
          await saveReminder(selectedTripPlanId, created.id, createReminderUpdateRequestModel({
            title: payload.title,
            description: payload.description,
            reminderAt: payload.reminderAt,
            isCompleted: true,
          }));
        }
      }

      cancelReminderEdit();
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editReminder = (reminder) => {
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
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await saveReminder(selectedTripPlanId, reminder.id, createReminderUpdateRequestModel({
        title: reminder.title,
        description: reminder.description || null,
        reminderAt: reminder.reminderAt,
        isCompleted,
      }));
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const deleteReminder = async (reminderId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await removeReminder(selectedTripPlanId, reminderId);
      if (editingReminderId === reminderId) {
        cancelReminderEdit();
      }
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const createShare = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    if (stopInvalidSubmit("share", validateShare({ accessLevel: shareAccessLevel, expiresAt: shareExpiresAt }))) {
      return;
    }

    try {
      const created = await createShareToken(
        selectedTripPlanId,
        createShareRequestModel({ accessLevel: shareAccessLevel, expiresAt: shareExpiresAt }, selectedTripPlanId),
      );

      setShareExpiresAt("");
      setGeneratedShareLink(buildSharedTripPlanLink(created.token));
      setVisibleShareQrId(created.id ?? null);
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const revokeShare = async (shareId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await revokeShareToken(selectedTripPlanId, shareId);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const renderActiveTripTab = () => {
    if (!selectedTripPlan) {
      return null;
    }

    if (activeTripTab === "route") {
      return (
        <DestinationsSection
          destinations={sortedDestinations}
          editingDestinationId={editingDestinationId}
          errors={formErrors.destination}
          form={destinationForm}
          onCancelEdit={cancelDestinationEdit}
          onChange={updateDestinationField}
          onDelete={deleteDestination}
          onEdit={editDestination}
          onSubmit={submitDestination}
          tripPlan={selectedTripPlan}
        />
      );
    }

    if (activeTripTab === "schedule") {
      return (
        <ActivitiesSection
          activities={activities}
          activityGroups={activityGroups}
          activityViewMode={activityViewMode}
          editingActivityId={editingActivityId}
          errors={formErrors.activity}
          form={activityForm}
          initialCalendarDate={selectedTripPlan.startDate}
          onCancelEdit={cancelActivityEdit}
          onChange={updateActivityField}
          onDelete={deleteActivity}
          onEdit={editActivity}
          onSubmit={submitActivity}
          onViewModeChange={setActivityViewMode}
          tripPlan={selectedTripPlan}
        />
      );
    }

    if (activeTripTab === "budget") {
      return (
        <ExpensesSection
          budgetSummary={budgetSummary}
          editingExpenseId={editingExpenseId}
          errors={formErrors.expense}
          expenses={expenses}
          form={expenseForm}
          onCancelEdit={cancelExpenseEdit}
          onChange={updateExpenseField}
          onDelete={deleteExpense}
          onEdit={editExpense}
          onSubmit={submitExpense}
          selectedTripPlan={selectedTripPlan}
        />
      );
    }

    if (activeTripTab === "prep") {
      return (
        <div className="prep-grid">
          <RemindersSection
            editingReminderId={editingReminderId}
            errors={formErrors.reminder}
            form={reminderForm}
            onCancelEdit={cancelReminderEdit}
            onChange={updateReminderField}
            onDelete={deleteReminder}
            onEdit={editReminder}
            onSubmit={submitReminder}
            onToggle={toggleReminder}
            reminders={reminders}
          />

          <ChecklistSection
            checklistItems={checklistItems}
            editingChecklistItemId={editingChecklistItemId}
            errors={formErrors.checklist}
            form={checklistForm}
            onCancelEdit={cancelChecklistEdit}
            onChange={updateChecklistField}
            onDelete={deleteChecklistItem}
            onEdit={editChecklistItem}
            onSubmit={submitChecklistItem}
            onToggle={toggleChecklistItem}
          />
        </div>
      );
    }

    if (activeTripTab === "notes") {
      return (
        <NotesSection
          editingNoteId={editingNoteId}
          errors={formErrors.note}
          form={noteForm}
          notes={notes}
          onCancelEdit={cancelNoteEdit}
          onChange={updateNoteField}
          onDelete={deleteNote}
          onEdit={editNote}
          onSubmit={submitNote}
        />
      );
    }

    if (activeTripTab === "sharing") {
      return (
        <SharingSection
          errors={formErrors.share}
          generatedShareLink={generatedShareLink}
          onAccessLevelChange={updateShareAccessLevel}
          onExpiresAtChange={updateShareExpiresAt}
          onRevoke={revokeShare}
          onSubmit={createShare}
          onToggleQr={setVisibleShareQrId}
          shareAccessLevel={shareAccessLevel}
          shareExpiresAt={shareExpiresAt}
          shares={shares}
          visibleShareQrId={visibleShareQrId}
        />
      );
    }

    return (
      <TripPlanDetails
        budgetSummary={budgetSummary}
        errors={formErrors.selectedTripPlan}
        form={selectedTripPlanForm}
        onChange={updateSelectedTripPlanField}
        onReset={resetSelectedTripPlanForm}
        onSubmit={updateSelectedTripPlan}
        tripPlan={selectedTripPlan}
      />
    );
  };

  return (
    <div className="page-stack">
      <header className="page-header">
        <div>
          <h1 className="page-title">Moji planovi</h1>
          <p className="page-subtitle">Planovi, ruta, raspored i budzet na jednom mestu.</p>
        </div>
        <button
          className="btn btn-primary"
          onClick={() => setIsCreatePanelOpen((currentValue) => !currentValue)}
          type="button"
        >
          <Plus className="btn-icon" aria-hidden="true" />
          {isCreatePanelOpen ? "Sakrij formu" : "Novi plan"}
        </button>
      </header>

      {error ? <p className="alert alert-error">{error}</p> : null}
      {message ? <p className="alert alert-success">{message}</p> : null}

      {isCreatePanelOpen ? (
        <section className="section-card trip-create-panel">
          <div className="section-header">
            <div>
              <h2 className="section-title">Novi plan putovanja</h2>
              <p className="section-subtitle">Naziv, datumi i okvirni budzet.</p>
            </div>
          </div>

          <TripPlanForm
            errors={formErrors.tripPlan}
            form={tripPlanForm}
            onChange={updateTripPlanField}
            onSubmit={submitTripPlan}
            submitLabel="Sacuvaj plan"
          />
        </section>
      ) : null}

      <section className="planner-layout">
        <aside className="planner-rail">
          <TripPlanList
            isLoading={isLoading}
            onDelete={deleteTripPlan}
            onSelect={selectTripPlan}
            selectedTripPlanId={selectedTripPlanId}
            tripPlans={tripPlans}
          />
        </aside>

        <section className="planner-workspace">
          {selectedTripPlan ? (
            <>
              <div className="workspace-hero">
                <div className="workspace-hero-main">
                  <span className="eyebrow">Izabrani plan</span>
                  <h2 className="workspace-title">{selectedTripPlan.title}</h2>
                  <p className="section-subtitle">
                    {formatDateRange(selectedTripPlan.startDate, selectedTripPlan.endDate)}
                  </p>
                </div>

                <div className="button-row workspace-actions">
                  <Link className="btn btn-secondary" to={`/trip-plans/${selectedTripPlan.id}/report`}>
                    <FileText className="btn-icon" aria-hidden="true" />
                    PDF izvestaj
                  </Link>
                  <button
                    className="btn btn-danger-soft"
                    onClick={() => deleteTripPlan(selectedTripPlan.id)}
                    type="button"
                  >
                    <Trash2 className="btn-icon" aria-hidden="true" />
                    Obrisi plan
                  </button>
                </div>

                <div className="workspace-metrics">
                  <span className="stat-tile">
                    <span className="stat-label">
                      <Wallet className="stat-icon" aria-hidden="true" />
                      Budzet
                    </span>
                    <span className="stat-value">
                      {formatMoney(budgetSummary?.plannedBudget ?? selectedTripPlan.plannedBudget)}
                    </span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">
                      <MapPinned className="stat-icon" aria-hidden="true" />
                      Destinacije
                    </span>
                    <span className="stat-value">{sortedDestinations.length}</span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">
                      <CalendarDays className="stat-icon" aria-hidden="true" />
                      Aktivnosti
                    </span>
                    <span className="stat-value">{activities.length}</span>
                  </span>
                  <span className="stat-tile">
                    <span className="stat-label">
                      <BellRing className="stat-icon" aria-hidden="true" />
                      Priprema
                    </span>
                    <span className="stat-value">{reminders.length + checklistItems.length}</span>
                  </span>
                </div>
              </div>

              <div className="workspace-tabs" aria-label="Delovi plana" role="tablist">
                {workspaceTabs.map(({ Icon, id, label }) => {
                  const count = tabCounts[id];

                  return (
                    <button
                      aria-selected={activeTripTab === id}
                      className={`workspace-tab${activeTripTab === id ? " is-active" : ""}`}
                      key={id}
                      onClick={() => setActiveTripTab(id)}
                      role="tab"
                      type="button"
                    >
                      <Icon className="workspace-tab-icon" aria-hidden="true" />
                      <span>{label}</span>
                      {typeof count === "number" ? <span className="tab-count">{count}</span> : null}
                    </button>
                  );
                })}
              </div>

              <div className="workspace-tab-panel" role="tabpanel">
                {renderActiveTripTab()}
              </div>
            </>
          ) : (
            <EmptyState>Kreiraj novi plan ili otvori sacuvani plan.</EmptyState>
          )}
        </section>
      </section>
    </div>
  );
}

function updateForm(setForm, event) {
  const { name, value } = event.target;
  setForm((currentForm) => ({
    ...currentForm,
    [name]: value,
  }));
}

function getRequestErrorMessage(requestError) {
  return requestError?.message || "Doslo je do greske.";
}
