<script setup>
  document.addEventListener('DOMContentLoaded', () => {
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
    });
</script>

<template>
  <!-- ════════════════════════ JAK TO DZIAŁA ════════════════════════ -->
  <section class="how-section">
    <div class="section-inner">

      <div class="how-header">
        <h2 class="section-title">Jak to działa?</h2>
        <p>Trzy proste kroki dzielą Cię od wymarzonej lektury</p>
      </div>

      <div class="how-cards">

        <!-- Krok 1 -->
        <div class="how-card">
          <div class="how-step-num">1</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="7" />
              <line x1="16.5" y1="16.5" x2="22" y2="22" />
            </svg>
          </div>
          <h3>Znajdź książkę</h3>
          <p>Przeglądaj tysiące ofert od czytelników w Twoim mieście. Filtruj po gatunku, autorze i typie oferty.</p>
        </div>

        <!-- Krok 2 -->
        <div class="how-card">
          <div class="how-step-num">2</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="17 1 21 5 17 9" />
              <path d="M3 11V9a4 4 0 0 1 4-4h14" />
              <polyline points="7 23 3 19 7 15" />
              <path d="M21 13v2a4 4 0 0 1-4 4H3" />
            </svg>
          </div>
          <h3>Wyślij prośbę</h3>
          <p>Skontaktuj się z właścicielem jednym kliknięciem. Zaproponuj wymianę lub umów się na wypożyczenie.</p>
        </div>

        <!-- Krok 3 -->
        <div class="how-card">
          <div class="how-step-num">3</div>
          <div class="how-icon-wrap">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none"
                 stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 11h1a3 3 0 0 1 0 6h-1" />
              <path d="M9 12H7a3 3 0 0 0 0 6h2" />
              <line x1="9" y1="15" x2="15" y2="15" />
              <path d="M7 7h1a2 2 0 0 0 0-4H4v18" />
              <path d="M17 7h-1a2 2 0 0 1 0-4h3v18" />
            </svg>
          </div>
          <h3>Dogadajcie się</h3>
          <p>Ustalcie szczegóły przez wbudowany czat. Spotkajcie się lokalnie lub wyślijcie pocztą — jak wam wygodnie.</p>
        </div>

      </div>
    </div>
  </section>

</template>

<style scoped>
  /* ══════════ HOW IT WORKS ══════════ */
  .how-section {
    background: var(--primary);
    padding: 88px 48px;
  }

  .how-header {
    text-align: center;
    margin-bottom: 60px;
  }

    .how-header .section-title {
      color: #fff;
      margin-bottom: 12px;
    }

    .how-header p {
      color: rgba(255,255,255,.62);
      font-size: 16px;
      font-weight: 300;
    }

  .how-cards {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 24px;
    max-width: 960px;
    margin: 0 auto;
  }

  .how-card {
    background: rgba(255,255,255,.07);
    border: 1px solid rgba(255,255,255,.11);
    border-radius: var(--r-card);
    padding: 44px 32px;
    text-align: center;
    position: relative;
    overflow: hidden;
    transition: all .25s ease;
  }

    .how-card:hover {
      background: rgba(255,255,255,.13);
      transform: translateY(-3px);
    }

  .how-step-num {
    position: absolute;
    top: 14px;
    right: 20px;
    font-family: 'Playfair Display', serif;
    font-size: 60px;
    font-weight: 700;
    color: rgba(255,255,255,.06);
    line-height: 1;
    user-select: none;
  }

  .how-icon-wrap {
    width: 62px;
    height: 62px;
    background: rgba(255,255,255,.13);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 28px;
  }

  .how-card h3 {
    font-family: 'Playfair Display', serif;
    font-size: 22px;
    font-weight: 600;
    color: #fff;
    margin-bottom: 12px;
  }

  .how-card p {
    color: rgba(255,255,255,.65);
    font-size: 14px;
    line-height: 1.75;
    font-weight: 300;
  }
</style>
