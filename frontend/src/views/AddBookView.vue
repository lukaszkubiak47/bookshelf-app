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
          <div class="results-label">5 wyników dla „{{ searchQuery }}”</div>
          <div class="results-list">
            <div v-for="b in mockResults" :key="b.id" class="result-row" @click="selectBook(b)">
              <img :src="`https://picsum.photos/52/70?random=${b.img}`" :alt="b.title" class="result-cover">
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
                  <option v-for="g in ['Fantasy', 'Sci-Fi', 'Kryminał', 'Literatura piękna', 'Biografia', 'Poradniki', 'Historia', 'Inne']" :key="g">{{ g }}</option>
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
          <button class="btn btn-filled btn-lg" :disabled="!form.offerType" @click="showSuccess = true">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12" /></svg>
            Dodaj na półkę
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

<script setup>
  import { ref, reactive, onMounted } from 'vue';

  /* ────── STAN APLIKACJI ────── */
  const currentStep = ref(1);
  const searchQuery = ref('Sapkowski');
  const isLoading = ref(false);
  const showResults = ref(false);
  const isManualOpen = ref(false);
  const showSuccess = ref(false);

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
  const manualForm = reactive({ title: '', author: '', genre: 'Literatura piękna', isbn: '', desc: '' });
  const errors = reactive({ title: false, author: false });

  /* Dropzone state */
  const isDragOver = ref(false);
  const previewUrl = ref(null);
  const fileInput = ref(null);

  /* Mockowane dane wyszukiwania */
  const mockResults = [
    { id: 1, title: "Wiedźmin: Ostatnie życzenie", author: "Andrzej Sapkowski", year: "1993", genre: "Fantasy", isbn: "978-83-7578-066-2", desc: "Zbiór opowiadań o wiedźminie Geralcie z Rivii — łowcy potworów.", img: 23 },
    { id: 2, title: "Krew elfów", author: "Andrzej Sapkowski", year: "1994", genre: "Fantasy", isbn: "978-83-7578-068-6", desc: "Pierwsza pełnowymiarowa powieść z cyklu Wiedźmin.", img: 51 },
    { id: 3, title: "Czas pogardy", author: "Andrzej Sapkowski", year: "1995", genre: "Fantasy", isbn: "978-83-7578-069-3", desc: "Ciri ucieka, Geralt walczy — drugi tom sagi.", img: 52 },
    { id: 4, title: "Narrenturm", author: "Andrzej Sapkowski", year: "2002", genre: "Fantasy historyczne", isbn: "978-83-7469-104-6", desc: "Powieść historyczna osadzona w XV-wiecznej Europie.", img: 53 },
    { id: 5, title: "Maladie i inne opowiadania", author: "Andrzej Sapkowski", year: "2012", genre: "Opowiadania", isbn: "978-83-7578-514-8", desc: "Zbiór fantastycznych opowiadań.", img: 54 }
  ];

  /* ────── LOGIKA METOD ────── */
  const runSearch = () => {
    if (!searchQuery.value.trim()) return;
    isLoading.value = true;
    showResults.value = false;
    form.selectedBook = null;

    setTimeout(() => {
      isLoading.value = false;
      showResults.value = true;
    }, 680);
  };

  const selectBook = (book) => {
    form.selectedBook = book;
    showResults.value = false;
  };

  const changeBookSelection = () => {
    form.selectedBook = null;
    showResults.value = true;
  };

  const submitManualForm = () => {
    errors.title = !manualForm.title.trim();
    errors.author = !manualForm.author.trim();
    if (errors.title || errors.author) return;

    form.selectedBook = {
      title: manualForm.title,
      author: manualForm.author,
      year: '—',
      genre: manualForm.genre,
      isbn: manualForm.isbn || '—',
      img: Math.floor(Math.random() * 80) + 20
    };
    isManualOpen.value = false;
    showResults.value = false;
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

  /* Automatyczne wyszukiwanie po załadowaniu strony (1 do 1) */
  onMounted(() => {
    setTimeout(() => { runSearch(); }, 400);
  });
</script>

<style scoped>
  /* ── CONFIG STYLI (ZMIENNE :ROOT) ── */
  :deep(:root), :host {
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
    --warning: #E67E22;
    --success: #27AE60;
    --shadow: 0 2px 8px rgba(44,24,16,.08);
    --shadow-h: 0 4px 16px rgba(44,24,16,.12);
    --r-card: 12px;
    --r-btn: 8px;
    --nav-h: 60px;
  }

  /* W Vue `scoped` aplikuje style tylko do tego komponentu */
  .wizard-outer {
    max-width: 700px;
    margin: 0 auto;
    padding: 40px 24px 80px;
    font-family: 'DM Sans', sans-serif;
  }

  /* ── PROGRESS BAR ── */
  .progress-bar {
    display: flex;
    align-items: center;
    margin-bottom: 36px;
  }

  .pb-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 7px;
    flex-shrink: 0;
  }

  .pb-circle {
    width: 38px;
    height: 38px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid var(--border);
    background: var(--surface);
    color: var(--text-3);
    font-size: 14px;
    font-weight: 600;
    transition: all .35s ease;
  }

  .pb-label {
    font-size: 12px;
    color: var(--text-3);
    font-weight: 500;
    transition: color .3s;
    white-space: nowrap;
  }

  .pb-step.active .pb-circle, .pb-step.done .pb-circle {
    border-color: var(--primary);
    background: var(--primary);
    color: #fff;
  }

  .pb-step.active .pb-label, .pb-step.done .pb-label {
    color: var(--primary);
  }

  .pb-line {
    flex: 1;
    height: 2px;
    background: var(--border);
    margin: 0 16px 22px;
    position: relative;
    overflow: hidden;
    border-radius: 2px;
  }

    .pb-line::after {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      height: 100%;
      background: var(--primary);
      width: 0;
      transition: width .5s ease .1s;
      border-radius: 2px;
    }

    .pb-line.filled::after {
      width: 100%;
    }

  /* ── BUTTONS & FORMS ── */
  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 9px 22px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    border: none;
    white-space: nowrap;
  }

    .btn:disabled {
      opacity: .45;
      cursor: not-allowed;
    }

  .btn-filled {
    background: var(--primary);
    color: #fff;
    border: 1.5px solid var(--primary);
  }

    .btn-filled:hover:not(:disabled) {
      background: var(--primary-h);
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

  .btn-ghost {
    background: none;
    border: none;
    color: var(--primary);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    padding: 2px 0;
    transition: opacity .2s;
  }

    .btn-ghost:hover {
      opacity: .7;
    }

  .btn-sm {
    padding: 6px 14px;
    font-size: 13px;
  }

  .btn-lg {
    padding: 13px 28px;
    font-size: 15px;
  }

  .btn-full {
    width: 100%;
  }

  .form-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
    display: block;
    margin-bottom: 6px;
  }

  .form-input {
    width: 100%;
    padding: 9px 13px;
    border: 1.5px solid var(--border);
    border-radius: 8px;
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    color: var(--text-1);
    background: var(--bg);
    outline: none;
    transition: border-color .2s, background .2s;
  }

    .form-input:focus {
      border-color: var(--primary);
      background: #fff;
    }

    .form-input::placeholder {
      color: var(--text-3);
    }

    .form-input.error {
      border-color: var(--danger);
    }

  .form-error {
    font-size: 11px;
    color: var(--danger);
    margin-top: 4px;
    display: none;
  }

    .form-error.show {
      display: block;
    }

  .form-field {
    margin-bottom: 14px;
  }

  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 14px;
  }

  /* ── WIZARD CARD ── */
  .wizard-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 16px;
    box-shadow: var(--shadow);
    overflow: hidden;
  }

  .wcard-head {
    padding: 28px 32px 0;
  }

  .wcard-title {
    font-family: 'Playfair Display',serif;
    font-size: 24px;
    font-weight: 600;
    color: var(--text-1);
    margin-bottom: 6px;
  }

  .wcard-desc {
    font-size: 14px;
    color: var(--text-2);
    font-weight: 300;
    line-height: 1.65;
  }

  .wcard-body {
    padding: 24px 32px 32px;
  }

  /* ── SEARCH ROW & RESULTS ── */
  .search-row {
    display: flex;
    gap: 10px;
    margin-bottom: 22px;
  }

  .search-wrap {
    position: relative;
    flex: 1;
  }

  .search-icon {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-3);
    pointer-events: none;
    display: flex;
  }

  .search-input {
    width: 100%;
    padding: 11px 14px 11px 38px;
    border: 1.5px solid var(--border);
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 14px;
    color: var(--text-1);
    background: var(--bg);
    outline: none;
    transition: all .2s;
  }

    .search-input:focus {
      border-color: var(--primary);
      background: #fff;
      box-shadow: 0 0 0 3px rgba(45,106,79,.1);
    }

  .results-section {
    margin-bottom: 20px;
  }

  .results-label {
    font-size: 12px;
    color: var(--text-3);
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: .5px;
    margin-bottom: 10px;
  }

  .results-list {
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    overflow: hidden;
  }

  .result-row {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 13px 16px;
    border-bottom: 1px solid var(--border);
    transition: background .15s;
    cursor: pointer;
  }

    .result-row:last-child {
      border-bottom: none;
    }

    .result-row:hover {
      background: var(--tag-bg);
    }

  .result-cover {
    width: 52px;
    height: 70px;
    border-radius: 5px;
    object-fit: cover;
    flex-shrink: 0;
    box-shadow: 0 2px 6px rgba(44,24,16,.12);
  }

  .result-info {
    flex: 1;
    min-width: 0;
  }

  .result-title {
    font-family: 'Playfair Display',serif;
    font-size: 14px;
    font-weight: 500;
    color: var(--text-1);
    margin-bottom: 2px;
  }

  .result-author {
    font-size: 12px;
    color: var(--text-2);
    margin-bottom: 4px;
  }

  .result-meta {
    font-size: 11px;
    color: var(--text-3);
    display: flex;
    gap: 8px;
    align-items: center;
    margin-bottom: 4px;
    flex-wrap: wrap;
  }

  .result-meta-dot {
    width: 3px;
    height: 3px;
    border-radius: 50%;
    background: var(--border);
    flex-shrink: 0;
  }

  .result-desc {
    font-size: 12px;
    color: var(--text-2);
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
  }

  .pick-btn {
    padding: 6px 14px;
    border-radius: var(--r-btn);
    border: 1.5px solid var(--primary);
    background: transparent;
    color: var(--primary);
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
  }

    .pick-btn:hover, .result-row:hover .pick-btn {
      background: var(--primary);
      color: #fff;
    }

  /* Skeleton Loader */
  .skel-row {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 14px 16px;
    border-bottom: 1px solid var(--border);
  }

    .skel-row:last-child {
      border-bottom: none;
    }

  .skel {
    background: linear-gradient(90deg, var(--border) 25%, var(--tag-bg) 50%, var(--border) 75%);
    background-size: 200% 100%;
    animation: shimmer 1.4s ease-in-out infinite;
    border-radius: 4px;
  }

  @keyframes shimmer {
    0% {
      background-position: 200% 0
    }

    100% {
      background-position: -200% 0
    }
  }

  .skel-cover {
    width: 52px;
    height: 70px;
    border-radius: 5px;
    flex-shrink: 0;
  }

  .skel-body {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .skel-line {
    height: 12px;
  }

  /* Selected book card */
  .selected-card {
    border: 2px solid var(--primary);
    border-radius: var(--r-card);
    background: #f1f8f4;
    padding: 18px 20px;
    display: flex;
    align-items: center;
    gap: 18px;
    margin-bottom: 20px;
  }

  .sel-check {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: var(--primary);
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    color: white;
  }

  .sel-cover {
    width: 72px;
    height: 96px;
    border-radius: 6px;
    object-fit: cover;
    flex-shrink: 0;
    box-shadow: 0 4px 12px rgba(44,24,16,.16);
  }

  .sel-info {
    flex: 1;
    min-width: 0;
  }

  .sel-title {
    font-family: 'Playfair Display',serif;
    font-size: 16px;
    font-weight: 600;
    color: var(--text-1);
    margin-bottom: 3px;
  }

  .sel-author {
    font-size: 13px;
    color: var(--text-2);
    margin-bottom: 6px;
  }

  .sel-meta {
    font-size: 12px;
    color: var(--text-3);
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
  }

  .sel-isbn {
    font-size: 11px;
    color: var(--text-3);
    margin-top: 4px;
  }

  /* Manual collapse form */
  .manual-toggle {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 0;
  }

  .manual-toggle-btn {
    background: none;
    border: none;
    color: var(--primary);
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: opacity .2s;
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 0;
  }

    .manual-toggle-btn:hover {
      opacity: .7;
    }

  .toggle-arrow {
    display: inline-block;
    transition: transform .3s;
    font-size: 10px;
    color: var(--text-3);
  }

  .manual-toggle-btn.open .toggle-arrow {
    transform: rotate(90deg);
  }

  .manual-collapse {
    max-height: 0;
    overflow: hidden;
    transition: max-height .4s ease;
  }

    .manual-collapse.open {
      max-height: 600px;
    }

  .manual-inner {
    padding-top: 20px;
  }

  .manual-divider {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 20px;
  }

    .manual-divider span {
      font-size: 11px;
      color: var(--text-3);
      white-space: nowrap;
    }

    .manual-divider::before, .manual-divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: var(--border);
    }

  /* ── STEP 2 (KROK 2 STYLI) ── */
  .book-recap {
    display: flex;
    align-items: center;
    gap: 14px;
    background: var(--tag-bg);
    border-radius: var(--r-card);
    padding: 14px 18px;
    margin-bottom: 28px;
  }

  .recap-cover {
    width: 48px;
    height: 64px;
    border-radius: 5px;
    object-fit: cover;
    box-shadow: 0 2px 8px rgba(44,24,16,.14);
    flex-shrink: 0;
  }

  .recap-info {
    flex: 1;
    min-width: 0;
  }

  .recap-title {
    font-family: 'Playfair Display',serif;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-1);
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .recap-author {
    font-size: 12px;
    color: var(--text-2);
  }

  .recap-change {
    font-size: 12px;
    color: var(--primary);
    font-weight: 500;
    cursor: pointer;
    text-decoration: none;
    white-space: nowrap;
    flex-shrink: 0;
    transition: opacity .2s;
  }

    .recap-change:hover {
      opacity: .7;
    }

  .offer-type-heading {
    font-size: 14px;
    font-weight: 600;
    color: var(--text-1);
    margin-bottom: 14px;
  }

  .offer-grid {
    display: grid;
    grid-template-columns: repeat(3,1fr);
    gap: 12px;
    margin-bottom: 22px;
  }

  .offer-card {
    border: 2px solid var(--border);
    border-radius: var(--r-card);
    padding: 20px 14px;
    text-align: center;
    cursor: pointer;
    transition: all .2s;
    background: var(--surface);
  }

    .offer-card:hover {
      border-color: var(--text-2);
      transform: translateY(-1px);
    }

  .offer-icon {
    width: 44px;
    height: 44px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 12px;
    transition: background .2s;
  }

  .offer-card h4 {
    font-family: 'Playfair Display',serif;
    font-size: 15px;
    font-weight: 600;
    color: var(--text-1);
    margin-bottom: 4px;
  }

  .offer-card p {
    font-size: 11px;
    color: var(--text-3);
    line-height: 1.5;
  }

  .offer-card.sel-loan {
    border-color: var(--primary);
    background: #f0f8f4;
  }

    .offer-card.sel-loan .offer-icon {
      background: rgba(45,106,79,.12);
    }

  .offer-card.sel-swap {
    border-color: var(--accent);
    background: #fdf8ea;
  }

    .offer-card.sel-swap .offer-icon {
      background: rgba(212,168,67,.15);
    }

  .offer-card.sel-sale {
    border-color: var(--blue);
    background: #eef3f9;
  }

    .offer-card.sel-sale .offer-icon {
      background: rgba(44,95,138,.12);
    }

  .offer-card.sel-loan .offer-icon svg {
    stroke: var(--primary) !important;
  }

  .offer-card.sel-swap .offer-icon svg {
    stroke: var(--accent) !important;
  }

  .offer-card.sel-sale .offer-icon svg {
    stroke: var(--blue) !important;
  }

  /* Conditional fields open/close code style 1 do 1 */
  .cond {
    max-height: 0;
    overflow: hidden;
    opacity: 0;
    transition: max-height .35s ease, opacity .28s ease;
  }

    .cond.open {
      max-height: 320px;
      opacity: 1;
    }

  .cond-inner {
    padding-top: 2px;
    padding-bottom: 4px;
  }

  .price-wrap {
    position: relative;
  }

  .price-icon {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-3);
    font-size: 14px;
    font-weight: 500;
    pointer-events: none;
  }

  .price-input {
    padding-left: 32px;
  }

  /* Dropzone style */
  .dz-section {
    margin-top: 22px;
  }

  .dz-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
    margin-bottom: 8px;
    display: block;
  }

  .dropzone {
    border: 2px dashed var(--border);
    border-radius: var(--r-card);
    background: var(--bg);
    padding: 28px 20px;
    text-align: center;
    cursor: pointer;
    transition: all .2s;
    position: relative;
    overflow: hidden;
  }

    .dropzone:hover, .dropzone.drag-over {
      border-color: var(--primary);
      background: #f4fbf7;
    }

  .dz-inner svg {
    color: var(--text-3);
    margin-bottom: 10px;
  }

  .dz-inner p {
    font-size: 13px;
    color: var(--text-2);
    margin-bottom: 4px;
  }

  .dz-inner span {
    font-size: 11px;
    color: var(--text-3);
  }

  .dz-link {
    color: var(--primary);
    font-weight: 500;
  }

  .dz-preview {
    display: none;
    position: relative;
  }

    .dz-preview img {
      height: 120px;
      width: auto;
      border-radius: 6px;
      box-shadow: var(--shadow-h);
    }

  .dz-remove {
    position: absolute;
    top: -8px;
    right: calc(50% - 40px);
    background: var(--danger);
    color: white;
    border: none;
    width: 22px;
    height: 22px;
    border-radius: 50%;
    font-size: 14px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .step-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-top: 20px;
    border-top: 1px solid var(--border);
    margin-top: 8px;
  }

  /* Animations */
  @keyframes fadeIn {
    from {
      opacity: 0;
      transform: translateY(8px);
    }

    to {
      opacity: 1;
      transform: none;
    }
  }

  .animate-in {
    animation: fadeIn .35s ease forwards;
  }

  /* ── SUCCESS OVERLAY ── */
  .success-overlay {
    position: fixed;
    inset: 0;
    background: rgba(44,24,16,.45);
    backdrop-filter: blur(4px);
    z-index: 900;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    pointer-events: none;
    transition: opacity .25s;
  }

    .success-overlay.open {
      opacity: 1;
      pointer-events: all;
    }

  .success-box {
    background: var(--surface);
    border-radius: 16px;
    padding: 40px 36px;
    text-align: center;
    max-width: 360px;
    width: 100%;
    box-shadow: 0 24px 60px rgba(44,24,16,.22);
    transform: scale(.93);
    transition: transform .28s;
  }

  .success-overlay.open .success-box {
    transform: scale(1);
  }

  .success-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: #eafaf1;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 20px;
  }

  .success-box h3 {
    font-family: 'Playfair Display',serif;
    font-size: 22px;
    font-weight: 600;
    margin-bottom: 8px;
    color: var(--text-1);
  }

  .success-box p {
    font-size: 14px;
    color: var(--text-2);
    margin-bottom: 24px;
  }
</style>
