# ---- Backend build ----
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS backend-build
WORKDIR /src
COPY backend/ .
RUN dotnet publish ModernWMS/ModernWMS.csproj -c Release -o /out

# ---- Frontend build ----
FROM node:16-alpine AS frontend-build
WORKDIR /src
RUN npm install -g yarn
COPY frontend/package.json frontend/yarn.lock* ./
RUN yarn install --frozen-lockfile || yarn install
COPY frontend/ .
ARG VITE_BASE_PATH=http://127.0.0.1
ARG VITE_SERVER_PORT=20011
ENV VITE_BASE_PATH=$VITE_BASE_PATH
ENV VITE_SERVER_PORT=$VITE_SERVER_PORT
RUN yarn build

# ---- Final image ----
FROM ubuntu:22.04
RUN apt-get update && apt-get install -y wget curl \
    && wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y aspnetcore-runtime-7.0 nginx \
    && rm -rf /var/lib/apt/lists/* packages-microsoft-prod.deb \
    && mkdir -p /app/ /frontend/

WORKDIR /app
COPY docker/nginx.conf /etc/nginx/nginx.conf
COPY --from=frontend-build /src/dist /frontend/
COPY --from=backend-build /out /app/
COPY docker/run.sh /app/run.sh
RUN chmod u+x /app/run.sh

EXPOSE 80 21011
ENTRYPOINT ["/bin/bash", "/app/run.sh"]
