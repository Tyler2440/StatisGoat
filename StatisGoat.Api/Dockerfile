FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:90
EXPOSE 90

ADD Modeling/ Modeling/

RUN set -xe \
    && apt-get update -y \
    && apt-get install python3-pip -y
RUN pip3 install --upgrade pip && \
    pip3 install numpy pandas scikit-learn joblib
COPY . .

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["StatisGoat.Api/StatisGoat.Api.csproj", "StatisGoat.Api/"]
COPY ["StatisGoat/StatisGoat.csproj", "StatisGoat/"]
RUN dotnet restore "StatisGoat.Api/StatisGoat.Api.csproj"
COPY . .
WORKDIR "/src/StatisGoat.Api"
RUN dotnet build "StatisGoat.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisGoat.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatisGoat.Api.dll", "--environment=Production"]
