# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Копируем опубликованные файлы в контейнер
COPY ./publish/ ./

# Устанавливаем точку входа
ENTRYPOINT ["dotnet", "AspTelegramBot.dll"]