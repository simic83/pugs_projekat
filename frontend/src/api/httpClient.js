import { tokenStorage } from "../utils/tokenStorage.js";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

export function getApiBaseUrl() {
  return apiBaseUrl;
}

export function createApiUrl(resourcePath) {
  if (!apiBaseUrl) {
    throw new Error("VITE_API_BASE_URL is not configured.");
  }

  const normalizedBaseUrl = apiBaseUrl.replace(/\/$/, "");
  const normalizedPath = resourcePath.replace(/^\//, "");

  return `${normalizedBaseUrl}/${normalizedPath}`;
}

export async function apiRequest(resourcePath, options = {}) {
  const { method = "GET", body, token, skipAuth = false } = options;
  const headers = new Headers();

  if (body !== undefined) {
    headers.set("Content-Type", "application/json");
  }

  const authToken = skipAuth ? null : (token ?? tokenStorage.getToken());
  if (authToken) {
    headers.set("Authorization", `Bearer ${authToken}`);
  }

  const response = await fetch(createApiUrl(resourcePath), {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const data = await readJsonResponse(response);

  if (!response.ok) {
    const error = new Error(data?.title ?? response.statusText);
    error.status = response.status;
    error.details = data;
    throw error;
  }

  return data;
}

async function readJsonResponse(response) {
  const text = await response.text();
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text);
  } catch {
    return { title: text };
  }
}
