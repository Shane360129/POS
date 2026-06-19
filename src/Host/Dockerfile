# 多階段建置：以 repo 根目錄為 build context（需 Directory.Build.props 與 src/）。
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/ ./src/

RUN dotnet restore src/Host/Host.csproj
RUN dotnet publish src/Host/Host.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Host.dll"]
