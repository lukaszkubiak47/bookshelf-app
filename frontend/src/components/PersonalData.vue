<template>
  <div class="card">
    <div class="card-header">
      <div>
        <div class="card-title">Dane osobowe</div>
        <div class="card-subtitle">{{ isEditMode ? 'Edytuj swoje dane publiczne' : 'Twoje dane publiczne' }}</div>
      </div>
      <button v-if="!isEditMode" class="btn btn-outline btn-sm" @click="enterEditMode">
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" /></svg>
        Edytuj
      </button>
    </div>

    <div v-if="!isEditMode" class="card-body">
      <div class="profile-data-grid">
        <div class="data-row">
          <span class="data-label">Imię i nazwisko</span>
          <span class="data-value">{{ user.name }} {{ user.surname }}</span>
        </div>
        <div class="data-row">
          <span class="data-label">Nick</span>
          <span class="data-value">@{{ user.nick }}</span>
        </div>
        <div class="data-row">
          <span class="data-label">Miasto</span>
          <span class="data-value">{{ user.city || '—' }}</span>
        </div>
        <div class="data-row">
          <span class="data-label">Email</span>
          <span class="data-value">{{ user.email }}</span>
        </div>
        <div class="data-row">
          <span class="data-label">O mnie</span>
          <span class="data-value">{{ user.bio || '—' }}</span>
        </div>
      </div>
    </div>

    <div v-else class="card-body">
      <div class="edit-subsection-title">Zdjęcie profilowe</div>
      <div class="avatar-picker-layout">
        <div class="current-av-col">
          <img :src="form.avatar" alt="Aktualny avatar" class="current-av">
          <span class="current-av-label">Aktualny</span>
        </div>
        <div class="av-right">
          <div class="av-grid">
            <div v-for="i in 10" :key="i" style="position: relative;">
              <img :src="`https://i.pravatar.cc/96?img=${i}`"
                   :class="['av-opt', { selected: selectedPreset === i && !customAvatar }]"
                   @click="selectPreset(i)"
                   :alt="`Avatar ${i}`" />
              <div class="av-check" v-if="selectedPreset === i && !customAvatar">
                <svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="3" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
              </div>
            </div>
          </div>

          <div class="av-sep">lub</div>

          <input type="file" ref="fileInput" accept="image/*" style="display:none" @change="handleFileChange">
          <div class="av-dropzone"
               :style="{ borderColor: isDragging ? 'var(--primary)' : '' }"
               @click="triggerFileInput"
               @dragover.prevent="isDragging = true"
               @dragleave="isDragging = false"
               @drop.prevent="handleDrop">
            <div v-if="!customAvatar">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="var(--text-3)" stroke-width="1.5" stroke-linecap="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" /><polyline points="17 8 12 3 7 8" /><line x1="12" y1="3" x2="12" y2="15" /></svg>
              <p>Przeciągnij zdjęcie lub <span class="dz-link">kliknij aby wybrać</span></p>
              <span>Maks. 2 MB, JPG / PNG</span>
            </div>
            <div v-else class="av-preview">
              <img :src="customAvatar" alt="Podgląd awatara">
              <button class="av-remove" @click.stop="removeCustomAvatar">Usuń</button>
            </div>
          </div>
        </div>
      </div>

      <div class="edit-subsection-divider"></div>

      <div class="edit-subsection-title" style="margin-top:20px">Dane</div>
      <div class="form-grid-2">
        <div class="form-field">
          <label class="form-label" for="fName">Imię</label>
          <input v-model="form.name" :class="['form-input', { error: errors.name }]" id="fName" type="text" placeholder="Twoje imię">
          <div :class="['form-error', { show: errors.name }]">Pole wymagane</div>
        </div>
        <div class="form-field">
          <label class="form-label" for="fSurname">Nazwisko</label>
          <input v-model="form.surname" :class="['form-input', { error: errors.surname }]" id="fSurname" type="text" placeholder="Twoje nazwisko">
          <div :class="['form-error', { show: errors.surname }]">Pole wymagane</div>
        </div>
        <div class="form-field">
          <label class="form-label" for="fNick">Nick</label>
          <div class="input-wrap">
            <span class="nick-prefix">@</span>
            <input v-model="form.nick" class="form-input with-prefix" id="fNick" type="text" placeholder="twoj_nick">
          </div>
        </div>
        <div class="form-field">
          <label class="form-label" for="fCity">Miasto</label>
          <div class="input-wrap">
            <input v-model="form.city" class="form-input" id="fCity" type="text" placeholder="Twoje miasto">
          </div>
        </div>
        <div class="form-field full">
          <label class="form-label" for="fEmail">Email</label>
          <div class="input-wrap">
            <input :value="user.email" class="form-input readonly" id="fEmail" type="email" readonly>
            <span class="input-icon">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
            </span>
          </div>
          <span style="font-size:11px;color:var(--text-3);margin-top:3px">Zmiana emaila wymaga weryfikacji — skontaktuj się z pomocą techniczną</span>
        </div>
        <div class="form-field full">
          <label class="form-label" for="fBio">O mnie (opcjonalnie)</label>
          <textarea v-model="form.bio" class="form-input" id="fBio" rows="3" placeholder="Kilka słów o sobie i swoich ulubionych gatunkach…" style="resize:vertical"></textarea>
        </div>
      </div>

      <div class="form-footer">
        <button class="btn btn-filled" @click="saveChanges">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12" /></svg>
          Zapisz zmiany
        </button>
        <button class="btn btn-outline" @click="cancelEdit">Anuluj</button>
      </div>
    </div>
  </div>
</template>

<script setup>import { ref, reactive } from 'vue';

const props = defineProps({
  user: { type: Object, required: true }
});

const emit = defineEmits(['update:user', 'notify']);

const isEditMode = ref(false);
const isDragging = ref(false);
const fileInput = ref(null);
const customAvatar = ref(null);
const selectedPreset = ref(15);

const form = reactive({ name: '', surname: '', nick: '', city: '', bio: '', avatar: '' });
const errors = reactive({ name: false, surname: false });

const enterEditMode = () => {
  Object.assign(form, { ...props.user });
  errors.name = false;
  errors.surname = false;
  isEditMode.value = true;
};

const cancelEdit = () => {
  isEditMode.value = false;
};

const selectPreset = (id) => {
  selectedPreset.value = id;
  customAvatar.value = null;
  form.avatar = `https://i.pravatar.cc/108?img=${id}`;
};

const triggerFileInput = () => {
  if (!customAvatar.value) fileInput.value.click();
};

const handleFileChange = (e) => {
  if (e.target.files[0]) loadFile(e.target.files[0]);
};

const handleDrop = (e) => {
  isDragging.value = false;
  const file = e.dataTransfer.files[0];
  if (file && file.type.startsWith('image/')) loadFile(file);
};

const loadFile = (file) => {
  const reader = new FileReader();
  reader.onload = (ev) => {
    customAvatar.value = ev.target.result;
    form.avatar = ev.target.result;
  };
  reader.readAsDataURL(file);
};

const removeCustomAvatar = () => {
  customAvatar.value = null;
  form.avatar = props.user.avatar;
};

const saveChanges = () => {
  errors.name = !form.name.trim();
  errors.surname = !form.surname.trim();

  if (errors.name || errors.surname) return;

  emit('update:user', { ...props.user, ...form });
  isEditMode.value = false;
  emit('notify', { message: 'Dane zostały zapisane ✓', type: 'success' });
};</script>

<style scoped>
  .card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    overflow: hidden;
  }

  .card-header {
    padding: 20px 24px;
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    justify-content: space-between;
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

  .card-body {
    padding: 24px;
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

  .input-icon {
    position: absolute;
    right: 11px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-3);
    display: flex;
    pointer-events: none;
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

    .form-input.readonly {
      background: var(--tag-bg);
      color: var(--text-2);
      cursor: not-allowed;
      padding-right: 36px;
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

  .nick-prefix {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-3);
    font-size: 14px;
    pointer-events: none;
  }

  .form-input.with-prefix {
    padding-left: 24px;
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

  .btn-outline {
    background: transparent;
    border: 1.5px solid var(--primary);
    color: var(--primary);
  }

    .btn-outline:hover {
      background: var(--primary);
      color: #fff;
    }

  .btn-sm {
    padding: 6px 14px;
    font-size: 13px;
  }

  .profile-data-grid {
    display: flex;
    flex-direction: column;
  }

  .data-row {
    display: grid;
    grid-template-columns: 140px 1fr;
    gap: 12px;
    align-items: baseline;
    padding: 11px 0;
    border-bottom: 1px solid var(--border);
  }

    .data-row:last-child {
      border-bottom: none;
    }

  .data-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
  }

  .data-value {
    font-size: 14px;
    color: var(--text-1);
    line-height: 1.5;
  }

  .edit-subsection-title {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-3);
    text-transform: uppercase;
    letter-spacing: .5px;
    margin-bottom: 14px;
  }

  .edit-subsection-divider {
    height: 1px;
    background: var(--border);
    margin: 20px 0 0;
  }

  .avatar-picker-layout {
    display: flex;
    align-items: flex-start;
    gap: 22px;
  }

  .current-av-col {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }

  .current-av {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    object-fit: cover;
    border: 3px solid var(--border);
    box-shadow: var(--shadow);
  }

  .current-av-label {
    font-size: 11px;
    color: var(--text-3);
    text-align: center;
  }

  .av-right {
    flex: 1;
    min-width: 0;
  }

  .av-grid {
    display: grid;
    grid-template-columns: repeat(5,48px);
    gap: 10px;
    margin-bottom: 18px;
  }

  .av-opt {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    cursor: pointer;
    border: 2.5px solid transparent;
    transition: all .2s;
    object-fit: cover;
    display: block;
  }

    .av-opt:hover {
      transform: scale(1.08);
      border-color: var(--border);
    }

    .av-opt.selected {
      border-color: var(--primary);
      box-shadow: 0 0 0 2px rgba(45,106,79,.25);
    }

  .av-check {
    position: absolute;
    bottom: -2px;
    right: -2px;
    width: 17px;
    height: 17px;
    background: var(--primary);
    border-radius: 50%;
    border: 2px solid var(--surface);
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .av-sep {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 14px;
    color: var(--text-3);
    font-size: 12px;
  }

    .av-sep::before, .av-sep::after {
      content: '';
      flex: 1;
      height: 1px;
      background: var(--border);
    }

  .av-dropzone {
    border: 2px dashed var(--border);
    border-radius: 8px;
    background: var(--bg);
    padding: 18px 14px;
    text-align: center;
    cursor: pointer;
    transition: all .2s;
  }

    .av-dropzone:hover {
      border-color: var(--primary);
      background: #f4fbf7;
    }

    .av-dropzone p {
      font-size: 13px;
      color: var(--text-2);
      margin-top: 8px;
    }

    .av-dropzone span {
      font-size: 11px;
      color: var(--text-3);
    }

  .dz-link {
    color: var(--primary);
    font-weight: 500;
  }

  .av-preview {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
  }

    .av-preview img {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid var(--primary);
    }

  .av-remove {
    font-size: 11px;
    color: var(--danger);
    background: none;
    border: none;
    cursor: pointer;
    font-family: 'DM Sans',sans-serif;
    transition: opacity .2s;
  }

    .av-remove:hover {
      opacity: .7;
    }

  @media (max-width:600px) {
    .form-grid-2 {
      grid-template-columns: 1fr;
    }

    .av-grid {
      grid-template-columns: repeat(5,44px);
      gap: 8px;
    }

    .avatar-picker-layout {
      flex-direction: column;
    }
  }
</style>
