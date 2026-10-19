# Практична робота 9 — асинхронність і фонові задачі

## Що реалізовано

- Обмежена черга `Channel<T>` місткістю 100.
- Singleton-сховище станів фонових задач у пам’яті.
- `BackgroundTaskWorker` на основі `BackgroundService`.
- Імітація підтвердження через `Task.Delay` на дві секунди з журналюванням.
- `POST /events/{eventId}/registrations` збережує реєстрацію та повертає `jobId`, не очікуючи завершення підтвердження.
- `GET /jobs/{id}` повертає `pending`, `processing`, `done`, `failed` або `cancelled`.
- Модульні тести черги та сховища станів.

## Запуск

Потрібен .NET 10 SDK і PostgreSQL. Рядок підключення зберігайте в User Secrets:

```bash
dotnet user-secrets init --project EventHub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventhub_dev;Username=postgres;Password=your_password" --project EventHub.Api
```

Якщо PostgreSQL запускається через Docker Compose:

```bash
POSTGRES_PASSWORD=your_password docker compose up -d
```

Застосуйте наявні міграції, якщо вони є у вашій версії solution, потім запустіть API:

```bash
dotnet run --project EventHub.Api --launch-profile http
```

Swagger доступний у Development. Спершу створіть подію через `POST /events`, потім використайте її ID для реєстрації.

## Перевірка HTTP

Створіть реєстрацію:

```http
POST http://localhost:5230/events/1/registrations
Content-Type: application/json

{
  "attendeeName": "Oksana",
  "attendeeEmail": "oksana@example.com"
}
```

У відповіді скопіюйте `jobId`. Перевірте стан:

```http
GET http://localhost:5230/jobs/ВАШ-JOB-ID
Accept: application/json
```

Після приблизно двох секунд очікуваний стан — `done`. Невідомий `jobId` повертає `404 Problem Details`.

## Тести

```bash
dotnet test EventHub.Tests/EventHub.Tests.csproj --no-restore
```

Для інтеграційних тестів із PostgreSQL Testcontainers потрібен запущений Docker Engine.

## Важливі обмеження

Стан і черга зберігаються лише в пам’яті та зникають після перезапуску процесу. Нові міграції та NuGet-пакети не додавалися. У наданій базовій версії EventHub немає поля місткості події або окремого RegistrationService, тому контролер перевіряє існування події та повторну реєстрацію email для тієї самої події, але не може перевіряти ліміт місць без зміни моделі.

## Питання для самоперевірки

1. Чим асинхронність відрізняється від фонового виконання?
2. Чи створює `async` новий потік?
3. Навіщо передавати `CancellationToken`?
4. Чому `Task.Run` не замінює керовану чергу?
5. Навіщо `Channel<T>` має обмежену місткість?
6. Чому черга й сховище станів зареєстровані як singleton?
7. Що відбувається зі станами після перезапуску застосунку?
8. Коли варто перейти до стійкої черги або зберігання задач у БД?
