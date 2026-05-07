import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { activitiesApi } from "../api/activitiesApi.js";
import { adminApi } from "../api/adminApi.js";
import { budgetApi } from "../api/budgetApi.js";
import { checklistApi } from "../api/checklistApi.js";
import { destinationsApi } from "../api/destinationsApi.js";
import { notesApi } from "../api/notesApi.js";
import { remindersApi } from "../api/remindersApi.js";
import { sharingApi } from "../api/sharingApi.js";
import { tripPlansApi } from "../api/tripPlansApi.js";
import { normalizeUsers } from "../models/auth.js";
import { createBudgetSummaryModel, normalizeExpenses } from "../models/budget.js";
import { normalizeChecklistItems } from "../models/checklist.js";
import { normalizeNotes } from "../models/notes.js";
import { normalizeReminders } from "../models/reminders.js";
import { createSharedTripPlanModel, normalizeShareTokens } from "../models/sharing.js";
import {
  createTripPlanModel,
  normalizeActivities,
  normalizeAdminTripPlans,
  normalizeDestinations,
  normalizeTripPlans,
} from "../models/tripPlan.js";
import { useAuth } from "./AuthContext.jsx";

const AppContext = createContext(null);

export function AppProvider({ children }) {
  const { token } = useAuth();
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
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const clearSelectedTripPlan = useCallback(() => {
    setSelectedTripPlan(null);
    setDestinations([]);
    setActivities([]);
    setExpenses([]);
    setChecklistItems([]);
    setNotes([]);
    setReminders([]);
    setShares([]);
    setBudgetSummary(null);
  }, []);

  const resetTripState = useCallback(() => {
    setTripPlans([]);
    setSelectedTripPlanId(null);
    clearSelectedTripPlan();
    setError("");
    setMessage("");
    setIsLoading(false);
  }, [clearSelectedTripPlan]);

  const loadTripPlans = useCallback(
    async (preferredTripPlanId = null) => {
      if (!token) {
        resetTripState();
        return [];
      }

      setIsLoading(true);
      setError("");

      try {
        const plans = normalizeTripPlans(await tripPlansApi.getAll());
        setTripPlans(plans);
        setSelectedTripPlanId((currentId) => {
          if (preferredTripPlanId && plans.some((plan) => plan.id === preferredTripPlanId)) {
            return preferredTripPlanId;
          }

          if (currentId && plans.some((plan) => plan.id === currentId)) {
            return currentId;
          }

          return plans[0]?.id ?? null;
        });

        return plans;
      } catch (requestError) {
        setError(getRequestErrorMessage(requestError));
        return [];
      } finally {
        setIsLoading(false);
      }
    },
    [resetTripState, token],
  );

  const loadTripPlanDetails = useCallback(
    async (tripPlanId) => {
      if (!token || !tripPlanId) {
        clearSelectedTripPlan();
        return null;
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
        ] = await Promise.all([
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

        const tripPlanModel = createTripPlanModel(tripPlan);
        setSelectedTripPlan(tripPlanModel);
        setDestinations(normalizeDestinations(tripDestinations));
        setActivities(normalizeActivities(tripActivities));
        setExpenses(normalizeExpenses(tripExpenses));
        setBudgetSummary(tripBudgetSummary ? createBudgetSummaryModel(tripBudgetSummary) : null);
        setChecklistItems(normalizeChecklistItems(tripChecklistItems));
        setNotes(normalizeNotes(tripNotes));
        setReminders(normalizeReminders(tripReminders));
        setShares(normalizeShareTokens(tripShares));

        return tripPlanModel;
      } catch (requestError) {
        setError(getRequestErrorMessage(requestError));
        return null;
      }
    },
    [clearSelectedTripPlan, token],
  );

  useEffect(() => {
    if (token) {
      void loadTripPlans();
    } else {
      resetTripState();
    }
  }, [loadTripPlans, resetTripState, token]);

  useEffect(() => {
    void loadTripPlanDetails(selectedTripPlanId);
  }, [loadTripPlanDetails, selectedTripPlanId]);

  const selectTripPlan = useCallback((tripPlanId) => {
    setSelectedTripPlanId(tripPlanId);
    setMessage("");
  }, []);

  const createTripPlan = useCallback(
    async (request) => {
      const created = await tripPlansApi.create(request);
      await loadTripPlans(created?.id);
      return created;
    },
    [loadTripPlans],
  );

  const updateTripPlan = useCallback(
    async (tripPlanId, request) => {
      await tripPlansApi.update(tripPlanId, request);
      await loadTripPlanDetails(tripPlanId);
      await loadTripPlans(tripPlanId);
    },
    [loadTripPlanDetails, loadTripPlans],
  );

  const deleteTripPlan = useCallback(
    async (tripPlanId) => {
      await tripPlansApi.remove(tripPlanId);
      await loadTripPlans();
    },
    [loadTripPlans],
  );

  const saveDestination = useCallback(
    async (tripPlanId, destinationId, request) => {
      if (destinationId) {
        await destinationsApi.update(tripPlanId, destinationId, request);
      } else {
        await destinationsApi.create(tripPlanId, request);
      }

      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const deleteDestination = useCallback(
    async (tripPlanId, destinationId) => {
      await destinationsApi.remove(tripPlanId, destinationId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const saveActivity = useCallback(
    async (tripPlanId, activityId, request) => {
      if (activityId) {
        await activitiesApi.update(tripPlanId, activityId, request);
      } else {
        await activitiesApi.create(tripPlanId, request);
      }

      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const deleteActivity = useCallback(
    async (tripPlanId, activityId) => {
      await activitiesApi.remove(tripPlanId, activityId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const saveExpense = useCallback(
    async (tripPlanId, expenseId, request) => {
      if (expenseId) {
        await budgetApi.updateExpense(tripPlanId, expenseId, request);
      } else {
        await budgetApi.createExpense(tripPlanId, request);
      }

      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const deleteExpense = useCallback(
    async (tripPlanId, expenseId) => {
      await budgetApi.deleteExpense(tripPlanId, expenseId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const saveChecklistItem = useCallback(
    async (tripPlanId, checklistItemId, request) => {
      if (checklistItemId) {
        await checklistApi.updateChecklistItem(tripPlanId, checklistItemId, request);
      } else {
        await checklistApi.createChecklistItem(tripPlanId, request);
      }

      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const deleteChecklistItem = useCallback(
    async (tripPlanId, checklistItemId) => {
      await checklistApi.deleteChecklistItem(tripPlanId, checklistItemId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const saveNote = useCallback(
    async (tripPlanId, noteId, request) => {
      if (noteId) {
        await notesApi.updateNote(tripPlanId, noteId, request);
      } else {
        await notesApi.createNote(tripPlanId, request);
      }

      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const deleteNote = useCallback(
    async (tripPlanId, noteId) => {
      await notesApi.deleteNote(tripPlanId, noteId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const saveReminder = useCallback(
    async (tripPlanId, reminderId, request) => {
      if (reminderId) {
        await remindersApi.updateReminder(tripPlanId, reminderId, request);
        await loadTripPlanDetails(tripPlanId);
        return null;
      }

      const created = await remindersApi.createReminder(tripPlanId, request);
      await loadTripPlanDetails(tripPlanId);
      return created;
    },
    [loadTripPlanDetails],
  );

  const deleteReminder = useCallback(
    async (tripPlanId, reminderId) => {
      await remindersApi.deleteReminder(tripPlanId, reminderId);
      await loadTripPlanDetails(tripPlanId);
    },
    [loadTripPlanDetails],
  );

  const createShareToken = useCallback(async (tripPlanId, request) => {
    const created = await sharingApi.createShare(tripPlanId, request);
    setShares(normalizeShareTokens(await sharingApi.getShares(tripPlanId)));
    return created;
  }, []);

  const revokeShareToken = useCallback(async (tripPlanId, shareId) => {
    await sharingApi.revokeShare(tripPlanId, shareId);
    setShares(normalizeShareTokens(await sharingApi.getShares(tripPlanId)));
  }, []);

  const adminService = useMemo(
    () => ({
      async getDashboard() {
        const [loadedUsers, loadedTripPlans] = await Promise.all([
          adminApi.getUsers(),
          adminApi.getAllTripPlans(),
        ]);

        return {
          users: normalizeUsers(loadedUsers),
          tripPlans: normalizeAdminTripPlans(loadedTripPlans),
        };
      },

      changeUserRole(userId, role) {
        return adminApi.changeUserRole(userId, role);
      },

      deleteUser(userId) {
        return adminApi.deleteUser(userId);
      },

      deleteTripPlan(tripPlanId) {
        return adminApi.deleteTripPlan(tripPlanId);
      },
    }),
    [],
  );

  const sharedTripPlanService = useMemo(
    () => ({
      async getByToken(shareToken) {
        return createSharedTripPlanModel(await sharingApi.getSharedTripPlan(shareToken));
      },

      updateTripPlan(shareToken, request) {
        return sharingApi.updateSharedTripPlan(shareToken, request);
      },

      saveDestination(shareToken, destinationId, request) {
        return destinationId
          ? sharingApi.updateSharedDestination(shareToken, destinationId, request)
          : sharingApi.createSharedDestination(shareToken, request);
      },

      deleteDestination(shareToken, destinationId) {
        return sharingApi.deleteSharedDestination(shareToken, destinationId);
      },

      saveActivity(shareToken, activityId, request) {
        return activityId
          ? sharingApi.updateSharedActivity(shareToken, activityId, request)
          : sharingApi.createSharedActivity(shareToken, request);
      },

      deleteActivity(shareToken, activityId) {
        return sharingApi.deleteSharedActivity(shareToken, activityId);
      },

      saveExpense(shareToken, expenseId, request) {
        return expenseId
          ? sharingApi.updateSharedExpense(shareToken, expenseId, request)
          : sharingApi.createSharedExpense(shareToken, request);
      },

      deleteExpense(shareToken, expenseId) {
        return sharingApi.deleteSharedExpense(shareToken, expenseId);
      },

      saveChecklistItem(shareToken, checklistItemId, request) {
        return checklistItemId
          ? sharingApi.updateSharedChecklistItem(shareToken, checklistItemId, request)
          : sharingApi.createSharedChecklistItem(shareToken, request);
      },

      deleteChecklistItem(shareToken, checklistItemId) {
        return sharingApi.deleteSharedChecklistItem(shareToken, checklistItemId);
      },

      saveNote(shareToken, noteId, request) {
        return noteId
          ? sharingApi.updateSharedNote(shareToken, noteId, request)
          : sharingApi.createSharedNote(shareToken, request);
      },

      deleteNote(shareToken, noteId) {
        return sharingApi.deleteSharedNote(shareToken, noteId);
      },
    }),
    [],
  );

  const tripPlanReportService = useMemo(
    () => ({
      async getByTripPlanId(tripPlanId) {
        const [
          loadedTripPlan,
          loadedDestinations,
          loadedActivities,
          loadedExpenses,
          loadedBudgetSummary,
          loadedChecklistItems,
          loadedNotes,
          loadedReminders,
        ] = await Promise.all([
          tripPlansApi.getById(tripPlanId),
          destinationsApi.getByTripPlanId(tripPlanId),
          activitiesApi.getByTripPlanId(tripPlanId),
          budgetApi.getExpenses(tripPlanId),
          budgetApi.getBudgetSummary(tripPlanId),
          checklistApi.getChecklistItems(tripPlanId),
          notesApi.getNotes(tripPlanId),
          remindersApi.getReminders(tripPlanId),
        ]);

        return {
          tripPlan: createTripPlanModel(loadedTripPlan),
          destinations: normalizeDestinations(loadedDestinations),
          activities: normalizeActivities(loadedActivities),
          expenses: normalizeExpenses(loadedExpenses),
          budgetSummary: loadedBudgetSummary ? createBudgetSummaryModel(loadedBudgetSummary) : null,
          checklistItems: normalizeChecklistItems(loadedChecklistItems),
          notes: normalizeNotes(loadedNotes),
          reminders: normalizeReminders(loadedReminders),
        };
      },
    }),
    [],
  );

  const value = useMemo(
    () => ({
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
      loadTripPlans,
      loadTripPlanDetails,
      selectTripPlan,
      createTripPlan,
      updateTripPlan,
      deleteTripPlan,
      saveDestination,
      deleteDestination,
      saveActivity,
      deleteActivity,
      saveExpense,
      deleteExpense,
      saveChecklistItem,
      deleteChecklistItem,
      saveNote,
      deleteNote,
      saveReminder,
      deleteReminder,
      createShareToken,
      revokeShareToken,
      adminService,
      sharedTripPlanService,
      tripPlanReportService,
    }),
    [
      adminService,
      activities,
      budgetSummary,
      checklistItems,
      createShareToken,
      createTripPlan,
      deleteActivity,
      deleteChecklistItem,
      deleteDestination,
      deleteExpense,
      deleteNote,
      deleteReminder,
      deleteTripPlan,
      destinations,
      error,
      expenses,
      isLoading,
      loadTripPlanDetails,
      loadTripPlans,
      message,
      notes,
      reminders,
      revokeShareToken,
      saveActivity,
      saveChecklistItem,
      saveDestination,
      saveExpense,
      saveNote,
      saveReminder,
      selectTripPlan,
      selectedTripPlan,
      selectedTripPlanId,
      sharedTripPlanService,
      shares,
      tripPlanReportService,
      tripPlans,
      updateTripPlan,
    ],
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp() {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error("useApp must be used inside AppProvider.");
  }

  return context;
}

function getRequestErrorMessage(requestError) {
  return requestError?.message || "Doslo je do greske.";
}
