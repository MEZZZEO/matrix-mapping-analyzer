# Matrix Mapping Analyzer

Поиск всех матриц смещения (offset), которые трансформируют набор матриц модели (Model) так, чтобы они совпали с матрицами пространства (Space):

```
∀ m ∈ Model : offset · m ∈ Space
```

Алгоритм перебирает кандидаты offset, строя их из матриц пространства и проверяя условие по всему набору модели. Проект включает:
- Параллельный вычислитель с контролем batch размера
- Корректную систему округления через tolerance → digits
- Диагностику и валидацию входных данных
- Визуализацию результатов (пул объектов + MaterialPropertyBlock, шейдер для Built‑in Render Pipeline)
- Экспорт результатов в JSON
- Чистую архитектуру с разделением Core / Services / Utils / View / Infrastructure

## 📦 Технологии
- Unity `2022.3.62f2` (Built‑in Render Pipeline)
- C# 9 (совместимо с Unity runtime)
- VContainer (DI), UniTask (асинхронность), R3 (реактивная модель), Newtonsoft.Json

## ✨ Возможности
- Поиск всех валидных смещений, удовлетворяющих условию полноты
- Параллельная проверка с динамическим `BatchSize` из конфига
- Округление матриц до `digits = round(-log10(tolerance))` для быстрых hash‑поисков
- Диагностика расхождений (hash vs NearlyEqual) для первых кандидатов и найденных смещений
- Визуализация групп смещённых матриц с цветовой дифференциацией
- Экспорт найденных смещений в JSON
- Пуллинг для избежания лишних аллокаций при повторном поиске

## 🚀 Быстрый старт
1. Откройте проект в Unity (версия из шапки).
2. Настройте `OffsetFinderConfig.asset`.
3. Откройте сцену 'Assets/Scenes/MatrixMappingAnalyzer.unity'.
4. Нажмите Play → «Найти».
5. При необходимости включите диагностику.
6. «Визуализировать» или «Экспортировать» результаты.

## 🗂 Архитектура директорий
```
Assets/Scripts/
  Config/            # ScriptableObject конфиг (OffsetFinderConfig)
  Core/              # Базовые модели данных: Matrix, Matrices, FoundOffset, MatrixData
  Extensions/        # Расширения (MatrixExtensions: Round, GetHash)
  Infrastructure/    # LifetimeScope (DI), ObjectPool, IPoolable
  Model/             # OffsetFinderModel (реактивное состояние, IDisposable)
  Services/          # Бизнес-сервисы: OffsetFinderService, ParallelOffsetCalculator, JsonMatrixLoader, JsonResultExporter
    Interfaces/      # Контракты (IOffsetCalculator, IMatrixLoader, IResultExporter, IOffsetVisualizer)
  Utils/             # Утилиты: ToleranceUtils, MatrixValidationUtils, SuperFastHash
  View/              # UI и визуализация: OffsetFinderUIController, OffsetVisualizer, VisualizationObject
```

### Роли слоёв
- Core: неизменяемые структуры и сущности, не тянут UI
- Utils: чистые статические утилиты (сравнение, хеши, диагностика)
- Services: оркестрация и алгоритмы (без прямой работы с UI)
- Model: реактивное состояние для UI (через R3/Observable)
- View: MonoBehaviour компоненты, подписки на Model, инициирование операций
- Infrastructure: DI, пуллинг

## 🔄 Жизненный цикл операции поиска
1. UI (OffsetFinderUIController) получает событие «Найти»
2. Готовит `digits = DigitsFromTolerance(tolerance)` и создает:
   ```csharp
   var model = new Matrices(ModelJsonPath, digits);
   var space = new Matrices(SpaceJsonPath, digits);
   ```
3. Передает их в `OffsetFinderService.FindOffsetsAsync(model, space)`
4. Сервис валидирует входные наборы (MatrixValidationUtils)
5. Параллельно перебирает кандидаты offset:
   ```csharp
   offset = space[i].Original * inverse(model[0].Original)
   ```
6. Для каждого offset проверяется условие через hash округленных произведений
7. Найденные offset публикуются в модель (ReactiveProperty + Subject)
8. UI обновляет прогресс, список, кнопки
9. Пользователь визуализирует / экспортирует результаты

## 🧠 Алгоритм проверки
- Округление: количество знаков `digits` рассчитывается от tolerance
- Каждая матрица хранит:
  - `Original` (Matrix4x4)
  - `Rounded` (округлено до digits)
  - `Hash` (SuperFastHash по бинарному образу Rounded)
- Условие валидации offset:
  ```csharp
  foreach (var m in model)
  {
      var transformed = offset * m.Original;
      if (!space.Contains(transformed.Round(digits).GetHash())) return false;
  }
  return true;
  ```
- Это обеспечивает O(1) membership check по хешу без покомпонентного сравнения на каждом шаге

## 🩺 Диагностика
Конфиг `OffsetFinderConfig` содержит флаги:
- `EnableDiagnostics` — анализ первых кандидатов (которые НЕ прошли)
- `DiagnoseFoundOffsets` — проверка нескольких найденных offset (должно быть 100% совпадений)
- `MaxCandidatesToDiagnose` — сколько кандидатов показать
Диагностика выводит:
```
[Diag:cand[0]] matches: hash=1/100, nearly=1/100, digits=2, tol=0.01
[Diag:found[0]] matches: hash=100/100, nearly=100/100, digits=2, tol=0.01
```
Если найденные offset показывают не 100/100 — проблема в данных.

## 🎯 Tolerance и digits
- Связь: tolerance → digits = clamp(round(-log10(tol)), 0..6)
- Малое tolerance (1e-6) → много знаков → строгий поиск
- Большое tolerance (1e-2) → меньше знаков → более «агрессивное» склеивание
- Если результаты "пропадают" на меньшем tolerance, возможно, ранее они были артефактами.

## ⚙️ Конфигурация (`OffsetFinderConfig`)
Параметры:
- Paths: `ModelJsonPath`, `SpaceJsonPath`, `OutputJsonPath`
- Поиск: `Tolerance`, `BatchSize`, `MaxThreads`
- Визуализация: `MaxGroupsToVisualize`
- Диагностика: флаги и лимиты как описано выше

## 🧮 Формат входных данных
`model.json`, `space.json` — массив объектов `MatrixData` (16 полей m00..m33). Пример:
```json
[
  {
    "m00": 1, "m01": 0, "m02": 0, "m03": 0,
    "m10": 0, "m11": 1, "m12": 0, "m13": 0,
    "m20": 0, "m21": 0, "m22": 1, "m23": 0,
    "m30": 0, "m31": 0, "m32": 0, "m33": 1
  }
]
```

## 🖥 Визуализация
- Использует `OffsetVisualizer` + пул `ObjectPool<T>`
- Цвета задаются градиентом
- Материалы меняются через `MaterialPropertyBlock` без дублирования материала
- Шейдер: простой, поддерживает цвет и instancing (должен находиться в `Shaders/` – добавьте при необходимости)

## 🧵 Параллельность
- `ParallelOffsetCalculator` использует batch‑итерации размера `_batchSize` из конфига
- Передача прогресса через `IProgress<int>` → обновление UI
- CancellationToken поддерживает остановку

## 🧹 Управление ресурсами
- `OffsetFinderModel` реализует `IDisposable`: освобождает ReactiveProperty и Subject
- `OffsetFinderService` освобождает `CancellationTokenSource`
- UI контроллер освобождает подписки (`CompositeDisposable`) в `OnDestroy`
- DI контейнер (VContainer) вызывает Dispose у Singletonов при завершении жизни scope

## 📤 Экспорт
Результаты (`List<FoundOffset>`) сохраняются в `OutputJsonPath` через `JsonResultExporter`.
