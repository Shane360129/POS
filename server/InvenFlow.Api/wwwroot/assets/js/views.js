/* ============================================================
   InvenFlow Admin Console — Views
   Each view fetches its own data from the API and renders into #main.
   ============================================================ */

const Views = {};

/* ---------- shared helpers ---------- */

function searchableTable({ rows, columns, filterKeys, renderActions, emptyText = "目前沒有資料" }) {
  const wrap = el("div");
  const search = input({ placeholder: "搜尋…", style: "width: 220px;" });
  wrap.appendChild(el("div", { style: "display:flex; justify-content:flex-end; margin-bottom:10px;" }, [search]));

  const table = el("table", { class: "data" });
  table.innerHTML = `<thead><tr>${columns.map(c => `<th class="${c.num ? 'num' : ''}">${c.label}</th>`).join("")}${renderActions ? "<th></th>" : ""}</tr></thead>`;
  const tbody = el("tbody");
  table.appendChild(tbody);
  wrap.appendChild(table);

  const empty = el("div", { class: "empty", text: emptyText });

  const render = (filter = "") => {
    tbody.innerHTML = "";
    const q = filter.trim().toLowerCase();
    const visible = !q ? rows : rows.filter(r => filterKeys.some(k => String(r[k] ?? "").toLowerCase().includes(q)));
    if (!visible.length) {
      tbody.appendChild(el("tr", { html: `<td colspan="${columns.length + (renderActions ? 1 : 0)}" style="text-align:center;color:#9aa0c1;padding:24px;">${q ? "找不到符合條件的資料" : emptyText}</td>` }));
      return;
    }
    visible.forEach(r => {
      const tr = el("tr");
      tr.innerHTML = columns.map(c => `<td class="${c.num ? 'num' : ''}">${c.format ? c.format(r[c.key], r) : (r[c.key] ?? "")}</td>`).join("");
      if (renderActions) {
        const td = el("td", { style: "text-align:right; white-space:nowrap;" });
        renderActions(td, r);
        tr.appendChild(td);
      }
      tbody.appendChild(tr);
    });
  };
  search.oninput = () => render(search.value);
  render();
  return wrap;
}

async function tryDelete(label, fn) {
  try { await fn(); toast(`已刪除${label}`, "ok"); router.refresh(); }
  catch (e) { toast(e.message || "刪除失敗", "err"); }
}

/* ------------------------------------------------------------ */
/* Dashboard */
/* ------------------------------------------------------------ */
Views.dashboard = async () => {
  const data = await API.dashboard();
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "儀表板" }));
  root.appendChild(el("p", { class: "page-sub", text: "後端 SQL Server 即時資料。所有指標由 ASP.NET Core 計算後回傳。" }));

  const metrics = el("div", { class: "row" });
  [
    ["銷貨收入", "NT$ " + fmtMoney(data.revenue), "近期累計"],
    ["銷貨成本", "NT$ " + fmtMoney(data.cogs), "WAC 加權平均"],
    ["毛利",     "NT$ " + fmtMoney(data.grossProfit), "毛利率 " + (data.grossMargin * 100).toFixed(1) + "%"],
    ["存貨價值", "NT$ " + fmtMoney(data.inventoryValue), data.lowStock + " 項低於安全庫存"],
    ["應收帳款", "NT$ " + fmtMoney(data.ar), "尚未收款餘額"],
    ["應付帳款", "NT$ " + fmtMoney(data.ap), "尚未支付餘額"],
  ].forEach(([lbl, val, sub]) => metrics.appendChild(el("div", { class: "metric" }, [
    el("div", { class: "lbl", text: lbl }),
    el("div", { class: "val", text: val }),
    el("div", { class: "sub", text: sub }),
  ])));
  root.appendChild(metrics);

  const cards = el("div", { class: "row", style: "margin-top: 18px;" });

  const salesCard = el("div", { class: "card col" });
  salesCard.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "近 30 天銷售" })]));
  if (!data.sales30?.length) {
    salesCard.appendChild(el("div", { class: "empty", text: "尚無銷售資料" }));
  } else {
    const max = Math.max(...data.sales30.map(s => s.amount));
    const list = el("div", { style: "display:flex; align-items:end; gap:4px; height:120px;" });
    data.sales30.forEach(s => {
      list.appendChild(el("div", { title: `${s.date} · NT$ ${fmtMoney(s.amount)}`,
        style: `flex:1; background:linear-gradient(180deg,#9a6bff,#6772ff); border-radius:4px 4px 0 0; height:${(s.amount / max) * 100}%;` }));
    });
    salesCard.appendChild(list);
  }
  cards.appendChild(salesCard);

  const sumCard = el("div", { class: "card col" });
  sumCard.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "主檔規模" })]));
  sumCard.appendChild(el("div", { class: "row" }, [
    el("div", { class: "metric" }, [el("div", { class: "lbl", text: "商品" }), el("div", { class: "val", text: data.productCount })]),
    el("div", { class: "metric" }, [el("div", { class: "lbl", text: "客戶" }), el("div", { class: "val", text: data.customerCount })]),
    el("div", { class: "metric" }, [el("div", { class: "lbl", text: "供應商" }), el("div", { class: "val", text: data.supplierCount })]),
  ]));
  cards.appendChild(sumCard);

  root.appendChild(cards);
  return root;
};

/* ------------------------------------------------------------ */
/* Products */
/* ------------------------------------------------------------ */
Views.products = async () => {
  const rows = await API.listProducts();
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "商品主檔" }));
  root.appendChild(el("p", { class: "page-sub", text: `共 ${rows.length} 項商品。WAC 平均成本由後端維護。` }));

  const card = el("div", { class: "card" });
  card.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "商品列表" }),
    el("button", { class: "btn btn-primary", text: "+ 新增商品", onclick: () => editProduct(null) })]));

  card.appendChild(searchableTable({
    rows,
    columns: [
      { key: "code", label: "編號" },
      { key: "name", label: "商品名稱" },
      { key: "category", label: "類別" },
      { key: "price", label: "售價", num: true, format: fmtMoney },
      { key: "avgCost", label: "平均成本", num: true, format: fmtMoney },
      { key: "stock", label: "庫存", num: true, format: fmtNumber },
      { key: "safetyStock", label: "安全庫存", num: true, format: fmtNumber },
    ],
    filterKeys: ["code", "name", "category"],
    renderActions: (td, p) => {
      td.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "編輯", onclick: () => editProduct(p) }));
      td.appendChild(el("button", { class: "btn btn-danger btn-sm", text: "刪除", style: "margin-left: 6px;",
        onclick: () => confirmModal(`確定刪除商品「${p.name}」？`,
          () => tryDelete("商品", () => API.deleteProduct(p.id))) }));
    }
  }));
  root.appendChild(card);
  return root;
};

function editProduct(p) {
  const isNew = !p;
  openModal(isNew ? "新增商品" : "編輯商品", (body) => {
    const code = input({ value: p?.code || "" });
    const name = input({ value: p?.name || "" });
    const cat  = input({ value: p?.category || "" });
    const unit = input({ value: p?.unit || "個" });
    const price = input({ type: "number", value: p?.price ?? 0, step: "0.01" });
    const safety = input({ type: "number", value: p?.safetyStock ?? 0 });

    body.appendChild(el("div", { class: "form" }, [
      field("商品編號", code), field("商品名稱", name),
      field("類別", cat), field("單位", unit),
      field("售價", price), field("安全庫存", safety),
    ]));

    return async () => {
      const payload = {
        id: p?.id || 0,
        code: code.value, name: name.value, category: cat.value, unit: unit.value,
        price: Number(price.value), safetyStock: Number(safety.value),
        avgCost: p?.avgCost || 0, stock: p?.stock || 0,
      };
      if (isNew) await API.createProduct(payload);
      else await API.updateProduct(p.id, payload);
      toast("已儲存", "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Suppliers / Customers (similar shape) */
/* ------------------------------------------------------------ */
function masterPage(title, lister, editor, deleter, columns, filterKeys) {
  return async () => {
    const rows = await lister();
    const root = el("div");
    root.appendChild(el("h1", { class: "page-h", text: title }));
    root.appendChild(el("p", { class: "page-sub", text: `共 ${rows.length} 筆資料。` }));

    const card = el("div", { class: "card" });
    card.appendChild(el("div", { class: "card-h" }, [
      el("h3", { text: title + "列表" }),
      el("button", { class: "btn btn-primary", text: "+ 新增", onclick: () => editor(null) })
    ]));

    card.appendChild(searchableTable({
      rows, columns, filterKeys,
      renderActions: (td, r) => {
        td.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "編輯", onclick: () => editor(r) }));
        td.appendChild(el("button", { class: "btn btn-danger btn-sm", text: "刪除", style: "margin-left: 6px;",
          onclick: () => confirmModal(`確定刪除${title}「${r.name}」？`,
            () => tryDelete(title, () => deleter(r.id))) }));
      }
    }));
    root.appendChild(card);
    return root;
  };
}

Views.suppliers = masterPage("供應商", API.listSuppliers,
  (s) => editParty(s, "供應商", API.createSupplier, API.updateSupplier), API.deleteSupplier,
  [{ key: "code", label: "編號" }, { key: "name", label: "名稱" }, { key: "contact", label: "聯絡人" }, { key: "phone", label: "電話" }, { key: "email", label: "Email" }],
  ["code", "name", "contact", "phone", "email"]);

Views.customers = masterPage("客戶", API.listCustomers,
  (c) => editParty(c, "客戶", API.createCustomer, API.updateCustomer, true), API.deleteCustomer,
  [{ key: "code", label: "編號" }, { key: "name", label: "名稱" }, { key: "contact", label: "聯絡人" }, { key: "phone", label: "電話" },
   { key: "creditLimit", label: "信用額度", num: true, format: fmtMoney }],
  ["code", "name", "contact", "phone"]);

Views.warehouses = masterPage("倉庫", API.listWarehouses, editWarehouse, API.deleteWarehouse,
  [{ key: "code", label: "編號" }, { key: "name", label: "名稱" }, { key: "location", label: "位置" }],
  ["code", "name", "location"]);

function editWarehouse(record) {
  const isNew = !record;
  openModal(isNew ? "新增倉庫" : "編輯倉庫", (body) => {
    const code = input({ value: record?.code || "" });
    const name = input({ value: record?.name || "" });
    const loc  = input({ value: record?.location || "" });
    body.appendChild(el("div", { class: "form" }, [
      field("編號", code), field("名稱", name), field("位置", loc),
    ]));
    return async () => {
      const payload = { id: record?.id || 0, code: code.value, name: name.value, location: loc.value };
      if (isNew) await API.createWarehouse(payload);
      else await API.updateWarehouse(record.id, payload);
      toast("已儲存", "ok");
      router.refresh();
    };
  });
}

function editParty(record, label, createFn, updateFn, withCredit = false) {
  const isNew = !record;
  openModal(isNew ? `新增${label}` : `編輯${label}`, (body) => {
    const code = input({ value: record?.code || "" });
    const name = input({ value: record?.name || "" });
    const contact = input({ value: record?.contact || "" });
    const phone = input({ value: record?.phone || "" });
    const email = input({ value: record?.email || "" });
    const credit = input({ type: "number", value: record?.creditLimit ?? 0, step: "0.01" });

    const form = el("div", { class: "form" }, [
      field("編號", code), field("名稱", name),
      field("聯絡人", contact), field("電話", phone),
      field("Email", email),
    ]);
    if (withCredit) form.appendChild(field("信用額度", credit));
    body.appendChild(form);

    return async () => {
      const payload = { id: record?.id || 0, code: code.value, name: name.value,
        contact: contact.value, phone: phone.value, email: email.value };
      if (withCredit) payload.creditLimit = Number(credit.value);
      if (isNew) await createFn(payload); else await updateFn(record.id, payload);
      toast("已儲存", "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Purchases */
/* ------------------------------------------------------------ */
Views.purchases = async () => {
  const rows = await API.listPurchases();
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "進貨作業" }));
  root.appendChild(el("p", { class: "page-sub", text: "每筆進貨會自動刷新加權平均成本，並產生借貸分錄。點單號可查看明細。" }));

  const card = el("div", { class: "card" });
  card.appendChild(el("div", { class: "card-h" }, [
    el("h3", { text: `進貨單列表（${rows.length}）` }),
    el("button", { class: "btn btn-primary", text: "+ 新增進貨", onclick: () => newPurchase() })
  ]));

  card.appendChild(searchableTable({
    rows,
    columns: [
      { key: "number", label: "單號", format: (v, r) => `<a href="javascript:void(0)" onclick='viewPurchase(${r.id})' style="color:#6772ff;font-weight:600;">${v}</a>` },
      { key: "date", label: "日期", format: fmtDate },
      { key: "supplierName", label: "供應商" },
      { key: "warehouseName", label: "倉庫" },
      { key: "totalAmount", label: "金額", num: true, format: fmtMoney },
      { key: "isCash", label: "付款", format: v => v ? "現金" : "賒帳" },
      { key: "status", label: "狀態", format: v => `<span class="pill pill-${v === 'Active' ? 'ok' : 'bad'}">${v === 'Active' ? '有效' : '已作廢'}</span>` },
    ],
    filterKeys: ["number", "supplierName", "warehouseName"],
    renderActions: (td, p) => {
      td.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "明細", onclick: () => viewPurchase(p.id) }));
      if (p.status === "Active") {
        td.appendChild(el("button", { class: "btn btn-danger btn-sm", text: "作廢", style: "margin-left: 6px;",
          onclick: () => confirmModal(`確定作廢 ${p.number}？`,
            async () => { await API.cancelPurchase(p.id); toast("已作廢", "ok"); router.refresh(); }) }));
      }
    }
  }));
  root.appendChild(card);
  return root;
};

async function viewPurchase(id) {
  const p = await API.getPurchase(id);
  openModal(`進貨單 ${p.number}`, (body) => {
    body.appendChild(buildDocHeader([
      ["日期", fmtDate(p.date)], ["供應商", p.supplierName],
      ["倉庫", p.warehouseName], ["付款方式", p.isCash ? "現金" : "賒帳"],
      ["狀態", p.status === "Active" ? "有效" : "已作廢"], ["備註", p.note || "—"],
    ]));
    const t = el("table", { class: "data", style: "margin-top: 14px;" });
    t.innerHTML = `<thead><tr><th>商品</th><th class="num">數量</th><th class="num">單價</th><th class="num">小計</th></tr></thead>`;
    const tb = el("tbody");
    p.items.forEach(i => tb.appendChild(el("tr", { html: `<td>${i.productCode} ${i.productName}</td>
      <td class="num">${fmtNumber(i.qty)}</td>
      <td class="num">${fmtMoney(i.unitCost)}</td>
      <td class="num">${fmtMoney(i.qty * i.unitCost)}</td>` })));
    tb.appendChild(el("tr", { html: `<td colspan="3" style="text-align:right;font-weight:600;">合計</td>
      <td class="num" style="font-weight:700;">${fmtMoney(p.totalAmount)}</td>` }));
    t.appendChild(tb);
    body.appendChild(t);
  });
}
window.viewPurchase = viewPurchase;

function buildDocHeader(rows) {
  const wrap = el("div", { style: "display:grid; grid-template-columns: 1fr 1fr 1fr; gap: 10px 24px; padding: 12px 0;" });
  rows.forEach(([k, v]) => wrap.appendChild(el("div", {}, [
    el("div", { style: "font-size:11px;color:#9aa0c1;text-transform:uppercase;letter-spacing:.5px;", text: k }),
    el("div", { style: "font-size:14px;font-weight:600;color:#1d2235;margin-top:2px;", text: v }),
  ])));
  return wrap;
}

async function newPurchase() {
  const [products, suppliers, warehouses] = await Promise.all([
    API.listProducts(), API.listSuppliers(), API.listWarehouses()]);

  openModal("新增進貨單", (body) => {
    const sup = select(suppliers.map(s => ({ value: s.id, label: `${s.code} ${s.name}` })));
    const wh  = select(warehouses.map(w => ({ value: w.id, label: `${w.code} ${w.name}` })));
    const cash = select([{ value: "false", label: "賒帳", selected: true }, { value: "true", label: "現金" }]);
    const date = input({ type: "date", value: todayISO() });
    const note = input({ value: "" });

    body.appendChild(el("div", { class: "form" }, [
      field("供應商", sup), field("倉庫", wh),
      field("付款方式", cash), field("日期", date),
    ]));
    body.appendChild(el("h4", { text: "明細", style: "margin: 16px 0 8px;" }));

    const lines = el("div");
    const items = [];
    const addLine = () => {
      const idx = items.length;
      const pSel = select(products.map(p => ({ value: p.id, label: `${p.code} ${p.name}（庫存 ${p.stock}）` })));
      const qty = input({ type: "number", min: 1, value: 1 });
      const cost = input({ type: "number", min: 0, step: "0.01", value: 0 });
      const del = el("button", { class: "btn btn-danger btn-sm", text: "✕", onclick: () => { items[idx] = null; row.remove(); } });
      const row = el("div", { class: "line-row" }, [
        field("商品", pSel), field("數量", qty), field("單價", cost), del,
      ]);
      lines.appendChild(row);
      items.push({ getProductId: () => Number(pSel.value), getQty: () => Number(qty.value), getCost: () => Number(cost.value) });
    };
    addLine();
    body.appendChild(lines);
    body.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "+ 新增明細", style: "margin-top: 8px;", onclick: addLine }));
    body.appendChild(el("div", { class: "full" }, [field("備註", note)]));

    return async () => {
      const payload = {
        supplierId: Number(sup.value),
        warehouseId: Number(wh.value),
        isCash: cash.value === "true",
        date: date.value,
        note: note.value,
        items: items.filter(Boolean).map(i => ({ productId: i.getProductId(), qty: i.getQty(), unitCost: i.getCost() }))
      };
      if (!payload.items.length) throw new Error("至少一項明細");
      const r = await API.createPurchase(payload);
      toast(`進貨單 ${r.number} 已建立`, "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Sales */
/* ------------------------------------------------------------ */
Views.sales = async () => {
  const rows = await API.listSales();
  rows.forEach(s => s.profit = s.totalAmount - s.totalCost);
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "銷售作業" }));
  root.appendChild(el("p", { class: "page-sub", text: "銷售時自動以當下加權平均成本鎖定銷貨成本。點單號可查看明細。" }));

  const card = el("div", { class: "card" });
  card.appendChild(el("div", { class: "card-h" }, [
    el("h3", { text: `銷貨單列表（${rows.length}）` }),
    el("button", { class: "btn btn-primary", text: "+ 新增銷售", onclick: () => newSale() })
  ]));

  card.appendChild(searchableTable({
    rows,
    columns: [
      { key: "number", label: "單號", format: (v, r) => `<a href="javascript:void(0)" onclick='viewSale(${r.id})' style="color:#6772ff;font-weight:600;">${v}</a>` },
      { key: "date", label: "日期", format: fmtDate },
      { key: "customerName", label: "客戶" },
      { key: "totalAmount", label: "收入", num: true, format: fmtMoney },
      { key: "totalCost", label: "成本", num: true, format: fmtMoney },
      { key: "profit", label: "毛利", num: true, format: fmtMoney },
      { key: "isCash", label: "付款", format: v => v ? "現金" : "賒帳" },
      { key: "status", label: "狀態", format: v => `<span class="pill pill-${v === 'Active' ? 'ok' : 'bad'}">${v === 'Active' ? '有效' : '已作廢'}</span>` },
    ],
    filterKeys: ["number", "customerName"],
    renderActions: (td, s) => {
      td.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "明細", onclick: () => viewSale(s.id) }));
      if (s.status === "Active") {
        td.appendChild(el("button", { class: "btn btn-danger btn-sm", text: "作廢", style: "margin-left: 6px;",
          onclick: () => confirmModal(`確定作廢 ${s.number}？`,
            async () => { await API.cancelSale(s.id); toast("已作廢", "ok"); router.refresh(); }) }));
      }
    }
  }));
  root.appendChild(card);
  return root;
};

async function viewSale(id) {
  const s = await API.getSale(id);
  openModal(`銷貨單 ${s.number}`, (body) => {
    body.appendChild(buildDocHeader([
      ["日期", fmtDate(s.date)], ["客戶", s.customerName],
      ["倉庫", s.warehouseName], ["付款方式", s.isCash ? "現金" : "賒帳"],
      ["狀態", s.status === "Active" ? "有效" : "已作廢"], ["備註", s.note || "—"],
    ]));
    const t = el("table", { class: "data", style: "margin-top: 14px;" });
    t.innerHTML = `<thead><tr><th>商品</th><th class="num">數量</th>
      <th class="num">售價</th><th class="num">成本</th><th class="num">小計</th></tr></thead>`;
    const tb = el("tbody");
    s.items.forEach(i => tb.appendChild(el("tr", { html: `<td>${i.productCode} ${i.productName}</td>
      <td class="num">${fmtNumber(i.qty)}</td>
      <td class="num">${fmtMoney(i.unitPrice)}</td>
      <td class="num">${fmtMoney(i.unitCost)}</td>
      <td class="num">${fmtMoney(i.qty * i.unitPrice)}</td>` })));
    tb.appendChild(el("tr", { html: `<td colspan="4" style="text-align:right;font-weight:600;">合計（收入 / 成本 / 毛利）</td>
      <td class="num" style="font-weight:700;">${fmtMoney(s.totalAmount)} / ${fmtMoney(s.totalCost)} / ${fmtMoney(s.totalAmount - s.totalCost)}</td>` }));
    t.appendChild(tb);
    body.appendChild(t);
  });
}
window.viewSale = viewSale;

async function newSale() {
  const [products, customers, warehouses] = await Promise.all([
    API.listProducts(), API.listCustomers(), API.listWarehouses()]);

  openModal("新增銷貨單", (body) => {
    const cust = select(customers.map(c => ({ value: c.id, label: `${c.code} ${c.name}` })));
    const wh   = select(warehouses.map(w => ({ value: w.id, label: `${w.code} ${w.name}` })));
    const cash = select([{ value: "false", label: "賒帳", selected: true }, { value: "true", label: "現金" }]);
    const date = input({ type: "date", value: todayISO() });
    const note = input({ value: "" });

    body.appendChild(el("div", { class: "form" }, [
      field("客戶", cust), field("倉庫", wh),
      field("付款方式", cash), field("日期", date),
    ]));
    body.appendChild(el("h4", { text: "明細", style: "margin: 16px 0 8px;" }));

    const lines = el("div");
    const items = [];
    const addLine = () => {
      const idx = items.length;
      const pSel = select(products.map(p => ({ value: p.id, label: `${p.code} ${p.name}（庫存 ${p.stock}）`, _price: p.price })));
      const qty = input({ type: "number", min: 1, value: 1 });
      const price = input({ type: "number", min: 0, step: "0.01", value: products[0]?.price || 0 });
      pSel.onchange = () => { const opt = pSel.selectedOptions[0]; price.value = products.find(p => p.id == pSel.value)?.price || 0; };
      const del = el("button", { class: "btn btn-danger btn-sm", text: "✕", onclick: () => { items[idx] = null; row.remove(); } });
      const row = el("div", { class: "line-row" }, [
        field("商品", pSel), field("數量", qty), field("售價", price), del,
      ]);
      lines.appendChild(row);
      items.push({ getProductId: () => Number(pSel.value), getQty: () => Number(qty.value), getPrice: () => Number(price.value) });
    };
    addLine();
    body.appendChild(lines);
    body.appendChild(el("button", { class: "btn btn-ghost btn-sm", text: "+ 新增明細", style: "margin-top: 8px;", onclick: addLine }));
    body.appendChild(el("div", { class: "full" }, [field("備註", note)]));

    return async () => {
      const payload = {
        customerId: Number(cust.value),
        warehouseId: Number(wh.value),
        isCash: cash.value === "true",
        date: date.value, note: note.value,
        items: items.filter(Boolean).map(i => ({ productId: i.getProductId(), qty: i.getQty(), unitPrice: i.getPrice() }))
      };
      if (!payload.items.length) throw new Error("至少一項明細");
      const r = await API.createSale(payload);
      toast(`銷貨單 ${r.number} 已建立`, "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Payments / Receipts */
/* ------------------------------------------------------------ */
Views.payments = async () => {
  const [rows, suppliers] = await Promise.all([API.listPayments(), API.listSuppliers()]);
  return cashList("付款（應付沖銷）", "+ 新增付款", rows, "supplierName",
    () => openCashModal("付款", suppliers, "供應商", API.createPayment, "supplierId"),
    (id) => API.cancelPayment(id));
};

Views.receipts = async () => {
  const [rows, customers] = await Promise.all([API.listReceipts(), API.listCustomers()]);
  return cashList("收款（應收沖銷）", "+ 新增收款", rows, "customerName",
    () => openCashModal("收款", customers, "客戶", API.createReceipt, "customerId"),
    (id) => API.cancelReceipt(id));
};

function cashList(title, addLabel, rows, partyKey, onAdd, onCancel) {
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: title }));

  const card = el("div", { class: "card" });
  card.appendChild(el("div", { class: "card-h" }, [
    el("h3", { text: `共 ${rows.length} 筆` }),
    el("button", { class: "btn btn-primary", text: addLabel, onclick: onAdd })
  ]));

  const table = el("table", { class: "data" });
  table.innerHTML = `<thead><tr><th>單號</th><th>日期</th><th>對象</th><th class="num">金額</th><th>方式</th><th>狀態</th><th></th></tr></thead>`;
  const tbody = el("tbody");
  rows.forEach(r => {
    const tr = el("tr");
    tr.innerHTML = `<td>${r.number}</td><td>${fmtDate(r.date)}</td><td>${r[partyKey]}</td>
      <td class="num">${fmtMoney(r.amount)}</td><td>${r.method}</td><td></td><td></td>`;
    tr.children[5].appendChild(pill(r.status === "Active" ? "有效" : "已作廢", r.status === "Active" ? "ok" : "bad"));
    if (r.status === "Active") {
      tr.children[6].appendChild(el("button", { class: "btn btn-danger btn-sm", text: "作廢",
        onclick: () => confirmModal(`確定作廢 ${r.number}？`, async () => { await onCancel(r.id); toast("已作廢", "ok"); router.refresh(); }) }));
    }
    tbody.appendChild(tr);
  });
  table.appendChild(tbody);
  card.appendChild(table);
  root.appendChild(card);
  return root;
}

function openCashModal(title, parties, partyLabel, createFn, partyKey) {
  openModal(`新增${title}`, (body) => {
    const party = select(parties.map(p => ({ value: p.id, label: `${p.code} ${p.name}` })));
    const amount = input({ type: "number", min: 0, step: "0.01", value: 0 });
    const method = select([
      { value: "現金", label: "現金" }, { value: "電匯", label: "電匯" },
      { value: "支票", label: "支票" }, { value: "ATM", label: "ATM" },
    ]);
    const date = input({ type: "date", value: todayISO() });
    const note = input({ value: "" });

    body.appendChild(el("div", { class: "form" }, [
      field(partyLabel, party), field("金額", amount),
      field("方式", method), field("日期", date),
      field("備註", note),
    ]));

    return async () => {
      const payload = { [partyKey]: Number(party.value), amount: Number(amount.value),
        method: method.value, date: date.value, note: note.value };
      const r = await createFn(payload);
      toast(`${title} ${r.number} 已建立`, "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Inventory + Stock Movements */
/* ------------------------------------------------------------ */
Views.inventory = async () => {
  const [products, movements] = await Promise.all([API.listProducts(), API.listStockMovements()]);
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "庫存管理" }));

  const card = el("div", { class: "card" });
  card.appendChild(el("div", { class: "card-h" }, [
    el("h3", { text: "庫存查詢" }),
    el("button", { class: "btn btn-primary", text: "盤點調整", onclick: () => adjustStock(products) })
  ]));
  products.forEach(p => { p.stockValue = p.stock * p.avgCost; });
  card.appendChild(searchableTable({
    rows: products,
    columns: [
      { key: "code", label: "編號" },
      { key: "name", label: "商品" },
      { key: "category", label: "類別" },
      { key: "stock", label: "庫存", num: true, format: fmtNumber },
      { key: "avgCost", label: "平均成本", num: true, format: fmtMoney },
      { key: "stockValue", label: "存貨價值", num: true, format: fmtMoney },
      { key: "stock", label: "狀態", format: (_, p) => p.stock < p.safetyStock
          ? `<span class="pill pill-warn">低於安全庫存</span>`
          : `<span class="pill pill-ok">正常</span>` },
    ],
    filterKeys: ["code", "name", "category"],
  }));
  root.appendChild(card);

  const card2 = el("div", { class: "card" });
  card2.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "近期庫存異動" })]));
  const tb2 = el("table", { class: "data" });
  tb2.innerHTML = `<thead><tr><th>日期</th><th>商品</th><th>倉庫</th><th>類型</th>
    <th class="num">數量</th><th class="num">成本</th><th>來源</th><th>備註</th></tr></thead>`;
  const body2 = el("tbody");
  movements.slice(0, 100).forEach(m => {
    body2.appendChild(el("tr", { html: `<td>${fmtDate(m.date)}</td><td>${m.productName}</td>
      <td>${m.warehouseName}</td><td>${m.type}</td>
      <td class="num">${fmtNumber(m.qty)}</td>
      <td class="num">${fmtMoney(m.unitCost)}</td>
      <td>${m.refType}</td><td>${m.note || ""}</td>` }));
  });
  tb2.appendChild(body2);
  card2.appendChild(tb2);
  root.appendChild(card2);
  return root;
};

async function adjustStock(products) {
  const warehouses = await API.listWarehouses();
  openModal("盤點調整", (body) => {
    const prod = select(products.map(p => ({ value: p.id, label: `${p.code} ${p.name}（庫存 ${p.stock}）` })));
    const wh = select(warehouses.map(w => ({ value: w.id, label: w.name })));
    const delta = input({ type: "number", value: 0 });
    const note = input({ value: "盤點" });
    body.appendChild(el("div", { class: "form" }, [
      field("商品", prod), field("倉庫", wh),
      field("調整數量（正盤盈 / 負盤虧）", delta), field("備註", note),
    ]));
    return async () => {
      await API.adjustStock({ productId: Number(prod.value), warehouseId: Number(wh.value),
        delta: Number(delta.value), note: note.value });
      toast("調整完成", "ok");
      router.refresh();
    };
  });
}

/* ------------------------------------------------------------ */
/* Journal */
/* ------------------------------------------------------------ */
Views.journal = async () => {
  const rows = await API.listJournals();
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "會計分錄" }));
  root.appendChild(el("p", { class: "page-sub", text: `共 ${rows.length} 筆分錄。所有事件自動產生借貸平衡分錄。` }));

  rows.slice(0, 50).forEach(j => {
    const card = el("div", { class: "card", style: "padding: 14px 18px; margin-bottom: 10px;" });
    card.appendChild(el("div", { class: "card-h" }, [
      el("div", {}, [
        el("div", { style: "font-weight:600;", text: `${j.number} · ${j.description}` }),
        el("div", { style: "font-size:12px; color:#6b709b; margin-top:2px;", text: `${fmtDate(j.date)} · ${j.refType}${j.isReversal ? " · 沖銷" : ""}` }),
      ]),
    ]));
    const t = el("table", { class: "data" });
    t.innerHTML = `<thead><tr><th>科目</th><th>說明</th><th class="num">借方</th><th class="num">貸方</th></tr></thead>`;
    const tb = el("tbody");
    let dr = 0, cr = 0;
    j.lines.forEach(l => {
      tb.appendChild(el("tr", { html: `<td>${l.accountCode} ${l.accountName || ""}</td>
        <td>${l.description}</td>
        <td class="num">${l.debit > 0 ? fmtMoney(l.debit) : ""}</td>
        <td class="num">${l.credit > 0 ? fmtMoney(l.credit) : ""}</td>` }));
      dr += l.debit; cr += l.credit;
    });
    tb.appendChild(el("tr", { html: `<td colspan="2" style="font-weight:600;text-align:right;">合計</td>
      <td class="num" style="font-weight:600;">${fmtMoney(dr)}</td>
      <td class="num" style="font-weight:600;">${fmtMoney(cr)}</td>` }));
    t.appendChild(tb);
    card.appendChild(t);
    root.appendChild(card);
  });

  return root;
};

/* ------------------------------------------------------------ */
/* Reports */
/* ------------------------------------------------------------ */
Views.reports = async () => {
  const today = todayISO();
  const from = new Date(); from.setDate(from.getDate() - 90);
  const fromISO = from.toISOString().slice(0, 10);

  const [gp, rk, tov, tb] = await Promise.all([
    API.grossProfit(fromISO, today),
    API.ranking(10),
    API.turnover(),
    API.trialBalance(fromISO, today),
  ]);

  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "報表分析" }));
  root.appendChild(el("p", { class: "page-sub", text: "後端即時計算。預設區間：近 90 天。" }));

  // Gross profit
  const c1 = el("div", { class: "card" });
  c1.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "毛利分析（依商品）" })]));
  c1.appendChild(metricRow([
    ["收入", fmtMoney(gp.totalRevenue)], ["成本", fmtMoney(gp.totalCost)],
    ["毛利", fmtMoney(gp.totalProfit)],
    ["毛利率", (gp.totalRevenue === 0 ? "0" : (gp.totalProfit / gp.totalRevenue * 100).toFixed(1)) + "%"],
  ]));
  const t1 = el("table", { class: "data", style: "margin-top: 12px;" });
  t1.innerHTML = `<thead><tr><th>商品</th><th class="num">數量</th>
    <th class="num">收入</th><th class="num">成本</th><th class="num">毛利</th></tr></thead>`;
  const b1 = el("tbody");
  gp.rows.forEach(r => b1.appendChild(el("tr", { html: `<td>${r.product}</td>
    <td class="num">${fmtNumber(r.qty)}</td>
    <td class="num">${fmtMoney(r.revenue)}</td>
    <td class="num">${fmtMoney(r.cost)}</td>
    <td class="num">${fmtMoney(r.profit)}</td>` })));
  t1.appendChild(b1);
  c1.appendChild(t1);
  root.appendChild(c1);

  // Ranking
  const c2 = el("div", { class: "card" });
  c2.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "商品銷售排行 Top 10" })]));
  const t2 = el("table", { class: "data" });
  t2.innerHTML = `<thead><tr><th>#</th><th>商品</th><th class="num">數量</th><th class="num">收入</th></tr></thead>`;
  const b2 = el("tbody");
  rk.ranking.forEach((r, i) => b2.appendChild(el("tr", {
    html: `<td>${i + 1}</td><td>${r.product}</td><td class="num">${fmtNumber(r.qty)}</td><td class="num">${fmtMoney(r.revenue)}</td>`
  })));
  t2.appendChild(b2);
  c2.appendChild(t2);
  root.appendChild(c2);

  // Turnover
  const c3 = el("div", { class: "card" });
  c3.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "庫存週轉" })]));
  const t3 = el("table", { class: "data" });
  t3.innerHTML = `<thead><tr><th>編號</th><th>商品</th><th class="num">庫存</th>
    <th class="num">已售</th><th class="num">週轉率</th><th class="num">存貨價值</th></tr></thead>`;
  const b3 = el("tbody");
  tov.rows.forEach(r => b3.appendChild(el("tr", { html: `<td>${r.code}</td><td>${r.name}</td>
    <td class="num">${fmtNumber(r.stock)}</td>
    <td class="num">${fmtNumber(r.sold)}</td>
    <td class="num">${Number(r.turnover).toFixed(2)}</td>
    <td class="num">${fmtMoney(r.value)}</td>` })));
  t3.appendChild(b3);
  c3.appendChild(t3);
  root.appendChild(c3);

  // Trial balance
  const c4 = el("div", { class: "card" });
  c4.appendChild(el("div", { class: "card-h" }, [
    el("h3", { text: "試算表" }),
    pill(tb.balanced ? "借貸平衡 ✓" : "不平衡 ✗", tb.balanced ? "ok" : "bad"),
  ]));
  const t4 = el("table", { class: "data" });
  t4.innerHTML = `<thead><tr><th>科目</th><th>名稱</th><th>類型</th>
    <th class="num">借方</th><th class="num">貸方</th><th class="num">餘額</th></tr></thead>`;
  const b4 = el("tbody");
  tb.rows.forEach(r => b4.appendChild(el("tr", { html: `<td>${r.code}</td><td>${r.name}</td>
    <td>${r.type}</td>
    <td class="num">${fmtMoney(r.debit)}</td>
    <td class="num">${fmtMoney(r.credit)}</td>
    <td class="num">${fmtMoney(r.balance)}</td>` })));
  b4.appendChild(el("tr", { html: `<td colspan="3" style="font-weight:700;text-align:right;">合計</td>
    <td class="num" style="font-weight:700;">${fmtMoney(tb.totalDebit)}</td>
    <td class="num" style="font-weight:700;">${fmtMoney(tb.totalCredit)}</td><td></td>` }));
  t4.appendChild(b4);
  c4.appendChild(t4);
  root.appendChild(c4);

  return root;
};

function metricRow(items) {
  return el("div", { class: "row" }, items.map(([lbl, val]) =>
    el("div", { class: "metric" }, [
      el("div", { class: "lbl", text: lbl }),
      el("div", { class: "val", text: val, style: "font-size:18px;" }),
    ])));
}

/* ------------------------------------------------------------ */
/* Settings */
/* ------------------------------------------------------------ */
Views.settings = async () => {
  const root = el("div");
  root.appendChild(el("h1", { class: "page-h", text: "系統設定" }));

  const c1 = el("div", { class: "card" });
  c1.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "資料管理" })]));
  c1.appendChild(el("p", { text: "重置示範資料：清空所有資料表後重新植入種子。", style: "color:#5a627d;" }));
  c1.appendChild(el("button", { class: "btn btn-danger", text: "重置示範資料",
    onclick: () => confirmModal("確定要清空資料並重新植入嗎？", async () => {
      await API.reset(); toast("已重置", "ok"); router.refresh();
    })
  }));
  root.appendChild(c1);

  const c2 = el("div", { class: "card" });
  c2.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "API 工具" })]));
  c2.appendChild(el("p", { html: 'Swagger 文件：<a href="/swagger" target="_blank">/swagger</a>（僅 Development 環境啟用）' }));
  c2.appendChild(el("p", { html: '健康檢查：<a href="/api/system/health" target="_blank">/api/system/health</a>' }));
  root.appendChild(c2);

  const c3 = el("div", { class: "card" });
  c3.appendChild(el("div", { class: "card-h" }, [el("h3", { text: "會計科目" })]));
  const accounts = await API.listAccounts();
  const t = el("table", { class: "data" });
  t.innerHTML = `<thead><tr><th>代號</th><th>名稱</th><th>類型</th></tr></thead>`;
  const tb = el("tbody");
  accounts.forEach(a => tb.appendChild(el("tr", { html: `<td>${a.code}</td><td>${a.name}</td><td>${a.type}</td>` })));
  t.appendChild(tb);
  c3.appendChild(t);
  root.appendChild(c3);

  return root;
};

window.Views = Views;
