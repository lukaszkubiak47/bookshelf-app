import type { BookDto, TransactionDto, UserBookDto } from './types'
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

export function mapUserBookToCard(dto: UserBookDto) {
  const imgSeed = seedFromGuid(dto.book.id, 80)
  const avSeed = seedFromGuid(dto.owner.id, 70)

  return {
    id: dto.id,
    bookId: dto.book.id,
    title: dto.book.title,
    author: dto.book.author,
    type: dto.transactionType,
    genre: dto.book.genreName ?? 'Inne',
    city: dto.owner.city,
    owner: dto.owner.username,
    av: avSeed,
    img: imgSeed,
    coverUrl: coverImageUrl(dto.book),
    avatarUrl: avatarImageUrl(dto.owner),
    isbn: dto.book.isbn ?? '',
    desc: dto.book.description ?? '',
    price: dto.price != null ? `${dto.price} zł` : undefined,
    wants: dto.exchangeDetails ?? undefined,
    createdAt: dto.createdAt,
  }
}

export function mapBookSearchResult(dto: BookDto) {
  const imgSeed = seedFromGuid(dto.id, 80)
  return {
    id: dto.id,
    title: dto.title,
    author: dto.author,
    year: '—',
    genre: dto.genreName ?? 'Inne',
    isbn: dto.isbn ?? '—',
    desc: dto.description ?? '',
    img: imgSeed,
    coverUrl: coverImageUrl(dto),
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
    coverUrl: coverImageUrl(dto.userBook.book),
    nick: counterparty.username,
    av: seedFromGuid(counterparty.id, 70),
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
    coverUrl: coverImageUrl(dto.book),
    price: dto.price,
    dateAdded: new Date(dto.createdAt).toLocaleDateString('pl-PL'),
    listStatus: dto.status === 'available' ? 'available' : 'active',
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
