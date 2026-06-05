/**
 * Profil zalogowanego użytkownika — odczyt, edycja, hasło, avatar.
 *
 * Kiedy tu zajrzeć: PersonalData.vue, zmiana hasła, upload/preset avatara, syncFromProfile w auth store.
 * Wszystkie metody wymagają auth: true (endpoint /me).
 *
 * Eksport: usersApi.
 * Backend: GET/PUT /api/users/me, PUT /me/password, PUT /me/avatar-preset, POST /me/avatar.
 */
import { apiRequest } from './client'
import type { UserProfile } from './types'

/** Wywołania REST pod kontroler Users */
export const usersApi = {
  getMe() {
    return apiRequest<UserProfile>('/api/users/me', { auth: true })
  },

  updateMe(body: {
    username?: string
    firstName?: string
    lastName?: string
    city?: string
  }) {
    return apiRequest<UserProfile>('/api/users/me', { method: 'PUT', auth: true, body })
  },

  changePassword(body: { currentPassword: string; newPassword: string }) {
    return apiRequest<void>('/api/users/me/password', { method: 'PUT', auth: true, body })
  },

  setAvatarPreset(presetId: number) {
    return apiRequest<UserProfile>('/api/users/me/avatar-preset', {
      method: 'PUT',
      auth: true,
      body: { presetId },
    })
  },

  uploadAvatar(file: File) {
    const form = new FormData()
    form.append('file', file)
    return apiRequest<UserProfile>('/api/users/me/avatar', {
      method: 'POST',
      auth: true,
      body: form,
    })
  },
}
