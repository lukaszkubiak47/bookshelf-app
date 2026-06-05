<script setup>
  /**
   * Główny szkielet aplikacji (root layout).
   *
   * Kiedy tu zajrzeć: globalny layout, toast, provide tokenów designu, ukrywanie navbara na /logowanie.
   * Powiązane: main.ts (montuje ten komponent), router/index.ts (RouterView), AppNavbar, AppFooter.
   *
   * Provide:
   * - toast — globalne powiadomienia (useToast)
   * - designTokensKey (T) — paleta kolorów dla ekranu auth (constants.js)
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
  <!-- Layout: navbar (bez /logowanie) → treść routingu → stopka → toast globalny -->
  <div id="app-layout">
    <AppNavbar v-if="$route.path !== '/logowanie'" />
    <main>
      <RouterView />
    </main>
    <AppFooter />

    <!-- Toast sukcesu/błędu — sterowany przez provideToast() / useToast() -->
    <div
      :class="['toast', toast.type === 'success' ? 'success-toast' : 'error-toast', { show: toast.visible }]"
      role="status"
      aria-live="polite"
    >
      <span v-html="toast.icon"></span> {{ toast.message }}
    </div>
  </div>
</template>
