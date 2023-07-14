# Web Notes Application Web API
Backend часть для приложения по организации личных дел.

+ .NET 6.0 Framework <br />
+ PostgreSQL
+ Entity Framework
+ MimeKit

Для корректной сборки проекта в корневом каталоге проекта необходимо добавить следущие файлы файлы с указанным содержимым:
+ appsettings.json - конфигурация приложения
+ DBConfig.json - строка подключения к СУБД
+ client_secret.json - файл с информацией для OAuth2

### appsettings.json
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "HostEmail": "Ваша почта здесь",
  "HostPassword": "Пароль от почты здесь",
  "FilesStorageFolder":  "Files"
  "FrontRedirectUrl": "URL-редиректа",
  "NotesPerUser":  10
}
```


### DBConfig.json
```
{
  "ConnectionStrings": {
    "PostgreSQLConnection": "Строка подклбючения к вашей БД"
  }
}
```
