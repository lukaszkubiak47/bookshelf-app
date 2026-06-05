/**
 * Deklaracje TypeScript dla useAnimations.js.
 *
 * Cel: typowanie opcji i eksportów animacji w plikach .vue / .ts (bez importu implementacji JS).
 * Kiedy używać: gdy komponent w TypeScript importuje funkcje z useAnimations.js i potrzebuje autouzupełniania.
 *
 * Powiązane pliki:
 * - composables/useAnimations.js — implementacja
 * - views/ShelfView.vue, BrowseView.vue — usePageAnimations + observeAnimationsIn
 * - assets/main.css — klasy .anim, .is-visible, animacja ripple
 */
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
