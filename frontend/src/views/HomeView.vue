<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import HeroSection from '@/components/HeroSection.vue'
  import BookCard from '@/components/BookCard.vue'
  import HowItWorks from '@/components/HowItWorks.vue'

  const books = ref([
    {
      id: 1,
      title: 'Sto lat samotności',
      author: 'Gabriel García Márquez',
      coverImage: 'https://picsum.photos/180/240?random=1',
      badgeText: 'Do wymiany',
      badgeClass: 'badge-swap',
      userAvatar: 'https://i.pravatar.cc/40?img=1',
      userNick: '@maria_kow',
      userCity: 'Warszawa',
      timeAgo: '2 godz. temu',
      delayClass: 'anim-d1'
    },
    {
      id: 2,
      title: 'Diuna',
      author: 'Frank Herbert',
      coverImage: 'https://picsum.photos/180/240?random=2',
      badgeText: 'DO WYPOŻYCZENIA',
      badgeClass: 'badge-loan',
      badgeStyle: 'font-size: 9px',
      userAvatar: 'https://i.pravatar.cc/40?img=5',
      userNick: '@piotr_n',
      userCity: 'Kraków',
      timeAgo: '3 godz. temu',
      delayClass: 'anim-d2'
    },
    {
      id: 3,
      title: 'Wiedźmin: Ostatnie życzenie',
      author: 'Andrzej Sapkowski',
      coverImage: 'https://picsum.photos/180/240?random=3',
      badgeText: 'Do sprzedaży',
      badgeClass: 'badge-sale',
      userAvatar: 'https://i.pravatar.cc/40?img=9',
      userNick: '@agata_ww',
      userCity: 'Gdańsk',
      timeAgo: '5 godz. temu',
      delayClass: 'anim-d3'
    },
    {
      id: 4,
      title: 'Mistrz i Małgorzata',
      author: 'Michaił Bułhakow',
      coverImage: 'https://picsum.photos/180/240?random=4',
      badgeText: 'Do wymiany',
      badgeClass: 'badge-swap',
      userAvatar: 'https://i.pravatar.cc/40?img=3',
      userNick: '@tomek_r',
      userCity: 'Wrocław',
      timeAgo: '8 godz. temu',
      delayClass: 'anim-d4'
    },
    {
      id: 5,
      title: 'Zbrodnia i kara',
      author: 'Fiodor Dostojewski',
      coverImage: 'https://picsum.photos/180/240?random=5',
      badgeText: 'Do wypożyczenia',
      badgeClass: 'badge-loan',
      badgeStyle: 'font-size: 9px',
      userAvatar: 'https://i.pravatar.cc/40?img=7',
      userNick: '@kasia_pzn',
      userCity: 'Poznań',
      timeAgo: 'wczoraj',
      delayClass: 'anim-d5'
    }
  ])

  onMounted(() => {
    // Animacja dla sekcji Hero odpala od razu po załadowaniu
    const heroEls = document.querySelectorAll('.hero .anim')
    heroEls.forEach(el => {
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          el.classList.add('is-visible')
        })
      })
    })

    // Reszta elementów animuje się przy przewijaniu strony (Intersection Observer)
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible')
          observer.unobserve(entry.target)
        }
      })
    }, { threshold: 0.1, rootMargin: '0px 0px -32px 0px' })

    document.querySelectorAll('.anim:not(.hero .anim)').forEach(el => observer.observe(el))

    // Feedback kliknięć (Ripple) z Twojego skryptu
    document.querySelectorAll('.btn').forEach(btn => {
      btn.addEventListener('click', function (e: any) {
        const r = document.createElement('span')
        const rect = this.getBoundingClientRect()
        const size = Math.max(rect.width, rect.height)
        Object.assign(r.style, {
          position: 'absolute', borderRadius: '50%',
          width: size + 'px', height: size + 'px',
          left: (e.clientX - rect.left - size / 2) + 'px',
          top: (e.clientY - rect.top - size / 2) + 'px',
          background: 'rgba(255,255,255,.25)',
          transform: 'scale(0)',
          animation: 'ripple .5s linear',
          pointerEvents: 'none',
        })
        this.style.position = 'relative'
        this.style.overflow = 'hidden'
        this.appendChild(r)
        setTimeout(() => r.remove(), 500)
      })
    })
  })
</script>

<template>
  <div>
    <HeroSection />

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
          <BookCard v-for="book in books"
                    :key="book.id"
                    :book="book" />
        </div>
      </div>
    </section>

    <HowItWorks />
  </div>
</template>

<style scoped>
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

  .books-grid {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 24px;
  }
</style>
