# Практична робота №8

**Тема:** Конфігурація та середовища  
**Проєкт:** EventHub  
**Платформа:** .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL

## Реалізовано

- Рядок підключення відсутній у файлах `appsettings*.json`.
- Додано `UserSecretsId` для локальної розробки.
- Додано `DatabaseOptions` із перевіркою через `ValidateOnStart()`.
- `AppDbContext` отримує рядок підключення через `IOptions<DatabaseOptions>`.
- Додано конфігурацію Development, Staging і Production.
- Максимальний розмір обкладинки: 10, 7 і 5 МБ відповідно.
- Додано `OpenApiOptions` із перевіркою обов'язкових `Title` і `Version`.
- `AddSwaggerGen` отримує метадані через `OpenApiOptions`.
- Swagger UI доступний лише в Development.
- Додано автоматизовані тести для перевірки параметрів OpenAPI.

## Вимоги

- .NET 10 SDK
- PowerShell
- Docker Desktop або локальний PostgreSQL 16

## Підготовка PostgreSQL

У корені проєкту виконайте:

```powershell
$env:POSTGRES_PASSWORD = "<ваш-пароль>"
docker compose up -d
```

## Локальний секрет

У корені проєкту виконайте:

```powershell
dotnet user-secrets init --project EventHub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventhub_dev;Username=postgres;Password=<ваш-пароль>" --project EventHub.Api
```

Замініть `<ваш-пароль>` на пароль локального PostgreSQL. Не додавайте пароль до Git.

## Перевірка

```powershell
dotnet --version
dotnet build EventHub.sln
dotnet test EventHub.sln
dotnet format EventHub.sln --verify-no-changes
```

## Запуск API

```powershell
dotnet run --project EventHub.Api --launch-profile http
```

Адреси:

- Swagger UI: http://localhost:5230/swagger
- Перевірка стану: http://localhost:5230/health
- Події: http://localhost:5230/events

## Перевірка конфігурації середовищ

```powershell
$environments = "Development", "Staging", "Production"
foreach ($environment in $environments) {
    $path = "EventHub.Api/appsettings.$environment.json"
    $settings = Get-Content $path | ConvertFrom-Json
    "$environment`: $($settings.FileStorage.MaxFileSizeMb) MB"
}
```

Очікуваний результат:

```text
Development: 10 MB
Staging: 7 MB
Production: 5 MB
```

Перевірка пріоритету змінної середовища:

```powershell
$env:OpenApi__Title = "EventHub API Local"
dotnet run --project EventHub.Api --launch-profile http
Remove-Item Env:OpenApi__Title
```

Перевірка відсутнього рядка підключення в Production:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
dotnet run --project EventHub.Api --no-launch-profile
Remove-Item Env:ASPNETCORE_ENVIRONMENT
```

Застосунок має завершити запуск із помилкою валідації `ConnectionStrings:DefaultConnection`.

## Питання для самоперевірки

1. Чим конфігурація відрізняється від секрету?
2. Чому пароль не можна зберігати в Git?
3. Які джерела конфігурації використовує ASP.NET Core?
4. Чому змінні середовища можуть замінити значення з JSON?
5. Чим Development відрізняється від Staging і Production?
6. Де зберігаються .NET user secrets?
7. Чому user secrets не призначені для робочого сервера?
8. Для чого потрібен `DatabaseOptions`?
9. Що перевіряє `ValidateOnStart()`?
10. Для чого потрібен `OpenApiOptions`?
11. Чому Swagger UI має бути доступний лише в Development?
12. Які способи зберігання секретів можна застосувати після розгортання?

## Підсумковий контрольний список

- [ ] Рядок підключення відсутній у `appsettings*.json`.
- [ ] API-проєкт має `UserSecretsId`.
- [ ] Рядок підключення збережено через .NET user secrets.
- [ ] `DatabaseOptions` перевіряється під час запуску.
- [ ] `AppDbContext` використовує `IOptions<DatabaseOptions>`.
- [ ] Для Development, Staging і Production задано межі 10, 7 і 5 МБ.
- [ ] `OpenApiOptions` перевіряє `Title` і `Version`.
- [ ] Змінна `OpenApi__Title` може замінити значення JSON.
- [ ] Swagger UI доступний лише в Development.
- [ ] Автоматизовані тести перевіряють коректні й некоректні OpenAPI-параметри.
- [ ] Усі тести проходять.
