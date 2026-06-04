import type { Directive, DirectiveBinding } from 'vue'

type ClickOutsideHandler = (event: MouseEvent) => void

interface ClickOutsideElement extends HTMLElement {
  _clickOutsideHandler?: (event: MouseEvent) => void
}

/** Zamyka dropdown/menu po kliknięciu poza elementem (handler w binding.value). */
export const clickOutside: Directive<ClickOutsideElement, ClickOutsideHandler | undefined> = {
  mounted(el, binding: DirectiveBinding<ClickOutsideHandler | undefined>) {
    const handler = (event: MouseEvent) => {
      if (!el.contains(event.target as Node)) {
        binding.value?.(event)
      }
    }
    el._clickOutsideHandler = handler
    document.addEventListener('click', handler)
  },
  unmounted(el) {
    const handler = el._clickOutsideHandler
    if (handler) {
      document.removeEventListener('click', handler)
      delete el._clickOutsideHandler
    }
  },
}
