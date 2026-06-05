<script setup>
  /**
   * Formularz logowania z wbudowanym dwuetapowym resetem hasła.
   * Użycie: AuthView.vue — zakładka „login”.
   * Emity: switch — przełączenie na RegisterForm.
   * Po sukcesie: authStore.login → przekierowanie na /.
   */
  import { ref, reactive, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { T, P, AUTH_STYLES } from '../assets/constants';
  import FormField from './FormField.vue';
  import { useAuthStore } from '../stores/auth';
  import { ApiError, apiFetch } from '../lib/api';

  defineEmits(['switch']);

  const router = useRouter();
  const auth = useAuthStore();

  const v = reactive({ email: '', password: '' });
  const touched = reactive({ email: false, password: false });
  const shaking = ref(false);
  const resetShaking = ref(false);
  const resetStepAnim = ref('');
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

  const triggerResetShake = () => {
    resetShaking.value = true;
    setTimeout(() => { resetShaking.value = false; }, 600);
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
    resetStepAnim.value = '';
    resetSuccess.value = false;
    resetServerError.value = '';
    reset.email = v.email;
  };

  const goBackToResetStep1 = () => {
    resetStepAnim.value = 'anim-reset-step-back';
    resetStep.value = 1;
    resetServerError.value = '';
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
      triggerResetShake();
      return;
    }

    resetLoading.value = true;
    try {
      await apiFetch('/auth/verify-password-reset', {
        method: 'POST',
        body: JSON.stringify(buildResetVerificationPayload()),
      });
      resetStepAnim.value = 'anim-reset-step-in';
      resetStep.value = 2;
    } catch (e) {
      resetServerError.value = (e instanceof Error && e.message)
        ? e.message
        : 'Podane dane nie pasuja do zadnego konta';
      triggerResetShake();
    } finally {
      resetLoading.value = false;
    }
  };

  const submitResetPassword = async () => {
    resetTouched.newPassword = true;
    resetTouched.confirmPassword = true;
    resetServerError.value = '';

    if (Object.keys(resetPasswordErrors.value).length > 0) {
      triggerResetShake();
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
      triggerResetShake();
    } finally {
      resetLoading.value = false;
    }
  };
</script>

<template>
  <!-- Trzy widoki: sukces logowania | reset hasła (2 kroki) | formularz logowania -->
  <div v-if="done" :style="AUTH_STYLES.successScreen">
    <div :style="AUTH_STYLES.successIconCircle">
      <svg width="30" height="30" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
    </div>
    <h3 :style="AUTH_STYLES.successTitle">
      Zalogowano pomyślnie!
    </h3>
    <p :style="{ color: T.t2, fontSize: '14px' }">Przekierowanie za chwilę…</p>
  </div>

  <!-- Reset hasła: krok 1 (weryfikacja tożsamości) → krok 2 (nowe hasło) -->
  <div v-else-if="resetOpen" class="anim-slide-right" :class="{ 'anim-shake': resetShaking }"
       :style="{ willChange: 'transform, opacity' }">
    <button type="button" @click="closeResetModal" :disabled="resetLoading"
            :style="{ background: 'none', border: 'none', color: T.t2, fontSize: '13px', cursor: resetLoading ? 'default' : 'pointer', fontFamily: '\'DM Sans\', sans-serif', padding: 0, marginBottom: '18px', opacity: resetLoading ? .6 : 1 }">
      ← Wróć do logowania
    </button>

    <div :style="{ display: 'flex', justifyContent: 'space-between', gap: '16px', alignItems: 'flex-start', marginBottom: '18px' }">
      <div>
        <h2 :style="AUTH_STYLES.heading">
          Reset hasła
        </h2>
        <p :style="{ ...AUTH_STYLES.subheading, marginBottom: 0 }">
          {{ resetStep === 1 ? 'Podaj dane konta, żeby potwierdzić, że to Ty.' : 'Dane pasują. Ustaw nowe hasło.' }}
        </p>
      </div>
    </div>

    <div v-if="resetSuccess" class="anim-reset-success" :style="{ textAlign: 'center', padding: '12px 0 4px' }">
      <div class="anim-reset-success-icon" :style="{ width: '54px', height: '54px', borderRadius: '50%', background: '#EBF7F0', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px' }">
        <svg width="26" height="26" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
      </div>
      <h4 :style="{ color: T.t1, fontSize: '18px', marginBottom: '8px' }">Hasło zostało zmienione</h4>
      <p :style="{ color: T.t2, fontSize: '13px', marginBottom: '18px' }">Możesz teraz zalogować się nowym hasłem.</p>
      <button type="button" class="btn btn-filled" @click="closeResetModal" :style="{ width: '100%' }">
        Wróć do logowania
      </button>
    </div>

    <div v-else>
      <div v-if="resetStep === 1" :key="'reset-step-1'" :class="resetStepAnim"
           :style="{ display: 'flex', flexDirection: 'column', gap: '14px', willChange: 'transform, opacity' }">
        <FormField label="Email" type="email" icon="mail" placeholder="jan.kowalski@email.com"
                   v-model="reset.email" @blur="resetBlur('email')" v-bind="resetFs('email')" />
        <FormField label="Nick" icon="at" placeholder="jankowalski"
                   v-model="reset.username" @blur="resetBlur('username')" v-bind="resetFs('username')" />
        <div :style="{ display: 'flex', flexDirection: 'column', gap: '5px' }">
          <label :style="{ fontSize: '12px', fontWeight: 500, color: T.t2, letterSpacing: '0.3px' }">Miesiąc założenia konta</label>
          <select v-model="reset.createdMonth" @blur="resetBlur('createdMonth')" class="bk-input"
                  :data-s="resetTouched.createdMonth && resetIdentityErrors.createdMonth ? 'error' : resetTouched.createdMonth && reset.createdMonth ? 'success' : undefined">
            <option value="">Wybierz miesiąc</option>
            <option v-for="month in 12" :key="month" :value="String(month)">{{ month }}</option>
          </select>
          <p v-if="resetTouched.createdMonth && resetIdentityErrors.createdMonth" :style="{ fontSize: '12px', color: T.danger, marginTop: '1px' }">
            {{ resetIdentityErrors.createdMonth }}
          </p>
        </div>
        <FormField label="Bonusowe pytanie: jak ma na imię założyciel strony?" placeholder="Opcjonalnie, dla klimatu"
                   v-model="reset.founderAnswer" />
      </div>

      <div v-else :key="'reset-step-2'" :class="resetStepAnim"
           :style="{ display: 'flex', flexDirection: 'column', gap: '14px', willChange: 'transform, opacity' }">
        <FormField label="Nowe hasło" type="password" icon="lock" placeholder="Min. 8 znaków"
                   v-model="reset.newPassword" @blur="resetBlur('newPassword')" v-bind="resetFs('newPassword')" />
        <FormField label="Powtórz nowe hasło" type="password" icon="lock" placeholder="Powtórz hasło"
                   v-model="reset.confirmPassword" @blur="resetBlur('confirmPassword')" v-bind="resetFs('confirmPassword')" />
      </div>

      <p v-if="resetServerError" role="alert"
         :style="{ background: '#FBEDEC', color: T.danger, fontSize: '13px', padding: '10px 12px', borderRadius: '8px', marginTop: '16px', border: `1px solid ${T.danger}33` }">
        {{ resetServerError }}
      </p>

      <div :style="{ display: 'flex', gap: '10px', marginTop: '22px' }">
        <button v-if="resetStep === 2" type="button" class="btn btn-outline" @click="goBackToResetStep1" :style="{ flex: 1 }">
          Wstecz
        </button>
        <button type="button" class="btn btn-filled" :disabled="resetLoading"
                @click="resetStep === 1 ? goToPasswordStep() : submitResetPassword()"
                :style="{ flex: 1, opacity: resetLoading ? .7 : 1 }">
          {{ resetLoading ? 'Zapisywanie…' : resetStep === 1 ? 'Dalej' : 'Zmień hasło' }}
        </button>
      </div>
    </div>
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
      <a href="#" @click.prevent="openResetModal" :style="{ fontSize: '13px', color: T.green, textDecoration: 'none', fontWeight: 500 }"
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
