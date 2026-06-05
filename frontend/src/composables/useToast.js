/**
 * Globalny toast — provide w App.vue, inject w komponentach potomnych.
 */
import { inject, provide, reactive } from 'vue'

import { toastKey } from '../injectionKeys'

const SUCCESS_ICON =
  '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round"><polyline points="20 6 9 17 4 12"/></svg>'

const ERROR_ICON =
  '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>'

export function provideToast() {
  const toast = reactive({
    visible: false,
    message: '',
    type: 'success',
    icon: '',
  })

  let timer = null

  function showToast({ message, type = 'success' }) {
    toast.message = message
    toast.type = type
    toast.icon = type === 'success' ? SUCCESS_ICON : ERROR_ICON
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
