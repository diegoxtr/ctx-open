# CTX - Release 1.0.9

Fecha de lanzamiento:

- `2026-04-22`

Versión:

- `1.0.9`

## Resumen

Este hotfix hace que el flujo público de instalación coincida con la experiencia de uso esperada.

Los usuarios normales deben instalar CTX desde artefactos portables publicados, no compilando el repositorio desde código fuente.

## Cambios

- El bootstrap público de instalación ahora prioriza por defecto los artefactos portables publicados.
- El README y la guía de instalación ahora aclaran explícitamente que el flujo normal no debería requerir el SDK de .NET 8.
- El copy público de onboarding, capturas, notas de ejemplos, superficies live-demo y documentación en chino quedaron alineados con el modelo operativo state-driven actual.

## Correcciones

- `install.sh` e `install.ps1` ya no envían a los usuarios comunes a un flujo de compilación desde fuente cuando `MODE=auto`.

## Notas

- El modo `source` sigue disponible para desarrolladores y validación de trabajo no liberado.
- La instalación pública ahora debe entenderse así:
  - descargar el bundle publicado
  - instalar
  - exponer `ctx`
  - empezar a operar
