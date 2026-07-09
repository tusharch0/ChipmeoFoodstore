FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FoodstoreApi.Core/*.csproj ./FoodstoreApi.Core/
COPY FoodstoreApi.Usecase/*.csproj ./FoodstoreApi.Usecase/
COPY FoodstoreApi.Infrastructure/*.csproj ./FoodstoreApi.Infrastructure/
COPY FoodstoreApi.Web/*.csproj ./FoodstoreApi.Web/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore FoodstoreApi.Web/FoodstoreApi.Web.csproj

COPY . .

RUN dotnet publish FoodstoreApi.Web/FoodstoreApi.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
RUN groupadd --system --gid 1001 foodstore && useradd --system --uid 1001 --gid foodstore foodstore
USER foodstore
EXPOSE 8080
ENTRYPOINT ["dotnet", "FoodstoreApi.Web.dll"]
