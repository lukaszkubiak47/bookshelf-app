<script setup>
  import { ref, reactive, onMounted, watch } from 'vue';
  import { booksApi } from '../lib/books';
  import { userBooksApi } from '../lib/userBooks';
  import { mapBookSearchResult, seedFromGuid, coverColor } from '../lib/mappers';
  import { ApiError } from '../lib/client';

  /* ────── STAN APLIKACJI ────── */
  const currentStep = ref(1);
  const searchQuery = ref('');
  const isLoading = ref(false);
  const showResults = ref(false);
  const isManualOpen = ref(false);
  const showSuccess = ref(false);
  const searchResults = ref([]);
  const submitError = ref('');
  const isPublishing = ref(false);

  // Okładki wyników, które nie załadują się (np. blokada Google na localhost) -> kafelek zastępczy.
  const failedCovers = ref(new Set());
  const markCoverFailed = (id) => { failedCovers.value = new Set(failedCovers.value).add(id); };
  const coverOk = (b) => b.coverUrl && !failedCovers.value.has(b.googleBooksId);

  const form = reactive({
    selectedBook: null,
    offerType: null,
    customCoverFile: null,
    offerDetails: {
      maxLoanTime: '1 miesiąc',
      deliveryMethod: 'Osobiście',
      swapLooking: 'any',
      swapBookTitle: '',
      swapGenre: 'Fantasy',
      price: null,
      condition: 'Dobry'
    }
  });

  /* Formularz ręczny i walidacja */
  const genres = ref([]);
  const manualForm = reactive({ title: '', author: '', genre: '', isbn: '', desc: '' });
  const errors = reactive({ title: false, author: false });

  /* Dropzone state */
  const isDragOver = ref(false);
  const previewUrl = ref(null);
  const fileInput = ref(null);

  /* ────── LOGIKA METOD ────── */
  const runSearch = async () => {
    if (!searchQuery.value.trim()) return;
    isLoading.value = true;
    showResults.value = false;
    form.selectedBook = null;
    submitError.value = '';

    try {
      const results = await booksApi.search(searchQuery.value.trim());
      searchResults.value = results.map(mapBookSearchResult);
      showResults.value = true;
    } catch {
      searchResults.value = [];
      submitError.value = 'Wyszukiwanie nie powiodło się';
      showResults.value = true;
    } finally {
      isLoading.value = false;
    }
  };

  // Zmiana zapytania chowa stare wyniki, żeby nie wisiał "duch" poprzedniego wyszukiwania.
  watch(searchQuery, () => {
    searchResults.value = [];
    showResults.value = false;
    submitError.value = '';
  });

  // Wynik Google: stage'ujemy do wspólnego kształtu. genreId puste — użytkownik wybierze gatunek w kroku 2.
  const selectBook = (book) => {
    form.selectedBook = {
      bookId: null,
      googleBooksId: book.googleBooksId,
      title: book.title,
      author: book.author,
      description: book.desc || null,
      isbn: book.isbn && book.isbn !== '—' ? book.isbn : null,
      coverUrl: book.coverUrl || null,
      genreId: '',
      // pola wyłącznie do podglądu w kreatorze:
      year: book.year,
      genre: book.genre,
      img: book.img,
    };
    showResults.value = false;
  };

  const changeBookSelection = () => {
    form.selectedBook = null;
    showResults.value = true;
  };

  // Ręczne dane: tylko stage'ujemy lokalnie. Zapis do bazy (POST /api/books) następuje przy publikacji oferty.
  const submitManualForm = () => {
    errors.title = !manualForm.title.trim();
    errors.author = !manualForm.author.trim();
    if (errors.title || errors.author) return;
    if (!manualForm.genre) {
      submitError.value = 'Wybierz gatunek książki.';
      return;
    }

    submitError.value = '';
    form.selectedBook = {
      bookId: null,                 // jeszcze nie zapisana w bazie
      googleBooksId: null,
      title: manualForm.title.trim(),
      author: manualForm.author.trim(),
      description: manualForm.desc.trim() || null,
      isbn: manualForm.isbn.trim() || null,
      genreId: manualForm.genre,    // Guid z selecta
      // pola wyłącznie do podglądu w kreatorze:
      year: '—',
      genre: genres.value.find(g => g.id === manualForm.genre)?.name ?? 'Inne',
      img: seedFromGuid(manualForm.title.trim() || 'book', 80),
    };
    isManualOpen.value = false;
    showResults.value = false;
  };

  const swapConditionMap = {
    any: 'anything',
    book: 'specific_book',
    genre: 'genre',
    free: 'free',
  };

  // Zapewnia bookId z bazy: jeśli książka nie była jeszcze zapisana, tworzy ją teraz (POST /api/books).
  async function ensureBookId() {
    const sb = form.selectedBook;
    if (sb.bookId) return sb.bookId;
    const created = await booksApi.create({
      title: sb.title,
      author: sb.author,
      description: sb.description || undefined,
      isbn: sb.isbn || undefined,
      genreId: sb.genreId,
      googleBooksId: sb.googleBooksId || undefined,
      coverUrl: sb.coverUrl || undefined,
    });
    sb.bookId = created.id;        // zapamiętujemy, by ponowna próba nie utworzyła duplikatu
    return created.id;
  }

  const publishOffer = async () => {
    if (!form.selectedBook || !form.offerType || isPublishing.value) return;

    // POST /api/books wymaga genreId. Dla wyniku Google wybiera się go w kroku 2 (poniższy <select>).
    if (!form.selectedBook.bookId && !form.selectedBook.genreId) {
      submitError.value = 'Wybierz gatunek książki.';
      return;
    }

    // Cena: wymagana i dodatnia tylko dla sprzedaży (backend odrzuca sprzedaż bez ceny).
    if (form.offerType === 'sale') {
      const price = Number(form.offerDetails.price);
      if (!price || price <= 0) {
        submitError.value = 'Podaj cenę sprzedaży (większą od 0).';
        return;
      }
    }

    submitError.value = '';
    isPublishing.value = true;
    try {
      const bookId = await ensureBookId();

      const body = {
        bookId,
        transactionType: form.offerType,
      };

      if (form.offerType === 'sale') {
        body.price = Number(form.offerDetails.price);
      }

      if (form.offerType === 'swap') {
        body.exchangeCondition = swapConditionMap[form.offerDetails.swapLooking] ?? 'anything';
        if (form.offerDetails.swapLooking === 'book') {
          body.exchangeDetails = form.offerDetails.swapBookTitle || undefined;
        } else if (form.offerDetails.swapLooking === 'genre') {
          body.exchangeDetails = form.offerDetails.swapGenre;
        }
      }

      await userBooksApi.create(body);
      showSuccess.value = true;
    } catch (err) {
      submitError.value = err instanceof ApiError ? err.message : 'Nie udało się dodać oferty';
    } finally {
      isPublishing.value = false;
    }
  };

  /* Dropzone logic */
  const triggerFileInput = () => { if (!previewUrl.value) fileInput.value.click(); };
  const handleFileChange = (e) => { processFile(e.target.files[0]); };
  const handleDrop = (e) => { isDragOver.value = false; processFile(e.dataTransfer.files[0]); };

  const processFile = (file) => {
    if (file && file.type.startsWith('image/')) {
      form.customCoverFile = file;
      const reader = new FileReader();
      reader.onload = (e) => { previewUrl.value = e.target.result; };
      reader.readAsDataURL(file);
    }
  };

  const removeCover = () => {
    previewUrl.value = null;
    form.customCoverFile = null;
    if (fileInput.value) fileInput.value.value = '';
  };

  /* Ładowanie gatunków + automatyczne wyszukiwanie po załadowaniu strony */
  onMounted(async () => {
    try {
      genres.value = await booksApi.getGenres();
      // Domyślnie pierwszy gatunek, żeby pole nie było puste przy wysyłce.
      if (genres.value.length && !manualForm.genre) manualForm.genre = genres.value[0].id;
    } catch {
      genres.value = [];
    }
    if (searchQuery.value.trim()) runSearch();
  });
</script>

<template>
  <div class="wizard-outer">

    <div class="progress-bar">
      <div class="pb-step" :class="{ active: currentStep === 1, done: currentStep > 1 }">
        <div class="pb-circle">
          <svg v-if="currentStep > 1" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.8" stroke-linecap="round">
            <polyline points="20 6 9 17 4 12" />
          </svg>
          <template v-else>
            1
          </template>
        </div>
        <div class="pb-label">Znajdź książkę</div>
      </div>
      <div class="pb-line" :class="{ filled: currentStep > 1 }"></div>
      <div class="pb-step" :class="{ active: currentStep === 2 }">
        <div class="pb-circle">2</div>
        <div class="pb-label">Skonfiguruj ofertę</div>
      </div>
    </div>

    <div v-if="currentStep === 1" class="wizard-card animate-in">
      <div class="wcard-head">
        <h2 class="wcard-title">Znajdź swoją książkę</h2>
        <p class="wcard-desc">Wyszukaj książkę, którą chcesz wystawić. Pobierzemy dane automatycznie.</p>
      </div>
      <div class="wcard-body">

        <div class="search-row">
          <div class="search-wrap">
            <span class="search-icon"><svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><circle cx="11" cy="11" r="7" /><line x1="16.5" y1="16.5" x2="22" y2="22" /></svg></span>
            <input type="text" class="search-input" v-model="searchQuery" @keydown.enter="runSearch" placeholder="Wpisz tytuł, autora lub ISBN…">
          </div>
          <button class="btn btn-filled" @click="runSearch" :disabled="isLoading" style="flex-shrink:0">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><circle cx="11" cy="11" r="7" /><line x1="16.5" y1="16.5" x2="22" y2="22" /></svg>
            {{ isLoading ? 'Szukam…' : 'Szukaj' }}
          </button>
        </div>

        <div v-if="showResults && !isLoading" class="results-section">
          <div class="results-label">{{ searchResults.length }} wyników dla „{{ searchQuery }}”</div>
          <div class="results-list">
            <div v-for="b in searchResults" :key="b.googleBooksId" class="result-row" @click="selectBook(b)">
              <img v-if="coverOk(b)" :src="b.coverUrl" :alt="b.title" class="result-cover" @error="markCoverFailed(b.googleBooksId)">
              <div v-else class="result-cover result-cover-fallback" :style="{ background: coverColor(b.title) }">
                <span class="rcf-title">{{ b.title }}</span>
                <span class="rcf-author">{{ b.author }}</span>
              </div>
              <div class="result-info">
                <div class="result-title">{{ b.title }}</div>
                <div class="result-author">{{ b.author }}</div>
                <div class="result-meta">
                  <span>{{ b.year }}</span>
                  <span class="result-meta-dot"></span>
                  <span>{{ b.genre }}</span>
                  <span class="result-meta-dot"></span>
                  <span>ISBN {{ b.isbn }}</span>
                </div>
                <div class="result-desc">{{ b.desc }}</div>
              </div>
              <div class="result-action">
                <button class="pick-btn" @click.stop="selectBook(b)">Wybierz</button>
              </div>
            </div>
          </div>
        </div>

        <div v-if="isLoading" class="results-section">
          <div class="results-label">Szukam…</div>
          <div class="results-list">
            <div v-for="i in 4" :key="i" class="skel-row">
              <div class="skel skel-cover"></div>
              <div class="skel-body">
                <div class="skel skel-line" style="width:65%"></div>
                <div class="skel skel-line" style="width:40%"></div>
                <div class="skel skel-line" style="width:80%;height:10px"></div>
              </div>
            </div>
          </div>
        </div>

        <div v-if="form.selectedBook && !showResults" class="selected-card animate-in">
          <div class="sel-check">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
          </div>
          <img :src="`https://picsum.photos/72/96?random=${form.selectedBook.img}`" alt="" class="sel-cover">
          <div class="sel-info">
            <div class="sel-title">{{ form.selectedBook.title }}</div>
            <div class="sel-author">{{ form.selectedBook.author }}</div>
            <div class="sel-meta">
              <span>{{ form.selectedBook.year }}</span>
              <span style="color:var(--border)">·</span>
              <span>{{ form.selectedBook.genre }}</span>
            </div>
            <div class="sel-isbn">ISBN: {{ form.selectedBook.isbn }}</div>
          </div>
          <button class="btn-ghost" @click="changeBookSelection" style="flex-shrink:0">Zmień wybór</button>
        </div>

        <div class="manual-toggle">
          <button class="manual-toggle-btn" :class="{ open: isManualOpen }" @click="isManualOpen = !isManualOpen">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" /></svg>
            Nie mogę znaleźć książki — dodaj ręcznie
            <span class="toggle-arrow">▶</span>
          </button>
        </div>

        <div class="manual-collapse" :class="{ open: isManualOpen }">
          <div class="manual-inner">
            <div class="manual-divider"><span>Formularz ręczny</span></div>
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Tytuł *</label>
                <input type="text" class="form-input" :class="{ error: errors.title }" v-model="manualForm.title" placeholder="Wpisz tytuł książki">
                <div class="form-error" :class="{ show: errors.title }">Pole wymagane</div>
              </div>
              <div class="form-field">
                <label class="form-label">Autor *</label>
                <input type="text" class="form-input" :class="{ error: errors.author }" v-model="manualForm.author" placeholder="Imię i nazwisko">
                <div class="form-error" :class="{ show: errors.author }">Pole wymagane</div>
              </div>
            </div>
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Gatunek</label>
                <select class="form-input" v-model="manualForm.genre">
                  <option value="" disabled>Wybierz gatunek…</option>
                  <option v-for="g in genres" :key="g.id" :value="g.id">{{ g.name }}</option>
                </select>
              </div>
              <div class="form-field">
                <label class="form-label">ISBN</label>
                <input type="text" class="form-input" v-model="manualForm.isbn" placeholder="np. 978-83-…">
              </div>
            </div>
            <div class="form-field">
              <label class="form-label">Opis (opcjonalnie)</label>
              <textarea class="form-input" v-model="manualForm.desc" rows="3" placeholder="Krótki opis książki…" style="resize:vertical"></textarea>
            </div>
            <button class="btn btn-filled btn-full" @click="submitManualForm">Użyj tych danych →</button>
          </div>
        </div>

        <p v-if="submitError" style="color:var(--danger);font-size:13px;margin:0 0 10px">{{ submitError }}</p>

        <div class="step-footer" style="margin-top:24px">
          <a href="#" style="font-size:13px;color:var(--text-3);text-decoration:none">Anuluj</a>
          <button class="btn btn-filled btn-lg" :disabled="!form.selectedBook" @click="currentStep = 2">
            Dalej
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><line x1="5" y1="12" x2="19" y2="12" /><polyline points="12 5 19 12 12 19" /></svg>
          </button>
        </div>
      </div>
    </div>

    <div v-if="currentStep === 2" class="wizard-card animate-in">
      <div class="wcard-head">
        <h2 class="wcard-title">Jak chcesz udostępnić tę książkę?</h2>
        <p class="wcard-desc">Wybierz typ oferty i uzupełnij szczegóły. Możesz to zmienić później.</p>
      </div>
      <div class="wcard-body">

        <div class="book-recap">
          <img :src="`https://picsum.photos/48/64?random=${form.selectedBook.img}`" alt="" class="recap-cover">
          <div class="recap-info">
            <div class="recap-title">{{ form.selectedBook.title }}</div>
            <div class="recap-author">{{ form.selectedBook.author }}</div>
          </div>
          <a href="#" class="recap-change" @click.prevent="currentStep = 1">← Zmień</a>
        </div>

        <div v-if="form.selectedBook?.googleBooksId" class="form-field" style="margin-bottom:18px">
          <label class="form-label">Gatunek książki</label>
          <select class="form-input" v-model="form.selectedBook.genreId">
            <option value="" disabled>Wybierz gatunek…</option>
            <option v-for="g in genres" :key="g.id" :value="g.id">{{ g.name }}</option>
          </select>
        </div>

        <div class="offer-type-heading">Wybierz typ oferty</div>
        <div class="offer-grid">
          <div class="offer-card" :class="{ 'sel-loan': form.offerType === 'loan' }" @click="form.offerType = 'loan'">
            <div class="offer-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="var(--text-3)" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></svg></div>
            <h4>Wypożyczę</h4>
            <p>Udostępnij na czas określony i odzyskaj książkę</p>
          </div>

          <div class="offer-card" :class="{ 'sel-swap': form.offerType === 'swap' }" @click="form.offerType = 'swap'">
            <div class="offer-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="var(--text-3)" stroke-width="2"><polyline points="17 1 21 5 17 9" /><path d="M3 11V9a4 4 0 0 1 4-4h14" /><polyline points="7 23 3 19 7 15" /><path d="M21 13v2a4 4 0 0 1-4 4H3" /></svg></div>
            <h4>Wymienię</h4>
            <p>Zamień się na inną książkę z kimś ze społeczności</p>
          </div>

          <div class="offer-card" :class="{ 'sel-sale': form.offerType === 'sale' }" @click="form.offerType = 'sale'">
            <div class="offer-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="var(--text-3)" stroke-width="2"><path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" /><line x1="7" y1="7" x2="7.01" y2="7" /></svg></div>
            <h4>Sprzedam</h4>
            <p>Ustal cenę i znajdź kupca wśród czytelników</p>
          </div>
        </div>

        <div class="cond" :class="{ open: form.offerType === 'loan' }">
          <div class="cond-inner">
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Maksymalny czas wypożyczenia</label>
                <select class="form-input" v-model="form.offerDetails.maxLoanTime">
                  <option v-for="t in ['Do uzgodnienia', '1 tydzień', '2 tygodnie', '1 miesiąc', '2 miesiące', '3 miesiące']" :key="t">{{ t }}</option>
                </select>
              </div>
              <div class="form-field">
                <label class="form-label">Sposób przekazania</label>
                <select class="form-input" v-model="form.offerDetails.deliveryMethod">
                  <option>Osobiście</option>
                  <option>Pocztą (na koszt pożyczkobiorcy)</option>
                  <option>Do uzgodnienia</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <div class="cond" :class="{ open: form.offerType === 'swap' }">
          <div class="cond-inner">
            <div class="form-field">
              <label class="form-label">Szukam w zamian</label>
              <select class="form-input" v-model="form.offerDetails.swapLooking">
                <option value="any">Cokolwiek pasującego</option>
                <option value="book">Konkretna książka</option>
                <option value="genre">Książki danego gatunku</option>
                <option value="free">Oddam za darmo</option>
              </select>
            </div>
            <div v-if="form.offerDetails.swapLooking === 'book'" class="form-field animate-in">
              <label class="form-label">Tytuł szukanej książki</label>
              <input type="text" class="form-input" v-model="form.offerDetails.swapBookTitle" placeholder="np. Ogniem i mieczem, Sienkiewicz…">
            </div>
            <div v-if="form.offerDetails.swapLooking === 'genre'" class="form-field animate-in">
              <label class="form-label">Szukany gatunek</label>
              <select class="form-input" v-model="form.offerDetails.swapGenre">
                <option v-for="g in ['Fantasy', 'Sci-Fi', 'Kryminał', 'Literatura piękna', 'Biografia', 'Historia']" :key="g">{{ g }}</option>
              </select>
            </div>
          </div>
        </div>

        <div class="cond" :class="{ open: form.offerType === 'sale' }">
          <div class="cond-inner">
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Cena (zł)</label>
                <div class="price-wrap">
                  <span class="price-icon">zł</span>
                  <input type="number" class="form-input price-input" v-model="form.offerDetails.price" placeholder="np. 35" min="1">
                </div>
              </div>
              <div class="form-field">
                <label class="form-label">Stan książki</label>
                <select class="form-input" v-model="form.offerDetails.condition">
                  <option>Jak nowa</option>
                  <option>Dobry</option>
                  <option>Widoczne zużycie</option>
                  <option>Do naprawy</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <div class="dz-section">
          <label class="dz-label">Własna okładka (opcjonalnie)</label>
          <div class="dropzone" :class="{ 'drag-over': isDragOver }" @click="triggerFileInput" @dragover.prevent="isDragOver = true" @dragleave="isDragOver = false" @drop.prevent="handleDrop">
            <input type="file" ref="fileInput" accept="image/*" style="display:none" @change="handleFileChange">

            <div v-if="!previewUrl" class="dz-inner">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" /><polyline points="17 8 12 3 7 8" /><line x1="12" y1="3" x2="12" y2="15" /></svg>
              <p>Przeciągnij okładkę lub <span class="dz-link">kliknij aby wybrać</span></p>
              <span>JPG, PNG · maks. 5 MB</span>
            </div>
            <div v-else class="dz-preview" style="display:block">
              <button class="dz-remove" @click.stop="removeCover">×</button>
              <img :src="previewUrl" alt="Podgląd okładki">
            </div>
          </div>
        </div>

        <div class="step-footer" style="margin-top:28px">
          <button class="btn btn-outline" @click="currentStep = 1">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><line x1="19" y1="12" x2="5" y2="12" /><polyline points="12 19 5 12 12 5" /></svg>
            Wróć
          </button>
          <p v-if="submitError" style="color:var(--danger);font-size:13px;margin-bottom:12px">{{ submitError }}</p>
          <button class="btn btn-filled btn-lg" :disabled="!form.offerType || isPublishing" @click="publishOffer">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12" /></svg>
            {{ isPublishing ? 'Dodawanie…' : 'Dodaj na półkę' }}
          </button>
        </div>
      </div>
    </div>
  </div>

  <div class="success-overlay" :class="{ open: showSuccess }">
    <div class="success-box">
      <div class="success-icon">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="var(--success)" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
      </div>
      <h3>Książka dodana!</h3>
      <p>Twoja oferta jest już widoczna dla innych czytelników.</p>
      <div style="display:flex;gap:10px">
        <a href="/polka" class="btn btn-outline btn-full btn-sm">Moja półka</a>
        <a href="/przegladanie" class="btn btn-filled btn-full btn-sm">Przeglądaj</a>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .result-cover-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 2px;
    padding: 4px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
    overflow: hidden;
  }

  .result-cover-fallback .rcf-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 9px;
    line-height: 1.1;
  }

  .result-cover-fallback .rcf-author {
    font-size: 8px;
    opacity: 0.8;
  }
</style>
