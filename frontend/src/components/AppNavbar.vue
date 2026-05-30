<template>
  <nav class="navbar">
    <RouterLink to="/" class="nav-logo">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
      </svg>
      Bookshelf
    </RouterLink>
    <ul class="nav-links">
      <li><RouterLink to="/">Strona główna</RouterLink></li>
      <li><RouterLink to="/przegladanie">Przeglądaj książki</RouterLink></li>
    </ul>
    <div class="nav-actions">
      <RouterLink to="/logowanie" class="btn btn-filled">Dołącz</RouterLink>
    </div>
  </nav>
</template>
