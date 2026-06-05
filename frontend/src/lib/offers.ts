/**
 * Zdarzenie przeglądarkowe synchronizujące listę ofert między widokami.
 *
 * Kiedy tu zajrzeć: po akceptacji/zakończeniu transakcji lub usunięciu oferty —
 * wywołaj notifyOffersChanged(), żeby BrowseView odświeżył listę bez przeładowania strony.
 *
 * Eksporty: OFFERS_CHANGED_EVENT, notifyOffersChanged().
 * Backend: brak — komunikacja tylko w obrębie SPA (CustomEvent na window).
 */
export const OFFERS_CHANGED_EVENT = 'bookshelf:offers-changed';

export function notifyOffersChanged(): void {
  window.dispatchEvent(new CustomEvent(OFFERS_CHANGED_EVENT));
}
