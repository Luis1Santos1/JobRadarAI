FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/JobRadar.Api/JobRadar.Api.csproj", "src/JobRadar.Api/"]
COPY ["src/JobRadar.Application/JobRadar.Application.csproj", "src/JobRadar.Application/"]
COPY ["src/JobRadar.Domain/JobRadar.Domain.csproj", "src/JobRadar.Domain/"]
COPY ["src/JobRadar.Infrastructure/JobRadar.Infrastructure.csproj", "src/JobRadar.Infrastructure/"]
COPY ["src/JobRadar.Contracts/JobRadar.Contracts.csproj", "src/JobRadar.Contracts/"]
COPY ["src/JobRadar.SharedKernel/JobRadar.SharedKernel.csproj", "src/JobRadar.SharedKernel/"]

RUN dotnet restore "src/JobRadar.Api/JobRadar.Api.csproj"

COPY . .

RUN dotnet publish "src/JobRadar.Api/JobRadar.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JobRadar.Api.dll"]
