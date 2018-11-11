FROM microsoft/dotnet:aspnetcore-runtime
COPY app/ /app/
WORKDIR /app

EXPOSE 80

ENTRYPOINT ["dotnet", "WebApi.dll"]
