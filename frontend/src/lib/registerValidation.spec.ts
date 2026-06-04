import { describe, expect, it } from 'vitest'

import { computeRegisterErrors, passwordStrength } from './registerValidation'

const valid = {
  firstName: 'Jan',
  lastName: 'Kowalski',
  username: 'jankow',
  email: 'jan@example.com',
  city: 'Warszawa',
  password: 'Haslo123!',
  confirmPw: 'Haslo123!',
}

describe('passwordStrength', () => {
  it('scores zero for empty password', () => {
    expect(passwordStrength('')).toBe(0)
  })

  it('rewards length, case, digit and symbol', () => {
    expect(passwordStrength('Haslo123!')).toBe(4)
  })
})

describe('computeRegisterErrors', () => {
  it('returns no errors when form is valid and terms accepted', () => {
    expect(computeRegisterErrors(valid, true)).toEqual({})
  })

  it('requires matching passwords', () => {
    const errors = computeRegisterErrors({ ...valid, confirmPw: 'inne' }, true)
    expect(errors.confirmPw).toBe('Hasła nie pasują')
  })

  it('requires terms acceptance', () => {
    expect(computeRegisterErrors(valid, false).terms).toBe('Akceptacja wymagana')
  })
})
