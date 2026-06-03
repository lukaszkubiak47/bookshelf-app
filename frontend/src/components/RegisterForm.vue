<script setup>
  /** Rejestracja konta — walidacja, siła hasła, authStore.register. */
  import { ref, reactive, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P, AUTH_STYLES } from '../assets/constants.js';
  import FormField from './FormField.vue';
  import { useAuthStore } from '../stores/auth';
  import { ApiError } from '../lib/api';

  defineEmits(['switch']);

  const router = useRouter();
  const auth = useAuthStore();

  const v = reactive({
    firstName: '', lastName: '', username: '', email: '', city: '', password: '', confirmPw: '',
  });
  const terms = ref(false);
  const touched = reactive({
    firstName: false, lastName: false, username: false, email: false, city: false, password: false, confirmPw: false, terms: false
  });

  const shaking = ref(false);
  const loading = ref(false);
  const done = ref(false);
  const hov = ref(false);
  const serverError = ref('');

  const strColor = ['', T.danger, T.warning, T.gold, T.success];
  const strLabel = ['', 'Słabe', 'Mierne', 'Dobre', 'Silne'];

  const pwStrength = computed(() => {
    if (!v.password) return 0;
    let s = 0;
    if (v.password.length >= 8) s++;
    if (/[A-Z]/.test(v.password)) s++;
    if (/[0-9]/.test(v.password)) s++;
    if (/[^A-Za-z0-9]/.test(v.password)) s++;
    return s;
  });

  const errors = computed(() => {
    const e = {};
    if (!v.firstName) e.firstName = 'Wymagane';
    if (!v.lastName) e.lastName = 'Wymagane';
    if (!v.username) e.username = 'Nick jest wymagany';
    else if (v.username.length < 3) e.username = 'Min. 3 znaki';
    else if (!/^[a-zA-Z0-9_]+$/.test(v.username)) e.username = 'Tylko litery, cyfry i _';

    if (!v.email) e.email = 'Email jest wymagany';
    else if (!/\S+@\S+\.\S+/.test(v.email)) e.email = 'Nieprawidłowy format';

    if (!v.password) e.password = 'Hasło jest wymagane';
    else if (v.password.length < 8) e.password = 'Min. 8 znaków';

    if (!v.confirmPw) e.confirmPw = 'Potwierdź hasło';
    else if (v.password !== v.confirmPw) e.confirmPw = 'Hasła nie pasują';

    if (!terms.value) e.terms = 'Akceptacja wymagana';
    return e;
  });

  const termsErr = computed(() => touched.terms && !!errors.value.terms);

  const fs = (key) => {
    if (!touched[key]) return { state: 'idle' };
    if (errors.value[key]) return { state: 'error', error: errors.value[key] };
    if (key === 'terms' ? terms.value : v[key]) return { state: 'success' };
    return { state: 'idle' };
  };

  const blur = (key) => { touched[key] = true; };
  const toggleTerms = (e) => {
    terms.value = e.target.checked;
    touched.terms = true;
  };

  const triggerShake = () => {
    shaking.value = true;
    setTimeout(() => { shaking.value = false; }, 600);
  };

  const submit = async () => {
    Object.keys(touched).forEach(k => { touched[k] = true; });

    if (Object.keys(errors.value).length > 0) {
      triggerShake();
      return;
    }

    serverError.value = '';
    loading.value = true;
    try {
      // confirmPw i terms NIE są wysyłane — backend ich nie potrzebuje.
      await auth.register({
        email: v.email,
        password: v.password,
        username: v.username,
        firstName: v.firstName,
        lastName: v.lastName,
        city: v.city,
      });
      done.value = true;
      setTimeout(() => { router.push('/'); }, 900);
    } catch (e) {
      // Backend zwraca 409 zarówno dla zajętego emaila, jak i nicka — pokazujemy jego komunikat,
      // bo poprawnie rozróżnia oba przypadki.
      serverError.value = (e instanceof ApiError && e.status === 409)
        ? (e.message || 'Konto z tym adresem email już istnieje')
        : (e instanceof Error && e.message) ? e.message : 'Rejestracja nie powiodła się';
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
      Konto utworzone!
    </h3>
    <p :style="{ color: T.t2, fontSize: '14px' }">Przekierowanie za chwilę…</p>
  </div>

  <form v-else @submit.prevent="submit" :class="{ 'anim-shake': shaking }" noValidate>
    <h2 :style="AUTH_STYLES.heading">
      Dołącz do nas
    </h2>
    <p :style="{ ...AUTH_STYLES.subheading, marginBottom: '22px' }">Stwórz konto w kilka sekund</p>

    <div :style="{ display: 'flex', flexDirection: 'column', gap: '13px' }">
      <div :style="{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px' }">
        <FormField label="Imię" placeholder="Jan" v-model="v.firstName" @blur="blur('firstName')" v-bind="fs('firstName')" />
        <FormField label="Nazwisko" placeholder="Kowalski" v-model="v.lastName" @blur="blur('lastName')" v-bind="fs('lastName')" />
      </div>

      <FormField label="Nick / Username" icon="at" placeholder="jankowalski" v-model="v.username" @blur="blur('username')" v-bind="fs('username')" />
      <FormField label="Email" type="email" icon="mail" placeholder="jan.kowalski@email.com" v-model="v.email" @blur="blur('email')" v-bind="fs('email')" />
      <FormField label="Miasto" icon="pin" placeholder="Warszawa" v-model="v.city" @blur="blur('city')" v-bind="fs('city')" />

      <div :style="{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px' }">
        <FormField label="Hasło" type="password" icon="lock" placeholder="Min. 8 znaków" v-model="v.password" @blur="blur('password')" v-bind="fs('password')">
          <div v-if="v.password" :style="{ marginTop: '6px' }">
            <div :style="{ display: 'flex', gap: '4px' }">
              <div v-for="i in 4" :key="i" class="strength-seg"
                   :style="{ background: i <= pwStrength ? strColor[pwStrength] : undefined }" />
            </div>
            <p :style="{ fontSize: '11px', color: strColor[pwStrength], marginTop: '4px', fontWeight: 500 }">
              {{ strLabel[pwStrength] }}
            </p>
          </div>
        </FormField>

        <FormField label="Potwierdź hasło" type="password" icon="lock" placeholder="Powtórz" v-model="v.confirmPw" @blur="blur('confirmPw')" v-bind="fs('confirmPw')" />
      </div>

      <div>
        <label :style="{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: 'pointer', userSelect: 'none' }">
          <div :style="{ position: 'relative', marginTop: '1px', flexShrink: 0 }">
            <input type="checkbox" :checked="terms" @change="toggleTerms" :style="{ position: 'absolute', opacity: 0, width: '18px', height: '18px', margin: 0, cursor: 'pointer' }" />
            <div :class="['bk-check-box', { 'checked': terms, 'errored': termsErr }]">
              <svg v-if="terms" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
            </div>
          </div>
          <span :style="{ fontSize: '13px', color: T.t2, lineHeight: 1.5, paddingTop: '1px' }">
            Akceptuję{{ ' ' }}
            <RouterLink to="/regulamin" target="_blank" rel="noopener" @click.stop :style="{ color: T.green, textDecoration: 'none', fontWeight: 500 }">Regulamin</RouterLink>
            {{ ' ' }}i{{ ' ' }}
            <RouterLink to="/polityka-prywatnosci" target="_blank" rel="noopener" @click.stop :style="{ color: T.green, textDecoration: 'none', fontWeight: 500 }">Politykę prywatności</RouterLink>
          </span>
        </label>
        <p v-if="termsErr" :style="{ fontSize: '12px', color: T.danger, marginTop: '5px', marginLeft: '28px' }">
          {{ errors.terms }}
        </p>
      </div>
    </div>

    <p v-if="serverError" role="alert" :style="{ ...AUTH_STYLES.errorAlert, marginTop: '16px' }">
      {{ serverError }}
    </p>

    <div :style="{ marginTop: '20px' }">
      <button type="submit" :disabled="loading" @mouseenter="hov = true" @mouseleave="hov = false"
              :style="AUTH_STYLES.submitButton(loading, hov)">
        <span v-if="loading" :style="AUTH_STYLES.submitSpinner" />
        {{ loading ? 'Tworzenie konta…' : 'Stwórz konto' }}
      </button>
    </div>

    <div :style="{ marginTop: '18px' }">
      <p :style="{ textAlign: 'center', fontSize: '14px', color: T.t2 }">
        Masz już konto?{{ ' ' }}
        <button type="button" @click="$emit('switch')"
                :style="{ background: 'none', border: 'none', color: T.green, fontWeight: 600, fontSize: '14px', cursor: 'pointer', fontFamily: '\'DM Sans\', sans-serif', padding: 0, textDecoration: 'none' }">
          Zaloguj się →
        </button>
      </p>
    </div>
  </form>
</template>
