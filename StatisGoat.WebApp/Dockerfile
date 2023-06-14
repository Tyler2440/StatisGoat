FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["StatisGoat.WebApp/StatisGoat.WebApp.csproj", "StatisGoat.WebApp/"]
COPY ["StatisGoat/StatisGoat.csproj", "StatisGoat/"]
RUN dotnet restore "StatisGoat.WebApp/StatisGoat.WebApp.csproj"
COPY . .
WORKDIR "/src/StatisGoat.WebApp"
RUN dotnet build "StatisGoat.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisGoat.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatisGoat.WebApp.dll"]