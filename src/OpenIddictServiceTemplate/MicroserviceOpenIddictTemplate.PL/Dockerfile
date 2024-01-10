FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 10000
EXPOSE 10001

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MicroserviceOpenIddictTemplate.PL/MicroserviceOpenIddictTemplate.PL.csproj", "MicroserviceOpenIddictTemplate.PL/"]
COPY ["MicroserviceOpenIddictTemplate.BL/MicroserviceOpenIddictTemplate.BL.csproj", "MicroserviceOpenIddictTemplate.BL/"]
COPY ["MicroserviceOpenIddictTemplate.DAL/MicroserviceOpenIddictTemplate.DAL.csproj", "MicroserviceOpenIddictTemplate.DAL/"]
RUN dotnet restore "MicroserviceOpenIddictTemplate.PL/MicroserviceOpenIddictTemplate.PL.csproj"
COPY . .
WORKDIR "/src/MicroserviceOpenIddictTemplate.PL"
RUN dotnet build "MicroserviceOpenIddictTemplate.PL.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MicroserviceOpenIddictTemplate.PL.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=https://+:10001;http://+:10000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroserviceOpenIddictTemplate.PL.dll"]