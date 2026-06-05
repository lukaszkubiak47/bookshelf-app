/**
 * Composable: sloty okładek książek do dekoracji UI (np. panel logowania).
 *
 * Cel: ładuje N okładek — najpierw z API (dostępne oferty), brakujące uzupełnia losowymi obrazami.
 * Kiedy używać: gdy potrzebujesz listy okładek z loadingiem, niezależnie od strony głównej.
 *
 * Powiązane pliki:
 * - components/AuthLeftPanel.vue — 3 sloty przy logowaniu/rejestracji
 * - lib/showcaseOffers.ts — fetchShowcaseSlots (API + fallback picsum)
 * - composables/useHomeShowcase.js — podobne dane, ale dedykowane stronie głównej
 */
import { ref, onMounted } from 'vue'
import { fetchShowcaseSlots } from '../lib/showcaseOffers'

/**
 * @param {number} count — liczba slotów okładek
 * @param {{ w: number, h: number } | undefined} coverSize — wymiary miniatury
 */
export function useShowcaseOffers(count, coverSize) {
  const slots = ref([])
  const loading = ref(true)

  // --- Pobranie slotów po zamontowaniu komponentu ---

  onMounted(async () => {
    slots.value = await fetchShowcaseSlots(count, coverSize)
    loading.value = false
  })

  return { slots, loading }
}
