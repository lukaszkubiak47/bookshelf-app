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
