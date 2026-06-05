/**
 * Dyrektywa v-click-outside — zamyka dropdowny i menu po kliknięciu poza elementem.
 *
 * Kiedy tu zajrzeć: przy modalach, filtrach, menu kontekstowych (np. v-click-outside na wrapperze).
 * Używa VueUse onClickOutside; handler przekazujesz w wartości dyrektywy.
 *
 * Eksport: clickOutside (Vue Directive).
 * Backend: brak.
 */
import type { Directive, DirectiveBinding } from 'vue'
import { onClickOutside } from '@vueuse/core'

type ClickOutsideHandler = (event: MouseEvent) => void

interface ClickOutsideElement extends HTMLElement {
  _stopClickOutside?: () => void
}

export const clickOutside: Directive<ClickOutsideElement, ClickOutsideHandler | undefined> = {
  mounted(el, binding: DirectiveBinding<ClickOutsideHandler | undefined>) {
    el._stopClickOutside = onClickOutside(el, (event) => binding.value?.(event))
  },
  unmounted(el) {
    el._stopClickOutside?.()
    delete el._stopClickOutside
  },
}
