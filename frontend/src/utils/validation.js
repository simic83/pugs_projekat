import { EXPENSE_CATEGORIES } from "../models/budget.js";
import { SHARE_ACCESS_LEVEL_OPTIONS } from "../models/sharing.js";
import { ACTIVITY_STATUSES } from "../models/tripPlan.js";

export const validationRules = {
  tripDates: "end-date-must-not-be-before-start-date",
  budget: "budget-must-not-be-negative",
};

const ACTIVITY_STATUS_LABELS = ACTIVITY_STATUSES.map((status) => status.label);
const EXPENSE_CATEGORY_LABELS = EXPENSE_CATEGORIES.map((category) => category.label);
const SHARE_ACCESS_LEVEL_LABELS = SHARE_ACCESS_LEVEL_OPTIONS.map((accessLevel) => accessLevel.label);
export const MINIMUM_PASSWORD_LENGTH = 7;

export function validateTripPlan(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv je obavezan.";
  }

  if (isBlank(data?.startDate)) {
    errors.startDate = "Datum pocetka je obavezan.";
  }

  if (isBlank(data?.endDate)) {
    errors.endDate = "Datum kraja je obavezan.";
  }

  if (!errors.startDate && !errors.endDate && isAfter(data.startDate, data.endDate)) {
    errors.endDate = "Datum kraja ne sme biti pre datuma pocetka.";
  }

  if (isNegative(data?.plannedBudget)) {
    errors.plannedBudget = "Planirani budzet ne sme biti negativan.";
  }

  return errors;
}

export function validateDestination(data, tripPlan = null) {
  const errors = {};

  if (isBlank(data?.name)) {
    errors.name = "Naziv je obavezan.";
  }

  if (isBlank(data?.arrivalDate)) {
    errors.arrivalDate = "Datum dolaska je obavezan.";
  }

  if (isBlank(data?.departureDate)) {
    errors.departureDate = "Datum odlaska je obavezan.";
  }

  if (!errors.arrivalDate && !errors.departureDate && isAfter(data.arrivalDate, data.departureDate)) {
    errors.departureDate = "Datum odlaska ne sme biti pre datuma dolaska.";
  }

  if (!errors.arrivalDate && !isWithinTripDates(data.arrivalDate, tripPlan)) {
    errors.arrivalDate = createTripDateRangeMessage(tripPlan);
  }

  if (!errors.departureDate && !isWithinTripDates(data.departureDate, tripPlan)) {
    errors.departureDate = createTripDateRangeMessage(tripPlan);
  }

  return errors;
}

export function validateActivity(data, tripPlan = null) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv aktivnosti je obavezan.";
  }

  if (isBlank(data?.activityDate)) {
    errors.activityDate = "Datum aktivnosti je obavezan.";
  } else if (!isWithinTripDates(data.activityDate, tripPlan)) {
    errors.activityDate = createTripDateRangeMessage(tripPlan);
  }

  if (isNegative(data?.estimatedCost)) {
    errors.estimatedCost = "Procenjeni trosak ne sme biti negativan.";
  }

  if (!isAllowedOption(data?.status, ACTIVITY_STATUS_LABELS)) {
    errors.status = "Status mora biti Planned, Reserved, Completed ili Cancelled.";
  }

  return errors;
}

export function validateExpense(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv troska je obavezan.";
  }

  if (isBlank(data?.category)) {
    errors.category = "Kategorija je obavezna.";
  } else if (!isAllowedOption(data.category, EXPENSE_CATEGORY_LABELS)) {
    errors.category = "Kategorija mora biti Transport, Accommodation, Food, Tickets, Shopping ili Other.";
  }

  if (isNegative(data?.amount)) {
    errors.amount = "Iznos ne sme biti negativan.";
  }

  if (isBlank(data?.expenseDate)) {
    errors.expenseDate = "Datum troska je obavezan.";
  }

  return errors;
}

export function validateChecklistItem(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv stavke je obavezan.";
  } else if (String(data.title).trim().length > 150) {
    errors.title = "Naziv stavke moze imati najvise 150 karaktera.";
  }

  return errors;
}

export function validateNote(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naslov je obavezan.";
  } else if (String(data.title).trim().length > 150) {
    errors.title = "Naslov moze imati najvise 150 karaktera.";
  }

  return errors;
}

export function validateReminder(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv podsjetnika je obavezan.";
  } else if (String(data.title).trim().length > 150) {
    errors.title = "Naziv podsjetnika moze imati najvise 150 karaktera.";
  }

  if (isBlank(data?.reminderAt)) {
    errors.reminderAt = "Datum i vreme podsjetnika su obavezni.";
  }

  return errors;
}

export function validateShare(data) {
  const errors = {};

  if (isBlank(data?.accessLevel)) {
    errors.accessLevel = "AccessLevel je obavezan.";
  } else if (!isAllowedOption(data.accessLevel, SHARE_ACCESS_LEVEL_LABELS)) {
    errors.accessLevel = "AccessLevel mora biti VIEW ili EDIT.";
  }

  if (!isBlank(data?.expiresAt) && isBeforeToday(data.expiresAt)) {
    errors.expiresAt = "Datum isteka ne sme biti u proslosti.";
  }

  return errors;
}

export function validateLogin(data) {
  const errors = {};

  if (isBlank(data?.email)) {
    errors.email = "Email je obavezan.";
  }

  if (isBlank(data?.password)) {
    errors.password = "Lozinka je obavezna.";
  }

  return errors;
}

export function validateRegister(data) {
  const errors = {};

  if (isBlank(data?.name)) {
    errors.name = "Ime je obavezno.";
  }

  if (isBlank(data?.email)) {
    errors.email = "Email je obavezan.";
  } else if (!String(data.email).includes("@")) {
    errors.email = "Email mora sadrzati @.";
  }

  if (isBlank(data?.password)) {
    errors.password = "Lozinka je obavezna.";
  } else if (String(data.password).length < MINIMUM_PASSWORD_LENGTH) {
    errors.password = `Lozinka mora imati najmanje ${MINIMUM_PASSWORD_LENGTH} karaktera.`;
  }

  return errors;
}

export function hasValidationErrors(errors) {
  return Object.keys(errors ?? {}).length > 0;
}

function isBlank(value) {
  return value === null || value === undefined || String(value).trim() === "";
}

function isNegative(value) {
  return !isBlank(value) && Number(value) < 0;
}

function isAfter(firstValue, secondValue) {
  return toComparableDate(firstValue) > toComparableDate(secondValue);
}

function isBeforeToday(value) {
  const date = toComparableDate(value);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return date < today.getTime();
}

function isWithinTripDates(value, tripPlan) {
  if (!tripPlan?.startDate || !tripPlan?.endDate || isBlank(value)) {
    return true;
  }

  const date = toComparableDate(value);
  return date >= toComparableDate(tripPlan.startDate) && date <= toComparableDate(tripPlan.endDate);
}

function createTripDateRangeMessage(tripPlan) {
  return `Datum mora biti u periodu plana (${formatDateForMessage(tripPlan.startDate)} - ${formatDateForMessage(tripPlan.endDate)}).`;
}

function formatDateForMessage(value) {
  return String(value).slice(0, 10);
}

function toComparableDate(value) {
  if (isBlank(value)) {
    return Number.NaN;
  }

  const normalized = String(value).includes("T") ? String(value) : `${value}T00:00:00`;
  return new Date(normalized).getTime();
}

function isAllowedOption(value, allowedLabels) {
  if (isBlank(value)) {
    return false;
  }

  const numericValue = Number(value);
  if (Number.isInteger(numericValue) && numericValue >= 0 && numericValue < allowedLabels.length) {
    return true;
  }

  return allowedLabels.some((label) => label.toLowerCase() === String(value).trim().toLowerCase());
}
