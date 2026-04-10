# CTX - Documento de Objetivo, Alcance y Plan hacia V1
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## 1. Proposito

Este documento define:

- el objetivo del producto CTX;
- el alcance funcional de la primera version utilizable;
- la posicion de propiedad intelectual del proyecto;
- las etapas de desarrollo necesarias para llegar a una V1;
- el plan inicial para empezar a probar la solucion con usuarios reales.

Su objetivo es alinear producto, arquitectura, implementacion y validacion.

## 2. Vision del producto

CTX es un sistema de control de versiones cognitivas orientado a trabajo con razonamiento estructurado. En lugar de versionar conversaciones, versiona artefactos cognitivos explicitos:

- contexto;
- objetivos;
- tareas;
- hipotesis;
- decisiones;
- evidencia;
- conclusiones;
- ejecuciones de IA;
- evolucion del razonamiento.

La vision es que CTX permita trabajar con sistemas de IA de manera reproducible, auditable y menos costosa que un enfoque basado en historiales conversacionales opacos.

## 3. Objetivo de negocio y producto

### Objetivo principal

Construir una base comercializable de CTX capaz de:

- iniciar un repositorio cognitivo local;
- registrar artefactos estructurados;
- generar paquetes de contexto optimizados;
- ejecutar corridas con proveedores de IA;
- persistir commits cognitivos reproducibles;
- comparar cambios entre estados;
- trabajar con ramas y merges;
- detectar conflictos cognitivos;
- permitir observabilidad operativa para pruebas reales.

### Resultado esperado de V1

Una version usable por un grupo reducido de testers tecnicos que permita validar:

- utilidad real del modelo de trabajo;
- claridad de los artefactos cognitivos;
- trazabilidad de decisiones;
- reduccion de iteraciones redundantes;
- viabilidad de uso con proveedores reales;
- calidad del flujo CLI.

## 4. Problema que resuelve

CTX busca resolver estos problemas:

- perdida de contexto entre sesiones de trabajo con IA;
- dificultad para saber por que se tomo una decision;
- duplicacion de razonamiento ya realizado;
- ausencia de trazabilidad entre hipotesis, evidencia y conclusiones;
- imposibilidad de reproducir un estado mental o cognitivo de trabajo;
- costo excesivo por reenviar contexto irrelevante;
- ausencia de mecanismos de branching y merge para razonamiento estructurado.

## 5. Alcance del producto

## 5.1 Alcance funcional incluido para V1

La V1 debe incluir como minimo:

- repositorio local `.ctx/`;
- CLI funcional;
- modelo de dominio estructurado;
- commits cognitivos reproducibles;
- diff entre estados cognitivos;
- ramas, checkout y merge;
- deteccion de conflictos cognitivos;
- construccion de `ContextPacket`;
- ejecucion de `Run` con proveedores intercambiables;
- metrica basica de costo y uso;
- inspeccion de goals, tasks, hypotheses, decisions, evidence, conclusions;
- inspeccion de runs, packets, providers y metrics;
- tests automatizados del flujo central.

## 5.2 Alcance fuera de V1

No forma parte de la V1:

- UI web o desktop;
- multiusuario en red;
- sincronizacion remota;
- permisos y seguridad avanzada por roles;
- autenticacion enterprise;
- marketplace de providers o plugins;
- motor semantico avanzado con indexacion compleja;
- optimizacion de prompts por aprendizaje automatico;
- dashboards visuales completos;
- integraciones IDE profundas;
- servidor compartido.

## 6. Propiedad intelectual

## 6.1 Titularidad

La propiedad intelectual del software CTX, su arquitectura, codigo fuente, documentacion, mejoras, derivados y materiales asociados pertenece a Diego Mariano Verrastro, segun los terminos establecidos en:

- `LICENSE`
- `CONTRIBUTOR_ASSIGNMENT.md`
- `NOTICE`

## 6.2 Colaboracion de terceros

Se permite colaboracion externa, pero con estas condiciones:

- los aportes deben realizarse bajo los terminos definidos por el proyecto;
- la titularidad del proyecto no se transfiere;
- los aportes quedan sujetos a cesion o licencia exclusiva a favor del titular;
- la explotacion economica y comercial permanece bajo control del titular.

## 6.3 Nota operativa

Para una explotacion comercial robusta, cualquier colaborador externo relevante deberia aceptar formalmente un acuerdo escrito consistente con `CONTRIBUTOR_ASSIGNMENT.md`.

## 7. Usuarios objetivo de V1

Los primeros usuarios a validar son:

- arquitectos de software;
- ingenieros de plataforma;
- equipos que trabajan con LLMs de forma iterativa;
- investigadores tecnicos;
- personas que necesitan trazabilidad de decisiones;
- desarrolladores que quieren reducir costo y ruido conversacional.

## 8. Propuesta de valor de V1

La V1 debe demostrar:

- que el razonamiento estructurado es mas reutilizable que una conversacion cruda;
- que el historial cognitivo puede versionarse con reglas claras;
- que el costo de iteracion puede medirse;
- que los conflictos de razonamiento pueden detectarse como entidades explicitas;
- que un CLI puede ser suficiente para pruebas tempranas de producto.

## 9. Estado actual del proyecto

Hoy el repositorio ya cuenta con una base funcional que incluye:

- solucion .NET 8;
- arquitectura por capas;
- dominio tipado;
- persistencia filesystem en `.ctx/`;
- CLI con operaciones principales;
- providers OpenAI y Anthropic con fallback offline;
- merge con conflictos cognitivos;
- tests automatizados del core.

Esto significa que la etapa actual no es ideacion, sino consolidacion hacia una V1 testeable.

## 10. Etapas para llegar a V1

## Etapa 1 - Fundacion estable

Objetivo:
- consolidar el nucleo tecnico para evitar retrabajo estructural.

Entregables:
- dominio estable;
- persistencia consistente;
- cobertura de tests del flujo central;
- comandos CLI esenciales;
- manejo de errores basico consistente.

Estado:
- mayormente avanzado.

## Etapa 2 - Completitud funcional de repositorio cognitivo

Objetivo:
- asegurar que todos los artefactos cognitivos importantes puedan crearse, inspeccionarse, versionarse y compararse.

Tareas:
- completar comandos faltantes de lectura y navegacion;
- mejorar mensajes y errores de CLI;
- reforzar diffs por tipo de artefacto;
- mejorar merge con resolucion guiada de conflictos;
- garantizar trazabilidad completa entre entidades.

Criterio de salida:
- un usuario tecnico puede operar CTX de punta a punta sin editar archivos manualmente.

## Etapa 3 - Calidad operativa para piloto

Objetivo:
- preparar la herramienta para uso real de baja escala.

Tareas:
- endurecer validaciones de entrada;
- mejorar serializacion y compatibilidad de archivos;
- agregar exportes diagnosticos;
- agregar logs operativos claros;
- definir estrategia de versionado de repositorio `.ctx/`;
- asegurar reproducibilidad de commits y packets.

Criterio de salida:
- una prueba piloto puede correrse sin asistencia constante del desarrollador.

## Etapa 4 - Integracion real con proveedores

Objetivo:
- pasar de simulacion/controlado a uso real con cuentas y modelos productivos.

Tareas:
- endurecer adaptadores de OpenAI y Anthropic;
- soportar configuracion por archivo y variables de entorno;
- definir manejo de errores y retries;
- registrar costos de manera mas precisa;
- permitir politicas de seleccion de modelo.

Criterio de salida:
- los testers pueden correr casos reales con providers y medir costo/beneficio.

## Etapa 5 - V1 piloto

Objetivo:
- liberar una version de prueba controlada.

Entregables:
- instalacion clara;
- guia de uso inicial;
- escenarios de prueba;
- comandos estables;
- changelog de version;
- feedback loop documentado.

Criterio de salida:
- al menos 3 a 5 usuarios tecnicos pueden completar flujos reales y devolver feedback accionable.

## 11. Backlog de desarrollo hacia V1

### Prioridad alta

- comando `log` con mejor navegacion y filtros;
- resolucion de conflictos cognitivos asistida;
- configuracion de providers por archivo y secretos;
- manejo de version de repositorio y migraciones;
- mejora del diff entre commits con salida mas legible;
- pruebas de integracion CLI end-to-end;
- command help mas completo;
- validaciones de referencias cruzadas;
- export/import de repositorios cognitivos;
- documentacion de flujo de trabajo.

### Prioridad media

- indices de busqueda en `.ctx/index`;
- heuristicas mejores de `ContextBuilder`;
- soporte para tags y filtros temporales;
- snapshots de packets asociados a commits;
- resumen automatico de historial cognitivo;
- comandos de estadisticas por repositorio.

### Prioridad baja para despues de V1

- interfaz grafica;
- sincronizacion remota;
- colaboracion concurrente multiusuario;
- integraciones editor/IDE;
- server mode;
- paneles visuales.

## 12. Definicion de V1

Se considera que existe una V1 cuando se cumplan estos criterios:

- el repositorio puede inicializarse y usarse sin editar JSON manualmente;
- todos los artefactos cognitivos principales pueden crearse, listarse y consultarse;
- puede construirse contexto y ejecutar runs;
- los commits son reproducibles;
- el diff entre estados es util para usuarios;
- el merge identifica conflictos cognitivos;
- existe metrica basica de uso;
- los tests automatizados cubren el core;
- existe documentacion suficiente para onboarding tecnico;
- al menos un piloto controlado puede ejecutarse.

## 13. Plan de prueba inicial

## 13.1 Objetivo del piloto

Validar si CTX mejora la calidad y eficiencia del trabajo con IA en escenarios reales de analisis y desarrollo.

## 13.2 Perfil de tester

- tecnico;
- familiarizado con CLI;
- capaz de formular objetivos, tareas, hipotesis y decisiones;
- dispuesto a registrar feedback estructurado.

## 13.3 Escenarios de prueba sugeridos

- diseno de arquitectura de un modulo nuevo;
- evaluacion de alternativas tecnicas;
- investigacion de bug complejo;
- analisis de decisiones con evidencia;
- iteracion de un caso de producto con varias hipotesis.

## 13.4 Indicadores a observar

- tiempo para llegar a una conclusion util;
- cantidad de iteraciones repetidas;
- cantidad de contexto reenviado;
- claridad de decisiones registradas;
- facilidad de recuperar estado previo;
- comprension de diffs y merges;
- costo de tokens o ACUs.

## 13.5 Resultado esperado del piloto

El piloto debe responder:

- si el modelo mental de CTX es entendible;
- si el costo de uso operativo es aceptable;
- si el CLI es suficiente;
- si la estructura de artefactos agrega valor real;
- si conviene avanzar a una V1 publica restringida o a una beta privada.

## 14. Riesgos principales

- exceso de complejidad en el modelo de dominio;
- friccion de uso en CLI;
- dificultad para que usuarios piensen en artefactos estructurados;
- costo real mayor al esperado en providers;
- merges cognitivos poco intuitivos;
- validacion insuficiente de propiedad intelectual en contribuciones;
- falta de casos reales de prueba.

## 15. Decision operativa recomendada

La recomendacion para avanzar es:

1. consolidar el backlog de prioridad alta;
2. preparar 3 escenarios de prueba reales;
3. correr un piloto tecnico cerrado;
4. medir costo, trazabilidad y utilidad;
5. usar ese feedback para congelar alcance de V1.

## 16. Proximo paso inmediato recomendado

El proximo paso concreto para el proyecto es:

- cerrar la experiencia CLI de uso real;
- mejorar la inspeccion y resolucion de conflictos;
- preparar una guia corta de prueba;
- ejecutar un piloto interno de V1 candidata.

Ese paso transforma CTX de una base tecnica prometedora en una solucion realmente evaluable.

