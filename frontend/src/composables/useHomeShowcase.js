/**
 * Composable: dane wystawy na stronie głównej (hero + świeże oferty).
 *
 * Cel: jedno pobranie z API zamiast osobnych requestów w sekcjach — spójne dane i mniej obciążenia.
 * Kiedy używać: w HomeView lub jego sekcjach, gdy potrzebujesz heroSlots i recentBooks.
 *
 * Powiązane pliki:
 * - views/HomeView.vue — konsument composable
 * - lib/showcaseOffers.ts — fetchHomeShowcase (logika API)
 * - components/FirstSectionHomeView.vue, SecondSectionHomeView.vue — sekcje korzystające z danych
 */
import { ref, onMounted } from 'vue'
import { fetchHomeShowcase } from '../lib/showcaseOffers'

export function useHomeShowcase() {
  const heroSlots = ref([])
  const recentBooks = ref([])
  const loading = ref(true)

  // --- Pobranie danych po zamontowaniu widoku ---

  onMounted(async () => {
    const data = await fetchHomeShowcase({ w: 112, h: 150 })
    heroSlots.value = data.heroSlots
    recentBooks.value = data.recentBooks
    loading.value = false
  })

  return { heroSlots, recentBooks, loading }
}
