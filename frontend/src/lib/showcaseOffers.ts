/**
 * Oferty do dekoracyjnych okładek (hero, panel logowania).
 * Pobiera te same dane co przeglądaj; brakujące sloty uzupełnia losowymi picsum (bez linku).
 */
import { userBooksApi } from './userBooks'
import { mapUserBookToCard, coverColor, seedFromGuid } from './mappers'

export type MappedOfferCard = ReturnType<typeof mapUserBookToCard>

export interface ShowcaseSlot {
  id: string
  offerId: string | null
  title: string
  author: string
  type: string
  /** Okładka z API — jak w BookCard; brak URL → kafelek cover-fallback, nie picsum. */
  coverUrl: string | null
  /** Losowy obraz tylko gdy brakuje oferty do wypełnienia siatki (isPlaceholder). */
  placeholderImage?: string
  isPlaceholder: boolean
}

export function placeholderSlot(index: number, width = 112, height = 150): ShowcaseSlot {
  const seed = 300 + index
  return {
    id: `placeholder-${seed}`,
    offerId: null,
    title: '',
    author: '',
    type: 'loan',
    coverUrl: null,
    placeholderImage: `https://picsum.photos/${width}/${height}?random=${seed}`,
    isPlaceholder: true,
  }
}

export function cardToShowcaseSlot(book: MappedOfferCard): ShowcaseSlot {
  return {
    id: book.id,
    offerId: book.id,
    title: book.title,
    author: book.author,
    type: book.type,
    coverUrl: book.coverUrl,
    isPlaceholder: false,
  }
}

export function buildShowcaseSlots(
  books: MappedOfferCard[],
  count: number,
  coverSize?: { w: number; h: number },
): ShowcaseSlot[] {
  const w = coverSize?.w ?? 112
  const h = coverSize?.h ?? 150
  const available = books.filter((b) => b.status === 'available')
  const slots = available.slice(0, count).map((b) => cardToShowcaseSlot(b))
  while (slots.length < count) {
    slots.push(placeholderSlot(slots.length, w, h))
  }
  return slots
}

export async function fetchShowcaseSlots(
  count: number,
  coverSize?: { w: number; h: number },
): Promise<ShowcaseSlot[]> {
  try {
    const result = await userBooksApi.getAll({
      page: 1,
      limit: Math.max(count, 12),
      sort: 'newest',
    })
    const mapped = result.items.map(mapUserBookToCard)
    return buildShowcaseSlots(mapped, count, coverSize)
  } catch {
    return buildShowcaseSlots([], count, coverSize)
  }
}

export { coverColor, seedFromGuid }
