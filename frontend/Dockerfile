# Bookshelf frontend — build Vue (Vite) i serwowanie przez nginx (multi-stage).

# ---- build ----
FROM node:22-alpine AS build
WORKDIR /app

# Najpierw manifesty -> warstwa npm ci cache'uje sie, dopoki nie zmienisz zaleznosci.
COPY package.json package-lock.json ./
RUN npm ci

# Reszta zrodel + build produkcyjny (vue-tsc + vite) -> /app/dist
COPY . ./
RUN npm run build

# ---- serve ----
FROM nginx:alpine AS serve
# Statyki SPA z etapu build.
COPY --from=build /app/dist /usr/share/nginx/html
# Nasza konfiguracja: SPA fallback + reverse-proxy /api -> api:8080.
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
