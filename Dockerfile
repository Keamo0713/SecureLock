# Use .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore pleSecureDoc.sln

# Build and publish
RUN dotnet publish pleSecureDoc/pleSecureDoc.csproj -c Release -o /out

# Use .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose port
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "pleSecureDoc.dll"]
