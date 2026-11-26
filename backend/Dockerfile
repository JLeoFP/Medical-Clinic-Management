# Imagens apenas para desenvolvimento local (as imagens de produção ficam para a Fase 5 — implantacao-aws).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS fonte
WORKDIR /src
COPY . .
RUN dotnet tool restore && dotnet restore Clinica.slnx

# Aplica as migrations com o papel dono do banco (a API nunca aplica migrations ao iniciar).
FROM fonte AS migrador
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "src/Clinica.Infrastructure"]

FROM fonte AS publicacao
RUN dotnet publish src/Clinica.Api/Clinica.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=publicacao /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Clinica.Api.dll"]
