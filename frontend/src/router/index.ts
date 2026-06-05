/**
 * Definicja tras URL → widoków oraz strażnik logowania (meta.requiresAuth / guestOnly).
 */
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
  // --- Publiczne ---
    { path: '/', name: 'home', component: () => import('../views/HomeView.vue') },
    { path: '/przegladanie', name: 'browse', component: () => import('../views/BrowseView.vue') },
    { path: '/logowanie', name: 'auth', component: () => import('../views/AuthView.vue'), meta: { guestOnly: true } },
    // --- Wymagają zalogowania ---
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

// Ochrona tras: trasy z requiresAuth wymagają logowania; guestOnly (logowanie) przekierowuje
// zalogowanych na stronę główną.
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
