FROM node:alpine AS build
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,id=npm,target=/root/.npm \
    npm ci
COPY . .
RUN npm run build

FROM node:alpine AS runtime
WORKDIR /app
RUN addgroup --system --gid 1001 foodstore && adduser --system --uid 1001 --ingroup foodstore foodstore
COPY --from=build --chown=foodstore:foodstore /app/build ./build
COPY --from=build --chown=foodstore:foodstore /app/node_modules ./node_modules
COPY --from=build --chown=foodstore:foodstore /app/package.json ./
COPY --from=build --chown=foodstore:foodstore /app/.svelte-kit ./.svelte-kit
USER foodstore
EXPOSE 3000
ENV NODE_ENV=production
ENV HOST=0.0.0.0
ENV PORT=3000
CMD ["node", "build"]
