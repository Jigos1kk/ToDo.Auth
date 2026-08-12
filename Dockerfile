FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ToDo.Auth.slnx ./
COPY src/ToDo.Auth.Api/ToDo.Auth.Api.csproj       src/ToDo.Auth.Api/
COPY src/ToDo.Auth.Business/ToDo.Auth.Business.csproj src/ToDo.Auth.Business/
COPY src/ToDo.Auth.Data/ToDo.Auth.Data.csproj       src/ToDo.Auth.Data/
RUN dotnet restore src/ToDo.Auth.Api/ToDo.Auth.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/ToDo.Auth.Api/ToDo.Auth.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

VOLUME ["/data"]

EXPOSE 80

USER app
ENTRYPOINT ["dotnet", "ToDo.Auth.Api.dll"]