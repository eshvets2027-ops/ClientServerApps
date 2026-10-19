# EventHub API

REST API для керування подіями та категоріями на ASP.NET Core 10, Entity Framework Core 10 і PostgreSQL.

## Технології

- C# та .NET 10
- ASP.NET Core Minimal API
- PostgreSQL 16
- Entity Framework Core 10
- Npgsql
- Swagger / OpenAPI
- Docker Compose

## Структура

- `EventHub.Api` — HTTP API, DTO та перевірка запитів.
- `EventHub.Domain` — сутності та контракти репозиторіїв.
- `EventHub.Infrastructure` — `DbContext`, Fluent API, міграції та репозиторії.

## Запуск

Потрібні .NET 10 SDK та Docker Desktop.

```bash
docker compose up -d
dotnet tool install --global dotnet-ef
dotnet restore
dotnet build
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
dotnet run --project EventHub.Api
```

Після запуску відкрий URL, який виведе застосунок, і додай `/swagger`.

## Міграції

У проєкті є дві міграції: `InitialCreate` створює таблиці `Categories` і `Events`, а `AddOrganizerToEvent` додає обов'язкове поле `Organizer` довжиною до 120 символів.

Створити SQL-скрипт міграцій:

```bash
dotnet ef migrations script --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

Переглянути застосовані міграції:

```bash
dotnet ef migrations list --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

## API

| Метод | Маршрут | Призначення |
|---|---|---|
| GET | `/events` | Отримати всі події |
| GET | `/events/{id}` | Отримати подію за ID |
| POST | `/events` | Створити подію |
| PUT | `/events/{id}` | Оновити подію |
| DELETE | `/events/{id}` | Видалити подію |
| GET | `/categories` | Отримати всі категорії |
| GET | `/categories/{id}` | Отримати категорію за ID |
| POST | `/categories` | Створити категорію |

## Приклади запитів

Створення категорії через `POST /categories`:

```json
{
  "name": "Навчання"
}
```

Створення події через `POST /events`:

```json
{
  "title": "Конференція з програмування",
  "description": "Зустріч розробників",
  "location": "Кременчук",
  "organizer": "Студентська рада",
  "date": "2026-11-20T10:00:00+02:00",
  "capacity": 100,
  "categoryId": 1
}
```

Для створення події спочатку створи категорію та використай її `id` у полі `categoryId`.

## База даних

Для локального навчального середовища використовується база `eventhub_dev` з користувачем `postgres`. Не використовуй навчальний пароль у виробничому середовищі та не зберігай справжні секрети в Git.
