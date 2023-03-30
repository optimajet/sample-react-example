FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY Backend.sln .
COPY WorkflowApi/WorkflowApi.csproj WorkflowApi/WorkflowApi.csproj
COPY WorkflowLib/WorkflowLib.csproj WorkflowLib/WorkflowLib.csproj
RUN dotnet restore Backend.sln --source https://api.nuget.org/v3/index.json

COPY ./ .
RUN dotnet build Backend.sln --configuration Release --output /app

FROM build AS publish
WORKDIR /src/WorkflowApi
RUN dotnet publish WorkflowApi.csproj --configuration Release --output /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app/sample

COPY --from=publish /app ./bin

RUN useradd --user-group --uid 1000 wfe
RUN chown -R wfe:wfe /app

USER wfe

WORKDIR /app/sample/bin
ENTRYPOINT ["dotnet", "WorkflowApi.dll"]
