# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:9b4b31da5246f575086b1901e9871b189ae2a80eb42fe9234e9d000b51febd4b AS build
WORKDIR /app

# Install Node.js and npm
RUN apk add --no-cache nodejs npm

# Copy only project files first for better layer caching
COPY CoreBackend/*.csproj ./CoreBackend/
COPY Core/*.csproj ./Core/
COPY CoreWeb/*.csproj ./CoreWeb/
COPY Web/*.csproj ./Web/
COPY SharedAssemblyInfo.props .
RUN dotnet restore Web/Web.csproj

# Install Node packages
WORKDIR /app/CoreWeb
COPY CoreWeb/package*.json ./
RUN npm ci

WORKDIR /app/Web
COPY Web/package*.json ./
RUN npm ci

# Copy the remaining source code
WORKDIR /app
COPY CoreBackend/. ./CoreBackend/
COPY Core/. ./Core/
COPY CoreWeb/. ./CoreWeb/
COPY Web/. ./Web/
RUN mkdir -p ./DatabaseScripts/ChangeScripts
COPY DatabaseScripts/ChangeScripts/. ./DatabaseScripts/ChangeScripts/

# Build front-end assets
WORKDIR /app/Web
RUN npx tsc --noEmit && node esbuild.config.js
WORKDIR /app

# ******************************* Make sure the above is the same as for Dockerfile.tests to be able to benefit from cached layers *******************************

# Back to app root to publish .NET
RUN dotnet publish Web/Web.csproj -c Release -o Web/publish


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:258b939d6d684ff05ad7ea16782b4bee55260de4acc6f99bec897fd11de7640c

# Install Alpine-native Chromium and Node.js (these are compiled for Apline's musl); .NET localization requires icu-libs
RUN apk add --no-cache icu-libs chromium nss freetype harfbuzz ca-certificates ttf-freefont nodejs xvfb dbus mesa-gl

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