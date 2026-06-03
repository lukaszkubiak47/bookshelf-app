<script setup>
  import AppNavbar from './components/AppNavbar.vue'
  import AppFooter from './components/AppFooter.vue'
  import { RouterView } from 'vue-router'
</script>

<template>
  <div id="app-layout">
    <AppNavbar v-if="$route.path !== '/logowanie'" />
    <main>
    <RouterView />
    </main>
    <AppFooter />
  </div>
</template>
