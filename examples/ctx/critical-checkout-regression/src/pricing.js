function roundCurrency(value) {
  return Math.round(value * 100) / 100;
}

function calculateTotal(input) {
  const subtotal = Number(input.subtotal ?? 0);
  const isVip = Boolean(input.isVip);
  const coupon = String(input.coupon ?? "").trim().toUpperCase();

  let discountRate = 0;

  if (isVip) {
    discountRate += 0.1;
  }

  if (coupon === "WELCOME5") {
    discountRate += 0.05;
  }

  discountRate = roundCurrency(discountRate);

  const discountAmount = roundCurrency(subtotal * discountRate);
  const total = roundCurrency(subtotal - discountAmount);

  return {
    subtotal: roundCurrency(subtotal),
    discountRate,
    discountAmount,
    total
  };
}

module.exports = {
  calculateTotal,
  roundCurrency
};
