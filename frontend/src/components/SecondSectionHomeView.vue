<script setup>
  /** Strona główna: siatka ostatnich ofert z API (GET /api/userbooks/recent). */
  import { ref, onMounted, watch, nextTick } from 'vue';
  import { RouterLink } from 'vue-router';
  import { userBooksApi } from '../lib/userBooks';
  import { mapUserBookToCard } from '../lib/mappers';
  import { usePageAnimations, observeAnimationsIn } from '../composables/useAnimations';
  import { offerBadgeClass, offerBrowseLabel } from '../lib/offerTypes';

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
          <RouterLink :to="'/przegladanie'" class="book-card">
            <div class="book-cover-wrap">
              <img :src="book.coverUrl || `https://picsum.photos/180/240?random=${book.img}`" :alt="book.title" class="book-cover">
              <span :class="['book-badge', badgeClass(book.type)]">{{ badgeText(book.type) }}</span>
              <span class="book-action">Zobacz ofertę</span>
            </div>
            <div class="book-info">
              <h4 class="book-title">{{ book.title }}</h4>
              <p class="book-author">{{ book.author }}</p>
            </div>
          </RouterLink>
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
