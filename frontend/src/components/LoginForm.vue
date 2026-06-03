<script setup>
  /** Formularz logowania — wywołuje authStore.login, potem przekierowanie na /. */
  import { ref, reactive, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P, AUTH_STYLES } from '../assets/constants';
  import FormField from './FormField.vue';
  import { useAuthStore } from '../stores/auth';
  import { ApiError } from '../lib/api';

  defineEmits(['switch']);

  const router = useRouter();
  const auth = useAuthStore();

  const v = reactive({ email: '', password: '' });
  const touched = reactive({ email: false, password: false });
  const shaking = ref(false);
  const loading = ref(false);
  const done = ref(false);
  const hov = ref(false);
  const serverError = ref('');

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

  const triggerShake = () => {
    shaking.value = true;
    setTimeout(() => { shaking.value = false; }, 600);
  };

  const submit = async () => {
    touched.email = true;
    touched.password = true;

    if (Object.keys(errors.value).length > 0) {
      triggerShake();
      return;
    }

    serverError.value = '';
    loading.value = true;
    try {
      await auth.login(v.email, v.password);
      done.value = true;
      setTimeout(() => { router.push('/'); }, 900);
    } catch (e) {
      serverError.value = (e instanceof ApiError && e.status === 401)
        ? 'Nieprawidłowy email lub hasło'
        : (e instanceof Error && e.message) ? e.message : 'Logowanie nie powiodło się';
      triggerShake();
    } finally {
      loading.value = false;
    }
  };
</script>

<template>
  <div v-if="done" :style="AUTH_STYLES.successScreen">
    <div :style="AUTH_STYLES.successIconCircle">
      <svg width="30" height="30" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
    </div>
    <h3 :style="AUTH_STYLES.successTitle">
      Zalogowano pomyślnie!
    </h3>
    <p :style="{ color: T.t2, fontSize: '14px' }">Przekierowanie za chwilę…</p>
  </div>

  <form v-else @submit.prevent="submit" :class="{ 'anim-shake': shaking }" noValidate>
    <h2 :style="AUTH_STYLES.heading">
      Witaj z powrotem
    </h2>
    <p :style="{ ...AUTH_STYLES.subheading, marginBottom: '26px' }">Zaloguj się na swoje konto</p>

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

    <p v-if="serverError" role="alert" :style="{ ...AUTH_STYLES.errorAlert, marginBottom: '16px' }">
      {{ serverError }}
    </p>

    <button type="submit" :disabled="loading"
            @mouseenter="hov = true" @mouseleave="hov = false"
            :style="AUTH_STYLES.submitButton(loading, hov)">
      <span v-if="loading" :style="AUTH_STYLES.submitSpinner" />
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
