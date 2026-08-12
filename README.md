# ToDo.Auth — сервис авторизации и ролей

## Назначение

Микросервис отвечает за регистрацию, аутентификацию, управление ролями пользователей
и выдачу JWT-токенов.

### Роли

- **User** — базовая роль, присваивается всем
- **Customer** — заказчик: создаёт проекты, управляет заявками
- **Freelancer** — фрилансер: подаёт заявки на проекты, выполняет задачи
- **Admin** — администратор: полный доступ ко всем проектам

При первом запуске автоматически создаётся администратор (настройки в `Admin` секции).

## API

| Метод | Путь | Описание |
| ----- | ---- | -------- |
| POST | `/api/auth/register` | Регистрация (Customer/Freelancer) |
| POST | `/api/auth/confirm-email` | Подтверждение email по токену |
| POST | `/api/auth/login` | Вход → JWT-токены |
| POST | `/api/auth/refresh` | Обновление пары токенов |
| GET | `/api/auth/me` | Профиль текущего пользователя |
| POST | `/api/auth/change-password` | Смена пароля (требует текущий) |
| POST | `/api/auth/forgot-password` | Запрос токена восстановления |
| POST | `/api/auth/reset-password` | Сброс пароля по токену |

## Запуск локально

```bash
cd ToDo.Auth
dotnet run --project src/ToDo.Auth.Api
# → http://localhost:5057
```

### Переменные окружения

| Переменная | По умолчанию | Описание |
| ---------- | ------------ | -------- |
| `Jwt__SecretKey` | значение из `appsettings.json` | Ключ подписи JWT (≥ 32 символа) |
| `Jwt__Issuer` | `ToDo.Auth` | Издатель токенов |
| `Jwt__Audience` | `ToDo.Platform` | Получатель токенов |
| `Admin__Password` | значение из `appsettings.json` | Пароль администратора по умолчанию |
| `ConnectionStrings__DefaultConnection` | `Data Source=auth.db` | Строка подключения SQLite |

## Docker

```bash
# Сборка
docker build -t todo-auth .

# Запуск
docker run -p 5057:80 \
  -e Jwt__SecretKey=ваш-секретный-ключ-минимум-32-символа \
  -e Admin__Password=безопасный-пароль \
  -v ./data:/data \
  -e ConnectionStrings__DefaultConnection="Data Source=/data/auth.db" \
  todo-auth
```

## Стек

- **.NET 10** (ASP.NET Core Web API)
- **SQLite** (Entity Framework Core)
- **JWT Bearer** аутентификация
- **Rate Limiting** на чувствительных endpoints