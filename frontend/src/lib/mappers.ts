/**
 * Mapowanie DTO z API → kształt wygodny dla Vue (karty, półka, profil).
 * Tu też: URL okładek/avatarów, kolory zastępcze okładek, format dat.
 */
import type { TransactionDto, UserBookDto } from './types'
import { resolveApiUrl } from './client'

export function seedFromGuid(id: string, max = 70): number {
  let hash = 0
  for (let i = 0; i < id.length; i++) {
    hash = (hash << 5) - hash + id.charCodeAt(i)
    hash |= 0
  }
  return (Math.abs(hash) % max) + 1
}

export function coverImageUrl(book: { id: string; coverUrl?: string | null }): string {
  if (book.coverUrl) return resolveApiUrl(book.coverUrl)
  return `https://picsum.photos/180/240?random=${seedFromGuid(book.id, 80)}`
}

export function avatarImageUrl(user: { id: string; avatarUrl?: string | null }): string {
  if (user.avatarUrl?.includes('/avatars/preset-')) {
    const match = user.avatarUrl.match(/preset-(\d+)/)
    const preset = match ? Number(match[1]) : seedFromGuid(user.id, 10)
    return `https://i.pravatar.cc/40?img=${preset}`
  }
  if (user.avatarUrl) return resolveApiUrl(user.avatarUrl)
  return `https://i.pravatar.cc/40?img=${seedFromGuid(user.id, 70)}`
}

// Realny URL okładki albo null (bez fallbacku na losowy obrazek) — kafelek-zastępczy renderuje komponent.
export function realCoverUrl(coverUrl?: string | null): string | null {
  return coverUrl ? resolveApiUrl(coverUrl) : null
}

// Realny URL avatara (preset -> pravatar, własny -> resolve) albo null, gdy użytkownik nie ma avatara.
export function realAvatarUrl(user: { id: string; avatarUrl?: string | null }): string | null {
  return user.avatarUrl ? avatarImageUrl(user) : null
}

// Deterministyczny, stonowany kolor tła okładki na podstawie tytułu (ten sam tytuł => ten sam kolor).
const COVER_COLORS = [
  '#6B4F3A', '#7A5C43', '#5A4636', '#6A5240', // brązy
  '#3E5641', '#445C4A', '#4E6151', '#3A5246', // zielenie
  '#2F3E57', '#384E6A', '#3B4A63', '#33425C', // granaty
]

export function coverColor(title: string): string {
  let hash = 0
  for (let i = 0; i < title.length; i++) {
    hash = (hash << 5) - hash + title.charCodeAt(i)
    hash |= 0
  }
  return COVER_COLORS[Math.abs(hash) % COVER_COLORS.length] ?? '#445C4A'
}

export type BookCardModel = ReturnType<typeof mapUserBookToCard>

export function mapUserBookToCard(dto: UserBookDto) {
  const imgSeed = seedFromGuid(dto.book.id, 80)
  const avSeed = seedFromGuid(dto.owner.id, 70)

  return {
    id: dto.id,
    status: dto.status,
    bookId: dto.book.id,
    title: dto.book.title,
    author: dto.book.author,
    type: dto.transactionType,
    genre: dto.book.genreName ?? 'Inne',
    city: dto.owner.city,
    ownerId: dto.owner.id,
    owner: dto.owner.username,
    av: avSeed,
    img: imgSeed,
    coverUrl: realCoverUrl(dto.book.coverUrl),
    avatarUrl: realAvatarUrl(dto.owner),
    isbn: dto.book.isbn ?? '',
    desc: dto.book.description ?? '',
    price: dto.price != null ? `${dto.price} zł` : undefined,
    wants: dto.exchangeDetails ?? undefined,
    createdAt: dto.createdAt,
  }
}

// Obsługuje zarówno wynik z Google (googleBooksId, bez id), jak i zapisaną książkę z bazy (id).
export function mapBookSearchResult(dto: {
  id?: string | null
  googleBooksId?: string | null
  title: string
  author: string
  description?: string | null
  coverUrl?: string | null
  isbn?: string | null
  genreName?: string | null
}) {
  const identity = dto.googleBooksId ?? dto.id ?? dto.title
  return {
    id: dto.id ?? null,                    // Guid z bazy (po zapisie) albo null dla surowego wyniku Google
    googleBooksId: dto.googleBooksId ?? null,
    title: dto.title,
    author: dto.author,
    year: '—',
    genre: dto.genreName ?? 'Inne',
    isbn: dto.isbn ?? '—',
    desc: dto.description ?? '',
    img: seedFromGuid(identity, 80),
    coverUrl: dto.coverUrl ? resolveApiUrl(dto.coverUrl) : null,
  }
}

function formatRelativeDate(iso: string): string {
  const date = new Date(iso)
  const diffMs = Date.now() - date.getTime()
  const days = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  if (days <= 0) return 'dziś'
  if (days === 1) return '1 dzień temu'
  if (days < 7) return `${days} dni temu`
  if (days < 30) return `${Math.floor(days / 7)} tyg. temu`
  return date.toLocaleDateString('pl-PL')
}

function formatDueDate(iso: string | null | undefined): string | undefined {
  if (!iso) return undefined
  return new Date(iso).toLocaleDateString('pl-PL')
}

export function mapTransactionToShelfItem(dto: TransactionDto, direction: 'received' | 'sent') {
  const counterparty = direction === 'received' ? dto.requester : dto.userBook.owner
  const imgSeed = seedFromGuid(dto.userBook.book.id, 80)

  return {
    id: dto.id,
    status: dto.status,
    type: dto.type,
    bookTitle: dto.userBook.book.title,
    bookAuthor: dto.userBook.book.author,
    img: imgSeed,
    coverUrl: realCoverUrl(dto.userBook.book.coverUrl),
    nick: counterparty.username,
    av: seedFromGuid(counterparty.id, 70),
    avatarUrl: realAvatarUrl(counterparty),
    date: formatRelativeDate(dto.createdAt),
    msg: dto.message ?? '',
    returnDate: formatDueDate(dto.dueDate),
    wants: dto.userBook.transactionType === 'swap' ? dto.message ?? '' : undefined,
    price: dto.userBook.price ?? undefined,
    contact: null as { email: string; nick: string } | null,
  }
}

export function mapUserBookToListed(dto: UserBookDto) {
  return {
    id: dto.id,
    type: dto.transactionType,
    bookTitle: dto.book.title,
    bookAuthor: dto.book.author,
    img: seedFromGuid(dto.book.id, 80),
    coverUrl: realCoverUrl(dto.book.coverUrl),
    price: dto.price,
    dateAdded: new Date(dto.createdAt).toLocaleDateString('pl-PL'),
    listStatus:
      dto.status === 'available'
        ? 'available'
        : dto.status === 'completed' || dto.status === 'sold'
          ? 'completed'
          : 'active',
  }
}

export function mapUserToProfile(dto: {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  city: string
  avatarUrl: string | null
  createdAt: string
  booksCount: number
  transactionsCount: number
}) {
  return {
    name: dto.firstName,
    surname: dto.lastName,
    nick: dto.username,
    city: dto.city,
    email: dto.email,
    bio: '',
    avatar: avatarImageUrl({ id: dto.id, avatarUrl: dto.avatarUrl }),
    createdAt: dto.createdAt,
    booksCount: dto.booksCount,
    transactionsCount: dto.transactionsCount,
  }
}
