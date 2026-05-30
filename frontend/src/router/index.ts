// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import BrowseView from '../views/BrowseView.vue'
import AuthView from '../views/AuthView.vue'
import ShelfView from '../views/ShelfView.vue'
import AddBookView from '../views/AddBookView.vue'
import ProfileView from '../views/ProfileView.vue'

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
      component: ShelfView
    },
    {
      path: '/dodawanie',
      name: 'addbook',
      component: AddBookView
    },
    {
      path: '/profil',
      name: 'profile',
      component: ProfileView
    }
  ]
})

export default router
