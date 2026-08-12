FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FakeTef.Api/FakeTef.Api.csproj", "src/FakeTef.Api/"]
RUN dotnet restore "src/FakeTef.Api/FakeTef.Api.csproj"
COPY . .
WORKDIR "/src/src/FakeTef.Api"
RUN dotnet build "FakeTef.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FakeTef.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeTef.Api.dll"]
