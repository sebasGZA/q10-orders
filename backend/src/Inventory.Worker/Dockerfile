FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Inventory.Worker.csproj ./
RUN dotnet restore ./Inventory.Worker.csproj

COPY . .
RUN dotnet publish ./Inventory.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Inventory.Worker.dll"]
