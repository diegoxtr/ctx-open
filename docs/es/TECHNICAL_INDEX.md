# Indice Tecnico de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento centraliza la documentacion tecnica y operativa disponible en el repositorio.

Su funcion es servir como punto de entrada para:

- desarrollo
- arquitectura
- operacion
- pruebas
- onboarding tecnico

## Mapa de Lectura Rapido

Si queres entender CTX rapido, este es el orden recomendado:

1. [README.md](C:/sources/ctx-open/README.md)
2. [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
3. [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
4. [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
5. [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
6. [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
7. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)

## Documentos por Categoria

## Producto y Alcance

- [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
  Resume objetivo, alcance, etapas de desarrollo, backlog y camino a V1.

- [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
  Define modulos, requisitos, criterios de aceptacion y definicion de terminado de V1.

- [RELEASE_1_0_5.md](../RELEASE_1_0_5.md)
  Resume la linea base funcional de la release estable actual.

- [archive/README.md](C:/sources/ctx-open/docs/es/archive/README.md)
  Reune releases internas historicas fuera del flujo principal de lectura.

## Arquitectura y DiseÃ±o

- [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
  Describe capas, responsabilidades, dependencias y flujos end-to-end.

- [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
  Documenta entidades, IDs fuertes, estados, relaciones y reglas del dominio.

- [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
  Explica la estructura fisica de `.ctx/`, archivos base, directorios e invariantes.

- [CTX_SPECIFICATION_V1.md](C:/sources/ctx-open/docs/es/CTX_SPECIFICATION_V1.md)
  Especificacion minima de CTX v1: como se guarda, se versiona y se estructura el contexto.

- [COGNITIVE_GRAPH_AND_LINEAGE.md](C:/sources/ctx-open/docs/COGNITIVE_GRAPH_AND_LINEAGE.md)
  Define la proyeccion relacional del conocimiento, sus relaciones y su roadmap de visualizacion.

- [COGNITIVE_THREAD_RECONSTRUCTION.md](C:/sources/ctx-open/docs/COGNITIVE_THREAD_RECONSTRUCTION.md)
  Define el modelo canonico para reconstruir el hilo cognitivo completo desde artefactos estructurados, commits y branches.

- [WORK_MODEL_AND_PRIORITIZATION.md](C:/sources/ctx-open/docs/WORK_MODEL_AND_PRIORITIZATION.md)
  Define la taxonomia canonica entre issue, gap, task, subtask, duplicate y blocker, ademas de la prioridad por cercania dentro de CTX.

- [OPERATIONAL_RUNBOOKS.md](C:/sources/ctx-open/docs/OPERATIONAL_RUNBOOKS.md)
  Define el diseno compacto de conocimiento operativo recurrente, su inyeccion en packets y el manejo de overflow.

- [WORK_TOPOLOGY_AND_EXECUTION_SCORING.md](C:/sources/ctx-open/docs/es/WORK_TOPOLOGY_AND_EXECUTION_SCORING.md)
  Explica como clasificar trabajo nuevo, evitar duplicacion y combinar prioridad con cercania operativa.

- [CTX_GOAL_FLOW_DIAGRAM.md](C:/sources/ctx-open/docs/CTX_GOAL_FLOW_DIAGRAM.md)
  Muestra un flujo claro y un ejemplo de comandos CTX para resolver un objetivo y construir el mapa en `.ctx`.

## Operacion y Uso

- [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
  Referencia completa de todos los comandos CLI implementados hoy.

- [COMMAND_ADOPTION_AND_COVERAGE.md](C:/sources/ctx-open/docs/COMMAND_ADOPTION_AND_COVERAGE.md)
  Resume que comandos se usan de verdad, cuales siguen frios y en que orden conviene adoptar las superficies de menor cobertura.

- [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)
  Onboarding operativo para instalar, ejecutar y usar CTX por primera vez.

- [PILOT_TESTING_GUIDE.md](C:/sources/ctx-open/docs/PILOT_TESTING_GUIDE.md)
  Guia para ejecutar pilotos controlados con testers.

- [CTX_VIEWER_GUIDE.md](C:/sources/ctx-open/docs/CTX_VIEWER_GUIDE.md)
  Explica paso a paso como interpretar el viewer, su timeline, ramas y paneles.

## Prompts de Operacion

- [CTX_BASE_PROMPT.md](C:/sources/ctx-open/prompts/CTX_BASE_PROMPT.md)
  Plantilla base para operar CTX con nuevas herramientas, definiendo objetivo, alcance y adaptacion.

- [CTX_AGENT_PROMPT.md](C:/sources/ctx-open/prompts/CTX_AGENT_PROMPT.md)
  Prompt operativo del agente CTX con reglas de continuidad, evidencia y cierre cognitivo.

- [CTX_AUTONOMOUS_OPERATOR_PROMPT.md](C:/sources/ctx-open/prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md)
  Prompt del operador autonomo con secuencia estricta de inspeccion, ejecucion y cierre.

## Raiz del Repositorio

- [README.md](C:/sources/ctx-open/README.md)
  Entrada general del proyecto.

- [CHANGELOG.md](C:/sources/ctx-open/CHANGELOG.md)
  Historial resumido de cambios del producto.

- [LICENSE](C:/sources/ctx-open/LICENSE)
  Licencia source-available del software.

- [COPYRIGHT.md](C:/sources/ctx-open/COPYRIGHT.md)
  Aviso de copyright del sistema y sus conceptos.

- [TRADEMARK.md](C:/sources/ctx-open/TRADEMARK.md)
  Regla de uso de la marca CTX.

- [CONTRIBUTOR_ASSIGNMENT.md](C:/sources/ctx-open/CONTRIBUTOR_ASSIGNMENT.md)
  Terminos de cesion de contribuciones.

- [NOTICE](C:/sources/ctx-open/NOTICE)
  Avisos complementarios del repositorio.

## Scripts de Validacion

- [run-smoke-test.ps1](C:/sources/ctx-open/scripts/run-smoke-test.ps1)
  Ejecuta un flujo funcional reproducible de validacion general.

- [run-merge-conflict-demo.ps1](C:/sources/ctx-open/scripts/run-merge-conflict-demo.ps1)
  Ejecuta una demo reproducible de branch, merge y conflicto cognitivo.

- [publish-local.ps1](C:/sources/ctx-open/scripts/publish-local.ps1)
  Publica la instalacion local en `C:\ctx` preservando el workspace versionado.

## Rutas Recomendadas por Perfil

### Si sos desarrollador

Lee en este orden:

1. [README.md](C:/sources/ctx-open/README.md)
2. [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
3. [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
4. [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
5. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)

### Si sos tester tecnico

Lee en este orden:

1. [README.md](C:/sources/ctx-open/README.md)
2. [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)
3. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
4. [PILOT_TESTING_GUIDE.md](C:/sources/ctx-open/docs/PILOT_TESTING_GUIDE.md)

### Si queres entender alcance de negocio y V1

Lee en este orden:

1. [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
2. [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
3. [RELEASE_1_0_0.md](C:/sources/ctx-open/docs/RELEASE_1_0_0.md)

## Estado Actual de Cobertura Documental

Hoy el repositorio ya documenta:

- objetivo del producto
- alcance de V1
- modelo de dominio
- arquitectura tecnica
- estructura de persistencia
- comandos CLI
- instalacion
- viewer
- piloto
- release estable
- propiedad intelectual y contribuciones
- grafo cognitivo
- reconstruccion formal del hilo cognitivo

## Proximos Documentos Utiles

Documentacion que todavia seria valiosa agregar:

- ADRs de decisiones tecnicas importantes
- guia de resolucion manual de conflictos cognitivos
- guia para agregar un nuevo provider
- guia de troubleshooting operativo
- roadmap posterior a V1

