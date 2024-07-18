# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Define base image with .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy project files to the container
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Define runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy build artifacts from build image
COPY --from=build-env /app/out .

# Expose the port your application will run on
#EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "CanFlyPipeline.dll"]
