<script setup>
  /**
   * Sekcja „Jak to działa?” — trzy kroki onboardingowe (treść statyczna).
   * Użycie: HomeView.vue — trzecia sekcja na dole strony.
   * Brak propsów i emitów; animacje scroll przez usePageAnimations.
   */
  import { ref } from 'vue';
  import { usePageAnimations } from '../composables/useAnimations';

  const sectionRef = ref(null);
  usePageAnimations({ hero: false, scroll: true, ripple: false, root: sectionRef });
</script>

<template>
  <section ref="sectionRef" class="how-section">
    <div class="section-inner">

      <div class="how-header">
        <h2 class="section-title anim">Jak to działa?</h2>
        <p class="anim anim-d1">Trzy proste kroki dzielą Cię od wymarzonej lektury</p>
      </div>

      <!-- Trzy karty kroków: znajdź → wyślij prośbę → dogadaj szczegóły -->
      <div class="how-cards">

        <div class="how-card anim anim-d1">
          <div class="how-step-num">1</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="7" />
              <line x1="16.5" y1="16.5" x2="22" y2="22" />
            </svg>
          </div>
          <h3>Znajdź książkę</h3>
          <p>Przeglądaj tysiące ofert od czytelników w Twoim mieście. Filtruj po gatunku, autorze i typie oferty.</p>
        </div>

        <div class="how-card anim anim-d2">
          <div class="how-step-num">2</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="17 1 21 5 17 9" />
              <path d="M3 11V9a4 4 0 0 1 4-4h14" />
              <polyline points="7 23 3 19 7 15" />
              <path d="M21 13v2a4 4 0 0 1-4 4H3" />
            </svg>
          </div>
          <h3>Wyślij prośbę</h3>
          <p>Skontaktuj się z właścicielem jednym kliknięciem. Zaproponuj wymianę lub umów się na wypożyczenie.</p>
        </div>

        <div class="how-card anim anim-d3">
          <div class="how-step-num">3</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 11h1a3 3 0 0 1 0 6h-1" />
              <path d="M9 12H7a3 3 0 0 0 0 6h2" />
              <line x1="9" y1="15" x2="15" y2="15" />
              <path d="M7 7h1a2 2 0 0 0 0-4H4v18" />
              <path d="M17 7h-1a2 2 0 0 1 0-4h3v18" />
            </svg>
          </div>
          <h3>Dogadaj szczegóły</h3>
          <p>Ustal szczegóły mailowo. Możecie spotkać się lokalnie lub wybrać wysyłkę – tak, jak Wam wygodnie.</p>
        </div>

      </div>
    </div>
  </section>
</template>
