# Практична робота №5

**Тема:** OpenAPI та інтерактивна документація HTTP API.

**Проєкт:** EventHub

## Вимоги

- .NET 10 SDK
- Visual Studio 2022 або VS Code

## Запуск

```bash
dotnet restore
dotnet run --project EventHub.Api
```

Swagger UI та JSON OpenAPI доступні лише в середовищі `Development`.

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project EventHub.Api
```

- Swagger UI: `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`

У Windows PowerShell перед запуском можна виконати `$env:ASPNETCORE_ENVIRONMENT="Development"`.

## Ендпоїнти

| Метод | Маршрут | Призначення | Успішний статус |
|---|---|---|---|
| GET | `/events` | Отримати події з фільтрами та пагінацією | 200 |
| GET | `/events/{id}` | Отримати подію | 200 |
| POST | `/events` | Створити подію | 201 |
| PUT | `/events/{id}` | Оновити подію | 200 |
| DELETE | `/events/{id}` | Видалити подію | 204 |
| GET | `/events/{id}/registrations` | Отримати реєстрації | 200 |
| POST | `/events/{id}/registrations` | Зареєструвати учасника | 201 |
| DELETE | `/events/{id}/registrations?email=name@example.com` | Скасувати реєстрацію | 204 |
| GET | `/categories` | Отримати категорії | 200 |
| POST | `/categories` | Створити категорію | 201 |

## Валідація та відповіді

DTO використовують атрибути Data Annotations. Для подій перевіряються назва, місце, дата, місткість і категорія; для реєстрації — формат email. Некоректні дані повертають `400`, відсутній ресурс — `404`, повторна реєстрація — `409`. Помилки подаються у форматі Problem Details.

## Перевірка Swagger UI

1. Запустити застосунок у середовищі `Development`.
2. Відкрити `/swagger`.
3. Розгорнути операцію, натиснути **Try it out**, заповнити параметри та виконати запит.
4. Переглянути код відповіді, заголовки та тіло відповіді.
5. Відкрити `/swagger/v1/swagger.json` і перевірити OpenAPI-опис.
6. Запустити застосунок у середовищі `Production` і перевірити, що Swagger UI та Swagger JSON не публікуються.

## XML-документація

XML-документація генерується для `EventHub.Api` та `EventHub.Application` і підключається до Swashbuckle, якщо відповідні XML-файли наявні. Контролери описують параметри та коди відповідей, а DTO — поля й обмеження валідації.

## Сумісність

Збережено основні маршрути подій і категорій попередньої версії. Для реєстрацій додано отримання списку та скасування реєстрації. Swagger UI доступний лише в `Development`; у `Production` документація не відкривається. Дані зберігаються в пам’яті та очищаються після перезапуску.
