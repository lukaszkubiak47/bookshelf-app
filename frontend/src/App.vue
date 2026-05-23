<script setup>
  import HomeView from './views/HomeView.vue'
</script>

<template>
  <div id="app-layout">
    <nav class="navbar">
      <div class="nav-container">
        <a href="#" class="logo">Bookshelf<span>.</span></a>
        <div class="nav-links">
          <a href="#" class="active">Start</a>
          <a href="#">Katalog</a>
          <a href="#">Społeczność</a>
          <button class="nav-btn">Moje konto</button>
        </div>
      </div>
    </nav>

    <main>
      <HomeView />
    </main>

    <footer class="footer">
      <p>&copy; 2026 Bookshelf. Projekt platformy wymiany książek.</p>
    </footer>
  </div>
</template>

<style scoped>
  .navbar {
    background-color: var(--surface);
    border-bottom: 1px solid var(--border);
    position: sticky;
    top: 0;
    z-index: 100;
  }

  .nav-container {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 2rem;
  }

  .logo {
    font-family: 'Playfair Display', serif;
    font-size: 1.8rem;
    font-weight: 700;
    text-decoration: none;
    color: var(--text-1);
  }

    .logo span {
      color: var(--primary);
    }

  .nav-links {
    display: flex;
    align-items: center;
    gap: 2rem;
  }

    .nav-links a {
      text-decoration: none;
      color: var(--text-2);
      font-weight: 500;
      transition: color 0.2s;
    }

      .nav-links a:hover, .nav-links a.active {
        color: var(--primary);
      }

  .nav-btn {
    background-color: var(--primary);
    color: #fff;
    border: none;
    padding: 0.6rem 1.2rem;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 500;
  }

  .footer {
    text-align: center;
    padding: 2rem;
    background-color: var(--surface);
    border-top: 1px solid var(--border);
    color: var(--text-3);
    font-size: 0.9rem;
  }
</style>
