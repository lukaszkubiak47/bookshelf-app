<template>
  <div class="profile-view-wrapper">
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

    <div class="profile-info-section">
      <div class="profile-info-inner">
        <h1 class="profile-name">{{ user.name }} {{ user.surname }}</h1>
        <div class="profile-nick">@{{ user.nick }}</div>
        <div class="profile-meta">
          <div class="meta-item">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" /></svg>
            <span>{{ user.city || 'Brak lokalizacji' }}</span>
          </div>
          <div class="meta-item">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></svg>
            Członek od stycznia 2025
          </div>
        </div>
        <div class="profile-stats">
          <div class="pstat"><strong>{{ user.booksCount }}</strong>książek</div>
          <div class="pstat-div"></div>
          <div class="pstat"><strong>{{ user.transactionsCount }}</strong>transakcji</div>
        </div>
      </div>
    </div>

    <div class="page">
      <PersonalData :user="user" @update:user="updateUser" @notify="triggerToast" />
      <Security @notify="triggerToast" />
    </div>

    <div :class="['toast', toast.type === 'success' ? 'success-toast' : 'error-toast', { show: toast.visible }]">
      <span v-html="toast.icon"></span> {{ toast.message }}
    </div>
  </div>
</template>

<script setup>
  import { ref, reactive } from 'vue';
  import PersonalData from '../components/PersonalData.vue';
  import Security from '../components/Security.vue';

  const user = ref({
    name: 'Jan',
    surname: 'Kowalski',
    nick: 'jan_kow',
    city: 'Gdańsk',
    email: 'jan.kowalski@email.com',
    bio: 'Zapalony czytelnik SF i fantasy. Zawsze szukam dobrego zamiennika za Sapkowskiego!',
    avatar: 'https://i.pravatar.cc/108?img=15',
    booksCount: 12,
    transactionsCount: 8
  });

  const toast = reactive({ visible: false, message: '', type: 'success', icon: '' });
  let toastTimer = null;

  const updateUser = (updatedData) => {
    user.value = updatedData;
  };

  const triggerToast = ({ message, type = 'success' }) => {
    toast.message = message;
    toast.type = type;
    toast.icon = type === 'success'
      ? `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12"/></svg>`
      : `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`;

    toast.visible = true;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => { toast.visible = false; }, 2800);
  };
</script>

<style scoped>
  .profile-hero {
    background: var(--primary);
    position: relative;
    height: 240px;
    overflow: hidden;
  }

  .hero-pattern {
    position: absolute;
    inset: 0;
    background-image: repeating-linear-gradient(90deg, rgba(255,255,255,.04) 0, rgba(255,255,255,.04) 1px, transparent 1px, transparent 48px), repeating-linear-gradient(0deg, rgba(255,255,255,.03) 0, rgba(255,255,255,.03) 1px, transparent 1px, transparent 48px);
  }

  .hero-shelves {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 100%;
    opacity: .07;
    pointer-events: none;
  }

  .hero-content {
    position: absolute;
    bottom: 24px;
    left: 0;
    right: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .profile-avatar-wrap {
    position: relative;
  }

  .profile-avatar {
    width: 108px;
    height: 108px;
    border-radius: 50%;
    object-fit: cover;
    border: 4px solid var(--surface);
    box-shadow: 0 4px 18px rgba(44,24,16,.2);
    display: block;
  }

  .avatar-online {
    position: absolute;
    bottom: 6px;
    right: 6px;
    width: 18px;
    height: 18px;
    border-radius: 50%;
    background: var(--success);
    border: 3px solid var(--surface);
  }

  .profile-info-section {
    background: var(--surface);
    border-bottom: 1px solid var(--border);
    padding: 28px 0 28px;
  }

  .profile-info-inner {
    max-width: 720px;
    margin: 0 auto;
    padding: 0 24px;
    text-align: center;
  }

  .profile-name {
    font-family: 'Playfair Display',serif;
    font-size: 24px;
    font-weight: 700;
    color: var(--text-1);
    margin-bottom: 3px;
  }

  .profile-nick {
    font-size: 14px;
    color: var(--text-3);
    margin-bottom: 12px;
  }

  .profile-meta {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 20px;
    flex-wrap: wrap;
    margin-bottom: 22px;
  }

  .meta-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 13px;
    color: var(--text-2);
  }

    .meta-item svg {
      color: var(--text-3);
    }

  .profile-stats {
    display: inline-flex;
    align-items: center;
    gap: 0;
    background: var(--tag-bg);
    border-radius: 24px;
    overflow: hidden;
    border: 1px solid var(--border);
  }

  .pstat {
    padding: 9px 20px;
    font-size: 13px;
    color: var(--text-2);
    font-weight: 500;
  }

    .pstat Red strong {
      color: var(--text-1);
      font-weight: 700;
      font-family: 'Playfair Display',serif;
      font-size: 15px;
      display: block;
    }

    .pstat strong {
      color: var(--text-1);
      font-weight: 700;
      font-family: 'Playfair Display',serif;
      font-size: 15px;
      display: block;
    }

  .pstat-div {
    width: 1px;
    background: var(--border);
    align-self: stretch;
  }

  .page {
    max-width: 720px;
    margin: 0 auto;
    padding: 32px 24px 80px;
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .toast {
    position: fixed;
    bottom: 28px;
    left: 50%;
    transform: translateX(-50%) translateY(16px);
    color: #fff;
    padding: 12px 24px;
    border-radius: 10px;
    font-size: 13px;
    font-weight: 500;
    z-index: 1000;
    opacity: 0;
    transition: opacity .25s, transform .25s;
    pointer-events: none;
    white-space: nowrap;
    display: flex;
    align-items: center;
    gap: 8px;
  }

    .toast.show {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }

    .toast.success-toast {
      background: var(--primary);
    }

    .toast.error-toast {
      background: var(--danger);
    }

  @media (max-width:600px) {
    .profile-hero {
      height: 130px;
    }

    .hero-content {
      bottom: -46px;
    }

    .profile-avatar {
      width: 88px;
      height: 88px;
    }

    .profile-info-section {
      padding: 54px 0 20px;
    }

    .profile-stats {
      flex-direction: column;
      border-radius: 12px;
    }

    .pstat-div {
      width: 100%;
      height: 1px;
    }

    .page {
      padding: 24px 16px 60px;
    }
  }
</style>
