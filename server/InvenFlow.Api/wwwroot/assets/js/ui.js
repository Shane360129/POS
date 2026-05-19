/* Tiny DOM helpers + modal/toast utilities */

function $(sel, scope = document) { return scope.querySelector(sel); }
function $$(sel, scope = document) { return [...scope.querySelectorAll(sel)]; }

function el(tag, attrs = {}, children = []) {
  const node = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (k === "class") node.className = v;
    else if (k === "text") node.textContent = v;
    else if (k === "html") node.innerHTML = v;
    else if (k.startsWith("on") && typeof v === "function") node.addEventListener(k.slice(2), v);
    else if (v === false || v == null) { /* skip */ }
    else node.setAttribute(k, v);
  }
  for (const c of [].concat(children)) {
    if (c == null) continue;
    node.appendChild(typeof c === "string" ? document.createTextNode(c) : c);
  }
  return node;
}

function toast(msg, kind = "") {
  const root = $("#toast-root");
  const t = el("div", { class: "toast " + kind, text: msg });
  root.appendChild(t);
  setTimeout(() => t.remove(), 2600);
}

/**
 * openModal(title, builder, opts)
 * builder(body, close) may return an async save function. If it does,
 * a primary "儲存" button is rendered that invokes it and closes on success.
 */
function openModal(title, builder, { saveLabel = "儲存", showCancel = true } = {}) {
  const root = $("#modal-root");
  const back = el("div", { class: "modal-back" });
  const modal = el("div", { class: "modal" });
  const body = el("div", {});
  const close = () => back.remove();

  modal.appendChild(el("h3", { text: title }));
  modal.appendChild(body);
  const actions = el("div", { class: "modal-actions" });
  if (showCancel) actions.appendChild(el("button", { class: "btn btn-ghost", text: "取消", onclick: close }));
  modal.appendChild(actions);
  back.appendChild(modal);
  back.addEventListener("click", (e) => { if (e.target === back) close(); });
  root.appendChild(back);

  const saveFn = builder(body, close);
  if (typeof saveFn === "function") {
    actions.appendChild(el("button", {
      class: "btn btn-primary", text: saveLabel,
      onclick: async () => {
        try { await saveFn(); close(); } catch (e) { toast(e.message, "err"); }
      }
    }));
  }
}

function confirmModal(msg, onConfirm) {
  openModal("確認操作", (body) => {
    body.appendChild(el("p", { text: msg, style: "margin: 8px 0 0; color: #5a627d;" }));
    return async () => { await onConfirm(); };
  }, { saveLabel: "確定" });
}

function field(label, input) {
  return el("div", { class: "field" }, [el("label", { text: label }), input]);
}

function input(attrs = {}) { return el("input", attrs); }
function select(options, attrs = {}) {
  const s = el("select", attrs);
  for (const o of options) {
    s.appendChild(el("option", { value: o.value, text: o.label, selected: o.selected }));
  }
  return s;
}

function pill(label, kind = "info") {
  return el("span", { class: `pill pill-${kind}`, text: label });
}

window.$ = $; window.$$ = $$; window.el = el;
window.toast = toast; window.openModal = openModal; window.confirmModal = confirmModal;
window.field = field; window.input = input; window.select = select; window.pill = pill;
