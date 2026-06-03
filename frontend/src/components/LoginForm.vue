<script setup>
  import { ref, reactive, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P } from '../assets/constants';
  import FormField from './FormField.vue';
  import { useAuthStore } from '../stores/auth';
  import { ApiError, apiFetch } from '../lib/api';

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
  const resetOpen = ref(false);
  const resetStep = ref(1);
  const resetLoading = ref(false);
  const resetSuccess = ref(false);
  const resetServerError = ref('');

  const reset = reactive({
    email: '',
    username: '',
    createdMonth: '',
    founderAnswer: '',
    newPassword: '',
    confirmPassword: '',
  });

  const resetTouched = reactive({
    email: false,
    username: false,
    createdMonth: false,
    newPassword: false,
    confirmPassword: false,
  });

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

  const resetIdentityErrors = computed(() => {
    const e = {};
    if (!reset.email) e.email = 'Email jest wymagany';
    else if (!/\S+@\S+\.\S+/.test(reset.email)) e.email = 'Nieprawidlowy format email';
    if (!reset.username) e.username = 'Nick jest wymagany';
    else if (reset.username.length < 3) e.username = 'Min. 3 znaki';
    if (!reset.createdMonth) e.createdMonth = 'Wybierz miesiac';
    return e;
  });

  const resetPasswordErrors = computed(() => {
    const e = {};
    if (!reset.newPassword) e.newPassword = 'Nowe haslo jest wymagane';
    else if (reset.newPassword.length < 8) e.newPassword = 'Min. 8 znakow';
    if (!reset.confirmPassword) e.confirmPassword = 'Powtorz nowe haslo';
    else if (reset.newPassword !== reset.confirmPassword) e.confirmPassword = 'Hasla nie pasuja';
    return e;
  });

  const resetFs = (key) => {
    if (!resetTouched[key]) return { state: 'idle' };
    const errs = resetStep.value === 1 ? resetIdentityErrors.value : resetPasswordErrors.value;
    if (errs[key]) return { state: 'error', error: errs[key] };
    if (reset[key]) return { state: 'success' };
    return { state: 'idle' };
  };

  const blur = (key) => { touched[key] = true; };
  const resetBlur = (key) => { resetTouched[key] = true; };

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

  const openResetModal = () => {
    resetOpen.value = true;
    resetStep.value = 1;
    resetSuccess.value = false;
    resetServerError.value = '';
    reset.email = v.email;
  };

  const closeResetModal = () => {
    if (resetLoading.value) return;
    resetOpen.value = false;
  };

  const buildResetVerificationPayload = () => ({
    email: reset.email,
    username: reset.username,
    createdMonth: Number(reset.createdMonth),
    founderAnswer: reset.founderAnswer,
  });

  const goToPasswordStep = async () => {
    resetTouched.email = true;
    resetTouched.username = true;
    resetTouched.createdMonth = true;
    resetServerError.value = '';

    if (Object.keys(resetIdentityErrors.value).length > 0) {
      triggerShake();
      return;
    }

    resetLoading.value = true;
    try {
      await apiFetch('/auth/verify-password-reset', {
        method: 'POST',
        body: JSON.stringify(buildResetVerificationPayload()),
      });
      resetStep.value = 2;
    } catch (e) {
      resetServerError.value = (e instanceof Error && e.message)
        ? e.message
        : 'Podane dane nie pasuja do zadnego konta';
      triggerShake();
    } finally {
      resetLoading.value = false;
    }
  };

  const submitResetPassword = async () => {
    resetTouched.newPassword = true;
    resetTouched.confirmPassword = true;
    resetServerError.value = '';

    if (Object.keys(resetPasswordErrors.value).length > 0) {
      triggerShake();
      return;
    }

    resetLoading.value = true;
    try {
      await apiFetch('/auth/reset-password', {
        method: 'POST',
        body: JSON.stringify({
          verification: buildResetVerificationPayload(),
          newPassword: reset.newPassword,
        }),
      });
      resetSuccess.value = true;
      reset.newPassword = '';
      reset.confirmPassword = '';
    } catch (e) {
      resetServerError.value = (e instanceof Error && e.message)
        ? e.message
        : 'Nie udalo sie zresetowac hasla';
      triggerShake();
    } finally {
      resetLoading.value = false;
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
      <a href="#" @click.prevent="openResetModal" :style="{ fontSize: '13px', color: T.green, textDecoration: 'none', fontWeight: 500 }"
         @mouseenter="$event.target.style.textDecoration = 'underline'"
         @mouseleave="$event.target.style.textDecoration = 'none'">
        Zapomniałeś hasła?
      </a>
    </div>

    <p v-if="serverError" role="alert" :style="{
      background: '#FBEDEC', color: T.danger, fontSize: '13px',
      padding: '10px 12px', borderRadius: '8px', marginBottom: '16px',
      border: `1px solid ${T.danger}33`,
    }">
      {{ serverError }}
    </p>

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

  <div v-if="resetOpen"
       :style="{ position: 'fixed', inset: 0, background: 'rgba(44,24,16,.32)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '20px' }"
       @click.self="closeResetModal">
    <div :class="{ 'anim-shake': shaking }"
         :style="{ width: '100%', maxWidth: '430px', background: T.white, borderRadius: '14px', padding: '24px', boxShadow: '0 18px 60px rgba(44,24,16,.18)', border: `1px solid ${T.border}` }">
      <div :style="{ display: 'flex', justifyContent: 'space-between', gap: '16px', alignItems: 'flex-start', marginBottom: '18px' }">
        <div>
          <h3 :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '24px', color: T.t1, marginBottom: '6px' }">
            Reset hasla
          </h3>
          <p :style="{ color: T.t2, fontSize: '13px', lineHeight: 1.45 }">
            {{ resetStep === 1 ? 'Podaj dane konta, zeby potwierdzic, ze to Ty.' : 'Dane pasuja. Ustaw nowe haslo.' }}
          </p>
        </div>
        <button type="button" @click="closeResetModal"
                :style="{ border: 'none', background: T.tagBg, color: T.t2, width: '30px', height: '30px', borderRadius: '8px', cursor: 'pointer', fontSize: '18px', lineHeight: 1 }">
          x
        </button>
      </div>

      <div v-if="resetSuccess" :style="{ textAlign: 'center', padding: '12px 0 4px' }">
        <div :style="{ width: '54px', height: '54px', borderRadius: '50%', background: '#EBF7F0', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px' }">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
        </div>
        <h4 :style="{ color: T.t1, fontSize: '18px', marginBottom: '8px' }">Haslo zostalo zmienione</h4>
        <p :style="{ color: T.t2, fontSize: '13px', marginBottom: '18px' }">Mozesz teraz zalogowac sie nowym haslem.</p>
        <button type="button" class="btn btn-filled" @click="closeResetModal" :style="{ width: '100%' }">
          Wroc do logowania
        </button>
      </div>

      <div v-else>
        <div v-if="resetStep === 1" :style="{ display: 'flex', flexDirection: 'column', gap: '14px' }">
          <FormField label="Email" type="email" icon="mail" placeholder="jan.kowalski@email.com"
                     v-model="reset.email" @blur="resetBlur('email')" v-bind="resetFs('email')" />
          <FormField label="Nick" icon="at" placeholder="jankowalski"
                     v-model="reset.username" @blur="resetBlur('username')" v-bind="resetFs('username')" />
          <div :style="{ display: 'flex', flexDirection: 'column', gap: '5px' }">
            <label :style="{ fontSize: '12px', fontWeight: 500, color: T.t2, letterSpacing: '0.3px' }">Miesiac zalozenia konta</label>
            <select v-model="reset.createdMonth" @blur="resetBlur('createdMonth')" class="bk-input"
                    :data-s="resetTouched.createdMonth && resetIdentityErrors.createdMonth ? 'error' : resetTouched.createdMonth && reset.createdMonth ? 'success' : undefined">
              <option value="">Wybierz miesiac</option>
              <option v-for="month in 12" :key="month" :value="String(month)">{{ month }}</option>
            </select>
            <p v-if="resetTouched.createdMonth && resetIdentityErrors.createdMonth" :style="{ fontSize: '12px', color: T.danger, marginTop: '1px' }">
              {{ resetIdentityErrors.createdMonth }}
            </p>
          </div>
          <FormField label="Bonusowe pytanie: jak ma na imie zalozyciel strony?" placeholder="Opcjonalnie, dla klimatu"
                     v-model="reset.founderAnswer" />
        </div>

        <div v-else :style="{ display: 'flex', flexDirection: 'column', gap: '14px' }">
          <FormField label="Nowe haslo" type="password" icon="lock" placeholder="Min. 8 znakow"
                     v-model="reset.newPassword" @blur="resetBlur('newPassword')" v-bind="resetFs('newPassword')" />
          <FormField label="Powtorz nowe haslo" type="password" icon="lock" placeholder="Powtorz haslo"
                     v-model="reset.confirmPassword" @blur="resetBlur('confirmPassword')" v-bind="resetFs('confirmPassword')" />
        </div>

        <p v-if="resetServerError" role="alert"
           :style="{ background: '#FBEDEC', color: T.danger, fontSize: '13px', padding: '10px 12px', borderRadius: '8px', marginTop: '16px', border: `1px solid ${T.danger}33` }">
          {{ resetServerError }}
        </p>

        <div :style="{ display: 'flex', gap: '10px', marginTop: '22px' }">
          <button v-if="resetStep === 2" type="button" class="btn btn-outline" @click="resetStep = 1" :style="{ flex: 1 }">
            Wroc
          </button>
          <button type="button" class="btn btn-filled" :disabled="resetLoading"
                  @click="resetStep === 1 ? goToPasswordStep() : submitResetPassword()"
                  :style="{ flex: 1, opacity: resetLoading ? .7 : 1 }">
            {{ resetLoading ? 'Zapisywanie...' : resetStep === 1 ? 'Dalej' : 'Zmien haslo' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
