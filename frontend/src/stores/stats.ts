// src/stores/stats.ts
// Statystyki platformy z GET /api/stats (endpoint publiczny). Pobierane raz i cache'owane,
// współdzielone przez ekran auth i stronę główną.
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '../lib/api'

// Odpowiada StatsDto z backendu
export interface Stats {
  totalBooks: number
  totalUsers: number
  totalCities: number
  activeOffers: number
}

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
      stats.value = await apiFetch<Stats>('/stats')
    } catch {
      error.value = true
    } finally {
      loading.value = false
    }
  }

  return { stats, loading, error, fetchStats }
})
