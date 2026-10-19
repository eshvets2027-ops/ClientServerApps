# Практична робота №3 — EventHub

Тема: зв’язки, N+1, DTO, пагінація, фільтрування та сортування.

## Склад проєкту

- `EventHub.Api` — REST API та ендпоінти подій і реєстрацій.
- `EventHub.Application` — DTO, параметри запиту та модель результату пагінації.
- `EventHub.Domain` — сутності `Event`, `Category`, `Registration` та статус реєстрації.
- `EventHub.Infrastructure` — EF Core, PostgreSQL, конфігурації, репозиторії та міграції.

## Запуск

Потрібні .NET 10 SDK і Docker Desktop.

```bash
docker compose up -d
dotnet restore
dotnet build
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.Api --context AppDbContext
dotnet run --project EventHub.Api
```

Після запуску Swagger доступний за адресою, вказаною в консолі, зазвичай `/swagger` на локальному порту API.

## Основні ендпоінти

- `GET /events?page=1&pageSize=2`
- `GET /events?categoryId=1`
- `GET /events?search=workshop`
- `GET /events?location=Kyiv`
- `GET /events?dateFrom=2027-01-01&dateTo=2027-12-31`
- `GET /events?sortBy=date&order=desc`
- `GET /events/{id}`
- `POST /events/{eventId}/registrations`
- `GET /events/{eventId}/registrations`

Параметри списку: `page`, `pageSize` (1–100), `categoryId`, `dateFrom`, `dateTo`, `search`, `location`, `sortBy` (`title`, `date`, `location`) і `order` (`asc`, `desc`). Фільтри й сортування додаються до `IQueryable` перед виконанням SQL. Порядок сортування доповнюється унікальним `Id`, а `TotalCount` обчислюється після фільтрів та до пагінації.

## Реєстрація на подію

Приклад тіла `POST /events/1/registrations`:

```json
{
  "attendeeName": "Іван Петренко",
  "attendeeEmail": "ivan@example.com"
}
```

Статуси: `Pending`, `Confirmed`, `Cancelled`. Реєстрація має зовнішній ключ `EventId`, індекс за ним і каскадне видалення разом із подією. Реєстрації у детальній відповіді сортуються за часом створення у спадному порядку. Подія без реєстрацій повертає порожній масив `registrations` та `RegistrationCount: 0`.

## Перевірка N+1

Антиприклад для навчального експерименту — спочатку завантажити всі події, а потім виконувати `CountAsync` для кожної події в циклі. Для N подій це дає один запит списку та N запитів підрахунку. Увімкніть SQL-логування в середовищі розробки й перевірте кількість команд. Цей варіант не використовується у фінальному ендпоінті.

Фінальна реалізація проєктує `Category.Name` і `Registrations.Count` безпосередньо у DTO на боці PostgreSQL. Для списку виконується запит підрахунку результатів і запит даних сторінки; кількість запитів не збільшується пропорційно кількості подій. `AsNoTracking()` застосовується до read-only запитів. Lazy-loading proxies не ввімкнено у фінальній конфігурації.

## Перевірки

1. Створити кілька категорій та щонайменше п’ять подій.
2. Додати до різних подій різну кількість реєстрацій.
3. Перевірити `GET /events?page=1&pageSize=2` і переконатися, що `Items` містить не більше двох елементів.
4. Перевірити фільтри за категорією, назвою, місцем і датами.
5. Перевірити сортування `sortBy=date&order=desc`.
6. Перевірити `GET /events/1` та `GET /events/1/registrations`.
7. Переконатися, що список містить `RegistrationCount`, але не містить вкладену колекцію повних реєстрацій.
8. Перевірити, що SQL-команди не виконуються окремо для кожної події у списку.

## Питання для самоперевірки

1. На якій стороні зв’язку зберігається зовнішній ключ?
2. Чим відрізняються eager, explicit і lazy loading?
3. Як проєкція в DTO допомагає уникати N+1?
4. Як за SQL-логами виявити N+1?
5. Коли використовують `AsNoTracking()`?
6. Чим `IQueryable<T>` відрізняється від `IEnumerable<T>`?
7. Чому `TotalCount` обчислюється до `Skip` і `Take`?
8. Навіщо додавати `ThenBy(Id)` для стабільної пагінації?

## Самостійне завдання

Реалізовано фільтр за `Location`. Він обмежує довжину параметра, виконується на стороні бази даних і повертає DTO у пагінованій відповіді.


## Додатковий навчальний експеримент із lazy loading

У фінальній конфігурації lazy loading вимкнено. Для окремого експерименту можна тимчасово встановити пакет `Microsoft.EntityFrameworkCore.Proxies` у `EventHub.Infrastructure`, додати `.UseLazyLoadingProxies()` перед `.UseNpgsql(connectionString)` у конфігурації `AppDbContext`, прибрати `sealed` у класах `Event`, `Registration` і `Category` та позначити навігаційні властивості як `virtual`. Після цього завантажте список подій без `Include`, зверніться до `Registrations.Count` у циклі та перевірте тип через `GetType().FullName` і кількість SQL-команд. Після перевірки поверніть фінальну конфігурацію без `UseLazyLoadingProxies()`.
