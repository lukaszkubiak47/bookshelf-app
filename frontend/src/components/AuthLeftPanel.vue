<script setup>
  /**
   * Lewy panel strony logowania/rejestracji — branding i wizualna prezentacja platformy.
   * Użycie: AuthView.vue — stała kolumna obok LoginForm / RegisterForm.
   * Brak propsów; okładki z useShowcaseOffers, statystyki ze statsStore.
   * Klik w okładkę z offerId otwiera ofertę w przeglądaniu.
   */
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P, AUTH_DEMO_BOOK_BADGE } from '../assets/constants';
  import { offerBrowseLabel } from '../lib/offerTypes';
  import { openBrowseOffer } from '../lib/browseNavigation';
  import { coverColor } from '../lib/showcaseOffers';
  import { useShowcaseOffers } from '../composables/useShowcaseOffers';
  import { useStatsStore } from '../stores/stats';

  const router = useRouter();
  const statsStore = useStatsStore();
  const { slots: showcaseSlots } = useShowcaseOffers(3, { w: 150, h: 200 });
  const coverFailed = ref({});

  onMounted(() => statsStore.fetchStats());

  const fmt = (n) => (n == null ? '—' : n.toLocaleString('pl-PL'));

  const demoBadgeStyle = (type) => {
    const colors = AUTH_DEMO_BOOK_BADGE[type] ?? { bg: T.green, text: 'white' };
    return {
      background: colors.bg,
      color: colors.text,
      fontSize: '9px',
      fontWeight: 600,
      letterSpacing: '0.4px',
      padding: '3px 7px',
      borderRadius: '4px',
    };
  };

  const spreads = [
    { transform: 'rotate(-9deg) translate(-22px, 10px)', z: 1, float: 'float-a' },
    { transform: 'rotate(1deg)  translate(4px, -4px)', z: 3, float: 'float-c' },
    { transform: 'rotate(9deg)  translate(22px, 6px)', z: 2, float: 'float-b' },
  ];

  const stats = computed(() => [
    { icon: 'book', value: fmt(statsStore.stats?.totalBooks), label: 'książek' },
    { icon: 'users', value: fmt(statsStore.stats?.totalUsers), label: 'użytkowników' },
    { icon: 'repeat', value: fmt(statsStore.stats?.activeOffers), label: 'aktywnych ofert' },
  ]);

  const markCoverFailed = (id) => {
    coverFailed.value = { ...coverFailed.value, [id]: true };
  };

  const onCoverClick = (slot) => {
    if (slot.offerId) openBrowseOffer(router, slot.offerId);
  };
</script>

<template>
  <!-- Zielony panel: logo, fan okładek, liczniki platformy -->
  <div class="left-panel" :style="{
    flex: '0 0 50%',
    position: 'sticky', top: 0, height: '100vh',
    alignSelf: 'flex-start',
    background: T.green,
    display: 'flex', flexDirection: 'column',
    alignItems: 'center', justifyContent: 'space-between',
    padding: '56px 48px 48px',
    overflow: 'hidden',
  }">
    <svg aria-hidden="true" :style="{ position: 'absolute', inset: 0, width: '100%', height: '100%', opacity: 0.07, pointerEvents: 'none' }">
      <defs>
        <pattern id="bsgrid" width="48" height="48" patternUnits="userSpaceOnUse">
          <path d="M 48 0 L 0 0 0 48" fill="none" stroke="white" stroke-width="1" />
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="url(#bsgrid)" />
    </svg>

    <div aria-hidden="true" :style="{
      position: 'absolute',
      top: '28%', left: '50%',
      transform: 'translate(-50%, -50%)',
      width: '420px', height: '420px',
      background: 'radial-gradient(circle, rgba(212,168,67,0.18) 0%, transparent 68%)',
      pointerEvents: 'none',
    }" />

    <div :style="{ textAlign: 'center', position: 'relative', zIndex: 2 }">
      <RouterLink to="/" :style="{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '13px', marginBottom: '14px', textDecoration: 'none'}">
        <div :style="{
          width: '50px', height: '50px', borderRadius: '13px',
          background: 'rgba(255,255,255,0.14)',
          border: '1px solid rgba(255,255,255,0.2)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.book" />
        </div>
        <span :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '34px', fontWeight: 700, color: 'white', letterSpacing: '-0.5px' }">
          Bookshelf
        </span>
      </RouterLink>
      <p :style="{ color: 'rgba(255,255,255,0.6)', fontSize: '15px', fontWeight: 300, letterSpacing: '0.8px' }">
        Świat książek w Twoich rękach
      </p>
    </div>

    <!-- Trzy nachodzące okładki z animacją float — prawdziwe oferty lub placeholdery -->
    <div :style="{ position: 'relative', width: '200px', height: '290px', zIndex: 2 }">
      <div
        v-for="(slot, i) in showcaseSlots"
        :key="slot.id"
        :style="{
          position: 'absolute',
          width: '150px', height: '200px',
          top: '50%', left: '50%',
          marginTop: '-100px', marginLeft: '-75px',
          transform: spreads[i]?.transform,
          zIndex: spreads[i]?.z,
        }"
      >
        <div
          :class="[spreads[i]?.float, { 'auth-showcase--clickable': slot.offerId }]"
          :style="{
            width: '100%', height: '100%',
            borderRadius: '10px',
            overflow: 'hidden',
            boxShadow: '0 18px 48px rgba(0,0,0,0.45)',
            position: 'relative',
          }"
          :role="slot.offerId ? 'button' : undefined"
          :tabindex="slot.offerId ? 0 : undefined"
          @click="onCoverClick(slot)"
          @keydown.enter="onCoverClick(slot)"
        >
          <img
            v-if="slot.isPlaceholder"
            :src="slot.placeholderImage"
            alt=""
            :style="{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }"
          >
          <img
            v-else-if="slot.coverUrl && !coverFailed[slot.id]"
            :src="slot.coverUrl"
            :alt="slot.title"
            :style="{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }"
            @error="markCoverFailed(slot.id)"
          >
          <div
            v-else
            class="auth-showcase-fallback cover-fallback"
            :style="{ background: coverColor(slot.title) }"
          >
            <span class="cover-fallback-title">{{ slot.title }}</span>
            <span class="cover-fallback-author">{{ slot.author }}</span>
          </div>

          <template v-if="!slot.isPlaceholder">
            <div :style="{ position: 'absolute', top: '8px', left: '8px', ...demoBadgeStyle(slot.type) }">
              {{ offerBrowseLabel(slot.type) }}
            </div>

            <div :style="{
              position: 'absolute', bottom: 0, left: 0, right: 0,
              background: 'linear-gradient(to top, rgba(0,0,0,0.8) 0%, transparent 100%)',
              padding: '22px 10px 10px',
            }">
              <p :style="{ fontFamily: '\'Playfair Display\', serif', color: 'white', fontSize: '11px', fontWeight: 600, lineHeight: 1.3, marginBottom: '2px' }">
                {{ slot.title }}
              </p>
              <p :style="{ color: 'rgba(255,255,255,0.65)', fontSize: '10px' }">{{ slot.author }}</p>
            </div>
          </template>
        </div>
      </div>
    </div>

    <div :style="{ display: 'flex', gap: '28px', position: 'relative', zIndex: 2 }">
      <div v-for="(s, i) in stats" :key="i" :style="{ textAlign: 'center' }">
        <div :style="{
          width: '40px', height: '40px', borderRadius: '10px',
          background: 'rgba(255,255,255,0.1)',
          border: '1px solid rgba(255,255,255,0.15)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          margin: '0 auto 8px',
        }">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.7)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P[s.icon]" />
        </div>
        <p :style="{ fontFamily: '\'Playfair Display\', serif', color: 'white', fontSize: '20px', fontWeight: 700, lineHeight: 1 }">
          {{ s.value }}
        </p>
        <p :style="{ color: 'rgba(255,255,255,0.55)', fontSize: '11px', marginTop: '3px' }">{{ s.label }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .auth-showcase--clickable {
    cursor: pointer;
  }

  .auth-showcase--clickable:focus-visible {
    outline: 2px solid rgba(255, 255, 255, 0.85);
    outline-offset: 3px;
  }

  .auth-showcase-fallback.cover-fallback {
    width: 100%;
    height: 100%;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 12px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
  }

  .auth-showcase-fallback .cover-fallback-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 11px;
    line-height: 1.25;
  }

  .auth-showcase-fallback .cover-fallback-author {
    font-size: 9px;
    opacity: 0.8;
  }
</style>
