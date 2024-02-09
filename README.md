# RmPm
## TODO

1. `[Утилита]` ~~Создание клиента~~
2. `[Утилита]` ~~Список активных клиентов (процессов)~~
3. `[Утилита]` ~~Удаление клиента (by PID, ID, Name)~~
4. `[Утилита]` Чтение конфига в ~~`Base64Uri`~~, ~~`JSON`~~ и `QR` форматах
5. `[Утилита]` ~~Хранение конфигов клиентов в friendly виде (сопоставление юзер-конфиг)~~
6. `[Утилита]` Pretty output (`--output pretty`)
7. `[Утилита]` Редактирование `FriendlyName` конфигов
8. `[Бот]` Перенос возможностей утилиты в бота

## Output format

По умолчания все операции выполняются с использованием логгирования (Debug, Info) для ясности пользователю. Но в целях автоматизации, следует добавить поддержку вывода в json формате.

**Пример 1:**

```bash
RmPm all --output pretty
```

Получить результат:

```json
[
    {
        "id": "234243-12341234-12341234-12341234",
        "pid": "12345",
        "name": "sparrow",
        "config": "/etc/shadowsocks-libev/config1.json"
    }
]
```

**Пример 2:**

```bash
RmPm del -c 12345 --output pretty
```

```json
{
    "status": "ok",
    "message": ""
}
```

```json
{
    "status": "client_not_found",
    "message": "Не удалось найти клиента по указанному PID 12345, возможно, он не активен"
}
```

Так же статус можно определять по результату выполнения программы (0, 1 и тд)

## Ассоциация конфиг файлов

#### Json формат

```json
[
    {
    	"FriendlyName": "MyClient",
    	"ConfigPath": "/etc/shadowsocks-libev/config1.json"
	},
    {
        "FriendlyName": "MyClient-2",
    	"ConfigPath": "/etc/shadowsocks-libev/config2.json"
    }
]
```