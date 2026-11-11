# T3: powiadomienia, AI i logowanie Microsoft

> Dla: Jakub (T3, do 15.11). Ten plik prowadzi przez całe T3: start, zadania, kroki „jak” i kryteria „gotowe, gdy…”.
> Uzasadnienia i pełny plan projektu: `azure/01-analiza-i-zakres.md` (sekcje 4.1, 4.4 i 6.5).
> Pytania i zgody (np. na płatny zasób): Łukasz. Na koniec napisz mu krótko, co zrobiłeś, a czego nie, i dołącz zrzuty ekranu.

**Kolejność pracy:** 0 → 1 → 2 → 3 → 4 → 5 → 6.

- Krok 1 zrób pierwszego dnia: daje najwięcej punktów najmniejszym kosztem (Redis) i sprawdza, czy w ogóle da się utworzyć usługi potrzebne w krokach 2–4.
- Krok 4 ma największe ryzyko (uczelnia może blokować rejestrację aplikacji), dlatego jego sprawdzenie też jest w kroku 1.
- Krok 6 przebudowuje przepływ zdarzeń z kroku 2, więc zaczynaj go, gdy powiadomienia już działają.

## 0. Start

### 0.1 Repo i środowisko lokalne

Zacznij od świeżego `main` (w ostatnich dniach doszło sporo zmian) i własnej gałęzi:

```bash
git checkout main
git pull
git checkout -b t3/redis-powiadomienia
```

- **API i funkcje działają na .NET 10.** Zainstaluj .NET 10 SDK (`dotnet --list-sdks`) i odśwież narzędzie EF: `dotnet tool update --global dotnet-ef`.
- Frontend: Node 22, `npm install` w `frontend/`.
- Pełny stack lokalnie: `docker compose up --build` z roota repo (wymaga `.env` z `MSSQL_SA_PASSWORD` i `GOOGLE_BOOKS_API_KEY`). W stacku jest już Azurite, czyli lokalny zamiennik Azure Storage. Wejście: http://localhost:8088.
- Funkcje lokalnie: projekt `backend/Bookshelf.Functions`, ustawienia w `local.settings.json` (jest w `.gitignore`), uruchomienie przez Azure Functions Core Tools albo Visual Studio.
- Do podglądu Storage (kontenery, kolejki, tabele) przyda się darmowy **Azure Storage Explorer**.

### 0.2 Azure: logowanie i zasoby

- Logujesz się na https://portal.azure.com swoim kontem UG. Masz rolę **Contributor** na subskrypcji Łukasza: możesz tworzyć i zmieniać zasoby.
- Nie widzisz zasobów? Koło zębate → **Directories + subscriptions** → katalog **University of Gdansk (for Students)**. Możesz mieć też własną subskrypcję o tej samej nazwie „Azure for Students”; nasza to ta z grupą `rg-bookshelf`.

**Co już działa w chmurze:**

| Zasób | Nazwa | Uwagi |
|---|---|---|
| Grupa zasobów | `rg-bookshelf` | region **Germany West Central** |
| Serwer SQL i baza | `sql-bookshelf-lk47`, `BookshelfDB` | po dłuższej przerwie baza się budzi; pierwsze zapytanie może skończyć się błędem 500, po minucie działa |
| Plan App Service | `asp-bookshelf` (B1, Linux) | wspólny dla API i frontendu |
| API | `app-bookshelf-api-lk47` | https://app-bookshelf-api-lk47-ahhvhaendgekhgbq.germanywestcentral-01.azurewebsites.net (np. `/api/genres`) |
| Frontend | `app-bookshelf-web-lk47` | https://app-bookshelf-web-lk47-exafebcrfdd2bbfa.germanywestcentral-01.azurewebsites.net |
| Konto Storage | `stbookshelflk47` | kontenery `avatars`, `covers`, `thumbnails`; kolejki `transaction-events`, `image-jobs`; tabela `TransactionHistory` |
| Function App | w `rg-bookshelf` (Flex Consumption) | funkcje `ProcessTransactionEvent` (historia transakcji) i `GenerateThumbnail` (miniatury okładek) |
| Application Insights | `appi-bookshelf` | logi i błędy API oraz funkcji |

- Chrome może pokazać przy frontendzie „Niebezpieczna strona”. To fałszywy alarm Google, zgłoszony do sprawdzenia: **Szczegóły** → otwórz stronę.
- Ustawienia API i funkcji: zasób → **Settings → Environment variables**. `__` zastępuje `:` z `appsettings.json` (np. `ConnectionStrings__Redis` = `ConnectionStrings:Redis`). Po **Apply** aplikacja sama się restartuje, bez wdrożenia.
- **Wdrażanie jest automatyczne:** merge do `main` ze zmianami w `backend/Bookshelf.API/` wdraża API, w `backend/Bookshelf.Functions/` funkcje, a w `frontend/` frontend (GitHub → **Actions**). Migracje EF API stosuje samo przy starcie.
- Logi: App Service / Function App → **Log stream** albo `appi-bookshelf` → **Failures** / **Logs**.

### 0.3 Zasady

- **Region tylko Germany West Central.** Polityka konta studenckiego dopuszcza jeszcze tylko `italynorth`, `austriaeast`, `denmarkeast` i `switzerlandnorth`; inne zostaną zablokowane.
- Wszystko w `rg-bookshelf`, nazwy z przedrostkiem typu: `redis-…`, `cosmos-…`, `di-…`.
- **Płatne zasoby tylko po zgodzie Łukasza.** Zgoda z góry: Azure Managed Redis B0 (ok. 14 USD/mies.), Cosmos DB **tylko z darmowym tierem**, Document Intelligence **F0** (darmowy), plan **S1** na czas testu autoskalowania (ok. 0,1 USD/h; tego samego dnia wracasz na B1) i Service Bus **Standard** (ok. 10 USD/mies.). Na wszystko inne najpierw zapytaj.
- **Sekrety nigdy w repo** (klucze, connection stringi): tylko App Settings, sekrety GitHuba albo lokalne user-secrets / `local.settings.json`.
- Commity po polsku („dodano: …”). PR do `main` scalasz sam, ale każde scalenie od razu wdraża się na Azure, więc `main` musi się budować i działać. Najlepiej osobny PR na każdy krok.
- Zrzuty ekranu do raportu rób na bieżąco (portal, konfiguracja, efekt w aplikacji), bez haseł i kluczy. Nie commituj ich, bo repo jest publiczne; na koniec wyślij je Łukaszowi.

## 1. Na start: klucz Google Books, Redis i sprawdzenie usług

### 1.1 Klucz Google Books

Bez klucza wyszukiwarka Google Books w chmurze nie działa: zapytania bez klucza dzielą jeden wspólny dzienny limit i Google odpowiada 429.

**Jak:** API (`app-bookshelf-api-lk47`) → **Settings → Environment variables** → **+ Add** → `GoogleBooks__ApiKey` = Twój klucz → **Apply** w panelu → **Apply** na dole strony → **Confirm**.

**Test:** „Dodaj książkę” → wyszukaj tytuł → pojawia się lista wyników.

### 1.2 Redis (Azure Managed Redis)

Kod jest gotowy: bez Redisa API korzysta z zamienników w pamięci, a po dodaniu connection stringa samo przełącza się na Redis (cache wyników Google Books, gatunków i statystyk oraz limit prób logowania).

**Portal:** **Azure Managed Redis** → **Create**:

- grupa `rg-bookshelf`, nazwa `redis-bookshelf-lk47`, region **Germany West Central**;
- Performance / SKU: **Balanced B0**, **High availability: wyłączone** (wersja dev/test);
- **Networking:** Public endpoint;
- **Advanced:** **Access keys authentication: Enabled** (aplikacja łączy się kluczem); Clustering policy: **Enterprise** (jeden adres; nie da się tego zmienić później);
- **Review + create** → **Create** (tworzenie trwa kilkanaście minut). 📸

**Po utworzeniu:**

1. Skopiuj endpoint (`redis-bookshelf-lk47.<region>.redis.azure.net`, port **10000**) i klucz z **Authentication → Access keys**.
2. API → **Environment variables** → `ConnectionStrings__Redis` = `<endpoint>:10000,password=<klucz>,ssl=True,abortConnect=False` → **Apply** → **Confirm**.

**Test:**

- wyszukaj tę samą książkę dwa razy: drugie wyszukiwanie odpowiada od razu, z cache;
- 11 razy wpisz złe hasło przy logowaniu: 11. próba daje komunikat „Zbyt wiele prób” (HTTP 429). 📸

### 1.3 Utworzenie i sprawdzenie usług do kroków 2–4

**Cosmos DB (do kroku 2):** **Azure Cosmos DB** → **Create** → **Azure Cosmos DB for NoSQL**:

- grupa `rg-bookshelf`, konto `cosmos-bookshelf-lk47`, lokalizacja **Germany West Central**;
- Capacity mode: **Provisioned throughput**;
- **Apply Free Tier Discount: Apply** oraz zaznaczone **Limit total account throughput** (nie da się przekroczyć darmowej puli). Darmowy tier jest jeden na subskrypcję. Jeśli opcji nie ma albo jest wyszarzona, przerwij i napisz do Łukasza, bo konto byłoby płatne;
- geo-redundancja i zapis w wielu regionach: wyłączone; reszta domyślnie → **Review + create** (sprawdź, że widać darmowy tier) → **Create**. 📸
- Po utworzeniu: **Data Explorer** → **New Container**: baza `bookshelf` ze **wspólną przepustowością 400 RU/s** (Manual), kontener `notifications`, klucz partycji `/userId`. Drugi kontener w tej samej bazie: `notifications-dev` (do pracy lokalnej). Potem w kontenerach **Settings → Time to Live: On**, `2592000` sekund (30 dni).

**Document Intelligence (do kroku 3):** wyszukaj **Document Intelligence** → **Create**:

- grupa `rg-bookshelf`, region **Germany West Central**, nazwa `di-bookshelf-lk47`, Pricing tier **Free F0** (500 stron miesięcznie);
- jeśli F0 nie jest dostępny w tym regionie, wybierz **Italy North** albo **Switzerland North**;
- ⚠️ nie twórz i nie usuwaj zasobu „na próbę”: usunięty zasób AI leży 48 h w koszu i blokuje darmowy slot.
- Po utworzeniu: **Keys and Endpoint** → do API: `DocumentIntelligence__Endpoint` i `DocumentIntelligence__Key`. 📸

**Rejestracja aplikacji w Entra ID (do kroku 4):** **Microsoft Entra ID** → **App registrations** → **New registration**:

- nazwa `bookshelf-web`;
- Supported account types: **Accounts in any organizational directory and personal Microsoft accounts**;
- Redirect URI: platforma **Single-page application (SPA)**, adres frontendu (z tabeli w 0.2) → **Register**;
- potem **Authentication** → dodaj drugi adres SPA: `http://localhost:5173`;
- z **Overview** skopiuj **Application (client) ID**. To nie jest sekret.
- Jeśli uczelnia nie pozwala rejestrować aplikacji (komunikat o braku uprawnień): napisz do Łukasza. Plan B to rejestracja tej samej aplikacji w innym katalogu Microsoft (np. własnym, darmowym). Aplikacja i tak przyjmuje konta z dowolnej organizacji, więc logowanie działa tak samo. 📸

**Gotowe, gdy:** wyszukiwarka Google Books działa, Redis przechodzi oba testy, kontenery Cosmos istnieją, klucze Document Intelligence są w ustawieniach API, a rejestracja aplikacji ma client ID.

## 2. Powiadomienia w Cosmos DB

**Cel:** po każdej akcji na transakcji druga strona widzi powiadomienie: dzwonek z licznikiem nieprzeczytanych w pasku nawigacji.

**Stan dziś (na czym budujesz):**

- API przy każdej zmianie statusu prośby wysyła zdarzenie do kolejki `transaction-events`.
- Funkcja `ProcessTransactionEvent` (`backend/Bookshelf.Functions/ProcessTransactionEvent.cs`) odbiera je i zapisuje historię w Table Storage.
- Format komunikatu (JSON, nazwy pól jak w C#, czyli wielką literą; model `TransactionEvent` jest w API i w projekcie funkcji):

| Pole | Znaczenie |
|---|---|
| `EventId` | unikalny id zdarzenia |
| `TransactionId` | id prośby o transakcję |
| `Type` | `Requested`, `Accepted`, `AutoRejectedOnAccept`, `Rejected`, `Finished` albo `AutoRejectedOnFinish` |
| `ActorUserId` | kto wykonał akcję |
| `RecipientUserId` | **kogo dotyczy: to on dostaje powiadomienie** |
| `UserBookId`, `BookTitle` | oferta i tytuł książki (do treści powiadomienia) |
| `OccurredAt` | kiedy (UTC) |

**Do zrobienia:**

- [ ] `ProcessTransactionEvent` zapisuje też powiadomienie w Cosmos DB (kontener `notifications`, partycja `/userId`);
- [ ] API: `GET /api/notifications` (moje, najnowsze, z liczbą nieprzeczytanych) i `PUT /api/notifications/{id}/read`;
- [ ] frontend: dzwonek z licznikiem w `AppNavbar.vue`, odświeżany co ok. 45 s.

**Jak:**

1. **Funkcja.** Rozbuduj istniejącą `ProcessTransactionEvent`, zamiast dodawać drugą funkcję na tę samą kolejkę: dwie funkcje na jednej kolejce dzielą się komunikatami, więc każde zdarzenie trafiłoby tylko do jednej z nich.
   - Pakiet `Microsoft.Azure.Cosmos` w `Bookshelf.Functions`; w `Program.cs` `CosmosClient` jako singleton z ustawienia `CosmosConnection` (Function App → Environment variables; lokalnie `local.settings.json`), nazwa kontenera z ustawienia, np. `CosmosContainer` (`notifications` w Azure, `notifications-dev` lokalnie).
   - Przy okazji: jeśli w `Bookshelf.Functions.csproj` jest jeszcze `SixLabors.ImageSharp` 3.1.12, podbij go do **3.2.0** (starsza wersja ma znane luki bezpieczeństwa).
   - Po zapisie historii:

     ```csharp
     public record NotificationDocument(
         string id,            // = EventId: ponowione zdarzenie nie zrobi duplikatu
         string userId,        // = RecipientUserId, klucz partycji
         string type,
         string transactionId,
         string? bookTitle,
         DateTime createdAt,
         bool isRead);

     var doc = new NotificationDocument(evt.EventId.ToString(), evt.RecipientUserId.ToString(), evt.Type,
         evt.TransactionId.ToString(), evt.BookTitle, evt.OccurredAt, false);
     await notifications.UpsertItemAsync(doc, new PartitionKey(doc.userId));
     ```

2. **API** (pakiet `Microsoft.Azure.Cosmos`): `CosmosClient` rejestrowany w `Program.cs` tak jak Storage (jest `ConnectionStrings:Cosmos` → Cosmos, nie ma → zamiennik zwracający pustą listę). Nowy `NotificationsController` z `[Authorize]`:
   - `GET /api/notifications`: zapytanie `SELECT TOP 20 * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC` z `PartitionKey` zalogowanego użytkownika, a do tego licznik `SELECT VALUE COUNT(1) FROM c WHERE c.userId = @userId AND c.isRead = false`;
   - `PUT /api/notifications/{id}/read`: `PatchItemAsync<NotificationDocument>(id, new PartitionKey(userId), [PatchOperation.Set("/isRead", true)])`. Partycja to zawsze id zalogowanego, więc nikt nie oznaczy cudzego powiadomienia.
3. **Frontend:**
   - `src/lib/notifications.ts`: `getAll()` i `markRead(id)` na wzór `src/lib/transactions.ts`;
   - `src/components/AppNavbar.vue`: w części dla zalogowanego (`.nav-actions`, przy menu profilu) przycisk z dzwonkiem i licznikiem, po kliknięciu lista powiadomień; kliknięcie wpisu oznacza je jako przeczytane i przenosi na `/polka`;
   - odświeżanie: `setInterval` co ok. 45 s, tylko gdy użytkownik jest zalogowany; wyczyść przy wylogowaniu;
   - polskie opisy typów są już w `BookOffer.vue` (`historyLabels`); przenieś je do wspólnego pliku, np. `src/lib/transactionLabels.ts`, i użyj w obu miejscach.

**Test:** konto A wysyła prośbę o książkę konta B → w ciągu minuty B widzi dzwonek z „1” → kliknięcie oznacza powiadomienie jako przeczytane. W portalu (Cosmos → **Data Explorer** → `notifications`) widać dokumenty. 📸

**Gotowe, gdy:** powiadomienia pojawiają się po akcjach na transakcji.

## 3. Rozpoznanie książki ze zdjęcia (Document Intelligence)

**Cel:** w „Dodaj książkę” użytkownik robi zdjęcie okładki, API odczytuje z niego tekst, szuka go w Google Books i pokazuje wyniki do wyboru.

**Do zrobienia:**

- [ ] `POST /api/books/recognize` (zalogowany, plik JPG/PNG do 4 MB) zwraca listę wyników z Google Books;
- [ ] przycisk „Rozpoznaj ze zdjęcia” w `AddBookView.vue`, zdjęcie zmniejszane w przeglądarce.

**Jak:**

1. Pakiet `Azure.AI.DocumentIntelligence`; ustawienia `DocumentIntelligence:Endpoint` i `DocumentIntelligence:Key` (w Azure z `__`, lokalnie user-secrets).
2. Serwis, np. `BookRecognitionService.RecognizeAsync(IFormFile file)`:

   ```csharp
   var client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));
   var operation = await client.AnalyzeDocumentAsync(
       WaitUntil.Completed, "prebuilt-read", BinaryData.FromStream(file.OpenReadStream()));

   var lines = operation.Value.Pages
       .SelectMany(p => p.Lines)
       .Select(l => l.Content.Trim())
       .Where(t => t.Length > 2)
       .ToList();

   // Tytuł i autor są zwykle w pierwszych liniach okładki
   var query = string.Join(' ', lines.Take(3));
   return await _googleBooks.SearchAsync(query);   // IGoogleBooksService, z cache w Redisie
   ```

   Gdyby nazwa przeciążenia w SDK się różniła, zajrzyj do [quickstartu Document Intelligence dla C#](https://learn.microsoft.com/azure/ai-services/document-intelligence/quickstarts/get-started-sdks-rest-api?pivots=programming-language-csharp).
3. `BooksController`: `[HttpPost("recognize")]` z `[Authorize]` i limitem, np. `[RateLimit("book-recognize", limit: 10, windowSeconds: 3600)]` (atrybut już jest w projekcie). Chroni to darmowe 500 stron miesięcznie.
4. Frontend `src/views/AddBookView.vue`:
   - obok pola wyszukiwania (ok. linii 291) przycisk „Rozpoznaj ze zdjęcia” i ukryty `<input type="file" accept="image/*" capture="environment">`;
   - przed wysłaniem zmniejsz zdjęcie na `<canvas>` (dłuższy bok np. 1600 px, JPEG ~0,85), żeby zmieściło się w 4 MB;
   - `booksApi.recognize(file)` (FormData, pole `file`) i wyniki wpisz do tej samej listy co zwykłe wyszukiwanie (`searchResults`), żeby użytkownik wybierał jak dotąd.

**Test:** zdjęcie prawdziwej okładki → na liście wyników jest właściwa książka. 📸

**Gotowe, gdy:** rozpoznanie książki ze zdjęcia działa w aplikacji.

## 4. Logowanie kontem Microsoft (Entra ID)

**Cel:** przycisk „Zaloguj przez Microsoft” obok obecnego logowania. Pierwsze logowanie zakłada konto w Bookshelf, kolejne logują na to samo konto.

**Zasada bezpieczeństwa:** użytkownika rozpoznajemy po parze `tid` (katalog) + `oid` (użytkownik) z tokenu, **nigdy po e-mailu**. Adres e-mail w koncie z dowolnej organizacji może ustawić administrator obcego katalogu, więc łączenie kont po e-mailu pozwalałoby przejąć cudze konto.

**Stan dziś:**

- `User` (`Models/Entities/User.cs`): `Email` i `Username` są unikalne, `PasswordHash` jest wymagany, do tego `FirstName`, `LastName`, `City`.
- Logowanie: `AuthService` (`RegisterAsync`, `LoginAsync`) wydaje własny JWT przez `ITokenService.GenerateToken(user)` i zwraca `AuthResponseDto` (`Token` + `User`).
- Frontend: sesję zapisuje `setSession(res)` w `src/stores/auth.ts`.

**Do zrobienia:**

- [ ] tabela `ExternalLogins` (`UserId`, `Provider`, `TenantId`, `ObjectId`, `CreatedAt`, unikalny indeks na `Provider + TenantId + ObjectId`) i migracja;
- [ ] `POST /api/auth/microsoft`: sprawdza token od Microsoftu, znajduje albo zakłada konto, zwraca zwykły `AuthResponseDto`;
- [ ] przycisk „Zaloguj przez Microsoft” na stronie logowania.

**Jak:**

1. **Frontend:** `npm install @azure/msal-browser`. Client ID z kroku 1.3 jako zmienna `VITE_MSAL_CLIENT_ID`: lokalnie w `frontend/.env.local`, w GitHubie jako zmienna repo, a w `frontend.yml` dopisana do `env` buildu obok `VITE_API_BASE_URL`.

   ```ts
   const msal = new PublicClientApplication({
     auth: {
       clientId: import.meta.env.VITE_MSAL_CLIENT_ID,
       authority: 'https://login.microsoftonline.com/common',
       redirectUri: window.location.origin,
     },
   })
   await msal.initialize()
   const result = await msal.loginPopup({ scopes: ['openid', 'profile', 'email'] })
   // result.idToken -> POST /api/auth/microsoft { idToken } -> AuthResponse -> setSession(res)
   ```

2. **API: sprawdzenie tokenu** (pakiety `Microsoft.IdentityModel.Protocols.OpenIdConnect`, `Microsoft.IdentityModel.JsonWebTokens`, `Microsoft.IdentityModel.Validators`):

   ```csharp
   // singleton: pobiera i cache'uje klucze podpisu Microsoftu
   var oidc = new ConfigurationManager<OpenIdConnectConfiguration>(
       "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration",
       new OpenIdConnectConfigurationRetriever());

   var config = await oidc.GetConfigurationAsync();
   var result = await new JsonWebTokenHandler().ValidateTokenAsync(dto.IdToken, new TokenValidationParameters
   {
       ValidAudience = clientId,     // client ID z rejestracji aplikacji (ustawienie API, np. Microsoft__ClientId)
       IssuerSigningKeys = config.SigningKeys,
       // wiele katalogów + konta osobiste: walidator wystawcy od Microsoftu
       IssuerValidator = AadIssuerValidator.GetAadIssuerValidator("https://login.microsoftonline.com/common/v2.0").Validate,
       ValidateLifetime = true
   });
   if (!result.IsValid) throw new UnauthorizedException("Nieprawidłowy token Microsoft");

   var tid = result.Claims["tid"].ToString();
   var oid = result.Claims["oid"].ToString();
   ```

3. **Konto:**
   - jest wpis `ExternalLogins` z tym `tid` + `oid` → logujesz tego użytkownika;
   - nie ma → nowy `User`: imię i nazwisko z claimu `name` (albo `given_name` / `family_name`), `Username` z imienia + losowe cyfry (sprawdź unikalność), `City` pusty (użytkownik uzupełni w profilu), `PasswordHash` z losowej wartości (pole jest wymagane), e-mail z claimu `email` / `preferred_username`;
   - jeśli ten e-mail ma już konto z hasłem: **nie łącz kont**, zwróć 409 (`ConflictException`) z komunikatem „Konto z tym e-mailem już istnieje, zaloguj się hasłem”;
   - na koniec `ITokenService.GenerateToken(user)` i `AuthResponseDto`, tak jak w `LoginAsync`. Dodaj ten sam limit prób co przy logowaniu.
4. **Frontend:** przycisk „Zaloguj przez Microsoft” w widoku logowania (`AuthView.vue`), po sukcesie `setSession(res)` i przejście na stronę główną.

**Test:** logowanie kontem UG zakłada nowe konto → wyloguj → zaloguj ponownie: to samo konto. Logowanie prywatnym kontem Microsoft też działa. 📸

**Gotowe, gdy:** logowanie kontem Microsoft działa w aplikacji w chmurze.

## 5. Autoskalowanie aplikacji

**Cel:** przy dużym ruchu App Service sam dokłada instancje, a gdy ruch spada, zdejmuje je. Pokazujemy to testem obciążeniowym i wykresami.

**Do zrobienia:**

- [ ] plan S1 z regułami autoskalowania (1–3 instancje, według obciążenia procesora);
- [ ] test obciążeniowy, który wywoła skalowanie;
- [ ] zrzuty metryk i powrót na B1.

**Jak:**

1. Plan `asp-bookshelf` → **Scale up (App Service plan)** → **Standard S1** → **Select**. W B1 autoskalowania nie ma. Od tej chwili plan kosztuje ok. 0,1 USD/h, więc test i powrót zrób tego samego dnia.
2. **Scale out (App Service plan)** → **Rules Based** → **Custom autoscale**: minimum 1, maksimum 3, domyślnie 1 instancja; reguła **CPU Percentage > 70%** (średnio przez 5 min) → zwiększ liczbę instancji o 1; reguła **CPU Percentage < 30%** → zmniejsz o 1 → **Save**.
3. Na czas testu wyłącz limit prób, żeby test nie kończył się błędami 429: API → Environment variables → `RateLimiting__Enabled` = `false` → **Apply**.
4. Test obciążeniowy, np. [k6](https://k6.io/) z Twojego komputera: kilkadziesiąt wirtualnych użytkowników przez ok. 10 minut na `POST /api/auth/login`. Logowanie liczy BCrypt, więc mocno obciąża procesor. Przykładowy skrypt (konto testowe, nie prawdziwe hasło):

   ```js
   import http from 'k6/http';

   export const options = { vus: 50, duration: '10m' };

   export default function () {
     http.post('https://<adres API>/api/auth/login',
       JSON.stringify({ email: 'test-a@example.com', password: '<hasło konta testowego>' }),
       { headers: { 'Content-Type': 'application/json' } });
   }
   ```

5. Obserwuj plan → **Monitoring → Metrics** (CPU Percentage) i **Scale out → Run history** (liczba instancji). 📸 wykresu CPU i historii skalowania.
6. Powrót: `RateLimiting__Enabled` = `true`, w **Scale out** ręcznie 1 instancja (albo usuń reguły), **Scale up** → **B1**. 📸

**Gotowe, gdy:** zrzuty pokazują, że liczba instancji rośnie przy obciążeniu i spada po nim, a plan wrócił na B1.

## 6. Service Bus: jedno zdarzenie, dwóch odbiorców

**Cel:** zdarzenia transakcji idą do Service Bus (topic) zamiast do kolejki Storage. Historia i powiadomienia mają własne subskrypcje: każda dostaje każde zdarzenie, a awaria jednej nie blokuje drugiej. Kolejka `image-jobs` (miniatury) zostaje bez zmian.

**Do zrobienia:**

- [ ] namespace Service Bus (Standard), topic `transaction-events`, subskrypcje `history` i `notifications`;
- [ ] API wysyła zdarzenia do topicu (nowa implementacja `IEventPublisher`, ten sam format zdarzenia);
- [ ] dwie funkcje z triggerem Service Bus, historia (Table Storage) i powiadomienia (Cosmos DB), zamiast jednej funkcji na kolejce;
- [ ] historia i powiadomienia działają jak przed zmianą.

**Jak:**

1. Portal: **Service Bus** → **Create**: grupa `rg-bookshelf`, nazwa `sb-bookshelf-lk47`, region **Germany West Central**, Pricing tier **Standard** (topiki wymagają co najmniej Standard) → **Create**. 📸
   - **Entities → Topics → + Topic**: `transaction-events`;
   - w topicu **Subscriptions → + Subscription**: `history` i `notifications`;
   - w topicu **Settings → Shared access policies**: `api-send` (tylko **Send**) i `functions-listen` (tylko **Listen**). Connection string z `api-send` trafia do API jako `ConnectionStrings__ServiceBus`, a z `functions-listen` do Function App jako `ServiceBusConnection`. Dzięki temu API może tylko wysyłać, a funkcje tylko czytać.
2. API (pakiet `Azure.Messaging.ServiceBus`):

   ```csharp
   public class ServiceBusEventPublisher(ServiceBusSender sender) : IEventPublisher
   {
       public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default) =>
           sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(evt))
           {
               ContentType = "application/json",
               MessageId = evt.EventId.ToString()
           }, ct);
   }
   ```

   W `Program.cs`: jest `ConnectionStrings:ServiceBus` → `ServiceBusClient`, `CreateSender("transaction-events")` i ta implementacja; nie ma → dotychczasowy wybór (kolejka Storage albo zamiennik logujący).
3. Funkcje (pakiet `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`) zamiast `ProcessTransactionEvent`:

   ```csharp
   [Function("ProcessTransactionHistory")]
   public Task History(
       [ServiceBusTrigger("transaction-events", "history", Connection = "ServiceBusConnection")] string message)
   { /* zapis do TransactionHistory, jak w ProcessTransactionEvent */ }

   [Function("ProcessTransactionNotification")]
   public Task Notify(
       [ServiceBusTrigger("transaction-events", "notifications", Connection = "ServiceBusConnection")] string message)
   { /* zapis do Cosmos DB, kod z kroku 2 */ }
   ```

   Komunikaty Service Bus nie potrzebują Base64.
4. Kolejność przełączenia, żeby żadne zdarzenie nie zginęło:
   1. wdróż nowe funkcje **obok** starej i dodaj w Function App ustawienie `ServiceBusConnection`;
   2. wdróż API z nowym publisherem i dodaj w API `ConnectionStrings__ServiceBus` (po **Apply** API przełączy się na Service Bus);
   3. sprawdź historię i powiadomienia;
   4. kolejnym PR-em usuń starą funkcję `ProcessTransactionEvent`.

**Test:** prośba → akceptacja → zakończenie: historia w karcie transakcji i dzwonek działają jak wcześniej. W portalu (Service Bus → topic → **Subscriptions**) widać komunikaty przetworzone w obu subskrypcjach. 📸

**Gotowe, gdy:** historia i powiadomienia działają przez Service Bus.

## 7. Minimum na koniec T3

- Redis działa w chmurze (cache i limit prób, 429);
- powiadomienia pojawiają się po akcjach na transakcji;
- rozpoznanie książki ze zdjęcia działa;
- logowanie kontem Microsoft działa;
- zrzuty z testu autoskalowania, plan z powrotem na B1;
- historia i powiadomienia działają przez Service Bus;
- zrzuty ekranu z każdego kroku.

Na koniec napisz Łukaszowi, co zrobiłeś, a czego nie (i co ewentualnie przechodzi dalej), i wyślij zrzuty.
