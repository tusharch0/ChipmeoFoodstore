FROM node:alpine AS build
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,id=npm-landing,target=/root/.npm \
    npm ci
COPY . .
RUN npm run build

FROM node:alpine AS runtime
WORKDIR /app
RUN addgroup --system --gid 1001 foodstore && adduser --system --uid 1001 --ingroup foodstore foodstore
COPY --from=build --chown=foodstore:foodstore /app/dist ./dist
COPY --from=build --chown=foodstore:foodstore /app/node_modules ./node_modules
COPY --from=build --chown=foodstore:foodstore /app/package.json ./
USER foodstore
EXPOSE 4321
ENV HOST=0.0.0.0
ENV PORT=4321
CMD ["./node_modules/.bin/astro", "preview", "--host", "0.0.0.0", "--port", "4321"]
