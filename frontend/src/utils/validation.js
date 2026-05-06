export const validationRules = {
  tripDates: "end-date-must-not-be-before-start-date",
  budget: "budget-must-not-be-negative",
};

const ACTIVITY_STATUSES = ["Planned", "Reserved", "Completed", "Cancelled"];
const EXPENSE_CATEGORIES = ["Transport", "Accommodation", "Food", "Tickets", "Shopping", "Other"];
const SHARE_ACCESS_LEVELS = ["VIEW", "EDIT"];

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

export function validateDestination(data) {
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

  return errors;
}

export function validateActivity(data) {
  const errors = {};

  if (isBlank(data?.title)) {
    errors.title = "Naziv aktivnosti je obavezan.";
  }

  if (isBlank(data?.activityDate)) {
    errors.activityDate = "Datum aktivnosti je obavezan.";
  }

  if (isNegative(data?.estimatedCost)) {
    errors.estimatedCost = "Procenjeni trosak ne sme biti negativan.";
  }

  if (!isAllowedOption(data?.status, ACTIVITY_STATUSES)) {
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
  } else if (!isAllowedOption(data.category, EXPENSE_CATEGORIES)) {
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
  } else if (!isAllowedOption(data.accessLevel, SHARE_ACCESS_LEVELS)) {
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
  } else if (String(data.password).length < 6) {
    errors.password = "Lozinka mora imati najmanje 6 karaktera.";
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
