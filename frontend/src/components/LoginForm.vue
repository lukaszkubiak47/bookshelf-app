<script setup>
  import { ref, reactive, computed } from 'vue';
  import { T, P } from '../assets/constants';
  import FormField from './FormField.vue';

  defineEmits(['switch']);

  const v = reactive({ email: '', password: '' });
  const touched = reactive({ email: false, password: false });
  const shaking = ref(false);
  const loading = ref(false);
  const done = ref(false);
  const hov = ref(false);

  const errors = computed(() => {
    const e = {};
    if (!v.email) e.email = 'Email jest wymagany';
    else if (!/\S+@\S+\.\S+/.test(v.email)) e.email = 'Nieprawidłowy format email';
    if (!v.password) e.password = 'Hasło jest wymagane';
    else if (v.password.length < 6) e.password = 'Hasło musi mieć min. 6 znaków';
    return e;
  });

  const fs = (key) => {
    if (!touched[key]) return { state: 'idle' };
    if (errors.value[key]) return { state: 'error', error: errors.value[key] };
    if (v[key]) return { state: 'success' };
    return { state: 'idle' };
  };

  const blur = (key) => { touched[key] = true; };

  const submit = async () => {
    touched.email = true;
    touched.password = true;

    if (Object.keys(errors.value).length > 0) {
      shaking.value = true;
      setTimeout(() => { shaking.value = false; }, 600);
      return;
    }

    loading.value = true;
    await new Promise(r => setTimeout(r, 1400));
    loading.value = false;
    done.value = true;
  };
</script>

<template>
  <div v-if="done" :style="{ textAlign: 'center', padding: '20px 0', animation: 'fadeScaleIn 0.4s ease both' }">
    <div :style="{
      width: '64px', height: '64px', borderRadius: '50%', background: '#EBF7F0',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      margin: '0 auto 20px', animation: 'successPulse 0.6s ease 0.2s',
    }">
      <svg width="30" height="30" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
    </div>
    <h3 :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '22px', fontWeight: 700, color: T.t1, marginBottom: '8px' }">
      Zalogowano pomyślnie!
    </h3>
    <p :style="{ color: T.t2, fontSize: '14px' }">Przekierowanie za chwilę…</p>
  </div>

  <form v-else @submit.prevent="submit" :class="{ 'anim-shake': shaking }" noValidate>
    <h2 :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '28px', fontWeight: 700, color: T.t1, marginBottom: '6px', lineHeight: 1.2 }">
      Witaj z powrotem
    </h2>
    <p :style="{ color: T.t2, fontSize: '14px', marginBottom: '26px' }">Zaloguj się na swoje konto</p>

    <div :style="{ display: 'flex', flexDirection: 'column', gap: '16px' }">
      <FormField label="Email" type="email" icon="mail" placeholder="jan.kowalski@email.com"
                 v-model="v.email" @blur="blur('email')" v-bind="fs('email')" />

      <FormField label="Hasło" type="password" icon="lock" placeholder="••••••••"
                 v-model="v.password" @blur="blur('password')" v-bind="fs('password')" />
    </div>

    <div :style="{ textAlign: 'right', marginTop: '10px', marginBottom: '22px' }">
      <a href="#" @click.prevent :style="{ fontSize: '13px', color: T.green, textDecoration: 'none', fontWeight: 500 }"
         @mouseenter="$event.target.style.textDecoration = 'underline'"
         @mouseleave="$event.target.style.textDecoration = 'none'">
        Zapomniałeś hasła?
      </a>
    </div>

    <button type="submit" :disabled="loading"
            @mouseenter="hov = true" @mouseleave="hov = false"
            :style="{
              width: '100%', height: '46px',
              background: loading ? '#7BAF9A' : (hov ? T.greenHov : T.green),
              color: 'white', border: 'none', borderRadius: '8px',
              fontSize: '15px', fontWeight: 600, fontFamily: '\'DM Sans\', sans-serif',
              cursor: loading ? 'not-allowed' : 'pointer',
              transition: 'background 0.18s ease',
              display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px',
              letterSpacing: '0.2px',
            }">
      <span v-if="loading" :style="{
        width: '16px', height: '16px', flexShrink: 0,
        border: '2px solid rgba(255,255,255,0.3)',
        borderTopColor: 'white', borderRadius: '50%',
        display: 'inline-block', animation: 'spin 0.7s linear infinite'
      }" />
      {{ loading ? 'Logowanie…' : 'Zaloguj się' }}
    </button>

    <div :style="{ display: 'flex', alignItems: 'center', gap: '14px', margin: '20px 0' }">
      <div :style="{ flex: 1, height: '1px', background: T.border }" />
      <span :style="{ fontSize: '12px', color: T.t3 }">lub</span>
      <div :style="{ flex: 1, height: '1px', background: T.border }" />
    </div>

    <p :style="{ textAlign: 'center', fontSize: '14px', color: T.t2 }">
      Nie masz konta?{{ ' ' }}
      <button type="button" @click="$emit('switch')"
              :style="{ background: 'none', border: 'none', color: T.green, fontWeight: 600, fontSize: '14px', cursor: 'pointer', fontFamily: '\'DM Sans\', sans-serif', padding: 0, textDecoration: 'none' }">
        Zarejestruj się →
      </button>
    </p>
  </form>
</template>
