import { createBudgetSummaryModel, normalizeExpenses } from "./budget.js";
import { normalizeChecklistItems } from "./checklist.js";
import { booleanValue, dateOnlyToEndOfDayDateTime, enumNumberValue, normalizeArray, stringValue } from "./modelUtils.js";
import { normalizeNotes } from "./notes.js";
import { normalizeReminders } from "./reminders.js";
import { createTripPlanModel, normalizeActivities, normalizeDestinations } from "./tripPlan.js";

export const SHARE_ACCESS_LEVELS = {
  VIEW: 0,
  EDIT: 1,
};

export const SHARE_ACCESS_LEVEL_OPTIONS = [
  { value: SHARE_ACCESS_LEVELS.VIEW, label: "VIEW" },
  { value: SHARE_ACCESS_LEVELS.EDIT, label: "EDIT" },
];

export function createShareTokenModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    token: stringValue(source.token),
    accessLevel: enumNumberValue(source.accessLevel, SHARE_ACCESS_LEVELS.VIEW),
    createdByUserId: source.createdByUserId ?? "",
    createdAt: source.createdAt ?? "",
    expiresAt: source.expiresAt ?? null,
    isRevoked: booleanValue(source.isRevoked),
  };
}

export function createShareFormModel(source = {}) {
  source = source ?? {};

  return {
    accessLevel: enumNumberValue(source.accessLevel, SHARE_ACCESS_LEVELS.VIEW),
    expiresAt: source.expiresAt ? String(source.expiresAt).slice(0, 10) : "",
  };
}

export function createShareRequestModel(form = {}, tripPlanId = null) {
  form = form ?? {};

  return {
    ...(tripPlanId ? { tripPlanId } : {}),
    accessLevel: enumNumberValue(form.accessLevel, SHARE_ACCESS_LEVELS.VIEW),
    expiresAt: form.expiresAt ? dateOnlyToEndOfDayDateTime(form.expiresAt) : null,
  };
}

export function createSharedTripPlanModel(source = {}) {
  source = source ?? {};

  return {
    share: source.share ? createShareTokenModel(source.share) : null,
    tripPlan: source.tripPlan ? createTripPlanModel(source.tripPlan) : null,
    accessLevel: enumNumberValue(source.accessLevel ?? source.share?.accessLevel, SHARE_ACCESS_LEVELS.VIEW),
    destinations: normalizeDestinations(source.destinations),
    activities: normalizeActivities(source.activities),
    expenses: normalizeExpenses(source.expenses),
    budgetSummary: source.budgetSummary ? createBudgetSummaryModel(source.budgetSummary) : null,
    checklistItems: normalizeChecklistItems(source.checklistItems),
    notes: normalizeNotes(source.notes),
    reminders: normalizeReminders(source.reminders),
  };
}

export const normalizeShareTokens = (items) => normalizeArray(items, createShareTokenModel);
