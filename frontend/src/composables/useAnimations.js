/**
 * Animacje scroll / hero / ripple przycisków (klasy .anim z main.css).
 * usePageAnimations — podłącz na widoku; observeAnimationsIn — po załadowaniu list z API.
 *
 * Uwaga: obserwatory muszą używać natywnego API — VueUse composables wymagają
 * synchronicznego wywołania w setup(), a te funkcje działają też z onMounted/watch.
 */
import { onMounted, onUnmounted, nextTick } from 'vue';

/** Hero: elementy .hero .anim pojawiają się od razu (jak w index.html). */
export function runHeroAnimations() {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      document.querySelectorAll('.hero .anim').forEach((el) => {
        el.classList.add('is-visible');
      });
    });
  });
}

/** Scroll: .anim poza sekcją .hero — IntersectionObserver. */
export function setupScrollAnimations(root = document) {
  const obs = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          obs.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.1, rootMargin: '0px 0px -32px 0px' },
  );

  root.querySelectorAll('.anim').forEach((el) => {
    if (!el.closest('.hero')) obs.observe(el);
  });

  return obs;
}

/** Obserwuje nowe elementy .anim (np. po załadowaniu listy z API). */
export function observeAnimationsIn(root = document) {
  const obs = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          obs.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.1, rootMargin: '0px 0px -32px 0px' },
  );

  root.querySelectorAll('.anim:not(.is-visible)').forEach((el) => {
    if (!el.closest('.hero')) obs.observe(el);
  });

  return obs;
}

/** Efekt ripple na przyciskach .btn (jak w mockupie strony głównej). */
export function setupButtonRipple(root = document) {
  const handler = function handleRipple(e) {
    const rect = this.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const ripple = document.createElement('span');
    Object.assign(ripple.style, {
      position: 'absolute',
      borderRadius: '50%',
      width: `${size}px`,
      height: `${size}px`,
      left: `${e.clientX - rect.left - size / 2}px`,
      top: `${e.clientY - rect.top - size / 2}px`,
      background: 'rgba(255,255,255,.25)',
      transform: 'scale(0)',
      animation: 'ripple .5s linear',
      pointerEvents: 'none',
    });
    if (getComputedStyle(this).position === 'static') this.style.position = 'relative';
    if (getComputedStyle(this).overflow === 'visible') this.style.overflow = 'hidden';
    this.appendChild(ripple);
    setTimeout(() => ripple.remove(), 500);
  };

  const buttons = root.querySelectorAll('.btn');
  buttons.forEach((btn) => btn.addEventListener('click', handler));

  return () => buttons.forEach((btn) => btn.removeEventListener('click', handler));
}

function resolveScope(root) {
  if (!root) return document;
  if (root instanceof HTMLElement || root === document) return root;
  if (typeof root === 'object' && root.value) return root.value;
  return document;
}

/**
 * @param {{ hero?: boolean, scroll?: boolean, ripple?: boolean, root?: HTMLElement | Document | import('vue').Ref }} options
 */
export function usePageAnimations(options = {}) {
  const { hero = false, scroll = true, ripple = true, root } = options;
  let observer;
  let cleanupRipple;

  onMounted(async () => {
    await nextTick();
    const scope = resolveScope(root);

    if (hero) runHeroAnimations();
    if (scroll) observer = setupScrollAnimations(scope);
    if (ripple) cleanupRipple = setupButtonRipple(scope);
  });

  onUnmounted(() => {
    observer?.disconnect();
    cleanupRipple?.();
  });
}