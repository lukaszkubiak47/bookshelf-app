export interface RegisterFormValues {
  firstName: string
  lastName: string
  username: string
  email: string
  city: string
  password: string
  confirmPw: string
}

export function passwordStrength(password: string): number {
  if (!password) return 0
  let score = 0
  if (password.length >= 8) score++
  if (/[A-Z]/.test(password)) score++
  if (/[0-9]/.test(password)) score++
  if (/[^A-Za-z0-9]/.test(password)) score++
  return score
}

export function computeRegisterErrors(
  values: RegisterFormValues,
  termsAccepted: boolean,
): Record<string, string> {
  const errors: Record<string, string> = {}

  if (!values.firstName) errors.firstName = 'Wymagane'
  if (!values.lastName) errors.lastName = 'Wymagane'

  if (!values.username) errors.username = 'Nick jest wymagany'
  else if (values.username.length < 3) errors.username = 'Min. 3 znaki'
  else if (!/^[a-zA-Z0-9_]+$/.test(values.username)) errors.username = 'Tylko litery, cyfry i _'

  if (!values.email) errors.email = 'Email jest wymagany'
  else if (!/\S+@\S+\.\S+/.test(values.email)) errors.email = 'Nieprawidłowy format'

  if (!values.password) errors.password = 'Hasło jest wymagane'
  else if (values.password.length < 8) errors.password = 'Min. 8 znaków'

  if (!values.confirmPw) errors.confirmPw = 'Potwierdź hasło'
  else if (values.password !== values.confirmPw) errors.confirmPw = 'Hasła nie pasują'

  if (!termsAccepted) errors.terms = 'Akceptacja wymagana'

  return errors
}
