<script setup>
  // Strona statyczna — regulamin korzystania z platformy Bookshelf.
</script>

<template>
  <div class="legal-page">
    <article class="legal-card">
      <h1 class="legal-title">Regulamin</h1>
      <p class="legal-lead">
        Bookshelf to platforma społecznościowa (C2C), która łączy czytelników chcących wymieniać,
        wypożyczać i sprzedawać książki między sobą. Poniższe zasady opisują korzystanie z serwisu.
      </p>

      <section class="legal-section">
        <h2>1. Postanowienia ogólne</h2>
        <p>
          Bookshelf udostępnia narzędzia, dzięki którym użytkownicy mogą prezentować własne książki
          i nawiązywać kontakt w celu ich wymiany, wypożyczenia lub sprzedaży. Serwis pełni rolę
          pośrednika technicznego i nie jest stroną transakcji zawieranych między użytkownikami.
        </p>
      </section>

      <section class="legal-section">
        <h2>2. Konto użytkownika</h2>
        <p>
          Do korzystania z funkcji transakcyjnych wymagane jest konto. Podczas rejestracji podajesz
          adres e-mail, nazwę użytkownika, miasto oraz hasło. Odpowiadasz za poprawność swoich danych
          oraz za zachowanie poufności hasła. Jedno konto powinno należeć do jednej osoby.
        </p>
      </section>

      <section class="legal-section">
        <h2>3. Wystawianie ofert</h2>
        <p>
          Możesz wystawiać wyłącznie książki, do których masz prawo. Przy każdej ofercie wybierasz jej
          typ — sprzedaż, wymianę lub wypożyczenie — oraz warunki (np. cenę lub oczekiwania wymiany).
          Opis powinien rzetelnie odpowiadać rzeczywistemu stanowi książki.
        </p>
      </section>

      <section class="legal-section">
        <h2>4. Transakcje i kontakt</h2>
        <p>
          Zainteresowany użytkownik wysyła prośbę dotyczącą oferty, a jej właściciel może ją zaakceptować
          lub odrzucić. Dane kontaktowe (adres e-mail) obu stron udostępniane są dopiero po
          <strong>zaakceptowaniu</strong> prośby i wyłącznie stronom danej transakcji. Szczegóły przekazania
          książki, ewentualnej płatności oraz terminu zwrotu przy wypożyczeniu ustalają między sobą sami
          użytkownicy. Bookshelf nie pośredniczy w płatnościach ani dostawie i nie odpowiada za przebieg
          transakcji.
        </p>
      </section>

      <section class="legal-section">
        <h2>5. Odpowiedzialność użytkowników</h2>
        <p>
          Użytkownicy ponoszą odpowiedzialność za zawierane transakcje, w tym za stan przekazywanych
          książek, terminowy zwrot wypożyczeń oraz rozliczenia. Prosimy o uczciwość, kulturę wymiany
          informacji i poszanowanie ustaleń poczynionych z drugą stroną.
        </p>
      </section>

      <section class="legal-section">
        <h2>6. Treści zabronione</h2>
        <p>
          Zabronione jest publikowanie treści niezgodnych z prawem, wprowadzających w błąd, obraźliwych
          lub stanowiących spam. Konta naruszające zasady mogą zostać ograniczone.
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
