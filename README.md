# RmPm
Remote Proxy Manager

## План

1. Создание новых VPN пользователей
2. Хранение user-friendly настроек пользователей (bin/client.json -> Валентин и тд)
3. Чтение настроек (QR или другой формат) в удобном формате

## TODO

1. `[Утилита]` ~~Создание клиента~~
2. `[Утилита]` ~~Список активных клиентов (процессов)~~
3. `[Утилита]` ~~Удаление клиента (by PID, ID, Name)~~
4. `[Утилита]` Чтение конфига в `Base64Uri`, `JSON` и `QR` форматах
5. `[Утилита]` ~~Хранение конфигов клиентов в friendly виде (сопоставление юзер-конфиг)~~
6. `[Утилита]` Вывод результатов выполнения утилиты в формате `json` для любой операции
7. `[Утилита]` Редактирование `FriendlyName` конфигов
8. `[Бот]` Перенос возможностей утилиты в бота

## Output

По умолчания все операции выполняются с использованием логгирования (Debug, Info) для ясности пользователю. Но в целях автоматизации, следует добавить поддержку вывода в json формате.

```json
{
    "command": "delete/create/info",
    "status": "Ok/Error",
    "result": { },
    "error": {
        "exception": "exception_name",
        "message": "exception_message",
        "stack_trace": "stack"
    },
    "logs": [
        "logs..."
    ]
}
```

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