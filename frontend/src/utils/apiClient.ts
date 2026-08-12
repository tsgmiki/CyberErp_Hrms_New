const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Custom event for authentication errors
export const AUTH_ERROR_EVENT = "auth-error";

export interface ApiOptions extends RequestInit {
  skipAuthRedirect?: boolean;
}

/**
 * API client wrapper that handles authentication errors (401)
 * and automatically redirects to login when the server session expires
 */
export async function apiClient<T = unknown>(
  endpoint: string,
  options: ApiOptions = {},
): Promise<T> {
  const { skipAuthRedirect = false, ...fetchOptions } = options;

  // "Content-Type: application/json" is NOT a CORS-safelisted header value, and the SPA is a
  // different origin from the API. Sending it on a GET — which has no body to describe — made the
  // browser preflight EVERY read, doubling the round-trips of any screen that loads several lists.
  // Setting it only when there IS a body keeps reads "simple" requests, so they go straight out.
  const headers: HeadersInit = {
    ...(fetchOptions.body != null ? { "Content-Type": "application/json" } : {}),
    ...fetchOptions.headers,
  };

  const response = await fetch(API_BASE_URL + "/" + endpoint, {
    ...fetchOptions,
    credentials: "include",
    headers,
  });

  // Handle 401 Unauthorized - session expired on server
  if (response.status === 401) {
    if (!skipAuthRedirect) {
      // Dispatch custom event for auth error
      window.dispatchEvent(new CustomEvent(AUTH_ERROR_EVENT));

      // Redirect to login
      window.location.href = "/login";

      throw new Error("Session expired. Please login again.");
    } else {
      // For loginStatus check, we got a 401 meaning user is not authenticated
      // Return null to indicate unauthenticated state
      console.log("Received 401 with skipAuthRedirect=true, user is not authenticated");
      return null as unknown as T;
    }
  }

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP error! status: ${response.status}`);
  }

  // Handle empty responses (e.g., 204 No Content)
  const contentType = response.headers.get("content-type");
  if (contentType && contentType.includes("application/json")) {
    return response.json() as Promise<T>;
  }

  return response.text() as unknown as Promise<T>;
}

/**
 * Convenience methods for common HTTP verbs
 */
export const api = {
  get: <T = unknown>(endpoint: string, options?: ApiOptions) =>
    apiClient<T>(endpoint, { ...options, method: "GET" }),

  post: <T = unknown>(endpoint: string, body?: unknown, options?: ApiOptions) =>
    apiClient<T>(endpoint, {
      ...options,
      method: "POST",
      body: body ? JSON.stringify(body) : undefined,
    }),

  put: <T = unknown>(endpoint: string, body?: unknown, options?: ApiOptions) =>
    apiClient<T>(endpoint, {
      ...options,
      method: "PUT",
      body: body ? JSON.stringify(body) : undefined,
    }),

  patch: <T = unknown>(
    endpoint: string,
    body?: unknown,
    options?: ApiOptions,
  ) =>
    apiClient<T>(endpoint, {
      ...options,
      method: "PATCH",
      body: body ? JSON.stringify(body) : undefined,
    }),

  delete: <T = unknown>(endpoint: string, options?: ApiOptions) =>
    apiClient<T>(endpoint, { ...options, method: "DELETE" }),
};

export default apiClient;
