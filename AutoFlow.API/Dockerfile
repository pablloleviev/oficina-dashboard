# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# No contexto da subpasta AutoFlow.API, o csproj está na raiz (.)
COPY ["AutoFlow.API.csproj", "./"]
RUN dotnet restore "AutoFlow.API.csproj"

# Copia o restante dos arquivos (dentro da subpasta)
COPY . .

# Build e Publish
RUN dotnet publish "AutoFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8/9 usa a porta 8080 por padrão
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoFlow.API.dll"]
