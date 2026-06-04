import type { Router } from 'vue-router'

/** Przejście do przeglądaj z otwartym modalem wybranej oferty (query ?offer=id). */
export function openBrowseOffer(router: Router, offerId: string) {
  router.push({ name: 'browse', query: { offer: offerId } })
}
