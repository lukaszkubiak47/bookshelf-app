/**
 * Pinia store — statystyki platformy (liczniki książek, użytkowników, miast, ofert).
 *
 * Kiedy tu zajrzeć: strona główna, panel logowania/rejestracji — fetchStats() raz na sesję (cache).
 * activeOffers liczy przez userBooksApi.getAll (ta sama logika co BrowseView), nie surowy /stats.
 *
 * Eksporty: useStatsStore, typ Stats.
 * Backend: GET /api/stats, GET /api/userbooks?page=1&limit=1 (dla activeOffers).
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '../lib/api'
import { userBooksApi } from '../lib/userBooks'

/** Odpowiada StatsDto z backendu (+ activeOffers nadpisywane z userBooks) */
export interface Stats {
  totalBooks: number
  totalUsers: number
  totalCities: number
  activeOffers: number
}

/** Store z cache — fetchStats() no-op gdy dane już załadowane */
export const useStatsStore = defineStore('stats', () => {
  const stats = ref<Stats | null>(null)
  const loading = ref(false)
  const error = ref(false)

  async function fetchStats() {
    // Pobierz tylko raz — kolejne wywołania korzystają z cache.
    if (stats.value || loading.value) return
    loading.value = true
    error.value = false
    try {
      const [statsData, offersResult] = await Promise.all([
        apiFetch<Stats>('/stats'),
        userBooksApi.getAll({ page: 1, limit: 1 }),
      ])
      stats.value = {
        ...statsData,
        // Ta sama logika co „Przeglądaj książki” (WhereBrowsable), nie licznik z /stats.
        activeOffers: offersResult.total,
      }
    } catch {
      error.value = true
    } finally {
      loading.value = false
    }
  }

  return { stats, loading, error, fetchStats }
})
