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
