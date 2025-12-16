# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

# Install Node.js and npm
RUN apk add --no-cache nodejs npm

# Copy only project files first for better layer caching
COPY Core/*.csproj ./Core/
COPY Web/*.csproj ./Web/
COPY SharedAssemblyInfo.props .
RUN dotnet restore Web/Web.csproj

# Install Node packages
WORKDIR /app/Web
COPY Web/package*.json ./
RUN npm ci

# Copy the remaining source code
WORKDIR /app
COPY Core/. ./Core/
COPY Web/. ./Web/
RUN mkdir -p ./DatabaseScripts/ChangeScripts
COPY DatabaseScripts/ChangeScripts/. ./DatabaseScripts/ChangeScripts/

# Build front-end assets
WORKDIR /app/Web
RUN npx tsc --noEmit && node esbuild.config.js
WORKDIR /app

# Make sure the above is the same as for ./docker-ci/Dockerfile.tests to be able to benefit from cached layers

# Back to app root to publish .NET
RUN dotnet publish Web/Web.csproj -c Release -o Web/publish


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
RUN apk add --no-cache icu-libs # .NET localization requires icu-libs
COPY --from=build /app/Web/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 80
ENTRYPOINT ["dotnet", "MrWatchdog.Web.dll"]