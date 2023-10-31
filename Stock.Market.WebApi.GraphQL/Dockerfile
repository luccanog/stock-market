#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Stock.Market.WebApi.GraphQL/Stock.Market.WebApi.GraphQL.csproj", "Stock.Market.WebApi.GraphQL/"]
RUN dotnet restore "Stock.Market.WebApi.GraphQL/Stock.Market.WebApi.GraphQL.csproj"
COPY . .
WORKDIR "/src/Stock.Market.WebApi.GraphQL"
RUN dotnet build "Stock.Market.WebApi.GraphQL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stock.Market.WebApi.GraphQL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stock.Market.WebApi.GraphQL.dll"]