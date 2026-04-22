# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:732cd42c6f659814c9804ad7b05c7f761e83ef8379c5b2fdc3af673353caff73 AS build
WORKDIR /app

# Install Node.js and npm
RUN apk add --no-cache nodejs npm

# Copy only project files first for better layer caching
COPY Libraries/CoreBackend/CoreBackend/*.csproj ./Libraries/CoreBackend/CoreBackend/
COPY Libraries/CoreBackend/CoreBackend.Register.Castle/*.csproj ./Libraries/CoreBackend/CoreBackend.Register.Castle/
COPY Libraries/CoreBackend/CoreBackend.Register.ServiceProvider/*.csproj ./Libraries/CoreBackend/CoreBackend.Register.ServiceProvider/
COPY Libraries/CoreBackend/CoreBackend.Account/*.csproj ./Libraries/CoreBackend/CoreBackend.Account/
COPY Libraries/CoreBackend/CoreBackend.Account.Register.Castle/*.csproj ./Libraries/CoreBackend/CoreBackend.Account.Register.Castle/
COPY Libraries/CoreBackend/CoreBackend.Account.Register.ServiceProvider/*.csproj ./Libraries/CoreBackend/CoreBackend.Account.Register.ServiceProvider/
COPY Libraries/CoreBackend/CoreBackend.Postgres/*.csproj ./Libraries/CoreBackend/CoreBackend.Postgres/
COPY Core/*.csproj ./Core/
COPY Libraries/CoreWeb/CoreWeb/*.csproj ./Libraries/CoreWeb/CoreWeb/
COPY Libraries/CoreWeb/CoreWeb.Account/*.csproj ./Libraries/CoreWeb/CoreWeb.Account/
COPY Web/*.csproj ./Web/
COPY SharedAssemblyInfo.props .
RUN dotnet restore Web/Web.csproj

# Copy the remaining source code
WORKDIR /app
COPY Libraries/CoreBackend/CoreBackend/. ./Libraries/CoreBackend/CoreBackend/
COPY Libraries/CoreBackend/CoreBackend.Register.Castle/. ./Libraries/CoreBackend/CoreBackend.Register.Castle/
COPY Libraries/CoreBackend/CoreBackend.Register.ServiceProvider/. ./Libraries/CoreBackend/CoreBackend.Register.ServiceProvider/
COPY Libraries/CoreBackend/CoreBackend.Account/. ./Libraries/CoreBackend/CoreBackend.Account/
COPY Libraries/CoreBackend/CoreBackend.Account.Register.Castle/. ./Libraries/CoreBackend/CoreBackend.Account.Register.Castle/
COPY Libraries/CoreBackend/CoreBackend.Account.Register.ServiceProvider/. ./Libraries/CoreBackend/CoreBackend.Account.Register.ServiceProvider/
COPY Libraries/CoreBackend/CoreBackend.Postgres/. ./Libraries/CoreBackend/CoreBackend.Postgres/
COPY Core/. ./Core/
COPY Libraries/CoreWeb/CoreWeb/. ./Libraries/CoreWeb/CoreWeb/
COPY Libraries/CoreWeb/CoreWeb.Account/. ./Libraries/CoreWeb/CoreWeb.Account/
COPY Web/. ./Web/
RUN mkdir -p ./DatabaseScripts/ChangeScripts
COPY DatabaseScripts/ChangeScripts/. ./DatabaseScripts/ChangeScripts/

# ******************************* Make sure the above is the same as for Dockerfile.tests to be able to benefit from cached layers *******************************

# Install Node packages
WORKDIR /app/Libraries/CoreWeb/CoreWeb
COPY Libraries/CoreWeb/CoreWeb/package*.json ./
RUN npm ci

WORKDIR /app/Libraries/CoreWeb/CoreWeb.Account
COPY Libraries/CoreWeb/CoreWeb.Account/package*.json ./
RUN npm ci

WORKDIR /app/Web
COPY Web/package*.json ./
RUN npm ci

# Build front-end assets
WORKDIR /app/Web
RUN npx tsc --noEmit && node esbuild.config.js
WORKDIR /app

# Back to app root to publish .NET
RUN dotnet publish Web/Web.csproj -c Release -o Web/publish


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:1201dde897ab436b7c6b386f6dbd4f9a3ca0245f9c5a8aac8f8bcdccb4c7d484

# Install Alpine-native Chromium and Node.js (these are compiled for Apline's musl); .NET localization requires icu-libs
RUN apk add --no-cache icu-libs krb5-libs chromium nss freetype harfbuzz ca-certificates ttf-freefont nodejs xvfb dbus mesa-gl

WORKDIR /app

# Tell Playwright to use system Node
ENV PLAYWRIGHT_NODEJS_PATH=/usr/bin/node
# This tells the .NET driver where the Alpine-native browser is
ENV PlaywrightScraper__BrowserExecutablePath=/usr/bin/chromium-browser

COPY --from=build /app/Web/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ARG GIT_SHA
ENV Runtime__AppGitShaVersion=$GIT_SHA

EXPOSE 80
# Start Xvfb in the background to support headed Chrome browser and then run the application
ENTRYPOINT ["/bin/sh", "-c", "Xvfb :99 -screen 0 1920x1080x24 & export DISPLAY=:99 && dotnet MrWatchdog.Web.dll"]
