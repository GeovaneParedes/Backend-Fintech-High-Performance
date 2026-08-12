FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FintechCore.Api/FintechCore.Api.csproj", "src/FintechCore.Api/"]
RUN dotnet restore "src/FintechCore.Api/FintechCore.Api.csproj"
COPY . .
WORKDIR "/src/src/FintechCore.Api"
RUN dotnet build "FintechCore.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FintechCore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FintechCore.Api.dll"]
