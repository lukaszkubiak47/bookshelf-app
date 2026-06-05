/**
 * Composable: dostęp do tokenów designu (kolory, odstępy, czcionki) przez inject.
 *
 * Cel: udostępnia obiekt T z constants.js bez przekazywania propsów w głąb drzewa komponentów.
 * Kiedy używać: w formularzach lub komponentach UI, które potrzebują wartości z design systemu.
 *
 * Powiązane pliki:
 * - App.vue — provide(designTokensKey, T) na poziomie root
 * - assets/constants.js — źródło tokenów (T)
 * - injectionKeys.ts — designTokensKey (Symbol do inject/provide)
 */
import { inject } from 'vue'

import { T } from '../assets/constants.js'
import { designTokensKey } from '../injectionKeys'

export function useDesignTokens() {
  // Fallback na T, gdy provide nie został wywołany (np. w testach)
  return inject(designTokensKey, T)
}
