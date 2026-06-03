<script setup>
  /** Strona główna: siatka ostatnich ofert z API (GET /api/userbooks/recent). */
  import { ref, onMounted, watch, nextTick } from 'vue';
  import { RouterLink, useRouter } from 'vue-router';
  import { userBooksApi } from '../lib/userBooks';
  import { mapUserBookToCard, coverColor } from '../lib/mappers';
  import { openBrowseOffer } from '../lib/browseNavigation';
  import { usePageAnimations, observeAnimationsIn } from '../composables/useAnimations';
  import { offerBadgeClass, offerBrowseLabel } from '../lib/offerTypes';

  const router = useRouter();
  const coverFailed = ref({});

  const recentBooks = ref([]);
  const loadingRecent = ref(true);
  const sectionRef = ref(null);

  usePageAnimations({ hero: false, scroll: true, ripple: true, root: sectionRef });

  onMounted(async () => {
    try {
      const items = await userBooksApi.getRecent(5);
      recentBooks.value = items.map(mapUserBookToCard);
    } catch {
      recentBooks.value = [];
    } finally {
      loadingRecent.value = false;
    }
  });

  watch(recentBooks, async () => {
    await nextTick();
    if (sectionRef.value) observeAnimationsIn(sectionRef.value);
  });

  const badgeClass = (type) => offerBadgeClass(type);
  const badgeText = (type) => offerBrowseLabel(type);
  const delayClass = (i) => `anim-d${Math.min(i + 1, 5)}`;

  const markCoverFailed = (id) => {
    coverFailed.value = { ...coverFailed.value, [id]: true };
  };

  const openOffer = (offerId) => {
    openBrowseOffer(router, offerId);
  };
</script>

<template>
  <section ref="sectionRef" class="section">
    <div class="section-inner">
      <div class="section-header anim">
        <h2 class="section-title">Świeże oferty</h2>
        <RouterLink to="/przegladanie" class="section-link">
          Zobacz wszystkie
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M2 7h10M7 2l5 5-5 5" />
          </svg>
        </RouterLink>
      </div>

      <p v-if="loadingRecent" class="section-hint">Ładowanie ofert…</p>
      <p v-else-if="recentBooks.length === 0" class="section-hint">Brak ofert — dodaj pierwszą książkę!</p>

      <div v-else class="books-grid">
        <div
          v-for="(book, i) in recentBooks"
          :key="book.id"
          :class="['book-card-wrap', 'anim', delayClass(i)]"
        >
          <div
            class="book-card book-card--clickable"
            role="button"
            tabindex="0"
            @click="openOffer(book.id)"
            @keydown.enter="openOffer(book.id)"
          >
            <div class="book-cover-wrap">
              <img
                v-if="book.coverUrl && !coverFailed[book.id]"
                :src="book.coverUrl"
                :alt="book.title"
                class="book-cover"
                @error="markCoverFailed(book.id)"
              >
              <div
                v-else
                class="book-cover cover-fallback"
                :style="{ background: coverColor(book.title) }"
              >
                <span class="cover-fallback-title">{{ book.title }}</span>
                <span class="cover-fallback-author">{{ book.author }}</span>
              </div>
              <span :class="['book-badge', badgeClass(book.type)]">{{ badgeText(book.type) }}</span>
              <span class="book-action">Zobacz ofertę</span>
            </div>
            <div class="book-info">
              <h4 class="book-title">{{ book.title }}</h4>
              <p class="book-author">{{ book.author }}</p>
            </div>
          </div>
          <div class="book-meta">
            <img :src="book.avatarUrl || `https://i.pravatar.cc/40?img=${book.av}`" :alt="book.owner" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@{{ book.owner }}</span> · {{ book.city }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
  .book-card--clickable {
    cursor: pointer;
    text-decoration: none;
    color: inherit;
  }

  .book-card--clickable:focus-visible {
    outline: 2px solid var(--primary);
    outline-offset: 3px;
    border-radius: var(--r-card, 12px);
  }

  .cover-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 16px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
    aspect-ratio: 3 / 4;
  }

  .cover-fallback-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 14px;
    line-height: 1.25;
  }

  .cover-fallback-author {
    font-size: 11px;
    opacity: 0.8;
  }
</style>
