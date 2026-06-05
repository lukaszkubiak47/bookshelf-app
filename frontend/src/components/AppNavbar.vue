<script setup>
  /**
   * Górne menu nawigacji — linki, stan zalogowania, menu użytkownika.
   * Użycie: App.vue — ukryte na /logowanie.
   * Brak propsów; stan z authStore (avatar, username, isLoggedIn).
   * Mobilne menu hamburger + dropdown profilu z wylogowaniem.
   */
  import { ref, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { useAuthStore } from '../stores/auth';
  import { avatarImageUrl } from '../lib/mappers';

  const auth = useAuthStore();
  const router = useRouter();

  const menuOpen = ref(false);
  const mobileNavOpen = ref(false);

  const displayName = computed(() => {
    const u = auth.user;
    if (!u) return '';
    return `${u.firstName} ${u.lastName}`.trim();
  });

  const avatarSrc = computed(() => {
    if (!auth.user) return '';
    return avatarImageUrl({ id: auth.user.id, avatarUrl: auth.user.avatarUrl });
  });

  const toggleMenu = (event) => {
    event.stopPropagation();
    menuOpen.value = !menuOpen.value;
  };

  const closeMenu = () => {
    menuOpen.value = false;
  };

  const logout = () => {
    closeMenu();
    mobileNavOpen.value = false;
    auth.logout();
    router.push('/');
  };

  const toggleMobileNav = () => {
    mobileNavOpen.value = !mobileNavOpen.value;
  };

  const closeMobileNav = () => {
    mobileNavOpen.value = false;
  };

</script>

<template>
  <!-- Nawigacja: logo, linki (desktop/mobile), akcje auth lub avatar -->
  <nav class="navbar">
    <RouterLink to="/" class="nav-logo">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
      </svg>
      Bookshelf
    </RouterLink>
    <ul id="nav-links" class="nav-links" :class="{ 'is-open': mobileNavOpen }">
      <li><RouterLink to="/" @click="closeMobileNav">Strona główna</RouterLink></li>
      <li><RouterLink to="/przegladanie" @click="closeMobileNav">Przeglądaj książki</RouterLink></li>
      <template v-if="auth.isLoggedIn">
        <li><RouterLink to="/polka" @click="closeMobileNav">Moja półka</RouterLink></li>
      </template>
    </ul>

    <!-- Zalogowany: dropdown profilu | Gość: przyciski logowania i rejestracji -->
    <div class="nav-actions">
      <div v-if="auth.isLoggedIn"
           v-click-outside="closeMenu"
           class="nav-avatar-wrap"
           :class="{ open: menuOpen }">
        <button type="button" class="nav-avatar-btn" aria-haspopup="true" :aria-expanded="menuOpen" @click="toggleMenu">
          <img :key="auth.user?.avatarUrl ?? auth.user?.id" :src="avatarSrc" :alt="displayName" class="nav-avatar">
          <span class="nav-avatar-name">@{{ auth.user?.username }}</span>
          <span class="nav-chevron" aria-hidden="true">
            <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M3 5l4 4 4-4" />
            </svg>
          </span>
        </button>
        <div class="nav-dropdown">
          <div class="dd-head">
            <strong>{{ displayName }}</strong>
            <span>
              <a :href="`mailto:${auth.user?.email}`">{{ auth.user?.email }}</a>
            </span>
          </div>
          <RouterLink to="/profil" class="dd-item" @click="closeMenu(); closeMobileNav()">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <circle cx="12" cy="8" r="4" />
              <path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" />
            </svg>
            Mój profil
          </RouterLink>
          <div class="dd-divider"></div>
          <button type="button" class="dd-item danger" @click="logout">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <polyline points="16 17 21 12 16 7" />
              <line x1="21" y1="12" x2="9" y2="12" />
            </svg>
            Wyloguj się
          </button>
        </div>
      </div>
      <template v-else>
        <RouterLink to="/logowanie?tab=login" class="btn btn-outline">Zaloguj się</RouterLink>
        <RouterLink to="/logowanie?tab=register" class="btn btn-filled">Dołącz</RouterLink>
      </template>
    </div>

    <button
      type="button"
      class="nav-menu-btn"
      :aria-expanded="mobileNavOpen"
      aria-controls="nav-links"
      aria-label="Menu nawigacji"
      @click="toggleMobileNav"
    >
      <svg v-if="!mobileNavOpen" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
        <line x1="4" y1="7" x2="20" y2="7" />
        <line x1="4" y1="12" x2="20" y2="12" />
        <line x1="4" y1="17" x2="20" y2="17" />
      </svg>
      <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
        <line x1="6" y1="6" x2="18" y2="18" />
        <line x1="18" y1="6" x2="6" y2="18" />
      </svg>
    </button>
  </nav>
</template>
