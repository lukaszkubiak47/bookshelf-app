import { ref, onMounted } from 'vue'
import { fetchShowcaseSlots } from '../lib/showcaseOffers'

/**
 * Ładuje N slotów okładek: najpierw dostępne oferty z API, reszta — losowe picsum.
 * @param {number} count
 * @param {{ w: number, h: number } | undefined} coverSize
 */
export function useShowcaseOffers(count, coverSize) {
  const slots = ref([])
  const loading = ref(true)

  onMounted(async () => {
    slots.value = await fetchShowcaseSlots(count, coverSize)
    loading.value = false
  })

  return { slots, loading }
}
