# Matrix Mapping Analyzer

Поиск всех матриц смещения (offset), которые трансформируют набор матриц модели (Model) так, чтобы они совпали с матрицами пространства (Space): ∀m∈Model, offset·m∈Space. Проект включает быстрый параллельный вычислитель, визуализацию результатов с пулами объектов и шейдером под Built‑in Render Pipeline, а также экспорт результатов.

## 📦 Технологии

- Unity `2022.3.62f2` (Built‑in Render Pipeline)
- C# 9 / .NET Framework совместимо с Unity
- VContainer (DI), UniTask, R3, Newtonsoft.Json

## ✨ Возможности

- Поиск всех валидных offset‑матриц по условию ∀m∈Model, offset·m∈Space
- Параллельная проверка кандидатов, ранние прерывания, прогресс
- Корректное сравнение матриц с абсолютной+относительной погрешностью (Tolerance)
- Визуализация результатов:
  - Пул объектов без GC‑аллокаций
  - Индивидуальные цвета через MaterialPropertyBlock
  - Простой шейдер для Built‑in RP с поддержкой PropertyBlock и instancing
- Экспорт результатов в JSON
- Полезные утилиты для валидации входных данных

## 🛠️ Требования

- Unity `2022.3.62f2` или совместимая LTS
- Windows/macOS, GPU c поддержкой instancing (обычно по умолчанию)

## 🚀 Запуск

1) Откройте папку репозитория в Unity Hub (Unity `2022.3.62f2`).
2) Откройте сцену MatrixMappingAnalyzer.
3) Нажмите Play.
4) В UI:
   - Найти: запускает поиск offset‑ов
   - Визуализировать: отображает группы смещений
   - Экспорт: сохраняет JSON с результатами

## ⚙️ Настройка

Откройте `Assets/Config/OffsetFinderConfig.asset`:

- Paths:
  - ModelJsonPath: `Assets/Data/model.json`
  - SpaceJsonPath: `Assets/Data/space.json`
  - OutputJsonPath: `Assets/Data/offsets.json`
- Search:
  - Tolerance: `1e-4` (абс.+отн.)
  - BatchSize, MaxThreads
- Visualization:
  - MaxGroupsToVisualize

Файлы данных имеют формат массива `MatrixData` (16 элементов на матрицу). См. `Assets/Scripts/Core/MatrixData.cs`.

## 🧮 Алгоритм (кратко)

- Для каждого `space[i]` строим кандидата: `offset = space[i] · inverse(model[0])`.
- Проверяем условие: для каждого `m ∈ Model` проверяем, что `offset·m` присутствует в Space с учётом tolerance.
- Сравнение матриц покомпонентное с комбинированной абсолютной и относительной погрешностью. См. `Services/ToleranceUtils.cs`.

## 🎨 Визуализация

- Префаб модели: назначьте в `OffsetVisualizer` поле `Model Prefab` (например, куб с материалом)
- Root для размещения: `Visualization Root`
- Цвета групп смещений: Gradient в `OffsetVisualizer`
- Шейдер: `Assets/Shaders/OffsetVisualization.shader` (Built‑in, PropertyBlock + GPU Instancing)
- Компонент для объектов: `VisualizationObject` (кэш Renderer, SetColor через PropertyBlock)
- Пул: `Infrastructure/ObjectPool<T>` + интерфейс `IPoolable`

## 📁 Структура проекта (основное)

```
Assets/
  Scripts/
    Config/            // ScriptableObject конфиг
    Core/              // Базовые структуры (MatrixData, FoundOffset)
    Infrastructure/    // DI scope, IPoolable, ObjectPool
    Model/             // Реактивная модель состояния
    Presenters/        // Сервис-координатор (OffsetFinderService)
    Services/          // Загрузка, вычисление, экспорт, утилиты
      Interfaces/
  View/
    OffsetFinderUIController.cs
    OffsetVisualizer.cs
    VisualizationObject.cs
  Shaders/
    OffsetVisualization.shader
  Materials/
  Data/               // model.json, space.json, offsets.json
  Scenes/
```

## 📊 Формат данных (JSON)

`model.json`, `space.json` — массив объектов `MatrixData`:

```
[
  {
    "m00": 1, "m01": 0, "m02": 0, "m03": 0,
    "m10": 0, "m11": 1, "m12": 0, "m13": 0,
    "m20": 0, "m21": 0, "m22": 1, "m23": 0,
    "m30": 0, "m31": 0, "m32": 0, "m33": 1
  },
  ...
]
```

## 🎛️ Рекомендации по Tolerance

- Универсальный старт: `1e-4` (abs=1e-4, rel=1e-4)
- Чистая математика: `1e-6`
- Шумные данные/измерения: `1e-3 – 1e-2`
- Всегда используйте комбинированное сравнение (см. `ToleranceUtils`)


## 🧩 Важные классы

- `Presenters/OffsetFinderService.cs` — оркестрация загрузки/поиска/экспорта/визуализации
- `Services/ParallelOffsetCalculator.cs` — поиск offset‑ов (параллельно, корректная проверка)
- `Services/ToleranceUtils.cs` — сравнение чисел/векторов/матриц с толерантностью
- `Services/JsonMatrixLoader.cs` — загрузка JSON → Matrix4x4
- `Services/JsonResultExporter.cs` — экспорт результатов
- `View/OffsetFinderUIController.cs` — UI и прогресс
- `View/OffsetVisualizer.cs` + `View/VisualizationObject.cs` — визуализация, цвета, пуллинг
