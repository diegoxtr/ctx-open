# Reglas de negocio del almacen

Este documento esta escrito como contexto de producto. No describe componentes tecnicos: describe decisiones del negocio que el sistema debe respetar.

## Regla 1: yerba con descuento antes de las 18:00

La yerba con descuento solo se puede vender hasta las 18:00. Despues de esa hora el proveedor no reconoce la bonificacion y el almacen pierde margen.

La aplicacion debe bloquear la compra si el carrito contiene yerba con descuento y la hora actual es 18:00 o mas tarde.

## Regla 2: fiado incompatible con energizantes

El fiado se permite para clientes conocidos, pero no cuando el pedido incluye bebidas energizantes. La razon no es tecnica: el duenio detecto que esos pedidos suelen quedar impagos.

La aplicacion debe bloquear el fiado si el carrito contiene energizantes.

## Regla 3: promo de pan y queso cancelada por lluvia

La promo de pan y queso existe para mover stock fresco. Si llueve, se cancela porque baja la circulacion del barrio y el descuento deja de cumplir su objetivo.

La aplicacion debe quitar la promo si el clima esta marcado como lluvia.

## Regla 4: envio gratis con cupon capicua

Un cupon capicua como `1221` o `3443` habilita envio gratis, pero solo si el ticket supera 5000. Si el monto es menor, el costo logistico supera el margen.

La aplicacion debe aplicar envio gratis solo cuando se cumplen ambas condiciones.

## Regla 5: hielo y velas generan advertencia

Hielo y velas pueden comprarse juntos, pero se preparan en zonas distintas del almacen. No debe bloquearse la compra; debe mostrarse una advertencia de preparacion separada.

## Expectativa para CTX

Al correr bootstrap sobre esta carpeta, CTX deberia detectar que las reglas no son detalles visuales. Son restricciones de negocio que pueden convertirse en hipotesis, evidencia, decision y conclusion.

