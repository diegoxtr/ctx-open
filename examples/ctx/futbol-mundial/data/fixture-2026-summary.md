# Fixture Base - Mundial 2026

## Corte de informacion

Fecha de relevamiento: 10 de abril de 2026.

## Hechos aceptados

- El proximo Mundial masculino es la FIFA World Cup 2026.
- Se juega en Canada, Mexico y Estados Unidos.
- FIFA publico una pagina oficial con el calendario completo, sedes y resultados.
- Esa publicacion fue fechada el 31 de marzo de 2026.
- FIFA informo el 25 de marzo de 2026 que el torneo comienza en junio de 2026 y termina en julio de 2026.

## Decision de trabajo

Para este repositorio, el fixture debe construirse a partir de la fuente oficial de FIFA y no desde compilaciones manuales o fuentes secundarias.

## Alcance del entregable inicial

Este repositorio deja lista la trazabilidad para seguir con alguna de estas dos direcciones:

1. Documentar el fixture en Markdown para uso editorial.
2. Exportar el fixture oficial a JSON o CSV para uso en una app o sitio.

## Estado del entregable web

La version actual del repo ya incluye una pagina estatica en HTML, CSS y JavaScript.

- muestra una carga inicial de partidos y slots oficiales;
- permite filtrar por fase y sede anfitriona;
- evita inventar cruces cuando la carga actual solo confirma un slot del calendario.

## Proximos pasos recomendados

1. Extraer la tabla oficial de partidos desde FIFA.
2. Normalizar campos como fase, fecha, ciudad, estadio y equipos.
3. Versionar esa primera carga en el repo.
4. Registrar nueva evidencia en `ctx` cuando se cierre la ingesta completa.
