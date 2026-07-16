# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["novitec-contabilidad.csproj", "./"]
RUN dotnet restore "novitec-contabilidad.csproj"

# Copy all source files
COPY . .
RUN dotnet publish "novitec-contabilidad.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published application
COPY --from=build /app/publish .

# Copy Excel template to the runtime app directory
COPY ["PLANTILLA_CAJA_CHICA_NOVICOMPU.xlsx", "./"]

ENTRYPOINT ["dotnet", "NovitecContabilidad.dll"]
