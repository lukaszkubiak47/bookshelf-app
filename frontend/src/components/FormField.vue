<script setup lang="ts">
  /** Pole formularza auth z ikoną (Iconify), walidacją wizualną i przełącznikiem hasła. */
  import { ref, computed } from 'vue'
  import { Icon } from '@iconify/vue'
  import { T } from '../assets/constants.js'
  import { formIcons, type FormIconName } from '../lib/icons'
  import {
    fieldHasRightSpace,
    resolveIconColor,
    resolveInputType,
    type FieldState,
  } from '../lib/formFieldLogic'

  const props = withDefaults(
    defineProps<{
      label?: string
      type?: string
      icon?: FormIconName
      placeholder?: string
      modelValue?: string
      state?: FieldState
      error?: string
    }>(),
    {
      type: 'text',
      modelValue: '',
      state: 'idle',
    },
  )

  defineEmits<{
    'update:modelValue': [value: string]
    blur: []
  }>()

  const showPw = ref(false)

  const inputType = computed(() => resolveInputType(props.type, showPw.value))

  const hasRightSpace = computed(() => fieldHasRightSpace(props.type, props.state))

  const iconColor = computed(() =>
    resolveIconColor(props.state, { danger: T.danger, success: T.success, t3: T.t3 }),
  )

  const leftIcon = computed(() => (props.icon ? formIcons[props.icon] : null))
</script>

<template>
  <div :style="{ display: 'flex', flexDirection: 'column', gap: '5px' }">
    <label
      v-if="label"
      :style="{
        fontSize: '12px',
        fontWeight: 500,
        color: T.t2,
        letterSpacing: '0.3px',
        userSelect: 'none',
      }"
    >
      {{ label }}
    </label>

    <div :style="{ position: 'relative', display: 'flex', alignItems: 'center' }">
      <span
        v-if="leftIcon"
        :style="{ position: 'absolute', left: '13px', display: 'flex', pointerEvents: 'none', zIndex: 1 }"
      >
        <Icon :icon="leftIcon" width="15" height="15" :color="iconColor" />
      </span>

      <input
        :type="inputType"
        :value="modelValue"
        @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
        @blur="$emit('blur')"
        :placeholder="placeholder"
        :data-s="state === 'idle' ? undefined : state"
        :class="['bk-input', { 'has-left': icon, 'has-right': hasRightSpace }]"
        :autoComplete="type === 'password' ? 'current-password' : undefined"
      />

      <button
        v-if="type === 'password'"
        type="button"
        :tabIndex="-1"
        @click="showPw = !showPw"
        :style="{
          position: 'absolute',
          right: '12px',
          background: 'none',
          border: 'none',
          cursor: 'pointer',
          display: 'flex',
          padding: '2px',
          color: T.t3,
        }"
      >
        <Icon
          :icon="showPw ? formIcons.eyeOff : formIcons.eye"
          width="15"
          height="15"
          :color="T.t3"
        />
      </button>

      <span
        v-if="type !== 'password' && state === 'error'"
        :style="{ position: 'absolute', right: '12px', display: 'flex' }"
      >
        <Icon :icon="formIcons.xmark" width="15" height="15" :color="T.danger" />
      </span>
      <span
        v-if="type !== 'password' && state === 'success'"
        :style="{ position: 'absolute', right: '12px', display: 'flex' }"
      >
        <Icon :icon="formIcons.check" width="15" height="15" :color="T.success" />
      </span>
    </div>

    <p v-if="error" :style="{ fontSize: '12px', color: T.danger, marginTop: '1px', lineHeight: 1.3 }">
      {{ error }}
    </p>
    <slot name="hint" :state="state" />
  </div>
</template>
