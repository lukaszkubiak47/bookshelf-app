// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import BrowseView from '../views/BrowseView.vue'
import AuthView from '../views/AuthView.vue'
import ShelfView from '../views/ShelfView.vue'
import AddBookView from '../views/AddBookView.vue'
import ProfileView from '../views/ProfileView.vue'
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
      component: AuthView
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
      path: '/profil',
      name: 'profile',
      component: ProfileView,
      meta: { requiresAuth: true }
    }
  ]
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return { name: 'auth', query: { redirect: to.fullPath } }
  }
  if (to.name === 'auth' && auth.isLoggedIn) {
    return { name: 'browse' }
  }
})

export default router
