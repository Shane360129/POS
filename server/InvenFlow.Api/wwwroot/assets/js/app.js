/* ============================================================
   InvenFlow Admin Console — App Shell + Router
   Each route fetches its own data from the API on render.
   ============================================================ */

const routes = [
  { id: "dashboard",   label: "儀表板",   group: "OVERVIEW" },
  { id: "products",    label: "商品主檔", group: "MASTER" },
  { id: "suppliers",   label: "供應商",   group: "MASTER" },
  { id: "customers",   label: "客戶",     group: "MASTER" },
  { id: "warehouses",  label: "倉庫",     group: "MASTER" },
  { id: "purchases",   label: "進貨作業", group: "OPS" },
  { id: "sales",       label: "銷售作業", group: "OPS" },
  { id: "inventory",   label: "庫存管理", group: "OPS" },
  { id: "payments",    label: "付款",     group: "FINANCE" },
  { id: "receipts",    label: "收款",     group: "FINANCE" },
  { id: "journal",     label: "會計分錄", group: "FINANCE" },
  { id: "reports",     label: "報表分析", group: "REPORTS" },
  { id: "settings",    label: "系統設定", group: "SYSTEM" },
];
const groupLabels = {
  OVERVIEW: "總覽", MASTER: "主檔", OPS: "進銷作業",
  FINANCE: "財務", REPORTS: "報表", SYSTEM: "系統",
};

const router = {
  current: "dashboard",
  go(id) { location.hash = "#/" + id; },
  refresh() { this.render(); },
  init() {
    window.addEventListener("hashchange", () => this.render());
    this.render();
  },
  async render() {
    const hash = location.hash.replace(/^#\//, "") || "dashboard";
    const route = routes.find(r => r.id === hash) || routes[0];
    this.current = route.id;
    renderSidebar();
    renderTopbar(route);
    await renderMain(route);
  },
};

function renderSidebar() {
  const sb = $("#sidebar");
  sb.innerHTML = "";
  sb.appendChild(el("div", { class: "brand" }, [
    el("div", { class: "brand-mark", text: "iF" }),
    el("div", {}, [
      el("div", { class: "brand-name", text: "InvenFlow" }),
      el("div", { class: "brand-tag", text: "Full-stack Console" }),
    ]),
  ]));

  const groups = {};
  routes.forEach(r => (groups[r.group] ||= []).push(r));
  Object.keys(groups).forEach(g => {
    sb.appendChild(el("div", { class: "nav-group-title", text: groupLabels[g] || g }));
    groups[g].forEach(r => {
      sb.appendChild(el("a", {
        class: "nav-link" + (router.current === r.id ? " active" : ""),
        href: "#/" + r.id, text: r.label,
      }));
    });
  });
}

function renderTopbar(route) {
  const tb = $("#topbar");
  tb.innerHTML = "";
  tb.appendChild(el("div", { class: "crumbs" }, [
    el("span", { text: "InvenFlow" }),
    el("span", { text: " / " }),
    el("span", { text: groupLabels[route.group] || route.group }),
    el("span", { text: " / " }),
    el("span", { class: "current", text: route.label }),
  ]));
  const right = el("div", { class: "topbar-right" });
  const pill = el("span", { class: "api-pill", text: "API · 檢查中…" });
  right.appendChild(pill);
  tb.appendChild(right);
  API.health()
    .then(() => { pill.textContent = "API · 連線中"; pill.classList.remove("err"); })
    .catch(() => { pill.textContent = "API · 失敗"; pill.classList.add("err"); });
}

async function renderMain(route) {
  const main = $("#main");
  main.innerHTML = "";
  main.appendChild(el("div", { class: "empty", text: "載入中…" }));
  const view = Views[route.id];
  if (!view) {
    main.innerHTML = "";
    main.appendChild(el("div", { class: "empty", text: "頁面建構中" }));
    return;
  }
  try {
    const node = await view();
    main.innerHTML = "";
    main.appendChild(node);
  } catch (e) {
    console.error(e);
    main.innerHTML = "";
    main.appendChild(el("div", { class: "empty" }, [
      el("h4", { text: "頁面載入失敗" }),
      el("p", { text: e.message }),
    ]));
  }
}

document.addEventListener("DOMContentLoaded", () => router.init());
window.router = router;
