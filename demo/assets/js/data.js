/* ============================================================
   InvenFlow — Data Layer
   localStorage-based store with seed data for demo.

   Entities:
     products, suppliers, customers, warehouses,
     purchases (進貨單), sales (銷售單),
     stockLedger (庫存異動明細),
     payables, receivables, payments, receipts,
     journal (會計分錄).

   Cost method: Weighted Average Cost (WAC).
   Every transaction posts balanced debit/credit journal entries.
   ============================================================ */

const DB_KEY = "invenflow:v1";

const accounts = {
  CASH:        { id: "1100", name: "現金/銀行存款", type: "asset" },
  AR:          { id: "1130", name: "應收帳款",     type: "asset" },
  INVENTORY:   { id: "1310", name: "存貨",         type: "asset" },
  AP:          { id: "2100", name: "應付帳款",     type: "liability" },
  REVENUE:     { id: "4100", name: "銷貨收入",     type: "revenue" },
  COGS:        { id: "5100", name: "銷貨成本",     type: "expense" },
  ADJ_GAIN:    { id: "4900", name: "存貨盤盈",     type: "revenue" },
  ADJ_LOSS:    { id: "5900", name: "存貨盤虧",     type: "expense" },
};

const StatusMap = {
  purchaseStatus: {
    open:    { label: "未結",   pill: "warn" },
    partial: { label: "部分付款", pill: "info" },
    paid:    { label: "已結清", pill: "ok" },
    void:    { label: "已作廢", pill: "bad" },
  },
  saleStatus: {
    open:    { label: "未結",   pill: "warn" },
    partial: { label: "部分收款", pill: "info" },
    paid:    { label: "已結清", pill: "ok" },
    void:    { label: "已作廢", pill: "bad" },
  },
  productStatus: {
    active:    { label: "正常",   pill: "ok" },
    purchase_off: { label: "停止進貨", pill: "warn" },
    sale_off:    { label: "停止銷貨", pill: "warn" },
    inactive: { label: "停售",   pill: "bad" },
  },
};

/* -------- Storage helpers -------- */
function readDB() {
  const raw = localStorage.getItem(DB_KEY);
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return null; }
}
function writeDB(db) {
  localStorage.setItem(DB_KEY, JSON.stringify(db));
}
function resetDB() {
  localStorage.removeItem(DB_KEY);
  return seedDB();
}

/* -------- Util -------- */
function uid(prefix) {
  return prefix + "-" + Math.random().toString(36).slice(2, 8).toUpperCase();
}
function pad(n, w = 4) { return String(n).padStart(w, "0"); }
function todayISO() { return new Date().toISOString().slice(0, 10); }
function isoDaysAgo(d) {
  const x = new Date();
  x.setDate(x.getDate() - d);
  return x.toISOString().slice(0, 10);
}
function fmtMoney(n) {
  const v = Number(n) || 0;
  return v.toLocaleString("zh-TW", { minimumFractionDigits: 0, maximumFractionDigits: 2 });
}
function fmtMoneySigned(n) {
  const v = Number(n) || 0;
  return (v >= 0 ? "" : "-") + Math.abs(v).toLocaleString("zh-TW", { minimumFractionDigits: 0, maximumFractionDigits: 2 });
}
function fmtNumber(n) {
  return (Number(n) || 0).toLocaleString("zh-TW");
}
function round(n, d = 2) {
  const f = Math.pow(10, d);
  return Math.round((Number(n) || 0) * f) / f;
}

/* -------- Document numbering -------- */
function nextDocId(prefix, date, list) {
  const d = (date || todayISO()).replace(/-/g, "");
  const same = list.filter(x => x.id && x.id.startsWith(prefix + d));
  return `${prefix}${d}${pad(same.length + 1, 3)}`;
}

/* ============================================================
   Seed Data
   ============================================================ */
function seedDB() {
  const today = new Date();
  const dayOf = (offset) => {
    const x = new Date(today);
    x.setDate(x.getDate() + offset);
    return x.toISOString().slice(0, 10);
  };

  const db = {
    company: {
      name: "InvenFlow Demo 株式會社",
      taxId: "12345678",
      address: "台北市信義區松仁路 100 號 8 樓",
      phone: "02-2345-6789",
      fiscalStart: dayOf(-180).slice(0, 7),
      currency: "TWD",
    },
    user: { name: "示範管理員", role: "系統管理員", initials: "示" },

    warehouses: [
      { id: "WH01", name: "主倉庫",   location: "台北信義倉" },
      { id: "WH02", name: "門市倉",   location: "台北中山店" },
      { id: "WH03", name: "退貨暫存倉", location: "台北信義倉" },
    ],

    suppliers: [
      { id: "S001", name: "立森工坊有限公司", contact: "張承翰", phone: "02-2511-3000", email: "sales@lisen.tw", taxId: "53412678", address: "新北市三重區重新路三段 88 號", terms: "月結 30 天" },
      { id: "S002", name: "綠田農產合作社",   contact: "陳怡君", phone: "049-277-8800", email: "info@greentian.tw", taxId: "12876541", address: "南投縣埔里鎮中山路 200 號", terms: "貨到付款" },
      { id: "S003", name: "海立貿易股份有限公司", contact: "林冠廷", phone: "02-2799-2200", email: "biz@hailee.tw", taxId: "29871234", address: "台北市內湖區瑞光路 88 號", terms: "月結 60 天" },
      { id: "S004", name: "晨陽包裝實業",     contact: "黃雅婷", phone: "04-2389-0011", email: "service@chenyang.tw", taxId: "34678910", address: "台中市西屯區工業 8 路 12 號", terms: "月結 30 天" },
    ],

    customers: [
      { id: "C001", name: "晴日生活百貨",     contact: "李曉雯", phone: "02-2700-1234", email: "buy@sunday.tw", taxId: "10293847", address: "台北市大安區信義路四段 100 號", terms: "月結 30 天", type: "批發" },
      { id: "C002", name: "好食市集連鎖門市", contact: "蔡子翔", phone: "02-2655-7788", email: "po@goodfood.tw", taxId: "29384756", address: "台北市內湖區瑞光路 200 號", terms: "月結 45 天", type: "通路" },
      { id: "C003", name: "微光咖啡器具行",   contact: "吳采蓁", phone: "02-2331-6680", email: "owner@glowcoffee.tw", taxId: "55667788", address: "台北市中正區重慶南路一段 45 號", terms: "貨到付款", type: "零售" },
      { id: "C004", name: "山海農產直營",     contact: "鄭家豪", phone: "07-336-9911", email: "hi@shanhai.tw", taxId: "44556677", address: "高雄市前鎮區成功二路 75 號", terms: "月結 30 天", type: "通路" },
      { id: "C005", name: "Walk-in 散客",     contact: "—",       phone: "—", email: "—", taxId: "—", address: "—", terms: "現金", type: "零售" },
    ],

    products: [
      { id: "P001", sku: "OG-RICE-5KG",    name: "有機池上米 5kg",     category: "農產", unit: "包", listPrice: 520, stdCost: 360, status: "active" },
      { id: "P002", sku: "OG-RICE-2KG",    name: "有機池上米 2kg",     category: "農產", unit: "包", listPrice: 240, stdCost: 165, status: "active" },
      { id: "P003", sku: "TEA-OL-150",     name: "高山烏龍茶 150g",    category: "茶飲", unit: "罐", listPrice: 680, stdCost: 420, status: "active" },
      { id: "P004", sku: "TEA-RED-100",    name: "蜜香紅茶 100g",      category: "茶飲", unit: "罐", listPrice: 480, stdCost: 290, status: "active" },
      { id: "P005", sku: "COF-MED-200",    name: "中焙咖啡豆 200g",    category: "咖啡", unit: "包", listPrice: 560, stdCost: 320, status: "active" },
      { id: "P006", sku: "COF-DRK-200",    name: "深焙咖啡豆 200g",    category: "咖啡", unit: "包", listPrice: 580, stdCost: 335, status: "active" },
      { id: "P007", sku: "OIL-OLV-500",    name: "初榨橄欖油 500ml",   category: "食品", unit: "瓶", listPrice: 720, stdCost: 470, status: "active" },
      { id: "P008", sku: "OIL-SES-250",    name: "黑麻油 250ml",       category: "食品", unit: "瓶", listPrice: 380, stdCost: 240, status: "active" },
      { id: "P009", sku: "HONEY-LO-500",   name: "龍眼蜂蜜 500g",      category: "食品", unit: "瓶", listPrice: 560, stdCost: 350, status: "active" },
      { id: "P010", sku: "DRY-BEAN-300",   name: "綜合堅果 300g",      category: "食品", unit: "包", listPrice: 420, stdCost: 260, status: "active" },
      { id: "P011", sku: "BAG-KFT-500",    name: "牛皮包裝袋 500 入",   category: "包材", unit: "箱", listPrice: 540, stdCost: 380, status: "active" },
      { id: "P012", sku: "BAG-PET-1000",   name: "PET 真空袋 1000 入",  category: "包材", unit: "箱", listPrice: 880, stdCost: 620, status: "purchase_off" },
    ],

    purchases: [],
    sales: [],
    payments: [],
    receipts: [],
    stockLedger: [],     // {date, productId, type, refId, qtyIn, qtyOut, unitCost, balanceQty, balanceCost, avgCost, note}
    journal: [],         // {date, refType, refId, lines: [{account, debit, credit, memo}]}

    inventoryState: {},  // productId -> { qty, totalCost, avgCost }
    arBalance: {},       // customerId -> balance
    apBalance: {},       // supplierId -> balance
  };

  // initialize inventory state from products
  db.products.forEach(p => {
    db.inventoryState[p.id] = { qty: 0, totalCost: 0, avgCost: 0 };
  });

  // generate seed transactions to make demo lively
  seedTransactions(db);
  writeDB(db);
  return db;
}

/* -------- Seed Transactions -------- */
function seedTransactions(db) {
  // Opening receipts (purchase orders) to stock the warehouse
  const opening = [
    { supplierId: "S001", offset: -120, lines: [
      ["P001", 200, 350], ["P002", 300, 158],
    ]},
    { supplierId: "S002", offset: -100, lines: [
      ["P003", 150, 410], ["P004", 200, 285],
    ]},
    { supplierId: "S003", offset: -90, lines: [
      ["P005", 240, 315], ["P006", 180, 330], ["P007", 120, 460],
    ]},
    { supplierId: "S002", offset: -75, lines: [
      ["P009", 200, 345], ["P010", 240, 255], ["P008", 160, 235],
    ]},
    { supplierId: "S004", offset: -60, lines: [
      ["P011", 50, 375], ["P012", 30, 615],
    ]},
    // mid-period replenishment with different costs (so WAC updates)
    { supplierId: "S001", offset: -45, lines: [
      ["P001", 150, 375], ["P002", 200, 170],
    ]},
    { supplierId: "S003", offset: -35, lines: [
      ["P005", 120, 335], ["P006", 100, 350],
    ]},
    { supplierId: "S002", offset: -20, lines: [
      ["P003", 80, 432], ["P009", 100, 360],
    ]},
  ];
  opening.forEach((o, i) => {
    const items = o.lines.map(([pid, qty, cost]) => ({ productId: pid, qty, unitCost: cost }));
    Logic.createPurchase(db, {
      date: isoDaysAgo(-o.offset),
      supplierId: o.supplierId,
      warehouseId: i % 2 === 0 ? "WH01" : "WH02",
      payMethod: "credit",
      note: i === 0 ? "期初進貨" : "補貨入庫",
      items,
    });
  });

  // Pay off two early invoices (so AP balance is realistic and varied)
  const payTargets = db.purchases.slice(0, 2);
  payTargets.forEach(p => {
    Logic.recordPayment(db, {
      date: isoDaysAgo(p.date ? Math.max(1, daysBetween(p.date, todayISO()) - 30) : 30),
      supplierId: p.supplierId,
      purchaseId: p.id,
      amount: p.total,
      method: "bank",
      note: "依約 30 天結清",
    });
  });
  // Partial pay 3rd
  if (db.purchases[2]) {
    const p = db.purchases[2];
    Logic.recordPayment(db, {
      date: isoDaysAgo(20),
      supplierId: p.supplierId,
      purchaseId: p.id,
      amount: Math.round(p.total * 0.5),
      method: "bank",
      note: "部分付款",
    });
  }

  // Sales over the past 60 days
  const sales = [
    { customerId: "C001", offset: -55, lines: [["P001", 30], ["P003", 20], ["P005", 25]] },
    { customerId: "C002", offset: -50, lines: [["P002", 60], ["P010", 30]] },
    { customerId: "C003", offset: -45, lines: [["P005", 40], ["P006", 30]] },
    { customerId: "C004", offset: -40, lines: [["P007", 25], ["P009", 30], ["P008", 20]] },
    { customerId: "C001", offset: -32, lines: [["P001", 25], ["P003", 18]] },
    { customerId: "C005", offset: -28, lines: [["P004", 12], ["P010", 10]] },
    { customerId: "C002", offset: -22, lines: [["P002", 40], ["P004", 20], ["P011", 6]] },
    { customerId: "C003", offset: -16, lines: [["P006", 25], ["P005", 18]] },
    { customerId: "C004", offset: -12, lines: [["P009", 22], ["P007", 12]] },
    { customerId: "C001", offset: -8, lines: [["P003", 24], ["P001", 18]] },
    { customerId: "C002", offset: -5, lines: [["P010", 30], ["P002", 25]] },
    { customerId: "C003", offset: -3, lines: [["P006", 18]] },
    { customerId: "C001", offset: -1, lines: [["P001", 16], ["P003", 12], ["P005", 20]] },
  ];

  sales.forEach((s, i) => {
    const c = db.customers.find(x => x.id === s.customerId);
    const items = s.lines.map(([pid, qty]) => {
      const p = db.products.find(x => x.id === pid);
      // Apply slight discount for wholesale / 通路
      const discount = c.type === "批發" ? 0.92 : c.type === "通路" ? 0.95 : 1.0;
      return { productId: pid, qty, unitPrice: Math.round(p.listPrice * discount) };
    });
    Logic.createSale(db, {
      date: isoDaysAgo(-s.offset),
      customerId: s.customerId,
      warehouseId: "WH01",
      payMethod: c.id === "C005" ? "cash" : "credit",
      note: c.id === "C005" ? "現金交易" : "依約出貨",
      items,
    });
  });

  // Receive payments for the earliest sales
  const recvTargets = db.sales.slice(0, 4);
  recvTargets.forEach((s, i) => {
    Logic.recordReceipt(db, {
      date: isoDaysAgo(Math.max(1, daysBetween(s.date, todayISO()) - 25)),
      customerId: s.customerId,
      saleId: s.id,
      amount: i === 3 ? Math.round(s.total * 0.6) : s.total,
      method: "bank",
      note: i === 3 ? "部分收款" : "依約收款",
    });
  });
}

function daysBetween(a, b) {
  const da = new Date(a), dbb = new Date(b);
  return Math.round((dbb - da) / 86400000);
}

/* -------- Public API for booting -------- */
function loadDB() {
  let db = readDB();
  if (!db || !db.products) db = seedDB();
  // backfill missing maps
  if (!db.inventoryState) db.inventoryState = {};
  if (!db.arBalance) db.arBalance = {};
  if (!db.apBalance) db.apBalance = {};
  return db;
}

window.Data = {
  load: loadDB,
  save: writeDB,
  reset: resetDB,
  accounts,
  StatusMap,
};
window.fmtMoney = fmtMoney;
window.fmtMoneySigned = fmtMoneySigned;
window.fmtNumber = fmtNumber;
window.uid = uid;
window.todayISO = todayISO;
window.isoDaysAgo = isoDaysAgo;
window.nextDocId = nextDocId;
window.round = round;
