import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import { setStoredToken, getStoredToken } from '@/api/client'
import type { AuthUser, UserProfile } from '@/api/types'

const USER_KEY = 'bookshelf_user'

function loadUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    return null
  }
}

function saveUser(user: AuthUser | null) {
  if (user) localStorage.setItem(USER_KEY, JSON.stringify(user))
  else localStorage.removeItem(USER_KEY)
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(loadUser())
  const token = ref<string | null>(getStoredToken())

  const isLoggedIn = computed(() => !!token.value && !!user.value)

  function setSession(newToken: string, newUser: AuthUser) {
    token.value = newToken
    user.value = newUser
    setStoredToken(newToken)
    saveUser(newUser)
  }

  function logout() {
    token.value = null
    user.value = null
    setStoredToken(null)
    saveUser(null)
  }

  function init() {
    token.value = getStoredToken()
    user.value = loadUser()
  }

  function syncFromProfile(profile: UserProfile | AuthUser) {
    if (!user.value) return

    user.value = {
      id: profile.id,
      email: profile.email,
      username: profile.username,
      firstName: profile.firstName,
      lastName: profile.lastName,
      city: profile.city,
      avatarUrl: profile.avatarUrl,
      createdAt: profile.createdAt,
    }
    saveUser(user.value)
  }

  async function login(email: string, password: string) {
    const res = await authApi.login({ email, password })
    setSession(res.token, res.user)
  }

  async function register(payload: {
    email: string
    password: string
    username: string
    firstName: string
    lastName: string
    city: string
  }) {
    const res = await authApi.register(payload)
    setSession(res.token, res.user)
  }

  return { user, token, isLoggedIn, init, login, register, logout, setSession, syncFromProfile }
})
