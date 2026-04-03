# SS14.Launcher
<img width="1017" height="712" alt="изображение" src="https://github.com/user-attachments/assets/35deb3cf-3cce-4242-acad-341098d94a84" />

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
* Gameplay and data roots such as `Prototypes/` are ignored by the launcher and will never be included in the overlay zip.
* The loader enforces the same restriction again at mount time, so blocked roots are not exposed to the client even if they somehow end up inside the archive.

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
