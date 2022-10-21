# nng bot

[![License badge](https://img.shields.io/badge/license-EUPL-blue.svg)](LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/MrAlonas/nng-bot)](https://github.com/MrAlonas/nng-bot/issues)
[![Docker Build and Push](https://github.com/MrAlonas/nng-bot/actions/workflows/docker.yml/badge.svg)](https://github.com/MrAlonas/nng-bot/actions/workflows/docker.yml)

Чат-бот для групп nng, позволяющий пользователям подавать заявку на редактора и запрашивать разблокировку. Помимо этого так же предусмотрена админ-панель с возможностью просмотра статистики групп и рассмотрением запросов на разблокировку.

<p align="center">
  <img src=".github/IMAGES/bot.png">
</p>

## Установка

Воспользуйтесь готовым [Docker-контейнером](https://github.com/orgs/MrAlonas/packages/container/package/nng-bot).

По умолчанию используется порт `1220`, поэтому необходимо использовать прокси-сервер (например nginx).

## Настройка

### Переменные среды

* `DataURL` — Ссылка на [общий список](https://github.com/MrAlonas/nng#datajson)
* `EditorGrantEnabled` **(true)** — Включена ли выдача редакторов
* `LogUser` — Айди страницы человека, которому будут отправляться логи
* `UserToken` — Токен страницы, от которой выполняются действия
* `DialogGroupId` — Айди группы
* `DialogGroupToken` — Токен группы
* `DialogGroupSecret` — Секретный ключ
* `DialogGroupConfirm` — Строка, которую должен вернуть сервер
* `UpdateAtStart` **(true)** — Обновлять ли кэш при запуске
* `UpdatePerHours` **(4)** — Интервал обновления кэша в часах
* `AdminUsers` — Айди администраторов
* `EditorRestriction` **(20)** — Максимальное количество редакторов на человека
* `GroupManagersCeiling` **(100)** — Максимальное количество редакторов в группах
