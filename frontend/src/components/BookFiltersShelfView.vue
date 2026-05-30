<script setup lang="ts">
  // Automatyczne dwukierunkowe bindowanie w Vue 3
  const statusFilter = defineModel<string>('statusFilter');
  const typeFilter = defineModel<string>('typeFilter');
</script>

<template>
  <div class="filters" id="filtersRow">
    <select v-model="statusFilter" class="filter-select">
      <option value="">Wszystkie statusy</option>
      <option value="pending">Oczekujące</option>
      <option value="accepted">Zaakceptowane</option>
      <option value="done">Zakończone</option>
      <option value="rejected">Odrzucone</option>
    </select>

    <select v-model="typeFilter" class="filter-select">
      <option value="">Wszystkie typy</option>
      <option value="loan">Wypożyczenie</option>
      <option value="swap">Wymiana</option>
      <option value="sale">Sprzedaż</option>
    </select>
  </div>
</template>

<style scoped>
  .filters {
    display: flex;
    gap: 8px;
  }

  .filter-select {
    padding: 7px 10px;
    border: 1.5px solid var(--border);
    border-radius: var(--r-btn);
    font-family: 'DM Sans', sans-serif;
    font-size: 13px;
    color: var(--text-1);
    background: var(--surface);
    outline: none;
    cursor: pointer;
    transition: border-color .2s;
  }

    .filter-select:focus {
      border-color: var(--primary);
    }
</style>
