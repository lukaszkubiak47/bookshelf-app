/**
 * Stan sesji użytkownika (JWT + dane w localStorage).
 * Używany przez navbar, guard routera, formularze logowania i profil.
 * Logowanie/rejestracja idą przez apiFetch (lib/api.ts) — starsza warstwa HTTP.
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiFetch } from '../lib/api'

export interface User {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  city: string
  avatarUrl?: string
  createdAt: string
}

interface AuthResponse {
  token: string
  user: User
}

export interface RegisterPayload {
  email: string
  password: string
  username: string
  firstName: string
  lastName: string
  city: string
}

const TOKEN_KEY = 'bookshelf_token'
const USER_KEY = 'bookshelf_user'

function readUser(): User | null {
  try {
    return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null')
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user = ref<User | null>(readUser())

  const isLoggedIn = computed(() => !!token.value)

  function setSession(res: AuthResponse) {
    token.value = res.token
    user.value = res.user
    localStorage.setItem(TOKEN_KEY, res.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.user))
  }

  async function login(email: string, password: string) {
    const res = await apiFetch<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    setSession(res)
  }

  async function register(payload: RegisterPayload) {
    const res = await apiFetch<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    setSession(res)
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  /** Po zapisie profilu — aktualizuje user w pamięci i localStorage (np. nick w navbarze). */
  function syncFromProfile(updatedUser: User) {
    user.value = updatedUser
    localStorage.setItem(USER_KEY, JSON.stringify(updatedUser))
  }

  return { token, user, isLoggedIn, login, register, logout, syncFromProfile }
})
