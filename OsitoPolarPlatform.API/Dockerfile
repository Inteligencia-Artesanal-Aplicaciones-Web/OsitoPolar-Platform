# Imagen base oficial de .NET para apps ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar solución y restaurar dependencias
COPY ["OsitoPolarPlatform.API/OsitoPolarPlatform.API.csproj", "OsitoPolarPlatform.API/"]
RUN dotnet restore "OsitoPolarPlatform.API/OsitoPolarPlatform.API.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/OsitoPolarPlatform.API"
RUN dotnet build "OsitoPolarPlatform.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OsitoPolarPlatform.API.csproj" -c Release -o /app/publish

# Crear imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OsitoPolarPlatform.API.dll"]
