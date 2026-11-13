/**
 * Powiadomienia o akcjach na transakcjach (Cosmos DB, zapisuje je funkcja ProcessTransactionEvent).
 *
 * Kiedy tu zajrzeć: dzwonek z licznikiem w AppNavbar.vue.
 *
 * Eksport: notificationsApi.
 * Backend: GET /api/notifications, PUT /api/notifications/{id}/read.
 */
import { apiRequest } from './client'
import type { NotificationList } from './types'

/** Wywołania REST pod kontroler Notifications */
export const notificationsApi = {
  // Najnowsze powiadomienia zalogowanego użytkownika + liczba nieprzeczytanych.
  getAll() {
    return apiRequest<NotificationList>('/api/notifications', { auth: true })
  },

  markRead(id: string) {
    return apiRequest<void>(`/api/notifications/${encodeURIComponent(id)}/read`, {
      method: 'PUT',
      auth: true,
    })
  },
}
