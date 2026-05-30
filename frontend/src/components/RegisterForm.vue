<script setup>
  import { ref, reactive, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P } from '../assets/constants.js';
  import FormField from './FormField.vue';
  import { useAuthStore } from '../stores/auth';
  import { ApiError } from '../api/client';

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
  const apiError = ref('');

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

    if (!v.city?.trim()) e.city = 'Miasto jest wymagane';

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

  const submit = async () => {
    Object.keys(touched).forEach(k => { touched[k] = true; });

    if (Object.keys(errors.value).length > 0) {
      shaking.value = true;
      setTimeout(() => { shaking.value = false; }, 600);
      return;
    }

    loading.value = true;
    apiError.value = '';

    try {
      await auth.register({
        email: v.email,
        password: v.password,
        username: v.username,
        firstName: v.firstName,
        lastName: v.lastName,
        city: v.city.trim(),
      });
      loading.value = false;
      done.value = true;
      setTimeout(() => router.push('/przegladanie'), 1200);
    } catch (err) {
      loading.value = false;
      apiError.value = err instanceof ApiError ? err.message : 'Nie udało się utworzyć konta';
      shaking.value = true;
      setTimeout(() => { shaking.value = false; }, 600);
    }
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
      Konto utworzone!
    </h3>
    <p :style="{ color: T.t2, fontSize: '14px' }">Możesz od razu przeglądać i dodawać oferty.</p>
  </div>

  <form v-else @submit.prevent="submit" :class="{ 'anim-shake': shaking }" noValidate>
    <h2 :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '28px', fontWeight: 700, color: T.t1, marginBottom: '6px', lineHeight: 1.2 }">
      Dołącz do nas
    </h2>
    <p :style="{ color: T.t2, fontSize: '14px', marginBottom: '22px' }">Stwórz konto w kilka sekund</p>

    <p v-if="apiError" :style="{ fontSize: '13px', color: T.danger, marginBottom: '14px', padding: '10px 12px', background: '#FDEEEC', borderRadius: '8px' }">
      {{ apiError }}
    </p>

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
            <a href="#" @click.prevent :style="{ color: T.green, textDecoration: 'none', fontWeight: 500 }">Regulamin</a>
            {{ ' ' }}i{{ ' ' }}
            <a href="#" @click.prevent :style="{ color: T.green, textDecoration: 'none', fontWeight: 500 }">Politykę prywatności</a>
          </span>
        </label>
        <p v-if="termsErr" :style="{ fontSize: '12px', color: T.danger, marginTop: '5px', marginLeft: '28px' }">
          {{ errors.terms }}
        </p>
      </div>
    </div>

    <div :style="{ marginTop: '20px' }">
      <button type="submit" :disabled="loading" @mouseenter="hov = true" @mouseleave="hov = false"
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
