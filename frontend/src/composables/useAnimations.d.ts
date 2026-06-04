/** Typy TypeScript dla useAnimations.js (używane m.in. w ShelfView.vue). */
import type { Ref } from 'vue'

export interface PageAnimationsOptions {
  hero?: boolean
  scroll?: boolean
  ripple?: boolean
  root?: HTMLElement | Document | Ref<HTMLElement | null | undefined>
}

export function runHeroAnimations(): void
export function setupScrollAnimations(root?: HTMLElement | Document): IntersectionObserver | void
export function observeAnimationsIn(root?: HTMLElement | Document): IntersectionObserver | void
export function usePageAnimations(options?: PageAnimationsOptions): void
