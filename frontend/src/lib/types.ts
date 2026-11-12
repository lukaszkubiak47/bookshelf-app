/**
 * Typy odpowiedzi API (DTO) — odzwierciedlają kontrakty backendu ASP.NET.
 *
 * Kiedy tu zajrzeć: przy dodawaniu endpointu, debugowaniu kształtu JSON lub TypeScript errors w lib/*.
 * Komponenty UI zwykle dostają dane po mapowaniu w lib/mappers.ts, nie bezpośrednio te interfejsy.
 *
 * Kluczowe typy: AuthResponse, UserBookDto, TransactionDto, BookDto, GoogleBookResult, PagedResult.
 * Backend: wszystkie kontrolery Bookshelf.API — nazwy pól muszą być zgodne z serializacją C#.
 */
/** Ciało błędu z apiRequest (lib/client.ts) */
export interface ApiErrorBody {
  statusCode: number
  message: string
}

/** Auth i profil użytkownika */
export interface AuthUser {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  city: string
  avatarUrl: string | null
  createdAt: string
}

export interface AuthResponse {
  token: string
  user: AuthUser
}

export interface UserProfile extends AuthUser {
  booksCount: number
  transactionsCount: number
}

/** Książki i gatunki */
export interface BookDto {
  id: string
  title: string
  author: string
  description?: string | null
  coverUrl?: string | null
  isbn?: string | null
  genreName?: string | null
}

// Słownikowy gatunek z GET /api/genres (id = Guid jako string, używany jako CreateBookDto.GenreId).
export interface Genre {
  id: string
  name: string
}

// Wynik wyszukiwania w Google Books — to NIE jest książka z naszej bazy,
// więc ma googleBooksId, a nie id (Guid) — dopóki nie zostanie zapisana przez POST /api/books.
export interface GoogleBookResult {
  googleBooksId: string
  title: string
  author: string
  description?: string | null
  coverUrl?: string | null
  isbn?: string | null
}

/** Oferty użytkowników (UserBooks) */
export interface UserBookDto {
  id: string
  status: string
  transactionType: string
  price?: number | null
  exchangeCondition?: string | null
  exchangeDetails?: string | null
  hasCustomCover: boolean
  createdAt: string
  book: BookDto
  owner: {
    id: string
    username: string
    city: string
    avatarUrl?: string | null
  }
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  limit: number
  totalPages: number
}

/** Transakcje (prośby o wypożyczenie/wymianę/sprzedaż) */
export interface TransactionContact {
  username: string
  email: string
}

/** Wpis osi czasu transakcji (GET /api/transactions/{id}/history) */
export interface TransactionHistoryEntry {
  type: string
  occurredAt: string
  actorUserId: string
}

export interface NotificationItem {
  id: string
  type: string
  transactionId: string
  bookTitle?: string | null
  createdAt: string
  isRead: boolean
}

export interface NotificationList {
  items: NotificationItem[]
  unreadCount: number
}

export interface TransactionDto {
  id: string
  type: string
  status: string
  message?: string | null
  dueDate?: string | null
  createdAt: string
  updatedAt: string
  userBook: {
    id: string
    status: string
    transactionType: string
    price?: number | null
    book: {
      id: string
      title: string
      author: string
      coverUrl?: string | null
    }
    owner: {
      id: string
      username: string
      city: string
      avatarUrl?: string | null
    }
  }
  requester: {
    id: string
    username: string
    city: string
    avatarUrl?: string | null
  }
}
