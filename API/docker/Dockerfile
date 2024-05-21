FROM mcr.microsoft.com/dotnet/sdk:8.0 AS pre

WORKDIR /app

COPY . .
RUN dotnet restore

FROM pre AS dev

CMD dotnet run --no-launch-profile

FROM pre AS build

RUN dotnet publish SRkMatchmakerAPI.csproj -c release -o /app/Dist -r linux-x64 --self-contained # -p:PublishTrimmed=true

FROM ubuntu AS serve

ARG PORT
ENV PORT=${PORT}

WORKDIR /app
COPY --from=build /app/Dist ./

EXPOSE ${PORT}

#ENTRYPOINT dotnet WebApiTut.dll

ENTRYPOINT ./SRkMatchmakerAPI