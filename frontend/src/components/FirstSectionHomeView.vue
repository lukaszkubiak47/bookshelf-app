<template>
  <!-- ════════════════════════ HERO ════════════════════════ -->
  <section class="hero">
    <div class="hero-inner">

      <!-- LEFT -->
      <div class="hero-left">
        <div class="hero-eyebrow">
          <span class="hero-eyebrow-dot"></span>
          Społeczność czytelników
        </div>

        <h1 class="hero-title">
          Twoje książki<br>znajdą <em>nowy dom</em>
        </h1>

        <p class="hero-subtitle2">
          Wymieniaj, wypożyczaj i sprzedawaj książki lokalnie.
          Dołącz do społeczności czytelników.
        </p>

        <div class="hero-ctas">
          <RouterLink to="/przegladanie" class="btn btn-filled btn-lg">Przeglądaj książki</RouterLink>
          <RouterLink to="/logowanie" class="btn btn-outline btn-lg">Dołącz za darmo</RouterLink>
        </div>

        <div class="hero-stats">
          <div class="hero-stat">
            <span class="hero-stat-num">12 400+</span>
            <span class="hero-stat-label">Książek w obiegu</span>
          </div>
          <div class="stat-divider"></div>
          <div class="hero-stat">
            <span class="hero-stat-num">3 200+</span>
            <span class="hero-stat-label">Czytelników</span>
          </div>
          <div class="stat-divider"></div>
          <div class="hero-stat">
            <span class="hero-stat-num">86</span>
            <span class="hero-stat-label">Miast w Polsce</span>
          </div>
        </div>
      </div>

      <!-- RIGHT — decorative book grid -->
      <div class="hero-right">
        <div class="hero-blob"></div>
        <div class="hero-books">
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=10" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=11" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=12" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=13" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=14" alt="Book cover"></div>
          <div class="hero-book"><img src="https://picsum.photos/112/150?random=15" alt="Book cover"></div>
        </div>
      </div>

    </div>
  </section>
</template>
