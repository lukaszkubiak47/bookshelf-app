export const T: {
  bg: string
  white: string
  green: string
  greenHov: string
  greenLoading: string
  gold: string
  blue: string
  t1: string
  t2: string
  t3: string
  border: string
  tagBg: string
  danger: string
  success: string
  successBg: string
  errorBg: string
  warning: string
}

export const P: Record<string, string>

export const AUTH_STYLES: {
  successIconCircle: Record<string, string | number>
  successScreen: Record<string, string | number>
  errorAlert: Record<string, string>
  submitButton: (loading: boolean, hov: boolean) => Record<string, string | number>
  submitSpinner: Record<string, string | number>
  heading: Record<string, string | number>
  subheading: Record<string, string>
  successTitle: Record<string, string | number>
}

export const AUTH_DEMO_BOOK_BADGE: Record<string, { bg: string; text: string }>
