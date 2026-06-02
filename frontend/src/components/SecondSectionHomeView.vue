<script setup>
  import { ref, onMounted } from 'vue';
  import { RouterLink } from 'vue-router';
  import { userBooksApi } from '../lib/userBooks';
  import { mapUserBookToCard } from '../lib/mappers';

  const recentBooks = ref([]);
  const loadingRecent = ref(true);

  onMounted(async () => {
    try {
      const items = await userBooksApi.getRecent(5);
      recentBooks.value = items.map(mapUserBookToCard);
    } catch {
      recentBooks.value = [];
    } finally {
      loadingRecent.value = false;
    }
  });

  const badgeClass = (type) => ({ loan: 'badge-loan', swap: 'badge-swap', sale: 'badge-sale' }[type] || '');
  const badgeText = (type) => ({ loan: 'Do wypożyczenia', swap: 'Do wymiany', sale: 'Do sprzedaży' }[type] || '');

  // ── Hero animates immediately, staggered by delay classes ──
  const heroEls = document.querySelectorAll('.hero');
  heroEls.forEach(el => {
    // small tick so the browser registers the initial state
    requestAnimationFrame(() => requestAnimationFrame(() => {
      el.classList.add('is-visible');
    }));
  });

  // ── Everything else animates on scroll ──
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible');
        observer.unobserve(entry.target);
      }
    });
  }, { threshold: 0.1, rootMargin: '0px 0px -32px 0px' });

  document.querySelectorAll(':not(.hero)').forEach(el => observer.observe(el));

  // ── Button ripple feedback ──
  document.querySelectorAll('.btn').forEach(btn => {
    btn.addEventListener('click', function (e) {
      const r = document.createElement('span');
      const rect = this.getBoundingClientRect();
      const size = Math.max(rect.width, rect.height);
      Object.assign(r.style, {
        position: 'absolute', borderRadius: '50%',
        width: size + 'px', height: size + 'px',
        left: (e.clientX - rect.left - size / 2) + 'px',
        top: (e.clientY - rect.top - size / 2) + 'px',
        background: 'rgba(255,255,255,.25)',
        transform: 'scale(0)',
        animation: 'ripple .5s linear',
        pointerEvents: 'none',
      });
      this.style.position = 'relative';
      this.style.overflow = 'hidden';
      this.appendChild(r);
      setTimeout(() => r.remove(), 500);
    });
  });
</script>

<template>
  <!-- ════════════════════════ ŚWIEŻE OFERTY ════════════════════════ -->
  <section class="section">
    <div class="section-inner">
      <div class="section-header">
        <h2 class="section-title">Świeże oferty</h2>
        <RouterLink to="/przegladanie" class="section-link">
          Zobacz wszystkie
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M2 7h10M7 2l5 5-5 5" />
          </svg>
        </RouterLink>
      </div>

      <p v-if="loadingRecent" class="section-hint">Ładowanie ofert…</p>
      <p v-else-if="recentBooks.length === 0" class="section-hint">Brak ofert — dodaj pierwszą książkę!</p>

      <div v-else class="books-grid">
        <div v-for="book in recentBooks" :key="book.id" class="book-card-wrap">
          <RouterLink :to="'/przegladanie'" class="book-card">
            <div class="book-cover-wrap">
              <img :src="book.coverUrl || `https://picsum.photos/180/240?random=${book.img}`" :alt="book.title" class="book-cover">
              <span :class="['book-badge', badgeClass(book.type)]">{{ badgeText(book.type) }}</span>
              <span class="book-action">Zobacz ofertę</span>
            </div>
            <div class="book-info">
              <h4 class="book-title">{{ book.title }}</h4>
              <p class="book-author">{{ book.author }}</p>
            </div>
          </RouterLink>
          <div class="book-meta">
            <img :src="book.avatarUrl || `https://i.pravatar.cc/40?img=${book.av}`" :alt="book.owner" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@{{ book.owner }}</span> · {{ book.city }}
            </div>
          </div>
        </div>
      </div><!-- /books-grid -->
    </div>
  </section>
</template>
