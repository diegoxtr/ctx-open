# CTX - Release 1.0.5

Fecha de release: 2026-04-16

Version:

- `1.0.5`

Resumen:

- Patch estable para CTX 1.0.
- Congela la primera linea publica con instalador/bootstrap orientado a releases.

Highlights:

- CTX ahora publica un bootstrap unico para Windows (`install.ps1`) y Linux/macOS (`install.sh`) con deteccion de install, update y repair.
- El instalador ahora resuelve la ultima version publicada y el asset portable correcto desde GitHub Releases en lugar de depender de un numero de version hardcodeado como fuente publica de verdad.
- El helper de la CLI ahora carga un prompt dinamico desde `prompts/CTX_HELPER_PROMPT.md` para reanclar al operador o agente en el repo o install root activo antes de planificar.
- El empaquetado de distribucion ahora copia el helper prompt y los docs canonicos de CTX dentro del bundle para que la instalacion tenga su propia guia operativa.
- La documentacion publica ahora describe la estrategia de release branches y el flujo de instalacion publico sin filtrar rutas privadas ni URLs temporales del live demo.
