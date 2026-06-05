<script setup>
  /**
   * Stopka aplikacji z logo i copyright.
   * Użycie: App.vue — globalnie pod router-view.
   * Pełna stopka ukryta na /logowanie (zostaje tylko tekst copyright).
   * Brak propsów i emitów; route.path steruje widocznością.
   */
  import { useRoute } from 'vue-router';
  const route = useRoute();
</script>

<template>
  <footer v-if="route.path !== '/logowanie'" class="footer">
    <RouterLink to="/" class="footer-logo">
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
      </svg>
      Bookshelf
    </RouterLink>
    <span class="footer-copy">© 2025 Bookshelf. Wszystkie prawa zastrzeżone.</span>
  </footer>

  <span v-else class="footer-copy-auth">
    © 2025 Bookshelf. Wszystkie prawa zastrzeżone.
  </span>
</template>
