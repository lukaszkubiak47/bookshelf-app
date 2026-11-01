# T1: zakładanie zasobów w portalu Azure, krok po kroku

> Dla: Łukasz (T1). To ściąga: przechodzimy ją razem na czacie, jeden krok na raz.
> 📸 = zrób zrzut ekranu do raportu (sekcja „Przygotowanie projektu") i zapisz w `azure/raport/t1/`. Raport piszemy na końcu, ale zrzuty robimy na bieżąco, bo ekranów kreatora nie da się później odtworzyć. Na zrzutach nie może być haseł ani kluczy.
> Nazwy z sufiksem `-lk47` muszą być globalnie unikalne. Jeśli portal powie, że nazwa jest zajęta, zmień sufiks i zanotuj nową nazwę.

| Zasób | Nazwa |
|---|---|
| Grupa zasobów | `rg-bookshelf` |
| Serwer SQL / baza | `sql-bookshelf-lk47` / `BookshelfDB` |
| Plan App Service / aplikacja API | `asp-bookshelf` / `app-bookshelf-api-lk47` |
| Application Insights | `appi-bookshelf` |
| Static Web App (frontend) | `swa-bookshelf` |

## 0. Git (u Ciebie lokalnie)

```bash
git pull --ff-only
git switch -c t1/azure-fundament
```

Moje zmiany w plikach przejdą razem z Tobą na nową gałąź.

## 1. Region

Portal → **Policy** → **Assignments** → poszukaj przypisania typu *Allowed locations* / *Allowed resource deployment regions* i otwórz jego parametry, żeby zobaczyć listę dozwolonych regionów.

- Jeśli jest na niej **Poland Central**, bierzemy ją. W przeciwnym razie najbliższy region z listy (Germany West Central, Sweden Central, North Europe, West Europe).
- 📸 lista dozwolonych regionów.

**Wynik (01.11):** dozwolone `italynorth`, `austriaeast`, `germanywestcentral`, `denmarkeast`, `switzerlandnorth` (bez Poland Central). Wybrany **Germany West Central**: najbliżej, w cenniku są tam wszystkie usługi z planu, a ceny są jak w Poland Central.

## 2. Grupa zasobów

**Resource groups** → **Create**:

- Name: `rg-bookshelf`
- Region: **Germany West Central**
- (opcjonalnie) tag `project = bookshelf`

📸 po utworzeniu.

## 3. Dostawcy usług (jednorazowo)

Twoja subskrypcja → **Settings → Resource providers**. Zarejestruj te, które nie mają statusu *Registered*:

`Microsoft.Web`, `Microsoft.Sql`, `Microsoft.Storage`, `Microsoft.Cache`, `Microsoft.DocumentDB`, `Microsoft.CognitiveServices`, `Microsoft.ServiceBus`, `Microsoft.Insights`, `Microsoft.OperationalInsights`, `Microsoft.AlertsManagement`, `Microsoft.Compute`, `Microsoft.Network`.

## 4. Azure SQL Database (free tier)

**SQL databases** → **Create** → **SQL database (Free offer)**. Ta opcja od razu stosuje darmową ofertę, ale otwiera uproszczony formularz (tylko Basics i Review + create, bez ustawień sieci). Wpisz grupę, nazwę bazy i utwórz serwer, a potem kliknij **Advanced configuration**: otworzy się pełny formularz z zakładkami poniżej, a darmowa oferta zostaje. W pełnym formularzu sprawdź pole **Server**: u nas portal podmienił nazwę na login admina (`bookshelfadmin`). Wtedy kliknij **Create new** i wpisz serwer jeszcze raz, bo nazwy serwera nie da się później zmienić.

**Basics:**

- Resource group: `rg-bookshelf`, Database name: `BookshelfDB`.
- Server → **Create new**:
  - Name: `sql-bookshelf-lk47`, Location: Germany West Central;
  - Authentication method: **Use both SQL and Microsoft Entra authentication**;
  - Microsoft Entra admin: Ty;
  - Server admin login: np. `bookshelfadmin` + silne hasło (zapisz je w menedżerze haseł; bez znaków `;` `'` `"` `{` `}` `=`, które psują connection string).
- Elastic pool: **No**. Przy darmowej ofercie tego pola nie ma, bo darmowa baza nie może być w puli.
- Workload environment (jeśli jest): **Development**. Backup storage redundancy (jeśli da się wybrać): **Locally-redundant**.
- Nad formularzem kliknij baner **„Apply offer"** (potrzebne tylko w zwykłym „SQL database”; w opcji Free offer oferta jest od razu). Behavior when free limit reached: **Auto-pause the database until next month**.
- 📸 zakładka Basics z darmową ofertą.

**Networking:**

- Connectivity method: **Public endpoint**. W pełnym formularzu domyślnie jest *No access*, przy którym nikt się nie połączy.
- **Allow Azure services and resources to access this server: Yes** (dzięki temu łączy się App Service).
- **Add current client IP address: Yes** (dzięki temu łączysz się Ty).
- 📸.

**Security:** Microsoft Defender for SQL ustaw na **Not now** (to płatna usługa).

**Additional settings:** Use existing data: **None**. Tabele utworzą migracje EF przy pierwszym starcie API.

**Review + create** → sprawdź, że podsumowanie kosztów pokazuje darmową ofertę (0 USD) → **Create** → 📸 ekran „Your deployment is complete".

Na koniec sprawdź, że hasło `bookshelfadmin` jest w menedżerze haseł (np. w przeglądarce). To jedyny sekret bazy, którego portal później nie pokaże. Gdyby zginęło: serwer `sql-bookshelf-lk47` → **Overview** → **Reset password**, a potem nowe hasło trzeba wpisać też w connection stringu w App Service. Samego connection stringa nie przechowujemy, bo bez hasła nie jest tajny: w kroku 5 skopiujesz go z portalu i wstawisz hasło.

Test (📸): baza → **Query editor (preview)** → zakładka **SQL authentication** (Username `bookshelfadmin` + Password) → **Connect** → zapytanie poniżej → **Run**. Wynik: `BookshelfDB`, `0` tabel (utworzy je API) i wersja serwera. Potem zamknij edytor, bo otwarte połączenie nie pozwala bazie się uśpić i zużywa darmowy limit. Błąd *Database … is not currently available* po **Connect** oznacza, że baza spała (auto-pause) i to logowanie ją budzi: odczekaj około minuty i kliknij **Connect** jeszcze raz.

```sql
SELECT DB_NAME() AS Baza, (SELECT COUNT(*) FROM sys.tables) AS LiczbaTabel, @@VERSION AS Wersja;
```

## 5. App Service (API)

**App Services** → **Create** → **Web App**.

**Basics:**

- Resource group: `rg-bookshelf`, Name: `app-bookshelf-api-lk47`. Pod nazwą portal pokazuje adres z losową końcówką (*secure unique default hostname*). Jest włączone automatycznie, bez przełącznika.
- Publish: **Code**, Runtime stack: **.NET 10 (LTS)**, Operating System: **Linux**, Region: **Germany West Central**.
- Linux Plan → **Create new**: `asp-bookshelf` (`asp` = App Service plan, przedrostek z konwencji nazw Microsoftu). Pricing plan: **Basic B1** (sprawdź, bo portal potrafi podsunąć droższy Premium).
- Zone redundancy: **Disabled** (w B1 niedostępne). Managed Instance: **Off**.

**Database:** nie twórz bazy, mamy ją z kroku 4.

**Deployment:**

- Continuous deployment: **Disable**. Wdrażamy własnym workflowem z repo, więc portal nie dopisze nic do GitHuba.
- **Basic authentication: Enable**. Bez tego nie pobierzesz „publish profile".

**Networking:** bez zmian: public access **On**, virtual network integration **Off**.

**Monitor + secure:** Enable Application Insights: **Yes** → nowy `appi-bookshelf`. Portal może przy tym założyć grupę `DefaultResourceGroup-…` z obszarem Log Analytics; to normalne.

**Review + create** → sprawdź: Basic (B1) za ok. 13 USD/mies., Linux, .NET 10 (LTS), Germany West Central → **Create** → 📸.

Po utworzeniu:

1. **Overview** → skopiuj **Default domain**, np. `app-bookshelf-api-lk47….azurewebsites.net`.
2. **Settings → Environment variables → App settings** → dodaj poniższe → **Apply** (aplikacja się zrestartuje) → 📸 lista ustawień **z ukrytymi wartościami**.

   | Nazwa | Wartość |
   |---|---|
   | `ConnectionStrings__Default` | connection string bazy z wstawionym hasłem (jak go przygotować: pod tabelą) |
   | `JwtSettings__SecretKey` | **nowy** losowy sekret. W Git Bash: `openssl rand -base64 48` |
   | `GoogleBooks__ApiKey` | klucz Google Books. Opcjonalny: bez niego wyszukiwanie też działa, tylko Google szybciej je ogranicza. Doda go Jakub, gdy dostanie dostęp (krok 10) |
   | `RunMigrations` | `true` |

   Connection string: baza `BookshelfDB` → **Settings → Connection strings** → **ADO.NET (SQL authentication)** → skopiuj → wklej do nowej karty Notatnika → **Ctrl+H**: znajdź `{your_password}`, zamień na hasło z menedżera haseł → skopiuj całość jako wartość ustawienia → zamknij tę kartę (**Ctrl+W**) i wybierz **Nie zapisuj**. Samo zamknięcie okna nie wystarczy, bo Notatnik w Windows 11 przywraca niezapisane karty. Warianty *Microsoft Entra password* i *Microsoft Entra integrated* logują człowieka (hasłem konta uczelnianego albo kontem Windows w domenie organizacji), więc API ich nie użyje.

   `Cors__AllowedOrigins__0` dodasz w kroku 6, gdy będzie już adres frontendu.

3. **Settings → Configuration → General settings**: **Always On: On**, przy okazji sprawdź **HTTPS Only: On** → zapisz i potwierdź restart. Always On usuwa „zimny start". Bazy nie budzi, bo pinguje tylko `/`, a pod tym adresem API nic nie ma i żaden middleware nie łączy się z bazą (sprawdzone 01.11).
4. **Overview** → **Download publish profile** (plik `.PublishSettings`, to sekret, nie commitujemy go). Trzymaj go w Pobranych do kroku 7, potem usuń. `.gitignore` i tak ignoruje `*.publishsettings`. Gdyby wyciekł: **Reset publish profile** i pobierz nowy.

## 6. Static Web Apps (frontend)

> ⚠️ Static Web Apps pozwala wybrać tylko kilka regionów (m.in. West Europe, East US 2), a żadnego z nich nie ma na naszej liście dozwolonych. Najpewniej potrzebny będzie zamiennik (propozycja: statyczna strona w Azure Storage). Decyzja w tym kroku.

**Static Web Apps** → **Create**:

- Resource group: `rg-bookshelf`, Name: `swa-bookshelf`.
- Plan type: **Free**.
- Region (dla Functions/staging): np. **West Europe**. Statyczne pliki i tak idą globalnie.
- Deployment source: **Other**. Wdrażamy własnym workflowem, więc Azure nie commituje nic do repo.

**Review + create** → **Create** → 📸.

Po utworzeniu:

1. **Overview** → skopiuj **URL** (`https://….azurestaticapps.net`). W App Service → **Environment variables** dodaj `Cors__AllowedOrigins__0` = ten adres, bez `/` na końcu → **Apply**.
2. **Overview** → **Manage deployment token** → skopiuj token.

## 7. GitHub: sekrety i zmienne dla workflowów

Repo na GitHubie → **Settings → Secrets and variables → Actions**.

**Secrets:**

| Nazwa | Wartość |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | cała zawartość pliku publish profile z kroku 5 |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | token z kroku 6 |

**Variables:**

| Nazwa | Wartość |
|---|---|
| `AZURE_WEBAPP_NAME` | `app-bookshelf-api-lk47` |
| `VITE_API_BASE_URL` | `https://` + Default domain API z kroku 5 (bez `/` na końcu) |

## 8. Pierwsze wdrożenie

> Kolejność (01.11): najpierw samo API. Pierwszy PR ma commity backendu i `api.yml`, a frontend (krok 6, jego sekret i `frontend.yml`) wchodzi drugim PR-em, gdy zapadnie decyzja o hostingu. Jedna rzecz naraz = łatwiej znaleźć problem.

GitHub pokazuje przycisk **Run workflow** tylko dla workflowów, które są już na `main`. Dlatego pierwsze wdrożenie robimy przez scalenie gałęzi do `main`:

1. Zrób commity i push gałęzi `t1/azure-fundament`. Workflowy są w `.github/workflows/` (`api.yml`, `frontend.yml`).
2. GitHub → **Pull requests** → **New pull request** → `main` ← `t1/azure-fundament` → **Create pull request** → **Merge pull request**. Po scaleniu workflowy **API** i **Frontend** ruszą same, bo na `main` trafiły zmiany w `backend/` i `frontend/`. 📸 zielone przebiegi w zakładce **Actions**.
3. Wejdź na adres Static Web App i sprawdź rejestrację, logowanie, dodanie książki, przeglądanie i transakcję. 📸.

Kolejne wdrożenie bez zmian w kodzie (np. po zmianie zmiennej `VITE_API_BASE_URL`): **Actions** → wybierz workflow → **Run workflow**. Zmiana App Settings w portalu wdrożenia nie wymaga, bo App Service sam się restartuje.

## 9. Azure Managed Redis (po udanym pierwszym wdrożeniu)

Kod jest gotowy: bez Redisa API działa na zamiennikach w pamięci, a po dodaniu connection stringa samo przełącza się na Redis.

**Azure Managed Redis** → **Create**:

- Resource group: `rg-bookshelf`, Name: `redis-bookshelf-lk47`, Region: Twój. Jeśli Redis nie jest dostępny w tym regionie, wybierz najbliższy dozwolony.
- Performance / SKU: **Balanced B0**, **High availability: wyłączone** (wersja dev/test, ok. 0,021 USD/h).
- **Networking:** Public endpoint.
- **Advanced:**
  - **Access keys authentication: Enabled** (aplikacja łączy się kluczem);
  - Clustering policy: **Enterprise** (jeden adres, najprostsza sieć; nie da się tego zmienić później).
- 📸.

Po utworzeniu:

1. Skopiuj endpoint (`redis-bookshelf-lk47.<region>.redis.azure.net`, port **10000**) i klucz z sekcji **Authentication → Access keys**.
2. App Service → **Environment variables** → dodaj:

   | Nazwa | Wartość |
   |---|---|
   | `ConnectionStrings__Redis` | `<endpoint>:10000,password=<klucz>,ssl=True,abortConnect=False` |

3. Test: wyszukaj tę samą książkę w Google Books dwa razy (drugie wyszukiwanie z cache), a potem 11 razy wpisz złe hasło przy logowaniu (11. próba daje komunikat „Zbyt wiele prób"). 📸.

## 10. (Opcjonalnie dziś) dostęp dla zespołu

> Subskrypcja jest w katalogu uczelni: University of Gdansk (for Students), `studms.ug.edu.pl`. Jeśli Wojciech i Jakub mają konta studenckie UG, prawdopodobnie już są w tym katalogu. Wtedy pomijasz zaproszenie (punkt 1) i od razu nadajesz rolę. Zapraszanie gości może być w katalogu uczelni zablokowane.

1. **Microsoft Entra ID → Users → New user → Invite external user**: e-maile Wojciecha i Jakuba.
2. Subskrypcja → **Access control (IAM) → Add role assignment** → **Contributor** → wybierz ich.
3. Jakub dodaje klucz Google Books: App Service → **Settings → Environment variables** → **+ Add** → `GoogleBooks__ApiKey` → **Apply** na dole strony → **Confirm**.
