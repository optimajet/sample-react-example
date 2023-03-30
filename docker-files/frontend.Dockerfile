FROM node:lts-alpine3.17

RUN mkdir /app
WORKDIR /app

COPY package*.json .
RUN npm install --legacy-peer-deps

ENTRYPOINT ["npm", "run", "start"]
