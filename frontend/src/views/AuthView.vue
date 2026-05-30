<template>
  <div :style="{ display: 'flex', minHeight: '100vh' }">
    <AuthLeftPanel />

    <div class="right-panel" :style="{
      flex: '0 0 50%',
      minHeight: '100vh',
      background: T.bg,
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      padding: '48px 24px',
      overflowY: 'auto',
    }">
      <div :style="{ width: '100%', maxWidth: '380px' }">

        <div :style="{ display: 'flex', background: T.tagBg, borderRadius: '10px', padding: '4px', marginBottom: '28px' }">
          <button v-for="[id, label] in [['login', 'Logowanie'], ['register', 'Rejestracja']]"
                  :key="id" type="button" @click="switchTo(id)"
                  :style="{
                    flex: 1, height: '36px', border: 'none', borderRadius: '7px',
                    fontSize: '14px', fontWeight: mode === id ? 600 : 400,
                    fontFamily: '\'DM Sans\', sans-serif', cursor: 'pointer', transition: 'all 0.2s ease',
                    background: mode === id ? T.white : 'transparent',
                    color: mode === id ? T.t1 : T.t2,
                    boxShadow: mode === id ? '0 1px 5px rgba(44,24,16,0.1)' : 'none',
                  }">
            {{ label }}
          </button>
        </div>

        <div :key="viewKey" :class="animCls" :style="{ willChange: 'transform, opacity' }">
          <LoginForm v-if="mode === 'login'" @switch="switchTo('register')" />
          <RegisterForm v-slot v-else @switch="switchTo('login')" />
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed } from 'vue';
  import { T } from '../assets/constants.js';
  import AuthLeftPanel from '../components/AuthLeftPanel.vue';
  import LoginForm from '../components/LoginForm.vue';
  import RegisterForm from '../components/RegisterForm.vue';

  const mode = ref('login');
  const prev = ref(null);
  const viewKey = ref(0);

  const switchTo = (next) => {
    if (next === mode.value) return;
    prev.value = mode.value;
    mode.value = next;
    viewKey.value += 1;
  };

  const animCls = computed(() => {
    if (prev.value === null) return '';
    return mode.value === 'register' ? 'anim-slide-right' : 'anim-slide-left';
  });
</script>
