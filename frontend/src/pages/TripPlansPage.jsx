import { useCallback, useEffect, useMemo, useState } from "react";
import { activitiesApi } from "../api/activitiesApi.js";
import { budgetApi } from "../api/budgetApi.js";
import { checklistApi } from "../api/checklistApi.js";
import { destinationsApi } from "../api/destinationsApi.js";
import { notesApi } from "../api/notesApi.js";
import { remindersApi } from "../api/remindersApi.js";
import { sharingApi } from "../api/sharingApi.js";
import { tripPlansApi } from "../api/tripPlansApi.js";
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
import { buildSharedTripPlanLink, compareDates } from "../components/trips/tripDisplayUtils.js";
import { SHARE_ACCESS_LEVELS } from "../models/sharing.js";
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

const emptyReminderForm = {
  title: "",
  description: "",
  reminderAt: "",
  isCompleted: false,
};

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

export function TripPlansPage() {
  const [tripPlans, setTripPlans] = useState([]);
  const [selectedTripPlanId, setSelectedTripPlanId] = useState(null);
  const [selectedTripPlan, setSelectedTripPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [checklistItems, setChecklistItems] = useState([]);
  const [notes, setNotes] = useState([]);
  const [reminders, setReminders] = useState([]);
  const [shares, setShares] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [tripPlanForm, setTripPlanForm] = useState(emptyTripPlanForm);
  const [selectedTripPlanForm, setSelectedTripPlanForm] = useState(emptyTripPlanForm);
  const [destinationForm, setDestinationForm] = useState(emptyDestinationForm);
  const [activityForm, setActivityForm] = useState(emptyActivityForm);
  const [expenseForm, setExpenseForm] = useState(emptyExpenseForm);
  const [checklistForm, setChecklistForm] = useState(emptyChecklistForm);
  const [noteForm, setNoteForm] = useState(emptyNoteForm);
  const [reminderForm, setReminderForm] = useState(emptyReminderForm);
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
  const [editingReminderId, setEditingReminderId] = useState(null);
  const [error, setError] = useState("");
  const [formErrors, setFormErrors] = useState(emptyFormErrors);
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
      setError(getRequestErrorMessage(requestError));
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
      setReminders([]);
      setShares([]);
      setBudgetSummary(null);
      setSelectedTripPlanForm(emptyTripPlanForm);
      setEditingDestinationId(null);
      setEditingActivityId(null);
      setEditingExpenseId(null);
      setEditingChecklistItemId(null);
      setEditingNoteId(null);
      setEditingReminderId(null);
      setDestinationForm(emptyDestinationForm);
      setActivityForm(emptyActivityForm);
      setExpenseForm(emptyExpenseForm);
      setChecklistForm(emptyChecklistForm);
      setNoteForm(emptyNoteForm);
      setReminderForm(emptyReminderForm);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      setFormErrors(emptyFormErrors);
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
        tripReminders,
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
          remindersApi.getReminders(tripPlanId),
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
      setReminders(tripReminders ?? []);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
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
    setSelectedTripPlanId(tripPlanId);
    setMessage("");
    setEditingDestinationId(null);
    setEditingActivityId(null);
    setEditingExpenseId(null);
    setEditingChecklistItemId(null);
    setEditingNoteId(null);
    setEditingReminderId(null);
    setDestinationForm(emptyDestinationForm);
    setActivityForm(emptyActivityForm);
    setExpenseForm(emptyExpenseForm);
    setChecklistForm(emptyChecklistForm);
    setNoteForm(emptyNoteForm);
    setReminderForm(emptyReminderForm);
    setGeneratedShareLink("");
    setVisibleShareQrId(null);
    setActivityViewMode("list");
    setFormErrors(emptyFormErrors);
  };

  const submitTripPlan = async (event) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (stopInvalidSubmit("tripPlan", validateTripPlan(tripPlanForm))) {
      return;
    }

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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
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
      setReminders([]);
      setShares([]);
      setBudgetSummary(null);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      await loadTripPlans();
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
      setError(getRequestErrorMessage(requestError));
    }
  };

  const resetSelectedTripPlanForm = () => {
    setSelectedTripPlanForm(toTripPlanForm(selectedTripPlan));
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

    if (stopInvalidSubmit("destination", validateDestination(destinationForm))) {
      return;
    }

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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editDestination = (destination) => {
    setEditingDestinationId(destination.id);
    clearFormErrors("destination");
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
    clearFormErrors("destination");
  };

  const deleteDestination = async (destinationId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await destinationsApi.remove(selectedTripPlanId, destinationId);
      if (editingDestinationId === destinationId) {
        cancelDestinationEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
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

    if (stopInvalidSubmit("activity", validateActivity(activityForm))) {
      return;
    }

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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editActivity = (activity) => {
    setEditingActivityId(activity.id);
    clearFormErrors("activity");
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
    clearFormErrors("activity");
  };

  const deleteActivity = async (activityId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await activitiesApi.remove(selectedTripPlanId, activityId);
      if (editingActivityId === activityId) {
        cancelActivityEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editExpense = (expense) => {
    setEditingExpenseId(expense.id);
    clearFormErrors("expense");
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
    clearFormErrors("expense");
  };

  const deleteExpense = async (expenseId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await budgetApi.deleteExpense(selectedTripPlanId, expenseId);
      if (editingExpenseId === expenseId) {
        cancelExpenseEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editChecklistItem = (checklistItem) => {
    setEditingChecklistItemId(checklistItem.id);
    clearFormErrors("checklist");
    setChecklistForm({
      title: checklistItem.title ?? "",
    });
  };

  const cancelChecklistEdit = () => {
    setEditingChecklistItemId(null);
    setChecklistForm(emptyChecklistForm);
    clearFormErrors("checklist");
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
      setError(getRequestErrorMessage(requestError));
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
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editNote = (note) => {
    setEditingNoteId(note.id);
    clearFormErrors("note");
    setNoteForm({
      title: note.title ?? "",
      content: note.content ?? "",
    });
  };

  const cancelNoteEdit = () => {
    setEditingNoteId(null);
    setNoteForm(emptyNoteForm);
    clearFormErrors("note");
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
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: reminderForm.title,
        description: reminderForm.description || null,
        reminderAt: toDateTimeLocalValue(reminderForm.reminderAt),
      };

      if (editingReminderId) {
        await remindersApi.updateReminder(selectedTripPlanId, editingReminderId, {
          title: payload.title,
          description: payload.description,
          reminderAt: payload.reminderAt,
          isCompleted: Boolean(reminderForm.isCompleted),
        });
      } else {
        const created = await remindersApi.createReminder(selectedTripPlanId, payload);
        if (reminderForm.isCompleted && created?.id) {
          await remindersApi.updateReminder(selectedTripPlanId, created.id, {
            title: payload.title,
            description: payload.description,
            reminderAt: payload.reminderAt,
            isCompleted: true,
          });
        }
      }

      cancelReminderEdit();
      await loadTripPlanDetails(selectedTripPlanId);
      setMessage("Uspesno sacuvano.");
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
    }
  };

  const editReminder = (reminder) => {
    setEditingReminderId(reminder.id);
    clearFormErrors("reminder");
    setReminderForm({
      title: reminder.title ?? "",
      description: reminder.description ?? "",
      reminderAt: toDateTimeInputValue(reminder.reminderAt),
      isCompleted: Boolean(reminder.isCompleted),
    });
  };

  const cancelReminderEdit = () => {
    setEditingReminderId(null);
    setReminderForm(emptyReminderForm);
    clearFormErrors("reminder");
  };

  const toggleReminder = async (reminder, isCompleted) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await remindersApi.updateReminder(selectedTripPlanId, reminder.id, {
        title: reminder.title,
        description: reminder.description || null,
        reminderAt: reminder.reminderAt,
        isCompleted,
      });

      await loadTripPlanDetails(selectedTripPlanId);
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
      await remindersApi.deleteReminder(selectedTripPlanId, reminderId);
      if (editingReminderId === reminderId) {
        cancelReminderEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
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
      const created = await sharingApi.createShare(selectedTripPlanId, {
        accessLevel: Number(shareAccessLevel),
        expiresAt: shareExpiresAt ? toExpirationDateTime(shareExpiresAt) : null,
      });

      setShareExpiresAt("");
      setGeneratedShareLink(buildSharedTripPlanLink(created.token));
      setVisibleShareQrId(created.id ?? null);
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
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
      await sharingApi.revokeShare(selectedTripPlanId, shareId);
      setGeneratedShareLink("");
      setVisibleShareQrId(null);
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(getRequestErrorMessage(requestError));
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

            <TripPlanForm
              errors={formErrors.tripPlan}
              form={tripPlanForm}
              onChange={updateTripPlanField}
              onSubmit={submitTripPlan}
            />
          </section>

          <TripPlanList
            isLoading={isLoading}
            onDelete={deleteTripPlan}
            onSelect={selectTripPlan}
            selectedTripPlanId={selectedTripPlanId}
            tripPlans={tripPlans}
          />
        </aside>

        <section className="details-column">
          {selectedTripPlan ? (
            <>
              <TripPlanDetails
                budgetSummary={budgetSummary}
                errors={formErrors.selectedTripPlan}
                form={selectedTripPlanForm}
                onChange={updateSelectedTripPlanField}
                onDelete={deleteTripPlan}
                onReset={resetSelectedTripPlanForm}
                onSubmit={updateSelectedTripPlan}
                tripPlan={selectedTripPlan}
              />

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
              />

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
              />

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
            </>
          ) : (
            <EmptyState>Kreiraj novi plan ili otvori detalje postojeceg plana.</EmptyState>
          )}
        </section>
      </section>
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

function toDateTimeLocalValue(dateTimeValue) {
  return dateTimeValue ? `${dateTimeValue}:00` : null;
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

function toDateTimeInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 16);
}

function getRequestErrorMessage(requestError) {
  return requestError?.message || "Doslo je do greske.";
}
