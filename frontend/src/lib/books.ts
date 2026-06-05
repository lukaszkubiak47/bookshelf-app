/**
 * Katalog książek w bazie — wyszukiwanie, gatunki, tworzenie encji Book.
 *
 * Kiedy tu zajrzeć: formularz dodawania oferty (wyszukaj w Google → wybierz gatunek → zapisz książkę).
 * Wynik search to GoogleBookResult (bez Guid); po POST /api/books dostajesz BookDto z id.
 *
 * Eksport: booksApi (search, getGenres, create).
 * Backend: GET /api/books/search, GET /api/genres, POST /api/books.
 */
import { apiRequest } from './client'
import type { BookDto, Genre, GoogleBookResult } from './types'

/** Wywołania REST pod kontroler Books */
export const booksApi = {
  // Backend: GET /api/books/search?q=... (proxy do Google Books). Zwraca wyniki Google, nie encje z bazy.
  search(query: string) {
    const qs = new URLSearchParams()
    qs.set('q', query)
    return apiRequest<GoogleBookResult[]>(`/api/books/search?${qs}`)
  },

  // Backend: GET /api/genres (publiczny). Słownik gatunków: { id: Guid, name }.
  getGenres() {
    return apiRequest<Genre[]>('/api/genres')
  },

  create(body: {
    title: string
    author: string
    description?: string
    isbn?: string
    genreId: string
    googleBooksId?: string
    coverUrl?: string
  }) {
    return apiRequest<BookDto>('/api/books', { method: 'POST', auth: true, body })
  },
}
