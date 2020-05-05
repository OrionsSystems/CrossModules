FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app-portal

# Copy csproj and restore as distinct layers
COPY *.Debug.csproj ./
RUN dotnet restore



# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out


# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app-portal
COPY --from=build-env /app-portal/out .

ENTRYPOINT ["dotnet", "Orions.Systems.CrossModules.Portal.dll"]