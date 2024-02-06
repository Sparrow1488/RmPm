# RmPm
Remote Proxy Manager

## План

1. Создание новых VPN пользователей
2. Хранение user-friendly настроек пользователей (bin/client.json -> Валентин и тд)
3. Чтение настроек (QR или другой формат) в удобном формате

## TODO

1. `[Утилита]` Создание клиента
2. `[Утилита]` Список активных клиентов (процессов)
3. `[Утилита]` Удаление клиента
4. `[Утилита]` Чтение конфига в `Inline`, `JSON` и `QR` форматах
5. `[Утилита]` Хранение конфигов клиентов в friendly виде (сопоставление юзер-конфиг)
6. `[Бот]` Перенос возможностей утилиты в бота

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