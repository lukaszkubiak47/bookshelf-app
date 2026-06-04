export type FieldState = 'idle' | 'error' | 'success'

export interface FieldColorTokens {
  danger: string
  success: string
  t3: string
}

export function resolveInputType(type: string, showPassword: boolean): string {
  if (type === 'password') return showPassword ? 'text' : 'password'
  return type
}

export function resolveIconColor(state: FieldState, tokens: FieldColorTokens): string {
  if (state === 'error') return tokens.danger
  if (state === 'success') return tokens.success
  return tokens.t3
}

export function fieldHasRightSpace(type: string, state: FieldState): boolean {
  return type === 'password' || state !== 'idle'
}
