# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy everything
COPY . ./

# Restore and build
RUN dotnet restore pleSecureDoc.sln
RUN dotnet publish pleSecureDoc/pleSecureDoc.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "pleSecureDoc.dll"]
