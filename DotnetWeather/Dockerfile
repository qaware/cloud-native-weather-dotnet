FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /DotnetWeather
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet build -c Release
RUN dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

WORKDIR /DotnetWeather
COPY --from=builder /DotnetWeather/bin/Release/net6.0/publish ./

CMD ["./DotnetWeather"]
