# Usar CTX Para Construir CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Usar CTX como repositorio cognitivo oficial para la evolucion del propio producto.

## Que debe guardar CTX en este proceso

- objetivos de producto
- backlog estructurado
- hipotesis de valor y de arquitectura
- evidencia de pruebas reales
- decisiones tecnicas y de producto
- conclusiones por iteracion
- snapshots cognitivos por commit

## Que falta hoy

- comparacion visual de commits desde la UI
- navegacion padre/hijo mas rica en timeline
- mejor captura de relaciones entre runs y artefactos
- modelo explicito de ponderacion de hipotesis
- flujo operativo mas cerrado para usar CTX todos los dias

## Workflow operativo recomendado

### 1. Abrir ciclo

```powershell
ctx status
ctx graph summary
ctx log
```

### 2. Crear objetivo de iteracion

Ejemplos:

- endurecer viewer
- mejorar diff cognitivo
- definir scoring de hipotesis
- preparar piloto V1

### 3. Bajar a tareas e hipotesis

Cada iteracion debe tener:

- 1 objetivo claro
- 2 a 5 tareas
- 1 a 3 hipotesis relevantes

### 4. Registrar evidencia al trabajar

Ejemplos:

- resultado de prueba
- limitacion encontrada
- feedback del viewer
- benchmark
- friccion de uso

### 5. Cerrar con decision y conclusion

No dejar commits cognitivos sin estas preguntas resueltas:

- que aprendimos
- que cambiamos
- por que lo cambiamos
- que queda abierto

### 6. Commit cognitivo

Cada bloque util debe terminar en:

```powershell
ctx commit -m "<resultado del bloque>"
```

### 7. Continuidad autonoma

Si el operador o modelo recibe solo `continua`, debe:

1. releer `ctx status`, `ctx graph summary` y `ctx log`
2. elegir el goal activo dominante
3. seleccionar la task mas bloqueante o de mayor valor
4. producir evidencia real
5. registrar conclusion
6. cerrar commit cognitivo

No debe esperar direccion manual si el repositorio CTX ya deja claro que sigue.

## Estructura sugerida del trabajo cognitivo

### Objetivos

- V1 local usable
- viewer operativo
- metodo de captura cognitiva
- piloto de validacion

### Tareas

- mejorar UI
- empaquetar release
- instalar CLI
- probar demos
- definir scoring

### Hipotesis

- la trazabilidad visual reduce retrabajo
- el viewer acelera comprension del historial
- un scoring simple mejora la calidad de decisiones

## Workspace recomendado

Usar un repositorio CTX especifico para el desarrollo del producto:

- `C:\ctx\workspace\ctx-self-host`

## Resultado esperado

Si CTX sirve para construir CTX, deberia poder demostrar:

- continuidad entre iteraciones
- menos perdida de contexto
- decisiones mas explicitas
- mejor calidad de backlog
- trazabilidad de por que el producto tomo su forma actual
- continuidad autonoma del trabajo sin depender del chat

