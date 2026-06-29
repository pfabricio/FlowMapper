### RFC-0005 — Generator Decomposition

Hoje

```
FlowCodeGenerator
```

Proposta

```
FlowCodeGenerator
↓
CodeWriter
↓
PropertyWriter
↓
ConstructorWriter
↓
NamespaceWriter
↓
UsingWriter
```

Evitar uma classe gigantesca.

Status

🟡 Futuro

---
