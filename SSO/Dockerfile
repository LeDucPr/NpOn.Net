FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SSO/SSO.csproj", "SSO/"]
COPY ["QuestionServiceObject/QuestionServiceObject.csproj", "QuestionServiceObject/"]
COPY ["CommonGrpcObject/CommonGrpcObject.csproj", "CommonGrpcObject/"]
COPY ["CommonDb/CommonDb.csproj", "CommonDb/"]
COPY ["CommonMode/CommonMode.csproj", "CommonMode/"]
COPY ["GenericEnums/GenericEnums.csproj", "GenericEnums/"]
COPY ["CommonObject/CommonObject.csproj", "CommonObject/"]
COPY ["ObjectHandleFlow/ObjectHandleFlow.csproj", "ObjectHandleFlow/"]
COPY ["CommonWebApplication/CommonWebApplication.csproj", "CommonWebApplication/"]
COPY ["GrpcAddService/GrpcAddService.csproj", "GrpcAddService/"]
COPY ["IQuestionService/IQuestionService.csproj", "IQuestionService/"]
COPY ["AccountServiceObject/AccountServiceObject.csproj", "AccountServiceObject/"]
COPY ["IAccountService/IAccountService.csproj", "IAccountService/"]
COPY ["IGeneralService/IGeneralService.csproj", "IGeneralService/"]
COPY ["GeneralServiceObject/GeneralServiceObject.csproj", "GeneralServiceObject/"]
COPY ["RabbitMqBroker/RabbitMqBroker.csproj", "RabbitMqBroker/"]
RUN dotnet restore "SSO/SSO.csproj"
COPY . .
WORKDIR "/src/SSO"
RUN dotnet build "./SSO.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SSO.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SSO.dll"]
