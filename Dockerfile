FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Instala Playwright CLI y navegadores
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && playwright install

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ApiTipoCambio.dll"]
