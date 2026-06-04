/**
 * Tokeny designu z constants.js — provide w App.vue, inject w formularzach auth.
 */
import { inject } from 'vue'

import { T } from '../assets/constants.js'
import { designTokensKey } from '../injectionKeys'

export function useDesignTokens() {
  return inject(designTokensKey, T)
}
