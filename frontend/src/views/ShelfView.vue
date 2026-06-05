<script setup lang="ts">
  /**
   * Trasa: /polka (name: shelf) — wymaga zalogowania (meta.requiresAuth).
   * Cel: panel użytkownika — transakcje (otrzymane/wysłane) i wystawione oferty.
   * API: transactionsApi.getReceived/getSent, transactionsApi.accept/reject/finish,
   *       userBooksApi.getMy/remove.
   * Komponenty: BookFiltersShelfView, BookOffer.
   * Po akceptacji/zakończeniu transakcji wywołuje notifyOffersChanged() — odświeża przeglądaj.
   */
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import BookFilters from '../components/BookFiltersShelfView.vue';
  import BookOffer from '../components/BookOffer.vue';
  import { transactionsApi } from '../lib/transactions';
  import { userBooksApi } from '../lib/userBooks';
  import { mapTransactionToShelfItem, mapUserBookToListed } from '../lib/mappers';
  import { ApiError } from '../lib/client';
  import { notifyOffersChanged } from '../lib/offers';
  import { usePageAnimations } from '../composables/useAnimations';

  const router = useRouter();
  const pageRef = ref(null);
  usePageAnimations({ scroll: true, ripple: true, root: pageRef });

  // Domyślnie "Wystawione" — po wejściu na półkę (i po dodaniu książki) user najpierw chce zobaczyć swoje oferty.
  const currentTab = ref<'received' | 'sent' | 'listed'>('listed');
  const statusFilter = ref('');
  const typeFilter = ref('');
  const loading = ref(false);
  const loadError = ref('');

  const receivedBooks = ref<any[]>([]);
  const sentBooks = ref<any[]>([]);
  const listedBooks = ref<any[]>([]);

  async function loadShelf() {
    loading.value = true;
    loadError.value = '';
    try {
      const [received, sent, listed] = await Promise.all([
        transactionsApi.getReceived(),
        transactionsApi.getSent(),
        userBooksApi.getMy(),
      ]);
      receivedBooks.value = received.map((t) => mapTransactionToShelfItem(t, 'received'));
      sentBooks.value = sent.map((t) => mapTransactionToShelfItem(t, 'sent'));
      listedBooks.value = listed.map(mapUserBookToListed);
    } catch (err) {
      loadError.value = err instanceof ApiError ? err.message : 'Nie udało się załadować półki';
    } finally {
      loading.value = false;
    }
  }

  onMounted(loadShelf);

  const filterFn = (list: any[]) => {
    if (!Array.isArray(list)) return [];
    return list.filter(t => {
      const matchStatus = !statusFilter.value || t.status === statusFilter.value;
      const matchType = !typeFilter.value || t.type === typeFilter.value;
      return matchStatus && matchType;
    });
  };

  const filteredReceived = computed(() => filterFn(receivedBooks.value));
  const filteredSent = computed(() => filterFn(sentBooks.value));
  const activeTransactionList = computed(() => currentTab.value === 'received' ? filteredReceived.value : filteredSent.value);

  const groupedTransactions = computed(() => {
    const list = activeTransactionList.value || [];
    return {
      pending: list.filter(t => t.status === 'pending'),
      accepted: list.filter(t => t.status === 'accepted'),
      rest: list.filter(t => t.status === 'done' || t.status === 'rejected'),
    };
  });

  const totalCount = computed(() => receivedBooks.value.length + sentBooks.value.length + listedBooks.value.length);

  async function reloadTransactions() {
    const [received, sent] = await Promise.all([
      transactionsApi.getReceived(),
      transactionsApi.getSent(),
    ]);
    receivedBooks.value = received.map((t) => mapTransactionToShelfItem(t, 'received'));
    sentBooks.value = sent.map((t) => mapTransactionToShelfItem(t, 'sent'));
  }

  const handleTransactionAction = async (payload: { id: string, status: string, returnDate?: string }) => {
    try {
      if (payload.status === 'accepted') {
        const dueDate = payload.returnDate
          ? payload.returnDate.split('.').reverse().join('-')
          : undefined;
        await transactionsApi.accept(payload.id, { dueDate });
      } else if (payload.status === 'rejected') {
        await transactionsApi.reject(payload.id);
      } else if (payload.status === 'finish') {
        await transactionsApi.finish(payload.id);
      }
      await reloadTransactions();
      if (payload.status === 'accepted' || payload.status === 'finish') {
        notifyOffersChanged();
      }
    } catch (err) {
      loadError.value = err instanceof ApiError ? err.message : 'Operacja nie powiodła się';
    }
  };

  const deleteListedBook = async (id: string) => {
    try {
      await userBooksApi.remove(id);
      listedBooks.value = listedBooks.value.filter(b => b.id !== id);
    } catch (err) {
      loadError.value = err instanceof ApiError ? err.message : 'Nie udało się usunąć oferty';
    }
  };

  const editListedBook = (book: { id: string }) => {
    router.push({ name: 'editbook', params: { id: book.id } });
  };
</script>

<template>
  <div ref="pageRef" class="shelf-page">
    <!-- SEKCJA: Nagłówek strony + przycisk dodawania książki -->
    <div class="page-header anim">
      <div class="page-header-start">
        <h1 class="page-title">Moja półka</h1>
        <span class="page-count">Razem: {{ totalCount }}</span>
      </div>
      <RouterLink to="/dodawanie" class="btn btn-filled">
        <svg width="13" height="13" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><line x1="7" y1="1" x2="7" y2="13" /><line x1="1" y1="7" x2="13" y2="7" /></svg>
        Dodaj książkę
      </RouterLink>
    </div>

    <!-- SEKCJA: Komunikaty ładowania i błędów -->
    <p v-if="loadError" class="load-error">{{ loadError }}</p>
    <p v-if="loading" class="load-hint">Ładowanie…</p>

    <!-- SEKCJA: Zakładki (Otrzymane / Wysłane / Wystawione) + filtry transakcji -->
    <div class="controls-row anim anim-d1">
      <div class="tabs">
        <button :class="['tab-btn', { active: currentTab === 'received' }]" @click="currentTab = 'received'">
          Otrzymane <span class="tab-cnt">({{ filteredReceived.length }})</span>
        </button>
        <button :class="['tab-btn', { active: currentTab === 'sent' }]" @click="currentTab = 'sent'">
          Wysłane <span class="tab-cnt">({{ filteredSent.length }})</span>
        </button>
        <button :class="['tab-btn', { active: currentTab === 'listed' }]" @click="currentTab = 'listed'">
          📚 Wystawione <span class="tab-cnt">({{ listedBooks.length }})</span>
        </button>
      </div>

      <BookFilters v-show="currentTab !== 'listed'"
                   v-model:statusFilter="statusFilter"
                   v-model:typeFilter="typeFilter" />
    </div>

    <!-- SEKCJA: Panele zakładek — lista transakcji lub wystawionych ofert -->
    <div class="tab-panels-wrap">
      <!-- Panel transakcji: grupowanie po statusie (oczekujące / aktywne / zakończone) -->
      <div v-if="currentTab === 'received' || currentTab === 'sent'" class="tab-panel active">
        <div v-if="activeTransactionList.length === 0" class="empty-state">
          <div class="empty-icon">📂</div>
          <h3>Brak transakcji</h3>
          <p>Brak transakcji w tej kategorii</p>
        </div>

        <div v-else>
          <div v-if="groupedTransactions.pending.length">
            <div class="section-div">
              <span>{{ currentTab === 'sent' ? '⏳ OCZEKUJĄ NA ODPOWIEDŹ' : '⚡ WYMAGAJĄ DZIAŁANIA' }} ({{ groupedTransactions.pending.length }})</span>
            </div>
            <BookOffer v-for="t in groupedTransactions.pending"
                       :key="'pending-' + t.id"
                       :item="t"
                       :direction="currentTab"
                       @action="handleTransactionAction" />
          </div>

          <div v-if="groupedTransactions.accepted.length">
            <div class="section-div"><span>🔄 AKTYWNE ({{ groupedTransactions.accepted.length }})</span></div>
            <BookOffer v-for="t in groupedTransactions.accepted"
                       :key="'accepted-' + t.id"
                       :item="t"
                       :direction="currentTab"
                       @action="handleTransactionAction" />
          </div>

          <div v-if="groupedTransactions.rest.length">
            <div class="section-div"><span>📋 ZAKOŃCZONE I ODRZUCONE ({{ groupedTransactions.rest.length }})</span></div>
            <BookOffer v-for="t in groupedTransactions.rest"
                       :key="'rest-' + t.id"
                       :item="t"
                       :direction="currentTab"
                       @action="handleTransactionAction" />
          </div>
        </div>
      </div>

      <!-- Panel wystawionych ofert — edycja i usuwanie własnych książek -->
      <div v-if="currentTab === 'listed'" class="tab-panel active">
        <div v-if="listedBooks.length === 0" class="empty-state">
          <div class="empty-icon">📚</div>
          <h3>Brak wystawionych książek</h3>
          <p>Dodaj swoją pierwszą ofertę</p>
        </div>
        <div v-else class="listed-grid">
          <BookOffer v-for="b in listedBooks"
                     :key="'listed-' + b.id"
                     :item="b"
                     :is-listed="true"
                     @delete="deleteListedBook"
                     @edit="editListedBook" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .load-error {
    color: var(--danger, #c0392b);
    margin-bottom: 12px;
    font-size: 14px;
  }

  .load-hint {
    color: var(--text-3);
    margin-bottom: 12px;
    font-size: 14px;
  }
</style>
