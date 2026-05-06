import { EXPENSE_CATEGORIES } from "../../models/budget.js";
import { SHARE_ACCESS_LEVELS } from "../../models/sharing.js";

export const ACTIVITY_STATUSES = [
  { value: 0, label: "Planned" },
  { value: 1, label: "Reserved" },
  { value: 2, label: "Completed" },
  { value: 3, label: "Cancelled" },
];

export function compareDates(firstValue, secondValue) {
  const firstTime = firstValue ? new Date(firstValue).getTime() : Number.MAX_SAFE_INTEGER;
  const secondTime = secondValue ? new Date(secondValue).getTime() : Number.MAX_SAFE_INTEGER;

  return firstTime - secondTime;
}

export function formatDate(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleDateString();
}

export function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString();
}

export function formatDateRange(startDate, endDate) {
  const formattedStartDate = formatDate(startDate);
  const formattedEndDate = formatDate(endDate);

  if (formattedStartDate && formattedEndDate) {
    return `${formattedStartDate} - ${formattedEndDate}`;
  }

  return formattedStartDate || formattedEndDate || "Bez datuma";
}

export function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

export function getStatusLabel(value) {
  const numericValue = Number(value);
  return ACTIVITY_STATUSES.find((status) => status.value === numericValue)?.label ?? "Planned";
}

export function getStatusClass(value) {
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

export function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}

export function getShareAccessLevelLabel(value) {
  return Number(value) === SHARE_ACCESS_LEVELS.EDIT ? "EDIT" : "VIEW";
}

export function todayDateInput() {
  return new Date().toISOString().slice(0, 10);
}

export function buildSharedTripPlanLink(token) {
  return `${window.location.origin}/shared/${token}`;
}
