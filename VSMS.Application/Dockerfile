FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8180
EXPOSE 8181

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VSMS.Application/VSMS.Application.csproj", "VSMS.Application/"]
COPY ["VSMS.Infrastructure/VSMS.Infrastructure.csproj", "VSMS.Infrastructure/"]
COPY ["VSMS.Domain/VSMS.Domain.csproj", "VSMS.Domain/"]
RUN dotnet restore "VSMS.Application/VSMS.Application.csproj"
COPY . .
WORKDIR "/src/VSMS.Application"
RUN dotnet build "./VSMS.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VSMS.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VSMS.Application.dll"]
