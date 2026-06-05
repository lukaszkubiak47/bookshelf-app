import { ref, onMounted } from 'vue'
import { fetchHomeShowcase } from '../lib/showcaseOffers'

/** Jedno pobranie ofert na stronę główną — hero i „Świeże oferty” bez duplikatów. */
export function useHomeShowcase() {
  const heroSlots = ref([])
  const recentBooks = ref([])
  const loading = ref(true)

  onMounted(async () => {
    const data = await fetchHomeShowcase({ w: 112, h: 150 })
    heroSlots.value = data.heroSlots
    recentBooks.value = data.recentBooks
    loading.value = false
  })

  return { heroSlots, recentBooks, loading }
}
