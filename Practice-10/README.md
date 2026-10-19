# Практична робота 10 — інтеграція зовнішнього HTTP API

## Реалізовано

- Контракт `IGeocodingService` та DTO `Coordinates` у Application.
- Typed `HttpClient` для Geoapify в Infrastructure.
- Конфігурація BaseUrl, timeout, мови, країни та TTL кешу.
- Ключ API зберігається через .NET User Secrets або змінну середовища.
- Nullable координати `Latitude` і `Longitude` та міграція.
- Кеш позитивних результатів на 60 хвилин і негативних на 10 хвилин.
- Тимчасові HTTP-, мережеві, timeout- і JSON-помилки не кешуються.
- Скасування запиту передається до зовнішнього HTTP-виклику.
- POST створює подію з координатами або null.
- PUT повторно геокодує лише після зміни адреси й очищає застарілі координати.
- WireMock-тести перевіряють успіх, порожній результат, HTTP-відмови й невалідний JSON.

## Налаштування

Потрібні .NET 10 SDK, PostgreSQL і ключ Geoapify.

У терміналі з кореня репозиторію:

```bash
dotnet user-secrets set "Geocoding:ApiKey" "<your-geoapify-key>" --project EventHub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventhub_dev;Username=postgres;Password=your_password" --project EventHub.Api
```

Не додавайте справжній ключ до `appsettings.json`, `.http`, README або Git.

Альтернатива для змінної середовища:

```bash
Geocoding__ApiKey=<your-geoapify-key>
```

## Запуск

```bash
dotnet restore EventHub.sln
dotnet build EventHub.sln
dotnet run --project EventHub.Api
```

У `appsettings.json` зберігаються лише несекретні параметри геокодування. Timeout за замовчуванням — 5 секунд.

## Міграція

Якщо база даних уже створена попередніми роботами:

```bash
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api
```

Якщо інструмент EF відсутній:

```bash
dotnet tool install --global dotnet-ef
```

Міграція `AddEventCoordinates` додає `Location`, `Latitude` і `Longitude` до таблиці `Events`. Вона передбачає, що таблиця `Events` вже існує з попередніх практичних робіт.

## Приклад POST

```http
POST http://localhost:5230/events
Content-Type: application/json

{
  "name": "ASP.NET Core Community Day",
  "description": "Подія для перевірки зовнішнього API",
  "location": "Майдан Незалежності, Київ, Україна",
  "startsAtUtc": "2027-05-20T07:00:00Z",
  "endsAtUtc": "2027-05-20T12:00:00Z"
}
```

Очікується `201 Created`. За успішного геокодування відповідь містить `latitude` і `longitude`. Якщо Geoapify не знаходить адресу або тимчасово недоступний, подія зберігається з `null` координатами.

## Приклад PUT

```http
PUT http://localhost:5230/events/1
Content-Type: application/json

{
  "name": "ASP.NET Core Community Day — оновлено",
  "description": "Нова адреса",
  "location": "площа Ринок, Львів, Україна",
  "startsAtUtc": "2027-05-20T07:00:00Z",
  "endsAtUtc": "2027-05-20T12:00:00Z"
}
```

Якщо нормалізована адреса не змінилася, координати зберігаються. Якщо адреса змінилася, оновлюються обидва поля координат, зокрема вони можуть стати `null`.

## Тести

```bash
dotnet test EventHub.Tests/EventHub.Tests.csproj --filter FullyQualifiedName~GeoapifyGeocodingServiceIntegrationTests
dotnet test EventHub.sln
```

WireMock запускає локальний контрольований HTTP-сервер. Тести не звертаються до Geoapify і не потребують справжнього ключа.

## Самоперевірка

1. Чим відрізняється порожній `results` від HTTP 429?
2. Чому таймаут не кешується?
3. Чому ключ API не зберігається в Git?
4. Як `CancellationToken` передається від контролера до HTTP-клієнта?
5. Чому під час зміни адреси потрібно оновити обидва поля координат?
