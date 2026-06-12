FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Domain/WedNest.Domain.csproj src/Domain/
COPY src/Application/WedNest.Application.csproj src/Application/
COPY src/Infrastructure/WedNest.Infrastructure.csproj src/Infrastructure/
COPY src/API/WedNest.API.csproj src/API/
RUN dotnet restore src/API/WedNest.API.csproj

COPY src/ src/
RUN dotnet publish src/API/WedNest.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "WedNest.API.dll"]
