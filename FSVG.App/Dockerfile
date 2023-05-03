FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FSVG.App/FSVG.App.fsproj", "FSVG.App/"]
RUN dotnet restore "FSVG.App/FSVG.App.fsproj"
COPY . .
WORKDIR "/src/FSVG.App"
RUN dotnet build "FSVG.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FSVG.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FSVG.App.dll"]
