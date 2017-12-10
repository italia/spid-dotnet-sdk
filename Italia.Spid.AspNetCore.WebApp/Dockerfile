FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY *.sln ./
COPY Italia.Spid.AspNetCore.WebApp/Italia.Spid.AspNetCore.WebApp.csproj Italia.Spid.AspNetCore.WebApp/
RUN dotnet restore
COPY . .
WORKDIR /src/Italia.Spid.AspNetCore.WebApp
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Italia.Spid.AspNetCore.WebApp.dll"]
