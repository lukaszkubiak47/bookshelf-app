import { describe, expect, it, vi } from 'vitest'
import type { Router } from 'vue-router'

import { openBrowseOffer } from './browseNavigation'

describe('openBrowseOffer', () => {
  it('navigates to browse with offer query', () => {
    const push = vi.fn()
    const router = { push } as unknown as Router

    openBrowseOffer(router, 'offer-uuid-123')

    expect(push).toHaveBeenCalledWith({
      name: 'browse',
      query: { offer: 'offer-uuid-123' },
    })
  })
})
