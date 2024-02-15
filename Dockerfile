FROM mcr.microsoft.com/dotnet/sdk:8.0.200-alpine3.19 AS build-env

WORKDIR /app

RUN apk add --no-cache clang build-base zlib-dev

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0.2-alpine3.19
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["./RinhaBackend"]
