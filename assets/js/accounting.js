/* ============================================================
   InvenFlow — Accounting & Business Logic Engine
   ------------------------------------------------------------
   Cost method: Weighted Average Cost (WAC).

   For a product:
     after purchase of qty Q at unit cost C:
       newQty   = oldQty + Q
       newValue = oldValue + Q*C
       avgCost  = newValue / newQty
     after sale of qty Q:
       cogs     = Q * avgCost (at moment of sale)
       newQty   = oldQty - Q
       newValue = oldValue - cogs
       avgCost  unchanged (until next purchase)

   Each business event posts a balanced journal entry (借貸平衡).
   ============================================================ */

const A = window.Data.accounts;

const Logic = {

  /* ----------------------------------------------------------
     Purchase (進貨入庫)
     - Increase inventory at unit cost
     - Update WAC per product
     - DR Inventory, CR AP (credit terms)  /  CR Cash (現金)
     - Update AP balance
  ----------------------------------------------------------- */
  createPurchase(db, payload) {
    const {
      date, supplierId, warehouseId, payMethod = "credit",
      note = "", items
    } = payload;
    if (!items || !items.length) throw new Error("進貨單需至少 1 項商品");

    const id = nextDocId("PI", date, db.purchases);
    let total = 0;

    const lineRows = items.map(it => {
      const qty = Number(it.qty);
      const unitCost = Number(it.unitCost);
      if (!qty || qty <= 0) throw new Error("數量需 > 0");
      if (unitCost < 0) throw new Error("成本不可為負");
      const subtotal = round(qty * unitCost, 2);
      total += subtotal;
      return { productId: it.productId, qty, unitCost, subtotal };
    });
    total = round(total, 2);

    const doc = {
      id, type: "purchase",
      date, supplierId, warehouseId, payMethod, note,
      items: lineRows, total,
      paid: 0, status: "open",
      createdAt: new Date().toISOString(),
    };

    // Apply stock movements and update WAC for each product
    lineRows.forEach(line => {
      const inv = db.inventoryState[line.productId] || (db.inventoryState[line.productId] = { qty: 0, totalCost: 0, avgCost: 0 });
      const newQty = inv.qty + line.qty;
      const newTotalCost = round(inv.totalCost + line.subtotal, 2);
      const newAvg = newQty > 0 ? round(newTotalCost / newQty, 4) : 0;

      inv.qty = newQty;
      inv.totalCost = newTotalCost;
      inv.avgCost = newAvg;

      db.stockLedger.push({
        id: uid("L"),
        date, productId: line.productId, warehouseId,
        type: "purchase", refType: "purchase", refId: id,
        qtyIn: line.qty, qtyOut: 0, unitCost: line.unitCost,
        balanceQty: newQty, balanceCost: newTotalCost, avgCost: newAvg,
        note,
      });
    });

    // Journal entry
    const lines = [
      { account: A.INVENTORY.id, debit: total, credit: 0, memo: `${id} 進貨入庫` },
    ];
    if (payMethod === "cash") {
      lines.push({ account: A.CASH.id, debit: 0, credit: total, memo: `${id} 現金支付` });
    } else {
      lines.push({ account: A.AP.id, debit: 0, credit: total, memo: `${id} 應付供應商` });
      db.apBalance[supplierId] = round((db.apBalance[supplierId] || 0) + total, 2);
    }
    if (payMethod === "cash") {
      doc.paid = total;
      doc.status = "paid";
    }

    db.journal.push({
      id: uid("J"), date, refType: "purchase", refId: id,
      memo: `進貨入庫 - ${supplierIdToName(db, supplierId)}`,
      lines,
    });

    db.purchases.push(doc);
    Data.save(db);
    return doc;
  },

  /* ----------------------------------------------------------
     Sale (銷售出貨)
     - Decrease inventory at current WAC -> COGS
     - DR AR / DR Cash, CR Revenue (sale)
     - DR COGS, CR Inventory (cost of goods sold)
  ----------------------------------------------------------- */
  createSale(db, payload) {
    const {
      date, customerId, warehouseId, payMethod = "credit",
      note = "", items
    } = payload;
    if (!items || !items.length) throw new Error("銷售單需至少 1 項商品");

    const id = nextDocId("SO", date, db.sales);
    let total = 0;
    let totalCogs = 0;

    const lineRows = items.map(it => {
      const qty = Number(it.qty);
      const unitPrice = Number(it.unitPrice);
      if (!qty || qty <= 0) throw new Error("數量需 > 0");
      if (unitPrice < 0) throw new Error("售價不可為負");

      const inv = db.inventoryState[it.productId];
      if (!inv || inv.qty < qty) {
        throw new Error(`商品 ${it.productId} 庫存不足（現有 ${inv ? inv.qty : 0}）`);
      }

      const lineRevenue = round(qty * unitPrice, 2);
      const lineCogs    = round(qty * inv.avgCost, 2);
      total += lineRevenue;
      totalCogs += lineCogs;

      return {
        productId: it.productId, qty,
        unitPrice, subtotal: lineRevenue,
        unitCost: inv.avgCost, cogs: lineCogs,
      };
    });
    total = round(total, 2);
    totalCogs = round(totalCogs, 2);

    const doc = {
      id, type: "sale",
      date, customerId, warehouseId, payMethod, note,
      items: lineRows, total, cogs: totalCogs,
      received: 0, status: "open",
      createdAt: new Date().toISOString(),
    };

    // Stock-out movements
    lineRows.forEach(line => {
      const inv = db.inventoryState[line.productId];
      const newQty = inv.qty - line.qty;
      const newTotalCost = round(inv.totalCost - line.cogs, 2);
      // avgCost unchanged on sale
      inv.qty = newQty;
      inv.totalCost = newTotalCost;
      // if qty becomes 0, reset avg to 0
      if (newQty <= 0) {
        inv.qty = 0;
        inv.totalCost = 0;
        inv.avgCost = 0;
      }
      db.stockLedger.push({
        id: uid("L"),
        date, productId: line.productId, warehouseId,
        type: "sale", refType: "sale", refId: id,
        qtyIn: 0, qtyOut: line.qty, unitCost: line.unitCost,
        balanceQty: inv.qty, balanceCost: inv.totalCost, avgCost: inv.avgCost,
        note,
      });
    });

    // Journal: revenue side + cost side
    const lines = [];
    if (payMethod === "cash") {
      lines.push({ account: A.CASH.id, debit: total, credit: 0, memo: `${id} 銷貨收款` });
    } else {
      lines.push({ account: A.AR.id, debit: total, credit: 0, memo: `${id} 應收客戶` });
      db.arBalance[customerId] = round((db.arBalance[customerId] || 0) + total, 2);
    }
    lines.push({ account: A.REVENUE.id, debit: 0, credit: total, memo: `${id} 銷貨收入` });
    lines.push({ account: A.COGS.id, debit: totalCogs, credit: 0, memo: `${id} 銷貨成本` });
    lines.push({ account: A.INVENTORY.id, debit: 0, credit: totalCogs, memo: `${id} 存貨轉成本` });

    if (payMethod === "cash") {
      doc.received = total;
      doc.status = "paid";
    }

    db.journal.push({
      id: uid("J"), date, refType: "sale", refId: id,
      memo: `銷貨出貨 - ${customerIdToName(db, customerId)}`,
      lines,
    });

    db.sales.push(doc);
    Data.save(db);
    return doc;
  },

  /* ----------------------------------------------------------
     Pay supplier
     DR AP, CR Cash
  ----------------------------------------------------------- */
  recordPayment(db, { date, supplierId, purchaseId, amount, method = "bank", note = "" }) {
    amount = round(amount, 2);
    if (amount <= 0) throw new Error("付款金額需 > 0");

    const id = nextDocId("PV", date, db.payments);
    const purchase = purchaseId ? db.purchases.find(x => x.id === purchaseId) : null;
    if (purchase) {
      const remain = round(purchase.total - purchase.paid, 2);
      if (amount > remain + 0.001) throw new Error(`付款金額超過剩餘 (剩 ${remain})`);
      purchase.paid = round(purchase.paid + amount, 2);
      const diff = round(purchase.total - purchase.paid, 2);
      purchase.status = diff <= 0.001 ? "paid" : "partial";
    }

    db.apBalance[supplierId] = round((db.apBalance[supplierId] || 0) - amount, 2);

    const pay = {
      id, date, supplierId, purchaseId, amount, method, note,
      createdAt: new Date().toISOString(),
    };
    db.payments.push(pay);

    db.journal.push({
      id: uid("J"), date, refType: "payment", refId: id,
      memo: `付款 - ${supplierIdToName(db, supplierId)}` + (purchaseId ? ` (${purchaseId})` : ""),
      lines: [
        { account: A.AP.id, debit: amount, credit: 0, memo: `${id} 沖銷應付` },
        { account: A.CASH.id, debit: 0, credit: amount, memo: `${id} 支付現金/銀行` },
      ],
    });

    Data.save(db);
    return pay;
  },

  /* ----------------------------------------------------------
     Receive from customer
     DR Cash, CR AR
  ----------------------------------------------------------- */
  recordReceipt(db, { date, customerId, saleId, amount, method = "bank", note = "" }) {
    amount = round(amount, 2);
    if (amount <= 0) throw new Error("收款金額需 > 0");

    const id = nextDocId("RV", date, db.receipts);
    const sale = saleId ? db.sales.find(x => x.id === saleId) : null;
    if (sale) {
      const remain = round(sale.total - sale.received, 2);
      if (amount > remain + 0.001) throw new Error(`收款金額超過剩餘 (剩 ${remain})`);
      sale.received = round(sale.received + amount, 2);
      const diff = round(sale.total - sale.received, 2);
      sale.status = diff <= 0.001 ? "paid" : "partial";
    }

    db.arBalance[customerId] = round((db.arBalance[customerId] || 0) - amount, 2);

    const rcv = {
      id, date, customerId, saleId, amount, method, note,
      createdAt: new Date().toISOString(),
    };
    db.receipts.push(rcv);

    db.journal.push({
      id: uid("J"), date, refType: "receipt", refId: id,
      memo: `收款 - ${customerIdToName(db, customerId)}` + (saleId ? ` (${saleId})` : ""),
      lines: [
        { account: A.CASH.id, debit: amount, credit: 0, memo: `${id} 收現/收銀行` },
        { account: A.AR.id, debit: 0, credit: amount, memo: `${id} 沖銷應收` },
      ],
    });

    Data.save(db);
    return rcv;
  },

  /* ----------------------------------------------------------
     Stock Adjustment (盤盈/盤虧)
     +qty: gain  -> DR Inventory, CR ADJ_GAIN  (use unitCost or current avg)
     -qty: loss  -> DR ADJ_LOSS, CR Inventory  (always at avgCost)
  ----------------------------------------------------------- */
  createAdjustment(db, { date, productId, warehouseId, qty, unitCost, note = "" }) {
    qty = Number(qty);
    if (!qty || qty === 0) throw new Error("調整數量需不為 0");
    const inv = db.inventoryState[productId] || (db.inventoryState[productId] = { qty: 0, totalCost: 0, avgCost: 0 });

    const id = uid("ADJ");
    let valueDelta = 0;
    let unitCostUsed = inv.avgCost;
    if (qty > 0) {
      // gain
      unitCostUsed = Number(unitCost) > 0 ? Number(unitCost) : inv.avgCost || Number(unitCost) || 0;
      valueDelta = round(qty * unitCostUsed, 2);
      const newQty = inv.qty + qty;
      const newTotalCost = round(inv.totalCost + valueDelta, 2);
      inv.qty = newQty;
      inv.totalCost = newTotalCost;
      inv.avgCost = newQty > 0 ? round(newTotalCost / newQty, 4) : 0;
    } else {
      // loss
      if (inv.qty + qty < 0) throw new Error(`調整後庫存為負 (現有 ${inv.qty})`);
      unitCostUsed = inv.avgCost;
      valueDelta = round(Math.abs(qty) * unitCostUsed, 2);
      inv.qty = inv.qty + qty; // qty is negative
      inv.totalCost = round(inv.totalCost - valueDelta, 2);
      if (inv.qty <= 0) { inv.qty = 0; inv.totalCost = 0; inv.avgCost = 0; }
    }

    db.stockLedger.push({
      id: uid("L"),
      date, productId, warehouseId,
      type: qty > 0 ? "adjustment_in" : "adjustment_out",
      refType: "adjustment", refId: id,
      qtyIn: qty > 0 ? qty : 0,
      qtyOut: qty < 0 ? -qty : 0,
      unitCost: unitCostUsed,
      balanceQty: inv.qty, balanceCost: inv.totalCost, avgCost: inv.avgCost,
      note,
    });

    const memo = `${id} 庫存${qty > 0 ? "盤盈" : "盤虧"} ${productIdToName(db, productId)}`;
    db.journal.push({
      id: uid("J"), date, refType: "adjustment", refId: id, memo,
      lines: qty > 0
        ? [
          { account: A.INVENTORY.id, debit: valueDelta, credit: 0, memo },
          { account: A.ADJ_GAIN.id, debit: 0, credit: valueDelta, memo },
        ]
        : [
          { account: A.ADJ_LOSS.id, debit: valueDelta, credit: 0, memo },
          { account: A.INVENTORY.id, debit: 0, credit: valueDelta, memo },
        ],
    });

    Data.save(db);
    return { id, qty, unitCostUsed, valueDelta };
  },

  /* ----------------------------------------------------------
     Void purchase/sale — reverses stock + journal.
     For demo, only support voiding documents with no partial settlement.
  ----------------------------------------------------------- */
  voidDoc(db, refType, refId) {
    if (refType === "purchase") {
      const doc = db.purchases.find(x => x.id === refId);
      if (!doc) throw new Error("找不到單據");
      if (doc.status === "void") throw new Error("已作廢");
      if (doc.paid > 0) throw new Error("已有付款，請先沖回");
      // reverse stock: remove these qty at the unit cost they came in
      doc.items.forEach(line => {
        const inv = db.inventoryState[line.productId];
        const newQty = inv.qty - line.qty;
        if (newQty < 0) throw new Error("庫存已不足以回沖");
        const newTotalCost = round(inv.totalCost - line.subtotal, 2);
        inv.qty = newQty;
        inv.totalCost = newTotalCost;
        inv.avgCost = newQty > 0 ? round(newTotalCost / newQty, 4) : 0;
        db.stockLedger.push({
          id: uid("L"), date: todayISO(),
          productId: line.productId, warehouseId: doc.warehouseId,
          type: "void", refType: "purchase_void", refId,
          qtyIn: 0, qtyOut: line.qty, unitCost: line.unitCost,
          balanceQty: inv.qty, balanceCost: inv.totalCost, avgCost: inv.avgCost,
          note: "進貨單作廢回沖",
        });
      });
      if (doc.payMethod === "credit") {
        db.apBalance[doc.supplierId] = round((db.apBalance[doc.supplierId] || 0) - doc.total, 2);
      }
      // Reverse journal
      db.journal.push({
        id: uid("J"), date: todayISO(), refType: "purchase_void", refId,
        memo: `進貨單 ${refId} 作廢回沖`,
        lines: [
          { account: A.INVENTORY.id, debit: 0, credit: doc.total, memo: "回沖存貨" },
          { account: doc.payMethod === "cash" ? A.CASH.id : A.AP.id, debit: doc.total, credit: 0, memo: "回沖應付" },
        ],
      });
      doc.status = "void";
    } else if (refType === "sale") {
      const doc = db.sales.find(x => x.id === refId);
      if (!doc) throw new Error("找不到單據");
      if (doc.status === "void") throw new Error("已作廢");
      if (doc.received > 0) throw new Error("已有收款，請先沖回");
      // reverse stock: put back qty at the cogs cost
      doc.items.forEach(line => {
        const inv = db.inventoryState[line.productId];
        const newQty = inv.qty + line.qty;
        const newTotalCost = round(inv.totalCost + line.cogs, 2);
        inv.qty = newQty;
        inv.totalCost = newTotalCost;
        inv.avgCost = newQty > 0 ? round(newTotalCost / newQty, 4) : 0;
        db.stockLedger.push({
          id: uid("L"), date: todayISO(),
          productId: line.productId, warehouseId: doc.warehouseId,
          type: "void", refType: "sale_void", refId,
          qtyIn: line.qty, qtyOut: 0, unitCost: line.unitCost,
          balanceQty: inv.qty, balanceCost: inv.totalCost, avgCost: inv.avgCost,
          note: "銷售單作廢回沖",
        });
      });
      if (doc.payMethod === "credit") {
        db.arBalance[doc.customerId] = round((db.arBalance[doc.customerId] || 0) - doc.total, 2);
      }
      db.journal.push({
        id: uid("J"), date: todayISO(), refType: "sale_void", refId,
        memo: `銷售單 ${refId} 作廢回沖`,
        lines: [
          { account: A.REVENUE.id, debit: doc.total, credit: 0, memo: "回沖收入" },
          { account: doc.payMethod === "cash" ? A.CASH.id : A.AR.id, debit: 0, credit: doc.total, memo: "回沖應收" },
          { account: A.INVENTORY.id, debit: doc.cogs, credit: 0, memo: "回沖存貨" },
          { account: A.COGS.id, debit: 0, credit: doc.cogs, memo: "回沖成本" },
        ],
      });
      doc.status = "void";
    }
    Data.save(db);
  },

  /* ----------------------------------------------------------
     Reporting helpers
  ----------------------------------------------------------- */
  inventoryValue(db) {
    return round(Object.values(db.inventoryState).reduce((s, i) => s + (i.totalCost || 0), 0), 2);
  },
  totalAR(db) { return round(Object.values(db.arBalance).reduce((s, v) => s + v, 0), 2); },
  totalAP(db) { return round(Object.values(db.apBalance).reduce((s, v) => s + v, 0), 2); },

  // Sales / COGS within range (exclusive of voids)
  rangeSales(db, fromISO, toISO) {
    let revenue = 0, cogs = 0, orders = 0;
    db.sales.forEach(s => {
      if (s.status === "void") return;
      if (fromISO && s.date < fromISO) return;
      if (toISO && s.date > toISO) return;
      revenue += s.total;
      cogs += s.cogs;
      orders++;
    });
    return { revenue: round(revenue, 2), cogs: round(cogs, 2), gross: round(revenue - cogs, 2), orders };
  },

  // Trial balance — sum debits/credits per account
  trialBalance(db) {
    const map = {};
    Object.values(A).forEach(a => map[a.id] = { account: a, debit: 0, credit: 0 });
    db.journal.forEach(je => {
      je.lines.forEach(ln => {
        if (!map[ln.account]) return;
        map[ln.account].debit += ln.debit || 0;
        map[ln.account].credit += ln.credit || 0;
      });
    });
    Object.values(map).forEach(r => {
      r.debit = round(r.debit, 2);
      r.credit = round(r.credit, 2);
      r.balance = round(r.debit - r.credit, 2);
    });
    return map;
  },
};

function supplierIdToName(db, id) { const s = db.suppliers.find(x => x.id === id); return s ? s.name : id; }
function customerIdToName(db, id) { const c = db.customers.find(x => x.id === id); return c ? c.name : id; }
function productIdToName(db, id) { const p = db.products.find(x => x.id === id); return p ? p.name : id; }

window.Logic = Logic;
window.supplierIdToName = supplierIdToName;
window.customerIdToName = customerIdToName;
window.productIdToName = productIdToName;
