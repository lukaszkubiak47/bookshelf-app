import { describe, expect, it } from 'vitest'

import {
  fieldHasRightSpace,
  resolveIconColor,
  resolveInputType,
} from './formFieldLogic'

const tokens = { danger: '#c00', success: '#0c0', t3: '#999' }

describe('resolveInputType', () => {
  it('toggles password visibility', () => {
    expect(resolveInputType('password', false)).toBe('password')
    expect(resolveInputType('password', true)).toBe('text')
  })

  it('passes through non-password types', () => {
    expect(resolveInputType('email', false)).toBe('email')
  })
})

describe('resolveIconColor', () => {
  it('maps validation state to palette', () => {
    expect(resolveIconColor('error', tokens)).toBe(tokens.danger)
    expect(resolveIconColor('success', tokens)).toBe(tokens.success)
    expect(resolveIconColor('idle', tokens)).toBe(tokens.t3)
  })
})

describe('fieldHasRightSpace', () => {
  it('reserves space for password toggle or status icon', () => {
    expect(fieldHasRightSpace('text', 'idle')).toBe(false)
    expect(fieldHasRightSpace('password', 'idle')).toBe(true)
    expect(fieldHasRightSpace('text', 'error')).toBe(true)
  })
})
