# ECS Code Generator

Incremental Source Generator for Unity 2022.3+ that automatically generates unique IDs for ECS-like components.

## Usage

Mark your components with `[ECSComponentAttribute]`:

```csharp
[ECSComponentAttribute]
public struct HealthComponent { public int Value; }

[ECSComponentAttribute]
public struct PositionComponent { public float X, Y; }
```

The generator will create ECSComponentIds.g.cs with:

```csharp
public static class ECSComponentIds
{
    public const int HealthComponent = 0;
    public const int PositionComponent = 1;
    
    public static bool TryGetId<T>(out int id) where T : struct { ... }
    public static int GetId<T>() where T : struct { ... }
    public static string GetComponentName(int id) { ... }
}
```

Use it:

```csharp
if (ECSComponentIds.TryGetId<HealthComponent>(out var id))
{
    Debug.Log($"Health ID: {id}");
}
```