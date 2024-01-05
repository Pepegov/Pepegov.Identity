FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 10000
EXPOSE 10001

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroserviceOpenIddictTemplate.PL.Api/MicroserviceOpenIddictTemplate.PL.Api.csproj", "MicroserviceOpenIddictTemplate.PL.Api/"]
COPY ["MicroserviceOpenIddictTemplate.BL/MicroserviceOpenIddictTemplate.BL.csproj", "MicroserviceOpenIddictTemplate.BL/"]
COPY ["MicroserviceOpenIddictTemplate.DAL/MicroserviceOpenIddictTemplate.DAL.csproj", "MicroserviceOpenIddictTemplate.DAL/"]
RUN dotnet restore "MicroserviceOpenIddictTemplate.PL.Api/MicroserviceOpenIddictTemplate.PL.Api.csproj"
COPY . .
WORKDIR "/src/MicroserviceOpenIddictTemplate.PL.Api"
RUN dotnet build "MicroserviceOpenIddictTemplate.PL.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroserviceOpenIddictTemplate.PL.Api.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_URLS=https://+:10001;http://+:10000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroserviceOpenIddictTemplate.PL.Api.dll"]
