# T2: obrazy w chmurze, zdarzenia i historia

> Dla: Wojtek (T2, 02.11–08.11). Ten plik prowadzi przez całe T2: start, zadania, kroki „jak” i kryteria „gotowe, gdy…”.
> Uzasadnienia i pełny plan projektu: `azure/01-analiza-i-zakres.md` (sekcje 4.1, 4.4 i 6.4).
> Pytania i zgody (np. na płatny zasób): Łukasz. Na koniec napisz mu krótko, co zrobiłeś, a czego nie.

**Kolejność pracy:** 0 → 1 → 2 → 3 → 4, a 5 i 6 tylko jeśli zostanie czas. Kroki 1 i 3 są fundamentem dla T3 (powiadomienia Jakuba korzystają z Twoich zdarzeń), więc warto je skończyć w pierwszej połowie tygodnia.

## 0. Start

### 0.1 Repo i środowisko lokalne

Zacznij od świeżego `main` (sporo się zmieniło) i własnej gałęzi:

```bash
git checkout main
git pull
git checkout -b t2/obrazy-zdarzenia
```

- **API działa na .NET 10.** Zainstaluj .NET 10 SDK (`dotnet --list-sdks`) i odśwież narzędzie EF: `dotnet tool update --global dotnet-ef`.
- Frontend: Node 22, `npm install` w `frontend/`.
- Pełny stack lokalnie: `docker compose up --build` z roota repo (wymaga `.env` z `MSSQL_SA_PASSWORD` i `GOOGLE_BOOKS_API_KEY`). Wejście: http://localhost:8088.
- Do podglądu Storage (kontenery, kolejki, tabele, także w Azurite) przyda się darmowy **Azure Storage Explorer**.

### 0.2 Azure: logowanie i zasoby

- Logujesz się na https://portal.azure.com swoim kontem UG. Masz rolę **Contributor** na subskrypcji Łukasza: możesz tworzyć i zmieniać zasoby.
- Nie widzisz zasobów? Koło zębate → **Directories + subscriptions** → katalog **University of Gdansk (for Students)**. Możesz mieć też własną subskrypcję o tej samej nazwie „Azure for Students”; nasza to ta z grupą `rg-bookshelf`.
- Dalej nie widzisz? Rola może wymagać aktywacji: wyszukaj **Privileged Identity Management** → **My roles** → **Azure resources** → **Contributor** → **Activate**.

**Co już działa w chmurze:**

| Zasób | Nazwa | Uwagi |
|---|---|---|
| Grupa zasobów | `rg-bookshelf` | region **Germany West Central** |
| Serwer SQL i baza | `sql-bookshelf-lk47`, `BookshelfDB` | darmowa oferta: po godzinie bez ruchu baza się usypia, pierwsze zapytanie budzi ją do minuty |
| Plan App Service | `asp-bookshelf` (B1, Linux) | płatny, wspólny dla API i frontendu |
| API | `app-bookshelf-api-lk47` | https://app-bookshelf-api-lk47-ahhvhaendgekhgbq.germanywestcentral-01.azurewebsites.net (np. `/api/genres`) |
| Frontend | `app-bookshelf-web-lk47` | https://app-bookshelf-web-lk47-exafebcrfdd2bbfa.germanywestcentral-01.azurewebsites.net |
| Application Insights | `appi-bookshelf` | logi, błędy i czasy odpowiedzi API |

- Chrome może pokazać przy frontendzie „Niebezpieczna strona”. To fałszywy alarm Google, zgłoszony do sprawdzenia: **Szczegóły** → otwórz stronę.
- Ustawienia API: App Service → **Settings → Environment variables**. Nowe ustawienia dodajesz tam; `__` zastępuje `:` z `appsettings.json` (np. `ConnectionStrings__Storage` = `ConnectionStrings:Storage`). Po **Apply** aplikacja sama się restartuje, bez wdrożenia.
- **Wdrażanie jest automatyczne:** merge do `main` ze zmianami w `backend/` uruchamia workflow „API -> Azure App Service”, a w `frontend/` „Frontend -> Azure App Service” (GitHub → **Actions**). Migracje EF API stosuje samo przy starcie (`RunMigrations=true`).
- Logi działającego API: App Service → **Log stream** albo `appi-bookshelf` → **Failures** / **Logs**.

### 0.3 Zasady

- **Region tylko Germany West Central.** Polityka konta studenckiego dopuszcza 5 regionów, inne zostaną zablokowane.
- Wszystko w `rg-bookshelf`, nazwy z przedrostkiem typu: `st…` (Storage: małe litery i cyfry, bez myślników), `func-…` (Function App).
- **Płatne zasoby tylko po zgodzie Łukasza.** Zgoda z góry: konto Storage (grosze) i Function App w planie Flex Consumption (mieści się w darmowej puli). VM z dodatku 3: najpierw zapytaj.
- **Sekrety nigdy w repo** (klucze, connection stringi, publish profile): tylko App Settings, sekrety GitHuba albo lokalne `.env` / `local.settings.json`.
- Commity po polsku („dodano: …”). PR do `main` scalasz sam, ale każde scalenie od razu wdraża się na Azure, więc `main` musi się budować i działać. Lepiej kilka mniejszych PR-ów (np. po każdym kroku) niż jeden ogromny.
- Zrzuty ekranu do raportu rób na bieżąco (portal, konfiguracja, efekt w aplikacji), bez haseł i kluczy. Nie commituj ich, bo repo jest publiczne; na koniec przekaż je Łukaszowi.

## 1. Fundament: Storage, kolejka zdarzeń i Function App

### 1.1 Konto Storage

**Jak:** portal → **Storage accounts** → **Create**:

- **Basics:** grupa `rg-bookshelf`, nazwa np. `stbookshelflk47`, region **Germany West Central**, Performance **Standard**, Redundancy **LRS**.
- **Advanced:** zostaw wyłączony anonimowy dostęp do kontenerów (obrazy wydajemy linkami SAS) i minimalny TLS 1.2.
- Reszta domyślnie → **Review + create** → **Create**. 📸

W koncie utwórz (portal albo z kodu przez `CreateIfNotExistsAsync` przy starcie): kontenery `avatars` i `covers` (później `thumbnails`), kolejkę `transaction-events` (później `image-jobs`), tabelę `TransactionHistory`.

Connection string: **Security + networking → Access keys** → **Show** → skopiuj → w API App Setting `ConnectionStrings__Storage`.

### 1.2 Lokalny zamiennik: Azurite

Każda integracja ma lokalny zamiennik, żeby dało się pracować bez Azure.

- Bez Dockera: Azurite z Visual Studio albo `npx azurite`, a w `appsettings.Development.json` lub user-secrets `ConnectionStrings:Storage = UseDevelopmentStorage=true`.
- W `docker-compose.yml` dodaj serwis:

  ```yaml
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    command: azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --skipApiVersionCheck
    ports: ["10000:10000", "10001:10001", "10002:10002"]
    networks: [bookshelf-net]
  ```

  a w serwisie `api` ustaw `ConnectionStrings__Storage` na pełny connection string Azurite z adresami `http://azurite:10000/devstoreaccount1` (blob), `:10001` (queue), `:10002` (table). Wzór i publiczny klucz emulatora: [dokumentacja Azurite, „HTTP connection strings”](https://learn.microsoft.com/azure/storage/common/storage-use-azurite#http-connection-strings).
- Pułapka: link SAS wygenerowany w kontenerze wskazuje na `azurite:10000`, a tego adresu przeglądarka nie zna. Najprościej dodać ustawienie z publicznym adresem bloba (np. `Storage__PublicBlobUrl = http://localhost:10000/devstoreaccount1`) i podmieniać początek linku przed przekierowaniem.

### 1.3 `IEventPublisher` w API

**Po co:** API ma tylko wrzucić zdarzenie do kolejki i od razu odpowiedzieć; resztę (historia, a w T3 powiadomienia) zrobi funkcja w tle.

**Jak:**

1. Pakiet: `dotnet add backend/Bookshelf.API package Azure.Storage.Queues`.
2. Model zdarzenia, np. `Models/Events/TransactionEvent.cs` (pola w kroku 3).
3. Interfejs i dwie implementacje, np. w `Services/Events/`:

   ```csharp
   public interface IEventPublisher
   {
       Task PublishAsync(TransactionEvent evt, CancellationToken ct = default);
   }

   // Azure albo Azurite: komunikat JSON do kolejki transaction-events.
   public class QueueEventPublisher(QueueClient queue) : IEventPublisher
   {
       public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default) =>
           queue.SendMessageAsync(JsonSerializer.Serialize(evt), ct);
   }

   // Zamiennik bez Storage: tylko log, żeby API działało bez Azure.
   public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
   {
       public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default)
       {
           logger.LogInformation("Zdarzenie {Type} transakcji {Id}", evt.Type, evt.TransactionId);
           return Task.CompletedTask;
       }
   }
   ```

4. Rejestracja w `Program.cs` tak jak Redis (jest connection string → Azure, nie ma → zamiennik):

   ```csharp
   var storageConnection = builder.Configuration.GetConnectionString("Storage");
   if (!string.IsNullOrWhiteSpace(storageConnection))
   {
       var queue = new QueueClient(storageConnection, "transaction-events",
           new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
       queue.CreateIfNotExists();
       builder.Services.AddSingleton(queue);
       builder.Services.AddSingleton<IEventPublisher, QueueEventPublisher>();
   }
   else
   {
       builder.Services.AddSingleton<IEventPublisher, LoggingEventPublisher>();
   }
   ```

⚠️ `MessageEncoding = Base64` jest konieczne: trigger kolejki w Functions oczekuje Base64, a bez tej opcji komunikaty lądują w kolejce `transaction-events-poison`.

### 1.4 Function App, projekt i workflow

**Portal:** **Function App** → **Create** → **Flex Consumption**:

- grupa `rg-bookshelf`, nazwa `func-bookshelf-lk47`, region **Germany West Central**, runtime **.NET**, wersja **10 (isolated worker)**;
- Storage: istniejące `stbookshelflk47`; Application Insights: `appi-bookshelf`; wdrożenie z GitHuba w kreatorze: wyłączone (mamy własny workflow).
- Po utworzeniu: **Configuration → General settings → SCM Basic Auth Publishing Credentials: On** (bez tego publish profile nie zadziała), FTP wyłącz. Potem **Overview → Download publish profile**.
- Jeśli Germany West Central nie ma na liście regionów albo kreator zgłasza błąd polityki: plan B to Function App na istniejącym planie `asp-bookshelf` (opcja hostingu **App Service plan**). Nie kosztuje nic więcej i ma Always On. Daj wtedy znać Łukaszowi.

**Projekt:** `backend/Bookshelf.Functions` (.NET 10 isolated), np. `func init Bookshelf.Functions --worker-runtime dotnet-isolated --target-framework net10.0` (Azure Functions Core Tools) albo szablon „Azure Functions” w Visual Studio.

```csharp
public class ProcessTransactionEvent(ILogger<ProcessTransactionEvent> logger)
{
    [Function(nameof(ProcessTransactionEvent))]
    public void Run([QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
    {
        logger.LogInformation("Zdarzenie z kolejki: {Message}", message);
    }
}
```

- `StorageConnection` to ustawienie z connection stringiem konta `stbookshelflk47`: w Function App **Settings → Environment variables**, lokalnie w `local.settings.json` (`UseDevelopmentStorage=true`). Nie polegamy na `AzureWebJobsStorage`, bo Flex Consumption może je ustawić inaczej (np. przez tożsamość zarządzaną).
- `local.settings.json` dopisz do `.gitignore`.

**Workflow** `.github/workflows/functions.yml` na wzór `api.yml` (push na `main` ze zmianami w `backend/Bookshelf.Functions/**` + ręczne uruchamianie):

```yaml
      - uses: actions/checkout@v5
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: '10.0.x'
      - name: Publish
        run: dotnet publish backend/Bookshelf.Functions -c Release -o "${{ runner.temp }}/func"
      - name: Deploy do Function App
        uses: Azure/functions-action@v1
        with:
          app-name: ${{ vars.AZURE_FUNCTIONAPP_NAME }}
          package: ${{ runner.temp }}/func
          publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
          sku: flexconsumption   # tylko dla Flex Consumption; przy planie B usuń tę linię
```

Sekret `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` (cała treść pliku `.PublishSettings`) i zmienną `AZURE_FUNCTIONAPP_NAME` dodaje w GitHubie (**Settings → Secrets and variables → Actions**) Łukasz albo Ty, jeśli masz uprawnienia.

**Test:** portal → konto Storage → **Queues** → `transaction-events` → **Add message** (dowolny tekst, zaznacz *Encode the message body in Base64*) → w Function App **Log stream** albo w Application Insights widać wpis z treścią.

**Gotowe, gdy:** funkcja wdraża się z `main`, a testowy komunikat pojawia się w jej logach. 📸

## 2. Obrazy w Blob Storage

**Cel:** okładki i avatary leżą w Blob Storage, w bazie zostaje tylko nazwa pliku, a obrazy wydajemy krótkimi linkami SAS. Przy okazji naprawiamy błąd: dziś nikt poza właścicielem nie widzi wgranego avatara.

**Stan dziś (gdzie co jest):**

| Co | Gdzie |
|---|---|
| Kolumny z obrazami (`varbinary(max)`) | `User.AvatarImageData` (`Models/Entities/User.cs:37`), `UserBook.CustomCoverImageData` (`UserBook.cs:37`); kolumny na typ pliku nie ma |
| Avatar | `UserService`: `UploadAvatarAsync` (`:82-102`, w `:99` ustawia `AvatarUrl = "/api/users/me/avatar/image"`), `GetAvatarImageAsync` (`:120-131`), preset: `SetAvatarPresetAsync` (`:106-118`) |
| Okładka | `UserBookService`: `UploadCoverAsync` (`:257-276`), `GetCoverImageAsync` (`:278-288`), `DeleteCoverAsync` (`:290-295`), adres okładki `ResolveCoverUrl` (`:350-356`), `HasCustomCover` (`:382`) |
| Endpointy | `UsersController` (cały wymaga logowania): `POST me/avatar`, `PUT me/avatar-preset`, `GET me/avatar/image`. `UserBooksController`: `POST {id}/cover`, `GET {id}/cover/image` (anonimowy), `DELETE {id}/cover` |
| Frontend | avatary biorą adres z pola `avatarUrl` przez `avatarImageUrl` / `realAvatarUrl` w `src/lib/mappers.ts`; `EditBookView.vue:88-91` ma na sztywno `/api/userbooks/{id}/cover/image` |

Dlaczego cudze avatary nie działają: wgrany avatar ma adres `/api/users/me/avatar/image`, a „me” to zawsze oglądający. Ten sam adres trafia do cudzych kart (właściciel oferty, strony transakcji), a `<img>` nie wysyła tokenu.

**Do zrobienia:**

- [ ] serwis do obrazów na Blob Storage: upload, usuwanie, link SAS;
- [ ] migracja: kolumny `varbinary` zastąpione nazwami plików;
- [ ] upload avatara i okładki zapisuje plik w blobie, a podmiana usuwa stary plik;
- [ ] anonimowe `GET /api/users/{id}/avatar` i dotychczasowe `GET /api/userbooks/{id}/cover/image` zwracają przekierowanie 302 na link SAS;
- [ ] `AvatarUrl` wgranego avatara = `/api/users/{id}/avatar`;
- [ ] usunięcie okładki usuwa plik;
- [ ] reguła cyklu życia w koncie Storage.

**Jak:**

1. Pakiet `Azure.Storage.Blobs`. Serwis np. `Services/Storage/IImageStorage.cs`:

   ```csharp
   public interface IImageStorage
   {
       Task<string> UploadAsync(string container, string prefix, IFormFile file);   // zwraca nazwę pliku
       Task DeleteAsync(string container, string? blobName);
       Uri GetReadUri(string container, string blobName);                           // link SAS
   }
   ```

   Implementacja na `BlobServiceClient`, zarejestrowana w `Program.cs` z `ConnectionStrings:Storage` (kontenery `CreateIfNotExists` przy starcie):
   - nazwa pliku `{prefix}/{Guid.NewGuid()}{rozszerzenie}`, np. w `covers` prefiksem jest id oferty;
   - typ pliku zapisz w nagłówku bloba (`BlobHttpHeaders.ContentType = file.ContentType`), więc w bazie nie trzeba kolumny na typ;
   - link: `blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15))`. Działa, bo connection string zawiera klucz konta;
   - lokalnie: `"ConnectionStrings": { "Storage": "UseDevelopmentStorage=true" }` w `appsettings.Development.json` (to nie sekret, to adres Azurite).
2. Encje: zamiast `AvatarImageData` pole `string? AvatarBlobName`, zamiast `CustomCoverImageData` pole `string? CustomCoverBlobName` (oba `[MaxLength(200)]`). Potem:

   ```bash
   dotnet ef migrations add ObrazyWBlobStorage --project backend/Bookshelf.API
   ```

   - Sprawdź, że migracja usuwa dwie kolumny `varbinary` i dodaje dwie tekstowe.
   - Dopisz w niej `migrationBuilder.Sql("UPDATE Users SET AvatarUrl = NULL WHERE AvatarUrl = '/api/users/me/avatar/image'");`, żeby istniejące konta nie wskazywały na usunięty avatar.
   - Obrazy zapisane dotąd w bazach (lokalnej i w chmurze) przepadną. To dane testowe, wystarczy wgrać je ponownie. W chmurze API zastosuje migrację samo przy starcie.
3. `UserService`:
   - `UploadAvatarAsync`: walidacja zostaje (2 MB, JPG/PNG); zamiast bajtów `AvatarBlobName = await _images.UploadAsync("avatars", user.Id.ToString(), file)`, poprzedni plik usuń, `AvatarUrl = $"/api/users/{user.Id}/avatar"`;
   - `SetAvatarPresetAsync`: zamiast czyszczenia bajtów usuń plik i wyczyść `AvatarBlobName`;
   - zamiast `GetAvatarImageAsync`: metoda zwracająca link SAS dla podanego `userId` (404, gdy pliku nie ma).
4. `UsersController`: nowe `[HttpGet("{id:guid}/avatar")]` z `[AllowAnonymous]` (cały kontroler ma `[Authorize]`), które zwraca `Redirect(uri.ToString())`. Stare `GET me/avatar/image` usuń.
5. `UserBookService` i `UserBooksController`:
   - `UploadCoverAsync`: zapis do `covers`, usunięcie poprzedniego pliku, `CustomCoverBlobName`;
   - `GetCoverImageAsync` zwraca link SAS (404, gdy pliku nie ma), a `GetCoverImage` w kontrolerze robi `Redirect(...)`;
   - `DeleteCoverAsync`: usuń plik i wyczyść nazwę;
   - `ResolveCoverUrl` i `HasCustomCover` sprawdzają `CustomCoverBlobName != null`.
6. Frontend: zwykle bez zmian. Wszystkie miejsca biorą avatar z pola `avatarUrl`, a `EditBookView.vue` dalej trafia w ten sam endpoint okładki, który teraz przekierowuje. Po zmianie sprawdź karty w przeglądaniu (`BookCard.vue`), szczegóły oferty (`OfferModal.vue`), karty transakcji (`BookOffer.vue`), pasek (`AppNavbar.vue`) i profil. Dane zalogowanego użytkownika leżą w `localStorage`, więc po zmianie zaloguj się ponownie.
7. Reguła cyklu życia: konto Storage → **Data management → Lifecycle management** → **Add a rule**, np. pliki niezmieniane od 30 dni → warstwa **Cool**. Bez przenoszenia do **Archive**, bo z tej warstwy obrazów nie da się od razu odczytać. 📸
8. Opcjonalnie: karty transakcji pokazują zawsze okładkę z katalogu (`TransactionService.cs:317`), nawet gdy oferta ma własną. Można tam użyć tej samej logiki co w `ResolveCoverUrl`.

**Test:**

- konto A wgrywa avatar i okładkę, a pliki widać w portalu (konto Storage → **Containers**) 📸;
- konto B widzi avatar A na karcie oferty i w transakcji;
- `GET /api/users/{id}/avatar` w przeglądarce kończy się na adresie `…blob.core.windows.net/avatars/…?sv=…&sig=…`;
- usunięcie okładki usuwa plik z kontenera.

**Gotowe, gdy:** obrazy leżą w Blob Storage (widać to w portalu i w aplikacji), a cudze avatary są widoczne.

## 3. Zdarzenia transakcji

**Cel:** każda zmiana statusu prośby trafia jako zdarzenie do kolejki `transaction-events`. Na tej podstawie funkcja zapisze historię (krok 4), a w T3 Jakub doda powiadomienia.

**Stan dziś:** prośby to encja `TransactionRequest` (`Models/Entities/TransactionRequest.cs`):

- pola `Id`, `RequesterId`, `UserBookId`, `Type` (`loan` / `swap` / `sale`), `Status`, `Message`, `DueDate`, `CreatedAt`, `UpdatedAt`;
- statusy to napisy `pending` / `accepted` / `done` / `rejected` (stałe w `TransactionService.cs:11-20`);
- właściciel oferty to `UserBook.UserId`.

Statusy zmienia `Services/TransactionService.cs`:

| Metoda | Co się dzieje | Zdarzenia do wysłania |
|---|---|---|
| `CreateAsync` (`:29-89`) | nowa prośba `pending` | `Requested`: od proszącego do właściciela |
| `AcceptAsync` (`:144-192`) | prośba → `accepted`; pozostałe oczekujące prośby na tę ofertę → `rejected` (lista `otherPendingTransactions`, `:174-185`) | `Accepted` od właściciela do proszącego; dla każdej odrzuconej automatycznie `AutoRejectedOnAccept` do jej autora |
| `RejectAsync` (`:194-210`) | prośba → `rejected` | `Rejected` od właściciela do proszącego |
| `FinishAsync` (`:212-243`) | prośba → `done`; pozostałe oczekujące → `rejected` (lista `otherPending`, `:225-236`) | `Finished` od właściciela do proszącego; dla każdej odrzuconej `AutoRejectedOnFinish` do jej autora |

**Do zrobienia:**

- [ ] model zdarzenia (pola niżej);
- [ ] `IEventPublisher` wstrzyknięty do `TransactionService`;
- [ ] zdarzenia z 6 ścieżek z tabeli, wysyłane na końcu metody, po zapisaniu zmian;
- [ ] błąd kolejki nie psuje akcji użytkownika: łapiesz wyjątek i logujesz ostrzeżenie (tak już działa cache);
- [ ] ostateczny format komunikatu opisany w PR, bo korzysta z niego T3.

**Jak:**

1. Model, np. `Models/Events/TransactionEvent.cs`:

   ```csharp
   public record TransactionEvent(
       Guid EventId,
       Guid TransactionId,
       string Type,              // Requested | Accepted | AutoRejectedOnAccept | Rejected | Finished | AutoRejectedOnFinish
       Guid ActorUserId,         // kto wykonał akcję
       Guid RecipientUserId,     // kogo dotyczy (w T3 dostanie powiadomienie)
       Guid UserBookId,
       string? BookTitle,
       DateTime OccurredAt);
   ```

2. W `TransactionService` (dodaj do konstruktora `IEventPublisher` i `ILogger<TransactionService>`) metoda pomocnicza:

   ```csharp
   private async Task PublishSafeAsync(TransactionEvent evt)
   {
       try { await _events.PublishAsync(evt); }
       catch (Exception ex) { _logger.LogWarning(ex, "Nie udało się wysłać zdarzenia {Type}", evt.Type); }
   }
   ```

   Wywołuj ją na końcu każdej z 4 metod. W `AcceptAsync` i `FinishAsync` dopiero po `SetOfferStatusAsync`, żeby zdarzenie poszło po obu zapisach. `BookTitle` weź z `UserBook.Book.Title`; jeśli w danej metodzie `Book` nie jest wczytany, dołóż `Include`.
3. Przy okazji popraw nieaktualny komentarz w `ITransactionService.cs:22`: Finish nie przywraca oferty jako dostępnej, tylko ustawia `sold` / `completed`.

**Test (lokalnie z Azurite albo w chmurze):**

- wyślij prośbę i ją odrzuć; wyślij dwie prośby z dwóch kont, zaakceptuj jedną, potem zakończ;
- w Azure Storage Explorer albo w portalu (**Queues** → `transaction-events`) widać komunikaty z właściwymi typami. Gdy działa już funkcja z kroku 1.4, komunikaty od razu znikają z kolejki, a ich treść widać w logach funkcji.

**Gotowe, gdy:** każda z 6 ścieżek wysyła zdarzenie, a format jest opisany w PR.

## 4. Historia transakcji

**Cel:** w karcie transakcji widać oś czasu: kiedy wysłano prośbę i kiedy ją zaakceptowano, zakończono albo odrzucono.

**Do zrobienia:**

- [ ] `ProcessTransactionEvent` zapisuje każde zdarzenie w tabeli `TransactionHistory`;
- [ ] `GET /api/transactions/{id}/history`, dostępny tylko dla stron transakcji;
- [ ] oś czasu w karcie transakcji we frontendzie.

**Jak:**

1. Funkcja (pakiet `Azure.Data.Tables` w projekcie Functions):

   ```csharp
   public class ProcessTransactionEvent(TableClient history, ILogger<ProcessTransactionEvent> logger)
   {
       [Function(nameof(ProcessTransactionEvent))]
       public async Task Run([QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
       {
           var evt = JsonSerializer.Deserialize<TransactionEvent>(message)!;
           var entity = new TableEntity(evt.TransactionId.ToString(), $"{evt.OccurredAt:yyyyMMddHHmmssfff}-{evt.EventId}")
           {
               ["Type"] = evt.Type,
               ["ActorUserId"] = evt.ActorUserId.ToString(),
               ["OccurredAt"] = evt.OccurredAt
           };
           await history.UpsertEntityAsync(entity);   // upsert: ponowione zdarzenie nie zrobi duplikatu
           logger.LogInformation("Historia: {Type} dla {Id}", evt.Type, evt.TransactionId);
       }
   }
   ```

   - `TableClient` dla `TransactionHistory` zarejestruj w `Program.cs` projektu Functions z ustawienia `StorageConnection`, z `CreateIfNotExists` przy starcie.
   - `TransactionEvent` skopiuj z API albo przenieś do małego wspólnego projektu.
   - RowKey zaczynający się od czasu sortuje wpisy chronologicznie, a `EventId` w kluczu chroni przed duplikatem przy ponowieniu.
2. API (pakiet `Azure.Data.Tables`): `TableClient` dla `TransactionHistory` rejestrujesz jak kolejkę (jest connection string → tabela, nie ma → pusta historia). W `TransactionService` metoda `GetHistoryAsync(currentUserId, transactionId)`:
   - wczytaj prośbę z `UserBook` i sprawdź, że wywołujący to proszący albo właściciel, tak jak w `GetContactAsync` (`:115`); inaczej 403;
   - `history.QueryAsync<TableEntity>(e => e.PartitionKey == transactionId.ToString())` → lista DTO `{ type, occurredAt, actorUserId }` posortowana po czasie.

   W `TransactionsController` (cały wymaga logowania): `[HttpGet("{id:guid}/history")]`.
3. Frontend:
   - `src/lib/transactions.ts`: `getHistory(id)` wołające `/api/transactions/${id}/history` z `{ auth: true }`; typ wpisu w `src/lib/types.ts`;
   - `src/components/BookOffer.vue`: w części z kartą transakcji (`:179-294`) przycisk „Historia”, który pobiera wpisy i pokazuje listę z datą i polskim opisem typu (np. `Requested` → „Prośba wysłana”, `AutoRejectedOnAccept` → „Odrzucona automatycznie”);
   - karty renderuje `ShelfView.vue` (zakładki Otrzymane / Wysłane), a dane mapuje `mapTransactionToShelfItem` w `src/lib/mappers.ts:135-158`.

**Test:** prośba → akceptacja → zakończenie, potem „Historia” w karcie pokazuje 3 wpisy we właściwej kolejności. W portalu (konto Storage → **Tables** → `TransactionHistory`) widać wiersze. 📸

**Gotowe, gdy:** po akcjach na transakcji historia jest widoczna w aplikacji.

## 5. Miniatury (jeśli zostanie czas)

**Cel:** lista ofert ładuje małe miniatury zamiast pełnych okładek.

**Jak:**

1. Po zapisaniu okładki API wrzuca do kolejki `image-jobs` komunikat `{ "container": "covers", "blobName": "…" }` (ten sam `QueueClient` z Base64, inna nazwa kolejki).
2. Funkcja `GenerateThumbnail` z `[QueueTrigger("image-jobs")]` pobiera plik, zmniejsza go biblioteką `SixLabors.ImageSharp` (np. szerokość 300 px) i zapisuje w kontenerze `thumbnails` pod tą samą nazwą.
3. Endpoint okładki z parametrem, np. `GET /api/userbooks/{id}/cover/image?size=thumb`, przekierowuje na miniaturę, jeśli istnieje, a jeśli nie, na oryginał. Lista ofert używa wersji `thumb`.

**Gotowe, gdy:** po wgraniu okładki w `thumbnails` pojawia się plik, a lista ofert go używa. Jeśli nie zdążysz, miniatury przechodzą na tydzień poprawek (T4).

## 6. Dodatki po T2 (do 15.11, jeśli zostanie czas)

Dodatek 3 (VM staging z Dockerem) i 4 (Azure Files na VM): opis w analizie, sekcja 6.6. VM ma płatny publiczny adres IP, więc najpierw zgoda Łukasza.

## 7. Minimum na koniec T2

- obrazy leżą w Blob Storage (widać to w portalu i w aplikacji);
- cudze avatary są widoczne;
- po akcjach na transakcji historia jest widoczna w aplikacji;
- Function App wdraża się automatycznie z `main`;
- zrzuty ekranu.

Na koniec napisz Łukaszowi, co zrobiłeś, a czego nie (i co ewentualnie przechodzi dalej).
