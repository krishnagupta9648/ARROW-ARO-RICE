# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project definition and restore dependencies
COPY ["ARROW-ARO-RICE.csproj", "./"]
RUN dotnet restore "ARROW-ARO-RICE.csproj"

# Copy all source files and publish release
COPY . .
RUN dotnet publish "ARROW-ARO-RICE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Default port for Render and container clouds
ENV PORT=10000
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ARROW-ARO-RICE.dll"]
