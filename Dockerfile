# ------------------- Stage 1: Build -------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first (layer caching)
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(find . -name "*.csproj"); do mkdir -p $(dirname $file)/obj; done
COPY . .

# Restore & Build
RUN dotnet restore Clinic.sln
RUN dotnet publish Clinic.sln -c Release -o /app/publish --no-restore

# ------------------- Stage 2: Runtime -------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy only the published output
COPY --from=build /app/publish .

# Environment variables (optional - change as needed)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Clinic.dll"]
# لو اسم الـ project مختلف (مش Clinic) → غيّر Clinic.dll لاسم الـ dll الحقيقي