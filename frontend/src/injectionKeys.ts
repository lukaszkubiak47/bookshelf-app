import type { InjectionKey } from 'vue'

export interface ToastPayload {
  message: string
  type?: 'success' | 'error'
}

export type ShowToast = (payload: ToastPayload) => void

export type DesignTokens = Record<string, string>

export const toastKey: InjectionKey<ShowToast> = Symbol('toast')
export const designTokensKey: InjectionKey<DesignTokens> = Symbol('designTokens')
