/**
 * Klucze provide/inject Vue — globalne usługi UI bez prop-drillingu.
 *
 * Kiedy tu zajrzeć: gdy komponent potrzebuje toastów lub tokenów designu z App.vue,
 * albo gdy dodajesz nową usługę udostępnianą przez provide().
 *
 * Eksporty: toastKey, designTokensKey, typy ToastPayload, ShowToast, DesignTokens.
 * Backend: brak — wyłącznie warstwa prezentacji.
 */
import type { InjectionKey } from 'vue'

/** Powiadomienia toast (App.vue provide → inject w komponentach) */
export interface ToastPayload {
  message: string
  type?: 'success' | 'error'
}

export type ShowToast = (payload: ToastPayload) => void

export type DesignTokens = Record<string, string>

/** Klucze Symbol używane w provide/inject */
export const toastKey: InjectionKey<ShowToast> = Symbol('toast')
export const designTokensKey: InjectionKey<DesignTokens> = Symbol('designTokens')
