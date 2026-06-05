<script setup>
  /**
   * Przeglądaj książki: lista publicznych ofert, filtry, sortowanie, modal szczegółów.
   * Po akceptacji/zakończeniu transakcji odświeża listę (OFFERS_CHANGED_EVENT).
   */
  import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { OFFERS_CHANGED_EVENT } from '../lib/offers';
  import { usePageAnimations, observeAnimationsIn } from '../composables/useAnimations';
  import BookFiltersBrowseView from '../components/BookFiltersBrowseView.vue';
  import BookCard from '../components/BookCard.vue';
  import OfferModal from '../components/OfferModal.vue';
  import { userBooksApi } from '../lib/userBooks';
  import { mapUserBookToCard } from '../lib/mappers';
  import { useAuthStore } from '../stores/auth';

  const route = useRoute();
  const auth = useAuthStore();
  const router = useRouter();
  const pageRef = ref(null);
  const books = ref([]);

  usePageAnimations({ scroll: true, ripple: true, root: pageRef });

  // --- Filtry i paginacja ---
  const total = ref(0);
  const loading = ref(false);
  const loadError = ref('');

  const search = ref('');
  const city = ref('');
  const stateTypes = ref(new Set(['loan', 'swap', 'sale']));
  const stateGenres = ref(new Set());
  const sortBy = ref('newest');
  const viewMode = ref('grid');
  const page = ref(1);
  const limit = ref(12);

  const typeCounts = computed(() => ({
    loan: books.value.filter(b => b.type === 'loan').length,
    swap: books.value.filter(b => b.type === 'swap').length,
    sale: books.value.filter(b => b.type === 'sale').length,
  }));

  const availableGenres = computed(() => {
    const genres = [...new Set(books.value.map(b => b.genre))].sort();
    return genres.map(g => ({
      name: g,
      count: books.value.filter(b => b.genre === g).length
    }));
  });

  const toggleType = (type) => {
    if (stateTypes.value.has(type)) stateTypes.value.delete(type);
    else stateTypes.value.add(type);
  };

  const toggleGenre = (genre) => {
    if (stateGenres.value.has(genre)) stateGenres.value.delete(genre);
    else stateGenres.value.add(genre);
  };

  const resetFilters = () => {
    stateTypes.value = new Set(['loan', 'swap', 'sale']);
    stateGenres.value.clear();
    search.value = '';
    city.value = '';
    page.value = 1;
  };

  /** Pobiera oferty z API; append=true dokleja kolejną stronę (przycisk „Więcej”). */
  async function fetchBooks(append = false) {
    loading.value = true;
    loadError.value = '';
    try {
      const types = [...stateTypes.value].join(',');
      const genres = [...stateGenres.value].join(',');
      const result = await userBooksApi.getAll({
        page: page.value,
        limit: limit.value,
        type: types || undefined,
        genre: genres || undefined,
        city: city.value || undefined,
        search: search.value || undefined,
        sort: sortBy.value,
      });
      const mapped = result.items.map(mapUserBookToCard);
      books.value = append ? [...books.value, ...mapped] : mapped;
      total.value = result.total;
      await nextTick();
      if (pageRef.value) observeAnimationsIn(pageRef.value);
    } catch {
      loadError.value = 'Nie udało się pobrać ofert. Upewnij się, że backend działa.';
      if (!append) books.value = [];
    } finally {
      loading.value = false;
    }
  }

  let debounceTimer;
  watch([search, city, stateTypes, stateGenres, sortBy], () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      page.value = 1;
      fetchBooks();
    }, 350);
  }, { deep: true });

  function refreshBrowse() {
    page.value = 1;
    fetchBooks();
  }

  watch(
    () => route.name,
    (name) => {
      if (name === 'browse') refreshBrowse();
    },
    { immediate: true },
  );

  onMounted(() => {
    window.addEventListener(OFFERS_CHANGED_EVENT, refreshBrowse);
  });

  onUnmounted(() => {
    window.removeEventListener(OFFERS_CHANGED_EVENT, refreshBrowse);
  });

  /** Dodatkowy filtr po stronie klienta — tylko status available (spójnie z backendem). */
  const displayedBooks = computed(() =>
    books.value.filter((b) => b.status === 'available'),
  );

  const loadMore = () => {
    if (books.value.length >= total.value) return;
    page.value += 1;
    fetchBooks(true);
  };

  const selectedOfferId = ref(null);
  const openModal = (id) => {
    selectedOfferId.value = id;
  };

  const closeModal = () => {
    selectedOfferId.value = null;
    if (route.query.offer) {
      router.replace({ name: 'browse', query: {} });
    }
  };

  /** Otwarcie modala z ?offer= (klik okładki na stronie głównej / logowaniu). */
  function syncOfferFromQuery() {
    const raw = route.query.offer;
    const id = typeof raw === 'string' ? raw : Array.isArray(raw) ? raw[0] : null;
    selectedOfferId.value = id || null;
  }

  watch(() => route.query.offer, syncOfferFromQuery, { immediate: true });

  const cardDelayClass = (i) => `anim-d${(i % 5) + 1}`;

  const isOwnOffer = (book) => !!auth.user?.id && book.ownerId === auth.user.id;
</script>

<template>
  <div ref="pageRef" class="page-wrapper">
    <div class="page-header anim">
      <h1 class="page-title">Przeglądaj książki</h1>
      <span class="result-count">Znaleziono <strong>{{ total }}</strong> ofert</span>
    </div>

    <p v-if="loadError" class="load-error">{{ loadError }}</p>

    <div class="page-layout">
      <BookFiltersBrowseView v-model:search="search"
                             v-model:city="city"
                             :types="stateTypes"
                             :genres="stateGenres"
                             :available-genres="availableGenres"
                             :counts="typeCounts"
                             @toggle-type="toggleType"
                             @toggle-genre="toggleGenre"
                             @reset="resetFilters" />

      <main>
        <div style="display:flex; justify-content:flex-end; margin-bottom:14px;">
          <RouterLink to="/dodawanie" class="btn btn-filled">
            <svg width="13" height="13" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><line x1="7" y1="1" x2="7" y2="13" /><line x1="1" y1="7" x2="13" y2="7" /></svg>
            Dodaj książkę
          </RouterLink>
        </div>

        <div class="sort-bar">
          <div class="sort-left">
            <span class="sort-label">Sortuj:</span>
            <select class="sort-select" v-model="sortBy">
              <option value="newest">Najnowsze</option>
              <option value="oldest">Najstarsze</option>
              <option value="title">Tytuł A–Z</option>
            </select>
          </div>
          <div class="view-toggle">
            <button :class="['view-btn', { active: viewMode === 'grid' }]" @click="viewMode = 'grid'" title="Widok siatki">
              <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor"><rect x="0" y="0" width="6" height="6" rx="1" /><rect x="8" y="0" width="6" height="6" rx="1" /><rect x="0" y="8" width="6" height="6" rx="1" /><rect x="8" y="8" width="6" height="6" rx="1" /></svg>
            </button>
            <button :class="['view-btn', { active: viewMode === 'list' }]" @click="viewMode = 'list'" title="Widok listy">
              <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><line x1="0" y1="3" x2="14" y2="3" /><line x1="0" y1="7" x2="14" y2="7" /><line x1="0" y1="11" x2="14" y2="11" /></svg>
            </button>
          </div>
        </div>

        <div v-if="loading && books.length === 0" class="empty">
          <p>Ładowanie ofert…</p>
        </div>

        <div v-else-if="displayedBooks.length === 0" class="empty">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"><circle cx="11" cy="11" r="7" /><line x1="16.5" y1="16.5" x2="22" y2="22" /></svg>
          <p>Brak ofert pasujących do filtrów</p>
        </div>

        <div v-else :class="['books-grid', { 'list': viewMode === 'list' }]">
          <BookCard
            v-for="(book, i) in displayedBooks"
            :key="book.id"
            :class="['anim', cardDelayClass(i)]"
            :book="book"
            :is-own="isOwnOffer(book)"
            @select="openModal"
          >
            <template v-if="viewMode === 'list'" #meta="{ book: cardBook, ownerInitial }">
              <div class="card-meta card-meta--list">
                <img v-if="cardBook.avatarUrl" :src="cardBook.avatarUrl" :alt="cardBook.owner" class="meta-av">
                <div v-else class="meta-av meta-av-fallback" aria-hidden="true">{{ ownerInitial }}</div>
                <div class="meta-info">
                  <span class="meta-nick">@{{ cardBook.owner }}</span>
                  <span class="meta-city">{{ cardBook.city }}</span>
                </div>
              </div>
            </template>
          </BookCard>
        </div>

        <div class="load-more" v-if="books.length < total">
          <button class="btn btn-outline" :disabled="loading" @click="loadMore" style="min-width:200px">
            {{ loading ? 'Ładowanie…' : 'Załaduj więcej ofert' }}
          </button>
        </div>
      </main>
    </div>

    <OfferModal :offer-id="selectedOfferId" @close="closeModal" />
  </div>
</template>

<style scoped>
  .card-meta--list {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .card-meta--list .meta-city {
    display: block;
    font-size: 12px;
    color: var(--text-3);
  }

  .card-meta--list .meta-av {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    object-fit: cover;
    flex-shrink: 0;
  }

  .card-meta--list .meta-av-fallback {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: var(--tag-bg);
    color: var(--text-2);
    font-weight: 600;
    font-size: 13px;
    flex-shrink: 0;
  }

  .load-error {
    color: var(--danger, #c0392b);
    margin-bottom: 16px;
    font-size: 14px;
  }
</style>
