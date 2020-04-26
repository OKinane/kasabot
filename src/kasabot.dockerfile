ARG CORE_VERSION=3.1
ARG CORE_CHANNEL=core
ARG PROJECT_DIR=KasaBot/src
ARG PROJECT_ASSEMBLYNAME=KasaBot

FROM mcr.microsoft.com/dotnet/${CORE_CHANNEL}/sdk:${CORE_VERSION}-alpine AS build
ARG PROJECT_DIR
ARG PROJECT_ASSEMBLYNAME
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /src
COPY . .
WORKDIR $PROJECT_DIR
RUN dotnet restore --runtime linux-musl-x64 --no-cache
RUN dotnet publish  -p:AssemblyName=$PROJECT_ASSEMBLYNAME \
                    -p:PublishTrimmed=true \
                    --nologo \
                    --no-restore \
                    --self-contained \
                    --configuration Release \
                    --runtime linux-musl-x64 \
                    --output /app/publish \
                    -maxcpucount

FROM mcr.microsoft.com/dotnet/${CORE_CHANNEL}/runtime-deps:${CORE_VERSION}-alpine
ARG PROJECT_ASSEMBLYNAME
ENV ENTRYPOINT=$PROJECT_ASSEMBLYNAME
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT "./$ENTRYPOINT"