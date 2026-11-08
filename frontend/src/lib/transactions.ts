/**
 * Transakcje — prośby o wypożyczenie, wymianę lub sprzedaż między użytkownikami.
 *
 * Kiedy tu zajrzeć: Moja półka (zakładki wysłane/otrzymane), OfferModal (wyślij prośbę),
 * akceptacja/odrzucenie/zakończenie, pobranie danych kontaktowych po akceptacji.
 *
 * Eksport: transactionsApi.
 * Backend: /api/transactions, /received, /sent, /{id}/accept|reject|finish|contact.
 */
import { apiRequest } from './client'
import type { TransactionDto, TransactionContact, TransactionHistoryEntry } from './types'

/** Wywołania REST pod kontroler Transactions */
export const transactionsApi = {
  // Dane kontaktowe drugiej strony — działa tylko dla accepted/done i tylko dla strony transakcji.
  getContact(id: string) {
    return apiRequest<TransactionContact>(`/api/transactions/${id}/contact`, { auth: true })
  },

  // Oś czasu transakcji (zapisywana przez funkcję z kolejki) — tylko dla strony transakcji.
  getHistory(id: string) {
    return apiRequest<TransactionHistoryEntry[]>(`/api/transactions/${id}/history`, { auth: true })
  },

  // Wyślij prośbę o ofertę. Typ transakcji backend dziedziczy z oferty — wystarczy userBookId.
  create(body: { userBookId: string; message?: string; dueDate?: string }) {
    return apiRequest<TransactionDto>('/api/transactions', { method: 'POST', auth: true, body })
  },

  getReceived() {
    return apiRequest<TransactionDto[]>('/api/transactions/received', { auth: true })
  },

  getSent() {
    return apiRequest<TransactionDto[]>('/api/transactions/sent', { auth: true })
  },

  accept(id: string, body: { dueDate?: string; message?: string }) {
    return apiRequest<TransactionDto>(`/api/transactions/${id}/accept`, {
      method: 'PUT',
      auth: true,
      body,
    })
  },

  reject(id: string) {
    return apiRequest<TransactionDto>(`/api/transactions/${id}/reject`, {
      method: 'PUT',
      auth: true,
    })
  },

  finish(id: string) {
    return apiRequest<TransactionDto>(`/api/transactions/${id}/finish`, {
      method: 'PUT',
      auth: true,
    })
  },
}
