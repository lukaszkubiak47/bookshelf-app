<script setup>
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '../stores/auth'

  const router = useRouter()
  const auth = useAuthStore()

  const logout = () => {
    auth.logout()
    router.push('/')
  }
</script>

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
      <li v-if="auth.isLoggedIn"><RouterLink to="/polka">Moja półka</RouterLink></li>
    </ul>
    <div class="nav-actions">
      <template v-if="auth.isLoggedIn">
        <RouterLink to="/profil" :style="{ color: 'var(--primary)', fontWeight: 600, fontSize: '14px', textDecoration: 'none' }">
          @{{ auth.user?.username }}
        </RouterLink>
        <button type="button" class="btn btn-outline" @click="logout">Wyloguj</button>
      </template>
      <template v-else>
        <RouterLink to="/logowanie?tab=login" class="btn btn-outline">Zaloguj się</RouterLink>
        <RouterLink to="/logowanie?tab=register" class="btn btn-filled">Dołącz</RouterLink>
      </template>
    </div>
  </nav>
</template>
