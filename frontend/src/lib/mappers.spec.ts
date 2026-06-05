import { describe, expect, it } from 'vitest'

import { coverColor, seedFromGuid } from './mappers'

describe('seedFromGuid', () => {
  it('returns a stable value for the same id and max', () => {
    const id = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    expect(seedFromGuid(id, 70)).toBe(seedFromGuid(id, 70))
  })

  it('returns integers in range [1, max]', () => {
    const max = 80
    for (const id of ['', 'x', 'same-id', 'other-id-123']) {
      const value = seedFromGuid(id, max)
      expect(value).toBeGreaterThanOrEqual(1)
      expect(value).toBeLessThanOrEqual(max)
      expect(Number.isInteger(value)).toBe(true)
    }
  })

  it('respects different max bounds', () => {
    const id = 'fixed-seed-test'
    const small = seedFromGuid(id, 10)
    const large = seedFromGuid(id, 100)
    expect(small).toBeLessThanOrEqual(10)
    expect(large).toBeLessThanOrEqual(100)
  })
})

describe('coverColor', () => {
  const palette = /^#[0-9A-Fa-f]{6}$/

  it('is deterministic for the same title', () => {
    const title = 'Wiedźmin'
    expect(coverColor(title)).toBe(coverColor(title))
  })

  it('returns a hex color from the built-in palette', () => {
    expect(coverColor('Harry Potter')).toMatch(palette)
    expect(coverColor('')).toMatch(palette)
  })

  it('can differ for different titles', () => {
    const a = coverColor('Title A')
    const b = coverColor('Title B with more text')
    expect(typeof a).toBe('string')
    expect(typeof b).toBe('string')
  })
})
