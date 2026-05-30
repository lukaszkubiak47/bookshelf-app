<script setup lang="ts">
  import { ref, computed } from 'vue';
  import BookFilters from '../components/BookFiltersShelfView.vue';
  import BookOffer from '../components/BookOffer.vue';

  // Definicja stanu i filtrów (zawsze jako stringi, by uniknąć undefined)
  const currentTab = ref<'received' | 'sent' | 'listed'>('received');
  const statusFilter = ref('');
  const typeFilter = ref('');

  const receivedBooks = ref([
    { id: 'r1', status: 'pending', type: 'loan', bookTitle: 'Hobbit', bookAuthor: 'J.R.R. Tolkien', img: 35, nick: 'anna_m', av: 3, date: '1 dzień temu', msg: 'Cześć! Czy mogę pożyczyć na 2 tygodnie? Zwrócę w idealnym stanie.', returnDate: '28.06.2026', contact: null },
    { id: 'r2', status: 'pending', type: 'swap', bookTitle: 'Lalka', bookAuthor: 'Bolesław Prus', img: 41, nick: 'zosia_k', av: 11, date: '2 dni temu', msg: 'Mam kilka książek z literatury pięknej, chętnie wymienię!', wants: 'Literatura piękna', contact: null },
    { id: 'r3', status: 'pending', type: 'sale', bookTitle: 'Sapiens', bookAuthor: 'Yuval Harari', img: 32, nick: 'kasia_w', av: 7, date: '3 dni temu', msg: 'Chciałabym kupić, czy cena jest do negocjacji?', price: 35, contact: null },
    { id: 'r4', status: 'accepted', type: 'loan', bookTitle: 'Solaris', bookAuthor: 'Stanisław Lem', img: 44, nick: 'bartek_p', av: 12, date: '1 tydzień temu', msg: '', returnDate: '15.07.2026', contact: { email: 'bartek.p@gmail.com', nick: '@bartek_p' } },
    { id: 'r5', status: 'accepted', type: 'swap', bookTitle: 'Diuna', bookAuthor: 'Frank Herbert', img: 22, nick: 'piotr_n', av: 5, date: '5 dni temu', msg: 'Mam kilka książek Sci-Fi, sprawdź mój profil!', wants: 'Sci-Fi', contact: { email: 'piotr.nowak@czytelnik.pl', nick: '@piotr_n' } },
    { id: 'r6', status: 'accepted', type: 'sale', bookTitle: 'Atomic Habits', bookAuthor: 'James Clear', img: 46, nick: 'magda_r', av: 13, date: '4 dni temu', msg: '', price: 28, contact: { email: 'magda.r@outlook.com', nick: '@magda_r' } },
    { id: 'r7', status: 'done', type: 'loan', bookTitle: 'Wiedźmin: Krew elfów', bookAuthor: 'A. Sapkowski', img: 47, nick: 'tomek_r', av: 4, date: '3 tygodnie temu', msg: '', returnDate: '01.06.2026', contact: null },
    { id: 'r8', status: 'rejected', type: 'swap', bookTitle: 'Ferdydurke', bookAuthor: 'Witold Gombrowicz', img: 48, nick: 'ola_s', av: 14, date: '2 tygodnie temu', msg: '', wants: 'Wiedźmin', contact: null },
    { id: 'r9', status: 'done', type: 'sale', bookTitle: 'Thinking, Fast and Slow', bookAuthor: 'Daniel Kahneman', img: 49, nick: 'adam_z', av: 15, date: '1 miesiąc temu', msg: '', price: 42, contact: null },
  ]);

  const sentBooks = ref([
    { id: 's1', status: 'pending', type: 'loan', bookTitle: 'Sto lat samotności', bookAuthor: 'G. García Márquez', img: 21, nick: 'maria_kow', av: 2, date: '3 dni temu', msg: 'Bardzo chętnie wypożyczę, mam wolne dwa tygodnie na spokojne czytanie.', returnDate: '10.07.2026', contact: null },
    { id: 's2', status: 'pending', type: 'swap', bookTitle: 'Wiedźmin: Ostatnie życzenie', bookAuthor: 'A. Sapkowski', img: 23, nick: 'marek_z', av: 9, date: '2 dni temu', msg: 'Chętnie wymienię za coś z fantasy!', wants: 'Fantasy', contact: null },
    { id: 's3', status: 'pending', type: 'sale', bookTitle: 'Clean Code', bookAuthor: 'Robert C. Martin', img: 50, nick: 'paweł_d', av: 16, date: '1 dzień temu', msg: 'Zainteresowany, czy można odebrać osobiście?', price: 55, contact: null },
    { id: 's4', status: 'accepted', type: 'loan', bookTitle: 'Pan Tadeusz', bookAuthor: 'Adam Mickiewicz', img: 51, nick: 'ewa_k', av: 6, date: '1 tydzień temu', msg: '', returnDate: '20.07.2026', contact: { email: 'ewa.k@wp.pl', nick: '@ewa_k' } },
    { id: 's5', status: 'accepted', type: 'swap', bookTitle: 'Władca Pierścieni', bookAuthor: 'J.R.R. Tolkien', img: 27, nick: 'tomek_r', av: 4, date: '1 tydzień temu', msg: '', wants: 'cokolwiek', contact: { email: 'tomek.r@gmail.com', nick: '@tomek_r' } },
    { id: 's6', status: 'accepted', type: 'sale', bookTitle: 'Potop', bookAuthor: 'Henryk Sienkiewicz', img: 52, nick: 'julia_m', av: 17, date: '5 dni temu', msg: '', price: 30, contact: { email: 'julia.m@onet.pl', nick: '@julia_m' } },
    { id: 's7', status: 'done', type: 'loan', bookTitle: 'Mały Książę', bookAuthor: 'A. de Saint-Exupéry', img: 29, nick: 'agata_ww', av: 8, date: '3 tygodnie temu', msg: '', returnDate: '25.05.2026', contact: null },
    { id: 's8', status: 'done', type: 'swap', bookTitle: 'Zbrodnia i kara', bookAuthor: 'Fiodor Dostojewski', img: 53, nick: 'natalia_b', av: 18, date: '1 miesiąc temu', msg: '', wants: 'Klasyka', contact: null },
    { id: 's9', status: 'rejected', type: 'sale', bookTitle: 'Rok 1984', bookAuthor: 'George Orwell', img: 28, nick: 'ewa_k', av: 6, date: '10 dni temu', msg: '', price: 25, contact: null }
  ]);

  const listedBooks = ref([
    { id: 'l1', type: 'loan', bookTitle: 'Hobbit', bookAuthor: 'J.R.R. Tolkien', img: 35, price: null, dateAdded: '02.05.2026', listStatus: 'active' },
    { id: 'l2', type: 'loan', bookTitle: 'Diuna', bookAuthor: 'Frank Herbert', img: 22, price: null, dateAdded: '08.05.2026', listStatus: 'available' },
    { id: 'l3', type: 'swap', bookTitle: 'Mistrz i Małgorzata', bookAuthor: 'Michaił Bułhakow', img: 24, price: null, dateAdded: '01.05.2026', listStatus: 'available' },
    { id: 'l4', type: 'swap', bookTitle: 'Sapiens', bookAuthor: 'Yuval Noah Harari', img: 32, price: null, dateAdded: '10.05.2026', listStatus: 'active' },
    { id: 'l5', type: 'sale', bookTitle: 'Wiedźmin: Ostatnie życzenie', bookAuthor: 'A. Sapkowski', img: 23, price: 35, dateAdded: '05.05.2026', listStatus: 'active' },
    { id: 'l6', type: 'sale', bookTitle: 'Rok 1984', bookAuthor: 'George Orwell', img: 28, price: 25, dateAdded: '12.05.2026', listStatus: 'available' },
  ]);

  // Bezpieczna funkcja filtrująca listy
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

  // Bezpieczne grupowanie na sekcje
  const groupedTransactions = computed(() => {
    const list = activeTransactionList.value || [];
    return {
      pending: list.filter(t => t.status === 'pending'),
      accepted: list.filter(t => t.status === 'accepted'),
      rest: list.filter(t => t.status === 'done' || t.status === 'rejected'),
    };
  });

  const totalCount = computed(() => receivedBooks.value.length + sentBooks.value.length + listedBooks.value.length);

  // Obsługa akcji z kart transakcji
  const handleTransactionAction = (payload: { id: string, status: string, returnDate?: string }) => {
    const targetList = currentTab.value === 'received' ? receivedBooks.value : sentBooks.value;
    const item = targetList.find(t => t.id === payload.id);
    if (item) {
      item.status = payload.status;
      if (payload.returnDate) item.returnDate = payload.returnDate;
      if (payload.status === 'accepted' && !item.contact) {
        item.contact = { email: `${item.nick}@czytelnik.pl`, nick: `@${item.nick}` };
      }
    }
  };

  // Obsługa akcji z kart wystawionych
  const deleteListedBook = (id: string) => {
    listedBooks.value = listedBooks.value.filter(b => b.id !== id);
  };

  const editListedBook = (book: any) => {
    alert(`Edycja oferty: „${book.bookTitle}”\n(funkcja w przygotowaniu)`);
  };
</script>

<template>
  <div class="page">
    <div class="page-header">
      <h1 class="page-title">Moja półka</h1>
      <span class="page-count">Razem: {{ totalCount }}</span>
    </div>

    <div class="controls-row">
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

    <div class="tab-panels-wrap">
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

<style>
  :root {
    --bg: #FAF7F2;
    --surface: #FFFFFF;
    --primary: #2D6A4F;
    --primary-h: #245A42;
    --accent: #D4A843;
    --blue: #2C5F8A;
    --text-1: #2C1810;
    --text-2: #7A6652;
    --text-3: #B5A090;
    --border: #E8DDD4;
    --tag-bg: #F0E8DF;
    --danger: #C0392B;
    --success: #27AE60;
    --warning: #E67E22;
    --st-pending-bg: #FFF3CD;
    --st-pending-tx: #856404;
    --st-accept-bg: #D1E7DD;
    --st-accept-tx: #0A3622;
    --st-done-bg: #E2E3E5;
    --st-done-tx: #41464B;
    --st-reject-bg: #F8D7DA;
    --st-reject-tx: #842029;
    --shadow: 0 2px 8px rgba(44,24,16,.08);
    --shadow-h: 0 4px 16px rgba(44,24,16,.12);
    --r-card: 12px;
    --r-btn: 8px;
  }

  body {
    font-family: 'DM Sans',sans-serif;
    background: var(--bg);
    color: var(--text-1);
    -webkit-font-smoothing: antialiased;
    margin: 0;
    padding: 0;
  }

  .page {
    max-width: 860px;
    margin: 0 auto;
    padding: 36px 24px 80px;
  }

  .page-header {
    display: flex;
    align-items: baseline;
    gap: 14px;
    margin-bottom: 24px;
  }

  .page-title {
    font-family: 'Playfair Display',serif;
    font-size: 32px;
    font-weight: 600;
  }

  .page-count {
    font-size: 14px;
    color: var(--text-3);
  }

  .controls-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 24px;
    flex-wrap: wrap;
  }

  .tabs {
    display: flex;
    gap: 6px;
  }

  .tab-btn {
    display: flex;
    align-items: center;
    gap: 7px;
    padding: 8px 18px;
    border-radius: 20px;
    border: none;
    cursor: pointer;
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    font-weight: 500;
    transition: all .2s;
    background: var(--tag-bg);
    color: var(--text-2);
    white-space: nowrap;
  }

    .tab-btn:hover {
      background: var(--border);
      color: var(--text-1);
    }

    .tab-btn.active {
      background: var(--primary);
      color: #fff;
    }

  .tab-cnt {
    font-size: 11px;
    opacity: .8;
  }

  .tab-panel {
    opacity: 1;
    position: relative;
  }

  .listed-grid {
    display: grid;
    grid-template-columns: repeat(3,1fr);
    gap: 18px;
  }

  .section-div {
    display: flex;
    align-items: center;
    gap: 12px;
    margin: 20px 0 14px;
  }

    .section-div span {
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: .6px;
      color: var(--text-3);
      white-space: nowrap;
    }

    .section-div::before, .section-div::after {
      content: '';
      flex: 1;
      height: 1px;
      background: var(--border);
    }

  .empty-state {
    text-align: center;
    padding: 60px 20px;
  }

  .empty-icon {
    font-size: 40px;
    margin-bottom: 10px;
  }

  @media (max-width:640px) {
    .listed-grid {
      grid-template-columns: repeat(2,1fr);
      gap: 12px;
    }

    .controls-row {
      flex-direction: column;
      align-items: flex-start;
    }

    .page {
      padding: 24px 16px 60px;
    }
  }
</style>
