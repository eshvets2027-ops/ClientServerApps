# Практична робота 11 — оптимізація запитів та індекси

## Реалізація

- Seeder генерує тестові події пакетами й доступний лише в Development.
- Складений B-tree індекс `IX_Events_StartsAtUtc_Id` покриває фільтрацію за `StartsAtUtc` і сортування за `StartsAtUtc, Id`.
- Список подій використовує `AsNoTracking()`, пагінацію та проєкцію з підрахунком реєстрацій на стороні PostgreSQL.
- `pageSize` обмежено діапазоном 1–100.
- Міграція `AddEventDateIndex` додає лише індекс; `Down()` видаляє лише індекс.

## Налаштування

Налаштуйте рядок підключення через User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventhub;Username=postgres;Password=YOUR_PASSWORD" --project EventHub.Api
```

Встановіть середовище Development.

## Застосування міграцій

```bash
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

## Заповнення тестовими даними

Запускайте лише локально в Development:

```bash
dotnet run --project EventHub.Api -- --PerformanceSeed:Enabled=true --PerformanceSeed:EventCount=100000 --PerformanceSeed:BatchSize=2000
```

Для заміни попередніх тестових даних додайте `--PerformanceSeed:Reset=true`. Seeder видаляє події, назви яких починаються з `Performance event `, тому не використовуйте цей префікс для реальних подій.

Після завершення процес заповнення завершується без запуску HTTP-сервера.

## Вимірювання до індексу

Щоб порівняти запит до/після, тимчасово відкотіть лише міграцію індексу:

```bash
dotnet ef database update 202610190001_AddEventCoordinates --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

Оновіть статистику та виконайте:

```sql
ANALYZE "Events";

EXPLAIN (ANALYZE, BUFFERS)
SELECT "Id", "StartsAtUtc", "Name"
FROM "Events"
WHERE "StartsAtUtc" >= '2035-06-15T00:00:00Z'
  AND "StartsAtUtc" < '2035-06-16T00:00:00Z'
ORDER BY "StartsAtUtc", "Id"
LIMIT 20;
```

Збережіть тип сканування, `Rows Removed by Filter`, наявність `Sort`, `Buffers` і `Execution Time`.

## Вимірювання після індексу

```bash
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

Повторіть той самий SQL. Планувальник може вибрати `Index Scan`, `Bitmap Index Scan` або `Seq Scan`, якщо вважає його дешевшим. Фіксуйте реальні показники, не підставляйте прикладові значення.

## Перевірка API

```http
GET http://localhost:5230/events?dateFrom=2035-06-15&dateTo=2035-06-15&page=1&pageSize=20
Accept: application/json
```

Перевірка межі сторінки:

```http
GET http://localhost:5230/events?pageSize=101
Accept: application/json
```

Очікується `400 Bad Request`.

## Перевірка індексу

```sql
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'Events'
ORDER BY indexname;
```

## Перевірка

```bash
dotnet build EventHub.sln
dotnet test EventHub.sln
```

Час виконання залежить від обладнання, кешу, статистики PostgreSQL та розміру даних. У цьому середовищі .NET SDK недоступний, тому компіляцію й тести тут не було запущено.
