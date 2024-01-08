FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroserviceOpenIddictTemplate.PL/MicroserviceOpenIddictTemplate.PL.csproj", "MicroserviceOpenIddictTemplate.PL/"]
RUN dotnet restore "MicroserviceOpenIddictTemplate.PL/MicroserviceOpenIddictTemplate.PL.csproj"
COPY . .
WORKDIR "/src/MicroserviceOpenIddictTemplate.PL"
RUN dotnet build "MicroserviceOpenIddictTemplate.PL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroserviceOpenIddictTemplate.PL.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroserviceOpenIddictTemplate.PL.dll"]
