# Directory Monitor

Система мониторинга изменений в файловых каталогах с функцией журналирования и уведомлениями.

---

## 📌 Описание

Desktop-приложение для Windows, которое отслеживает изменения в указанных папках и выполняет автоматические действия по настраиваемым правилам.

**Что умеет:**
- Отслеживать создание, изменение, удаление и переименование файлов и папок
- Применять правила с условиями (расширение, размер, имя, тип элемента)
- Выполнять действия: уведомления, запуск программ, копирование, перемещение, удаление, переименование, создание/удаление папок
- Подставлять переменные в путях и аргументах (`{FilePath}`, `{FileName}`, `{Timestamp}`, `{Year}/{Month}/{Day}` и др.)
- Работать в фоновом режиме со сворачиванием в системный трей
- Вести журнал событий и системный лог

---

## 🚀 Как использовать

1. Скачайте и распакуйте архив с приложением.
2. Запустите `DirectoryMonitor.exe`.
3. На вкладке **Watched Paths** добавьте папку для отслеживания.
4. На вкладке **Rules** создайте правило:
   - Выберите тип события (`Created`, `Changed`, `Deleted`, `Renamed`)
   - Добавьте условия (расширение, размер, имя, тип элемента)
   - Назначьте действия (уведомление, запуск программы, копирование и т.д.)
5. На вкладке **Actions** можно создать действия для переиспользования.
6. На вкладке **Journal** просматривайте историю событий с фильтрацией.

Приложение можно свернуть в трей — мониторинг продолжится в фоновом режиме.

---

## ⚙️ Технологии

| Компонент | Технология |
|-----------|------------|
| Язык | C# 12.0 |
| Платформа | .NET 8 |
| Интерфейс | WPF (MVVM) |
| База данных | SQLite |
| ORM | Entity Framework Core |
| Сериализация | Newtonsoft.Json |
| Системный трей | Hardcodet.NotifyIcon.Wpf |

---

## 🗄️ Схема базы данных

База данных SQLite состоит из 7 таблиц:
WatchedPaths (отслеживаемые папки)
↓
WatchedPathRules (связь папок и правил)
↓
Rules (правила)
↓
RuleActions (связь правил и действий)
↓
Actions (действия)

EventLogEntries (журнал событий файловой системы)
SystemLogs (системный журнал)


Связи: папки и правила — многие ко многим; правила и действия — многие ко многим с полем `Order` для порядка выполнения.

---
## 📂 Общая структура решения

```plaintext
├── DirectoryMonitor.sln                # Файл решения
├── DirectoryMonitor.csproj             # Файл проекта
├── App.xaml / App.xaml.cs              # Точка входа, DI-контейнер
├── appsettings.json                    # Конфигурация приложения
├── icon.ico                            # Иконка приложения
│
├── 📁 Models/                          # Модели данных
│   ├── Entities/                       # Сущности БД (7 таблиц)
│   ├── Conditions/                     # Классы условий (AND/OR/NOT, размер, имя...)
│   └── Enums/                          # Перечисления (8 файлов)
│
├── 📁 Services/                        # Бизнес-логика
│   ├── FileSystemWatcherWrapper.cs
│   ├── WatcherManager.cs
│   ├── RuleEngine.cs
│   ├── JournalService.cs
│   ├── SystemLogService.cs
│   ├── NotificationService.cs
│   └── SettingsService.cs
│
├── 📁 Data/                            # Доступ к данным
│   ├── AppDbContext.cs
│   └── Repositories/                   # 12 репозиториев
│
├── 📁 Views/                           # WPF-окна (XAML + .cs)
│   ├── MainWindow.xaml
│   ├── RuleEditorWindow.xaml
│   ├── ActionEditorWindow.xaml
│   └── ... (всего 13 окон/контролов)
│
├── 📁 ViewModels/                      # MVVM-модели (11 файлов)
├── 📁 Converters/                      # XAML-конвертеры (6 файлов)
├── 📁 Helpers/                         # Вспомогательные классы
├── 📁 Themes/                          # Светлая и тёмная темы
├── 📁 Migrations/                      # Миграции EF Core (8 файлов)
│
├── 📁 bin/                             # Сборки (Debug/Release)
└── 📁 obj/                             # Объектные файлы
```
---

## 🔧 Ключевые компоненты

### Условия (Conditions)
Все условия наследуются от абстрактного класса `ConditionNode` и реализуют метод `IsMet()`:
- `ConditionGroup` — логические операторы AND/OR/NOT
- `ExtensionCondition` — проверка расширения файла
- `FileNameCondition` — проверка имени (Equals, Contains, StartsWith, EndsWith, Regex)
- `SizeCondition` — проверка размера (Greater, Less, Equal) с единицами (Bytes, KB, MB, GB)
- `ItemTypeCondition` — проверка типа (File / Directory)

### Действия (Actions)
Поддерживаются 8 типов действий:
- `Notification` — всплывающее уведомление
- `RunProgram` — запуск внешней программы
- `CopyFile` / `MoveFile` / `DeleteFile` / `RenameFile`
- `CreateDirectory` / `DeleteDirectory`

### Переменные для подстановки
{FilePath}, {FileName}, {FileNameWithoutExt}, {Extension},
{DirectoryPath}, {EventType}, {OldFilePath},
{Timestamp}, {Date}, {Time},
{Year}, {Month}, {Day}, {Hour}, {Minute}, {Second}

---

## 📄 Лицензия

MIT License. Используйте, модифицируйте, распространяйте — с указанием авторства.

---

## 📬 Контакты

**Автор:** SummonFly 
**GitHub:** [SummonFly](https://github.com/SummonFly)  
**Habr:** [Alugard](https://career.habr.com/alugard_info)

---

> Разработано в рамках выпускной квалификационной работы. 2026 г.
