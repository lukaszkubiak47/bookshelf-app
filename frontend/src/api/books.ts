import { apiRequest } from './client'
import type { BookDto } from './types'

export const booksApi = {
  search(search: string, limit = 10) {
    const qs = new URLSearchParams()
    if (search) qs.set('search', search)
    qs.set('limit', String(limit))
    return apiRequest<BookDto[]>(`/api/books?${qs}`)
  },

  create(body: {
    title: string
    author: string
    description?: string
    isbn?: string
    genreName?: string
  }) {
    return apiRequest<BookDto>('/api/books', { method: 'POST', auth: true, body })
  },
}
