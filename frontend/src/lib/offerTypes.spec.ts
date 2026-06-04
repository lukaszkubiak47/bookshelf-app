import { describe, expect, it } from 'vitest'

import {
  isOfferType,
  offerBadgeClass,
  offerBadgeMeta,
  offerBrowseLabel,
  offerListedBadgeClass,
  offerTxCardClass,
  offerTxLabel,
  offerTxTypeClass,
} from './offerTypes'

describe('isOfferType', () => {
  it('accepts known offer types', () => {
    expect(isOfferType('loan')).toBe(true)
    expect(isOfferType('swap')).toBe(true)
    expect(isOfferType('sale')).toBe(true)
  })

  it('rejects unknown values', () => {
    expect(isOfferType('rent')).toBe(false)
    expect(isOfferType('')).toBe(false)
    expect(isOfferType('LOAN')).toBe(false)
  })
})

describe('offer type helpers', () => {
  it('maps loan to browse badge and label', () => {
    expect(offerBadgeClass('loan')).toBe('badge-loan')
    expect(offerBrowseLabel('loan')).toBe('Do wypożyczenia')
    expect(offerListedBadgeClass('loan')).toBe('lcb-loan')
    expect(offerTxLabel('loan')).toBe('Wypożyczenie')
    expect(offerTxTypeClass('loan')).toBe('type-loan')
    expect(offerTxCardClass('loan')).toBe('t-loan')
  })

  it('returns empty strings for invalid types', () => {
    expect(offerBadgeClass('unknown')).toBe('')
    expect(offerBrowseLabel('gift')).toBe('')
    expect(offerTxCardClass('')).toBe('')
  })

  it('combines badge class and label in offerBadgeMeta', () => {
    expect(offerBadgeMeta('swap')).toEqual({
      cls: 'badge-swap',
      label: 'Do wymiany',
    })
    expect(offerBadgeMeta('invalid')).toEqual({ cls: '', label: '' })
  })
})
