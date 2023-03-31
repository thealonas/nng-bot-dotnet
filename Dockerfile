FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 1220

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["nng-bot.csproj", "./"]
RUN dotnet restore "nng-bot.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "nng-bot.csproj" -c Release -o /app/build --no-self-contained

FROM build AS publish
RUN dotnet publish "nng-bot.csproj" -c Release -o /app/publish --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "nng-bot.dll"]
