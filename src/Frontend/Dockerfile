FROM node:25-alpine AS build
WORKDIR /app
COPY src/Frontend/package*.json ./
RUN npm install
COPY src/Frontend/ .
RUN npm run build

FROM nginx:stable-alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
