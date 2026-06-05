<script setup>
  /**
   * Trasa: /profil (name: profile) — wymaga zalogowania (meta.requiresAuth).
   * Cel: wyświetlanie i edycja danych konta, zmiana hasła.
   * API: usersApi.getMe (ładowanie profilu; sync z auth store).
   * Komponenty: PersonalData (dane osobowe), Security (hasło).
   */
  import { ref, computed, onMounted } from 'vue';
  import PersonalData from '../components/PersonalData.vue';
  import Security from '../components/Security.vue';
  import { usersApi } from '../lib/users';
  import { mapUserToProfile } from '../lib/mappers';
  import { ApiError } from '../lib/client';
  import { useAuthStore } from '../stores/auth';

  const user = ref(null);
  const profileLoading = ref(true);
  const profileError = ref('');
  const auth = useAuthStore();

  const memberSince = computed(() => {
    if (!user.value?.createdAt) return '';
    const date = new Date(user.value.createdAt);
    const month = date.toLocaleDateString('pl-PL', { month: 'long' });
    const year = date.getFullYear();
    return `Członek od ${month} ${year}`;
  });

  async function loadProfile() {
    profileLoading.value = true;
    profileError.value = '';
    try {
      const data = await usersApi.getMe();
      auth.syncFromProfile(data);
      user.value = mapUserToProfile(data);
    } catch (err) {
      profileError.value = err instanceof ApiError ? err.message : 'Nie udało się załadować profilu';
    } finally {
      profileLoading.value = false;
    }
  }

  onMounted(loadProfile);

  const updateUser = (updatedData) => {
    user.value = updatedData;
  };
</script>

<template>
  <div class="profile-view-wrapper">
    <!-- SEKCJA: Stany ładowania i błędu profilu -->
    <p v-if="profileLoading" class="profile-status">Ładowanie profilu…</p>
    <p v-else-if="profileError" class="profile-status profile-status--error">{{ profileError }}</p>

    <template v-else-if="user">
      <!-- SEKCJA: Baner hero z avatarem i dekoracyjną półką -->
      <div class="profile-hero">
        <div class="hero-pattern"></div>
        <svg class="hero-shelves" viewBox="0 0 1200 188" fill="none" preserveAspectRatio="xMidYMax slice" aria-hidden="true">
          <rect x="0" y="60" width="1200" height="3" fill="white" opacity=".12" />
          <rect x="0" y="130" width="1200" height="3" fill="white" opacity=".12" />
          <rect x="60" y="20" width="18" height="42" rx="2" fill="white" opacity=".08" />
          <rect x="82" y="28" width="12" height="34" rx="2" fill="white" opacity=".06" />
          <rect x="98" y="14" width="20" height="48" rx="2" fill="white" opacity=".1" />
          <rect x="122" y="24" width="15" height="38" rx="2" fill="white" opacity=".07" />
          <rect x="141" y="18" width="22" height="44" rx="2" fill="white" opacity=".09" />
          <rect x="167" y="30" width="10" height="32" rx="2" fill="white" opacity=".05" />
          <rect x="181" y="22" width="17" height="40" rx="2" fill="white" opacity=".08" />
          <rect x="240" y="16" width="20" height="46" rx="2" fill="white" opacity=".1" />
          <rect x="264" y="26" width="13" height="36" rx="2" fill="white" opacity=".06" />
          <rect x="281" y="12" width="24" height="50" rx="2" fill="white" opacity=".08" />
          <rect x="309" y="20" width="16" height="42" rx="2" fill="white" opacity=".07" />
          <rect x="420" y="18" width="19" height="44" rx="2" fill="white" opacity=".09" />
          <rect x="443" y="24" width="14" height="38" rx="2" fill="white" opacity=".06" />
          <rect x="461" y="10" width="21" height="52" rx="2" fill="white" opacity=".1" />
          <rect x="620" y="15" width="18" height="47" rx="2" fill="white" opacity=".08" />
          <rect x="642" y="22" width="15" height="40" rx="2" fill="white" opacity=".07" />
          <rect x="661" y="18" width="22" height="44" rx="2" fill="white" opacity=".09" />
          <rect x="800" y="20" width="16" height="42" rx="2" fill="white" opacity=".06" />
          <rect x="820" y="14" width="20" height="48" rx="2" fill="white" opacity=".1" />
          <rect x="1000" y="16" width="19" height="46" rx="2" fill="white" opacity=".08" />
          <rect x="1023" y="22" width="14" height="40" rx="2" fill="white" opacity=".07" />
          <rect x="80" y="90" width="20" height="42" rx="2" fill="white" opacity=".08" />
          <rect x="104" y="98" width="14" height="34" rx="2" fill="white" opacity=".06" />
          <rect x="122" y="84" width="18" height="48" rx="2" fill="white" opacity=".09" />
          <rect x="300" y="88" width="22" height="44" rx="2" fill="white" opacity=".08" />
          <rect x="326" y="94" width="12" height="38" rx="2" fill="white" opacity=".05" />
          <rect x="500" y="86" width="20" height="46" rx="2" fill="white" opacity=".1" />
          <rect x="524" y="92" width="16" height="40" rx="2" fill="white" opacity=".07" />
          <rect x="700" y="88" width="18" height="44" rx="2" fill="white" opacity=".09" />
          <rect x="722" y="82" width="22" height="50" rx="2" fill="white" opacity=".08" />
          <rect x="900" y="90" width="15" height="42" rx="2" fill="white" opacity=".06" />
          <rect x="919" y="84" width="20" height="48" rx="2" fill="white" opacity=".1" />
          <rect x="1100" y="88" width="18" height="44" rx="2" fill="white" opacity=".08" />
          <rect x="1122" y="94" width="14" height="38" rx="2" fill="white" opacity=".06" />
        </svg>
        <div class="hero-content">
          <div class="profile-avatar-wrap">
            <img :src="user.avatar" :alt="`${user.name} ${user.surname}`" class="profile-avatar">
            <div class="avatar-online"></div>
          </div>
        </div>
      </div>

      <!-- SEKCJA: Imię, nick, lokalizacja i statystyki użytkownika -->
      <div class="profile-info-section">
        <div class="profile-info-inner">
          <h1 class="profile-name">{{ user.name }} {{ user.surname }}</h1>
          <div class="profile-nick">@{{ user.nick }}</div>
          <div class="profile-meta">
            <div class="meta-item">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" /></svg>
              <span>{{ user.city || 'Brak lokalizacji' }}</span>
            </div>
            <div v-if="memberSince" class="meta-item">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></svg>
              {{ memberSince }}
            </div>
          </div>
          <div class="profile-stats">
            <div class="pstat"><strong>{{ user.booksCount }}</strong>książek</div>
            <div class="pstat-div"></div>
            <div class="pstat"><strong>{{ user.transactionsCount }}</strong>transakcji</div>
          </div>
        </div>
      </div>

      <!-- SEKCJA: Formularze edycji — dane osobowe i bezpieczeństwo -->
      <div class="profile-page">
        <PersonalData :user="user" @update:user="updateUser" @saved="loadProfile" />
        <Security />
      </div>
    </template>
  </div>
</template>

<style scoped>
  .profile-status {
    text-align: center;
    padding: 48px 24px;
    color: var(--text-2);
  }

  .profile-status--error {
    color: var(--danger, #c0392b);
  }
</style>
