# Web Notes Application Web API

+ .NET 6.0 Framework <br />
+ PostgreSQL

Для корректной сборки проекта в корневом каталоге проекта необходимо создать файлы с указанным содержимым:
-appsettings.json
-DBConfig.json

appsettings.json
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
}

DBConfig.json
{
  "ConnectionStrings": {
    "PostgreSQLConnection": "Строка подклбючения к вашей БД"
  }
}
