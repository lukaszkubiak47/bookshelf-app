<script setup>
  /** Zmiana hasła w profilu — rozwijana sekcja, PUT /api/users/me/password. */
  import { ref, reactive, computed } from 'vue';
  import { usersApi } from '../lib/users';
  import { ApiError } from '../lib/client';

  const emit = defineEmits(['notify']);

  const isOpen = ref(false);
  const passwords = reactive({ current: '', new: '', confirm: '' });
  const visibility = reactive({ current: false, new: false, confirm: false });
  const errors = reactive({ current: false, new: false, confirm: false });

  const strength = reactive({ width: '0', bg: '', text: 'Wpisz hasło aby ocenić siłę' });

  const isMatch = computed(() => passwords.new === passwords.confirm);

  const toggleOpen = () => {
    isOpen.value = !isOpen.value;
  };

  const evaluateStrength = () => {
    const v = passwords.new;
    if (!v) {
      strength.width = '0';
      strength.bg = '';
      strength.text = 'Wpisz hasło aby ocenić siłę';
      return;
    }
    let score = 0;
    if (v.length >= 8) score++;
    if (v.length >= 12) score++;
    if (/[A-Z]/.test(v)) score++;
    if (/[0-9]/.test(v)) score++;
    if (/[^A-Za-z0-9]/.test(v)) score++;

    const levels = [
      { width: '20%', bg: '#e74c3c', text: 'Bardzo słabe' },
      { width: '40%', bg: '#e67e22', text: 'Słabe' },
      { width: '60%', bg: '#f1c40f', text: 'Średnie' },
      { width: '80%', bg: '#2ecc71', text: 'Dobre' },
      { width: '100%', bg: '#27ae60', text: 'Silne' },
    ];

    const level = levels[Math.min(score, 4)];
    strength.width = level.width;
    strength.bg = level.bg;
    strength.text = level.text;
  };

  const changePassword = async () => {
    errors.current = !passwords.current;
    errors.new = passwords.new.length < 8;
    errors.confirm = passwords.new !== passwords.confirm;

    if (errors.current || errors.new || errors.confirm) return;

    try {
      await usersApi.changePassword({
        currentPassword: passwords.current,
        newPassword: passwords.new,
      });

      Object.assign(passwords, { current: '', new: '', confirm: '' });
      strength.width = '0';
      strength.text = 'Wpisz hasło aby ocenić siłę';
      strength.bg = '';

      emit('notify', { message: 'Hasło zmienione ✓', type: 'success' });
    } catch (err) {
      emit('notify', {
        message: err instanceof ApiError ? err.message : 'Nie udało się zmienić hasła',
        type: 'error',
      });
    }
  };
</script>

<template>
  <div class="card">
    <div class="sec-header" @click="toggleOpen" role="button" tabindex="0" :aria-expanded="isOpen">
      <div class="sec-title-wrap">
        <div class="sec-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /></svg>
        </div>
        <div>
          <div class="card-title">Bezpieczeństwo</div>
          <div class="card-subtitle">Zmień swoje hasło</div>
        </div>
      </div>
      <div :class="['sec-chevron', { open: isOpen }]">
        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 6l4 4 4-4" /></svg>
      </div>
    </div>

    <div :class="['sec-body', { open: isOpen }]">
      <div class="form-grid-2" style="margin-bottom:0">
        <div class="form-field full">
          <label class="form-label" for="pwCurrent">Aktualne hasło</label>
          <div class="input-wrap">
            <input v-model="passwords.current" :type="visibility.current ? 'text' : 'password'" :class="['form-input', { error: errors.current }]" id="pwCurrent" placeholder="Wpisz aktualne hasło">
            <button class="pw-toggle" type="button" @click="visibility.current = !visibility.current" aria-label="Pokaż/ukryj hasło">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
            </button>
            <div :class="['form-error', { show: errors.current }]">Wpisz aktualne hasło</div>
          </div>
        </div>

        <div class="form-field">
          <label class="form-label" for="pwNew">Nowe hasło</label>
          <div class="input-wrap">
            <input v-model="passwords.new" :type="visibility.new ? 'text' : 'password'" :class="['form-input', { error: errors.new }]" id="pwNew" placeholder="Min. 8 znaków" @input="evaluateStrength">
            <button class="pw-toggle" type="button" @click="visibility.new = !visibility.new" aria-label="Pokaż/ukryj hasło">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
            </button>
          </div>
          <div class="pw-strength">
            <div class="pw-strength-bar">
              <div class="pw-strength-fill" :style="{ width: strength.width, background: strength.bg }"></div>
            </div>
            <div class="pw-hint" :style="{ color: strength.bg }">{{ strength.text }}</div>
          </div>
          <div :class="['form-error', { show: errors.new }]">Hasło musi mieć min. 8 znaków</div>
        </div>

        <div class="form-field">
          <label class="form-label" for="pwConfirm">Potwierdź nowe hasło</label>
          <div class="input-wrap">
            <input v-model="passwords.confirm" :type="visibility.confirm ? 'text' : 'password'" :class="['form-input', { error: errors.confirm }]" id="pwConfirm" placeholder="Powtórz nowe hasło">
            <button class="pw-toggle" type="button" @click="visibility.confirm = !visibility.confirm" aria-label="Pokaż/ukryj hasło">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
            </button>
          </div>
          <div v-if="passwords.confirm" :class="['pw-match', isMatch ? 'ok' : 'bad']">
            {{ isMatch ? '✓ Hasła są zgodne' : '✗ Hasła się różnią' }}
          </div>
          <div :class="['form-error', { show: errors.confirm }]">Hasła się nie zgadzają</div>
        </div>
      </div>

      <div class="form-footer">
        <button class="btn btn-filled" @click="changePassword">Zmień hasło</button>
      </div>
    </div>
  </div>
</template>
