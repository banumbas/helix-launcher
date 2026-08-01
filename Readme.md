# SS14.Launcher

discord: https://discord.gg/68WfqhBJx3

# Лицензирование

Исходный код лаунчера Space Station 14, унаследованный от upstream-проекта, сохраняет условия лицензии MIT - см. [LICENSE.txt](LICENSE.txt).

Все изменения, созданные в рамках этого форка, распространяются по лицензии GNU Affero General Public License v3.0 (AGPL-3.0-only) - см. [LICENSE-AGPL-3.0.txt](LICENSE-AGPL-3.0.txt).

Код, написанный Wizden, распространяется по лицензии MIT, если в соответствующем файле не указано иное.

## Licensing

The Space Station 14 launcher source code inherited from the upstream project remains licensed under the MIT License - see [LICENSE.txt](LICENSE.txt).

All changes created for this fork are licensed under the GNU Affero General Public License v3.0 (AGPL-3.0-only) - see [LICENSE-AGPL-3.0.txt](LICENSE-AGPL-3.0.txt).

Code written by Wizden is licensed under the MIT License unless the relevant file states otherwise.

# Features
* Resource Packs
* Displaying game mode, map and ping in the launcher
* Redesigned menu with customization options
* Custom Discord RPC
* Config system

# Resource Packs

Resource packs replace client-facing asset files by path at launch time.

Pack directory:
* `%AppData%/Space Station 14/launcher/ResourcePacks/<PackName>` on Windows by default

Minimal pack structure:

```text
ResourcePacks/
  MyPack/
    meta.json
    Resources/
      Textures/
      Locale/
```

Minimal `meta.json`:

```json
{
  "name": "My Pack",
  "description": "Custom textures and locale",
  "target": ""
}
```

Notes:
* Files are overridden by their relative path inside `Resources/`.
* `target` is optional. Leave it empty to apply the pack to any fork.
* If you override files inside an `.rsi` directory, keep the correct `.rsi/meta.json` next to the changed textures.
* Only `Audio/`, `Fonts/`, `Locale/`, `Shaders/`, and `Textures/` roots are mounted from a pack.

# The LAUNCHER WON'T HAVE harmony SUPPORT, hwid spoofing, and the like, it's designed for FAIR PLAY.

# Features
* Ресурспаки
* Отображение текущего режима игры, карты и пинга прямо в лаунчере
* Переработанное меню с возможность кастомизации
* Кастомный Дискорд RPC
* Система конфигов

# Ресурспаки

Ресурспаки заменяют файлы игры по пути к ним во время запуска.

Каталог пакетов:
* "%AppData%/Space Station 14/launcher/ResourcePacks/<Имя пакета>" в Windows по умолчанию

Минимальная структура ресурспака:

```text
ResourcePacks/
  MyPack/
    meta.json
    Resources/
      Textures/
      Locale/
```

```json
{
  "name": "Имя",
  "description": "Описание",
  "target": ""
}
```

Записи:
* Файлы переопределяются их относительным путем внутри `Resources/`.
* `target` необязательно. Оставьте это поле пустым, чтобы применить пакет к любому форку.
* Если вы переопределяете файлы в каталоге `.rsi`, сохраняйте правильный `.rsi/meta.json рядом с измененными текстурами.
* Только `Audio/`, `Fonts/`, `Locale/`, `Shaders/`, и `Textures/` монтируются в пак.

# В ЛАУНЧЕРЕ НЕ БУДЕТ ПОДДЕРЖКИ harmony, спуфа хвидов и тому подобного, он предназначен для ЧЕСТНОЙ игры.
