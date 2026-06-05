/**
 * Nawigacja do widoku „Przeglądaj” z preotwartą ofertą (query ?offer=id).
 *
 * Kiedy tu zajrzeć: gdy klik w karcie oferty (strona główna, półka) ma otworzyć modal w BrowseView.
 *
 * Eksport: openBrowseOffer(router, offerId).
 * Backend: brak — tylko routing Vue Router (name: 'browse').
 */
import type { Router } from 'vue-router'

/** Przekierowanie na /browse?offer={id} — BrowseView odczytuje query i otwiera OfferModal. */
export function openBrowseOffer(router: Router, offerId: string) {
  router.push({ name: 'browse', query: { offer: offerId } })
}
