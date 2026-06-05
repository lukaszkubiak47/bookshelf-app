/**
 * Composable: animacje strony — scroll reveal, hero, efekt ripple na przyciskach.
 *
 * Cel: centralna logika animacji oparta na klasach CSS (.anim → .is-visible) i IntersectionObserver.
 * Kiedy używać: usePageAnimations() w widoku/sekcji; observeAnimationsIn() po dynamicznym dodaniu elementów (np. lista z API).
 *
 * Powiązane pliki:
 * - assets/main.css — style .anim, .hero, @keyframes ripple
 * - views/ShelfView.vue, BrowseView.vue, AddBookView.vue — usePageAnimations
 * - components/FirstSectionHomeView.vue, SecondSectionHomeView.vue — sekcje strony głównej
 * - useAnimations.d.ts — typy TypeScript dla tego modułu
 */
import { onMounted, onUnmounted, nextTick, shallowRef, onScopeDispose, computed } from 'vue';
import { useIntersectionObserver, useEventListener } from '@vueuse/core';

const SCROLL_IO = { threshold: 0.1, rootMargin: '0px 0px -32px 0px' };

// Rejestr skanerów — pozwala observeAnimationsIn() odświeżyć obserwatory po załadowaniu nowych .anim
const scrollScanners = new Set();

// --- Hero: natychmiastowe ujawnienie elementów w sekcji .hero ---

export function runHeroAnimations() {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      document.querySelectorAll('.hero .anim').forEach((el) => {
        el.classList.add('is-visible');
      });
    });
  });
}

// --- Pomocnicze: rozwiązanie root (document / HTMLElement / Ref) i zbieranie celów ---

function resolveScope(root) {
  if (!root) return document;
  if (root instanceof HTMLElement || root === document) return root;
  if (typeof root === 'object' && root.value) return root.value;
  return document;
}

function collectAnimTargets(root) {
  const scope = resolveScope(root);
  if (!scope?.querySelectorAll) return [];
  return [...scope.querySelectorAll('.anim')].filter(
    (el) => !el.closest('.hero') && !el.classList.contains('is-visible'),
  );
}

// --- Scroll reveal: IntersectionObserver przez @vueuse/core ---

function useScrollAnimTargets() {
  const targets = shallowRef([]);

  useIntersectionObserver(
    targets,
    (entries, observer) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          observer.unobserve(entry.target);
        }
      });
    },
    SCROLL_IO,
  );

  const scan = (root) => {
    const found = collectAnimTargets(root);
    if (!found.length) return;
    const merged = new Set(targets.value);
    found.forEach((el) => merged.add(el));
    targets.value = [...merged];
  };

  scrollScanners.add(scan);
  onScopeDispose(() => scrollScanners.delete(scan));

  return { scan };
}

/** Fallback gdy observeAnimationsIn wywołane przed montowaniem usePageAnimations. */
function observeAnimationsLegacy(root = document) {
  const obs = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          obs.unobserve(entry.target);
        }
      });
    },
    SCROLL_IO,
  );

  collectAnimTargets(root).forEach((el) => obs.observe(el));
  return obs;
}

/** Obserwuje nowe elementy .anim (np. po załadowaniu listy z API). */
export function observeAnimationsIn(root = document) {
  const scope = resolveScope(root);
  if (scrollScanners.size) {
    scrollScanners.forEach((scan) => scan(scope));
    return;
  }
  return observeAnimationsLegacy(scope);
}

/** @deprecated alias — używaj observeAnimationsIn */
export function setupScrollAnimations(root = document) {
  return observeAnimationsIn(root);
}

// --- Ripple: delegacja kliknięć na .btn w obrębie root ---

function createRippleHandler() {
  return function handleRipple(e) {
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
}

/**
 * Główny composable strony — włącza wybrane animacje po onMounted.
 * @param {{ hero?: boolean, scroll?: boolean, ripple?: boolean, root?: HTMLElement | Document | import('vue').Ref }} options
 */
export function usePageAnimations(options = {}) {
  const { hero = false, scroll = true, ripple = true, root } = options;
  let scrollScan = () => {};

  if (scroll) {
    ({ scan: scrollScan } = useScrollAnimTargets());
  }

  const eventRoot = computed(() => resolveScope(root) || document);

  if (ripple) {
    useEventListener(eventRoot, 'click', (e) => {
      const btn = e.target?.closest?.('.btn');
      if (!btn || !eventRoot.value?.contains(btn)) return;
      createRippleHandler().call(btn, e);
    });
  }

  onMounted(async () => {
    await nextTick();
    const scope = resolveScope(root);

    if (hero) runHeroAnimations();
    if (scroll) scrollScan(scope);
  });
}
