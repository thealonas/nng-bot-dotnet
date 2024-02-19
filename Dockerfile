FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 1220

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build
ARG TARGETARCH
WORKDIR /src
COPY ["nng-bot.csproj", "."]
RUN dotnet restore -a $TARGETARCH "./nng-bot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "nng-bot.csproj" -c Release -o /app/build --no-self-contained

FROM build AS publish
RUN dotnet publish "nng-bot.csproj" -c Release -a $TARGETARCH -o /app/publish --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "nng-bot.dll"]
