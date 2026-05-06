export function normalizeArray(items, createModel) {
  return Array.isArray(items) ? items.map((item) => createModel(item)) : [];
}

export function stringValue(value) {
  return value === null || value === undefined ? "" : String(value);
}

export function nullableString(value) {
  const text = stringValue(value).trim();
  return text ? text : null;
}

export function numberValue(value, fallback = 0) {
  const numericValue = Number(value);
  return Number.isFinite(numericValue) ? numericValue : fallback;
}

export function enumNumberValue(value, fallback = 0) {
  const numericValue = Number(value);
  return Number.isInteger(numericValue) ? numericValue : fallback;
}

export function booleanValue(value, fallback = false) {
  if (value === null || value === undefined) {
    return fallback;
  }

  return Boolean(value);
}

export function dateInputValue(value) {
  return value ? String(value).slice(0, 10) : "";
}

export function timeInputValue(value) {
  return value ? String(value).slice(0, 5) : "";
}

export function dateTimeInputValue(value) {
  return value ? String(value).slice(0, 16) : "";
}

export function dateOnlyToDateTime(value) {
  return value ? `${String(value).slice(0, 10)}T00:00:00` : null;
}

export function dateOnlyToEndOfDayDateTime(value) {
  return value ? `${String(value).slice(0, 10)}T23:59:59` : null;
}

export function dateTimeInputToDateTime(value) {
  return value ? `${String(value).slice(0, 16)}:00` : null;
}
