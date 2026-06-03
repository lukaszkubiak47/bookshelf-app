/** Sygnał do odświeżenia listy ofert w przeglądaniu (np. po akceptacji/zakończeniu na półce). */
export const OFFERS_CHANGED_EVENT = 'bookshelf:offers-changed';

export function notifyOffersChanged(): void {
  window.dispatchEvent(new CustomEvent(OFFERS_CHANGED_EVENT));
}
