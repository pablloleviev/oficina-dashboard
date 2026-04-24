# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o csproj usando o caminho relativo à raiz do repositório
COPY ["AutoFlow.API/AutoFlow.API.csproj", "AutoFlow.API/"]
RUN dotnet restore "AutoFlow.API/AutoFlow.API.csproj"

# Copia o restante dos arquivos da pasta backend
COPY ["AutoFlow.API/", "AutoFlow.API/"]

# Muda para a pasta do projeto para buildar
WORKDIR "/src/AutoFlow.API"
RUN dotnet publish "AutoFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8/9 usa a porta 8080 por padrão em containers
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoFlow.API.dll"]
