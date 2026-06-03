<script setup>
  /** Sekcja hero na stronie głównej: statystyki platformy, CTA, animacje wejścia. */
  import { computed, onMounted } from 'vue';
  import { useStatsStore } from '../stores/stats';
  import { useAuthStore } from '../stores/auth';
  import { usePageAnimations } from '../composables/useAnimations';

  const statsStore = useStatsStore();
  const auth = useAuthStore();
  usePageAnimations({ hero: true, scroll: false, ripple: false });

  onMounted(() => statsStore.fetchStats());

  const fmt = (n) => (n == null ? '—' : n.toLocaleString('pl-PL'));

  const heroStats = computed(() => [
    { num: fmt(statsStore.stats?.activeOffers), label: 'Książek w obiegu' },
    { num: fmt(statsStore.stats?.totalUsers), label: 'Czytelników' },
    { num: fmt(statsStore.stats?.totalCities), label: 'Miast w Polsce' },
  ]);
</script>

<template>
  <!-- ════════════════════════ HERO ════════════════════════ -->
  <section class="hero">
    <div class="hero-inner">

      <!-- LEFT -->
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

      <!-- RIGHT — decorative book grid -->
      <div class="hero-right anim anim-d2">
        <div class="hero-blob"></div>
        <div class="hero-books">
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=10" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=11" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=12" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=13" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=14" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=15" alt="Book cover"></div>
        </div>
      </div>

    </div>
  </section>
</template>
