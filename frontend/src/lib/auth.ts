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

export const authApi = {
  login(payload: LoginPayload) {
    return apiRequest<AuthResponse>('/api/auth/login', { method: 'POST', body: payload })
  },

  register(payload: RegisterPayload) {
    return apiRequest<AuthResponse>('/api/auth/register', { method: 'POST', body: payload })
  },
}
