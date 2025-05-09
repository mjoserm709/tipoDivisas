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

# Etapa de ejecución con librerías necesarias para Chromium
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Instalar librerías necesarias para Playwright y Chromium
RUN apt-get update && apt-get install -y \
    libglib2.0-0 \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libasound2 \
    libxshmfence1 \
    libgtk-3-0 \
    libx11-xcb1 \
    libx11-6 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /root/.cache/ms-playwright/ /root/.cache/ms-playwright/

ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ApiTipoCambio.dll"]
