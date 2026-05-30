<script setup>
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
        <a href="#" class="section-link">
          Zobacz wszystkie
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M2 7h10M7 2l5 5-5 5" />
          </svg>
        </a>
      </div>

      <div class="books-grid">

        <!-- KARTA 1 -->
        <div class="book-card-wrap">
          <div class="book-card">
            <div class="book-cover-wrap">
              <img src="https://picsum.photos/180/240?random=1" alt="Okładka" class="book-cover">
              <span class="book-badge badge-swap">Do wymiany</span>
              <button class="book-action">Zobacz ofertę</button>
            </div>
            <div class="book-info">
              <h4 class="book-title">Sto lat samotności</h4>
              <p class="book-author">Gabriel García Márquez</p>
            </div>
          </div>
          <div class="book-meta">
            <img src="https://i.pravatar.cc/40?img=1" alt="Avatar" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@maria_kow</span> · Warszawa<br>2 godz. temu
            </div>
          </div>
        </div>

        <!-- KARTA 2 -->
        <div class="book-card-wrap">
          <div class="book-card">
            <div class="book-cover-wrap">
              <img src="https://picsum.photos/180/240?random=2" alt="Okładka" class="book-cover">
              <span class="book-badge badge-loan" style="font-size: 9px">DO WYPOŻYCZENIA</span>
              <button class="book-action">Zobacz ofertę</button>
            </div>
            <div class="book-info">
              <h4 class="book-title">Diuna</h4>
              <p class="book-author">Frank Herbert</p>
            </div>
          </div>
          <div class="book-meta">
            <img src="https://i.pravatar.cc/40?img=5" alt="Avatar" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@piotr_n</span> · Kraków<br>3 godz. temu
            </div>
          </div>
        </div>

        <!-- KARTA 3 -->
        <div class="book-card-wrap">
          <div class="book-card">
            <div class="book-cover-wrap">
              <img src="https://picsum.photos/180/240?random=3" alt="Okładka" class="book-cover">
              <span class="book-badge badge-sale">Do sprzedaży</span>
              <button class="book-action">Zobacz ofertę</button>
            </div>
            <div class="book-info">
              <h4 class="book-title">Wiedźmin: Ostatnie życzenie</h4>
              <p class="book-author">Andrzej Sapkowski</p>
            </div>
          </div>
          <div class="book-meta">
            <img src="https://i.pravatar.cc/40?img=9" alt="Avatar" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@agata_ww</span> · Gdańsk<br>5 godz. temu
            </div>
          </div>
        </div>

        <!-- KARTA 4 -->
        <div class="book-card-wrap">
          <div class="book-card">
            <div class="book-cover-wrap">
              <img src="https://picsum.photos/180/240?random=4" alt="Okładka" class="book-cover">
              <span class="book-badge badge-swap">Do wymiany</span>
              <button class="book-action">Zobacz ofertę</button>
            </div>
            <div class="book-info">
              <h4 class="book-title">Mistrz i Małgorzata</h4>
              <p class="book-author">Michaił Bułhakow</p>
            </div>
          </div>
          <div class="book-meta">
            <img src="https://i.pravatar.cc/40?img=3" alt="Avatar" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@tomek_r</span> · Wrocław<br>8 godz. temu
            </div>
          </div>
        </div>

        <!-- KARTA 5 -->
        <div class="book-card-wrap">
          <div class="book-card">
            <div class="book-cover-wrap">
              <img src="https://picsum.photos/180/240?random=5" alt="Okładka" class="book-cover">
              <span class="book-badge badge-loan" style="font-size: 9px">Do wypożyczenia</span>
              <button class="book-action">Zobacz ofertę</button>
            </div>
            <div class="book-info">
              <h4 class="book-title">Zbrodnia i kara</h4>
              <p class="book-author">Fiodor Dostojewski</p>
            </div>
          </div>
          <div class="book-meta">
            <img src="https://i.pravatar.cc/40?img=7" alt="Avatar" class="meta-avatar">
            <div class="meta-text">
              <span class="meta-nick">@kasia_pzn</span> · Poznań<br>wczoraj
            </div>
          </div>
        </div>

      </div><!-- /books-grid -->
    </div>
  </section>
</template>
