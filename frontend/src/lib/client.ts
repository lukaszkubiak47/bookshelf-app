/**
 * Główna warstwa HTTP — fetch z JWT, JSON/FormData i obsługą błędów ASP.NET.
 *
 * Kiedy tu zajrzeć: przy każdym nowym endpoincie w lib/*; przy problemach z tokenem lub URL API.
 * Ustaw VITE_API_BASE_URL w .env (pusty = proxy dev na /api).
 *
 * Eksporty: apiRequest, ApiError, getApiBaseUrl, resolveApiUrl, getStoredToken, setStoredToken.
 * Backend: wszystkie ścieżki /api/* wywoływane przez moduły lib/*.
 */
import type { ApiErrorBody } from './types'

export class ApiError extends Error {
  statusCode: number

  constructor(statusCode: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
  }
}

const TOKEN_KEY = 'bookshelf_token'

/** Konfiguracja URL i token JWT w localStorage */
export function getApiBaseUrl(): string {
  const base = import.meta.env.VITE_API_BASE_URL ?? ''
  return base.replace(/\/$/, '')
}

export function resolveApiUrl(path: string | null | undefined): string {
  if (!path) return ''
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  const base = getApiBaseUrl()
  return `${base}${path.startsWith('/') ? path : `/${path}`}`
}

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setStoredToken(token: string | null): void {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
  auth?: boolean
}

/** Główna funkcja HTTP — auth: true dołącza Bearer z localStorage */
export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, auth = false, headers: customHeaders, ...rest } = options

  const headers = new Headers(customHeaders)
  if (body !== undefined && !(body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  if (auth) {
    const token = getStoredToken()
    if (token) headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...rest,
    headers,
    body:
      body === undefined
        ? undefined
        : body instanceof FormData
          ? body
          : JSON.stringify(body),
  })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  let data: unknown = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      throw new ApiError(response.status, 'Nieprawidłowa odpowiedź serwera — sprawdź, czy backend działa')
    }
  }

  if (!response.ok) {
    const err = data as ApiErrorBody | null
    let message = err?.message ?? 'Wystąpił błąd serwera'

    if (!err?.message && data && typeof data === 'object') {
      const problem = data as { title?: string; errors?: Record<string, string[]> }
      if (problem.errors) {
        const first = Object.values(problem.errors).flat()[0]
        if (first) message = first
      } else if (problem.title) {
        message = problem.title
      }
    }

    throw new ApiError(response.status, message)
  }

  return data as T
}
