<template>
  <div class="left-panel" :style="{
    flex: '0 0 50%',
    position: 'sticky', top: 0, height: '100vh',
    alignSelf: 'flex-start',
    background: T.green,
    display: 'flex', flexDirection: 'column',
    alignItems: 'center', justifyContent: 'space-between',
    padding: '56px 48px 48px',
    overflow: 'hidden',
  }">
    <svg aria-hidden="true" :style="{ position: 'absolute', inset: 0, width: '100%', height: '100%', opacity: 0.07, pointerEvents: 'none' }">
      <defs>
        <pattern id="bsgrid" width="48" height="48" patternUnits="userSpaceOnUse">
          <path d="M 48 0 L 0 0 0 48" fill="none" stroke="white" stroke-width="1" />
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="url(#bsgrid)" />
    </svg>

    <div aria-hidden="true" :style="{
      position: 'absolute',
      top: '28%', left: '50%',
      transform: 'translate(-50%, -50%)',
      width: '420px', height: '420px',
      background: 'radial-gradient(circle, rgba(212,168,67,0.18) 0%, transparent 68%)',
      pointerEvents: 'none',
    }" />

    <div :style="{ textAlign: 'center', position: 'relative', zIndex: 2 }">
      <div :style="{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '13px', marginBottom: '14px' }">
        <div :style="{
          width: '50px', height: '50px', borderRadius: '13px',
          background: 'rgba(255,255,255,0.14)',
          border: '1px solid rgba(255,255,255,0.2)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P.book" />
        </div>
        <span :style="{ fontFamily: '\'Playfair Display\', serif', fontSize: '34px', fontWeight: 700, color: 'white', letterSpacing: '-0.5px' }">
          Bookshelf
        </span>
      </div>
      <p :style="{ color: 'rgba(255,255,255,0.6)', fontSize: '15px', fontWeight: 300, letterSpacing: '0.8px' }">
        Świat książek w Twoich rękach
      </p>
    </div>

    <div :style="{ position: 'relative', width: '200px', height: '290px', zIndex: 2 }">
      <div v-for="(book, i) in books" :key="book.id" :style="{
        position: 'absolute',
        width: '150px', height: '200px',
        top: '50%', left: '50%',
        marginTop: '-100px', marginLeft: '-75px',
        transform: spreads[i].transform,
        zIndex: spreads[i].z,
      }">
        <div :class="spreads[i].float" :style="{
          width: '100%', height: '100%',
          borderRadius: '10px',
          overflow: 'hidden',
          boxShadow: '0 18px 48px rgba(0,0,0,0.45)',
          position: 'relative'
        }">
          <img :src="`https://picsum.photos/150/200?random=${book.id}`" :alt="book.title" :style="{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }" />

          <div :style="{
            position: 'absolute', top: '8px', left: '8px',
            background: book.badgeColor,
            color: book.badgeTxt,
            fontSize: '9px', fontWeight: 600, letterSpacing: '0.4px',
            padding: '3px 7px', borderRadius: '4px',
          }">
            {{ book.badge }}
          </div>

          <div :style="{
            position: 'absolute', bottom: 0, left: 0, right: 0,
            background: 'linear-gradient(to top, rgba(0,0,0,0.8) 0%, transparent 100%)',
            padding: '22px 10px 10px',
          }">
            <p :style="{ fontFamily: '\'Playfair Display\', serif', color: 'white', fontSize: '11px', fontWeight: 600, lineHeight: 1.3, marginBottom: '2px' }">
              {{ book.title }}
            </p>
            <p :style="{ color: 'rgba(255,255,255,0.65)', fontSize: '10px' }">{{ book.author }}</p>
          </div>
        </div>
      </div>
    </div>

    <div :style="{ display: 'flex', gap: '28px', position: 'relative', zIndex: 2 }">
      <div v-for="(s, i) in stats" :key="i" :style="{ textAlign: 'center' }">
        <div :style="{
          width: '40px', height: '40px', borderRadius: '10px',
          background: 'rgba(255,255,255,0.1)',
          border: '1px solid rgba(255,255,255,0.15)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          margin: '0 auto 8px',
        }">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.7)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" v-html="P[s.icon]" />
        </div>
        <p :style="{ fontFamily: '\'Playfair Display\', serif', color: 'white', fontSize: '20px', fontWeight: 700, lineHeight: 1 }">
          {{ s.value }}
        </p>
        <p :style="{ color: 'rgba(255,255,255,0.55)', fontSize: '11px', marginTop: '3px' }">{{ s.label }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>import { T, P } from '../assets/constants';

const books = [
  { id: 17, title: 'Mistrz i Małgorzata', author: 'M. Bułhakow', badge: 'Do wymiany', badgeColor: T.gold, badgeTxt: T.t1 },
  { id: 43, title: 'Dżuma', author: 'A. Camus', badge: 'Do wypożyczenia', badgeColor: T.green, badgeTxt: 'white' },
  { id: 28, title: 'Sto lat samotności', author: 'G.G. Márquez', badge: 'Do sprzedaży', badgeColor: '#2C5F8A', badgeTxt: 'white' },
];

const spreads = [
  { transform: 'rotate(-9deg) translate(-22px, 10px)', z: 1, float: 'float-a' },
  { transform: 'rotate(1deg)  translate(4px, -4px)',   z: 3, float: 'float-c' },
  { transform: 'rotate(9deg)  translate(22px, 6px)',   z: 2, float: 'float-b' },
];

const stats = [
  { icon: 'book', value: '2 847', label: 'książek' },
  { icon: 'users', value: '1 203', label: 'użytkowników' },
  { icon: 'repeat', value: '4 521', label: 'transakcji' },
];</script>
