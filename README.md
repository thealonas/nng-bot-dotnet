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

### appsettings.json

```
{
  "DataURL": "Ссылка на общий список (см. MrAlonas/nng)",
  "EditorGrantEnabled": true, <- Включена ли выдача редакторов
  "LogUser": Айди страницы человека, которому будут отправляться логи,
  "Auth": {
    "UserToken": "Токен страницы, от которого выполняются действия",
    "DialogGroupId": Айди группы,
    "DialogGroupToken": "Токен группы",
    "DialogGroupSecret": "Секретный ключ",
    "DialogGroupConfirm": "Строка, которую должен вернуть сервер"
  },
  "Cache": {
    "UpdateAtStart": true, <- Обновлять ли кэш при запуске
    "UpdatePerHours": Интервал обновления кэша в часах
  }
}
```

### users.json

```
{
  "BannedUsers": [Айди заблокированных пользователей],
  "PriorityUsers": [Айди приоритетных пользователей],
  "AdminUsers": [Айди администраторов],
  "EditorRestriction": Максимальное количество редакторов на человека,
  "GroupManagersCelling": Максимальное количество редакторов в группах
}
```
