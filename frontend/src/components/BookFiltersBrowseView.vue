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

<script setup>
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

<style scoped>
  .sidebar {
    position: sticky;
    top: 14px;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    max-height: calc(100vh - 28px);
    overflow-y: auto;
  }

    .sidebar::-webkit-scrollbar {
      width: 4px;
    }

    .sidebar::-webkit-scrollbar-thumb {
      background: var(--border);
      border-radius: 2px;
    }

  .filter-header {
    padding: 14px 18px;
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-shrink: 0;
  }

    .filter-header h3 {
      font-size: 12px;
      font-weight: 700;
      color: var(--text-1);
      text-transform: uppercase;
      letter-spacing: .6px;
    }

  .filter-reset {
    background: none;
    border: none;
    font-size: 12px;
    color: var(--text-3);
    cursor: pointer;
    font-family: 'DM Sans', sans-serif;
    transition: color .2s;
  }

    .filter-reset:hover {
      color: var(--danger);
    }

  .filter-section {
    padding: 14px 18px;
    border-bottom: 1px solid var(--border);
  }

    .filter-section:last-child {
      border-bottom: none;
    }

  .filter-section-title {
    font-size: 10px;
    font-weight: 700;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .7px;
    margin-bottom: 11px;
  }

  .input-wrap {
    position: relative;
  }

  .input-icon {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-3);
    pointer-events: none;
    display: flex;
  }

  .filter-input {
    width: 100%;
    padding: 7px 11px 7px 32px;
    border: 1.5px solid var(--border);
    border-radius: var(--r-input);
    font-family: 'DM Sans', sans-serif;
    font-size: 13px;
    color: var(--text-1);
    background: var(--bg);
    outline: none;
    transition: border-color .2s;
  }

    .filter-input::placeholder {
      color: var(--text-3);
    }

    .filter-input:focus {
      border-color: var(--primary);
      background: #fff;
    }

  .check-list {
    display: flex;
    flex-direction: column;
    gap: 7px;
    list-style: none;
  }

  .check-item {
    display: flex;
    align-items: center;
    gap: 9px;
    cursor: pointer;
    user-select: none;
  }

    .check-item input[type="checkbox"] {
      appearance: none;
      width: 16px;
      height: 16px;
      flex-shrink: 0;
      border: 1.5px solid var(--border);
      border-radius: 4px;
      background: var(--bg);
      cursor: pointer;
      transition: all .15s;
      position: relative;
    }

      .check-item input[type="checkbox"]:checked {
        background: var(--primary);
        border-color: var(--primary);
      }

        .check-item input[type="checkbox"]:checked::after {
          content: '';
          position: absolute;
          top: 1px;
          left: 4px;
          width: 5px;
          height: 8px;
          border: 2px solid white;
          border-top: none;
          border-left: none;
          transform: rotate(45deg);
        }

  .check-label {
    font-size: 13px;
    color: var(--text-2);
    flex: 1;
    line-height: 1;
  }

  .flt-badge {
    padding: 2px 7px;
    border-radius: 10px;
    font-size: 10px;
    font-weight: 600;
  }

  .flt-loan {
    background: var(--primary);
    color: white;
  }

  .flt-swap {
    background: var(--accent);
    color: var(--text-1);
  }

  .flt-sale {
    background: var(--blue);
    color: white;
  }

  .flt-cnt {
    background: var(--tag-bg);
    color: var(--text-2);
    font-weight: 400;
  }
</style>
