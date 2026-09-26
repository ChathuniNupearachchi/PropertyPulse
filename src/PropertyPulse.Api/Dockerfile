FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first so the restore layer is cached until a dependency changes.
COPY src/PropertyPulse.Domain/PropertyPulse.Domain.csproj src/PropertyPulse.Domain/
COPY src/PropertyPulse.Infrastructure/PropertyPulse.Infrastructure.csproj src/PropertyPulse.Infrastructure/
COPY src/PropertyPulse.Shared/PropertyPulse.Shared.csproj src/PropertyPulse.Shared/
COPY src/PropertyPulse.Api/PropertyPulse.Api.csproj src/PropertyPulse.Api/
RUN dotnet restore src/PropertyPulse.Api/PropertyPulse.Api.csproj

COPY src/PropertyPulse.Domain/ src/PropertyPulse.Domain/
COPY src/PropertyPulse.Infrastructure/ src/PropertyPulse.Infrastructure/
COPY src/PropertyPulse.Shared/ src/PropertyPulse.Shared/
COPY src/PropertyPulse.Api/ src/PropertyPulse.Api/
RUN dotnet publish src/PropertyPulse.Api/PropertyPulse.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PropertyPulse.Api.dll"]
