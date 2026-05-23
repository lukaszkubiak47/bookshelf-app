<script setup>
  defineProps({
    book: {
      type: Object,
      required: true
    }
  })
</script>

<template>
  <div class="book-card">
    <div class="card-image">
      <img :src="book.image" :alt="book.title" />
      <span class="category-tag">{{ book.category }}</span>
    </div>
    <div class="card-body">
      <h3>{{ book.title }}</h3>
      <p class="author">{{ book.author }}</p>
      <div class="card-footer">
        <span :class="['status', book.status === 'Dostępna' ? 'status-available' : 'status-rented']">
          {{ book.status }}
        </span>
        <button class="btn-sm">Szczegóły</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .book-card {
    background: var(--surface);
    border-radius: 12px;
    overflow: hidden;
    box-shadow: var(--shadow);
    transition: transform 0.3s ease, box-shadow 0.3s ease;
  }

    .book-card:hover {
      transform: translateY(-5px);
      box-shadow: var(--shadow-h);
    }

  .card-image {
    position: relative;
    height: 220px;
    overflow: hidden;
  }

    .card-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

  .category-tag {
    position: absolute;
    top: 1rem;
    left: 1rem;
    background: rgba(255, 255, 255, 0.95);
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.8rem;
    font-weight: 500;
    color: var(--text-1);
  }

  .card-body {
    padding: 1.5rem;
  }

    .card-body h3 {
      margin: 0 0 0.5rem 0;
      font-size: 1.3rem;
      color: var(--text-1);
    }

  .author {
    color: var(--text-2);
    font-size: 0.95rem;
    margin: 0 0 1.5rem 0;
  }

  .card-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .status {
    font-size: 0.85rem;
    font-weight: 600;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
  }

  .status-available {
    background-color: #E8F5E9;
    color: var(--primary);
  }

  .status-rented {
    background-color: #ECEFF1;
    color: var(--text-2);
  }

  .btn-sm {
    background: none;
    border: 1px solid var(--primary);
    color: var(--primary);
    padding: 0.4rem 1rem;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 500;
    transition: all 0.2s;
  }

    .btn-sm:hover {
      background: var(--primary);
      color: #fff;
    }
</style>
