# CTX - Release 1.0.6

Fecha de release: 2026-04-17

Version:

- `1.0.6`

Resumen:

- Patch estable para CTX 1.0.
- Congela la primera linea publica segura donde conviven bootstrap indexing, branch-like hypothesis semantics y surfaces de viewer orientadas a interpretaciones.

Highlights:

- CTX ahora expone `ctx bootstrap map` y `ctx bootstrap apply` de forma publica para que agentes construyan hilos cognitivos provisionales desde articulos y proyectos antes de promoverlos a trabajo durable en CTX.
- El modelo de dominio y la CLI publicos ahora soportan semantica branch-like para hipotesis, incluyendo branch state, branch role, lineage grouping, relaciones entre hipotesis, merge, supersede y evidence share.
- El viewer publico ahora incluye la tab `Interpretations` y el toggle opcional `Show interpretation relations` para mantener hipotesis competidoras visibles sin degradar el trace graph por defecto.
- El repo publico ahora publica packs sanitizados del ejemplo agricola para `v1`, `v2`, `v3` y `v4`, con planes y notas de testing real, sin copiar workspaces `.ctx` privados.
- El helper y la documentacion tecnica publica ahora quedan alineados con la frontera entre repo privado y repo publico para mantener una operacion publica segura y consistente.
