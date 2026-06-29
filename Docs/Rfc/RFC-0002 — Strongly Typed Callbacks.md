### RFC-0002 — Strongly Typed Callbacks

Hoje:

```csharp
.AfterMap(nameof(CalcularFrete))
```

Proposta:

```csharp
.AfterMap(CalcularFrete)
```

ou

```csharp
.AfterMap((s,d)=>...)
```

### Benefícios

✔ compile-time safety

✔ refactoring automático

✔ IntelliSense

✔ validação do Roslyn

Status:

🟡 Em avaliação

---
