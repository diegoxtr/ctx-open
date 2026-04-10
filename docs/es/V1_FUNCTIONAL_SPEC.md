# CTX - Especificacion Funcional V1
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## 1. Objetivo del documento

Este documento establece la especificacion funcional objetivo para la V1 de CTX.

Su funcion es:

- definir que debe hacer el producto;
- delimitar que queda dentro y fuera de V1;
- establecer criterios de aceptacion;
- servir como referencia de ejecucion tecnica y validacion.

## 2. Descripcion resumida del producto

CTX es un sistema de control de versiones cognitivas que permite registrar, versionar, comparar y reutilizar razonamiento estructurado.

El producto trabaja sobre artefactos cognitivos explicitos:

- contexto;
- goals;
- tasks;
- hypotheses;
- decisions;
- evidence;
- conclusions;
- runs;
- packets;
- commits.

## 3. Objetivo funcional de V1

La V1 debe permitir que un usuario tecnico pueda:

- iniciar un repositorio cognitivo;
- modelar un problema mediante artefactos estructurados;
- construir contexto relevante;
- ejecutar iteraciones con IA;
- guardar snapshots cognitivos reproducibles;
- comparar cambios;
- trabajar con ramas;
- detectar conflictos cognitivos;
- inspeccionar operativamente el estado del repositorio.

## 4. Alcance funcional de V1

## 4.1 Incluido en V1

- repositorio local `.ctx/`;
- persistencia filesystem;
- CLI como interfaz principal;
- providers intercambiables;
- contexto estructurado;
- commits cognitivos;
- diff cognitivo;
- branching y merge;
- metricas basicas;
- tests automatizados del nucleo;
- documentacion operativa base.

## 4.2 Excluido de V1

- UI web;
- sincronizacion remota;
- trabajo colaborativo multiusuario real;
- autenticacion y autorizacion avanzadas;
- dashboards visuales;
- integraciones profundas con IDE;
- servicio cloud;
- control de acceso por workspace;
- busqueda semantica avanzada.

## 5. Modulos funcionales

## Modulo A - Repositorio cognitivo

### Objetivo

Permitir inicializar y mantener un repositorio CTX local con estructura reproducible.

### Requisitos funcionales

- crear `.ctx/` con estructura conocida;
- persistir `version.json`, `config.json`, `project.json`, `HEAD`;
- crear directorios funcionales del repositorio;
- cargar y guardar el estado de trabajo;
- permitir evolucion futura del formato del repositorio.

### Criterios de aceptacion

- `ctx init` crea la estructura completa;
- `ctx status` puede leer el repositorio sin errores;
- el estado persiste entre ejecuciones;
- el formato del repositorio queda identificado por version.

## Modulo B - Modelo de dominio cognitivo

### Objetivo

Representar razonamiento estructurado mediante entidades trazables.

### Requisitos funcionales

- soportar `Project`;
- soportar `Goal`;
- soportar `Task`;
- soportar `Hypothesis`;
- soportar `Decision`;
- soportar `Evidence`;
- soportar `Conclusion`;
- soportar `Run`;
- soportar `ContextCommit`;
- soportar `ContextPacket`;
- soportar `WorkingContext`.

### Criterios de aceptacion

- todas las entidades tienen identificador fuerte;
- todas las entidades tienen estado o trazabilidad relevante;
- las relaciones entre entidades pueden persistirse y recuperarse;
- el modelo permite reconstruir el estado cognitivo completo.

## Modulo C - Gestion de artefactos cognitivos

### Objetivo

Permitir crear, listar y consultar artefactos cognitivos desde la CLI.

### Requisitos funcionales

- crear goals;
- crear tasks;
- crear hypotheses;
- crear evidence;
- crear decisions;
- crear conclusions;
- listar artefactos por tipo;
- mostrar artefactos individuales por ID;
- validar referencias cruzadas.

### Criterios de aceptacion

- cada artefacto puede crearse por comando;
- cada artefacto puede inspeccionarse por `list/show`;
- errores de referencia inexistente son claros;
- la relacion entre hypothesis, evidence, decision y conclusion queda trazada.

## Modulo D - ContextBuilder

### Objetivo

Construir paquetes de contexto optimizados para iteraciones con IA.

### Requisitos funcionales

- seleccionar artefactos relevantes segun objetivo o tarea;
- evitar incluir informacion vacia o irrelevante;
- generar fingerprint de contexto;
- estimar tokens;
- persistir packets.

### Criterios de aceptacion

- `ctx context` devuelve un packet usable;
- el packet contiene secciones coherentes;
- el fingerprint cambia cuando cambia el contenido relevante;
- el packet puede recuperarse desde el repositorio.

## Modulo E - Runs y providers

### Objetivo

Ejecutar corridas de IA con proveedores intercambiables y registrar su resultado.

### Requisitos funcionales

- definir interfaz `IAIProvider`;
- soportar al menos OpenAI;
- soportar al menos Anthropic;
- ejecutar `Run` a partir de un `ContextPacket`;
- registrar artifacts del run;
- registrar uso, costo y duracion;
- permitir fallback offline para prueba controlada.

### Criterios de aceptacion

- `ctx run` ejecuta sin romper el flujo;
- el run queda persistido;
- `run list/show` funciona;
- `metrics show` refleja impacto del run;
- sin claves, el sistema sigue siendo testeable.

## Modulo F - Commits cognitivos

### Objetivo

Persistir snapshots cognitivos reproducibles del estado del repositorio.

### Requisitos funcionales

- construir commit desde `WorkingContext`;
- calcular hash del snapshot;
- registrar parent commit;
- limpiar estado dirty al commitear;
- persistir commit en `.ctx/commits`.

### Criterios de aceptacion

- `ctx commit` genera un commit estable;
- el snapshot queda persistido;
- el `HEAD` se actualiza;
- la rama activa apunta al commit creado.

## Modulo G - Diff cognitivo

### Objetivo

Comparar estados cognitivos y resaltar cambios relevantes.

### Requisitos funcionales

- detectar cambios en tasks;
- detectar cambios en hypotheses;
- detectar cambios en decisions;
- detectar cambios en evidence;
- detectar cambios en conclusions;
- permitir comparar working state o commits.

### Criterios de aceptacion

- `ctx diff` devuelve cambios entendibles;
- los cambios distinguen agregado, modificado y removido;
- el resumen del diff es util para usuarios.

## Modulo H - Branching y merge

### Objetivo

Permitir explorar lineas alternativas de razonamiento.

### Requisitos funcionales

- crear ramas;
- cambiar de rama;
- mergear ramas;
- detectar conflictos cognitivos divergentes;
- devolver resultado interpretable del merge.

### Criterios de aceptacion

- `ctx branch` crea una rama utilizable;
- `ctx checkout` cambia el contexto activo;
- `ctx merge` produce un resultado claro;
- los conflictos se listan explicitamente.

## Modulo I - Observabilidad operativa

### Objetivo

Dar visibilidad del uso del sistema para prueba y analisis.

### Requisitos funcionales

- listar providers;
- listar runs;
- listar packets;
- mostrar metrics;
- mostrar estado de repositorio;
- mostrar historial de commits.

### Criterios de aceptacion

- el usuario puede auditar lo hecho desde la CLI;
- el costo y cantidad de iteraciones puede revisarse;
- el estado operativo es entendible sin abrir archivos manualmente.

## Modulo J - Documentacion de uso

### Objetivo

Permitir onboarding y prueba del producto sin dependencia directa del desarrollador.

### Requisitos funcionales

- documentar instalacion;
- documentar primer uso;
- documentar plan de V1;
- documentar piloto;
- documentar release interna.

### Criterios de aceptacion

- un tester tecnico puede empezar con documentacion escrita;
- existe una guia de piloto;
- existe una guia de instalacion;
- existe una referencia de alcance V1.

## 6. Requisitos no funcionales

La V1 debe cumplir:

- compatibilidad con .NET 8;
- persistencia local reproducible;
- errores comprensibles;
- arquitectura modular;
- bajo acoplamiento entre capas;
- posibilidad de agregar nuevos providers sin rediseno mayor;
- pruebas automatizadas del nucleo;
- salida CLI estructurada.

## 7. Requisitos de calidad

Para considerar aceptable la V1:

- el build debe pasar;
- los tests del core deben pasar;
- no debe requerirse editar JSON manualmente para el flujo principal;
- la CLI debe poder cubrir un caso real simple de punta a punta;
- el producto debe soportar al menos un piloto tecnico controlado.

## 8. Definicion de terminado por modulo

Un modulo se considera terminado si:

- tiene comportamiento implementado;
- tiene validacion minima;
- tiene cobertura automatizada donde corresponda;
- tiene persistencia consistente;
- puede usarse desde la CLI si aplica;
- tiene documentacion basica si impacta al usuario.

## 9. Matriz de prioridad V1

### Critico

- repositorio cognitivo;
- modelo de dominio;
- gestion de artefactos;
- context builder;
- runs;
- commits;
- diff;
- branching y merge;
- observabilidad minima.

### Importante

- documentacion de instalacion;
- documentacion de piloto;
- release interna;
- mejoras de UX CLI.

### Post-V1

- UI visual;
- sincronizacion remota;
- multiusuario;
- integraciones avanzadas;
- analitica avanzada.

## 10. Criterio formal de aceptacion de V1

La V1 se considera aceptable cuando:

- el flujo principal funciona de extremo a extremo;
- existe una release interna estable;
- existe una documentacion suficiente para piloto;
- el producto soporta al menos un conjunto de casos reales acotados;
- los feedbacks iniciales no revelan bloqueos estructurales.

## 11. Resultado esperado

Si esta especificacion se cumple, CTX alcanza una V1 funcional orientada a piloto tecnico, con base suficiente para decidir continuidad comercial y tecnica.

