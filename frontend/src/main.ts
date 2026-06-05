/**
 * Punkt wejścia aplikacji Vue (bootstrap).
 *
 * Cel: utworzenie instancji aplikacji, podłączenie pluginów i zamontowanie w DOM.
 * Kiedy używać: ten plik uruchamia się raz przy starcie — zwykle nie edytujesz go
 * przy dodawaniu widoków (to robi router) ani komponentów (to robi App.vue).
 *
 * Powiązane pliki:
 * - App.vue — główny layout i provide (toast, tokeny designu)
 * - router/index.ts — definicja tras i strażnik logowania
 * - directives/clickOutside.ts — dyrektywa v-click-outside
 * - assets/main.css — globalne style (importowane tutaj)
 * - stores/* — Pinia rejestrowana tutaj, używana w widokach i routerze
 */
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { clickOutside } from './directives/clickOutside'
import './assets/main.css'

// --- Tworzenie instancji i rejestracja rozszerzeń ---

const app = createApp(App)

app.directive('click-outside', clickOutside)
app.use(createPinia())
app.use(router)

// --- Montowanie w elemencie #app z index.html ---
app.mount('#app')
