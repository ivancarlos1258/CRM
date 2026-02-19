FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CRM.Server/CRM.Server.csproj", "CRM.Server/"]
COPY ["CRM.Application/CRM.Application.csproj", "CRM.Application/"]
COPY ["CRM.Domain/CRM.Domain.csproj", "CRM.Domain/"]
COPY ["CRM.Infrastructure/CRM.Infrastructure.csproj", "CRM.Infrastructure/"]

RUN dotnet restore "CRM.Server/CRM.Server.csproj"

COPY . .

WORKDIR "/src/CRM.Server"
RUN dotnet build "CRM.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRM.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM.Server.dll"]
