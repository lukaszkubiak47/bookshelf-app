<template>
  <div class="page-wrapper">
    <div class="page-header">
      <h1 class="page-title">Przeglądaj książki</h1>
      <span class="result-count">Znaleziono <strong>{{ filteredBooks.length }}</strong> ofert</span>
    </div>

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
          <a href="Dodaj książkę.html" class="btn btn-filled">
            <svg width="13" height="13" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><line x1="7" y1="1" x2="7" y2="13" /><line x1="1" y1="7" x2="13" y2="7" /></svg>
            Dodaj książkę
          </a>
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

        <div v-if="displayedBooks.length === 0" class="empty">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"><circle cx="11" cy="11" r="7" /><line x1="16.5" y1="16.5" x2="22" y2="22" /></svg>
          <p>Brak ofert pasujących do filtrów</p>
        </div>

        <div v-else :class="['books-grid', { 'list': viewMode === 'list' }]">
          <BookCard v-for="book in displayedBooks"
                    :key="book.id"
                    :book="book"
                    @select="openModal" />
        </div>

        <div class="load-more" v-if="filteredBooks.length > limit">
          <button class="btn btn-outline" @click="limit += 6" style="min-width:200px">Załaduj więcej ofert</button>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed } from 'vue';
  import BookFiltersBrowseView from '../components/BookFiltersBrowseView.vue';
  import BookCard from '../components/BookCard.vue';

  const BOOKS = [
    { id: 1, title: "Sto lat samotności", author: "Gabriel García Márquez", type: "swap", genre: "Literatura piękna", city: "Warszawa", owner: "maria_kow", av: 1, ownerRating: 4.8, rating: 5, img: 21, isbn: "978-83-08-04821-3", desc: "Epicka saga rodziny Buendía w fikcyjnym miasteczku Macondo — arcydzieło realizmu magicznego, które zmienia sposób patrzenia na literaturę i rzeczywistość. Powieść zdobyła Nagrodę Nobla dla autora.", wants: "Fantasy lub cokolwiek" },
    { id: 2, title: "Diuna", author: "Frank Herbert", type: "loan", genre: "Sci-Fi", city: "Kraków", owner: "piotr_n", av: 5, ownerRating: 4.5, rating: 5, img: 22, isbn: "978-83-7469-983-7", desc: "Saga o rodzinie Atrydów na pustynnej planecie Arrakis, pełna intryg politycznych i mistycyzmu. Jedna z najważniejszych i najchętniej czytanych powieści science-fiction w historii gatunku.", delivery: "Osobiście" },
    { id: 3, title: "Wiedźmin: Ostatnie życzenie", author: "Andrzej Sapkowski", type: "sale", genre: "Fantasy", city: "Gdańsk", owner: "agata_ww", av: 9, ownerRating: 4.9, rating: 4, img: 23, isbn: "978-83-7578-066-2", desc: "Zbiór opowiadań o Geralcie z Rivii — łowcy potworów balansującym na granicy dobra i zła. Fundament polskiej literatury fantasy, osnuty na ludowych baśniach z mrocznym twistem.", price: "35 zł", condition: "Dobry" },
    { id: 4, title: "Mistrz i Małgorzata", author: "Michaił Bułhakow", type: "swap", genre: "Literatura piękna", city: "Wrocław", owner: "tomek_r", av: 3, ownerRating: 4.7, rating: 5, img: 24, isbn: "978-83-240-1383-8", desc: "Szatan odwiedza stalinowską Moskwę — satyryczna, miłosna i mistyczna opowieść w jednym. Jedno z najważniejszych dzieł literatury rosyjskiej XX wieku, pisane w konspiracji.", wants: "Kryminał lub Sci-Fi" },
    { id: 5, title: "Zbrodnia i kara", author: "Fiodor Dostojewski", type: "loan", genre: "Literatura piękna", city: "Poznań", owner: "kasia_pzn", av: 7, ownerRating: 4.6, rating: 4, img: 25, isbn: "978-83-7243-472-5", desc: "Psychologiczny portret studenta Raskolnikowa, który popełnia zbrodnię i zmaga się z wyrzutami sumienia. Klasyk rosyjskiej literatury, pełen egzystencjalnych pytań i napięcia.", delivery: "Pocztą (koszt po stronie pożyczkobiorcy)" },
    { id: 6, title: "Lśnienie", author: "Stephen King", type: "swap", genre: "Kryminał", city: "Łódź", owner: "bartek_ld", av: 12, ownerRating: 4.3, rating: 4, img: 26, isbn: "978-83-7552-919-5", desc: "Rodzina spędza zimę w odizolowanym hotelu Overlook — mroczna opowieść o szaleństwie i demonach przeszłości. Jedna z kultowych powieści grozy, którą King napisał po własnych doświadczeniach.", wants: "Horror lub Thriller" },
    { id: 7, title: "Władca Pierścieni", author: "J.R.R. Tolkien", type: "sale", genre: "Fantasy", city: "Warszawa", owner: "ela_book", av: 2, ownerRating: 5.0, rating: 5, img: 27, isbn: "978-83-7469-214-2", desc: "Epopeja fantasy śledząca Drużynę Pierścienia w misji zniszczenia Jedynego Pierścienia. Fundament nowoczesnej literatury fantastycznej, inspiracja dla niezliczonej liczby twórców.", price: "65 zł", condition: "Jak nowa" },
    { id: 8, title: "Rok 1984", author: "George Orwell", type: "loan", genre: "Sci-Fi", city: "Kraków", owner: "marek_k", av: 6, ownerRating: 4.8, rating: 5, img: 28, isbn: "978-83-7469-718-5", desc: "Dystopijny obraz totalitarnego państwa, gdzie Wielki Brat kontroluje każdy aspect życia. Profetyczna wizja napisana w 1949 roku, która wciąż niepokoi swoją aktualnością.", delivery: "Do uzgodnienia" },
    { id: 9, title: "Mały Książę", author: "Antoine de Saint-Exupéry", type: "swap", genre: "Literatura piękna", city: "Gdynia", owner: "zofia_g", av: 10, ownerRating: 4.9, rating: 5, img: 29, isbn: "978-83-7469-441-2", desc: "Filozoficzna baśń o małym księciu podróżującym przez planety w poszukiwaniu przyjaźni i sensu życia. Arcydzieło czytane przez wszystkich, niezależnie od wieku.", wants: "Literatura dla dzieci" },
    { id: 10, title: "Harry Potter i Kamień Filozoficzny", author: "J.K. Rowling", type: "sale", genre: "Fantasy", city: "Białystok", owner: "adam_bk", av: 4, ownerRating: 4.4, rating: 4, img: 30, isbn: "978-83-7469-321-7", desc: "Chłopiec odkrywa, że jest czarodziejem i trafia do szkoły magii Hogwart. Początek największej sagi fantasy dla młodych czytelników, przetłumaczonej na ponad 80 języków.", price: "45 zł", condition: "Dobry" },
    { id: 11, title: "Fabryka lodów", author: "Camilla Läckberg", type: "loan", genre: "Kryminał", city: "Gdańsk", owner: "nina_lk", av: 8, ownerRating: 4.5, rating: 4, img: 31, isbn: "978-83-7552-344-5", desc: "Erica Falck bada serię tajemniczych zbrodni w szwedzkiej miejscowości. Gęsta atmosfera skandynawskiego noir, pełna zaskakujących zwrotów i psychologicznej głębi.", delivery: "Osobiście" },
    { id: 12, title: "Sapiens. Od zwierząt do bogów", author: "Yuval Noah Harari", type: "sale", genre: "Biografia", city: "Wrocław", owner: "pawel_hr", av: 11, ownerRating: 4.7, rating: 5, img: 32, isbn: "978-83-7785-101-3", desc: "Brawurowa historia ludzkości od pierwszych hominidów do rewolucji cyfrowej. Bestseller, który zmienił sposób rozumienia naszego gatunku i jego roli na Ziemi.", price: "55 zł", condition: "Widoczne zużycie" },
  ];

  // Reaktywny stan aplikacji
  const search = ref('');
  const city = ref('');
  const stateTypes = ref(new Set(['loan', 'swap', 'sale']));
  const stateGenres = ref(new Set());
  const sortBy = ref('newest');
  const viewMode = ref('grid');
  const limit = ref(12);

  // Statystyki dla filtrów
  const typeCounts = computed(() => ({
    loan: BOOKS.filter(b => b.type === 'loan').length,
    swap: BOOKS.filter(b => b.type === 'swap').length,
    sale: BOOKS.filter(b => b.type === 'sale').length,
  }));

  const availableGenres = computed(() => {
    const genres = [...new Set(BOOKS.map(b => b.genre))].sort();
    return genres.map(g => ({
      name: g,
      count: BOOKS.filter(b => b.genre === g).length
    }));
  });

  // Obsługa kliknięć filtrów typu i gatunku
  const toggleType = (type) => {
    if (stateTypes.value.has(type)) stateTypes.value.delete(type);
    else stateTypes.value.add(type);
  };

  const toggleGenre = (genre) => {
    if (stateGenres.value.has(genre)) stateGenres.value.delete(genre);
    else stateGenres.value.add(genre);
  };

  // Reset filtrów
  const resetFilters = () => {
    stateTypes.value = new Set(['loan', 'swap', 'sale']);
    stateGenres.value.clear();
    search.value = '';
    city.value = '';
  };

  // Filtrowanie i sortowanie danych wejściowych
  const filteredBooks = computed(() => {
    let arr = [...BOOKS];

    if (stateTypes.value.size > 0) {
      arr = arr.filter(b => stateTypes.value.has(b.type));
    }
    if (stateGenres.value.size > 0) {
      arr = arr.filter(b => stateGenres.value.has(b.genre));
    }
    if (search.value) {
      const q = search.value.toLowerCase();
      arr = arr.filter(b =>
        b.title.toLowerCase().includes(q) ||
        b.author.toLowerCase().includes(q) ||
        b.isbn.includes(q)
      );
    }
    if (city.value) {
      arr = arr.filter(b => b.city.toLowerCase().includes(city.value.toLowerCase()));
    }

    // Sortowanie
    if (sortBy.value === 'title') arr.sort((a, b) => a.title.localeCompare(b.title));
    if (sortBy.value === 'oldest') arr.sort((a, b) => a.id - b.id);
    if (sortBy.value === 'newest') arr.sort((a, b) => b.id - a.id);

    return arr;
  });

  // Ograniczenie liczby wyświetlanych elementów (Load More)
  const displayedBooks = computed(() => {
    return filteredBooks.value.slice(0, limit.value);
  });

  const openModal = (id) => {
    console.log("Otwieranie modala dla książki o ID:", id);
  };
</script>

<style>
  /* ── GLOBALNE STYLE (ZGODNE Z PROJEKTEM ROOT) ── */
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
    --shadow: 0 2px 8px rgba(44,24,16,.08);
    --shadow-h: 0 4px 16px rgba(44,24,16,.12);
    --r-card: 12px;
    --r-btn: 8px;
    --r-input: 8px;
  }

  body {
    font-family: 'DM Sans', sans-serif;
    background: var(--bg);
    color: var(--text-1);
    -webkit-font-smoothing: antialiased;
  }

  /* Przyciski i struktura strony */
  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 9px 20px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans', sans-serif;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    text-decoration: none;
    border: none;
    white-space: nowrap;
  }

  .btn-outline {
    background: transparent;
    border: 1.5px solid var(--primary);
    color: var(--primary);
  }

    .btn-outline:hover {
      background: var(--primary);
      color: #fff;
    }

  .btn-filled {
    background: var(--primary);
    color: #fff;
    border: 1.5px solid var(--primary);
  }

    .btn-filled:hover {
      background: var(--primary-h);
    }

  .page-wrapper {
    max-width: 1280px;
    margin: 0 auto;
    padding: 32px 48px 80px;
  }

  .page-header {
    display: flex;
    align-items: baseline;
    gap: 14px;
    margin-bottom: 24px;
  }

  .page-title {
    font-family: 'Playfair Display', serif;
    font-size: 32px;
    font-weight: 600;
  }

  .result-count {
    font-size: 14px;
    color: var(--text-3);
  }

    .result-count strong {
      color: var(--text-2);
    }

  .page-layout {
    display: grid;
    grid-template-columns: 260px 1fr;
    gap: 28px;
    align-items: start;
  }

  /* Sort Bar & Grid */
  .sort-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 9px 16px;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    margin-bottom: 22px;
  }

  .sort-left {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .sort-label {
    font-size: 13px;
    color: var(--text-2);
  }

  .sort-select {
    background: var(--bg);
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 5px 10px;
    font-family: 'DM Sans', sans-serif;
    font-size: 13px;
    color: var(--text-1);
    cursor: pointer;
    outline: none;
    transition: border-color .2s;
  }

    .sort-select:focus {
      border-color: var(--primary);
    }

  .view-toggle {
    display: flex;
    gap: 4px;
  }

  .view-btn {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid var(--border);
    border-radius: 6px;
    background: var(--bg);
    color: var(--text-3);
    cursor: pointer;
    transition: all .2s;
  }

    .view-btn.active, .view-btn:hover {
      border-color: var(--primary);
      color: var(--primary);
      background: var(--surface);
    }

  .books-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 18px;
  }

    .books-grid.list {
      grid-template-columns: 1fr;
      gap: 10px;
    }

  .load-more {
    text-align: center;
    margin-top: 40px;
  }

  .empty {
    grid-column: 1 / -1;
    text-align: center;
    padding: 64px 20px;
  }

    .empty svg {
      color: var(--border);
      margin-bottom: 14px;
    }

    .empty p {
      font-size: 15px;
      color: var(--text-3);
    }
</style>
