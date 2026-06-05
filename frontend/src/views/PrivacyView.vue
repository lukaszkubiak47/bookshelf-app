<script setup>
  /**
   * Trasa: /polityka-prywatnosci (name: privacy) — strona publiczna, bez API.
   * Cel: polityka prywatności — jakie dane zbiera Bookshelf i komu je udostępnia.
   * Komponenty: brak — cała treść w szablonie tego widoku.
   */
</script>

<template>
  <div class="legal-page">
    <!-- SEKCJA: Karta z treścią polityki prywatności -->
    <article class="legal-card">
      <h1 class="legal-title">Polityka prywatności</h1>
      <p class="legal-lead">
        Dbamy o Twoje dane. Poniżej wyjaśniamy, jakie informacje zbiera Bookshelf, w jakim celu
        i komu są udostępniane.
      </p>

      <section class="legal-section">
        <h2>1. Jakie dane zbieramy</h2>
        <p>
          Przy zakładaniu konta i korzystaniu z serwisu przetwarzamy: adres e-mail, nazwę użytkownika,
          miasto oraz — opcjonalnie — zdjęcie profilowe (avatar). Dodatkowo zapisujemy dane wystawianych
          przez Ciebie ofert oraz historię Twoich próśb i transakcji.
        </p>
      </section>

      <section class="legal-section">
        <h2>2. W jakim celu</h2>
        <p>
          Dane służą do prowadzenia Twojego konta, prezentowania ofert innym czytelnikom oraz umożliwienia
          kontaktu między stronami transakcji. Miasto pomaga znajdować oferty w okolicy.
        </p>
      </section>

      <section class="legal-section">
        <h2>3. Udostępnianie danych</h2>
        <p>
          Twój <strong>adres e-mail udostępniamy wyłącznie drugiej stronie zaakceptowanej transakcji</strong>,
          aby umożliwić umówienie szczegółów. Publicznie widoczne pozostają jedynie nazwa użytkownika,
          miasto, avatar oraz Twoje aktywne oferty. <strong>Nie sprzedajemy</strong> danych i nie przekazujemy
          ich stronom trzecim w celach marketingowych.
        </p>
      </section>

      <section class="legal-section">
        <h2>4. Bezpieczeństwo</h2>
        <p>
          Hasła przechowujemy wyłącznie w postaci zahaszowanej (BCrypt) — nie mamy dostępu do ich treści.
          Dostęp do funkcji wymagających logowania zabezpieczony jest tokenem uwierzytelniającym.
        </p>
      </section>

      <section class="legal-section">
        <h2>5. Twoje prawa</h2>
        <p>
          W każdej chwili możesz przejrzeć i zaktualizować swoje dane w widoku „Mój profil". W razie
          pytań dotyczących przetwarzania danych skontaktuj się z administracją serwisu.
        </p>
      </section>

      <p class="legal-note">
        Dokument ma charakter poglądowy i powstał na potrzeby projektu zaliczeniowego Bookshelf.
      </p>
    </article>
  </div>
</template>

<style scoped>
  .legal-page {
    background: var(--bg);
    min-height: calc(100vh - var(--nav-h));
    padding: 40px 24px;
  }

  .legal-card {
    max-width: 760px;
    margin: 0 auto;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--r-card);
    box-shadow: var(--shadow);
    padding: 40px 44px;
  }

  .legal-title {
    font-family: 'Playfair Display', serif;
    font-size: 32px;
    font-weight: 700;
    color: var(--text-1);
    margin-bottom: 14px;
  }

  .legal-lead {
    color: var(--text-2);
    font-size: 16px;
    line-height: 1.65;
    margin-bottom: 8px;
  }

  .legal-section {
    margin-top: 26px;
  }

  .legal-section h2 {
    font-family: 'Playfair Display', serif;
    font-size: 19px;
    font-weight: 700;
    color: var(--text-1);
    margin-bottom: 8px;
  }

  .legal-section p {
    color: var(--text-2);
    font-size: 15px;
    line-height: 1.7;
  }

  .legal-note {
    margin-top: 32px;
    padding-top: 18px;
    border-top: 1px solid var(--border);
    color: var(--text-3);
    font-size: 13px;
    font-style: italic;
  }
</style>
