/**
 * Punkt wejścia aplikacji Vue.
 * Montuje App, rejestruje Pinia (stan globalny) i Vue Router (trasy).
 */
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { clickOutside } from './directives/clickOutside'
import './assets/main.css'

const app = createApp(App)

app.directive('click-outside', clickOutside)
app.use(createPinia())
app.use(router)

app.mount('#app')
