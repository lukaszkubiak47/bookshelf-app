/**
 * Konfiguracja Vue Router — mapowanie URL → widoków.
 *
 * Cel: centralna definicja tras, lazy-loading widoków i ochrona tras wymagających logowania.
 * Kiedy używać: dodajesz nową stronę tutaj; w komponentach nawigujesz przez useRouter()
 * lub <RouterLink>, a nie importując widoków bezpośrednio.
 *
 * Powiązane pliki:
 * - main.ts — rejestruje router przez app.use(router)
 * - stores/auth.ts — stan logowania sprawdzany w beforeEach
 * - views/* — komponenty ładowane dynamicznie (import())
 * - App.vue — <RouterView /> renderuje aktywny widok
 * - components/AppNavbar.vue — linki nawigacyjne do tras
 */
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

// --- Definicja tras ---
// meta.requiresAuth — tylko dla zalogowanych; meta.guestOnly — tylko dla gości (np. logowanie)

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // Publiczne
    { path: '/', name: 'home', component: () => import('../views/HomeView.vue') },
    { path: '/przegladanie', name: 'browse', component: () => import('../views/BrowseView.vue') },
    { path: '/logowanie', name: 'auth', component: () => import('../views/AuthView.vue'), meta: { guestOnly: true } },
    // Wymagają zalogowania
    { path: '/polka', name: 'shelf', component: () => import('../views/ShelfView.vue'), meta: { requiresAuth: true } },
    { path: '/dodawanie', name: 'addbook', component: () => import('../views/AddBookView.vue'), meta: { requiresAuth: true } },
    { path: '/polka/edytuj/:id', name: 'editbook', component: () => import('../views/EditBookView.vue'), meta: { requiresAuth: true } },
    { path: '/profil', name: 'profile', component: () => import('../views/ProfileView.vue'), meta: { requiresAuth: true } },
    {
      path: '/regulamin',
      name: 'terms',
      component: () => import('../views/TermsView.vue')
    },
    {
      path: '/polityka-prywatnosci',
      name: 'privacy',
      component: () => import('../views/PrivacyView.vue')
    }
  ]
})

// --- Strażnik nawigacji (navigation guard) ---
// Uruchamia się przed każdą zmianą trasy; zwrócenie { name: '...' } = przekierowanie.

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return { name: 'auth' }
  }
  if (to.meta.guestOnly && auth.isLoggedIn) {
    return { name: 'home' }
  }
})

export default router
