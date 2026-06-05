<script setup>
  /** Strona główna: składa sekcje hero, świeże oferty i „Jak to działa”. */
  import FirstSectionHomeView from '../components/FirstSectionHomeView.vue'
  import SecondSectionHomeView from '../components/SecondSectionHomeView.vue'
  import ThirdSectionHomeView from '../components/ThirdSectionHomeView.vue'
  import { useHomeShowcase } from '../composables/useHomeShowcase'

  const { heroSlots, recentBooks, loading } = useHomeShowcase()
</script>

<template>
  <FirstSectionHomeView :showcase-slots="heroSlots" />
  <SecondSectionHomeView :recent-books="recentBooks" :loading-recent="loading" />
  <ThirdSectionHomeView />
</template>
