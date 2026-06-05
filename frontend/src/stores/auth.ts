/**
 * Pinia store — sesja użytkownika (JWT + dane w localStorage).
 *
 * Kiedy tu zajrzeć: logowanie/rejestracja, guard routera, navbar (isLoggedIn, user),
 * wylogowanie, aktualizacja user po zapisie profilu (syncFromProfile).
 *
 * Eksporty: useAuthStore, typy User, RegisterPayload.
 * Backend: POST /api/auth/login, POST /api/auth/register (przez lib/api.ts → apiFetch).
 */
import { defineStore } from 'pinia'
import { computed } from 'vue'
import { useLocalStorage } from '@vueuse/core'
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

/** Odczyt user z localStorage z obsługą uszkodzonego JSON */
function readStoredUser(raw: string | null): User | null {
  if (!raw) return null
  try {
    return JSON.parse(raw) as User
  } catch {
    return null
  }
}

/** Reaktywny stan sesji — token i user w localStorage (VueUse) */
export const useAuthStore = defineStore('auth', () => {
  const token = useLocalStorage<string | null>(TOKEN_KEY, null)
  const user = useLocalStorage<User | null>(USER_KEY, null, {
    serializer: {
      read: readStoredUser,
      write: (value) => JSON.stringify(value),
    },
  })

  const isLoggedIn = computed(() => !!token.value)

  function setSession(res: AuthResponse) {
    token.value = res.token
    user.value = res.user
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
  }

  /** Po zapisie profilu — aktualizuje user w pamięci i localStorage (np. nick w navbarze). */
  function syncFromProfile(updatedUser: User) {
    user.value = updatedUser
  }

  return { token, user, isLoggedIn, login, register, logout, syncFromProfile }
})
