import {
  dateInputValue,
  dateOnlyToDateTime,
  enumNumberValue,
  normalizeArray,
  nullableString,
  numberValue,
  stringValue,
} from "./modelUtils.js";

export const EXPENSE_CATEGORY = {
  TRANSPORT: 0,
  ACCOMMODATION: 1,
  FOOD: 2,
  TICKETS: 3,
  SHOPPING: 4,
  OTHER: 5,
};

export const EXPENSE_CATEGORIES = [
  { value: EXPENSE_CATEGORY.TRANSPORT, label: "Transport" },
  { value: EXPENSE_CATEGORY.ACCOMMODATION, label: "Smestaj" },
  { value: EXPENSE_CATEGORY.FOOD, label: "Hrana" },
  { value: EXPENSE_CATEGORY.TICKETS, label: "Ulaznice" },
  { value: EXPENSE_CATEGORY.SHOPPING, label: "Kupovina" },
  { value: EXPENSE_CATEGORY.OTHER, label: "Ostalo" },
];

export function createExpenseModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    title: stringValue(source.title),
    category: enumNumberValue(source.category, EXPENSE_CATEGORY.OTHER),
    amount: numberValue(source.amount),
    expenseDate: source.expenseDate ?? "",
    description: source.description ?? null,
    createdAt: source.createdAt ?? "",
    updatedAt: source.updatedAt ?? null,
  };
}

export function createExpenseFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
    category: enumNumberValue(source.category, EXPENSE_CATEGORY.TRANSPORT),
    amount: stringValue(source.amount ?? 0),
    expenseDate: dateInputValue(source.expenseDate),
    description: stringValue(source.description),
  };
}

export function createExpenseRequestModel(form = {}, tripPlanId = null) {
  form = form ?? {};

  return {
    ...(tripPlanId ? { tripPlanId } : {}),
    title: stringValue(form.title).trim(),
    category: enumNumberValue(form.category, EXPENSE_CATEGORY.TRANSPORT),
    amount: numberValue(form.amount),
    expenseDate: dateOnlyToDateTime(form.expenseDate),
    description: nullableString(form.description),
  };
}

export function createBudgetSummaryModel(source = {}) {
  source = source ?? {};

  return {
    tripPlanId: source.tripPlanId ?? "",
    plannedBudget: numberValue(source.plannedBudget),
    totalExpenses: numberValue(source.totalExpenses),
    remainingBudget: numberValue(source.remainingBudget),
  };
}

export const normalizeExpenses = (items) => normalizeArray(items, createExpenseModel);
