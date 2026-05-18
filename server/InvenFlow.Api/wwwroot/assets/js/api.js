/* ============================================================
   InvenFlow — API client (browser side)
   Talks to the ASP.NET Core backend at the same origin.
   ============================================================ */

const API = (() => {
  const base = ""; // same origin

  async function http(method, path, body) {
    const opts = { method, headers: { "Accept": "application/json" } };
    if (body !== undefined) {
      opts.headers["Content-Type"] = "application/json";
      opts.body = JSON.stringify(body);
    }
    const res = await fetch(base + path, opts);
    if (!res.ok) {
      let err;
      try { err = await res.json(); } catch { err = { error: res.statusText }; }
      throw new Error(err.error || `HTTP ${res.status}`);
    }
    if (res.status === 204) return null;
    return res.json();
  }

  return {
    // master
    listProducts:   () => http("GET",  "/api/products"),
    createProduct:  (p) => http("POST", "/api/products", p),
    updateProduct:  (id, p) => http("PUT", `/api/products/${id}`, p),
    deleteProduct:  (id) => http("DELETE", `/api/products/${id}`),

    listSuppliers:  () => http("GET",  "/api/suppliers"),
    createSupplier: (p) => http("POST", "/api/suppliers", p),
    updateSupplier: (id, p) => http("PUT", `/api/suppliers/${id}`, p),
    deleteSupplier: (id) => http("DELETE", `/api/suppliers/${id}`),

    listCustomers:  () => http("GET",  "/api/customers"),
    createCustomer: (p) => http("POST", "/api/customers", p),
    updateCustomer: (id, p) => http("PUT", `/api/customers/${id}`, p),
    deleteCustomer: (id) => http("DELETE", `/api/customers/${id}`),

    listWarehouses: () => http("GET",  "/api/warehouses"),
    listAccounts:   () => http("GET",  "/api/accounts"),

    // transactions
    listPurchases:  () => http("GET",  "/api/purchases"),
    createPurchase: (p) => http("POST", "/api/purchases", p),
    cancelPurchase: (id) => http("POST", `/api/purchases/${id}/cancel`),

    listSales:      () => http("GET",  "/api/sales"),
    createSale:     (p) => http("POST", "/api/sales", p),
    cancelSale:     (id) => http("POST", `/api/sales/${id}/cancel`),

    listPayments:   () => http("GET",  "/api/payments"),
    createPayment:  (p) => http("POST", "/api/payments", p),
    cancelPayment:  (id) => http("POST", `/api/payments/${id}/cancel`),

    listReceipts:   () => http("GET",  "/api/receipts"),
    createReceipt:  (p) => http("POST", "/api/receipts", p),
    cancelReceipt:  (id) => http("POST", `/api/receipts/${id}/cancel`),

    listStockMovements: () => http("GET", "/api/stock-movements"),
    adjustStock:    (p) => http("POST", "/api/stock-movements/adjust", p),

    listJournals:   () => http("GET",  "/api/journals"),

    // reports
    dashboard:      () => http("GET",  "/api/reports/dashboard"),
    grossProfit:    (from, to) => http("GET", `/api/reports/gross-profit?from=${from}&to=${to}`),
    ranking:        (top = 10) => http("GET", `/api/reports/product-ranking?top=${top}`),
    turnover:       () => http("GET",  "/api/reports/inventory-turnover"),
    trialBalance:   (from, to) => http("GET", `/api/reports/trial-balance?from=${from}&to=${to}`),

    // system
    health:         () => http("GET",  "/api/system/health"),
    reset:          () => http("POST", "/api/system/reset"),
  };
})();

window.API = API;

// Format helpers (mirrors of the static demo)
window.fmtMoney  = (n) => (Number(n) || 0).toLocaleString("zh-TW", { maximumFractionDigits: 2 });
window.fmtNumber = (n) => (Number(n) || 0).toLocaleString("zh-TW");
window.fmtDate   = (s) => { if (!s) return ""; return s.slice(0, 10); };
window.todayISO  = () => new Date().toISOString().slice(0, 10);
