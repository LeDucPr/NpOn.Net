FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GeneralService/GeneralService.csproj", "GeneralService/"]
COPY ["CommonWebApplication/CommonWebApplication.csproj", "CommonWebApplication/"]
COPY ["QuestionServiceObject/QuestionServiceObject.csproj", "QuestionServiceObject/"]
COPY ["CommonGrpcObject/CommonGrpcObject.csproj", "CommonGrpcObject/"]
COPY ["CommonDb/CommonDb.csproj", "CommonDb/"]
COPY ["CommonMode/CommonMode.csproj", "CommonMode/"]
COPY ["GenericEnums/GenericEnums.csproj", "GenericEnums/"]
COPY ["CommonObject/CommonObject.csproj", "CommonObject/"]
COPY ["ObjectHandleFlow/ObjectHandleFlow.csproj", "ObjectHandleFlow/"]
COPY ["GrpcAddService/GrpcAddService.csproj", "GrpcAddService/"]
COPY ["IQuestionService/IQuestionService.csproj", "IQuestionService/"]
COPY ["AccountServiceObject/AccountServiceObject.csproj", "AccountServiceObject/"]
COPY ["IAccountService/IAccountService.csproj", "IAccountService/"]
COPY ["IGeneralService/IGeneralService.csproj", "IGeneralService/"]
COPY ["GeneralServiceObject/GeneralServiceObject.csproj", "GeneralServiceObject/"]
COPY ["RabbitMqBroker/RabbitMqBroker.csproj", "RabbitMqBroker/"]
COPY ["DbFactory/DbFactory.csproj", "DbFactory/"]
COPY ["CassandraExtCm/CassandraExtCm.csproj", "CassandraExtCm/"]
COPY ["PostgresExtCm/PostgresExtCm.csproj", "PostgresExtCm/"]
COPY ["MongoDbExtCm/MongoDbExtCm.csproj", "MongoDbExtCm/"]
COPY ["MssqlExtCm/MssqlExtCm.csproj", "MssqlExtCm/"]
COPY ["HandleFlow/HandleFlow.csproj", "HandleFlow/"]
COPY ["ProjectEntry/ProjectEntry.csproj", "ProjectEntry/"]
RUN dotnet restore "GeneralService/GeneralService.csproj"
COPY . .
WORKDIR "/src/GeneralService"
RUN dotnet build "./GeneralService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GeneralService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeneralService.dll"]
