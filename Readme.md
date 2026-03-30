# SS14.Launcher
<img width="1017" height="712" alt="изображение" src="https://github.com/user-attachments/assets/35deb3cf-3cce-4242-acad-341098d94a84" />

discord: https://discord.gg/68WfqhBJx3

# Features
* Resource Packs
* Displaying game mode, map and ping in the launcher
* Reworked menu

# Resource Packs

Resource packs replace game files by path at launch time. 

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

# The LAUNCHER WON'T HAVE harmony SUPPORT, hwid spoofing, and the like, it's designed for FAIR PLAY.

# Features
* Ресурспаки
* Отображение текущего режима игры, карты и пинга прямо в лаунчере
* Переработанное меню

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


# В ЛАУНЧЕРЕ НЕ БУДЕТ ПОДДЕРЖКИ harmony, спуфа хвидов и тому подобного, он предназначен для ЧЕСТНОЙ игры.
