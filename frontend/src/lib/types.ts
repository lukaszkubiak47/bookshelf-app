export interface ApiErrorBody {
  statusCode: number
  message: string
}

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
