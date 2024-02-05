FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 10000
EXPOSE 10001

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG build_mode
WORKDIR /src
COPY ["Pepegov.Identity.PL/Pepegov.Identity.PL.csproj", "Pepegov.Identity.PL/"]
COPY ["Pepegov.Identity.BL/Pepegov.Identity.BL.csproj", "Pepegov.Identity.BL/"]
COPY ["Pepegov.Identity.DAL/Pepegov.Identity.DAL.csproj", "Pepegov.Identity.DAL/"]
RUN dotnet restore "Pepegov.Identity.PL/Pepegov.Identity.PL.csproj"
COPY . .
WORKDIR "/src/Pepegov.Identity.PL"
RUN dotnet build "Pepegov.Identity.PL.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Pepegov.Identity.PL.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=https://+:10001;http://+:10000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pepegov.Identity.PL.dll"]