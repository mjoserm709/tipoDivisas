# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Instala Playwright y navegadores en etapa de build
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"
RUN playwright install --with-deps

# Etapa de ejecución (solo runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /root/.cache/ms-playwright/ /root/.cache/ms-playwright/ 

ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ApiTipoCambio.dll"]
