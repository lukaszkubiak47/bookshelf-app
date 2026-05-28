<script setup>

</script>

<template>
  <nav class="navbar">
    <a href="#" class="nav-logo">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
      </svg>
      Bookshelf
    </a>
    <ul class="nav-links">
      <li><a href="#" class="active">Strona główna</a></li>
      <li><a href="#">Przeglądaj książki</a></li>
    </ul>
    <div class="nav-actions">
      <button class="btn btn-filled">Dołącz</button>
    </div>
  </nav>
</template>
<style scoped>
  /* ══════════ NAVBAR ══════════ */
  .navbar {
    position: sticky;
    top: 0;
    z-index: 1000;
    height: 60px;
    background: var(--surface);
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    padding: 0 48px;
    gap: 40px;
  }

  .nav-logo {
    display: flex;
    align-items: center;
    gap: 9px;
    text-decoration: none;
    color: var(--primary);
    font-family: 'Playfair Display', serif;
    font-size: 20px;
    font-weight: 700;
    flex-shrink: 0;
  }

  .nav-links {
    display: flex;
    align-items: center;
    gap: 28px;
    list-style: none;
    flex: 1;
  }

    .nav-links a {
      text-decoration: none;
      color: var(--text-2);
      font-size: 14px;
      font-weight: 400;
      transition: color .2s;
    }

      .nav-links a:hover {
        color: var(--text-1);
      }


      .nav-links a.active {
        color: var(--primary);
        font-weight: 600;
      }

  .nav-actions {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-shrink: 0;
  }
</style>

<!--
<script setup>
  import { ref } from 'vue'
  // Zakładamy podstawową strukturę authStore (możesz dostosować ścieżkę importu)
  import { useAuthStore } from '@/stores/authStore'

  const authStore = useAuthStore()
  const isDropdownOpen = ref(false)

  const toggleDropdown = () => {
    isDropdownOpen.value = !isDropdownOpen.value
  }

  const closeDropdown = () => {
    isDropdownOpen.value = false
  }

  const handleLogout = () => {
    closeDropdown()
    authStore.logout() // Zakładana metoda wylogowania w Twoim storze
  }

  // Lokalna dyrektywa v-click-outside obsługująca kliknięcie poza element oraz klawisz Escape
  const vClickOutside = {
    mounted(el, binding) {
      el.clickOutsideEvent = (event) => {
        // Sprawdzamy czy kliknięcie nastąpiło poza elementem i jego dziećmi
        if (!(el === event.target || el.contains(event.target))) {
          binding.value()
        }
      }
      el.keydownEvent = (event) => {
        if (event.key === 'Escape') {
          binding.value()
        }
      }
      document.addEventListener('click', el.clickOutsideEvent)
      document.addEventListener('keydown', el.keydownEvent)
    },
    unmounted(el) {
      document.removeEventListener('click', el.clickOutsideEvent)
      document.removeEventListener('keydown', el.keydownEvent)
    }
  }
</script>

<template>
  <nav class="navbar">
    <a href="#" class="nav-logo">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="14" y="3" width="8" height="18" rx="1.5" fill="currentColor" opacity=".55" />
        <rect x="7" y="3" width="10" height="18" rx="1.5" fill="currentColor" />
      </svg>
      Bookshelf
    </a>

    <ul class="nav-links">
      <li><a href="#" class="active">Strona główna</a></li>
      <li><a href="#">Przeglądaj książki</a></li>
      <li v-if="authStore.isAuthenticated"><a href="#">Moja półka</a></li>
    </ul>

    <div class="nav-actions">

      <template v-if="!authStore.isAuthenticated">
        <button class="btn btn-outline" @click="$emit('login')">Zaloguj się</button>
        <button class="btn btn-filled" @click="$emit('register')">Dołącz</button>
      </template>

      <div v-else
           class="nav-avatar-wrap"
           :class="{ open: isDropdownOpen }"
           v-click-outside="closeDropdown">
        <button class="nav-avatar-btn" @click="toggleDropdown">
          <img :src="authStore.user.avatarUrl || 'https://i.pravatar.cc/40?img=15'" alt="avatar" class="nav-avatar">
          <span class="nav-avatar-name">@{{ authStore.user.username }}</span>
          <span class="nav-chevron">
            <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M3 5l4 4 4-4" />
            </svg>
          </span>
        </button>

        <div class="nav-dropdown">
          <div class="dd-head">
            <strong>{{ authStore.user.fullName }}</strong>
            <span>{{ authStore.user.email }}</span>
          </div>

          <a href="#" class="dd-item" @click="closeDropdown">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <circle cx="12" cy="8" r="4" />
              <path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" />
            </svg>
            Mój profil
          </a>

          <div class="dd-divider"></div>

          <a href="#" class="dd-item danger" @click.prevent="handleLogout">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <polyline points="16 17 21 12 16 7" />
              <line x1="21" y1="12" x2="9" y2="12" />
            </svg>
            Wyloguj się
          </a>
        </div>
      </div>

    </div>
  </nav>
</template>

<style scoped>
  /* ══ NAVBAR ══ */
  .navbar {
    position: sticky;
    top: 0;
    z-index: 1000;
    height: 60px;
    background: var(--surface);
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    padding: 0 48px;
    gap: 36px;
  }

  .nav-logo {
    display: flex;
    align-items: center;
    gap: 9px;
    text-decoration: none;
    color: var(--primary);
    font-family: 'Playfair Display', serif;
    font-size: 20px;
    font-weight: 700;
    flex-shrink: 0;
  }

  .nav-links {
    display: flex;
    gap: 28px;
    list-style: none;
    flex: 1;
  }

    .nav-links a {
      text-decoration: none;
      color: var(--text-2);
      font-size: 14px;
      transition: color .2s;
    }

      .nav-links a:hover {
        color: var(--text-1);
      }

      .nav-links a.active {
        color: var(--primary);
        font-weight: 600;
      }

  .nav-actions {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-shrink: 0;
  }

  /* Przyciski dla niezalogowanego (uzupełnienie) */
  .btn {
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
  }

  .btn-outline {
    background: transparent;
    border: 1px solid var(--border);
    color: var(--text-1);
  }

    .btn-outline:hover {
      background: var(--tag-bg);
    }

  .btn-filled {
    background: var(--primary);
    border: 1px solid var(--primary);
    color: white;
  }

    .btn-filled:hover {
      opacity: 0.9;
    }

  /* ══ AVATAR & DROPDOWN ══ */
  .nav-avatar-wrap {
    position: relative;
  }

  .nav-avatar-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    background: none;
    border: none;
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 8px;
    transition: background .2s;
  }

    .nav-avatar-btn:hover {
      background: var(--tag-bg);
    }

  .nav-avatar {
    width: 34px;
    height: 34px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid var(--border);
  }

  .nav-avatar-name {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-1);
  }

  .nav-chevron {
    color: var(--text-3);
    transition: transform .2s;
    display: flex;
  }

  .nav-avatar-wrap.open .nav-chevron {
    transform: rotate(180deg);
  }

  .nav-dropdown {
    position: absolute;
    top: calc(100% + 8px);
    right: 0;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 8px;
    box-shadow: var(--shadow-h);
    width: 210px;
    overflow: hidden;
    opacity: 0;
    transform: translateY(-6px);
    transition: opacity .18s, transform .18s;
    pointer-events: none;
    z-index: 300;
  }

  .nav-avatar-wrap.open .nav-dropdown {
    opacity: 1;
    transform: translateY(0);
    pointer-events: all;
  }

  .dd-head {
    padding: 14px 16px 11px;
    border-bottom: 1px solid var(--border);
  }

    .dd-head strong {
      font-size: 13px;
      font-weight: 600;
      display: block;
      color: var(--text-1);
    }

    .dd-head span {
      font-size: 11px;
      color: var(--text-3);
    }

  .dd-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 16px;
    font-size: 13px;
    color: var(--text-2);
    cursor: pointer;
    transition: background .15s;
    text-decoration: none;
  }

    .dd-item:hover {
      background: var(--tag-bg);
      color: var(--text-1);
    }

    .dd-item.danger {
      color: var(--danger);
    }

      .dd-item.danger:hover {
        background: #fef0ee;
      }

  .dd-divider {
    height: 1px;
    background: var(--border);
    margin: 4px 0;
  }
</style>
-->
