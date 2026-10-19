# Практична робота №6 — інтеграційні тести ASP.NET Core API

Проєкт: EventHub  
Платформа: .NET 10  
Тестування: xUnit, Moq, WebApplicationFactory, Testcontainers PostgreSQL, Coverlet

## Вимоги
- .NET 10 SDK
- Docker Desktop або сумісний Docker Engine
- Docker має бути запущений перед інтеграційними тестами

## Перевірка середовища
```bash
dotnet --version
docker info
```

## Запуск
З кореня `Practice-06`:
```bash
dotnet restore EventHub.sln
dotnet build EventHub.sln
dotnet test EventHub.Tests/EventHub.Tests.csproj --filter FullyQualifiedName~Unit
dotnet test EventHub.Tests/EventHub.Tests.csproj --filter FullyQualifiedName~Integration --settings coverlet.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults
```

Інтеграційні тести запускають одноразовий PostgreSQL-контейнер через Testcontainers. Вони не використовують базу розробки. Перший запуск може тривати довше через завантаження Docker-образу.

## Реалізовані сценарії
- Модульний тест `RegistrationService` з Moq.
- Успішне створення реєстрації: `201 Created`, `Location`, JSON та запис у БД.
- Повторний email: `409 Conflict` без дубліката.
- Відсутня подія: `404 Not Found`.
- Немає вільних місць: `422 Unprocessable Entity` без нового запису.
- Попередня реєстрація скасована: нова реєстрація дозволена.
- Некоректне тіло запиту: `400 Bad Request`.
- PostgreSQL схема створюється міграціями перед тестовими сценаріями.
- Один PostgreSQL-контейнер спільний для інтеграційної колекції; дані очищуються перед кожним тестом.

## Чому використано PostgreSQL Testcontainer
EF Core InMemory не є реляційною базою та не відтворює повністю SQL, зовнішні ключі, транзакції й специфічну поведінку PostgreSQL. Testcontainers запускає справжній PostgreSQL у Docker, тому тести перевіряють EF Core, міграції та HTTP-потік ближче до робочого середовища.

## Arrange–Act–Assert
Кожен тест готує мінімальні дані (Arrange), виконує HTTP-запит або виклик сервісу (Act) і перевіряє статус, тіло відповіді та/або стан БД (Assert).

## Самоперевірка
1. Юніт-тест перевіряє невелику одиницю логіки із заміненими залежностями.
2. Інтеграційний тест перевіряє взаємодію HTTP, middleware, контролера, сервісу, EF Core і БД.
3. Mock задає поведінку залежності, але не перевіряє справжню реалізацію цієї залежності.
4. `WebApplicationFactory<Program>` запускає застосунок через `TestServer`, тому окремий `dotnet run` не потрібен.
5. `ICollectionFixture` дозволяє повторно використати контейнер між тестовими класами.
6. БД очищується перед кожним сценарієм, щоб тести не залежали від порядку запуску.
7. EF Core InMemory не перевіряє реальну поведінку PostgreSQL і SQL-перетворення.
8. Покриття показує виконані рядки або гілки, але не гарантує якість перевірок.

## Примітка
Покриття коду генерується локально у `TestResults/` і не додається до Git. Фактичний відсоток залежить від запуску тестів та версій інструментів; його не слід вважати підтвердженим до локального запуску.
