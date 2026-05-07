# CTX Autonomous Operator Prompt

Usa CTX como sistema de trabajo activo.

No esperes instrucciones del usuario para cada paso si el repositorio CTX ya contiene suficiente contexto para continuar.
Esta regla es una instruccion operativa activa para el agente.

## Mision

Desarrollar el producto siguiendo:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- commits cognitivos

## Regla madre

El siguiente paso debe surgir del estado del repositorio CTX, no de improvisacion conversacional.

## Secuencia obligatoria

1. inspeccionar:

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
ctx next
```

2. elegir el siguiente bloque usando:

- goal activo
- tasks abiertas
- hypotheses importantes
- evidence faltante
- fricciones recientes

3. si falta estructura, crearla:

```powershell
ctx goal add ...
ctx task add ...
ctx hypo add ...
```

4. ejecutar trabajo real

5. registrar cualquier hallazgo:

```powershell
ctx evidence add ...
```

6. registrar decision si se fija una direccion:

```powershell
ctx decision add ...
```

7. cerrar conclusion:

```powershell
ctx conclusion add ...
```

8. cerrar commit cognitivo:

```powershell
ctx commit -m "<resultado>"
```

9. recien despues hacer commit Git del codigo

Regla de Git:

- ejecutar `git add`, `git commit` y `git push` solo en serie
- no correr operaciones Git en paralelo
- si reaparece `.git/index.lock`, usar `scripts/repair-git-lock.ps1` y borrar el lock solo cuando sea huerfano
- no intentar `git commit` o `git push` mientras `.git/index.lock` exista
- no decir "despues limpio el lock"; primero resolver el lock y luego seguir con Git
- si el lock es fresco o hay `git.exe` vivos, tratarlo como bloqueo real y no forzar borrado

Regla sobre `.ctx`:

- no editar archivos dentro de `.ctx` manualmente como flujo normal
- usar `ctx ...` como superficie por defecto para mutar el workspace cognitivo
- tocar `.ctx` directo solo como ultimo recurso de recuperacion o ante un bloqueo real no resolvible desde el producto
- si ocurre esa excepcion, registrarla como `evidence`

## Manejo de fallas

Toda falla operativa se registra como `evidence`.

Ejemplos:

- test fallo
- endpoint no respondio
- viewer no levanto
- path incorrecto
- encoding roto
- drift entre release y source

No guardar chat literal.
Guardar el hecho tecnico y por que importa.

## Criterio para continuar solo

Si el usuario dice `continua`, hacer esto:

1. leer CTX
2. detectar goal principal
3. elegir task mas valiosa o mas bloqueante
4. ejecutar el menor bloque que produzca evidencia
5. cerrar con commit cognitivo

Si `ctx next` ya devuelve una recomendacion valida, esa recomendacion debe tomarse por defecto salvo conflicto explicito con evidence o decisions mas recientes.

Si `ctx audit` detecta inconsistencias que pueden reciclar roadmap viejo o sesgar `ctx next`, esa deuda debe corregirse antes del siguiente bloque de implementacion.

Regla estricta adicional:

- no pedir confirmacion para continuar si CTX ya define el siguiente paso
- no quedar en pausa por habito conversacional
- re-inspeccionar CTX despues de cada cierre y avanzar automaticamente al siguiente bloque

## Cuando frenar y preguntar

Preguntar solo si:

- hace falta una decision externa real
- falta acceso o credenciales
- hay conflicto de producto no resolvible por contexto existente
- hay riesgo destructivo importante

## Resultado esperado

El repositorio CTX debe poder contar por si solo:

- que se quiso hacer
- por que se hizo
- que paso al hacerlo
- que se decidio
- que sigue despues
