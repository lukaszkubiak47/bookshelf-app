/**
 * Endpointy logowania i rejestracji przez client.ts (apiRequest).
 *
 * Kiedy tu zajrzeć: przy unifikacji HTTP — store auth dziś używa lib/api.ts zamiast tego pliku.
 * Po migracji auth store można tu przenieść login/register i usunąć duplikat ApiError.
 *
 * Eksporty: authApi, LoginPayload, RegisterPayload.
 * Backend: POST /api/auth/login, POST /api/auth/register.
 */
import { apiRequest } from './client'
import type { AuthResponse } from './types'

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  email: string
  password: string
  username: string
  firstName: string
  lastName: string
  city: string
}

/** Wywołania REST pod kontroler Auth */
export const authApi = {
  login(payload: LoginPayload) {
    return apiRequest<AuthResponse>('/api/auth/login', { method: 'POST', body: payload })
  },

  register(payload: RegisterPayload) {
    return apiRequest<AuthResponse>('/api/auth/register', { method: 'POST', body: payload })
  },
}
