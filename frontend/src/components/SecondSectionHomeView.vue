<script setup>
  // ── Hero animates immediately, staggered by delay classes ──
  const heroEls = document.querySelectorAll('.hero .anim');
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

  document.querySelectorAll('.anim:not(.hero .anim)').forEach(el => observer.observe(el));

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
        <h2 class="section-title anim">Świeże oferty</h2>
        <a href="#" class="section-link anim">
          Zobacz wszystkie
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
            <path d="M2 7h10M7 2l5 5-5 5" />
          </svg>
        </a>
      </div>

      <div class="books-grid">

        <!-- KARTA 1 -->
        <div class="book-card-wrap anim anim-d1">
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
        <div class="book-card-wrap anim anim-d2">
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
        <div class="book-card-wrap anim anim-d3">
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
        <div class="book-card-wrap anim anim-d4">
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
        <div class="book-card-wrap anim anim-d5">
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

<style scoped>


  /* ══════════ SECTION BASE ══════════ */
  .section {
    padding: 80px 48px;
  }

  .section-inner {
    max-width: 1200px;
    margin: 0 auto;
  }

  .section-header {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    margin-bottom: 40px;
  }

  .section-title {
    font-family: 'Playfair Display', serif;
    font-size: 36px;
    font-weight: 600;
    color: var(--text-1);
  }

  .section-link {
    color: var(--primary);
    text-decoration: none;
    font-size: 14px;
    font-weight: 500;
    transition: opacity .2s;
    display: flex;
    align-items: center;
    gap: 4px;
  }

    .section-link:hover {
      opacity: .7;
    }

  /* ══════════ BOOK CARDS ══════════ */
  .books-grid {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 24px;
  }

  .book-card-wrap {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .book-card {
    background: var(--surface);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    overflow: hidden;
    cursor: pointer;
    transition: all .22s ease;
    position: relative;
  }

    .book-card:hover {
      transform: translateY(-2px);
      box-shadow: var(--shadow-h);
    }

  .book-cover-wrap {
    position: relative;
    aspect-ratio: 3/4;
    overflow: hidden;
  }

  .book-cover {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform .32s ease;
    display: block;
  }

  .book-card:hover .book-cover {
    transform: scale(1.04);
  }

  .book-badge {
    position: absolute;
    top: 9px;
    left: 9px;
    padding: 3px 9px;
    border-radius: 4px;
    font-size: 10px;
    font-weight: 600;
    letter-spacing: .2px;
    text-transform: uppercase;
    z-index: 1;
  }

  .badge-loan {
    background: var(--primary);
    color: #fff;
  }

  .badge-swap {
    background: var(--accent);
    color: var(--text-1);
  }

  .badge-sale {
    background: var(--blue);
    color: #fff;
  }

  .book-action {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    padding: 11px;
    background: rgba(45,106,79,.92);
    backdrop-filter: blur(3px);
    color: #fff;
    border: none;
    cursor: pointer;
    font-family: 'DM Sans', sans-serif;
    font-size: 13px;
    font-weight: 500;
    letter-spacing: .2px;
    opacity: 0;
    transform: translateY(100%);
    transition: opacity .22s ease, transform .22s ease;
  }

  .book-card:hover .book-action {
    opacity: 1;
    transform: translateY(0);
  }

  .book-info {
    padding: 10px 12px 13px;
  }

  .book-title {
    font-family: 'Playfair Display', serif;
    font-size: 14px;
    font-weight: 500;
    color: var(--text-1);
    line-height: 1.3;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    margin-bottom: 4px;
  }

  .book-author {
    font-size: 12px;
    color: var(--text-2);
    margin-bottom: 7px;
  }

  .book-stars {
    color: var(--accent);
    font-size: 11px;
    letter-spacing: 1px;
  }

  .book-meta {
    display: flex;
    align-items: center;
    gap: 9px;
  }

  .meta-avatar {
    width: 28px;
    height: 28px;
    border-radius: 50%;
    object-fit: cover;
    flex-shrink: 0;
    border: 2px solid var(--border);
  }

  .meta-text {
    font-size: 11px;
    color: var(--text-2);
    line-height: 1.4;
  }

  .meta-nick {
    font-weight: 600;
    color: var(--text-1);
  }
</style>
