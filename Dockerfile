FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RestaurantAPI.slnx", "./"]
COPY ["RestaurantAPI/RestaurantAPI.csproj", "RestaurantAPI/"]
COPY ["RestaurantSERVICE/RestaurantSERVICE.csproj", "RestaurantSERVICE/"]
COPY ["RestaurantDAL/RestaurantDAL.csproj", "RestaurantDAL/"]
COPY ["RestaurantDATA/RestaurantDATA.csproj", "RestaurantDATA/"]
RUN dotnet restore "RestaurantAPI/RestaurantAPI.csproj"
COPY . .
WORKDIR "/src/RestaurantAPI"
RUN dotnet build "RestaurantAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestaurantAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantAPI.dll"]
