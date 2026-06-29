### RFC-0003 — Pipeline Refinement

Hoje:

```
Roslyn
↓
FlowBuilder
↓
Generator
```

Proposta

```
Roslyn
↓
MapperDefinition
↓
FlowBuilder
↓
Validator
↓
Generator
```

Separar descoberta da construção do Flow.

Status

🟡 Futuro

---
