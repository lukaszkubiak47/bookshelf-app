/**
 * Etykiety i klasy CSS dla typów ofert (loan / swap / sale).
 *
 * Kiedy tu zajrzeć: badge na kartach, etykiety w modalu oferty, typ transakcji na półce —
 * jedno źródło prawdy zamiast rozproszonych stringów w komponentach.
 *
 * Eksporty: OfferType, isOfferType, stałe OFFER_*_CLASS/LABEL, funkcje offer*().
 * Backend: wartości zgodne z transactionType w UserBookDto (loan | swap | sale).
 */
/** Typ transakcji / oferty w UI (zgodny z backendem: loan | swap | sale). */
export type OfferType = 'loan' | 'swap' | 'sale'

const OFFER_TYPES: OfferType[] = ['loan', 'swap', 'sale']

/** Guard i helpery zwracające etykietę/klasę dla danego typu */
export function isOfferType(value: string): value is OfferType {
  return OFFER_TYPES.includes(value as OfferType)
}

/** Klasy badge na kartach przeglądaj / strona główna (.badge-loan itd.). */
export const OFFER_BADGE_CLASS: Record<OfferType, string> = {
  loan: 'badge-loan',
  swap: 'badge-swap',
  sale: 'badge-sale',
}

/** Etykiety na listach i kartach ofert. */
export const OFFER_LABEL_BROWSE: Record<OfferType, string> = {
  loan: 'Do wypożyczenia',
  swap: 'Do wymiany',
  sale: 'Do sprzedaży',
}

/** Etykiety na karcie transakcji (Moja półka). */
export const OFFER_LABEL_TX: Record<OfferType, string> = {
  loan: 'Wypożyczenie',
  swap: 'Wymiana',
  sale: 'Sprzedaż',
}

/** Badge na wystawionych ofertach (.lcb-loan itd.). */
export const OFFER_LISTED_BADGE_CLASS: Record<OfferType, string> = {
  loan: 'lcb-loan',
  swap: 'lcb-swap',
  sale: 'lcb-sale',
}

/** Badge typu na karcie transakcji (.type-loan itd.). */
export const OFFER_TX_TYPE_CLASS: Record<OfferType, string> = {
  loan: 'type-loan',
  swap: 'type-swap',
  sale: 'type-sale',
}

/** Klasa modyfikatora karty transakcji (.t-loan — animacja pulse). */
export function offerTxCardClass(type: string): string {
  return isOfferType(type) ? `t-${type}` : ''
}

export function offerBadgeClass(type: string): string {
  return isOfferType(type) ? OFFER_BADGE_CLASS[type] : ''
}

export function offerBrowseLabel(type: string): string {
  return isOfferType(type) ? OFFER_LABEL_BROWSE[type] : ''
}

export function offerTxLabel(type: string): string {
  return isOfferType(type) ? OFFER_LABEL_TX[type] : ''
}

export function offerListedBadgeClass(type: string): string {
  return isOfferType(type) ? OFFER_LISTED_BADGE_CLASS[type] : ''
}

export function offerTxTypeClass(type: string): string {
  return isOfferType(type) ? OFFER_TX_TYPE_CLASS[type] : ''
}

export function offerBadgeMeta(type: string): { cls: string; label: string } {
  return {
    cls: offerBadgeClass(type),
    label: offerBrowseLabel(type),
  }
}
