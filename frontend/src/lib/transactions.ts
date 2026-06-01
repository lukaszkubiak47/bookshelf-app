import { apiRequest } from './client'
import type { TransactionDto } from './types'

export const transactionsApi = {
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
