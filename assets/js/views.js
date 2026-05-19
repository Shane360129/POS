/* ============================================================
   InvenFlow — Views
   View renderers for each route. Each view returns an HTMLElement.
   ============================================================ */

const Icons = {
  dashboard: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="9"/><rect x="14" y="3" width="7" height="5"/><rect x="14" y="12" width="7" height="9"/><rect x="3" y="16" width="7" height="5"/></svg>',
  package:   '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16.5 9.4 7.55 4.24"/><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><path d="m3.27 6.96 8.73 5.05 8.73-5.05"/><path d="M12 22.08V12"/></svg>',
  truck:     '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10 17h4V5H2v12h3"/><path d="M20 17h2v-3.34a4 4 0 0 0-1.17-2.83L19 9h-5"/><circle cx="7.5" cy="17.5" r="2.5"/><circle cx="17.5" cy="17.5" r="2.5"/></svg>',
  users:     '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
  building:  '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 22V12h6v10"/></svg>',
  shop:      '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m2 7 4.41-4.41A2 2 0 0 1 7.83 2h8.34a2 2 0 0 1 1.41.58L22 7"/><path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"/><path d="M15 22v-4a2 2 0 0 0-2-2h-2a2 2 0 0 0-2 2v4"/><path d="M2 7h20"/></svg>',
  cart:      '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg>',
  receipt:   '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 2v20l2-2 2 2 2-2 2 2 2-2 2 2 2-2 2 2V2l-2 2-2-2-2 2-2-2-2 2-2-2-2 2L4 2z"/><path d="M16 8h-6a2 2 0 1 0 0 4h4a2 2 0 1 1 0 4H8"/><path d="M12 17.5v-11"/></svg>',
  warehouse: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 8.35V20a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V8.35A2 2 0 0 1 3.26 6.5l8-3.2a2 2 0 0 1 1.48 0l8 3.2A2 2 0 0 1 22 8.35Z"/><path d="M6 18h12"/><path d="M6 14h12"/><path d="M6 10h12"/></svg>',
  chart:     '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 3v18h18"/><path d="m7 14 4-4 4 4 6-6"/></svg>',
  ledger:    '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/></svg>',
  settings:  '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09a1.65 1.65 0 0 0 1.51-1 1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33h0a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51h0a1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82v0a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>',
  plus:      '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>',
  trash:     '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6 17.45 19.74A2 2 0 0 1 15.46 21H8.54a2 2 0 0 1-1.99-1.26L5 6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2"/></svg>',
  edit:      '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>',
  search:    '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>',
  bell:      '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>',
  close:     '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>',
  arrowUp:   '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="19" x2="12" y2="5"/><polyline points="5 12 12 5 19 12"/></svg>',
  arrowDown: '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><polyline points="19 12 12 19 5 12"/></svg>',
};

/* ============================================================
   Generic helpers
   ============================================================ */
const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

function el(tag, props = {}, children = []) {
  const e = document.createElement(tag);
  Object.entries(props).forEach(([k, v]) => {
    if (k === "class") e.className = v;
    else if (k === "html") e.innerHTML = v;
    else if (k === "text") e.textContent = v;
    else if (k.startsWith("on")) e.addEventListener(k.slice(2).toLowerCase(), v);
    else if (k === "value") e.value = v;
    else if (k === "checked") e.checked = !!v;
    else if (v !== null && v !== undefined && v !== false) e.setAttribute(k, v);
  });
  (Array.isArray(children) ? children : [children]).forEach(c => {
    if (c == null || c === false) return;
    if (typeof c === "string") e.appendChild(document.createTextNode(c));
    else e.appendChild(c);
  });
  return e;
}

function toast(msg, type = "success") {
  const t = el("div", { class: `toast ${type}` }, [msg]);
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 2400);
}

function openModal({ title, body, size = "", actions = [], onClose }) {
  const close = () => { backdrop.remove(); onClose && onClose(); };
  const headerActions = el("button", { class: "modal-close", html: Icons.close, onclick: close });
  const head = el("div", { class: "modal-head" }, [el("h2", { text: title }), headerActions]);
  const bodyWrap = el("div", { class: "modal-body" });
  if (typeof body === "string") bodyWrap.innerHTML = body;
  else if (body) bodyWrap.appendChild(body);
  const footEls = actions.map(a => {
    const btn = el("button", { class: `btn ${a.variant || ""}`, text: a.label });
    btn.addEventListener("click", () => a.onClick && a.onClick(close));
    return btn;
  });
  const foot = footEls.length ? el("div", { class: "modal-foot" }, footEls) : null;
  const modal = el("div", { class: `modal ${size}` }, [head, bodyWrap, foot].filter(Boolean));
  const backdrop = el("div", { class: "modal-backdrop" }, [modal]);
  backdrop.addEventListener("click", (e) => { if (e.target === backdrop) close(); });
  document.body.appendChild(backdrop);
  return { close, body: bodyWrap, modal };
}

function pill(state) {
  if (!state) return el("span", { class: "pill", text: "—" });
  return el("span", { class: `pill ${state.pill || ""}` }, [
    el("span", { class: "dot" }), state.label,
  ]);
}

function tableEmpty(colspan, text = "目前沒有資料") {
  return el("tr", {}, [el("td", { colspan, class: "empty" }, [
    el("div", { class: "empty" }, [
      el("div", { class: "icon-circle", html: Icons.search }),
      el("h4", { text: "尚無資料" }),
      el("p", { text }),
    ]),
  ])]);
}

/* ============================================================
   Top-level views
   ============================================================ */
const Views = {

  /* ------------------ Dashboard ------------------ */
  dashboard(db) {
    const today = todayISO();
    const monthStart = today.slice(0, 7) + "-01";
    const month = Logic.rangeSales(db, monthStart, today);
    const last30 = Logic.rangeSales(db, isoDaysAgo(30), today);

    const invValue = Logic.inventoryValue(db);
    const ar = Logic.totalAR(db);
    const ap = Logic.totalAP(db);
    const skuLow = db.products.filter(p => {
      const inv = db.inventoryState[p.id];
      return inv && inv.qty <= 30 && inv.qty > 0;
    });
    const skuOut = db.products.filter(p => {
      const inv = db.inventoryState[p.id];
      return !inv || inv.qty <= 0;
    });

    const kpis = el("div", { class: "kpi-grid" }, [
      kpiCard({ label: "本月銷售淨額", icon: Icons.chart, value: `NT$ ${fmtMoney(month.revenue)}`, trend: `${month.orders} 筆訂單` }),
      kpiCard({ tone: "emerald", label: "本月毛利", icon: Icons.receipt, value: `NT$ ${fmtMoney(month.gross)}`, trend: month.revenue > 0 ? `毛利率 ${((month.gross / month.revenue) * 100).toFixed(1)}%` : "—" }),
      kpiCard({ tone: "amber", label: "庫存帳面總值 (WAC)", icon: Icons.package, value: `NT$ ${fmtMoney(invValue)}`, trend: `${db.products.length} 個品項` }),
      kpiCard({ tone: "rose", label: "應收 / 應付", icon: Icons.ledger, value: `NT$ ${fmtMoney(ar)} / ${fmtMoney(ap)}`, trend: "客戶 / 供應商" }),
    ]);

    // Last 14-day sales trend
    const days = [];
    for (let i = 13; i >= 0; i--) days.push(isoDaysAgo(i));
    const byDate = {}; days.forEach(d => byDate[d] = { rev: 0, cogs: 0 });
    db.sales.forEach(s => {
      if (s.status === "void") return;
      if (byDate[s.date]) { byDate[s.date].rev += s.total; byDate[s.date].cogs += s.cogs; }
    });

    const trendChart = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [
        el("div", {}, [
          el("h3", { text: "近 14 日銷售與毛利" }),
          el("div", { class: "sub", text: "依出貨日加總；含應收與現金交易，排除作廢單" }),
        ]),
        el("div", { class: "flex-row" }, [
          legendDot("#6366f1", "銷售淨額"),
          legendDot("#10b981", "毛利"),
        ]),
      ]),
      el("div", { class: "card-body" }, [renderSalesTrendChart(days, byDate)]),
    ]);

    // Top products by gross profit (last 60 days)
    const top = computeTopProducts(db, 60);
    const topCard = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [
        el("div", {}, [
          el("h3", { text: "近 60 日熱銷商品 (按毛利)" }),
          el("div", { class: "sub", text: "前 6 名，含成本以 WAC 計算" }),
        ]),
      ]),
      el("div", { class: "card-body" }, [renderTopProductsBar(top)]),
    ]);

    // Stock alert
    const alertCard = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [
        el("div", {}, [
          el("h3", { text: "庫存警示" }),
          el("div", { class: "sub", text: "低庫存 ≤ 30 件 / 無庫存" }),
        ]),
        el("button", { class: "btn sm ghost", text: "查看全部", onclick: () => router.go("inventory") }),
      ]),
      el("div", { class: "card-body flush" }, [stockAlertTable(db, skuOut, skuLow)]),
    ]);

    // Recent activity
    const recent = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [
        el("div", {}, [
          el("h3", { text: "最近交易" }),
          el("div", { class: "sub", text: "最新 8 筆進銷紀錄" }),
        ]),
        el("button", { class: "btn sm ghost", text: "查看全部", onclick: () => router.go("sales") }),
      ]),
      el("div", { class: "card-body flush" }, [recentActivityList(db)]),
    ]);

    return el("div", {}, [
      pageHead({
        title: "儀表板",
        subtitle: "InvenFlow 即時掌握您今日的庫存、銷售與會計狀態",
      }),
      kpis,
      trendChart,
      el("div", { style: "height:16px" }),
      el("div", { class: "grid-2" }, [topCard, alertCard]),
      el("div", { style: "height:16px" }),
      recent,
    ]);
  },

  /* ------------------ Products ------------------ */
  products(db) {
    const state = { filter: "", category: "", status: "" };
    const root = el("div");
    const cardBody = el("div", { class: "card-body flush" });

    const refresh = () => {
      cardBody.innerHTML = "";
      const rows = db.products.filter(p => {
        if (state.filter && !(p.name.includes(state.filter) || p.sku.toLowerCase().includes(state.filter.toLowerCase()))) return false;
        if (state.category && p.category !== state.category) return false;
        if (state.status && p.status !== state.status) return false;
        return true;
      });
      cardBody.appendChild(productsTable(db, rows));
    };

    const categories = Array.from(new Set(db.products.map(p => p.category)));
    const toolbar = el("div", { class: "toolbar" }, [
      field("關鍵字", inputEl({ placeholder: "搜尋商品名稱 / SKU…", oninput: (e) => { state.filter = e.target.value; refresh(); } })),
      field("分類", selectEl([["", "全部分類"], ...categories.map(c => [c, c])], "", (v) => { state.category = v; refresh(); })),
      field("狀態", selectEl([
        ["", "全部"],
        ...Object.entries(Data.StatusMap.productStatus).map(([k, v]) => [k, v.label]),
      ], "", (v) => { state.status = v; refresh(); })),
      el("div", { class: "spacer" }),
      el("button", {
        class: "btn primary",
        html: `${Icons.plus} <span style="margin-left:4px">新增商品</span>`,
        onclick: () => openProductForm(db, null, () => { refresh(); router.refresh(); }),
      }),
    ]);

    root.append(
      pageHead({
        title: "商品主檔",
        subtitle: "商品管理、平均成本與目前庫存",
      }),
      el("div", { class: "card" }, [toolbar, cardBody]),
    );
    refresh();
    return root;
  },

  /* ------------------ Suppliers ------------------ */
  suppliers(db) {
    const root = el("div");
    const tbody = el("tbody");

    const refresh = () => {
      tbody.innerHTML = "";
      db.suppliers.forEach(s => {
        const ap = db.apBalance[s.id] || 0;
        tbody.appendChild(el("tr", {}, [
          el("td", {}, [
            el("div", { class: "title-w-sub" }, [
              el("strong", { text: s.name }),
              el("span", { class: "muted", text: `${s.id} · ${s.taxId}` }),
            ]),
          ]),
          el("td", { text: s.contact }),
          el("td", { text: s.phone }),
          el("td", { text: s.terms }),
          el("td", { class: "num" }, [
            ap > 0
              ? el("span", { class: "text-warning mono", text: `NT$ ${fmtMoney(ap)}` })
              : el("span", { class: "muted", text: "—" }),
          ]),
          el("td", { class: "actions" }, [
            el("button", { class: "btn sm", html: Icons.edit + " 編輯", onclick: () => openPartnerForm(db, "supplier", s, () => { refresh(); }) }),
          ]),
        ]));
      });
      if (!tbody.children.length) tbody.appendChild(tableEmpty(6));
    };

    root.append(
      pageHead({
        title: "供應商主檔",
        subtitle: "管理採購來源與應付餘額",
        actions: [
          el("button", {
            class: "btn primary",
            html: `${Icons.plus} <span style="margin-left:4px">新增供應商</span>`,
            onclick: () => openPartnerForm(db, "supplier", null, () => { refresh(); }),
          }),
        ],
      }),
      el("div", { class: "card" }, [
        el("table", { class: "tbl" }, [
          el("thead", {}, [
            el("tr", {}, ["供應商", "聯絡人", "電話", "付款條件", "應付餘額", "操作"]
              .map(t => el("th", { class: t === "應付餘額" ? "num" : "" }, [t]))),
          ]),
          tbody,
        ]),
      ]),
    );
    refresh();
    return root;
  },

  /* ------------------ Customers ------------------ */
  customers(db) {
    const root = el("div");
    const tbody = el("tbody");
    const refresh = () => {
      tbody.innerHTML = "";
      db.customers.forEach(c => {
        const ar = db.arBalance[c.id] || 0;
        tbody.appendChild(el("tr", {}, [
          el("td", {}, [
            el("div", { class: "title-w-sub" }, [
              el("strong", { text: c.name }),
              el("span", { class: "muted", text: `${c.id} · ${c.taxId}` }),
            ]),
          ]),
          el("td", {}, [pill({ label: c.type, pill: c.type === "批發" ? "primary" : c.type === "通路" ? "info" : "" })]),
          el("td", { text: c.contact }),
          el("td", { text: c.phone }),
          el("td", { text: c.terms }),
          el("td", { class: "num" }, [
            ar > 0
              ? el("span", { class: "text-warning mono", text: `NT$ ${fmtMoney(ar)}` })
              : el("span", { class: "muted", text: "—" }),
          ]),
          el("td", { class: "actions" }, [
            el("button", { class: "btn sm", html: Icons.edit + " 編輯", onclick: () => openPartnerForm(db, "customer", c, () => refresh()) }),
          ]),
        ]));
      });
      if (!tbody.children.length) tbody.appendChild(tableEmpty(7));
    };

    root.append(
      pageHead({
        title: "客戶主檔",
        subtitle: "管理銷售對象與應收餘額",
        actions: [
          el("button", {
            class: "btn primary",
            html: `${Icons.plus} <span style="margin-left:4px">新增客戶</span>`,
            onclick: () => openPartnerForm(db, "customer", null, () => refresh()),
          }),
        ],
      }),
      el("div", { class: "card" }, [
        el("table", { class: "tbl" }, [
          el("thead", {}, [
            el("tr", {}, ["客戶", "類型", "聯絡人", "電話", "付款條件", "應收餘額", "操作"]
              .map(t => el("th", { class: t === "應收餘額" ? "num" : "" }, [t]))),
          ]),
          tbody,
        ]),
      ]),
    );
    refresh();
    return root;
  },

  /* ------------------ Warehouses ------------------ */
  warehouses(db) {
    const root = el("div");
    const grid = el("div", { class: "grid-3" });

    db.warehouses.forEach(w => {
      // count items + value for this warehouse
      const totals = computeWarehouseTotals(db, w.id);
      grid.appendChild(
        el("div", { class: "card" }, [
          el("div", { class: "card-body" }, [
            el("div", { class: "flex-between", style: "margin-bottom:14px" }, [
              el("div", { class: "title-w-sub" }, [
                el("strong", { text: w.name, style: "font-size:15px" }),
                el("span", { class: "muted", text: `${w.id} · ${w.location}` }),
              ]),
              el("div", { class: "kpi-bubble" }, [pill({ label: "啟用中", pill: "ok" })]),
            ]),
            el("div", { class: "form-row cols-3" }, [
              kvBlock("近 30 日入庫", fmtNumber(totals.in30)),
              kvBlock("近 30 日出庫", fmtNumber(totals.out30)),
              kvBlock("異動筆數", fmtNumber(totals.movements)),
            ]),
          ]),
        ]),
      );
    });

    root.append(
      pageHead({
        title: "倉庫管理",
        subtitle: "InvenFlow 支援多倉異動紀錄；本 Demo 庫存採全公司平均成本，倉庫僅作為儲位標記",
      }),
      grid,
    );
    return root;
  },

  /* ------------------ Purchases ------------------ */
  purchases(db) {
    const root = el("div");
    const cardBody = el("div", { class: "card-body flush" });
    const state = { status: "", supplier: "" };

    const refresh = () => {
      cardBody.innerHTML = "";
      cardBody.appendChild(purchasesTable(db, state));
    };

    const toolbar = el("div", { class: "toolbar" }, [
      field("狀態", selectEl([
        ["", "全部狀態"],
        ...Object.entries(Data.StatusMap.purchaseStatus).map(([k, v]) => [k, v.label]),
      ], "", v => { state.status = v; refresh(); })),
      field("供應商", selectEl(
        [["", "全部供應商"], ...db.suppliers.map(s => [s.id, s.name])],
        "", v => { state.supplier = v; refresh(); },
      )),
      el("div", { class: "spacer" }),
      el("button", {
        class: "btn primary",
        html: `${Icons.plus} <span style="margin-left:4px">建立進貨單</span>`,
        onclick: () => openPurchaseForm(db, () => { refresh(); }),
      }),
    ]);

    root.append(
      pageHead({
        title: "進貨作業",
        subtitle: "建立進貨單即同步入庫並更新平均成本與應付帳款",
      }),
      el("div", { class: "card" }, [toolbar, cardBody]),
    );
    refresh();
    return root;
  },

  /* ------------------ Sales ------------------ */
  sales(db) {
    const root = el("div");
    const cardBody = el("div", { class: "card-body flush" });
    const state = { status: "", customer: "" };

    const refresh = () => {
      cardBody.innerHTML = "";
      cardBody.appendChild(salesTable(db, state));
    };

    const toolbar = el("div", { class: "toolbar" }, [
      field("狀態", selectEl([
        ["", "全部狀態"],
        ...Object.entries(Data.StatusMap.saleStatus).map(([k, v]) => [k, v.label]),
      ], "", v => { state.status = v; refresh(); })),
      field("客戶", selectEl(
        [["", "全部客戶"], ...db.customers.map(c => [c.id, c.name])],
        "", v => { state.customer = v; refresh(); },
      )),
      el("div", { class: "spacer" }),
      el("button", {
        class: "btn primary",
        html: `${Icons.plus} <span style="margin-left:4px">建立銷售單</span>`,
        onclick: () => openSaleForm(db, () => { refresh(); }),
      }),
    ]);

    root.append(
      pageHead({
        title: "銷售作業",
        subtitle: "出貨同步扣庫存並依當下平均成本記錄銷貨成本 (COGS)",
      }),
      el("div", { class: "card" }, [toolbar, cardBody]),
    );
    refresh();
    return root;
  },

  /* ------------------ Inventory ------------------ */
  inventory(db) {
    const root = el("div");
    const tabs = ["庫存查詢", "庫存異動明細", "庫存調整"];
    let active = 0;
    const tabsEl = el("div", { class: "tabs" });
    const content = el("div");

    function renderActive() {
      tabsEl.innerHTML = "";
      tabs.forEach((t, i) => {
        tabsEl.appendChild(el("div", {
          class: "tab" + (i === active ? " active" : ""),
          text: t,
          onclick: () => { active = i; renderActive(); },
        }));
      });
      content.innerHTML = "";
      if (active === 0) content.appendChild(inventoryStockView(db));
      else if (active === 1) content.appendChild(inventoryLedgerView(db));
      else content.appendChild(inventoryAdjustView(db, () => renderActive()));
    }

    root.append(
      pageHead({
        title: "庫存管理",
        subtitle: "即時庫存、加權平均成本與異動明細",
      }),
      tabsEl, content,
    );
    renderActive();
    return root;
  },

  /* ------------------ Payables ------------------ */
  payables(db) {
    return paymentLikeView(db, "payables");
  },
  receivables(db) {
    return paymentLikeView(db, "receivables");
  },

  /* ------------------ Reports ------------------ */
  reports(db) {
    const root = el("div");
    const tabs = ["毛利分析", "商品銷售排行", "庫存週轉", "試算表"];
    let active = 0;
    const tabsEl = el("div", { class: "tabs" });
    const content = el("div");
    function render() {
      tabsEl.innerHTML = "";
      tabs.forEach((t, i) => {
        tabsEl.appendChild(el("div", {
          class: "tab" + (i === active ? " active" : ""),
          text: t, onclick: () => { active = i; render(); },
        }));
      });
      content.innerHTML = "";
      if (active === 0) content.appendChild(reportGrossMargin(db));
      else if (active === 1) content.appendChild(reportProductRanking(db));
      else if (active === 2) content.appendChild(reportInventoryTurnover(db));
      else content.appendChild(reportTrialBalance(db));
    }
    root.append(pageHead({
      title: "報表分析",
      subtitle: "依會計分錄與庫存異動推算的營運指標",
    }), tabsEl, content);
    render();
    return root;
  },

  /* ------------------ Journal ------------------ */
  journal(db) {
    const root = el("div");
    const tbody = el("tbody");
    const journals = [...db.journal].sort((a, b) => b.date.localeCompare(a.date));
    journals.forEach(je => {
      const linesHtml = je.lines.map(l => {
        const acct = Object.values(Data.accounts).find(a => a.id === l.account) || { id: l.account, name: l.account };
        return `<div style="display:grid;grid-template-columns:90px 1fr 100px 100px;gap:8px;font-size:12px;padding:2px 0;">
                  <span class="muted mono">${acct.id}</span>
                  <span>${acct.name}</span>
                  <span class="num mono">${l.debit ? fmtMoney(l.debit) : "&nbsp;"}</span>
                  <span class="num mono">${l.credit ? fmtMoney(l.credit) : "&nbsp;"}</span>
                </div>`;
      }).join("");
      const totalDr = je.lines.reduce((s, l) => s + (l.debit || 0), 0);
      tbody.appendChild(el("tr", {}, [
        el("td", { text: je.date }),
        el("td", {}, [
          el("div", { class: "title-w-sub" }, [
            el("strong", { text: je.memo }),
            el("span", { class: "muted", text: `${je.refType} · ${je.refId}` }),
          ]),
        ]),
        el("td", { html: `<div style="display:grid;grid-template-columns:90px 1fr 100px 100px;gap:8px;font-size:11px;color:var(--c-text-muted);font-weight:600;padding-bottom:4px;border-bottom:1px solid var(--c-border-soft);margin-bottom:4px;"><span>科目</span><span>名稱</span><span class="num">借方</span><span class="num">貸方</span></div>${linesHtml}` }),
        el("td", { class: "num mono", text: `NT$ ${fmtMoney(totalDr)}` }),
      ]));
    });
    if (!tbody.children.length) tbody.appendChild(tableEmpty(4));

    root.append(
      pageHead({
        title: "會計分錄",
        subtitle: "由各交易事件自動產生，借貸自動平衡",
      }),
      el("div", { class: "card" }, [
        el("table", { class: "tbl" }, [
          el("thead", {}, [el("tr", {}, ["日期", "說明", "明細", "金額"].map(t =>
            el("th", { class: t === "金額" ? "num" : "" }, [t])))]),
          tbody,
        ]),
      ]),
    );
    return root;
  },

  /* ------------------ Settings ------------------ */
  settings(db) {
    const c = db.company;
    const root = el("div");

    const form = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [el("h3", { text: "公司資訊" })]),
      el("div", { class: "card-body" }, [
        el("div", { class: "form-row" }, [
          formGroup("公司名稱", inputEl({ value: c.name, oninput: (e) => { c.name = e.target.value; } })),
          formGroup("統一編號", inputEl({ value: c.taxId, oninput: (e) => { c.taxId = e.target.value; } })),
        ]),
        el("div", { class: "form-row" }, [
          formGroup("電話", inputEl({ value: c.phone, oninput: (e) => { c.phone = e.target.value; } })),
          formGroup("會計年度起", inputEl({ type: "month", value: c.fiscalStart, oninput: (e) => { c.fiscalStart = e.target.value; } })),
        ]),
        el("div", { class: "form-row cols-1" }, [
          formGroup("地址", inputEl({ value: c.address, oninput: (e) => { c.address = e.target.value; } })),
        ]),
        el("div", { class: "flex-row", style: "justify-content:flex-end" }, [
          el("button", { class: "btn primary", text: "儲存設定", onclick: () => { Data.save(db); toast("公司資訊已儲存"); router.refresh(); } }),
        ]),
      ]),
    ]);

    const dangerZone = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [el("h3", { text: "示範資料" })]),
      el("div", { class: "card-body" }, [
        el("p", { class: "muted", style: "margin:0 0 14px" }, [
          "重置會清除本機的所有交易與主檔，並重新匯入示範資料（含 ~21 張單據與相應的會計分錄）。",
        ]),
        el("button", {
          class: "btn danger",
          text: "重置 Demo 資料",
          onclick: () => {
            if (confirm("確定要重置所有 Demo 資料嗎？此動作不可復原")) {
              Data.reset();
              toast("已重置示範資料");
              setTimeout(() => location.reload(), 600);
            }
          },
        }),
      ]),
    ]);

    const accountsList = el("div", { class: "card" }, [
      el("div", { class: "card-head" }, [el("h3", { text: "會計科目表" })]),
      el("div", { class: "card-body flush" }, [
        el("table", { class: "tbl" }, [
          el("thead", {}, [el("tr", {}, ["科目代碼", "科目名稱", "類型"].map(t => el("th", {}, [t])))]),
          el("tbody", {},
            Object.values(Data.accounts).map(a => el("tr", {}, [
              el("td", { class: "mono", text: a.id }),
              el("td", { text: a.name }),
              el("td", {}, [pill({
                label: { asset: "資產", liability: "負債", equity: "權益", revenue: "收入", expense: "費用" }[a.type] || a.type,
                pill: { asset: "info", liability: "warn", revenue: "ok", expense: "bad" }[a.type] || "",
              })]),
            ]))),
        ]),
      ]),
    ]);

    root.append(pageHead({
      title: "系統設定",
      subtitle: "公司資訊、會計科目與示範資料管理",
    }), form, accountsList, dangerZone);
    return root;
  },
};

/* ============================================================
   Reusable UI fragments
   ============================================================ */
function pageHead({ title, subtitle, actions = [] }) {
  return el("div", { class: "page-head" }, [
    el("div", { class: "title-w-sub" }, [
      el("h1", { text: title }),
      el("div", { class: "subtitle", text: subtitle || "" }),
    ]),
    actions.length ? el("div", { class: "flex-row" }, actions) : null,
  ].filter(Boolean));
}

function kpiCard({ tone = "", label, icon, value, trend }) {
  return el("div", { class: "kpi " + (tone ? "tone-" + tone : "") }, [
    el("div", { class: "label" }, [
      el("span", { class: "icon", html: icon }),
      label,
    ]),
    el("div", { class: "value", text: value }),
    el("div", { class: "trend", text: trend || "" }),
  ]);
}

function kvBlock(label, value) {
  return el("div", { class: "form-group" }, [
    el("label", { text: label }),
    el("div", { class: "mono", style: "font-weight:700;font-size:16px;", text: value }),
  ]);
}

function field(label, input) {
  return el("div", { class: "field" }, [
    el("label", { text: label }),
    input,
  ]);
}
function inputEl(props) {
  const e = el("input", { class: "input" });
  Object.entries(props || {}).forEach(([k, v]) => {
    if (k === "oninput") e.addEventListener("input", v);
    else if (k === "onchange") e.addEventListener("change", v);
    else if (k === "value") e.value = v;
    else e.setAttribute(k, v);
  });
  return e;
}
function selectEl(options, value, onchange) {
  const s = el("select", { class: "input" });
  options.forEach(([v, label]) => {
    const o = el("option", { value: v, text: label });
    if (v === value) o.selected = true;
    s.appendChild(o);
  });
  s.addEventListener("change", e => onchange(e.target.value));
  return s;
}
function formGroup(label, input, hint) {
  return el("div", { class: "form-group" }, [
    el("label", { text: label }),
    input,
    hint ? el("span", { class: "hint", text: hint }) : null,
  ].filter(Boolean));
}
function legendDot(color, label) {
  return el("div", { class: "flex-row" }, [
    el("span", { style: `width:10px;height:10px;border-radius:50%;background:${color};display:inline-block;` }),
    el("span", { class: "muted", style: "font-size:12px", text: label }),
  ]);
}

/* ============================================================
   Charts (SVG, dependency-free)
   ============================================================ */
function renderSalesTrendChart(days, byDate) {
  const W = 760, H = 240, pad = { l: 40, r: 20, t: 20, b: 30 };
  const max = Math.max(1, ...days.map(d => byDate[d].rev));
  const x = i => pad.l + (i * (W - pad.l - pad.r) / (days.length - 1));
  const y = v => H - pad.b - (v / max) * (H - pad.t - pad.b);

  const revPts = days.map((d, i) => [x(i), y(byDate[d].rev)]);
  const grossPts = days.map((d, i) => [x(i), y(byDate[d].rev - byDate[d].cogs)]);

  const revPath = "M" + revPts.map(p => p.join(",")).join(" L");
  const grossPath = "M" + grossPts.map(p => p.join(",")).join(" L");

  const areaPath = revPath + ` L${x(days.length - 1)},${H - pad.b} L${pad.l},${H - pad.b} Z`;

  const grid = [];
  for (let i = 0; i <= 4; i++) {
    const yy = pad.t + i * (H - pad.t - pad.b) / 4;
    grid.push(`<line x1="${pad.l}" x2="${W - pad.r}" y1="${yy}" y2="${yy}" stroke="#f1f5f9" stroke-width="1"/>`);
    const val = max - (i * max / 4);
    grid.push(`<text x="${pad.l - 6}" y="${yy + 3}" font-size="10" fill="#94a3b8" text-anchor="end">${fmtCompact(val)}</text>`);
  }
  const ticks = days.map((d, i) => {
    if (i % 2 !== 0) return "";
    return `<text x="${x(i)}" y="${H - 10}" font-size="10" fill="#94a3b8" text-anchor="middle">${d.slice(5)}</text>`;
  }).join("");

  const dots = revPts.map((p, i) => `<circle cx="${p[0]}" cy="${p[1]}" r="3" fill="#6366f1"/>`).join("");

  return el("div", {
    class: "chart-wrap",
    html: `
      <svg viewBox="0 0 ${W} ${H}" preserveAspectRatio="none" style="width:100%;height:240px;">
        <defs>
          <linearGradient id="grad1" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0%" stop-color="#6366f1" stop-opacity="0.3"/>
            <stop offset="100%" stop-color="#6366f1" stop-opacity="0"/>
          </linearGradient>
        </defs>
        ${grid.join("")}
        <path d="${areaPath}" fill="url(#grad1)" stroke="none"/>
        <path d="${revPath}" fill="none" stroke="#6366f1" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
        <path d="${grossPath}" fill="none" stroke="#10b981" stroke-width="2.2" stroke-dasharray="4 4" stroke-linecap="round" stroke-linejoin="round"/>
        ${dots}
        ${ticks}
      </svg>
    `,
  });
}

function fmtCompact(v) {
  if (v >= 1e6) return (v / 1e6).toFixed(1) + "M";
  if (v >= 1e3) return (v / 1e3).toFixed(0) + "K";
  return Math.round(v);
}

function computeTopProducts(db, days) {
  const since = isoDaysAgo(days);
  const map = {};
  db.sales.forEach(s => {
    if (s.status === "void" || s.date < since) return;
    s.items.forEach(it => {
      if (!map[it.productId]) map[it.productId] = { id: it.productId, qty: 0, revenue: 0, cogs: 0 };
      map[it.productId].qty += it.qty;
      map[it.productId].revenue += it.subtotal;
      map[it.productId].cogs += it.cogs;
    });
  });
  const arr = Object.values(map).map(x => ({ ...x, gross: x.revenue - x.cogs }));
  arr.sort((a, b) => b.gross - a.gross);
  return arr.slice(0, 6).map(x => ({
    ...x, name: productIdToName(db, x.id),
  }));
}

function renderTopProductsBar(items) {
  if (!items.length) return el("div", { class: "empty", text: "尚無銷售紀錄" });
  const max = Math.max(...items.map(i => i.gross));
  const wrap = el("div", { style: "display:flex;flex-direction:column;gap:12px;" });
  items.forEach(item => {
    const pct = max ? (item.gross / max) * 100 : 0;
    const margin = item.revenue ? ((item.gross / item.revenue) * 100).toFixed(1) : 0;
    wrap.appendChild(el("div", {}, [
      el("div", { class: "flex-between", style: "margin-bottom:4px" }, [
        el("span", { style: "font-size:13px;font-weight:600", text: item.name }),
        el("span", { class: "muted mono", text: `NT$ ${fmtMoney(item.gross)} · ${margin}%` }),
      ]),
      el("div", { style: "height:8px;border-radius:999px;background:var(--c-border-soft);overflow:hidden;" }, [
        el("div", { style: `width:${pct}%;height:100%;background:linear-gradient(90deg,#6366f1,#06b6d4);` }),
      ]),
    ]));
  });
  return wrap;
}

function stockAlertTable(db, out, low) {
  const items = [
    ...out.map(p => ({ ...p, kind: "out" })),
    ...low.map(p => ({ ...p, kind: "low" })),
  ].slice(0, 6);
  if (!items.length) {
    return el("div", { class: "empty" }, [el("div", { class: "icon-circle", html: Icons.package }), el("h4", { text: "庫存皆充足" }), el("p", { text: "目前沒有需要警示的品項" })]);
  }
  const tbody = el("tbody");
  items.forEach(p => {
    const inv = db.inventoryState[p.id] || { qty: 0, avgCost: 0 };
    tbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: p.name }),
          el("span", { class: "muted", text: p.sku }),
        ]),
      ]),
      el("td", { class: "num mono", text: fmtNumber(inv.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(inv.avgCost)}` }),
      el("td", {}, [pill({
        label: p.kind === "out" ? "斷貨" : "低庫存",
        pill: p.kind === "out" ? "bad" : "warn",
      })]),
    ]));
  });
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["商品", "庫存", "平均成本", "狀態"].map(t => el("th", { class: t === "庫存" || t === "平均成本" ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

function recentActivityList(db) {
  const events = [];
  db.purchases.forEach(p => events.push({ date: p.date, type: "purchase", refId: p.id, who: supplierIdToName(db, p.supplierId), amount: p.total, status: p.status }));
  db.sales.forEach(s => events.push({ date: s.date, type: "sale", refId: s.id, who: customerIdToName(db, s.customerId), amount: s.total, status: s.status }));
  events.sort((a, b) => b.date.localeCompare(a.date));
  const top = events.slice(0, 8);
  if (!top.length) return el("div", { class: "empty", text: "尚無交易" });
  const tbody = el("tbody");
  top.forEach(ev => {
    const isP = ev.type === "purchase";
    tbody.appendChild(el("tr", {}, [
      el("td", { text: ev.date, class: "muted no-wrap" }),
      el("td", {}, [pill({ label: isP ? "進貨" : "銷售", pill: isP ? "info" : "primary" })]),
      el("td", { class: "mono no-wrap", text: ev.refId }),
      el("td", { text: ev.who }),
      el("td", { class: "num mono no-wrap", text: `NT$ ${fmtMoney(ev.amount)}` }),
      el("td", {}, [pill(Data.StatusMap[isP ? "purchaseStatus" : "saleStatus"][ev.status])]),
    ]));
  });
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["日期", "類型", "單號", "對象", "金額", "狀態"].map(t => el("th", { class: t === "金額" ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

/* ============================================================
   Products table + form
   ============================================================ */
function productsTable(db, products) {
  const tbody = el("tbody");
  products.forEach(p => {
    const inv = db.inventoryState[p.id] || { qty: 0, avgCost: 0, totalCost: 0 };
    const status = Data.StatusMap.productStatus[p.status] || { label: p.status, pill: "" };
    tbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: p.name }),
          el("span", { class: "muted mono", text: p.sku }),
        ]),
      ]),
      el("td", {}, [pill({ label: p.category, pill: "" })]),
      el("td", { class: "center", text: p.unit }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.listPrice)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(inv.avgCost)}` }),
      el("td", { class: "num mono", text: fmtNumber(inv.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(inv.totalCost)}` }),
      el("td", {}, [pill(status)]),
      el("td", { class: "actions" }, [
        el("button", { class: "btn sm", html: Icons.edit, onclick: () => openProductForm(db, p, () => router.refresh()) }),
      ]),
    ]));
  });
  if (!products.length) tbody.appendChild(tableEmpty(9));
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["商品", "分類", "單位", "標準售價", "平均成本", "現有庫存", "庫存帳面值", "狀態", "操作"].map(t =>
      el("th", { class: ["標準售價", "平均成本", "現有庫存", "庫存帳面值"].includes(t) ? "num" : t === "單位" ? "center" : "" }, [t])))]),
    tbody,
  ]);
}

function openProductForm(db, product, onSaved) {
  const p = product ? { ...product } : {
    id: "P" + pad(db.products.length + 1, 3),
    sku: "", name: "", category: "農產", unit: "包",
    listPrice: 0, stdCost: 0, status: "active",
  };
  const categories = Array.from(new Set(["農產", "茶飲", "咖啡", "食品", "包材", "其他", ...db.products.map(x => x.category)]));

  const form = el("div", {}, [
    el("div", { class: "form-row" }, [
      formGroup("商品編號", inputEl({ value: p.id, disabled: product ? "disabled" : null, oninput: e => p.id = e.target.value })),
      formGroup("SKU", inputEl({ value: p.sku, placeholder: "例如 OG-RICE-5KG", oninput: e => p.sku = e.target.value })),
    ]),
    el("div", { class: "form-row cols-1" }, [
      formGroup("商品名稱", inputEl({ value: p.name, oninput: e => p.name = e.target.value })),
    ]),
    el("div", { class: "form-row" }, [
      formGroup("分類", selectEl(categories.map(c => [c, c]), p.category, v => p.category = v)),
      formGroup("單位", inputEl({ value: p.unit, oninput: e => p.unit = e.target.value })),
    ]),
    el("div", { class: "form-row cols-3" }, [
      formGroup("標準售價 (NT$)", inputEl({ type: "number", value: p.listPrice, oninput: e => p.listPrice = Number(e.target.value) })),
      formGroup("標準成本 (NT$)", inputEl({ type: "number", value: p.stdCost, oninput: e => p.stdCost = Number(e.target.value) }), "僅供參考；實際成本由 WAC 計算"),
      formGroup("狀態", selectEl(
        Object.entries(Data.StatusMap.productStatus).map(([k, v]) => [k, v.label]),
        p.status, v => p.status = v,
      )),
    ]),
  ]);

  openModal({
    title: product ? "編輯商品" : "新增商品",
    body: form,
    actions: [
      { label: "取消", onClick: c => c() },
      {
        label: product ? "儲存變更" : "建立商品", variant: "primary",
        onClick: (close) => {
          if (!p.name || !p.sku) return toast("請填入 SKU 與商品名稱", "error");
          if (product) {
            Object.assign(product, p);
          } else {
            if (!db.inventoryState[p.id]) db.inventoryState[p.id] = { qty: 0, totalCost: 0, avgCost: 0 };
            db.products.push(p);
          }
          Data.save(db);
          toast(product ? "商品已更新" : "商品已建立");
          close();
          onSaved && onSaved();
        },
      },
    ],
  });
}

/* ============================================================
   Partner (supplier/customer) form
   ============================================================ */
function openPartnerForm(db, kind, partner, onSaved) {
  const isSup = kind === "supplier";
  const list = isSup ? db.suppliers : db.customers;
  const prefix = isSup ? "S" : "C";
  const p = partner ? { ...partner } : {
    id: prefix + pad(list.length + 1, 3),
    name: "", contact: "", phone: "", email: "", taxId: "",
    address: "", terms: "月結 30 天", type: "通路",
  };

  const form = el("div", {}, [
    el("div", { class: "form-row" }, [
      formGroup("編號", inputEl({ value: p.id, disabled: partner ? "disabled" : null, oninput: e => p.id = e.target.value })),
      formGroup("名稱", inputEl({ value: p.name, oninput: e => p.name = e.target.value })),
    ]),
    el("div", { class: "form-row" }, [
      formGroup("聯絡人", inputEl({ value: p.contact, oninput: e => p.contact = e.target.value })),
      formGroup("電話", inputEl({ value: p.phone, oninput: e => p.phone = e.target.value })),
    ]),
    el("div", { class: "form-row" }, [
      formGroup("Email", inputEl({ value: p.email, oninput: e => p.email = e.target.value })),
      formGroup("統一編號", inputEl({ value: p.taxId, oninput: e => p.taxId = e.target.value })),
    ]),
    el("div", { class: "form-row cols-1" }, [
      formGroup("地址", inputEl({ value: p.address, oninput: e => p.address = e.target.value })),
    ]),
    el("div", { class: "form-row" }, [
      formGroup("付款條件", selectEl(
        [["貨到付款", "貨到付款"], ["月結 30 天", "月結 30 天"], ["月結 45 天", "月結 45 天"], ["月結 60 天", "月結 60 天"], ["現金", "現金"]],
        p.terms, v => p.terms = v,
      )),
      isSup
        ? el("div")
        : formGroup("客戶類型", selectEl([["批發", "批發"], ["通路", "通路"], ["零售", "零售"]], p.type, v => p.type = v)),
    ]),
  ]);

  openModal({
    title: (partner ? "編輯" : "新增") + (isSup ? "供應商" : "客戶"),
    body: form,
    actions: [
      { label: "取消", onClick: c => c() },
      {
        label: partner ? "儲存" : "建立", variant: "primary",
        onClick: (close) => {
          if (!p.name) return toast("請填入名稱", "error");
          if (partner) Object.assign(partner, p);
          else list.push(p);
          Data.save(db);
          toast(partner ? "已更新" : "已建立");
          close();
          onSaved && onSaved();
        },
      },
    ],
  });
}

/* ============================================================
   Purchases table + form
   ============================================================ */
function purchasesTable(db, state) {
  const tbody = el("tbody");
  const list = [...db.purchases].sort((a, b) => b.date.localeCompare(a.date)).filter(p => {
    if (state.status && p.status !== state.status) return false;
    if (state.supplier && p.supplierId !== state.supplier) return false;
    return true;
  });
  list.forEach(p => {
    tbody.appendChild(el("tr", {}, [
      el("td", { class: "mono", text: p.id }),
      el("td", { text: p.date }),
      el("td", { text: supplierIdToName(db, p.supplierId) }),
      el("td", { class: "center mono", text: p.items.length }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.total)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.paid)}` }),
      el("td", {}, [pill(Data.StatusMap.purchaseStatus[p.status])]),
      el("td", { class: "actions" }, [
        el("button", { class: "btn sm", text: "檢視", onclick: () => openPurchaseDetail(db, p, () => router.refresh()) }),
      ]),
    ]));
  });
  if (!list.length) tbody.appendChild(tableEmpty(8));
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["單號", "日期", "供應商", "品項數", "總金額", "已付", "狀態", "操作"].map(t =>
      el("th", { class: ["品項數"].includes(t) ? "center" : ["總金額", "已付"].includes(t) ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

function openPurchaseForm(db, onCreated) {
  const draft = {
    date: todayISO(),
    supplierId: db.suppliers[0]?.id || "",
    warehouseId: db.warehouses[0]?.id || "",
    payMethod: "credit",
    note: "",
    items: [{ productId: db.products[0]?.id || "", qty: 1, unitCost: 0 }],
  };

  const linesBody = el("tbody");
  const totalEl = el("span", { class: "val total mono", text: "NT$ 0" });

  const renderLines = () => {
    linesBody.innerHTML = "";
    let total = 0;
    draft.items.forEach((it, idx) => {
      const sub = (Number(it.qty) || 0) * (Number(it.unitCost) || 0);
      total += sub;
      const productSelect = selectEl(
        db.products.filter(p => p.status !== "inactive" && p.status !== "purchase_off").map(p => [p.id, `${p.name} (${p.sku})`]),
        it.productId, v => { it.productId = v; renderLines(); }
      );
      const qtyInput = inputEl({ type: "number", value: it.qty, oninput: e => { it.qty = Number(e.target.value); renderLines(); } });
      const costInput = inputEl({ type: "number", step: "0.01", value: it.unitCost, oninput: e => { it.unitCost = Number(e.target.value); renderLines(); } });
      const removeBtn = el("button", {
        class: "row-remove", html: Icons.trash,
        onclick: () => { draft.items.splice(idx, 1); if (!draft.items.length) draft.items.push({ productId: "", qty: 1, unitCost: 0 }); renderLines(); },
      });
      linesBody.appendChild(el("tr", {}, [
        el("td", { style: "min-width:240px" }, [productSelect]),
        el("td", { class: "num", style: "width:110px" }, [qtyInput]),
        el("td", { class: "num", style: "width:140px" }, [costInput]),
        el("td", { class: "num mono", text: `NT$ ${fmtMoney(sub)}`, style: "width:140px;padding-right:14px;" }),
        el("td", { style: "width:40px" }, [removeBtn]),
      ]));
    });
    totalEl.textContent = `NT$ ${fmtMoney(total)}`;
  };

  const supplierSelect = selectEl(db.suppliers.map(s => [s.id, s.name]), draft.supplierId, v => draft.supplierId = v);
  const warehouseSelect = selectEl(db.warehouses.map(w => [w.id, w.name]), draft.warehouseId, v => draft.warehouseId = v);
  const dateInput = inputEl({ type: "date", value: draft.date, onchange: e => draft.date = e.target.value });
  const paySelect = selectEl([["credit", "月結/應付帳款"], ["cash", "現金/銀行支付"]], draft.payMethod, v => draft.payMethod = v);
  const noteInput = inputEl({ value: draft.note, placeholder: "備註（選填）", oninput: e => draft.note = e.target.value });

  const form = el("div", {}, [
    el("div", { class: "form-row cols-4" }, [
      formGroup("進貨日期", dateInput),
      formGroup("供應商", supplierSelect),
      formGroup("入庫倉", warehouseSelect),
      formGroup("付款方式", paySelect),
    ]),
    el("div", { class: "form-row cols-1" }, [formGroup("備註", noteInput)]),
    el("div", { class: "doc-section-title", text: "進貨明細" }),
    el("table", { class: "line-items" }, [
      el("thead", {}, [el("tr", {}, ["商品", "數量", "進貨單價", "小計", ""].map(t => el("th", { class: ["數量", "進貨單價", "小計"].includes(t) ? "num" : "" }, [t])))]),
      linesBody,
    ]),
    el("div", { class: "flex-row", style: "justify-content:space-between" }, [
      el("button", { class: "btn ghost sm", html: Icons.plus + " 新增一行", onclick: () => { draft.items.push({ productId: db.products[0]?.id || "", qty: 1, unitCost: 0 }); renderLines(); } }),
      el("div", { class: "line-summary" }, [
        el("div", { class: "pair" }, [el("span", { class: "lbl", text: "總金額" }), totalEl]),
      ]),
    ]),
  ]);

  openModal({
    title: "新增進貨單",
    size: "lg",
    body: form,
    actions: [
      { label: "取消", onClick: c => c() },
      {
        label: "送出並入庫", variant: "primary",
        onClick: (close) => {
          try {
            // sanitize items
            const items = draft.items.filter(i => i.productId && i.qty > 0);
            if (!items.length) throw new Error("至少需 1 項商品");
            const doc = Logic.createPurchase(db, { ...draft, items });
            toast(`已建立 ${doc.id}，庫存已更新`);
            close();
            onCreated && onCreated();
          } catch (e) { toast(e.message, "error"); }
        },
      },
    ],
  });
  renderLines();
}

function openPurchaseDetail(db, p, onChange) {
  const supplier = db.suppliers.find(s => s.id === p.supplierId);
  const status = Data.StatusMap.purchaseStatus[p.status];
  const remain = round(p.total - p.paid, 2);

  const itemsTbody = el("tbody");
  p.items.forEach(it => {
    const prod = db.products.find(x => x.id === it.productId);
    itemsTbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: prod ? prod.name : it.productId }),
          el("span", { class: "muted mono", text: prod ? prod.sku : "" }),
        ]),
      ]),
      el("td", { class: "num mono", text: fmtNumber(it.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.unitCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.subtotal)}` }),
    ]));
  });

  const doc = el("div", { class: "doc" }, [
    el("div", { class: "doc-head" }, [
      el("div", {}, [
        el("h2", { text: `進貨單 ${p.id}` }),
        pill(status),
      ]),
      el("div", { class: "meta-pairs" }, [
        el("span", { class: "lbl", text: "供應商" }),  el("span", { text: supplier?.name || "" }),
        el("span", { class: "lbl", text: "進貨日期" }), el("span", { text: p.date }),
        el("span", { class: "lbl", text: "入庫倉" }), el("span", { text: db.warehouses.find(w => w.id === p.warehouseId)?.name || "" }),
        el("span", { class: "lbl", text: "付款方式" }), el("span", { text: p.payMethod === "cash" ? "現金/銀行支付" : "月結/應付帳款" }),
        p.note ? el("span", { class: "lbl", text: "備註" }) : null,
        p.note ? el("span", { text: p.note }) : null,
      ].filter(Boolean)),
    ]),
    el("div", { class: "doc-section-title", text: "進貨明細" }),
    el("table", { class: "tbl" }, [
      el("thead", {}, [el("tr", {}, ["商品", "數量", "進貨單價", "小計"].map(t =>
        el("th", { class: ["數量", "進貨單價", "小計"].includes(t) ? "num" : "" }, [t])))]),
      itemsTbody,
    ]),
    el("div", { class: "line-summary" }, [
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "總金額" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(p.total)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "已付" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(p.paid)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "未付" }), el("span", { class: "val total mono", text: `NT$ ${fmtMoney(remain)}` })]),
    ]),
  ]);

  const modal = openModal({
    title: `進貨單 ${p.id}`,
    size: "lg",
    body: doc,
    actions: [
      p.status !== "void" && p.status !== "paid" && p.payMethod === "credit"
        ? { label: "登錄付款", variant: "primary", onClick: c => { c(); openPaymentForRef(db, "supplier", p, onChange); } }
        : null,
      p.status !== "void" && p.paid === 0
        ? { label: "作廢", variant: "danger", onClick: (close) => {
            if (!confirm("確定要作廢這張進貨單？庫存與應付會自動回沖")) return;
            try { Logic.voidDoc(db, "purchase", p.id); toast("已作廢"); close(); onChange && onChange(); }
            catch (e) { toast(e.message, "error"); }
          } }
        : null,
      { label: "關閉", onClick: c => c() },
    ].filter(Boolean),
  });
}

/* ============================================================
   Sales table + form
   ============================================================ */
function salesTable(db, state) {
  const tbody = el("tbody");
  const list = [...db.sales].sort((a, b) => b.date.localeCompare(a.date)).filter(s => {
    if (state.status && s.status !== state.status) return false;
    if (state.customer && s.customerId !== state.customer) return false;
    return true;
  });
  list.forEach(s => {
    const margin = s.total > 0 ? ((s.total - s.cogs) / s.total) * 100 : 0;
    tbody.appendChild(el("tr", {}, [
      el("td", { class: "mono", text: s.id }),
      el("td", { text: s.date }),
      el("td", { text: customerIdToName(db, s.customerId) }),
      el("td", { class: "center mono", text: s.items.length }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(s.total)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(s.cogs)}` }),
      el("td", { class: "num mono " + (margin >= 30 ? "text-success" : margin >= 15 ? "" : "text-warning"), text: `${margin.toFixed(1)}%` }),
      el("td", {}, [pill(Data.StatusMap.saleStatus[s.status])]),
      el("td", { class: "actions" }, [
        el("button", { class: "btn sm", text: "檢視", onclick: () => openSaleDetail(db, s, () => router.refresh()) }),
      ]),
    ]));
  });
  if (!list.length) tbody.appendChild(tableEmpty(9));
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["單號", "日期", "客戶", "品項數", "金額", "成本", "毛利率", "狀態", "操作"].map(t =>
      el("th", { class: t === "品項數" ? "center" : ["金額", "成本", "毛利率"].includes(t) ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

function openSaleForm(db, onCreated) {
  const draft = {
    date: todayISO(),
    customerId: db.customers[0]?.id || "",
    warehouseId: db.warehouses[0]?.id || "",
    payMethod: "credit",
    note: "",
    items: [{ productId: db.products[0]?.id || "", qty: 1, unitPrice: db.products[0]?.listPrice || 0 }],
  };

  const linesBody = el("tbody");
  const totalEl = el("span", { class: "val total mono", text: "NT$ 0" });
  const cogsEl = el("span", { class: "val mono", text: "NT$ 0" });
  const grossEl = el("span", { class: "val mono", text: "NT$ 0" });

  const renderLines = () => {
    linesBody.innerHTML = "";
    let total = 0, cogs = 0;
    draft.items.forEach((it, idx) => {
      const inv = db.inventoryState[it.productId] || { qty: 0, avgCost: 0 };
      const sub = (Number(it.qty) || 0) * (Number(it.unitPrice) || 0);
      const lineCogs = (Number(it.qty) || 0) * (Number(inv.avgCost) || 0);
      total += sub; cogs += lineCogs;

      const stockBadge = inv.qty < it.qty
        ? el("span", { class: "pill bad", style: "margin-left:6px;font-size:10px", text: `庫存 ${inv.qty}` })
        : el("span", { class: "muted", style: "margin-left:6px;font-size:11px", text: `庫存 ${inv.qty}` });

      const productCell = el("td", { style: "min-width:280px" }, [
        el("div", {}, [
          selectEl(
            db.products.filter(p => p.status !== "inactive" && p.status !== "sale_off").map(p => [p.id, `${p.name} (${p.sku})`]),
            it.productId, v => {
              it.productId = v;
              const p = db.products.find(x => x.id === v);
              if (p) it.unitPrice = p.listPrice;
              renderLines();
            }
          ),
          el("div", { style: "padding:2px 6px 0 6px;font-size:11px" }, [
            el("span", { class: "muted", text: `平均成本 NT$ ${fmtMoney(inv.avgCost)}` }),
            stockBadge,
          ]),
        ]),
      ]);
      const qtyInput = inputEl({ type: "number", value: it.qty, oninput: e => { it.qty = Number(e.target.value); renderLines(); } });
      const priceInput = inputEl({ type: "number", step: "0.01", value: it.unitPrice, oninput: e => { it.unitPrice = Number(e.target.value); renderLines(); } });
      const removeBtn = el("button", {
        class: "row-remove", html: Icons.trash,
        onclick: () => { draft.items.splice(idx, 1); if (!draft.items.length) draft.items.push({ productId: "", qty: 1, unitPrice: 0 }); renderLines(); },
      });
      linesBody.appendChild(el("tr", {}, [
        productCell,
        el("td", { class: "num", style: "width:100px" }, [qtyInput]),
        el("td", { class: "num", style: "width:130px" }, [priceInput]),
        el("td", { class: "num mono", style: "width:140px;padding-right:14px", text: `NT$ ${fmtMoney(sub)}` }),
        el("td", { style: "width:40px" }, [removeBtn]),
      ]));
    });
    totalEl.textContent = `NT$ ${fmtMoney(total)}`;
    cogsEl.textContent = `NT$ ${fmtMoney(cogs)}`;
    const gross = total - cogs;
    const mPct = total > 0 ? ((gross / total) * 100).toFixed(1) : 0;
    grossEl.textContent = `NT$ ${fmtMoney(gross)} (${mPct}%)`;
  };

  const customerSelect = selectEl(db.customers.map(c => [c.id, c.name]), draft.customerId, v => {
    draft.customerId = v;
    // auto-pick credit vs cash based on customer terms
    const c = db.customers.find(x => x.id === v);
    if (c && c.terms === "現金") draft.payMethod = "cash"; else draft.payMethod = "credit";
    renderLines();
  });
  const warehouseSelect = selectEl(db.warehouses.map(w => [w.id, w.name]), draft.warehouseId, v => draft.warehouseId = v);
  const dateInput = inputEl({ type: "date", value: draft.date, onchange: e => draft.date = e.target.value });
  const paySelect = selectEl([["credit", "月結/應收帳款"], ["cash", "現金/銀行收款"]], draft.payMethod, v => draft.payMethod = v);
  const noteInput = inputEl({ value: draft.note, placeholder: "備註（選填）", oninput: e => draft.note = e.target.value });

  const form = el("div", {}, [
    el("div", { class: "form-row cols-4" }, [
      formGroup("出貨日期", dateInput),
      formGroup("客戶", customerSelect),
      formGroup("出貨倉", warehouseSelect),
      formGroup("收款方式", paySelect),
    ]),
    el("div", { class: "form-row cols-1" }, [formGroup("備註", noteInput)]),
    el("div", { class: "doc-section-title", text: "銷售明細" }),
    el("table", { class: "line-items" }, [
      el("thead", {}, [el("tr", {}, ["商品", "數量", "售價", "小計", ""].map(t => el("th", { class: ["數量", "售價", "小計"].includes(t) ? "num" : "" }, [t])))]),
      linesBody,
    ]),
    el("div", { class: "flex-row", style: "justify-content:space-between" }, [
      el("button", { class: "btn ghost sm", html: Icons.plus + " 新增一行", onclick: () => { draft.items.push({ productId: db.products[0]?.id || "", qty: 1, unitPrice: db.products[0]?.listPrice || 0 }); renderLines(); } }),
      el("div", { class: "line-summary" }, [
        el("div", { class: "pair" }, [el("span", { class: "lbl", text: "成本 (WAC)" }), cogsEl]),
        el("div", { class: "pair" }, [el("span", { class: "lbl", text: "毛利" }), grossEl]),
        el("div", { class: "pair" }, [el("span", { class: "lbl", text: "總金額" }), totalEl]),
      ]),
    ]),
  ]);

  openModal({
    title: "新增銷售單",
    size: "lg",
    body: form,
    actions: [
      { label: "取消", onClick: c => c() },
      {
        label: "送出並出貨", variant: "primary",
        onClick: (close) => {
          try {
            const items = draft.items.filter(i => i.productId && i.qty > 0);
            if (!items.length) throw new Error("至少需 1 項商品");
            const doc = Logic.createSale(db, { ...draft, items });
            toast(`已建立 ${doc.id}，庫存與成本已更新`);
            close();
            onCreated && onCreated();
          } catch (e) { toast(e.message, "error"); }
        },
      },
    ],
  });
  renderLines();
}

function openSaleDetail(db, s, onChange) {
  const customer = db.customers.find(c => c.id === s.customerId);
  const remain = round(s.total - s.received, 2);

  const tbody = el("tbody");
  s.items.forEach(it => {
    const prod = db.products.find(x => x.id === it.productId);
    tbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: prod?.name || it.productId }),
          el("span", { class: "muted mono", text: prod?.sku || "" }),
        ]),
      ]),
      el("td", { class: "num mono", text: fmtNumber(it.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.unitPrice)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.unitCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.subtotal)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(it.cogs)}` }),
    ]));
  });

  const doc = el("div", { class: "doc" }, [
    el("div", { class: "doc-head" }, [
      el("div", {}, [
        el("h2", { text: `銷售單 ${s.id}` }),
        pill(Data.StatusMap.saleStatus[s.status]),
      ]),
      el("div", { class: "meta-pairs" }, [
        el("span", { class: "lbl", text: "客戶" }),    el("span", { text: customer?.name || "" }),
        el("span", { class: "lbl", text: "出貨日期" }), el("span", { text: s.date }),
        el("span", { class: "lbl", text: "出貨倉" }), el("span", { text: db.warehouses.find(w => w.id === s.warehouseId)?.name || "" }),
        el("span", { class: "lbl", text: "收款方式" }), el("span", { text: s.payMethod === "cash" ? "現金/銀行" : "月結/應收" }),
        s.note ? el("span", { class: "lbl", text: "備註" }) : null,
        s.note ? el("span", { text: s.note }) : null,
      ].filter(Boolean)),
    ]),
    el("div", { class: "doc-section-title", text: "明細 (含 WAC 成本)" }),
    el("table", { class: "tbl" }, [
      el("thead", {}, [el("tr", {}, ["商品", "數量", "售價", "WAC單成本", "小計", "成本"].map(t =>
        el("th", { class: ["數量", "售價", "WAC單成本", "小計", "成本"].includes(t) ? "num" : "" }, [t])))]),
      tbody,
    ]),
    el("div", { class: "line-summary" }, [
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "成本" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(s.cogs)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "毛利" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(s.total - s.cogs)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "金額" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(s.total)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "已收" }), el("span", { class: "val mono", text: `NT$ ${fmtMoney(s.received)}` })]),
      el("div", { class: "pair" }, [el("span", { class: "lbl", text: "未收" }), el("span", { class: "val total mono", text: `NT$ ${fmtMoney(remain)}` })]),
    ]),
  ]);

  openModal({
    title: `銷售單 ${s.id}`,
    size: "lg",
    body: doc,
    actions: [
      s.status !== "void" && s.status !== "paid" && s.payMethod === "credit"
        ? { label: "登錄收款", variant: "primary", onClick: c => { c(); openPaymentForRef(db, "customer", s, onChange); } }
        : null,
      s.status !== "void" && s.received === 0
        ? { label: "作廢", variant: "danger", onClick: (close) => {
            if (!confirm("確定要作廢這張銷售單？庫存、成本與應收會自動回沖")) return;
            try { Logic.voidDoc(db, "sale", s.id); toast("已作廢"); close(); onChange && onChange(); }
            catch (e) { toast(e.message, "error"); }
          } }
        : null,
      { label: "關閉", onClick: c => c() },
    ].filter(Boolean),
  });
}

/* ============================================================
   Payments / Receipts (AP / AR)
   ============================================================ */
function paymentLikeView(db, kind) {
  const isAP = kind === "payables";
  const title = isAP ? "應付帳款" : "應收帳款";
  const subtitle = isAP
    ? "應付供應商款項，登錄付款即沖銷餘額並產生會計分錄"
    : "應收客戶款項，登錄收款即沖銷餘額並產生會計分錄";
  const list = isAP ? db.purchases : db.sales;
  const partnerList = isAP ? db.suppliers : db.customers;
  const balanceMap = isAP ? db.apBalance : db.arBalance;

  const root = el("div");

  // Per-partner summary
  const summaryRows = partnerList.map(p => {
    const bal = balanceMap[p.id] || 0;
    return { partner: p, balance: bal };
  }).filter(r => r.balance > 0).sort((a, b) => b.balance - a.balance);

  const summaryCard = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: isAP ? "供應商應付餘額" : "客戶應收餘額" }),
        el("div", { class: "sub", text: `共 ${summaryRows.length} 個對象 / 合計 NT$ ${fmtMoney(summaryRows.reduce((s, r) => s + r.balance, 0))}` }),
      ]),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, [isAP ? "供應商" : "客戶", "聯絡人", "電話", "餘額"].map(t => el("th", { class: t === "餘額" ? "num" : "" }, [t])))]),
        el("tbody", {}, summaryRows.map(r =>
          el("tr", {}, [
            el("td", {}, [
              el("div", { class: "title-w-sub" }, [
                el("strong", { text: r.partner.name }),
                el("span", { class: "muted mono", text: r.partner.id }),
              ]),
            ]),
            el("td", { text: r.partner.contact }),
            el("td", { text: r.partner.phone }),
            el("td", { class: "num mono text-warning", text: `NT$ ${fmtMoney(r.balance)}` }),
          ])
        )),
      ]),
      summaryRows.length === 0 ? el("div", { class: "empty", text: "目前無未沖銷餘額" }) : null,
    ].filter(Boolean)),
  ]);

  // Outstanding documents
  const outstanding = list.filter(d => d.status !== "void" && d.status !== "paid" && d.payMethod === "credit");

  const outBody = el("tbody");
  outstanding.forEach(d => {
    const partner = partnerList.find(p => p.id === (isAP ? d.supplierId : d.customerId));
    const remain = round(d.total - (isAP ? d.paid : d.received), 2);
    outBody.appendChild(el("tr", {}, [
      el("td", { class: "mono", text: d.id }),
      el("td", { text: d.date }),
      el("td", { text: partner?.name || "" }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(d.total)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(isAP ? d.paid : d.received)}` }),
      el("td", { class: "num mono text-warning", text: `NT$ ${fmtMoney(remain)}` }),
      el("td", { class: "actions" }, [
        el("button", {
          class: "btn sm primary",
          text: isAP ? "登錄付款" : "登錄收款",
          onclick: () => openPaymentForRef(db, isAP ? "supplier" : "customer", d, () => router.refresh()),
        }),
      ]),
    ]));
  });
  if (!outstanding.length) outBody.appendChild(tableEmpty(7, "目前沒有未結清的單據"));

  const outCard = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: "未結清單據" }),
        el("div", { class: "sub", text: `共 ${outstanding.length} 筆` }),
      ]),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, [
          "單號", "日期", isAP ? "供應商" : "客戶", "金額", isAP ? "已付" : "已收", "未沖銷", "動作",
        ].map(t => el("th", { class: ["金額", "已付", "已收", "未沖銷"].includes(t) ? "num" : "" }, [t])))]),
        outBody,
      ]),
    ]),
  ]);

  // History
  const history = isAP ? db.payments : db.receipts;
  const histBody = el("tbody");
  [...history].sort((a, b) => b.date.localeCompare(a.date)).slice(0, 12).forEach(h => {
    const partner = partnerList.find(p => p.id === (isAP ? h.supplierId : h.customerId));
    histBody.appendChild(el("tr", {}, [
      el("td", { text: h.date }),
      el("td", { class: "mono", text: h.id }),
      el("td", { text: partner?.name || "" }),
      el("td", { class: "mono", text: (isAP ? h.purchaseId : h.saleId) || "—" }),
      el("td", {}, [pill({ label: h.method === "cash" ? "現金" : h.method === "bank" ? "銀行" : h.method, pill: "info" })]),
      el("td", { class: "num mono text-success", text: `NT$ ${fmtMoney(h.amount)}` }),
      el("td", { text: h.note || "" }),
    ]));
  });
  if (!history.length) histBody.appendChild(tableEmpty(7, "尚無歷史紀錄"));

  const histCard = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: isAP ? "近期付款紀錄" : "近期收款紀錄" }),
        el("div", { class: "sub", text: "最新 12 筆" }),
      ]),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["日期", "單號", isAP ? "供應商" : "客戶", "沖銷對象", "方式", "金額", "備註"].map(t => el("th", { class: t === "金額" ? "num" : "" }, [t])))]),
        histBody,
      ]),
    ]),
  ]);

  root.append(
    pageHead({ title, subtitle }),
    summaryCard,
    el("div", { style: "height:16px" }),
    outCard,
    el("div", { style: "height:16px" }),
    histCard,
  );
  return root;
}

function openPaymentForRef(db, kind, doc, onDone) {
  const isAP = kind === "supplier";
  const partner = isAP ? db.suppliers.find(s => s.id === doc.supplierId) : db.customers.find(c => c.id === doc.customerId);
  const remain = round(doc.total - (isAP ? doc.paid : doc.received), 2);

  const draft = {
    date: todayISO(),
    amount: remain,
    method: "bank",
    note: "",
  };

  const form = el("div", {}, [
    el("div", { class: "form-row cols-3" }, [
      formGroup(isAP ? "付款日期" : "收款日期", inputEl({ type: "date", value: draft.date, onchange: e => draft.date = e.target.value })),
      formGroup("方式", selectEl([["bank", "銀行轉帳"], ["cash", "現金"], ["check", "支票"]], draft.method, v => draft.method = v)),
      formGroup("金額 (NT$)", inputEl({ type: "number", value: draft.amount, oninput: e => draft.amount = Number(e.target.value) }), `本單剩餘 NT$ ${fmtMoney(remain)}`),
    ]),
    el("div", { class: "form-row cols-1" }, [
      formGroup("備註", inputEl({ value: draft.note, oninput: e => draft.note = e.target.value })),
    ]),
    el("div", { class: "card", style: "background:#fafbfd" }, [
      el("div", { class: "card-body" }, [
        el("div", { class: "form-row cols-2" }, [
          kvBlock(isAP ? "供應商" : "客戶", partner?.name || "—"),
          kvBlock("對應單號", doc.id),
        ]),
      ]),
    ]),
  ]);

  openModal({
    title: isAP ? "登錄付款" : "登錄收款",
    body: form,
    actions: [
      { label: "取消", onClick: c => c() },
      {
        label: "確認", variant: "primary",
        onClick: (close) => {
          try {
            if (isAP) {
              Logic.recordPayment(db, {
                date: draft.date, supplierId: partner.id, purchaseId: doc.id,
                amount: draft.amount, method: draft.method, note: draft.note,
              });
            } else {
              Logic.recordReceipt(db, {
                date: draft.date, customerId: partner.id, saleId: doc.id,
                amount: draft.amount, method: draft.method, note: draft.note,
              });
            }
            toast(isAP ? "付款已登錄" : "收款已登錄");
            close();
            onDone && onDone();
          } catch (e) { toast(e.message, "error"); }
        },
      },
    ],
  });
}

/* ============================================================
   Inventory views
   ============================================================ */
function inventoryStockView(db) {
  const cardBody = el("div", { class: "card-body flush" });
  const state = { keyword: "", category: "", lowOnly: false };
  const refresh = () => {
    const rows = db.products.filter(p => {
      const inv = db.inventoryState[p.id] || { qty: 0 };
      if (state.keyword && !(p.name.includes(state.keyword) || p.sku.toLowerCase().includes(state.keyword.toLowerCase()))) return false;
      if (state.category && p.category !== state.category) return false;
      if (state.lowOnly && inv.qty > 30) return false;
      return true;
    });
    cardBody.innerHTML = "";
    cardBody.appendChild(stockTable(db, rows));
  };
  const cats = Array.from(new Set(db.products.map(p => p.category)));
  const toolbar = el("div", { class: "toolbar" }, [
    field("關鍵字", inputEl({ placeholder: "搜尋商品 / SKU…", oninput: e => { state.keyword = e.target.value; refresh(); } })),
    field("分類", selectEl([["", "全部"], ...cats.map(c => [c, c])], "", v => { state.category = v; refresh(); })),
    el("div", { class: "field" }, [
      el("label", { text: "顯示" }),
      el("label", { class: "flex-row", style: "padding:6px 0" }, [
        (() => { const cb = el("input", { type: "checkbox", style: "margin-right:6px" }); cb.addEventListener("change", e => { state.lowOnly = e.target.checked; refresh(); }); return cb; })(),
        el("span", { text: "只看庫存 ≤ 30", class: "muted", style: "font-size:13px" }),
      ]),
    ]),
    el("div", { class: "spacer" }),
  ]);
  const card = el("div", { class: "card" }, [toolbar, cardBody]);
  refresh();
  return card;
}

function stockTable(db, products) {
  const tbody = el("tbody");
  products.forEach(p => {
    const inv = db.inventoryState[p.id] || { qty: 0, totalCost: 0, avgCost: 0 };
    const status = inv.qty <= 0 ? { label: "斷貨", pill: "bad" } : inv.qty <= 30 ? { label: "低庫存", pill: "warn" } : { label: "充足", pill: "ok" };
    tbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: p.name }),
          el("span", { class: "muted mono", text: `${p.sku} · ${p.id}` }),
        ]),
      ]),
      el("td", {}, [pill({ label: p.category, pill: "" })]),
      el("td", { class: "num mono", text: fmtNumber(inv.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(inv.avgCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(inv.totalCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.listPrice)}` }),
      el("td", {}, [pill(status)]),
    ]));
  });
  if (!products.length) tbody.appendChild(tableEmpty(7));
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["商品", "分類", "現有庫存", "平均成本", "庫存帳面值", "標準售價", "狀態"].map(t =>
      el("th", { class: ["現有庫存", "平均成本", "庫存帳面值", "標準售價"].includes(t) ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

function inventoryLedgerView(db) {
  const cardBody = el("div", { class: "card-body flush" });
  const state = { productId: "", type: "" };

  const refresh = () => {
    cardBody.innerHTML = "";
    const rows = [...db.stockLedger]
      .sort((a, b) => (b.date + b.id).localeCompare(a.date + a.id))
      .filter(r => {
        if (state.productId && r.productId !== state.productId) return false;
        if (state.type && r.type !== state.type) return false;
        return true;
      })
      .slice(0, 200);
    cardBody.appendChild(ledgerTable(db, rows));
  };

  const typeOpts = [
    ["", "全部類型"],
    ["purchase", "進貨"],
    ["sale", "銷售"],
    ["adjustment_in", "盤盈"],
    ["adjustment_out", "盤虧"],
    ["void", "作廢回沖"],
  ];
  const toolbar = el("div", { class: "toolbar" }, [
    field("商品", selectEl([["", "全部商品"], ...db.products.map(p => [p.id, p.name])], "", v => { state.productId = v; refresh(); })),
    field("類型", selectEl(typeOpts, "", v => { state.type = v; refresh(); })),
    el("div", { class: "spacer" }),
    el("span", { class: "muted", style: "font-size:12px", text: "顯示最新 200 筆" }),
  ]);

  const card = el("div", { class: "card" }, [toolbar, cardBody]);
  refresh();
  return card;
}

function ledgerTable(db, rows) {
  const tbody = el("tbody");
  rows.forEach(r => {
    const p = db.products.find(x => x.id === r.productId);
    const w = db.warehouses.find(x => x.id === r.warehouseId);
    const typePill = {
      purchase: { label: "進貨", pill: "info" },
      sale: { label: "銷售", pill: "primary" },
      adjustment_in: { label: "盤盈", pill: "ok" },
      adjustment_out: { label: "盤虧", pill: "bad" },
      void: { label: "作廢回沖", pill: "warn" },
    }[r.type] || { label: r.type };
    tbody.appendChild(el("tr", {}, [
      el("td", { text: r.date }),
      el("td", { class: "mono", text: r.refId }),
      el("td", {}, [pill(typePill)]),
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: p?.name || r.productId }),
          el("span", { class: "muted mono", text: p?.sku || "" }),
        ]),
      ]),
      el("td", { text: w?.name || r.warehouseId, class: "muted" }),
      el("td", { class: "num mono " + (r.qtyIn ? "text-success" : ""), text: r.qtyIn ? `+${fmtNumber(r.qtyIn)}` : "—" }),
      el("td", { class: "num mono " + (r.qtyOut ? "text-danger" : ""), text: r.qtyOut ? `-${fmtNumber(r.qtyOut)}` : "—" }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.unitCost)}` }),
      el("td", { class: "num mono", text: fmtNumber(r.balanceQty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.avgCost)}` }),
    ]));
  });
  if (!rows.length) tbody.appendChild(tableEmpty(10));
  return el("table", { class: "tbl" }, [
    el("thead", {}, [el("tr", {}, ["日期", "單號", "類型", "商品", "倉", "入庫", "出庫", "單價/成本", "結存", "平均成本"]
      .map(t => el("th", { class: ["入庫", "出庫", "單價/成本", "結存", "平均成本"].includes(t) ? "num" : "" }, [t])))]),
    tbody,
  ]);
}

function inventoryAdjustView(db, onChange) {
  const root = el("div");
  const draft = { productId: db.products[0]?.id || "", warehouseId: db.warehouses[0]?.id || "", qty: 0, unitCost: 0, note: "" };
  const hint = el("div", { class: "muted", style: "font-size:12px" });

  const updateHint = () => {
    const inv = db.inventoryState[draft.productId] || { qty: 0, avgCost: 0 };
    const isGain = draft.qty > 0;
    hint.textContent = `目前庫存 ${inv.qty}，平均成本 NT$ ${fmtMoney(inv.avgCost)}。${isGain ? "盤盈將以指定成本計入；若空白則沿用平均成本。" : draft.qty < 0 ? "盤虧將以目前平均成本記入損失。" : ""}`;
  };

  const form = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: "庫存調整" }),
        el("div", { class: "sub", text: "盤盈(正數) / 盤虧(負數)，自動產生會計分錄並更新 WAC" }),
      ]),
    ]),
    el("div", { class: "card-body" }, [
      el("div", { class: "form-row cols-3" }, [
        formGroup("商品", selectEl(db.products.map(p => [p.id, p.name]), draft.productId, v => { draft.productId = v; updateHint(); })),
        formGroup("倉庫", selectEl(db.warehouses.map(w => [w.id, w.name]), draft.warehouseId, v => draft.warehouseId = v)),
        formGroup("調整數量 (±)", inputEl({ type: "number", value: 0, oninput: e => { draft.qty = Number(e.target.value); updateHint(); } }), "盤盈為正、盤虧為負"),
      ]),
      el("div", { class: "form-row" }, [
        formGroup("盤盈單位成本 (NT$，選填)", inputEl({ type: "number", step: "0.01", value: 0, oninput: e => draft.unitCost = Number(e.target.value) })),
        formGroup("備註", inputEl({ value: "", oninput: e => draft.note = e.target.value })),
      ]),
      hint,
      el("div", { class: "flex-row", style: "justify-content:flex-end;margin-top:14px" }, [
        el("button", { class: "btn primary", text: "送出調整", onclick: () => {
          try {
            const r = Logic.createAdjustment(db, { date: todayISO(), ...draft });
            toast(`已記錄調整 ${r.id}`);
            onChange && onChange();
          } catch (e) { toast(e.message, "error"); }
        }}),
      ]),
    ]),
  ]);

  updateHint();
  root.append(form);

  // Adjustment history
  const adjRows = db.stockLedger.filter(r => r.refType === "adjustment").sort((a, b) => (b.date + b.id).localeCompare(a.date + a.id)).slice(0, 30);
  const histTbody = el("tbody");
  adjRows.forEach(r => {
    const p = db.products.find(x => x.id === r.productId);
    const isGain = r.qtyIn > 0;
    histTbody.appendChild(el("tr", {}, [
      el("td", { text: r.date }),
      el("td", { class: "mono", text: r.refId }),
      el("td", {}, [pill({ label: isGain ? "盤盈" : "盤虧", pill: isGain ? "ok" : "bad" })]),
      el("td", { text: p?.name }),
      el("td", { class: "num mono", text: isGain ? `+${fmtNumber(r.qtyIn)}` : `-${fmtNumber(r.qtyOut)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.unitCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(isGain ? r.qtyIn * r.unitCost : r.qtyOut * r.unitCost)}` }),
      el("td", { text: r.note, class: "muted" }),
    ]));
  });
  if (!adjRows.length) histTbody.appendChild(tableEmpty(8, "尚未有調整紀錄"));

  root.append(el("div", { style: "height:16px" }), el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [el("h3", { text: "調整紀錄" })]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["日期", "單號", "類型", "商品", "數量", "單價", "金額", "備註"].map(t => el("th", { class: ["數量", "單價", "金額"].includes(t) ? "num" : "" }, [t])))]),
        histTbody,
      ]),
    ]),
  ]));
  return root;
}

function computeWarehouseTotals(db, whId) {
  const since = isoDaysAgo(30);
  let in30 = 0, out30 = 0, movements = 0;
  db.stockLedger.forEach(r => {
    if (r.warehouseId !== whId) return;
    if (r.date < since) return;
    in30 += r.qtyIn || 0;
    out30 += r.qtyOut || 0;
    movements++;
  });
  return { in30, out30, movements };
}

/* ============================================================
   Reports
   ============================================================ */
function reportGrossMargin(db) {
  // by month for last 6 months
  const months = [];
  const now = new Date();
  for (let i = 5; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    months.push(d.toISOString().slice(0, 7));
  }
  const data = {};
  months.forEach(m => data[m] = { revenue: 0, cogs: 0, orders: 0 });
  db.sales.forEach(s => {
    if (s.status === "void") return;
    const m = s.date.slice(0, 7);
    if (data[m]) {
      data[m].revenue += s.total;
      data[m].cogs += s.cogs;
      data[m].orders++;
    }
  });

  const tbody = el("tbody");
  let totRev = 0, totCogs = 0, totOrders = 0;
  months.forEach(m => {
    const d = data[m];
    const gross = d.revenue - d.cogs;
    const margin = d.revenue ? (gross / d.revenue) * 100 : 0;
    totRev += d.revenue; totCogs += d.cogs; totOrders += d.orders;
    tbody.appendChild(el("tr", {}, [
      el("td", { class: "mono", text: m }),
      el("td", { class: "num mono", text: fmtNumber(d.orders) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(d.revenue)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(d.cogs)}` }),
      el("td", { class: "num mono " + (gross >= 0 ? "text-success" : "text-danger"), text: `NT$ ${fmtMoneySigned(gross)}` }),
      el("td", { class: "num mono", text: `${margin.toFixed(1)}%` }),
    ]));
  });
  const totalGross = totRev - totCogs;
  const totalMargin = totRev ? (totalGross / totRev) * 100 : 0;
  tbody.appendChild(el("tr", { style: "background:#fafbfd;font-weight:600" }, [
    el("td", { text: "合計" }),
    el("td", { class: "num mono", text: fmtNumber(totOrders) }),
    el("td", { class: "num mono", text: `NT$ ${fmtMoney(totRev)}` }),
    el("td", { class: "num mono", text: `NT$ ${fmtMoney(totCogs)}` }),
    el("td", { class: "num mono " + (totalGross >= 0 ? "text-success" : "text-danger"), text: `NT$ ${fmtMoneySigned(totalGross)}` }),
    el("td", { class: "num mono", text: `${totalMargin.toFixed(1)}%` }),
  ]));

  return el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: "毛利分析（近 6 個月）" }),
        el("div", { class: "sub", text: "成本以 WAC 在出貨當下凍結；不受後續進貨成本影響" }),
      ]),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["月份", "訂單", "銷售額", "銷貨成本", "毛利", "毛利率"].map(t => el("th", { class: ["訂單", "銷售額", "銷貨成本", "毛利", "毛利率"].includes(t) ? "num" : "" }, [t])))]),
        tbody,
      ]),
    ]),
  ]);
}

function reportProductRanking(db) {
  const top = computeTopProducts(db, 9999);
  const allByQty = computeTopProductsByQty(db);
  const left = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [el("h3", { text: "毛利排行 (Top 6)" })]),
    el("div", { class: "card-body" }, [renderTopProductsBar(top)]),
  ]);
  const tbody = el("tbody");
  allByQty.forEach((p, idx) => {
    const margin = p.revenue ? (p.gross / p.revenue) * 100 : 0;
    tbody.appendChild(el("tr", {}, [
      el("td", { class: "center mono", text: idx + 1 }),
      el("td", { text: p.name }),
      el("td", { class: "num mono", text: fmtNumber(p.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.revenue)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(p.cogs)}` }),
      el("td", { class: "num mono " + (p.gross >= 0 ? "text-success" : "text-danger"), text: `NT$ ${fmtMoneySigned(p.gross)}` }),
      el("td", { class: "num mono", text: `${margin.toFixed(1)}%` }),
    ]));
  });
  const right = el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [el("h3", { text: "所有商品銷售明細 (按數量)" })]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["#", "商品", "銷售量", "銷售額", "成本", "毛利", "毛利率"].map((t, i) => el("th", { class: i === 0 ? "center" : ["銷售量", "銷售額", "成本", "毛利", "毛利率"].includes(t) ? "num" : "" }, [t])))]),
        tbody,
      ]),
    ]),
  ]);
  return el("div", { class: "grid-2" }, [left, right]);
}

function computeTopProductsByQty(db) {
  const map = {};
  db.sales.forEach(s => {
    if (s.status === "void") return;
    s.items.forEach(it => {
      if (!map[it.productId]) map[it.productId] = { id: it.productId, qty: 0, revenue: 0, cogs: 0 };
      map[it.productId].qty += it.qty;
      map[it.productId].revenue += it.subtotal;
      map[it.productId].cogs += it.cogs;
    });
  });
  return Object.values(map)
    .map(x => ({ ...x, name: productIdToName(db, x.id), gross: x.revenue - x.cogs }))
    .sort((a, b) => b.qty - a.qty);
}

function reportInventoryTurnover(db) {
  // turnover = COGS / Avg Inventory (use current inventory value as proxy)
  // Per product, 30/60/90 days
  const since30 = isoDaysAgo(30);
  const since60 = isoDaysAgo(60);
  const since90 = isoDaysAgo(90);

  const rows = db.products.map(p => {
    const inv = db.inventoryState[p.id] || { qty: 0, totalCost: 0 };
    const cogs30 = sumCogsSince(db, p.id, since30);
    const cogs60 = sumCogsSince(db, p.id, since60);
    const cogs90 = sumCogsSince(db, p.id, since90);
    const turn = inv.totalCost > 0 ? cogs90 / inv.totalCost : 0;
    return { p, inv, cogs30, cogs60, cogs90, turn };
  });
  rows.sort((a, b) => b.turn - a.turn);

  const tbody = el("tbody");
  rows.forEach(r => {
    tbody.appendChild(el("tr", {}, [
      el("td", {}, [
        el("div", { class: "title-w-sub" }, [
          el("strong", { text: r.p.name }),
          el("span", { class: "muted mono", text: r.p.sku }),
        ]),
      ]),
      el("td", { class: "num mono", text: fmtNumber(r.inv.qty) }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.inv.totalCost)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.cogs30)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.cogs60)}` }),
      el("td", { class: "num mono", text: `NT$ ${fmtMoney(r.cogs90)}` }),
      el("td", { class: "num mono " + (r.turn >= 0.5 ? "text-success" : "text-warning"), text: r.turn.toFixed(2) }),
    ]));
  });

  return el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: "庫存週轉分析" }),
        el("div", { class: "sub", text: "週轉率 = 近 90 日 COGS / 目前庫存帳面值 (越高越好)" }),
      ]),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["商品", "現有庫存", "庫存帳面值", "近30日COGS", "近60日COGS", "近90日COGS", "週轉率"].map(t => el("th", { class: t === "商品" ? "" : "num" }, [t])))]),
        tbody,
      ]),
    ]),
  ]);
}

function sumCogsSince(db, productId, sinceDate) {
  let s = 0;
  db.sales.forEach(sale => {
    if (sale.status === "void" || sale.date < sinceDate) return;
    sale.items.forEach(it => { if (it.productId === productId) s += it.cogs; });
  });
  return round(s, 2);
}

function reportTrialBalance(db) {
  const tb = Logic.trialBalance(db);
  let totalDr = 0, totalCr = 0;
  const tbody = el("tbody");
  Object.values(tb).forEach(row => {
    totalDr += row.debit; totalCr += row.credit;
    tbody.appendChild(el("tr", {}, [
      el("td", { class: "mono", text: row.account.id }),
      el("td", { text: row.account.name }),
      el("td", {}, [pill({
        label: { asset: "資產", liability: "負債", equity: "權益", revenue: "收入", expense: "費用" }[row.account.type] || row.account.type,
        pill: { asset: "info", liability: "warn", revenue: "ok", expense: "bad" }[row.account.type] || "",
      })]),
      el("td", { class: "num mono", text: row.debit ? `NT$ ${fmtMoney(row.debit)}` : "—" }),
      el("td", { class: "num mono", text: row.credit ? `NT$ ${fmtMoney(row.credit)}` : "—" }),
      el("td", { class: "num mono " + (row.balance >= 0 ? "text-success" : "text-danger"), text: `NT$ ${fmtMoneySigned(row.balance)}` }),
    ]));
  });
  tbody.appendChild(el("tr", { style: "background:#fafbfd;font-weight:600" }, [
    el("td", { colspan: 3, text: "合計" }),
    el("td", { class: "num mono", text: `NT$ ${fmtMoney(totalDr)}` }),
    el("td", { class: "num mono", text: `NT$ ${fmtMoney(totalCr)}` }),
    el("td", { class: "num mono " + (Math.abs(totalDr - totalCr) < 0.01 ? "text-success" : "text-danger"), text: Math.abs(totalDr - totalCr) < 0.01 ? "借貸平衡 ✓" : `差額 ${fmtMoneySigned(totalDr - totalCr)}` }),
  ]));
  return el("div", { class: "card" }, [
    el("div", { class: "card-head" }, [
      el("div", {}, [
        el("h3", { text: "試算表 (Trial Balance)" }),
        el("div", { class: "sub", text: "依會計分錄彙總；合計借方 = 貸方表示帳務平衡" }),
      ]),
      el("button", { class: "btn sm", text: "查看分錄", onclick: () => router.go("journal") }),
    ]),
    el("div", { class: "card-body flush" }, [
      el("table", { class: "tbl" }, [
        el("thead", {}, [el("tr", {}, ["科目代碼", "科目", "類型", "借方", "貸方", "餘額"].map(t => el("th", { class: ["借方", "貸方", "餘額"].includes(t) ? "num" : "" }, [t])))]),
        tbody,
      ]),
    ]),
  ]);
}

window.Views = Views;
window.Icons = Icons;
window.el = el;
window.toast = toast;
window.openModal = openModal;
