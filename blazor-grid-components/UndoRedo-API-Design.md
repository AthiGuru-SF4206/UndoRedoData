# Undo & Redo API Design

## GridEditSettings

### EnableUndoRedo
```csharp
[Parameter]
public bool EnableUndoRedo { get; set; } = false;
```

### UndoRedoLimit
```csharp
[Parameter]
public int UndoRedoLimit { get; set; } = 20;
```

## Public Methods

### UndoAsync
```csharp
Task UndoAsync();
```

### RedoAsync
```csharp
Task RedoAsync();
```

### UndoAllAsync
```csharp
Task UndoAllAsync();
```

### RedoAllAsync
```csharp
Task RedoAllAsync();
```

### ClearUndoRedoAsync
```csharp
Task ClearUndoRedoAsync();
```

## Stack Properties

```csharp
public int UndoCount { get; }
public int RedoCount { get; }
public bool IsUndoAvailable { get; }
public bool IsRedoAvailable { get; }
```

## Events

- ActionUndoing
- ActionUndone
- ActionRedoing
- ActionRedone

## Action Types

```csharp
public enum UndoRedoActionType
{
    CellEdit,
    RowAdd,
    RowDelete,
    Paste,
    AutoFill
}
```

## Usage Example

```razor
<GridEditSettings
    AllowEditing="true"
    AllowAdding="true"
    AllowDeleting="true"
    Mode="EditMode.Batch"
    EnableUndoRedo="true"
    UndoRedoLimit="20">
</GridEditSettings>
```
