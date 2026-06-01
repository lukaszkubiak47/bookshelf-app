<script setup>
  import { ref, reactive } from 'vue';
  import { usersApi } from '../lib/users';
  import { mapUserToProfile } from '../lib/mappers';
  import { ApiError } from '../lib/client';
  import { useAuthStore } from '../stores/auth';

  const props = defineProps({
    user: { type: Object, required: true }
  });

  const emit = defineEmits(['update:user', 'notify', 'saved']);
  const auth = useAuthStore();

  const isEditMode = ref(false);
  const isDragging = ref(false);
  const fileInput = ref(null);
  const customAvatar = ref(null);
  const selectedPreset = ref(1);
  const saving = ref(false);

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

  const saveChanges = async () => {
    errors.name = !form.name.trim();
    errors.surname = !form.surname.trim();

    if (errors.name || errors.surname) return;

    saving.value = true;
    try {
      let profile = await usersApi.updateMe({
        firstName: form.name.trim(),
        lastName: form.surname.trim(),
        username: form.nick.trim(),
        city: form.city?.trim() || undefined,
      });

      if (customAvatar.value && fileInput.value?.files?.[0]) {
        profile = await usersApi.uploadAvatar(fileInput.value.files[0]);
      } else if (!customAvatar.value && selectedPreset.value) {
        profile = await usersApi.setAvatarPreset(Math.min(10, Math.max(1, selectedPreset.value)));
      }

      const mapped = mapUserToProfile(profile);
      auth.syncFromProfile(profile);
      emit('update:user', mapped);
      isEditMode.value = false;
      emit('saved');
      emit('notify', { message: 'Dane zostały zapisane ✓', type: 'success' });
    } catch (err) {
      emit('notify', {
        message: err instanceof ApiError ? err.message : 'Nie udało się zapisać zmian',
        type: 'error',
      });
    } finally {
      saving.value = false;
    }
  };
</script>

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
