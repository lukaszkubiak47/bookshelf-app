<script setup>
  /**
   * Sekcja hero strony głównej — nagłówek, CTA, statystyki i dekoracyjna siatka okładek.
   * Użycie: HomeView.vue — pierwsza sekcja (showcaseSlots z useHomeShowcase).
   * Props: showcaseSlots (tablica slotów okładek z ofertami lub placeholderami).
   * Klik w okładkę z offerId otwiera ofertę w przeglądaniu.
   */
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useStatsStore } from '../stores/stats';
  import { useAuthStore } from '../stores/auth';
  import { usePageAnimations } from '../composables/useAnimations';
  import { openBrowseOffer } from '../lib/browseNavigation';
  import { coverColor } from '../lib/showcaseOffers';

  defineProps({
    showcaseSlots: { type: Array, required: true },
  });

  const router = useRouter();
  const statsStore = useStatsStore();
  const auth = useAuthStore();
  const coverFailed = ref({});
  usePageAnimations({ hero: true, scroll: false, ripple: false });

  onMounted(() => statsStore.fetchStats());

  const markCoverFailed = (id) => {
    coverFailed.value = { ...coverFailed.value, [id]: true };
  };

  const onCoverClick = (slot) => {
    if (slot.offerId) openBrowseOffer(router, slot.offerId);
  };

  const fmt = (n) => (n == null ? '—' : n.toLocaleString('pl-PL'));

  const heroStats = computed(() => [
    { num: fmt(statsStore.stats?.activeOffers), label: 'Aktywnych ofert' },
    { num: fmt(statsStore.stats?.totalUsers), label: 'Czytelników' },
    { num: fmt(statsStore.stats?.totalCities), label: 'Miast w Polsce' },
  ]);
</script>

<template>
  <!-- Hero: lewa kolumna (tekst, CTA, statystyki) + prawa (siatka okładek) -->
  <section class="hero">
    <div class="hero-inner">

      <!-- Lewa kolumna — nagłówek, przyciski, liczniki z API -->
      <div class="hero-left">
        <div class="hero-eyebrow anim">
          <span class="hero-eyebrow-dot"></span>
          Społeczność czytelników
        </div>

        <h1 class="hero-title anim anim-d1">
          Twoje książki<br>znajdą <em>nowy dom</em>
        </h1>

        <p class="hero-subtitle2 anim anim-d2">
          Wymieniaj, wypożyczaj i sprzedawaj książki lokalnie.
          Dołącz do społeczności czytelników.
        </p>

        <div class="hero-ctas anim anim-d3">
          <RouterLink to="/przegladanie" class="btn btn-filled btn-lg">Przeglądaj książki</RouterLink>
          <RouterLink v-if="!auth.isLoggedIn" to="/logowanie?tab=register" class="btn btn-outline btn-lg">Dołącz za darmo</RouterLink>
        </div>

        <div class="hero-stats anim anim-d4">
          <template v-for="(s, i) in heroStats" :key="s.label">
            <div v-if="i > 0" class="stat-divider"></div>
            <div class="hero-stat">
              <span class="hero-stat-num">{{ s.num }}</span>
              <span class="hero-stat-label">{{ s.label }}</span>
            </div>
          </template>
        </div>
      </div>

      <!-- Prawa kolumna — dekoracyjna siatka okładek (klikalne oferty) -->
      <div class="hero-right anim anim-d2">
        <div class="hero-blob"></div>
        <div class="hero-books">
          <div
            v-for="slot in showcaseSlots"
            :key="slot.id"
            :class="['hero-book', { 'hero-book--clickable': slot.offerId }]"
            :role="slot.offerId ? 'button' : undefined"
            :tabindex="slot.offerId ? 0 : undefined"
            @click="onCoverClick(slot)"
            @keydown.enter="onCoverClick(slot)"
          >
            <img
              v-if="slot.isPlaceholder"
              :src="slot.placeholderImage"
              alt=""
            >
            <img
              v-else-if="slot.coverUrl && !coverFailed[slot.id]"
              :src="slot.coverUrl"
              :alt="slot.title"
              @error="markCoverFailed(slot.id)"
            >
            <div
              v-else
              class="hero-book-fallback cover-fallback"
              :style="{ background: coverColor(slot.title) }"
            >
              <span class="cover-fallback-title">{{ slot.title }}</span>
              <span class="cover-fallback-author">{{ slot.author }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>
  </section>
</template>

<style scoped>
  .hero-book--clickable {
    cursor: pointer;
  }

  .hero-book--clickable:focus-visible {
    outline: 2px solid var(--primary);
    outline-offset: 3px;
  }

  .hero-book-fallback.cover-fallback {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 12px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
  }

  .hero-book-fallback .cover-fallback-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 12px;
    line-height: 1.25;
  }

  .hero-book-fallback .cover-fallback-author {
    font-size: 10px;
    opacity: 0.8;
  }
</style>
