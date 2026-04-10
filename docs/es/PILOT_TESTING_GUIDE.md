# CTX - Guia de Prueba Piloto
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## 1. Objetivo

Esta guia define como ejecutar una prueba piloto inicial de CTX para validar si la solucion ya esta lo suficientemente madura como para evolucionar hacia una V1 testeable con usuarios tecnicos.

La meta no es solo verificar que el software funcione, sino comprobar si:

- el modelo de trabajo es entendible;
- el flujo operativo aporta valor real;
- la estructura cognitiva mejora el uso de IA;
- la herramienta reduce retrabajo;
- el costo de iteracion es razonable.

## 2. Alcance del piloto

La prueba piloto debe centrarse en:

- uso local por CLI;
- casos tecnicos reales o muy cercanos a casos reales;
- registro estructurado de objetivos, tareas, hipotesis, evidencia, decisiones y conclusiones;
- uso de commits cognitivos;
- generacion de contexto;
- ejecucion de runs;
- revision de diffs y merges;
- inspeccion de metrics, packets y runs.

No debe incluir aun:

- integraciones remotas;
- uso multiusuario real;
- interfaces graficas;
- escenarios enterprise;
- automatizaciones externas complejas.

## 3. Perfil recomendado del tester

El tester ideal para este piloto debe ser:

- tecnico;
- comodo trabajando con CLI;
- capaz de estructurar razonamiento en pasos;
- capaz de explicar por que una decision fue buena o mala;
- dispuesto a registrar feedback claro.

Idealmente:

- arquitecto;
- desarrollador senior;
- ingeniero de plataforma;
- investigador tecnico;
- usuario frecuente de LLMs.

## 4. Preparacion del entorno

## 4.1 Requisitos

- .NET SDK 8 instalado;
- repositorio CTX compilando;
- acceso a consola;
- opcionalmente claves de proveedor si se quiere probar ejecucion real:
  - `OPENAI_API_KEY`
  - `ANTHROPIC_API_KEY`

Si no hay claves, el sistema sigue siendo testeable mediante fallback offline.

## 4.2 Verificacion inicial

Ejecutar:

```powershell
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

Resultado esperado:

- compilacion exitosa;
- tests exitosos;
- sin errores criticos de entorno.

## 4.3 Preparar un repositorio piloto

Dentro de una carpeta de trabajo:

```powershell
dotnet run --project .\Ctx.Cli -- init --name CTX-PILOTO --description "Piloto inicial de validacion"
dotnet run --project .\Ctx.Cli -- status
```

Resultado esperado:

- se crea `.ctx/`;
- `status` informa rama `main`;
- no hay errores de inicializacion.

## 5. Objetivos del piloto

Durante la prueba se debe responder:

1. El usuario entiende como modelar el problema?
2. El flujo cognitivo estructurado ayuda mas que una conversacion libre?
3. La herramienta facilita retomar trabajo anterior?
4. Los commits y diffs resultan utiles?
5. Los merges y conflictos son interpretables?
6. El costo de contexto y ejecucion es aceptable?
7. El usuario volveria a usar CTX para un caso real?

## 6. Escenarios de prueba recomendados

## Escenario 1 - Analisis de arquitectura

Objetivo:
- evaluar una decision de arquitectura con hipotesis y evidencia.

Ejemplo:
- definir como disenar un modulo nuevo;
- comparar alternativas;
- registrar evidencia;
- tomar una decision;
- cerrar con conclusion.

## Escenario 2 - Investigacion tecnica

Objetivo:
- usar CTX para estructurar una investigacion de causa raiz.

Ejemplo:
- bug complejo;
- degradacion de performance;
- problema de integracion;
- incertidumbre tecnica con varias hipotesis.

## Escenario 3 - Iteracion guiada con IA

Objetivo:
- medir si CTX reduce retrabajo al ejecutar varias corridas de IA.

Ejemplo:
- formular objetivo;
- crear tareas e hipotesis;
- construir packet;
- ejecutar `run`;
- revisar output;
- registrar decision;
- repetir ciclo con cambios controlados.

## 7. Flujo recomendado de prueba

## Paso 1 - Crear el objetivo

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Evaluar arquitectura del modulo X" --description "Definir alternativa principal"
```

## Paso 2 - Crear tareas

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Analizar opcion A" --description "Ventajas y riesgos"
dotnet run --project .\Ctx.Cli -- task add --title "Analizar opcion B" --description "Ventajas y riesgos"
```

## Paso 3 - Registrar hipotesis

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "La opcion A reduce complejidad operativa" --rationale "Menos componentes"
dotnet run --project .\Ctx.Cli -- hypo add --statement "La opcion B escala mejor a futuro" --rationale "Mayor flexibilidad"
```

## Paso 4 - Registrar evidencia

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Benchmark inicial" --summary "A muestra menor latencia" --source "prueba local" --kind Benchmark --supports hypothesis:<hypothesisId>
```

## Paso 5 - Tomar decisiones

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adoptar opcion A para piloto" --rationale "Menor complejidad y evidencia inicial favorable" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

## Paso 6 - Registrar conclusion

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Se avanza con A para validar mas rapido" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

## Paso 7 - Ejecutar corrida con IA

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --purpose "Revisar la decision tomada y proponer riesgos"
```

## Paso 8 - Generar commit cognitivo

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "piloto escenario arquitectura"
```

## Paso 9 - Inspeccionar resultados

```powershell
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- metrics show
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
```

## 8. Checklist operativo del tester

Durante la prueba, el tester debe marcar:

- pude inicializar el repositorio sin ayuda;
- entendi como crear goals, tasks e hypotheses;
- entendi como conectar evidencia con hipotesis o decisiones;
- me resulto claro registrar decisiones y conclusiones;
- el output de `status`, `log`, `diff` y `metrics` fue entendible;
- el packet generado tenia contexto util;
- el resultado del `run` fue reutilizable;
- el commit cognitivo represento bien el estado alcanzado;
- el flujo fue mejor o peor que trabajar con prompts sueltos;
- volveria a usar CTX en un caso real.

## 9. Criterios de evaluacion

## 9.1 Criterios funcionales

Se considera exitoso si:

- el flujo se puede completar sin editar JSON manualmente;
- no aparecen errores bloqueantes;
- los comandos devuelven resultados consistentes;
- los artefactos quedan trazados entre si.

## 9.2 Criterios de usabilidad

Se considera aceptable si:

- el usuario entiende el modelo con poca explicacion;
- la CLI no genera confusion grave;
- los nombres de comandos resultan razonables;
- la estructura obliga a pensar mejor, pero no frena demasiado.

## 9.3 Criterios de valor

Se considera prometedor si:

- el usuario siente que pierde menos contexto;
- puede justificar mejor decisiones;
- puede retomar el trabajo luego de una pausa;
- ve utilidad en los commits cognitivos;
- percibe menor repeticion en el trabajo con IA.

## 10. Indicadores a medir

Registrar para cada escenario:

- tiempo total de ejecucion;
- cantidad de comandos ejecutados;
- cantidad de goals;
- cantidad de tasks;
- cantidad de hypotheses;
- cantidad de evidence items;
- cantidad de decisions;
- cantidad de conclusions;
- cantidad de runs;
- tokens usados;
- ACUs o costo estimado;
- cantidad de iteraciones repetidas;
- cantidad de conflictos cognitivos detectados;
- percepcion subjetiva de utilidad.

## 11. Plantilla de feedback del tester

Usar esta plantilla al finalizar cada escenario:

### Informacion general

- nombre del tester:
- fecha:
- escenario:
- duracion:

### Evaluacion

- problema trabajado:
- objetivo logrado:
- comandos mas utiles:
- comandos confusos:
- que parte del flujo aporto mas valor:
- que parte del flujo genero friccion:
- que informacion falto:
- que comandos o ayudas hacen falta:
- volverias a usar CTX en este tipo de caso: si/no

### Puntuacion sugerida

Calificar de 1 a 5:

- claridad del modelo:
- facilidad de uso:
- valor del contexto estructurado:
- utilidad del commit cognitivo:
- utilidad del diff:
- utilidad del merge:
- utilidad de metrics:
- probabilidad de reuso:

## 12. Decisiones posteriores al piloto

Al terminar el piloto, decidir:

### Avanzar a V1 candidata

Si:

- no hay bloqueos graves;
- los testers entienden el flujo;
- el valor percibido es alto;
- los costos son razonables.

### Mantener en iteracion interna

Si:

- el concepto gusta;
- pero la UX CLI o el modelo todavia generan demasiada friccion.

### Replantear antes de V1

Si:

- el valor percibido es bajo;
- el modelo no se entiende;
- el costo operativo es demasiado alto;
- los artefactos no ayudan a decidir mejor.

## 13. Recomendacion practica

El piloto inicial deberia hacerse con:

- 3 escenarios;
- 3 a 5 testers tecnicos;
- una duracion acotada;
- feedback escrito obligatorio;
- una sesion final de revision.

## 14. Resultado esperado de esta guia

Si esta guia se ejecuta correctamente, al finalizar deberias tener:

- evidencia concreta de utilidad o friccion;
- una lista real de mejoras antes de V1;
- una base objetiva para decidir si CTX ya puede entrar en prueba controlada de producto.

