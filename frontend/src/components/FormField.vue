<script setup>
  /**
   * Wspólne pole formularza auth z ikoną, walidacją wizualną i podglądem hasła.
   * Użycie: LoginForm.vue, RegisterForm.vue.
   * Props: label, type, icon, placeholder, modelValue, state (idle|error|success), error.
   * Emity: update:modelValue, blur.
   * Slot hint — np. pasek siły hasła w rejestracji.
   */
  import { ref, computed } from 'vue';
  import { T, P } from '../assets/constants.js';

  const props = defineProps({
    label: String,
    type: { type: String, default: 'text' },
    icon: String,
    placeholder: String,
    modelValue: String,
    state: { type: String, default: 'idle' },
    error: String
  });

  defineEmits(['update:modelValue', 'blur']);

  const showPw = ref(false);

  const inputType = computed(() => {
    if (props.type === 'password') {
      return showPw.value ? 'text' : 'password';
    }
    return props.type;
  });

  const hasRightSpace = computed(() => {
    return props.type === 'password' || props.state !== 'idle';
  });

  const iconColor = computed(() => {
    if (props.state === 'error') return T.danger;
    if (props.state === 'success') return T.success;
    return T.t3;
  });
</script>

<template>
  <div :style="{ display: 'flex', flexDirection: 'column', gap: '5px' }">
    <label v-if="label" :style="{ fontSize: '12px', fontWeight: 500, color: T.t2, letterSpacing: '0.3px', userSelect: 'none' }">
      {{ label }}
    </label>

    <div :style="{ position: 'relative', display: 'flex', alignItems: 'center' }">
      <span v-if="icon" :style="{ position: 'absolute', left: '13px', display: 'flex', pointerEvents: 'none', zIndex: 1 }">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
             :stroke="iconColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
             v-html="P[icon]" />
      </span>

      <input :type="inputType"
             :value="modelValue"
             @input="$emit('update:modelValue', $event.target.value)"
             @blur="$emit('blur')"
             :placeholder="placeholder"
             :data-s="state === 'idle' ? undefined : state"
             :class="['bk-input', { 'has-left': icon, 'has-right': hasRightSpace }]"
             :autoComplete="type === 'password' ? 'current-password' : undefined" />

      <button v-if="type === 'password'" type="button" :tabIndex="-1" @click="showPw = !showPw"
              :style="{ position: 'absolute', right: '12px', background: 'none', border: 'none', cursor: 'pointer', display: 'flex', padding: '2px', color: T.t3 }">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
             :stroke="T.t3" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
             v-html="showPw ? P.eyeOff : P.eye" />
      </button>

      <span v-if="type !== 'password' && state === 'error'" :style="{ position: 'absolute', right: '12px', display: 'flex' }">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" :stroke="T.danger" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.xmark" />
      </span>
      <span v-if="type !== 'password' && state === 'success'" :style="{ position: 'absolute', right: '12px', display: 'flex' }">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" :stroke="T.success" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.check" />
      </span>
    </div>

    <p v-if="error" :style="{ fontSize: '12px', color: T.danger, marginTop: '1px', lineHeight: 1.3 }">{{ error }}</p>
    <slot name="hint" :state="state" />
  </div>
</template>
