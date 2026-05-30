<template>
  <div class="book-card" @click="$emit('select', book.id)">
    <div class="cover-wrap">
      <img :src="`https://picsum.photos/180/240?random=${book.img}`" :alt="book.title" class="cover-img" loading="lazy">
      <span :class="['bk-badge', badgeClass]">{{ badgeText }}</span>
      <button class="see-btn">Zobacz ofertę</button>
    </div>
    <div class="card-body">
      <h3 class="card-title">{{ book.title }}</h3>
      <p class="card-author">{{ book.author }}</p>
      <div class="card-meta">
        <img :src="`https://i.pravatar.cc/40?img=${book.av}`" :alt="book.owner" class="meta-av">
        <div class="meta-info">
          <span class="meta-nick">@{{ book.owner }}</span><br>{{ book.city }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { computed } from 'vue';

  const props = defineProps({
    book: {
      type: Object,
      required: true
    }
  });

  defineEmits(['select']);

  const badgeClass = computed(() => {
    const mapping = { loan: 'badge-loan', swap: 'badge-swap', sale: 'badge-sale' };
    return mapping[props.book.type] || '';
  });

  const badgeText = computed(() => {
    const mapping = { loan: 'Do wypożyczenia', swap: 'Do wymiany', sale: 'Do sprzedaży' };
    return mapping[props.book.type] || '';
  });
</script>

<style scoped>
  .book-card {
    background: var(--surface);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    overflow: hidden;
    cursor: pointer;
    transition: all .22s ease;
    border: 1px solid transparent;
  }

    .book-card:hover {
      transform: translateY(-3px);
      box-shadow: var(--shadow-h);
      border-color: var(--border);
    }

  .cover-wrap {
    position: relative;
    aspect-ratio: 3/4;
    overflow: hidden;
  }

  .cover-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
    transition: transform .3s ease;
  }

  .book-card:hover .cover-img {
    transform: scale(1.05);
  }

  .bk-badge {
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

  .badge-loan {
    background: var(--primary);
    color: white;
  }

  .badge-swap {
    background: var(--accent);
    color: var(--text-1);
  }

  .badge-sale {
    background: var(--blue);
    color: white;
  }

  .see-btn {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    padding: 10px 12px;
    background: rgba(45,106,79,.92);
    backdrop-filter: blur(3px);
    color: white;
    border: none;
    cursor: pointer;
    font-family: 'DM Sans', sans-serif;
    font-size: 13px;
    font-weight: 500;
    opacity: 0;
    transform: translateY(100%);
    transition: opacity .22s, transform .22s;
  }

  .book-card:hover .see-btn {
    opacity: 1;
    transform: translateY(0);
  }

  .card-body {
    padding: 10px 12px 13px;
  }

  .card-title {
    font-family: 'Playfair Display', serif;
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

  .card-author {
    font-size: 12px;
    color: var(--text-2);
    margin-bottom: 6px;
  }

  .card-meta {
    display: flex;
    align-items: center;
    gap: 8px;
    padding-top: 8px;
    border-top: 1px solid var(--border);
  }

  .meta-av {
    width: 24px;
    height: 24px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid var(--border);
    flex-shrink: 0;
  }

  .meta-info {
    font-size: 11px;
    color: var(--text-3);
    line-height: 1.35;
  }

  .meta-nick {
    font-weight: 600;
    color: var(--text-2);
  }

  /* Obsługa widoku listy z zachowaniem hermetyzacji stylów scoped */
  :deep(.list) .book-card {
    display: grid;
    grid-template-columns: 110px 1fr;
  }

  :deep(.list) .cover-wrap {
    aspect-ratio: unset;
    height: 148px;
  }

  :deep(.list) .card-body {
    padding: 14px 16px;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  :deep(.list) .card-title {
    font-size: 15px;
    -webkit-line-clamp: 1;
  }
</style>
