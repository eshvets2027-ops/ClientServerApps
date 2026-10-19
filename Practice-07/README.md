# Практична робота №7

**Тема:** Завантаження обкладинки події  
**Проєкт:** EventHub  
**Платформа:** .NET 10, ASP.NET Core Web API, EF Core, PostgreSQL

## Реалізовано

- Завантаження JPEG і PNG через `multipart/form-data` у полі `cover`.
- Перевірка порожнього файла, розміру до 5 МБ, MIME-типу та розширення.
- Генерація випадкового імені файла на сервері.
- Збереження імені файла в `Events.CoverImagePath`.
- Локальне сховище через `ICoverImageStorage`.
- Типізовані налаштування `FileStorageOptions` з перевіркою під час запуску.
- Компенсаційне видалення нового файла при помилці запису в БД.
- Видалення попередньої обкладинки після успішної заміни.
- Читання обкладинки з коректним `Content-Type`.
- Видалення обкладинки через `DELETE /events/{id}/cover`.
- Видалення обкладинки разом із подією.
- Problem Details для помилок.
- Модульні тести валідатора.

## Структура

- `EventHub.Domain`
- `EventHub.Application`
- `EventHub.Infrastructure`
- `EventHub.Api`
- `EventHub.Tests`

## Запуск PostgreSQL

```bash
docker compose up -d
```

## Збірка та тести

```bash
dotnet --version
dotnet build EventHub.sln
dotnet test EventHub.sln
```

## Міграції

Для нового середовища створіть міграцію початкової схеми:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
```

Якщо використовується база з попередніх практичних робіт, перенесіть `CoverImagePath` у нову міграцію `AddEventCoverImage`, не створюючи повторно наявні таблиці.

## Запуск API

```bash
dotnet run --project EventHub.Api --launch-profile http
```

Swagger UI: `http://localhost:5230/swagger`

## API

| Метод | Маршрут | Призначення |
|---|---|---|
| GET | `/events` | Список подій |
| GET | `/events/{id}` | Деталі події |
| POST | `/events` | Створення події |
| POST | `/events/{id}/cover` | Завантаження або заміна обкладинки |
| GET | `/events/{id}/cover` | Отримання обкладинки |
| DELETE | `/events/{id}/cover` | Видалення обкладинки |
| DELETE | `/events/{id}` | Видалення події |

## Завантаження файла

```powershell
curl.exe -i -X POST -F "cover=@C:\temp\event-cover.png;type=image/png" http://localhost:5230/events/1/cover
```

## Отримання файла

```powershell
curl.exe -i http://localhost:5230/events/1/cover
```

## Перевірка негативного сценарію

```powershell
curl.exe -i -X POST -F "cover=@C:\temp\notes.txt;type=text/plain" http://localhost:5230/events/1/cover
```

## Видалення обкладинки

```powershell
curl.exe -i -X DELETE http://localhost:5230/events/1/cover
```

Видалення обкладинки в наявної події без обкладинки є ідемпотентним і повертає `204 No Content`. Для невідомої події повертається `404 Not Found`.

## Налаштування сховища

У `EventHub.Api/appsettings.json`:

```json
"FileStorage": {
  "UploadPath": "wwwroot/uploads",
  "MaxFileSizeMb": 5
}
```

`UploadPath` можна змінювати для середовища. Відносний шлях має залишатися в межах каталогу застосунку. Каталог `wwwroot/uploads` виключено з Git.

## Важливо

Перевірка MIME-типу та розширення не доводить справжній формат байтів. Для промислового застосунку потрібно додати перевірку сигнатури або декодування зображення, обмеження розмірів у пікселях та за потреби антивірусний аналіз. Локальне сховище підходить для навчального середовища з одним екземпляром API; для кількох екземплярів потрібне спільне об'єктне сховище.
