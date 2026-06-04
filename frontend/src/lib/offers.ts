/**
 * Zdarzenie między widokami: po zmianie statusu oferty na półce
 * BrowseView ponownie pobiera listę (ukrywa zajęte/zakończone oferty).
 */
export const OFFERS_CHANGED_EVENT = 'bookshelf:offers-changed';

export function notifyOffersChanged(): void {
  window.dispatchEvent(new CustomEvent(OFFERS_CHANGED_EVENT));
}
