<script setup>
  /**
   * Główny layout: nawigacja (oprócz /logowanie), treść z routera, stopka.
   * Provide: globalny toast + tokeny designu (constants.js).
   */
  import { provide } from 'vue'
  import AppNavbar from './components/AppNavbar.vue'
  import AppFooter from './components/AppFooter.vue'
  import { RouterView } from 'vue-router'
  import { T } from './assets/constants.js'
  import { designTokensKey } from './injectionKeys'
  import { provideToast } from './composables/useToast.js'

  const { toast } = provideToast()
  provide(designTokensKey, T)
</script>

<template>
  <div id="app-layout">
    <AppNavbar v-if="$route.path !== '/logowanie'" />
    <main>
      <RouterView />
    </main>
    <AppFooter />

    <div
      :class="['toast', toast.type === 'success' ? 'success-toast' : 'error-toast', { show: toast.visible }]"
      role="status"
      aria-live="polite"
    >
      <span v-html="toast.icon"></span> {{ toast.message }}
    </div>
  </div>
</template>
