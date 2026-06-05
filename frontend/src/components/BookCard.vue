<script setup>
  /**
   * Karta pojedynczej oferty książki w siatce przeglądania.
   * Użycie: BrowseView.vue — klik emituje select i otwiera OfferModal.
   * Props: book (obiekt oferty), isOwn (podświetlenie własnej oferty).
   * Emity: select (id oferty).
   * Sloty: cover-badges, meta — nadpisanie domyślnych odznak i metadanych właściciela.
   */
  import { ref, computed } from 'vue';
  import { coverColor } from '../lib/mappers';
  import { offerBadgeClass, offerBrowseLabel } from '../lib/offerTypes';

  const props = defineProps({
    book: {
      type: Object,
      required: true,
    },
    isOwn: {
      type: Boolean,
      default: false,
    },
  });

  defineEmits(['select']);

  const coverFailed = ref(false);

  const badgeClass = computed(() => offerBadgeClass(props.book.type));
  const badgeText = computed(() => offerBrowseLabel(props.book.type));

  const ownerInitial = computed(() => (props.book.owner?.[0] || '?').toUpperCase());
</script>

<template>
  <!-- Klikalna karta: okładka z odznakami + tytuł i właściciel -->
  <div :class="['book-card', { 'book-card--own': isOwn }]" @click="$emit('select', book.id)">
    <div class="cover-wrap">
      <img v-if="book.coverUrl && !coverFailed" :src="book.coverUrl" :alt="book.title" class="cover-img" loading="lazy" @error="coverFailed = true">
      <div v-else class="cover-img cover-fallback" :style="{ background: coverColor(book.title) }">
        <span class="cover-fallback-title">{{ book.title }}</span>
        <span class="cover-fallback-author">{{ book.author }}</span>
      </div>
      <div class="cover-badges">
        <slot name="cover-badges" :book="book" :is-own="isOwn" :badge-class="badgeClass" :badge-text="badgeText">
          <div v-if="isOwn" class="own-offer-badge" aria-label="Twoja oferta">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
            <span>Twoja oferta</span>
          </div>
          <span :class="['bk-badge', badgeClass]">{{ badgeText }}</span>
        </slot>
      </div>
      <button class="see-btn">Zobacz ofertę</button>
    </div>
    <div class="card-body">
      <h3 class="card-title">{{ book.title }}</h3>
      <p class="card-author">{{ book.author }}</p>
      <slot name="meta" :book="book" :owner-initial="ownerInitial">
        <div class="card-meta">
          <img v-if="book.avatarUrl" :src="book.avatarUrl" :alt="book.owner" class="meta-av">
          <div v-else class="meta-av meta-av-fallback" aria-hidden="true">{{ ownerInitial }}</div>
          <div class="meta-info">
            <span class="meta-nick">@{{ book.owner }}</span><br>{{ book.city }}
          </div>
        </div>
      </slot>
    </div>
  </div>
</template>

<style scoped>
  .cover-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 16px;
    text-align: center;
    color: rgba(255, 255, 255, 0.92);
  }

  .cover-fallback-title {
    font-family: 'Playfair Display', serif;
    font-weight: 700;
    font-size: 15px;
    line-height: 1.25;
  }

  .cover-fallback-author {
    font-size: 12px;
    opacity: 0.8;
  }

  .meta-av-fallback {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: var(--tag-bg, #f0e8df);
    color: var(--text-2, #7a6652);
    font-weight: 600;
    font-size: 13px;
  }
</style>
