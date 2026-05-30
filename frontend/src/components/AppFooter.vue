<script setup>
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

<style scoped>
  /* ══════════ STANDARDOWY FOOTER ══════════ */
  .footer {
    background: var(--surface);
    border-top: 1px solid var(--border);
    padding: 20px 48px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    box-sizing: border-box;
  }

  .footer-logo {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--primary);
    font-family: 'Playfair Display', serif;
    font-size: 17px;
    font-weight: 700;
    text-decoration: none;
  }

  .footer-copy {
    font-size: 13px;
    color: var(--text-3);
  }

  /* ══════════ SAM TEKST DLA LOGOWANIA (BEZ KONTENERA) ══════════ */
  .footer-copy-auth {
    position: fixed; /* Tekst zostaje przypięty bezpośrednio do okna przeglądarki */
    bottom: 24px; /* Odstęp od dolnej krawędzi */
    right: 48px; /* Odstęp od prawej krawędzi (taki sam jak padding logo) */
    font-size: 12px;
    color: var(--text-3);
    z-index: 10; /* Upewnia się, że tekst jest nad tłem strony */
    pointer-events: none; /* Sprawia, że tekst nie blokuje kliknięć pod spodem */
  }
</style>
