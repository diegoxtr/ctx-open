# Caso operativo: almacen barrial

Hipotesis principal: el sistema del almacen debe separar bloqueo de compra, ajuste economico y advertencia logistica para proteger margen sin perder ventas validas.

Hipotesis secundaria: la combinacion de fiado y energizantes representa riesgo comercial y debe bloquear el pedido.

Hipotesis secundaria: hielo y velas no deben bloquear la compra; deben generar una advertencia logistica porque se preparan en zonas separadas.

## Problema central

El sistema debe distinguir tres tipos de reglas:

- reglas que bloquean la compra
- reglas que cambian descuentos, envio o margen
- reglas que solo generan advertencias operativas

## Regla 1: yerba con descuento

La yerba con descuento no debe venderse desde las 18:00.

Razon de negocio: despues de esa hora el proveedor no reconoce la bonificacion y el almacen pierde margen.

Hipotesis de negocio: la restriccion horaria de la yerba protege margen y debe bloquear el pedido, no solo mostrar un aviso.

## Regla 2: fiado con energizantes

El fiado esta permitido para clientes conocidos, salvo cuando el carrito incluye bebidas energizantes.

Razon de negocio: el duenio observo que los pedidos fiados con energizantes suelen quedar impagos.

Hipotesis de negocio: la combinacion de fiado y energizantes representa riesgo comercial y debe bloquear el pedido.

## Regla 3: pan y queso con lluvia

La promo de pan y queso se aplica cuando no llueve. Si llueve, la promo se cancela.

Razon de negocio: con lluvia baja la circulacion del barrio y el descuento deja de atraer suficientes compradores.

Hipotesis de negocio: la lluvia no bloquea la compra, pero debe cancelar el descuento para preservar margen.

## Regla 4: envio gratis con cupon capicua

El envio gratis solo aplica si el cupon es capicua y el ticket supera 5000.

Razon de negocio: si el ticket es menor, el costo logistico supera el margen del pedido.

Hipotesis de negocio: el cupon capicua no alcanza por si solo; tambien debe validarse el monto minimo.

## Regla 5: hielo y velas

Hielo y velas pueden comprarse juntos, pero se preparan en zonas separadas del almacen.

Razon operativa: hielo requiere frio y velas se preparan con mercaderia seca.

Hipotesis de negocio: hielo y velas no deben bloquear la compra; deben generar una advertencia logistica.
