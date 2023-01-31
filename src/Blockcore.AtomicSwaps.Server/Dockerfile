FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5500
ENTRYPOINT ["dotnet", "Blockcore.AtomicSwaps.Server.dll"]
