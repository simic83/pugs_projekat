import {
  dateInputValue,
  dateOnlyToDateTime,
  enumNumberValue,
  normalizeArray,
  nullableString,
  numberValue,
  stringValue,
  timeInputValue,
} from "./modelUtils.js";

export const ACTIVITY_STATUS = {
  PLANNED: 0,
  RESERVED: 1,
  COMPLETED: 2,
  CANCELLED: 3,
};

export const ACTIVITY_STATUSES = [
  { value: ACTIVITY_STATUS.PLANNED, label: "Planirano" },
  { value: ACTIVITY_STATUS.RESERVED, label: "Rezervisano" },
  { value: ACTIVITY_STATUS.COMPLETED, label: "Zavrseno" },
  { value: ACTIVITY_STATUS.CANCELLED, label: "Otkazano" },
];

export const ACTIVITY_STATUS_OPTIONS = ACTIVITY_STATUSES;

export function createTripPlanModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    ownerUserId: source.ownerUserId ?? "",
    title: stringValue(source.title),
    description: source.description ?? null,
    startDate: source.startDate ?? "",
    endDate: source.endDate ?? "",
    plannedBudget: numberValue(source.plannedBudget),
    notes: source.notes ?? null,
    createdAtUtc: source.createdAtUtc ?? "",
    updatedAtUtc: source.updatedAtUtc ?? null,
  };
}

export function createAdminTripPlanModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    ownerUserId: source.ownerUserId ?? "",
    ownerName: source.ownerName ?? null,
    ownerEmail: source.ownerEmail ?? null,
    title: stringValue(source.title),
    description: source.description ?? null,
    startDate: source.startDate ?? "",
    endDate: source.endDate ?? "",
    plannedBudget: numberValue(source.plannedBudget),
    createdAtUtc: source.createdAtUtc ?? "",
  };
}

export function createTripPlanFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
    description: stringValue(source.description),
    startDate: dateInputValue(source.startDate),
    endDate: dateInputValue(source.endDate),
    plannedBudget: stringValue(source.plannedBudget ?? 0),
    notes: stringValue(source.notes),
  };
}

export function createTripPlanRequestModel(form = {}) {
  form = form ?? {};

  return {
    title: stringValue(form.title).trim(),
    description: nullableString(form.description),
    startDate: dateOnlyToDateTime(form.startDate),
    endDate: dateOnlyToDateTime(form.endDate),
    plannedBudget: numberValue(form.plannedBudget),
    notes: nullableString(form.notes),
  };
}

export function createDestinationModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    name: stringValue(source.name),
    location: source.location ?? null,
    arrivalDate: source.arrivalDate ?? "",
    departureDate: source.departureDate ?? "",
    description: source.description ?? null,
    createdAtUtc: source.createdAtUtc ?? "",
    updatedAtUtc: source.updatedAtUtc ?? null,
  };
}

export function createDestinationFormModel(source = {}) {
  source = source ?? {};

  return {
    name: stringValue(source.name),
    location: stringValue(source.location),
    arrivalDate: dateInputValue(source.arrivalDate),
    departureDate: dateInputValue(source.departureDate),
    description: stringValue(source.description),
  };
}

export function createDestinationRequestModel(form = {}) {
  form = form ?? {};

  return {
    name: stringValue(form.name).trim(),
    location: nullableString(form.location),
    arrivalDate: dateOnlyToDateTime(form.arrivalDate),
    departureDate: dateOnlyToDateTime(form.departureDate),
    description: nullableString(form.description),
  };
}

export function createActivityModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    title: stringValue(source.title),
    activityDate: source.activityDate ?? "",
    activityTime: source.activityTime ?? null,
    location: source.location ?? null,
    description: source.description ?? null,
    estimatedCost: numberValue(source.estimatedCost),
    status: enumNumberValue(source.status, ACTIVITY_STATUS.PLANNED),
    createdAtUtc: source.createdAtUtc ?? "",
    updatedAtUtc: source.updatedAtUtc ?? null,
  };
}

export function createActivityFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
    activityDate: dateInputValue(source.activityDate),
    activityTime: timeInputValue(source.activityTime),
    location: stringValue(source.location),
    description: stringValue(source.description),
    estimatedCost: stringValue(source.estimatedCost ?? 0),
    status: enumNumberValue(source.status, ACTIVITY_STATUS.PLANNED),
  };
}

export function createActivityRequestModel(form = {}) {
  form = form ?? {};

  return {
    title: stringValue(form.title).trim(),
    activityDate: dateOnlyToDateTime(form.activityDate),
    activityTime: form.activityTime ? `${timeInputValue(form.activityTime)}:00` : null,
    location: nullableString(form.location),
    description: nullableString(form.description),
    estimatedCost: numberValue(form.estimatedCost),
    status: enumNumberValue(form.status, ACTIVITY_STATUS.PLANNED),
  };
}

export const normalizeTripPlans = (items) => normalizeArray(items, createTripPlanModel);
export const normalizeAdminTripPlans = (items) => normalizeArray(items, createAdminTripPlanModel);
export const normalizeDestinations = (items) => normalizeArray(items, createDestinationModel);
export const normalizeActivities = (items) => normalizeArray(items, createActivityModel);
