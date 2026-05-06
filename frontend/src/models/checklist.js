import { booleanValue, normalizeArray, stringValue } from "./modelUtils.js";

export function createChecklistItemModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    title: stringValue(source.title),
    isCompleted: booleanValue(source.isCompleted),
    createdAt: source.createdAt ?? "",
    updatedAt: source.updatedAt ?? null,
  };
}

export function createChecklistItemFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
  };
}

export function createChecklistItemRequestModel(form = {}, tripPlanId = null) {
  form = form ?? {};

  return {
    ...(tripPlanId ? { tripPlanId } : {}),
    title: stringValue(form.title).trim(),
  };
}

export function createChecklistItemUpdateRequestModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title).trim(),
    isCompleted: booleanValue(source.isCompleted),
  };
}

export const normalizeChecklistItems = (items) => normalizeArray(items, createChecklistItemModel);
