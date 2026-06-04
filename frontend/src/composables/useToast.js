/**
 * Globalny toast — provide w App.vue, inject w komponentach potomnych.
 */
import { inject, provide, reactive } from 'vue'

import { toastKey } from '../injectionKeys'

import { toastIcons } from '../lib/icons'

export function provideToast() {
  const toast = reactive({
    visible: false,
    message: '',
    type: 'success',
    icon: toastIcons.success,
  })

  let timer = null

  function showToast({ message, type = 'success' }) {
    toast.message = message
    toast.type = type
    toast.icon = type === 'success' ? toastIcons.success : toastIcons.error
    toast.visible = true
    clearTimeout(timer)
    timer = setTimeout(() => {
      toast.visible = false
    }, 2800)
  }

  provide(toastKey, showToast)

  return { toast, showToast }
}

export function useToast() {
  const showToast = inject(toastKey, null)
  if (!showToast) {
    throw new Error('useToast() wymaga provideToast() w App.vue')
  }
  return showToast
}
