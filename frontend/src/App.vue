<script setup>
  //import { ref, onMounted } from 'vue'
  import AppNavbar from './components/AppNavbar.vue'
  import AppFooter from './components/AppFooter.vue'
  import { RouterView } from 'vue-router'
  /*
  // 1. Definiujemy reaktywne zmienne dla stanu zalogowania i danych użytkownika
  const isLoggedIn = ref(false)
  const user = ref(null)
  const loading = ref(true)

  // 2. Pobieramy adres URL z pliku .env (zgodnie z poprzednim krokiem)
  const API_URL = import.meta.env.VITE_API_URL

  // 3. Funkcja, która zapyta API o status użytkownika podczas ładowania strony
  const checkAuthStatus = async () => {
    try {
      // Zakładamy, że API ma endpoint np. /api/auth/me lub /api/user
      // Jeśli używasz tokenów (JWT) zapisanych w localStorage, możesz je tu przekazać w nagłówku
      const response = await fetch(`${API_URL}/api/user`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          // 'Authorization': `Bearer ${localStorage.getItem('token')}` // odkomentuj jeśli używasz JWT
        }
      })

      if (response.ok) {
        const data = await response.json()
        user.value = data         // Zapisujemy dane użytkownika (np. data.username, data.avatar)
        isLoggedIn.value = true   // Ustawiamy flagę zalogowania na true
      } else {
        isLoggedIn.value = false
      }
    } catch (error) {
      console.error("Błąd podczas sprawdzania autoryzacji:", error)
      isLoggedIn.value = false
    } finally {
      loading.value = false // Kończymy proces ładowania
    }
  }

  // 4. Wywołujemy funkcję od razu po zamontowaniu komponentu
  onMounted(() => {
    checkAuthStatus()
  })
  */
</script>

<template>
  <div id="app-layout">
    <AppNavbar />
    <!--  1. v-if jest AuthView = brak 2. zalogowany: avatar + @username + wyloguj; niezalogowany: zaloguj się + dołącz -->
    <main>
    <!-- 1. Jesli niezalogowany to nie da sie wejsc na ProfileView -->
    <RouterView />
    </main>
    <!-- 1. v-if jest AuthView = brak 2. do AuthView dodac "© 2025 Bookshelf. Wszystkie prawa zastrzeżone.", analogicznie jak jest w stopce*/ -->
    <AppFooter />
  </div>
</template>

<style scoped>

</style>
