import {
  booleanValue,
  dateTimeInputToDateTime,
  dateTimeInputValue,
  normalizeArray,
  nullableString,
  stringValue,
} from "./modelUtils.js";

export function createReminderModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    title: stringValue(source.title),
    description: source.description ?? null,
    reminderAt: source.reminderAt ?? "",
    isCompleted: booleanValue(source.isCompleted),
    createdAt: source.createdAt ?? "",
    updatedAt: source.updatedAt ?? null,
  };
}

export function createReminderFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
    description: stringValue(source.description),
    reminderAt: dateTimeInputValue(source.reminderAt),
    isCompleted: booleanValue(source.isCompleted),
  };
}

export function createReminderRequestModel(form = {}, tripPlanId = null) {
  form = form ?? {};

  return {
    ...(tripPlanId ? { tripPlanId } : {}),
    title: stringValue(form.title).trim(),
    description: nullableString(form.description),
    reminderAt: dateTimeInputToDateTime(form.reminderAt),
  };
}

export function createReminderUpdateRequestModel(form = {}) {
  form = form ?? {};

  return {
    title: stringValue(form.title).trim(),
    description: nullableString(form.description),
    reminderAt: dateTimeInputToDateTime(form.reminderAt) ?? form.reminderAt,
    isCompleted: booleanValue(form.isCompleted),
  };
}

export const normalizeReminders = (items) => normalizeArray(items, createReminderModel);
