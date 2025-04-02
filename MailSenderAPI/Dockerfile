#FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Use the official .NET Core SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

#RUN dotnet restore

WORKDIR "/src/"
RUN dotnet build -v diag -c Release -o /app/build

FROM base AS final
WORKDIR /app
COPY --from=build /app/build .

ENTRYPOINT ["./MailSenderAPI"]