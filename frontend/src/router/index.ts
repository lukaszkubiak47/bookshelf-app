// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import BrowseView from '../views/BrowseView.vue'
import AuthView from '../views/AuthView.vue'
import ShelfView from '../views/ShelfView.vue'

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
    }
  ]
})

export default router
