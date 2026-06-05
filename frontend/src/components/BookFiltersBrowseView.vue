<script setup>
  /**
   * Boczny panel filtrów na stronie przeglądania ofert.
   * Użycie: BrowseView.vue — sidebar obok siatki BookCard.
   * Props: search, city, types (Set), genres (Set), availableGenres, counts.
   * Emity: update:search, update:city, toggle-type, toggle-genre, reset.
   */
  import DataList from './DataList.vue';

  defineProps({
    search: String,
    city: String,
    types: Set,
    genres: Set,
    availableGenres: Array,
    counts: Object,
  });

  defineEmits(['update:search', 'update:city', 'toggle-type', 'toggle-genre', 'reset']);

  const offerTypeFilters = [
    { key: 'loan', label: 'Do wypożyczenia', badgeClass: 'flt-loan' },
    { key: 'swap', label: 'Do wymiany', badgeClass: 'flt-swap' },
    { key: 'sale', label: 'Do sprzedaży', badgeClass: 'flt-sale' },
  ];

  const genreInputId = (name) => `g_${name.replace(/\s/g, '_')}`;
</script>

<template>
  <!-- Sidebar: szukaj, typ oferty, gatunki (DataList), miasto -->
  <aside class="sidebar">
    <div class="filter-header">
      <h3>Filtry</h3>
      <button class="filter-reset" @click="$emit('reset')">Resetuj</button>
    </div>

    <div class="filter-section">
      <div class="filter-section-title">Szukaj</div>
      <div class="input-wrap">
        <span class="input-icon">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
            <circle cx="11" cy="11" r="7" />
            <line x1="16.5" y1="16.5" x2="22" y2="22" />
          </svg>
        </span>
        <input type="text"
               class="filter-input"
               :value="search"
               @input="$emit('update:search', $event.target.value)"
               placeholder="Tytuł, autor, ISBN…">
      </div>
    </div>

    <div class="filter-section">
      <div class="filter-section-title">Typ oferty</div>
      <DataList
        :items="offerTypeFilters"
        item-key="key"
        list-class="check-list"
        item-class="check-item"
      >
        <template #default="{ item: offerType }">
          <input
            type="checkbox"
            :id="`f_${offerType.key}`"
            :checked="types.has(offerType.key)"
            @change="$emit('toggle-type', offerType.key)"
          >
          <label class="check-label" :for="`f_${offerType.key}`">{{ offerType.label }}</label>
          <span :class="['flt-badge', offerType.badgeClass]">{{ counts[offerType.key] }}</span>
        </template>
      </DataList>
    </div>

    <div class="filter-section">
      <div class="filter-section-title">Gatunek</div>
      <DataList
        :items="availableGenres"
        item-key="name"
        list-class="check-list"
        item-class="check-item"
      >
        <template #default="{ item: genre }">
          <input
            type="checkbox"
            :id="genreInputId(genre.name)"
            :checked="genres.has(genre.name)"
            @change="$emit('toggle-genre', genre.name)"
          >
          <label class="check-label" :for="genreInputId(genre.name)">{{ genre.name }}</label>
          <span class="flt-badge flt-cnt">{{ genre.count }}</span>
        </template>
      </DataList>
    </div>

    <div class="filter-section">
      <div class="filter-section-title">Miasto</div>
      <div class="input-wrap">
        <span class="input-icon">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
            <circle cx="12" cy="10" r="3" />
          </svg>
        </span>
        <input type="text"
               class="filter-input"
               :value="city"
               @input="$emit('update:city', $event.target.value)"
               placeholder="np. Warszawa">
      </div>
    </div>
  </aside>
</template>
