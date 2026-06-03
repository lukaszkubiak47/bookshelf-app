// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import BrowseView from '../views/BrowseView.vue'
import AuthView from '../views/AuthView.vue'
import ShelfView from '../views/ShelfView.vue'
import AddBookView from '../views/AddBookView.vue'
import EditBookView from '../views/EditBookView.vue'
import ProfileView from '../views/ProfileView.vue'
import TermsView from '../views/TermsView.vue'
import PrivacyView from '../views/PrivacyView.vue'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/przegladanie',
      name: 'browse',
      component: BrowseView
    },
    {
      path: '/logowanie',
      name: 'auth',
      component: AuthView,
      meta: { guestOnly: true }
    },
    {
      path: '/polka',
      name: 'shelf',
      component: ShelfView,
      meta: { requiresAuth: true }
    },
    {
      path: '/dodawanie',
      name: 'addbook',
      component: AddBookView,
      meta: { requiresAuth: true }
    },
    {
      path: '/polka/edytuj/:id',
      name: 'editbook',
      component: EditBookView,
      meta: { requiresAuth: true }
    },
    {
      path: '/profil',
      name: 'profile',
      component: ProfileView,
      meta: { requiresAuth: true }
    },
    {
      path: '/regulamin',
      name: 'terms',
      component: TermsView
    },
    {
      path: '/polityka-prywatnosci',
      name: 'privacy',
      component: PrivacyView
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
