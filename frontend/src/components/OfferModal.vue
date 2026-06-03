<script setup>
  import { ref, watch, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { userBooksApi } from '../lib/userBooks';
  import { transactionsApi } from '../lib/transactions';
  import { realCoverUrl, coverColor, avatarImageUrl } from '../lib/mappers';
  import { ApiError } from '../lib/client';
  import { useAuthStore } from '../stores/auth';

  const props = defineProps({
    offerId: { type: String, default: null },
  });
  const emit = defineEmits(['close']);

  const router = useRouter();
  const auth = useAuthStore();

  const offer = ref(null);
  const loading = ref(false);
  const error = ref('');
  const coverFailed = ref(false);

  // Stan wysyłania prośby
  const sending = ref(false);
  const sent = ref(false);
  const sendError = ref('');

  // Nie można złożyć prośby o własną ofertę.
  const isOwn = computed(() => !!offer.value && offer.value.owner.id === auth.user?.id);

  const isUnavailable = computed(() => !!offer.value && offer.value.status !== 'available');

  const badge = computed(() => {
    const map = {
      loan: { cls: 'badge-loan', label: 'Do wypożyczenia' },
      swap: { cls: 'badge-swap', label: 'Do wymiany' },
      sale: { cls: 'badge-sale', label: 'Do sprzedaży' },
    };
    return map[offer.value?.transactionType] || { cls: '', label: '' };
  });

  const coverUrl = computed(() => realCoverUrl(offer.value?.book?.coverUrl));

  const ownerAvatar = computed(() =>
    offer.value?.owner?.avatarUrl
      ? avatarImageUrl({ id: offer.value.owner.id, avatarUrl: offer.value.owner.avatarUrl })
      : null
  );
  const ownerInitial = computed(() => (offer.value?.owner?.username?.[0] || '?').toUpperCase());

  const exchangeConditionLabel = computed(() => {
    const map = {
      anything: 'Cokolwiek pasującego',
      specific_book: 'Konkretna książka',
      genre: 'Książki danego gatunku',
      free: 'Oddam za darmo',
    };
    const c = offer.value?.exchangeCondition;
    return c ? (map[c] || c) : null;
  });

  async function load(id) {
    loading.value = true;
    error.value = '';
    offer.value = null;
    coverFailed.value = false;
    // reset stanu wysyłki przy każdym otwarciu/zmianie oferty
    sent.value = false;
    sendError.value = '';
    sending.value = false;
    try {
      offer.value = await userBooksApi.getById(id);
    } catch (err) {
      error.value = err instanceof ApiError ? err.message : 'Nie udało się wczytać oferty';
    } finally {
      loading.value = false;
    }
  }

  watch(() => props.offerId, (id) => {
    if (id) load(id);
  }, { immediate: true });

  async function sendRequest() {
    // Trzeba być zalogowanym — w przeciwnym razie przekierowanie na logowanie.
    if (!auth.isLoggedIn) {
      router.push('/logowanie?tab=login');
      return;
    }
    if (!offer.value || sending.value) return;

    sendError.value = '';
    sending.value = true;
    try {
      await transactionsApi.create({ userBookId: offer.value.id });
      sent.value = true;
    } catch (err) {
      // Backend: własna oferta 400, niedostępna 409, duplikat pending 409 — pokazujemy komunikat z API.
      sendError.value = err instanceof ApiError ? err.message : 'Nie udało się wysłać prośby';
    } finally {
      sending.value = false;
    }
  }

  const close = () => emit('close');
</script>

<template>
  <Teleport to="body">
    <Transition name="offer-fade">
      <div v-if="offerId" class="offer-backdrop" @click.self="close">
        <div class="offer-modal" role="dialog" aria-modal="true">
          <button class="offer-close" type="button" @click="close" aria-label="Zamknij">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></svg>
          </button>

          <p v-if="loading" class="offer-status">Ładowanie oferty…</p>
          <p v-else-if="error" class="offer-status offer-status--error">{{ error }}</p>

          <div v-else-if="offer" class="offer-grid">
            <div class="offer-cover-col">
              <img v-if="coverUrl && !coverFailed" :src="coverUrl" :alt="offer.book.title" class="offer-cover" @error="coverFailed = true">
              <div v-else class="offer-cover offer-cover-fallback" :style="{ background: coverColor(offer.book.title) }">
                <span class="ocf-title">{{ offer.book.title }}</span>
                <span class="ocf-author">{{ offer.book.author }}</span>
              </div>
            </div>

            <div class="offer-details">
              <span class="offer-badge" :class="badge.cls">{{ badge.label }}</span>
              <h2 class="offer-title">{{ offer.book.title }}</h2>
              <p class="offer-author">{{ offer.book.author }}</p>
              <p v-if="offer.book.genreName" class="offer-genre">{{ offer.book.genreName }}</p>

              <p v-if="offer.book.description" class="offer-desc">{{ offer.book.description }}</p>

              <div v-if="offer.transactionType === 'sale' && offer.price != null" class="offer-price">{{ offer.price }} zł</div>
              <div v-else-if="offer.transactionType === 'swap' && exchangeConditionLabel" class="offer-cond">
                <strong>W zamian:</strong> {{ exchangeConditionLabel }}<template v-if="offer.exchangeDetails"> — {{ offer.exchangeDetails }}</template>
              </div>

              <div class="offer-owner">
                <img v-if="ownerAvatar" :src="ownerAvatar" :alt="offer.owner.username" class="offer-owner-av">
                <div v-else class="offer-owner-av offer-owner-av-fallback" aria-hidden="true">{{ ownerInitial }}</div>
                <div class="offer-owner-info">
                  <span class="offer-owner-nick">@{{ offer.owner.username }}</span>
                  <span class="offer-owner-city">{{ offer.owner.city }}</span>
                </div>
              </div>

              <p v-if="isOwn" class="offer-note">To Twoja oferta.</p>
              <p v-else-if="isUnavailable" class="offer-note">Ta oferta nie jest już dostępna.</p>
              <div v-else-if="sent" class="offer-sent">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
                Prośba wysłana!
              </div>
              <template v-else-if="!isUnavailable">
                <p v-if="sendError" class="offer-send-error">{{ sendError }}</p>
                <button class="btn btn-filled offer-cta" type="button" :disabled="sending" @click="sendRequest">
                  {{ sending ? 'Wysyłanie…' : (auth.isLoggedIn ? 'Wyślij prośbę' : 'Zaloguj się, aby wysłać prośbę') }}
                </button>
              </template>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
  .offer-backdrop {
    position: fixed;
    inset: 0;
    z-index: 2000;
    background: rgba(44, 24, 16, 0.45);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px;
  }

  .offer-modal {
    position: relative;
    width: 100%;
    max-width: 720px;
    max-height: 90vh;
    overflow-y: auto;
    background: var(--surface);
    border-radius: var(--r-card);
    box-shadow: var(--shadow-h);
    padding: 28px;
  }

  .offer-close {
    position: absolute;
    top: 14px;
    right: 14px;
    width: 32px;
    height: 32px;
    border: none;
    border-radius: 50%;
    background: var(--tag-bg);
    color: var(--text-2);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: background 0.18s ease;
  }
  .offer-close:hover {
    background: var(--border);
  }

  .offer-status {
    text-align: center;
    padding: 48px 24px;
    color: var(--text-2);
  }
  .offer-status--error {
    color: var(--danger);
  }

  .offer-grid {
    display: grid;
    grid-template-columns: 200px 1fr;
    gap: 28px;
  }

  .offer-cover-col {
    flex-shrink: 0;
  }

  .offer-cover {
    width: 200px;
    height: 280px;
    border-radius: 10px;
    object-fit: cover;
    display: block;
    box-shadow: var(--shadow);
  }

  .offer-cover-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 20px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
  }
  .offer-cover-fallback .ocf-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 18px;
    line-height: 1.25;
  }
  .offer-cover-fallback .ocf-author {
    font-size: 13px;
    opacity: 0.85;
  }

  .offer-details {
    display: flex;
    flex-direction: column;
    gap: 8px;
    min-width: 0;
  }

  .offer-badge {
    align-self: flex-start;
    padding: 4px 10px;
    border-radius: 5px;
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  .offer-title {
    font-family: 'Playfair Display', serif;
    font-size: 24px;
    font-weight: 700;
    color: var(--text-1);
    line-height: 1.2;
    margin: 4px 0 0;
  }
  .offer-author {
    color: var(--text-2);
    font-size: 15px;
  }
  .offer-genre {
    font-size: 12px;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: 0.4px;
  }
  .offer-desc {
    color: var(--text-2);
    font-size: 14px;
    line-height: 1.6;
    margin: 6px 0;
  }
  .offer-price {
    font-family: 'Playfair Display', serif;
    font-size: 22px;
    font-weight: 700;
    color: var(--primary);
  }
  .offer-cond {
    font-size: 14px;
    color: var(--text-2);
  }

  .offer-owner {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-top: 10px;
    padding-top: 14px;
    border-top: 1px solid var(--border);
  }
  .offer-owner-av {
    width: 38px;
    height: 38px;
    border-radius: 50%;
    object-fit: cover;
  }
  .offer-owner-av-fallback {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: var(--tag-bg);
    color: var(--text-2);
    font-weight: 600;
    font-size: 15px;
  }
  .offer-owner-info {
    display: flex;
    flex-direction: column;
    line-height: 1.3;
  }
  .offer-owner-nick {
    font-weight: 600;
    color: var(--text-1);
    font-size: 14px;
  }
  .offer-owner-city {
    font-size: 13px;
    color: var(--text-3);
  }

  .offer-cta {
    margin-top: 16px;
    align-self: flex-start;
  }

  .offer-note {
    margin-top: 16px;
    font-size: 14px;
    color: var(--text-3);
    font-style: italic;
  }

  .offer-sent {
    margin-top: 16px;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    align-self: flex-start;
    padding: 10px 16px;
    border-radius: var(--r-btn);
    background: var(--st-accept-bg);
    color: var(--st-accept-tx);
    font-weight: 600;
    font-size: 14px;
  }

  .offer-send-error {
    color: var(--danger);
    font-size: 13px;
    margin-bottom: 4px;
  }

  /* Animacja otwarcia/zamknięcia */
  .offer-fade-enter-active,
  .offer-fade-leave-active {
    transition: opacity 0.22s ease;
  }
  .offer-fade-enter-active .offer-modal,
  .offer-fade-leave-active .offer-modal {
    transition: transform 0.22s ease, opacity 0.22s ease;
  }
  .offer-fade-enter-from,
  .offer-fade-leave-to {
    opacity: 0;
  }
  .offer-fade-enter-from .offer-modal,
  .offer-fade-leave-to .offer-modal {
    transform: translateY(12px) scale(0.98);
    opacity: 0;
  }

  @media (max-width: 560px) {
    .offer-grid {
      grid-template-columns: 1fr;
      gap: 18px;
    }
    .offer-cover,
    .offer-cover-fallback {
      width: 100%;
      height: 220px;
    }
  }
</style>
