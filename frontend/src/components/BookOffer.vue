<script setup lang="ts">
  /**
   * Karta oferty lub transakcji na stronie Moja półka.
   * Użycie: ShelfView.vue — zakładki transakcji i wystawione oferty.
   * Props: item (dane karty), isListed (true = wystawiona oferta), direction (received|sent).
   * Emity: action (zmiana statusu), delete, edit.
   * Dwa widoki: listed-card (menu edycji) lub tx-card (akceptacja/odrzucenie/kontakt).
   */
  import { ref, computed, onMounted } from 'vue';
  import { coverColor } from '../lib/mappers';
  import { transactionsApi } from '../lib/transactions';
  import {
    offerBrowseLabel,
    offerListedBadgeClass,
    offerTxCardClass,
    offerTxLabel,
    offerTxTypeClass,
  } from '../lib/offerTypes';

  const props = withDefaults(defineProps<{
    item: any;
    isListed?: boolean;
    direction?: 'received' | 'sent';
  }>(), {
    isListed: false,
    direction: 'received'
  });

  const emit = defineEmits(['action', 'delete', 'edit']);

  const isMenuOpen = ref(false);
  const isAcceptFormOpen = ref(false);
  const isRejectConfirmOpen = ref(false);
  const flashClass = ref('');
  const isDeletedAnimate = ref(false);
  const coverFailed = ref(false);
  const formReturnDate = ref(props.item.returnDate ? props.item.returnDate.split('.').reverse().join('-') : '2026-06-28');

  const isDimmed = computed(() => props.item.status === 'done' || props.item.status === 'rejected');
  const statusMeta = computed(() => {
    const meta: Record<string, any> = {
      pending: { label: 'Oczekuje', cls: 'st-pending', icon: '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>' },
      accepted: { label: 'Zaakceptowana', cls: 'st-accepted', icon: '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>' },
      done: { label: 'Zakończona', cls: 'st-done', icon: '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/></svg>' },
      rejected: { label: 'Odrzucona', cls: 'st-rejected', icon: '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>' },
    };
    return meta[props.item.status] || {};
  });

  const cardStyle = computed(() => {
    if (isDeletedAnimate.value) {
      return { transition: 'opacity .3s, transform .3s', opacity: '0', transform: 'scale(.93)' };
    }
    return {};
  });

  const toggleDropdown = () => {
    isMenuOpen.value = !isMenuOpen.value;
  };
  const closeDropdown = () => {
    isMenuOpen.value = false;
  };

  const canEdit = computed(() => props.item.canEdit !== false);

  const handleEdit = () => {
    if (!canEdit.value) return;
    closeDropdown();
    emit('edit', props.item);
  };

  const handleDelete = () => {
    isDeletedAnimate.value = true;
    setTimeout(() => emit('delete', props.item.id), 300);
  };

  const triggerAction = (status: string, flashType: 'g' | 'r', extraData = {}) => {
    flashClass.value = flashType === 'g' ? 'flash-g' : 'flash-r';
    setTimeout(() => {
      flashClass.value = '';
      emit('action', { id: props.item.id, status, ...extraData });
      isAcceptFormOpen.value = false;
      isRejectConfirmOpen.value = false;
    }, 680);
  };

  const confirmAccept = () => {
    const formattedDate = formReturnDate.value.split('-').reverse().join('.');
    triggerAction('accepted', 'g', { returnDate: formattedDate });
  };

  const confirmReject = () => {
    triggerAction('rejected', 'r');
  };

  const counterpartyInitial = computed(() => (props.item.nick?.[0] || '?').toUpperCase());

  /** Email/nick drugiej strony — ładowane po akceptacji transakcji. */
  const contact = ref<{ username: string; email: string } | null>(null);

  async function loadContact() {
    if (props.isListed) return;
    if (props.item.status !== 'accepted' && props.item.status !== 'done') return;
    try {
      contact.value = await transactionsApi.getContact(props.item.id);
    } catch {
      contact.value = null;
    }
  }

  onMounted(() => {
    loadContact();
  });
</script>

<template>
  <!-- Widok A: wystawiona oferta z menu edycji/usuwania -->
  <div v-if="isListed" class="listed-card" :style="cardStyle">
    <div class="lc-cover-wrap">
      <img v-if="item.coverUrl && !coverFailed" :src="item.coverUrl" :alt="item.bookTitle" class="lc-cover" loading="lazy" @error="coverFailed = true">
      <div v-else class="lc-cover cover-fallback" :style="{ background: coverColor(item.bookTitle) }">
        <span class="cover-fallback-title">{{ item.bookTitle }}</span>
        <span class="cover-fallback-author">{{ item.bookAuthor }}</span>
      </div>
      <div class="lc-cover-badges">
        <div
          v-if="!canEdit && item.editBlockBanner"
          class="own-offer-badge"
          :aria-label="item.editBlockBanner"
        >
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
          <span>{{ item.editBlockBanner }}</span>
        </div>
        <span :class="['lc-badge', offerListedBadgeClass(item.type)]">
          {{ offerBrowseLabel(item.type) }}
        </span>
      </div>
      <div class="lc-menu-wrap" v-click-outside="closeDropdown">
        <button class="lc-menu-btn" @click.stop="toggleDropdown" aria-label="Opcje">
          <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor"><circle cx="7" cy="2" r="1.3" /><circle cx="7" cy="7" r="1.3" /><circle cx="7" cy="12" r="1.3" /></svg>
        </button>
        <div :class="['lc-dropdown', { open: isMenuOpen }]">
          <button
            class="lc-dd-item"
            :class="{ 'lc-dd-item-disabled': !canEdit }"
            :disabled="!canEdit"
            :title="canEdit ? undefined : (item.editBlockReason ?? 'Edycja niedostępna')"
            @click.stop="handleEdit"
          >
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" /></svg>
            Edytuj ofertę
          </button>
          <button class="lc-dd-item danger" @click.stop="handleDelete">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><polyline points="3 6 5 6 21 6" /><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" /><path d="M10 11v6" /><path d="M14 11v6" /><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" /></svg>
            Usuń z półki
          </button>
        </div>
      </div>
    </div>
    <div class="lc-body">
      <div class="lc-title">{{ item.bookTitle }}</div>
      <div class="lc-author">{{ item.bookAuthor }}</div>
      <div v-if="item.type === 'sale' && item.price" class="lc-price">{{ item.price }} zł</div>
      <div class="lc-date">Wystawiono: {{ item.dateAdded }}</div>
      <div class="lc-status">
        <span :class="['lc-dot', item.listStatus === 'active' ? 'lc-dot-active' : item.listStatus === 'completed' ? 'lc-dot-completed' : 'lc-dot-available']"></span>
        <span :class="item.listStatus === 'active' ? 'lc-status-active' : item.listStatus === 'completed' ? 'lc-status-completed' : 'lc-status-available'">
          {{ item.listStatus === 'active' ? 'W trakcie transakcji' : item.listStatus === 'completed' ? 'Zrealizowana' : 'Dostępna' }}
        </span>
      </div>
    </div>
  </div>

  <!-- Widok B: transakcja — status, akcje właściciela, dane kontaktowe po akceptacji -->
  <div v-else :class="['tx-card', offerTxCardClass(item.type), { dimmed: isDimmed, 'pending-pulse': item.status === 'pending' }, flashClass]">
    <div class="tx-main">
      <div class="tx-left">
        <img v-if="item.coverUrl && !coverFailed" :src="item.coverUrl" :alt="item.bookTitle" class="tx-cover" @error="coverFailed = true">
        <div v-else class="tx-cover cover-fallback cover-fallback-sm" :style="{ background: coverColor(item.bookTitle) }">
          <span class="cover-fallback-title">{{ item.bookTitle }}</span>
        </div>
        <div class="tx-book-title">{{ item.bookTitle }}</div>
        <div class="tx-book-author">{{ item.bookAuthor }}</div>
      </div>
      <div class="tx-mid">
        <div class="tx-row1">
          <span :class="['type-badge', offerTxTypeClass(item.type)]">
            {{ offerTxLabel(item.type) }}
          </span>
          <div class="tx-dir">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2">
              <template v-if="direction === 'received'">
                <polyline points="8 17 12 21 16 17" />
                <line x1="12" y1="3" x2="12" y2="21" />
              </template>
              <template v-else>
                <polyline points="16 7 12 3 8 7" />
                <line x1="12" y1="3" x2="12" y2="21" />
              </template>
            </svg>
            <img v-if="item.avatarUrl" :src="item.avatarUrl" :alt="item.nick">
            <span v-else class="tx-av-fallback" aria-hidden="true">{{ counterpartyInitial }}</span>
            <span>{{ direction === 'received' ? 'Prośba od ' : 'Twoja prośba do ' }}<strong>@{{ item.nick }}</strong></span>
          </div>
        </div>
        <div class="tx-meta-row">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10" /><polyline points="12 6 12 12 16 14" /></svg>
          {{ item.date }}
        </div>
        <div v-if="item.msg" class="tx-msg">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 21c3 0 7-1 7-8V5c0-1.25-.756-2.017-2-2H4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2 1 0 1 0 1 1v1c0 1-1 2-2 2s-1 .008-1 1.031V20c0 1 0 1 1 1z" /></svg>
          "{{ item.msg.length > 80 ? item.msg.slice(0,80)+'…' : item.msg }}"
        </div>
        <div v-if="item.type === 'loan' && item.returnDate" class="tx-return">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></svg>
          {{ direction === 'received' ? 'Proponowany zwrot:' : 'Zwrot do:' }} <strong>{{ item.returnDate }}</strong>
        </div>
        <div v-if="item.type === 'swap' && item.wants" class="tx-swap-cond">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="17 1 21 5 17 9" /><path d="M3 11V9a4 4 0 0 1 4-4h14" /><polyline points="7 23 3 19 7 15" /><path d="M21 13v2a4 4 0 0 1-4 4H3" /></svg>
          Szuka: <strong>{{ item.wants }}</strong>
        </div>
        <div v-if="item.type === 'sale' && item.price" :class="['tx-price-line', { sold: item.status === 'done' }]">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <template v-if="item.status === 'done'">
              <path d="M21 8v13H3V8" /><path d="M1 3h22v5H1z" />
            </template>
            <template v-else>
              <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
            </template>
          </svg>
          {{ item.status === 'done' ? 'Sprzedane za ' : 'Cena: ' }}<strong>{{ item.price }} zł</strong>
        </div>
      </div>
      <div class="tx-right">
        <div :class="['status-badge', statusMeta.cls]" v-html="statusMeta.icon + ' ' + statusMeta.label"></div>

        <div v-if="direction === 'received' && item.status === 'pending' && !isAcceptFormOpen && !isRejectConfirmOpen" class="tx-actions">
          <button class="btn-accept" @click="isAcceptFormOpen = true">Zaakceptuj</button>
          <button class="btn-reject" @click="isRejectConfirmOpen = true">Odrzuć</button>
        </div>

        <div v-if="isRejectConfirmOpen" class="reject-confirm show">
          <div class="rq-text">Na pewno odrzucić?</div>
          <div class="rq-btns">
            <button class="btn-confirm-rej" @click="confirmReject">Odrzuć</button>
            <button class="btn-cancel-rej" @click="isRejectConfirmOpen = false">Anuluj</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="(item.status === 'accepted' || item.status === 'done') && contact" class="contact-section">
      <div class="contact-head">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
        Dane kontaktowe
      </div>

      <div class="contact-row">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" /></svg>
        <a :href="`mailto:${contact?.email}`">Skontaktuj się: {{ contact?.email }}</a>
      </div>

      <div class="contact-row">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="8" r="4" /><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" /></svg>
        <span>@{{ contact?.username }}</span>
      </div>

      <div class="contact-note">Skontaktuj się, aby ustalić szczegóły odbioru</div>

      <div class="contact-footer" v-if="direction === 'received' && item.status === 'accepted'">
        <button class="btn-finish" @click="triggerAction('finish', 'g')">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M21 8v13H3V8" /><path d="M1 3h22v5H1z" /></svg>
          Zakończ transakcję
        </button>
      </div>
    </div>

    <div :class="['accept-form', { open: isAcceptFormOpen }]">
      <div v-if="item.type === 'loan'" class="af-row">
        <div>
          <label class="af-label">Termin zwrotu</label>
          <input class="af-input" type="date" v-model="formReturnDate">
        </div>
      </div>
      <div class="af-actions">
        <button class="btn-confirm-accept" @click="confirmAccept">Potwierdź akceptację</button>
        <button class="btn-cancel-accept" @click="isAcceptFormOpen = false">Anuluj</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .cover-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 10px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
    overflow: hidden;
  }

  .cover-fallback-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 13px;
    line-height: 1.2;
  }

  .cover-fallback-author {
    font-size: 11px;
    opacity: 0.8;
  }

  .cover-fallback-sm {
    padding: 6px;
  }

  .cover-fallback-sm .cover-fallback-title {
    font-size: 10px;
    line-height: 1.15;
  }

  .lc-cover-badges {
    position: absolute;
    top: 9px;
    left: 9px;
    z-index: 1;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
    max-width: calc(100% - 18px);
  }

  .lc-cover-badges .lc-badge {
    position: static;
  }

  .lc-dd-item-disabled,
  .lc-dd-item:disabled {
    opacity: 0.45;
    cursor: not-allowed;
    pointer-events: none;
  }

  .tx-av-fallback {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background: var(--tag-bg, #f0e8df);
    color: var(--text-2, #7a6652);
    font-weight: 600;
    font-size: 10px;
  }
</style>
