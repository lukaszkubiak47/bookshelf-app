<script setup>
  /** Panel filtrów bocznych na stronie Przeglądaj (typ, gatunek, miasto, szukaj). */
  defineProps({
    search: String,
    city: String,
    types: Set,
    genres: Set,
    availableGenres: Array,
    counts: Object
  });

  defineEmits(['update:search', 'update:city', 'toggle-type', 'toggle-genre', 'reset']);
</script>

<template>
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
      <ul class="check-list">
        <li class="check-item">
          <input type="checkbox" id="fLoan" :checked="types.has('loan')" @change="$emit('toggle-type', 'loan')">
          <label class="check-label" for="fLoan">Do wypożyczenia</label>
          <span class="flt-badge flt-loan">{{ counts.loan }}</span>
        </li>
        <li class="check-item">
          <input type="checkbox" id="fSwap" :checked="types.has('swap')" @change="$emit('toggle-type', 'swap')">
          <label class="check-label" for="fSwap">Do wymiany</label>
          <span class="flt-badge flt-swap">{{ counts.swap }}</span>
        </li>
        <li class="check-item">
          <input type="checkbox" id="fSale" :checked="types.has('sale')" @change="$emit('toggle-type', 'sale')">
          <label class="check-label" for="fSale">Do sprzedaży</label>
          <span class="flt-badge flt-sale">{{ counts.sale }}</span>
        </li>
      </ul>
    </div>

    <div class="filter-section">
      <div class="filter-section-title">Gatunek</div>
      <ul class="check-list">
        <li v-for="genre in availableGenres" :key="genre.name" class="check-item">
          <input type="checkbox"
                 :id="'g_' + genre.name.replace(/\s/g,'_')"
                 :checked="genres.has(genre.name)"
                 @change="$emit('toggle-genre', genre.name)">
          <label class="check-label" :for="'g_' + genre.name.replace(/\s/g,'_')">{{ genre.name }}</label>
          <span class="flt-badge flt-cnt">{{ genre.count }}</span>
        </li>
      </ul>
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
