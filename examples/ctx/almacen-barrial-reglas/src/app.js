const precios = {
  yerbaDescuento: 2600,
  energizante: 1500,
  pan: 1200,
  queso: 2300,
  hielo: 900,
  velas: 800
};

const nombres = {
  yerbaDescuento: "Yerba con descuento",
  energizante: "Bebida energizante",
  pan: "Pan",
  queso: "Queso",
  hielo: "Hielo",
  velas: "Velas"
};

const formulario = document.querySelector("#pedido-form");
const resultado = document.querySelector("#resultado");

function esCapicua(valor) {
  const limpio = valor.trim().replace(/\D/g, "");
  return limpio.length > 1 && limpio === limpio.split("").reverse().join("");
}

function evaluarPedido(productos, opciones) {
  const subtotal = productos.reduce((total, producto) => total + precios[producto], 0);
  const bloqueos = [];
  const advertencias = [];
  let descuento = 0;
  let envio = 900;

  if (productos.includes("yerbaDescuento") && opciones.hora >= 18) {
    bloqueos.push("La yerba con descuento solo se vende antes de las 18:00.");
  }

  if (opciones.tipoPago === "fiado" && productos.includes("energizante")) {
    bloqueos.push("El fiado no esta permitido si el pedido incluye energizantes.");
  }

  if (productos.includes("pan") && productos.includes("queso") && opciones.clima !== "lluvia") {
    descuento += 700;
  }

  if (productos.includes("pan") && productos.includes("queso") && opciones.clima === "lluvia") {
    advertencias.push("La promo de pan y queso se cancela por lluvia.");
  }

  if (esCapicua(opciones.cupon) && subtotal > 5000) {
    envio = 0;
  }

  if (esCapicua(opciones.cupon) && subtotal <= 5000) {
    advertencias.push("El cupon capicua no habilita envio gratis porque el ticket no supera 5000.");
  }

  if (productos.includes("hielo") && productos.includes("velas")) {
    advertencias.push("Hielo y velas se preparan por separado. No bloquea el pedido.");
  }

  return {
    subtotal,
    descuento,
    envio,
    total: Math.max(0, subtotal - descuento + envio),
    bloqueos,
    advertencias
  };
}

function productosSeleccionados() {
  return Array.from(formulario.querySelectorAll("input[name='producto']:checked")).map((input) => input.value);
}

function mostrarResultado(productos, evaluacion) {
  const estado = evaluacion.bloqueos.length ? "Pedido bloqueado" : "Pedido aprobado";
  const clase = evaluacion.bloqueos.length ? "bloqueado" : "aprobado";
  const items = productos.length ? productos.map((producto) => nombres[producto]).join(", ") : "Sin productos";

  resultado.innerHTML = `
    <h2 class="${clase}">${estado}</h2>
    <p><strong>Productos:</strong> ${items}</p>
    <p><strong>Total:</strong> $${evaluacion.total}</p>
    ${evaluacion.bloqueos.map((texto) => `<p class="bloqueado">${texto}</p>`).join("")}
    ${evaluacion.advertencias.map((texto) => `<p class="advertencia">${texto}</p>`).join("")}
  `;
}

formulario.addEventListener("submit", (evento) => {
  evento.preventDefault();

  const productos = productosSeleccionados();
  const evaluacion = evaluarPedido(productos, {
    tipoPago: document.querySelector("#tipo-pago").value,
    clima: document.querySelector("#clima").value,
    hora: Number(document.querySelector("#hora").value),
    cupon: document.querySelector("#cupon").value
  });

  mostrarResultado(productos, evaluacion);
});

