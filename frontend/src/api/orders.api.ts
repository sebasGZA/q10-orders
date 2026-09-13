import type { CustomFetchOptions } from "../types/custom-fetch-options.interface";

const API_URL = import.meta.env.VITE_API_URL ?? "";

export const ordersApi = async (endpoint: string, options: CustomFetchOptions = {}) => {
  const defaultHeaders = {
    'Content-Type': 'application/json',
  };

  const config = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
  };

  return fetch(`${API_URL}${endpoint}`, config);
};
