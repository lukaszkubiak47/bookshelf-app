// src/lib/api.ts
// Cienki wrapper na fetch: wspólny prefiks /api (proxowany przez Vite do backendu),
// automatyczny nagłówek Authorization oraz ujednolicona obsługa błędów (BookshelfException -> { statusCode, message }).

const BASE = '/api'
const TOKEN_KEY = 'bookshelf_token'

export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
    this.name = 'ApiError'
  }
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem(TOKEN_KEY)
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> | undefined),
  }
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(`${BASE}${path}`, { ...options, headers })

  if (!res.ok) {
    let message = `Błąd ${res.status}`
    try {
      const body = await res.json()
      if (body?.message) message = body.message
    } catch {
      // odpowiedź bez ciała JSON — zostaje domyślny komunikat
    }
    throw new ApiError(res.status, message)
  }

  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}
