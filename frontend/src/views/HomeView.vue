<script setup>
  /**
   * Trasa: / (name: home) — strona startowa, dostępna bez logowania.
   * Cel: landing page — hero z ofertami, siatka świeżych książek i sekcja „Jak to działa”.
   * API: fetchHomeShowcase() przez composable useHomeShowcase (heroSlots + recentBooks).
   * Komponenty: FirstSectionHomeView, SecondSectionHomeView, ThirdSectionHomeView.
   */
  import FirstSectionHomeView from '../components/FirstSectionHomeView.vue'
  import SecondSectionHomeView from '../components/SecondSectionHomeView.vue'
  import ThirdSectionHomeView from '../components/ThirdSectionHomeView.vue'
  import { useHomeShowcase } from '../composables/useHomeShowcase'

  const { heroSlots, recentBooks, loading } = useHomeShowcase()
</script>

<template>
  <!-- SEKCJA: Hero — rotujące okładki wyróżnionych ofert -->
  <FirstSectionHomeView :showcase-slots="heroSlots" />
  <!-- SEKCJA: Świeże oferty — ostatnio dodane książki z API -->
  <SecondSectionHomeView :recent-books="recentBooks" :loading-recent="loading" />
  <!-- SEKCJA: Jak to działa — kroki dla nowych użytkowników -->
  <ThirdSectionHomeView />
</template>
