import { normalizeArray, nullableString, stringValue } from "./modelUtils.js";

export function createNoteModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    tripPlanId: source.tripPlanId ?? "",
    title: stringValue(source.title),
    content: source.content ?? null,
    createdAt: source.createdAt ?? "",
    updatedAt: source.updatedAt ?? null,
  };
}

export function createNoteFormModel(source = {}) {
  source = source ?? {};

  return {
    title: stringValue(source.title),
    content: stringValue(source.content),
  };
}

export function createNoteRequestModel(form = {}, tripPlanId = null) {
  form = form ?? {};

  return {
    ...(tripPlanId ? { tripPlanId } : {}),
    title: stringValue(form.title).trim(),
    content: nullableString(form.content),
  };
}

export const normalizeNotes = (items) => normalizeArray(items, createNoteModel);
