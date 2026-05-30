<script setup lang="ts">
  import { ref, computed, onMounted, onUnmounted } from 'vue';

  const props = withDefaults(defineProps<{
    item: any;
    isListed?: boolean;
    direction?: 'received' | 'sent';
  }>(), {
    isListed: false,
    direction: 'received'
  });

  const emit = defineEmits(['action', 'delete', 'edit']);

  // State
  const isMenuOpen = ref(false);
  const isAcceptFormOpen = ref(false);
  const isRejectConfirmOpen = ref(false);
  const flashClass = ref('');
  const isDeletedAnimate = ref(false);
  const formReturnDate = ref(props.item.returnDate ? props.item.returnDate.split('.').reverse().join('-') : '2026-06-28');

  // Computed dla statusów transakcji
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

  // Metody
  const toggleDropdown = () => {
    isMenuOpen.value = !isMenuOpen.value;
  };
  const closeDropdown = () => {
    isMenuOpen.value = false;
  };

  const handleEdit = () => {
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

  onMounted(() => document.addEventListener('click', closeDropdown));
  onUnmounted(() => document.removeEventListener('click', closeDropdown));
</script>

<template>
  <div v-if="isListed" class="listed-card" :style="cardStyle">
    <div class="lc-cover-wrap">
      <img :src="`https://picsum.photos/180/240?random=${item.img}`" :alt="item.bookTitle" class="lc-cover" loading="lazy">
      <span :class="['lc-badge', item.type === 'loan' ? 'lcb-loan' : item.type === 'swap' ? 'lcb-swap' : 'lcb-sale']">
        {{ item.type === 'loan' ? 'Do wypożyczenia' : item.type === 'swap' ? 'Do wymiany' : 'Do sprzedaży' }}
      </span>
      <button class="lc-menu-btn" @click.stop="toggleDropdown" aria-label="Opcje">
        <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor"><circle cx="7" cy="2" r="1.3" /><circle cx="7" cy="7" r="1.3" /><circle cx="7" cy="12" r="1.3" /></svg>
      </button>
      <div :class="['lc-dropdown', { open: isMenuOpen }]">
        <button class="lc-dd-item" @click.stop="handleEdit">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" /></svg>
          Edytuj ofertę
        </button>
        <button class="lc-dd-item danger" @click.stop="handleDelete">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><polyline points="3 6 5 6 21 6" /><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" /><path d="M10 11v6" /><path d="M14 11v6" /><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" /></svg>
          Usuń z półki
        </button>
      </div>
    </div>
    <div class="lc-body">
      <div class="lc-title">{{ item.bookTitle }}</div>
      <div class="lc-author">{{ item.bookAuthor }}</div>
      <div v-if="item.type === 'sale' && item.price" class="lc-price">{{ item.price }} zł</div>
      <div class="lc-date">Wystawiono: {{ item.dateAdded }}</div>
      <div class="lc-status">
        <span :class="['lc-dot', item.listStatus === 'active' ? 'lc-dot-active' : 'lc-dot-available']"></span>
        <span :class="item.listStatus === 'active' ? 'lc-status-active' : 'lc-status-available'">
          {{ item.listStatus === 'active' ? 'W trakcie transakcji' : 'Dostępna' }}
        </span>
      </div>
    </div>
  </div>

  <div v-else :class="['tx-card', `t-${item.type}`, { dimmed: isDimmed, 'pending-pulse': item.status === 'pending' }, flashClass]">
    <div class="tx-main">
      <div class="tx-left">
        <img :src="`https://picsum.photos/60/80?random=${item.img}`" :alt="item.bookTitle" class="tx-cover">
        <div class="tx-book-title">{{ item.bookTitle }}</div>
        <div class="tx-book-author">{{ item.bookAuthor }}</div>
      </div>
      <div class="tx-mid">
        <div class="tx-row1">
          <span :class="['type-badge', item.type === 'loan' ? 'type-loan' : item.type === 'swap' ? 'type-swap' : 'type-sale']">
            {{ item.type === 'loan' ? 'Wypożyczenie' : item.type === 'swap' ? 'Wymiana' : 'Sprzedaż' }}
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
            <img :src="`https://i.pravatar.cc/40?img=${item.av}`" :alt="item.nick">
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

    <div v-if="item.status === 'accepted' && item.contact" class="contact-section">
      <div class="contact-head">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
        Dane kontaktowe
      </div>

      <div class="contact-row">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" /></svg>
        <a :href="`mailto:${item.contact.email}`">{{ item.contact.email }}</a>
      </div>

      <div class="contact-row">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="8" r="4" /><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" /></svg>
        <span>{{ item.contact.nick }}</span>
      </div>

      <div class="contact-note">Skontaktuj się, aby ustalić szczegóły odbioru</div>

      <div class="contact-footer" v-if="direction === 'received'">
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
  .tx-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    border-left: 4px solid var(--border);
    padding: 18px 20px;
    margin-bottom: 12px;
    box-shadow: var(--shadow);
    transition: box-shadow .2s, opacity .3s;
  }

    .tx-card:hover {
      box-shadow: var(--shadow-h);
    }

    .tx-card.t-loan {
      border-left-color: var(--primary);
    }

    .tx-card.t-swap {
      border-left-color: var(--accent);
    }

    .tx-card.t-sale {
      border-left-color: var(--blue);
    }

    .tx-card.dimmed {
      opacity: .68;
    }

    .tx-card.pending-pulse {
      animation: pulseLoan 2.4s ease-in-out infinite;
    }

  @keyframes pulseLoan {
    0%,100% {
      opacity: 1
    }

    50% {
      opacity: 0.6
    }
  }

  .tx-main {
    display: grid;
    grid-template-columns: 80px 1fr 160px;
    gap: 16px;
    align-items: start;
  }

  .tx-left {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .tx-cover {
    width: 60px;
    height: 80px;
    border-radius: 6px;
    object-fit: cover;
    box-shadow: 0 2px 8px rgba(44,24,16,.14);
    display: block;
  }

  .tx-book-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-1);
    line-height: 1.3;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .tx-book-author {
    font-size: 11px;
    color: var(--text-2);
  }

  .tx-mid {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .tx-row1 {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
  }

  .type-badge {
    padding: 3px 9px;
    border-radius: 10px;
    font-size: 10px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: .3px;
    white-space: nowrap;
  }

  .type-loan {
    background: #D1E7DD;
    color: #0A3622;
  }

  .type-swap {
    background: #FFF3CD;
    color: #856404;
  }

  .type-sale {
    background: #CFE2FF;
    color: #084298;
  }

  .tx-dir {
    display: flex;
    align-items: center;
    gap: 7px;
    font-size: 13px;
    color: var(--text-2);
  }

    .tx-dir img {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      object-fit: cover;
      border: 2px solid var(--border);
      flex-shrink: 0;
    }

    .tx-dir strong {
      color: var(--text-1);
      font-weight: 600;
    }

  .tx-meta-row {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 12px;
    color: var(--text-3);
  }

  .tx-msg {
    display: flex;
    align-items: flex-start;
    gap: 6px;
    font-size: 12px;
    color: var(--text-2);
    font-style: italic;
    line-height: 1.5;
  }

  .tx-return {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 12px;
    color: var(--text-2);
  }

    .tx-return strong {
      font-weight: 600;
      color: var(--primary);
    }

  .tx-swap-cond {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 12px;
    color: var(--text-2);
  }

    .tx-swap-cond strong {
      font-weight: 600;
      color: var(--accent);
    }

  .tx-price-line {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 12px;
    color: var(--text-2);
  }

    .tx-price-line.sold {
      color: var(--st-done-tx);
    }

  .tx-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 10px;
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 7px 13px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
    white-space: nowrap;
  }

  .st-pending {
    background: var(--st-pending-bg);
    color: var(--st-pending-tx);
  }

  .st-accepted {
    background: var(--st-accept-bg);
    color: var(--st-accept-tx);
  }

  .st-done {
    background: var(--st-done-bg);
    color: var(--st-done-tx);
  }

  .st-rejected {
    background: var(--st-reject-bg);
    color: var(--st-reject-tx);
  }

  .tx-actions {
    display: flex;
    flex-direction: column;
    gap: 6px;
    width: 100%;
  }

  .btn-accept {
    background: var(--primary);
    color: #fff;
    border: 1.5px solid var(--primary);
    padding: 7px 12px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
  }

    .btn-accept:hover {
      background: var(--primary-h);
    }

  .btn-reject {
    background: transparent;
    color: var(--danger);
    border: 1.5px solid var(--danger);
    padding: 7px 12px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
  }

    .btn-reject:hover {
      background: #fef0ee;
    }

  .btn-finish {
    background: transparent;
    color: var(--st-done-tx);
    border: 1.5px solid var(--st-done-tx);
    padding: 6px 12px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    display: inline-flex;
    align-items: center;
    gap: 5px;
    margin-top: 6px;
  }

    .btn-finish:hover {
      background: var(--st-done-bg);
    }

  .reject-confirm {
    background: #fff5f5;
    border: 1px solid #f5c6c8;
    border-radius: 8px;
    padding: 10px 12px;
    width: 100%;
    display: none;
  }

    .reject-confirm.show {
      display: block;
    }

  .rq-text {
    font-size: 12px;
    font-weight: 600;
    color: var(--st-reject-tx);
    margin-bottom: 8px;
  }

  .rq-btns {
    display: flex;
    gap: 6px;
  }

  .btn-confirm-rej {
    background: var(--danger);
    color: #fff;
    border: none;
    padding: 5px 10px;
    border-radius: 6px;
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    flex: 1;
  }

    .btn-confirm-rej:hover {
      opacity: .85;
    }

  .btn-cancel-rej {
    background: transparent;
    color: var(--text-2);
    border: 1.5px solid var(--border);
    padding: 5px 10px;
    border-radius: 6px;
    font-family: 'DM Sans',sans-serif;
    font-size: 12px;
    cursor: pointer;
    flex: 1;
  }

    .btn-cancel-rej:hover {
      background: var(--tag-bg);
    }

  .contact-section {
    margin-top: 14px;
    background: var(--tag-bg);
    border-radius: 8px;
    padding: 12px 16px;
    width: 100%;
    text-align: left;
  }

  .contact-head {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    font-weight: 600;
    color: var(--primary);
    margin-bottom: 10px;
  }

  .contact-row {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    color: var(--text-1);
    margin-bottom: 5px;
  }

    .contact-row a {
      color: var(--primary);
      text-decoration: none;
      font-weight: 500;
    }

  .contact-footer {
    display: flex;
    justify-content: flex-end;
    margin-top: 10px;
  }

  .accept-form {
    max-height: 0;
    overflow: hidden;
    transition: max-height .38s ease, opacity .28s ease;
    opacity: 0;
    border-top: 1px dashed transparent;
    width: 100%;
    text-align: left;
  }

    .accept-form.open {
      max-height: 300px;
      opacity: 1;
      border-top: 1px dashed var(--border);
      margin-top: 14px;
      padding-top: 14px;
    }

  .af-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
    display: block;
    margin-bottom: 5px;
  }

  .af-input {
    width: 100%;
    padding: 8px 12px;
    border: 1.5px solid var(--border);
    border-radius: 8px;
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    color: var(--text-1);
    background: var(--bg);
    outline: none;
    transition: border-color .2s;
  }

    .af-input:focus {
      border-color: var(--primary);
      background: #fff;
    }

  .af-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    margin-bottom: 12px;
  }

  .af-actions {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-top: 4px;
  }

  .btn-confirm-accept {
    background: var(--primary);
    color: #fff;
    border: 1.5px solid var(--primary);
    padding: 8px 18px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    display: flex;
    align-items: center;
    gap: 6px;
  }

    .btn-confirm-accept:hover {
      background: var(--primary-h);
    }

  .btn-cancel-accept {
    background: none;
    border: none;
    color: var(--text-3);
    font-family: 'DM Sans',sans-serif;
    font-size: 13px;
    cursor: pointer;
    transition: color .2s;
  }

  .flash-g {
    animation: flashGreen .7s ease;
  }

  .flash-r {
    animation: flashRed .7s ease;
  }

  @keyframes flashGreen {
    0% {
      background: var(--surface)
    }

    25% {
      background: #e8f5ee
    }

    100% {
      background: var(--surface)
    }
  }

  @keyframes flashRed {
    0% {
      background: var(--surface)
    }

    25% {
      background: #fdf1f0
    }

    100% {
      background: var(--surface)
    }
  }

  /* Listed Grid Styles */
  .listed-card {
    background: var(--surface);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    overflow: visible;
    cursor: pointer;
    transition: transform .22s ease, box-shadow .22s ease;
    border: 1px solid transparent;
    position: relative;
    text-align: left;
  }

    .listed-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-h);
      border-color: var(--border);
    }

  .lc-cover-wrap {
    position: relative;
    aspect-ratio: 3/4;
    overflow: hidden;
  }

  .lc-cover {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
    border-radius: 8px 8px 0 0;
    transition: transform .3s ease;
  }

  .listed-card:hover .lc-cover {
    transform: scale(1.04);
  }

  .lc-badge {
    position: absolute;
    top: 9px;
    left: 9px;
    padding: 3px 9px;
    border-radius: 4px;
    font-size: 9px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: .3px;
    z-index: 1;
  }

  .lcb-loan {
    background: var(--primary);
    color: #fff;
  }

  .lcb-swap {
    background: var(--accent);
    color: var(--text-1);
  }

  .lcb-sale {
    background: var(--blue);
    color: #fff;
  }

  .lc-menu-btn {
    position: absolute;
    top: 9px;
    right: 9px;
    width: 30px;
    height: 30px;
    border-radius: 50%;
    background: rgba(255,255,255,.92);
    backdrop-filter: blur(4px);
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--text-2);
    z-index: 15;
    opacity: 0;
    transform: scale(.8);
    transition: opacity .18s, transform .18s;
    box-shadow: 0 1px 4px rgba(44,24,16,.15);
  }

  .listed-card:hover .lc-menu-btn {
    opacity: 1;
    transform: scale(1);
  }

  .lc-menu-btn:hover {
    background: #fff;
    color: var(--text-1);
  }

  .lc-dropdown {
    position: absolute;
    top: 42px;
    right: 9px;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 10px;
    box-shadow: var(--shadow-h);
    min-width: 164px;
    overflow: hidden;
    z-index: 100;
    opacity: 0;
    transform: translateY(-6px) scale(.96);
    pointer-events: none;
    transition: opacity .16s, transform .16s;
  }

    .lc-dropdown.open {
      opacity: 1;
      transform: translateY(0) scale(1);
      pointer-events: all;
    }

  .lc-dd-item {
    display: flex;
    align-items: center;
    gap: 9px;
    padding: 10px 14px;
    font-size: 13px;
    color: var(--text-2);
    cursor: pointer;
    transition: background .14s;
    background: none;
    border: none;
    width: 100%;
    text-align: left;
    font-family: 'DM Sans',sans-serif;
    font-weight: 400;
  }

    .lc-dd-item:hover {
      background: var(--tag-bg);
      color: var(--text-1);
    }

    .lc-dd-item.danger {
      color: var(--danger);
    }

      .lc-dd-item.danger:hover {
        background: #fef0ee;
      }

  .lc-body {
    padding: 11px 13px 14px;
    background: var(--surface);
    border-radius: 0 0 var(--r-card) var(--r-card);
  }

  .lc-title {
    font-family: 'Playfair Display',serif;
    font-size: 14px;
    font-weight: 500;
    color: var(--text-1);
    line-height: 1.3;
    margin-bottom: 3px;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .lc-author {
    font-size: 12px;
    color: var(--text-2);
    margin-bottom: 3px;
  }

  .lc-price {
    font-size: 13px;
    font-weight: 600;
    color: var(--blue);
    margin-bottom: 4px;
  }

  .lc-date {
    font-size: 11px;
    color: var(--text-3);
    margin-bottom: 8px;
  }

  .lc-status {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    font-weight: 500;
  }

  .lc-dot {
    width: 7px;
    height: 7px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  .lc-dot-available {
    background: var(--success);
  }

  .lc-dot-active {
    background: var(--accent);
  }

  .lc-status-available {
    color: var(--success);
  }

  .lc-status-active {
    color: #856404;
  }

  /* ══ NAPRAWA KLAS DANYCH KONTAKTOWYCH ══ */
  .contact-head {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    font-weight: 600;
    color: var(--primary);
    margin-bottom: 10px;
  }

  .contact-row {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    color: var(--text-1);
    margin-bottom: 5px;
  }

    .contact-row svg {
      color: var(--text-3);
      flex-shrink: 0;
    }

    .contact-row a {
      color: var(--primary);
      text-decoration: none;
      font-weight: 500;
    }

      .contact-row a:hover {
        text-decoration: underline;
      }

  /* To są dokładnie te reguły, które odpowiadają za szary, mniejszy tekst i pasek u góry */
  .contact-note {
    font-size: 11px;
    color: var(--text-3);
    margin-top: 8px;
    border-top: 1px solid var(--border);
    padding-top: 7px;
  }
</style>
