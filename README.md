# nng bot

[![License badge](https://img.shields.io/badge/license-EUPL-blue.svg)](LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/MrAlonas/nng-bot)](https://github.com/MrAlonas/nng-bot/issues)
[![Docker Build and Push](https://github.com/MrAlonas/nng-bot/actions/workflows/docker.yml/badge.svg)](https://github.com/MrAlonas/nng-bot/actions/workflows/docker.yml)

Чат-бот для групп nng, позволяющий пользователям подавать заявку на редактора и запрашивать разблокировку.

<p align="center">
  <img src=".github/bot.png">
</p>

## Установка

Воспользуйтесь готовым [Docker-контейнером](https://github.com/orgs/MrAlonas/packages/container/package/nng-bot).

По умолчанию используется порт `1220`, поэтому необходимо использовать прокси-сервер (например nginx).

## Настройка

### Переменные среды

* `REDIS_URL` — Ссылка на базу данных
