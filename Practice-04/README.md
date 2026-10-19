# Практична робота №4

Тема: Валідація даних, глобальний обробник помилок.

Проєкт: EventHub

## Запуск

Потрібен .NET 10 SDK.

```bash
dotnet run --project EventHub.Api
```

## Основні маршрути

- `GET /events`
- `GET /events/{id}`
- `POST /events`
- `PUT /events/{id}`
- `DELETE /events/{id}`
- `POST /events/{id}/registrations`
- `GET /categories`
- `POST /categories`

Помилки повертаються у форматі Problem Details. Для реєстрації використано перевірку дати події, дублювання email і місткості.
