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

<script setup>import { ref, reactive, computed } from 'vue';

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

const changePassword = () => {
  errors.current = !passwords.current;
  errors.new = passwords.new.length < 8;
  errors.confirm = passwords.new !== passwords.confirm;

  if (errors.current || errors.new || errors.confirm) return;

  // Czyszczenie pól po pomyślnym wysłaniu
  Object.assign(passwords, { current: '', new: '', confirm: '' });
  strength.width = '0';
  strength.text = 'Wpisz hasło aby ocenić siłę';
  strength.bg = '';

  emit('notify', { message: 'Hasło zmienione ✓', type: 'success' });
};</script>

<style scoped>
  .card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    overflow: hidden;
  }

  .card-title {
    font-family: 'Playfair Display',serif;
    font-size: 18px;
    font-weight: 600;
    color: var(--text-1);
  }

  .card-subtitle {
    font-size: 12px;
    color: var(--text-3);
    margin-top: 2px;
  }

  .form-grid-2 {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
  }

  .form-field {
    display: flex;
    flex-direction: column;
    gap: 5px;
  }

    .form-field.full {
      grid-column: 1/-1;
    }

  .form-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
  }

  .input-wrap {
    position: relative;
  }

  .form-input {
    width: 100%;
    padding: 9px 13px;
    border: 1.5px solid var(--border);
    border-radius: 8px;
    font-family: 'DM Sans',sans-serif;
    font-size: 14px;
    color: var(--text-1);
    background: var(--bg);
    outline: none;
    transition: all .2s;
  }

    .form-input:focus {
      border-color: var(--primary);
      background: #fff;
      box-shadow: 0 0 0 3px rgba(45,106,79,.09);
    }

    .form-input.error {
      border-color: var(--danger);
    }

  .form-error {
    font-size: 11px;
    color: var(--danger);
    display: none;
    margin-top: 2px;
  }

    .form-error.show {
      display: block;
    }

  .form-footer {
    padding-top: 20px;
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 7px;
    padding: 9px 22px;
    border-radius: var(--r-btn);
    font-family: 'DM Sans',sans-serif;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all .2s;
    border: none;
    white-space: nowrap;
  }

  .btn-filled {
    background: var(--primary);
    color: #fff;
    border: 1.5px solid var(--primary);
  }

    .btn-filled:hover {
      background: var(--primary-h);
    }

  .sec-header {
    padding: 18px 24px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    cursor: pointer;
    user-select: none;
    transition: background .15s;
  }

    .sec-header:hover {
      background: var(--tag-bg);
    }

  .sec-title-wrap {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .sec-icon {
    width: 36px;
    height: 36px;
    border-radius: 10px;
    background: var(--tag-bg);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--primary);
  }

  .sec-chevron {
    color: var(--text-3);
    transition: transform .3s;
    display: flex;
  }

    .sec-chevron.open {
      transform: rotate(180deg);
    }

  .sec-body {
    max-height: 0;
    overflow: hidden;
    transition: max-height .4s ease, padding .4s;
    padding: 0 24px;
  }

    .sec-body.open {
      max-height: 400px;
      padding: 4px 24px 24px;
    }

  .pw-strength {
    margin-top: 6px;
  }

  .pw-strength-bar {
    height: 4px;
    border-radius: 2px;
    background: var(--border);
    overflow: hidden;
    margin-bottom: 4px;
  }

  .pw-strength-fill {
    height: 100%;
    width: 0;
    border-radius: 2px;
    transition: width .3s, background .3s;
  }

  .pw-hint {
    font-size: 11px;
    color: var(--text-3);
  }

  .pw-match {
    font-size: 11px;
    margin-top: 4px;
  }

    .pw-match.ok {
      color: var(--success);
    }

    .pw-match.bad {
      color: var(--danger);
    }

  .pw-toggle {
    position: absolute;
    right: 11px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    cursor: pointer;
    color: var(--text-3);
    display: flex;
    transition: color .2s;
  }

    .pw-toggle:hover {
      color: var(--text-1);
    }

  @media (max-width:600px) {
    .form-grid-2 {
      grid-template-columns: 1fr;
    }
  }
</style>
