# Servidor de desenvolvimento (ng serve) — apenas ambiente local.
FROM node:24-alpine
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
EXPOSE 4200
# O proxy aponta para o serviço "api" da rede do compose, mantendo a mesma origem para cookies.
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--proxy-config", "proxy.conf.docker.json", "--poll", "2000"]
