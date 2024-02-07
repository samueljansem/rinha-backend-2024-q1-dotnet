FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

RUN apt-get update \
  && apt-get install -y clang zlib1g-dev \
  && rm -rf /var/lib/apt/lists/*

COPY . ./

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["./RinhaBackend"]