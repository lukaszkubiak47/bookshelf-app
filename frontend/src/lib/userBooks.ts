/**
 * Oferty użytkowników (UserBooks) — przeglądanie, moja półka, CRUD, okładki.
 *
 * Kiedy tu zajrzeć: BrowseView (lista + filtry), ShelfView (moje oferty), dodawanie/edycja oferty.
 * getAll() obsługuje paginację i filtry query; getMy() wymaga auth.
 *
 * Eksporty: userBooksApi, UserBooksQuery.
 * Backend: GET/POST /api/userbooks, GET /my, GET /recent, PUT/DELETE /{id}, POST/DELETE /{id}/cover.
 */
import { apiRequest } from './client'
import type { PagedResult, UserBookDto } from './types'

export interface UserBooksQuery {
  page?: number
  limit?: number
  type?: string
  genre?: string
  city?: string
  search?: string
  sort?: string
}

/** Serializacja filtrów do query string */
function toQuery(params: UserBooksQuery): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params) as [keyof UserBooksQuery, string | number | undefined][]) {
    if (value !== undefined && value !== '') search.set(key, String(value))
  }
  const qs = search.toString()
  return qs ? `?${qs}` : ''
}

/** Wywołania REST pod kontroler UserBooks */
export const userBooksApi = {
  getAll(query: UserBooksQuery = {}) {
    return apiRequest<PagedResult<UserBookDto>>(`/api/userbooks${toQuery(query)}`)
  },

  getRecent(limit = 5) {
    return apiRequest<UserBookDto[]>(`/api/userbooks/recent?limit=${limit}`)
  },

  // Szczegóły jednej oferty (publiczny endpoint).
  getById(id: string) {
    return apiRequest<UserBookDto>(`/api/userbooks/${id}`)
  },

  getMy() {
    return apiRequest<UserBookDto[]>('/api/userbooks/my', { auth: true })
  },

  create(body: {
    bookId: string
    transactionType: string
    price?: number
    exchangeCondition?: string
    exchangeDetails?: string
  }) {
    return apiRequest<UserBookDto>('/api/userbooks', {
      method: 'POST',
      auth: true,
      body,
    })
  },

  update(id: string, body: {
    transactionType?: string
    price?: number
    exchangeCondition?: string
    exchangeDetails?: string
    bookDescription?: string
  }) {
    return apiRequest<UserBookDto>(`/api/userbooks/${id}`, {
      method: 'PUT',
      auth: true,
      body,
    })
  },

  uploadCover(id: string, file: File) {
    const form = new FormData()
    form.append('file', file)
    return apiRequest<UserBookDto>(`/api/userbooks/${id}/cover`, {
      method: 'POST',
      auth: true,
      body: form,
    })
  },

  removeCover(id: string) {
    return apiRequest<void>(`/api/userbooks/${id}/cover`, { method: 'DELETE', auth: true })
  },

  remove(id: string) {
    return apiRequest<void>(`/api/userbooks/${id}`, { method: 'DELETE', auth: true })
  },
}
