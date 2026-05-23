<template>
  <section class="hero">
    <div class="hero-inner">
      <div class="hero-left">
        <div class="hero-eyebrow anim">
          <span class="hero-eyebrow-dot"></span>
          Społeczność czytelników
        </div>

        <h1 class="hero-title anim anim-d1">
          Twoje książki<br>znajdą <em>nowy dom</em>
        </h1>

        <p class="hero-subtitle anim anim-d2">
          Wymieniaj, wypożyczaj i sprzedawaj książki lokalnie.
          Dołącz do społeczności czytelników.
        </p>

        <div class="hero-ctas anim anim-d3">
          <button class="btn btn-filled btn-lg">Przeglądaj książki</button>
          <button class="btn btn-outline btn-lg">Dołącz za darmo</button>
        </div>

        <div class="hero-stats anim anim-d4">
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

      <div class="hero-right anim anim-d2">
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

<style scoped>
  .hero {
    min-height: calc(100vh - 60px);
    display: flex;
    align-items: center;
    background: linear-gradient(140deg, var(--bg) 35%, #F0E8DF 100%);
    padding: 80px 48px;
    position: relative;
    overflow: hidden;
  }

    .hero::after {
      content: '';
      position: absolute;
      inset: 0;
      background-image: repeating-linear-gradient(0deg, transparent, transparent 2px, rgba(44,24,16,.012) 2px, rgba(44,24,16,.012) 3px);
      pointer-events: none;
    }

  .hero-inner {
    max-width: 1200px;
    margin: 0 auto;
    width: 100%;
    display: grid;
    grid-template-columns: 60fr 40fr;
    gap: 60px;
    align-items: center;
    position: relative;
    z-index: 1;
  }

  .hero-eyebrow {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    background: var(--tag-bg);
    color: var(--primary);
    font-size: 12px;
    font-weight: 600;
    padding: 5px 14px;
    border-radius: 20px;
    margin-bottom: 24px;
    letter-spacing: .4px;
    text-transform: uppercase;
  }

  .hero-eyebrow-dot {
    width: 6px;
    height: 6px;
    background: var(--primary);
    border-radius: 50%;
    animation: pulse 2s ease-in-out infinite;
  }

  @keyframes pulse {
    0%, 100% {
      opacity: 1;
      transform: scale(1);
    }

    50% {
      opacity: .5;
      transform: scale(.7);
    }
  }

  .hero-title {
    font-family: 'Playfair Display', serif;
    font-size: clamp(40px, 4.2vw, 62px);
    font-weight: 700;
    line-height: 1.1;
    color: var(--text-1);
    margin-bottom: 22px;
    text-wrap: pretty;
  }

    .hero-title em {
      color: var(--primary);
      font-style: italic;
    }

  .hero-subtitle {
    font-size: 17px;
    font-weight: 300;
    color: var(--text-2);
    line-height: 1.75;
    margin-bottom: 40px;
    max-width: 460px;
  }

  .hero-ctas {
    display: flex;
    align-items: center;
    gap: 14px;
  }

  .hero-stats {
    display: flex;
    align-items: center;
    gap: 28px;
    margin-top: 52px;
    padding-top: 36px;
    border-top: 1px solid var(--border);
  }

  .hero-stat {
    display: flex;
    flex-direction: column;
    gap: 1px;
  }

  .hero-stat-num {
    font-family: 'Playfair Display', serif;
    font-size: 22px;
    font-weight: 700;
    color: var(--text-1);
    line-height: 1;
  }

  .hero-stat-label {
    font-size: 12px;
    color: var(--text-3);
    font-weight: 400;
  }

  .stat-divider {
    width: 1px;
    height: 30px;
    background: var(--border);
  }

  .hero-right {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 440px;
  }

  .hero-blob {
    position: absolute;
    width: 440px;
    height: 420px;
    background: #F0E8DF;
    border-radius: 58% 42% 68% 32% / 48% 58% 42% 52%;
    animation: blobMorph 14s ease-in-out infinite alternate;
    opacity: .85;
  }

  @keyframes blobMorph {
    0% {
      border-radius: 58% 42% 68% 32% / 48% 58% 42% 52%;
    }

    50% {
      border-radius: 42% 58% 35% 65% / 60% 36% 64% 40%;
    }

    100% {
      border-radius: 64% 36% 52% 48% / 38% 62% 38% 62%;
    }
  }

  .hero-books {
    position: relative;
    z-index: 1;
    display: grid;
    grid-template-columns: repeat(2, 112px);
    grid-template-rows: repeat(3, 150px);
    gap: 16px;
  }

  .hero-book {
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 6px 22px rgba(44,24,16,.18);
    transition: transform .35s ease, box-shadow .35s ease;
  }

    .hero-book img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }

    .hero-book:nth-child(1) {
      transform: rotate(-3deg);
    }

    .hero-book:nth-child(2) {
      transform: rotate(2deg) translateY(12px);
    }

    .hero-book:nth-child(3) {
      transform: rotate(1.5deg) translateY(-5px);
    }

    .hero-book:nth-child(4) {
      transform: rotate(-2deg) translateY(8px);
    }

    .hero-book:nth-child(5) {
      transform: rotate(-1deg) translateY(2px);
    }

    .hero-book:nth-child(6) {
      transform: rotate(3deg) translateY(6px);
    }

    .hero-book:hover {
      transform: rotate(0deg) translateY(-6px) scale(1.06) !important;
      box-shadow: 0 12px 32px rgba(44,24,16,.26);
      z-index: 2;
    }
</style>
