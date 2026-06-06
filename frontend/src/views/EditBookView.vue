<script setup>
  /**
   * Trasa: /polka/edytuj/:id (name: editbook) — wymaga zalogowania (meta.requiresAuth).
   * Cel: edycja własnej wystawionej oferty — typ, warunki, opis, okładka.
   * API: userBooksApi.getById, userBooksApi.update/uploadCover/removeCover.
   * Komponenty: brak — formularz w całości w tym widoku.
   * Dostęp tylko dla właściciela oferty (sprawdzenie owner.id).
   */
  import { ref, reactive, onMounted } from 'vue';
  import { useRoute } from 'vue-router';
  import { userBooksApi } from '../lib/userBooks';
  import { realCoverUrl, coverColor } from '../lib/mappers';
  import { ApiError, resolveApiUrl } from '../lib/client';
  import { useAuthStore } from '../stores/auth';

  const route = useRoute();
  const auth = useAuthStore();

  const offerId = ref('');
  const isLoadingOffer = ref(true);
  const loadError = ref('');
  const showSuccess = ref(false);
  const submitError = ref('');
  const isSaving = ref(false);
  const bookDescription = ref('');
  const customCoverFile = ref(null);
  const hadCustomCover = ref(false);
  const coverRemoved = ref(false);
  const isDragOver = ref(false);
  const previewUrl = ref(null);
  const fileInput = ref(null);
  const recapCoverFailed = ref(false);

  const form = reactive({
    selectedBook: null,
    offerType: null,
    offerDetails: {
      maxLoanTime: '1 miesiąc',
      deliveryMethod: 'Osobiście',
      swapLooking: 'any',
      swapBookTitle: '',
      swapGenre: 'Fantasy',
      price: null,
      condition: 'Dobry',
    },
  });

  const swapConditionMap = {
    any: 'anything',
    book: 'specific_book',
    genre: 'genre',
    free: 'free',
  };

  const swapConditionReverse = {
    anything: 'any',
    specific_book: 'book',
    genre: 'genre',
    free: 'free',
  };

  function fillFormFromOffer(dto) {
    offerId.value = dto.id;
    form.selectedBook = {
      bookId: dto.book.id,
      title: dto.book.title,
      author: dto.book.author,
      description: dto.book.description || null,
      isbn: dto.book.isbn || null,
      coverUrl: realCoverUrl(dto.book.coverUrl),
      genre: dto.book.genreName ?? 'Inne',
      year: '—',
    };
    recapCoverFailed.value = false;
    form.offerType = dto.transactionType;
    if (dto.transactionType === 'sale' && dto.price != null) {
      form.offerDetails.price = dto.price;
    }
    if (dto.transactionType === 'swap' && dto.exchangeCondition) {
      form.offerDetails.swapLooking = swapConditionReverse[dto.exchangeCondition] ?? 'any';
      if (form.offerDetails.swapLooking === 'book') {
        form.offerDetails.swapBookTitle = dto.exchangeDetails ?? '';
      } else if (form.offerDetails.swapLooking === 'genre') {
        form.offerDetails.swapGenre = dto.exchangeDetails ?? 'Fantasy';
      }
    }
    bookDescription.value = dto.book.description ?? '';
    hadCustomCover.value = dto.hasCustomCover;
    if (dto.hasCustomCover) {
      previewUrl.value = resolveApiUrl(`/api/userbooks/${dto.id}/cover/image`);
    }
  }

  const triggerFileInput = () => { fileInput.value?.click(); };
  const handleFileChange = (e) => { processFile(e.target.files[0]); };
  const handleDrop = (e) => { isDragOver.value = false; processFile(e.dataTransfer.files[0]); };

  const processFile = (file) => {
    if (file && file.type.startsWith('image/')) {
      customCoverFile.value = file;
      coverRemoved.value = false;
      const reader = new FileReader();
      reader.onload = (ev) => { previewUrl.value = ev.target.result; };
      reader.readAsDataURL(file);
    }
  };

  const removeCover = () => {
    previewUrl.value = null;
    customCoverFile.value = null;
    if (fileInput.value) fileInput.value.value = '';
    if (hadCustomCover.value) coverRemoved.value = true;
  };

  onMounted(async () => {
    const id = route.params.id;
    if (!id || typeof id !== 'string') {
      loadError.value = 'Nieprawidłowy identyfikator oferty.';
      isLoadingOffer.value = false;
      return;
    }

    try {
      const dto = await userBooksApi.getById(id);
      if (!auth.user || dto.owner.id !== auth.user.id) {
        loadError.value = 'Możesz edytować tylko swoje oferty.';
        return;
      }
      fillFormFromOffer(dto);
    } catch (err) {
      loadError.value = err instanceof ApiError ? err.message : 'Nie udało się załadować oferty';
    } finally {
      isLoadingOffer.value = false;
    }
  });

  const saveOffer = async () => {
    if (!form.selectedBook || !form.offerType || !offerId.value || isSaving.value) return;

    if (form.offerType === 'sale') {
      const price = Number(form.offerDetails.price);
      if (!price || price <= 0) {
        submitError.value = 'Podaj cenę sprzedaży (większą od 0).';
        return;
      }
    }

    submitError.value = '';
    isSaving.value = true;
    try {
      const body = {
        transactionType: form.offerType,
        bookDescription: bookDescription.value.trim(),
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

      await userBooksApi.update(offerId.value, body);

      if (customCoverFile.value) {
        await userBooksApi.uploadCover(offerId.value, customCoverFile.value);
      } else if (coverRemoved.value) {
        await userBooksApi.removeCover(offerId.value);
      }

      showSuccess.value = true;
    } catch (err) {
      submitError.value = err instanceof ApiError ? err.message : 'Nie udało się zapisać zmian';
    } finally {
      isSaving.value = false;
    }
  };
</script>

<template>
  <div class="wizard-outer">

    <!-- SEKCJA: Stany ładowania i błędu oferty -->
    <p v-if="loadError" class="load-error">{{ loadError }}</p>
    <p v-if="isLoadingOffer" class="load-hint">Ładowanie oferty…</p>

    <!-- SEKCJA: Formularz edycji oferty (tylko dla właściciela) -->
    <template v-if="!isLoadingOffer && !loadError && form.selectedBook">

      <div class="wizard-card animate-in">
        <div class="wcard-head">
          <h2 class="wcard-title">Edytuj ofertę</h2>
          <p class="wcard-desc">Zmień typ oferty lub uzupełnij szczegóły. Zapisz, gdy skończysz.</p>
        </div>
        <div class="wcard-body">

          <div class="book-recap">
            <img
              v-if="form.selectedBook.coverUrl && !recapCoverFailed"
              :src="form.selectedBook.coverUrl"
              :alt="form.selectedBook.title"
              class="recap-cover"
              @error="recapCoverFailed = true"
            >
            <div
              v-else
              class="recap-cover recap-cover-fallback"
              :style="{ background: coverColor(form.selectedBook.title) }"
            >
              <span class="rcf-title">{{ form.selectedBook.title }}</span>
              <span class="rcf-author">{{ form.selectedBook.author }}</span>
            </div>
            <div class="recap-info">
              <div class="recap-title">{{ form.selectedBook.title }}</div>
              <div class="recap-author">{{ form.selectedBook.author }}</div>
            </div>
            <RouterLink to="/polka" class="recap-change">← Anuluj</RouterLink>
          </div>

          <div class="offer-type-heading">Typ oferty</div>
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

          <div class="form-field" style="margin-top:20px">
            <label class="form-label">Opis (opcjonalnie - max 2000 znaków)</label>
            <textarea class="form-input" v-model="bookDescription" rows="3" placeholder="Krótki opis książki…" style="resize:vertical"></textarea>
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
            <RouterLink to="/polka" class="btn btn-outline">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><line x1="19" y1="12" x2="5" y2="12" /><polyline points="12 19 5 12 12 5" /></svg>
              Anuluj
            </RouterLink>
            <p v-if="submitError" style="color:var(--danger);font-size:13px;margin-bottom:12px">{{ submitError }}</p>
            <button class="btn btn-filled btn-lg" :disabled="!form.offerType || isSaving" @click="saveOffer">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12" /></svg>
              {{ isSaving ? 'Zapisywanie…' : 'Zapisz zmiany' }}
            </button>
          </div>
        </div>
      </div>
    </template>

    <div v-if="!isLoadingOffer && loadError" class="step-footer" style="margin-top:24px">
      <RouterLink to="/polka" class="btn btn-outline">Wróć na półkę</RouterLink>
    </div>
  </div>

  <!-- SEKCJA: Overlay sukcesu po zapisaniu zmian -->
  <div class="success-overlay" :class="{ open: showSuccess }">
    <div class="success-box">
      <div class="success-icon">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="var(--success)" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
      </div>
      <h3>Oferta zaktualizowana!</h3>
      <p>Zmiany są już widoczne na Twojej półce.</p>
      <div style="display:flex;gap:10px">
        <RouterLink to="/polka" class="btn btn-outline btn-full btn-sm">Moja półka</RouterLink>
        <RouterLink to="/przegladanie" class="btn btn-filled btn-full btn-sm">Przeglądaj</RouterLink>
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

  .recap-cover-fallback {
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

  .recap-cover-fallback .rcf-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 8px;
    line-height: 1.1;
  }

  .recap-cover-fallback .rcf-author {
    font-size: 8px;
    opacity: 0.8;
  }
</style>
