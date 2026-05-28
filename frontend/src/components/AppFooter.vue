<script setup>

</script>

<template>
    <footer class="footer">
      <a href="#" class="footer-logo">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
          <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
          <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
          <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
        </svg>
        Bookshelf
      </a>
      <span class="footer-copy">© 2025 Bookshelf. Wszystkie prawa zastrzeżone.</span>
    </footer>
</template>

<style scoped>
  /* ══════════ FOOTER ══════════ */
  .footer {
    background: var(--surface);
    border-top: 1px solid var(--border);
    padding: 20px 48px;
    display: flex;
    align-items: center;
    justify-content: space-between;
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
</style>
