/* ============================================================
   InvenFlow — App Shell & Router
   ============================================================ */

const routes = [
  { id: "dashboard",     label: "儀表板",   group: "OVERVIEW",   icon: Icons.dashboard },
  { id: "products",      label: "商品主檔",  group: "MASTER",     icon: Icons.package },
  { id: "suppliers",     label: "供應商",   group: "MASTER",     icon: Icons.truck },
  { id: "customers",     label: "客戶",     group: "MASTER",     icon: Icons.users },
  { id: "warehouses",    label: "倉庫",     group: "MASTER",     icon: Icons.warehouse },
  { id: "purchases",     label: "進貨作業", group: "OPERATIONS", icon: Icons.shop },
  { id: "sales",         label: "銷售作業", group: "OPERATIONS", icon: Icons.cart },
  { id: "inventory",     label: "庫存管理", group: "OPERATIONS", icon: Icons.warehouse },
  { id: "payables",      label: "應付帳款", group: "FINANCE",    icon: Icons.receipt },
  { id: "receivables",   label: "應收帳款", group: "FINANCE",    icon: Icons.receipt },
  { id: "journal",       label: "會計分錄", group: "FINANCE",    icon: Icons.ledger },
  { id: "reports",       label: "報表分析", group: "REPORTS",    icon: Icons.chart },
  { id: "settings",      label: "系統設定", group: "SYSTEM",     icon: Icons.settings },
];

const groupLabels = {
  OVERVIEW:   "總覽",
  MASTER:     "主檔管理",
  OPERATIONS: "進銷作業",
  FINANCE:    "財務帳款",
  REPORTS:    "報表",
  SYSTEM:     "系統",
};

const router = {
  current: "dashboard",
  go(id) {
    location.hash = "#/" + id;
  },
  refresh() {
    this.render();
  },
  init() {
    window.addEventListener("hashchange", () => this.render());
    this.render();
  },
  render() {
    const hash = location.hash.replace(/^#\//, "") || "dashboard";
    const route = routes.find(r => r.id === hash) || routes[0];
    this.current = route.id;
    renderSidebar();
    renderTopbar(route);
    renderMain(route);
  },
};

function renderSidebar() {
  const db = Data.load();
  const sb = $("#sidebar");
  if (!sb) return;
  sb.innerHTML = "";

  sb.appendChild(el("div", { class: "brand" }, [
    el("div", { class: "brand-mark", text: "iF" }),
    el("div", {}, [
      el("div", { class: "brand-name", text: "InvenFlow" }),
      el("div", { class: "brand-tagline", text: "智慧進銷存平台" }),
    ]),
  ]));

  // group routes
  const groups = {};
  routes.forEach(r => {
    if (!groups[r.group]) groups[r.group] = [];
    groups[r.group].push(r);
  });
  Object.keys(groups).forEach(g => {
    const wrap = el("div", { class: "nav-group" }, [
      el("div", { class: "nav-group-title", text: groupLabels[g] || g }),
    ]);
    groups[g].forEach(r => {
      const link = el("a", {
        class: "nav-link" + (router.current === r.id ? " active" : ""),
        href: "#/" + r.id,
        html: `${r.icon}<span>${r.label}</span>`,
      });
      wrap.appendChild(link);
    });
    sb.appendChild(wrap);
  });

  sb.appendChild(el("div", { class: "sidebar-foot" }, [
    el("div", { class: "avatar", text: db.user.initials }),
    el("div", { class: "meta" }, [
      el("div", { class: "name", text: db.user.name }),
      el("div", { class: "role", text: db.user.role }),
    ]),
  ]));
}

function renderTopbar(route) {
  const tb = $("#topbar");
  if (!tb) return;
  tb.innerHTML = "";
  const groupLbl = groupLabels[route.group] || route.group;
  tb.appendChild(el("div", { class: "crumbs" }, [
    el("span", { text: "InvenFlow" }),
    el("span", { class: "sep", text: "/" }),
    el("span", { text: groupLbl }),
    el("span", { class: "sep", text: "/" }),
    el("span", { class: "current", text: route.label }),
  ]));
  tb.appendChild(el("div", { class: "topbar-right" }, [
    el("div", { class: "search" }, [
      el("span", { html: Icons.search }),
      (() => {
        const i = el("input", { placeholder: "搜尋單號 / 商品 / 客戶…", oninput: globalSearch });
        return i;
      })(),
    ]),
    el("button", { class: "icon-btn", html: Icons.bell, title: "通知", onclick: () => toast("您有 0 則新通知", "success") }, [
      el("span", { class: "dot-badge" }),
    ]),
  ]));
}

function globalSearch(e) {
  const q = (e.target.value || "").trim().toLowerCase();
  if (!q || q.length < 2) return;
  const db = Data.load();
  // jump to matching record if obvious
  const po = db.purchases.find(p => p.id.toLowerCase() === q);
  if (po) { router.go("purchases"); setTimeout(() => openPurchaseDetail(db, po, () => router.refresh()), 100); e.target.value = ""; return; }
  const so = db.sales.find(s => s.id.toLowerCase() === q);
  if (so) { router.go("sales"); setTimeout(() => openSaleDetail(db, so, () => router.refresh()), 100); e.target.value = ""; return; }
  const prod = db.products.find(p => p.sku.toLowerCase() === q || p.name.toLowerCase() === q);
  if (prod) { router.go("products"); e.target.value = ""; return; }
}

function renderMain(route) {
  const main = $("#main");
  if (!main) return;
  main.innerHTML = "";
  const db = Data.load();
  const view = Views[route.id];
  if (!view) {
    main.appendChild(el("div", { class: "empty", text: "頁面建構中" }));
    return;
  }
  try {
    main.appendChild(view(db));
  } catch (e) {
    console.error(e);
    main.appendChild(el("div", { class: "empty" }, [
      el("h4", { text: "頁面發生錯誤" }),
      el("p", { text: e.message }),
    ]));
  }
}

// boot
document.addEventListener("DOMContentLoaded", () => {
  Data.load();      // initialize seed if needed
  router.init();
});

window.router = router;
