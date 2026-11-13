<script setup>
  /**
   * Górne menu nawigacji — linki, stan zalogowania, menu użytkownika.
   * Użycie: App.vue — ukryte na /logowanie.
   * Brak propsów; stan z authStore (avatar, username, isLoggedIn).
   * Mobilne menu hamburger + dropdown profilu z wylogowaniem.
   * Dzwonek powiadomień (Cosmos DB przez /api/notifications), odświeżany co 45 s, gdy zalogowany.
   */
  import { ref, computed, watch, onBeforeUnmount } from 'vue';
  import { useRouter } from 'vue-router';
  import { useAuthStore } from '../stores/auth';
  import { avatarImageUrl } from '../lib/mappers';
  import { notificationsApi } from '../lib/notifications';
  import { notificationLabels, labelFor } from '../lib/transactionLabels';

  const auth = useAuthStore();
  const router = useRouter();

  const menuOpen = ref(false);
  const mobileNavOpen = ref(false);

  /** Powiadomienia: lista, licznik nieprzeczytanych i odświeżanie w tle */
  const NOTIFICATIONS_REFRESH_MS = 45_000;
  const notifOpen = ref(false);
  const notifications = ref([]);
  const unreadCount = ref(0);
  let notifTimer = null;

  const unreadBadge = computed(() => (unreadCount.value > 9 ? '9+' : String(unreadCount.value)));

  async function loadNotifications() {
    try {
      const res = await notificationsApi.getAll();
      notifications.value = res.items;
      unreadCount.value = res.unreadCount;
    } catch {
      // Brak sieci albo wygasła sesja: zostawiamy poprzedni stan, kolejna próba za 45 s.
    }
  }

  function stopNotifications() {
    if (notifTimer) clearInterval(notifTimer);
    notifTimer = null;
  }

  // Start po zalogowaniu, stop i wyczyszczenie po wylogowaniu
  watch(() => auth.isLoggedIn, (loggedIn) => {
    stopNotifications();
    if (loggedIn) {
      loadNotifications();
      notifTimer = setInterval(() => {
        if (!document.hidden) loadNotifications(); // karta w tle: nie odpytujemy API
      }, NOTIFICATIONS_REFRESH_MS);
    } else {
      notifications.value = [];
      unreadCount.value = 0;
      notifOpen.value = false;
    }
  }, { immediate: true });

  onBeforeUnmount(stopNotifications);

  const toggleNotifications = (event) => {
    event.stopPropagation();
    notifOpen.value = !notifOpen.value;
    if (notifOpen.value) {
      menuOpen.value = false;
      loadNotifications();
    }
  };

  const closeNotifications = () => {
    notifOpen.value = false;
  };

  async function openNotification(n) {
    if (!n.isRead) {
      n.isRead = true;
      unreadCount.value = Math.max(0, unreadCount.value - 1);
      try {
        await notificationsApi.markRead(n.id);
      } catch {
        // Nie udało się zapisać: przy następnym odświeżeniu wróci jako nieprzeczytane.
      }
    }
    closeNotifications();
    closeMobileNav();
    router.push('/polka');
  }

  const formatNotificationDate = (iso) =>
    new Date(iso).toLocaleString('pl-PL', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });

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
    if (menuOpen.value) notifOpen.value = false;
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
           v-click-outside="closeNotifications"
           class="nav-notif-wrap"
           :class="{ open: notifOpen }">
        <button type="button"
                class="nav-notif-btn"
                aria-haspopup="true"
                :aria-expanded="notifOpen"
                :aria-label="unreadCount ? `Powiadomienia, nieprzeczytane: ${unreadCount}` : 'Powiadomienia'"
                @click="toggleNotifications">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
          </svg>
          <span v-if="unreadCount > 0" class="nav-notif-badge" aria-hidden="true">{{ unreadBadge }}</span>
        </button>
        <div class="nav-dropdown nav-notif-panel">
          <div class="dd-head">
            <strong>Powiadomienia</strong>
          </div>
          <p v-if="notifications.length === 0" class="notif-empty">Brak powiadomień</p>
          <ul v-else class="notif-list">
            <li v-for="n in notifications" :key="n.id">
              <button type="button" class="notif-item" :class="{ unread: !n.isRead }" @click="openNotification(n)">
                <span class="notif-text">{{ labelFor(notificationLabels, n.type) }}</span>
                <span v-if="n.bookTitle" class="notif-book">„{{ n.bookTitle }}”</span>
                <span class="notif-date">{{ formatNotificationDate(n.createdAt) }}</span>
              </button>
            </li>
          </ul>
        </div>
      </div>
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
