# InvenFlow → 企業級 MIS/ERP — 前端 UI/UX 設計

> 版本：**v1.0**
> 對象系統：由 vanilla JS + localStorage 的 InvenFlow demo，重構為 **React 18 + TypeScript** 的企業 ERP 前端。
> 上游前提（必讀）：`00-MIS-ERP-轉型總計畫.md`（v2.0）、`01-計畫檢核報告.md`（特別是 §12.3「給前端設計的指引」）。
> 範圍紀律：本文件**只負責前端 UI/UX**。可引用 API/資料欄位，但**不設計後端 endpoint 或資料表結構**（那屬於 `02`/`03`）。尚未存在的模組（`mfg`/`project`/`assets`/`hr`/`payroll`）**只列入導覽 backlog，不畫高保真畫面**。

---

## 0. 設計總綱（一句話原則）

1. **權限驅動 UI 是體驗、不是安全**：前端依 IAM 權限集合決定「看得到 / 點得到 / 看得清楚」；**真正的授權一律在後端 enforce**。任何「前端把按鈕藏起來」的行為，後端都必須再擋一次。
2. **以三大流程組織資訊架構**，而非以模組（schema）羅列：O2C（訂單到收款）、P2P（採購到付款）、Plan-to-Produce（計畫到生產，backlog）。
3. **ERP 是重表格、重表單的系統**：所有列表走伺服器端分頁/排序/篩選；所有單據是「單頭 + 可編輯明細表」，且**財務數字（稅額、毛利、WAC 成本、借貸平衡）即時、顯眼**。
4. **財務正確性要「看得見」**：稅額即時計算列、試算表借貸平衡紅綠燈、關期狀態在記帳畫面明示、樂觀鎖衝突明確提示。
5. **契約優先、不手寫型別**：所有與後端互動的型別與 API client 由 **OpenAPI 產生**；前端只消費，不自行定義 DTO。

---

## 1. 技術架構與專案結構

### 1.1 技術定案（與 `02`/`03` 共用基準，務必一致）

| 面向 | 選型 | 角色 |
| --- | --- | --- |
| 框架 | **React 18 + TypeScript** | UI 與型別安全 |
| 建置 | **Vite** | dev server / build / 環境變數 |
| UI 元件庫 | **Ant Design 5（antd）** | 表格、表單、Layout、Modal、Drawer、ConfigProvider（主題/深色/i18n） |
| 表單 | **react-hook-form + zod**（`@hookform/resolvers/zod`） | 非受控表單、效能、schema 驗證 |
| 伺服器狀態 | **TanStack Query（React Query）** | 查詢快取、分頁、mutation、失效重抓、樂觀更新 |
| 前端 UI 狀態 | **Zustand** | **僅**少量 UI 狀態（主題、語系、側欄收合、目前公司） |
| 圖表 | **ECharts**（`echarts-for-react`） | 儀表板、財務趨勢 |
| API 契約 | **OpenAPI → 產生 TS 型別與 client** | 不手寫型別 |
| 路由 | **React Router v6** | 巢狀路由、lazy route、路由守衛 |
| 國際化 | **react-i18next** | zh-TW 先行、文字外部化、預留 en |

> **狀態分工原則（重要）**：伺服器來的資料一律歸 **TanStack Query**（它就是快取與真相來源），**不要**把 server data 複製進 Zustand。Zustand 只放「重整頁面就可重建、與後端無關」的 UI 偏好。表單編輯中的暫態歸 **react-hook-form**，不進全域狀態。

### 1.2 OpenAPI → TS client 流程

後端（`03`）以 ASP.NET Core 產出 OpenAPI（Swagger）文件。前端流程：

```
後端 swagger.json ──(CI/本機 npm run gen:api)──▶ openapi-typescript-codegen / orval
                                                  │
                                                  ▼
                          src/api-client/  (自動產生，禁止手改)
                            ├─ models/        ← 所有 DTO 型別（Purchase, SaleLine, TaxCode…）
                            ├─ services/      ← 依 tag 分組的 API 函式
                            └─ core/          ← request 包裝（注入 baseURL / token / RowVersion header）
```

- 工具建議：**orval**（可直接吐 TanStack Query hooks）或 **openapi-typescript-codegen**（吐純 client，再自行包 hook）。本案採 **orval**，產生 `useGetSalesQuery / usePostSaleMutation` 等 hook，減少樣板。
- **產生物不進人工維護**：`src/api-client/**` 由 `npm run gen:api` 覆寫；CI 檢查「產生物與 swagger 一致」避免漂移。
- **認證注入**：在產生 client 的 `core/request` 注入 Keycloak token（`Authorization: Bearer`）、`X-Company-Id`（目前公司）、以及**樂觀鎖** `If-Match: <RowVersion>`（見 §7.4）。
- **錯誤模型**：後端回 `ProblemDetails`（`03` 規範），前端統一在 query client 的 `onError` 解析 `type/title/detail/errors`，轉成 Ant Design `message`/`notification` 或表單欄位錯誤。

### 1.3 專案資料夾結構（feature-first）

以「**功能/模組（feature）為主軸**」分層，而非以技術類型分層；跨功能共用的放 `shared`。

```
web/
├─ src/
│  ├─ api-client/            # OpenAPI 產生物（禁手改）
│  ├─ app/
│  │  ├─ App.tsx             # ConfigProvider(主題/深色/i18n) + RouterProvider + QueryClientProvider
│  │  ├─ router.tsx          # 路由表（依三大流程組織）、lazy import、路由守衛
│  │  ├─ providers/          # QueryClientProvider、AuthProvider(Keycloak)、ThemeProvider
│  │  └─ layout/             # AppShell：Sider + Header + Content + Breadcrumb
│  ├─ features/              # ★ 業務功能（對應模組，但以「畫面群」組織）
│  │  ├─ auth/               # 登入、回呼、無權限頁
│  │  ├─ dashboard/
│  │  ├─ mdm/                # 商品、客戶、供應商、倉庫、稅別、編號規則(唯讀檢視)
│  │  ├─ procure/            # 請購、採購單、進貨驗收 (P2P)
│  │  ├─ sales/              # 報價、訂單、出貨、退貨折讓、CRM 基本 (O2C)
│  │  ├─ inventory/          # 多倉庫存、異動明細、盤點調整
│  │  ├─ finance/            # 總帳、AR、AP、發票、試算表、會計期間
│  │  └─ iam/                # 使用者、角色、權限、組織/公司
│  ├─ shared/                # ★ 跨功能可重用
│  │  ├─ components/         # DataTable、DocumentForm、EditableLineTable、PageHeader、
│  │  │                      #   ConfirmAction、StatusTag、MoneyText、PermissionGate…
│  │  ├─ hooks/              # usePermission、useServerTable、useDebounce、useCompany…
│  │  ├─ auth/               # <Can>、權限常數、權限 store 橋接
│  │  ├─ formats/            # 金額/數量/日期/稅額格式化（zh-TW）
│  │  ├─ i18n/               # i18n 初始化 + locales/zh-TW/*.json（en 預留空殼）
│  │  └─ theme/              # design tokens、antd theme config、深色 token
│  ├─ stores/                # Zustand：uiStore（主題/語系/側欄）、sessionStore（目前公司/權限快取）
│  └─ main.tsx
├─ openapi/                  # swagger.json 快照（gen 來源）
├─ orval.config.ts
├─ vite.config.ts
└─ package.json
```

> **模組（features）與導覽是兩件事**：`features/` 仍依後端模組分目錄（便於對應 API 與權限），但**使用者看到的導覽（§4）是依三大流程重組的**。兩者透過路由表對映。

---

## 2. 設計系統（Design System）

以 **Ant Design 5 的 Design Token 系統**（`ConfigProvider theme`）為單一真相來源，集中於 `shared/theme/tokens.ts`，**不散落寫死的色碼**。

### 2.1 色彩（Color Tokens）

| 語意 | Light | 用途 |
| --- | --- | --- |
| `colorPrimary` | `#2563EB`（藍） | 主行動、連結、選中態 |
| `colorSuccess` | `#16A34A` | 已過帳、借貸平衡、正毛利 |
| `colorWarning` | `#D97706` | 未結、部分付款、低庫存、關期警示 |
| `colorError` | `#DC2626` | 作廢、紅字折讓、不平衡、超賣、負毛利 |
| `colorInfo` | `#0891B2` | 草稿、處理中、一般狀態標籤 |
| 文字主/次/弱 | `#1F2937 / #4B5563 / #9CA3AF` | 三級文字 |
| 表面/分隔線 | `#FFFFFF / #F8FAFC / #E5E7EB` | 卡片、列表、邊框 |

**財務語意色固定**：借方/正向/平衡=success 綠；貸方/負向/不平衡=error 紅。整個系統一致，使用者一眼可辨。金額一律右對齊、等寬（tabular-nums）。

### 2.2 深色模式（Dark Mode）

- 以 **antd `theme.darkAlgorithm`** 為基底，於 `ConfigProvider` 切換；自訂 token 同時提供 light/dark 兩套 surface 與分隔線。
- 切換方式：Header 開關 + `prefers-color-scheme` 初值；偏好存 `uiStore`（localStorage 持久化）。
- 深色下**財務語意色微調**（提高在深底上的對比，綠改 `#22C55E`、紅改 `#F87171`），確保紅綠燈仍清晰。
- ECharts 提供 light/dark 兩套主題，隨全域切換。

### 2.3 字級、間距、圓角、密度

| Token | 值 | 說明 |
| --- | --- | --- |
| `fontFamily` | `-apple-system, "PingFang TC", "Noto Sans TC", "Inter", sans-serif` | 中文優先 |
| `fontSize` | 14（base）/ 12（表格密集）/ 16-20（標題） | ERP 資訊密度高，base 不放大 |
| 數字字體 | `tabular-nums`（等寬數字） | 金額/數量對齊 |
| `borderRadius` | 6（控制項）/ 8（卡片） | 偏方正、專業感 |
| 間距基準 | 8px 網格（4 / 8 / 16 / 24） | 一致留白 |
| **密度** | **compact** | ERP 偏緊湊：採用 antd **`theme.compactAlgorithm`** + 表格 `size="small"`，提高單屏資訊量 |

### 2.4 版面（Layout）

標準三區式（antd `Layout`），承襲既有 demo 的「左 Sider + 頂 Header + 內容區」心智模型：

```
┌──────────────────────────────────────────────────────────────┐
│  Header：[≡收合] Logo  [公司切換▾]  ……  [搜尋] [語系] [深淺] [通知🔔] [使用者▾] │
├──────────┬───────────────────────────────────────────────────┤
│ Sider    │  Breadcrumb：首頁 / O2C 銷售 / 銷售單                 │
│ (導覽)   │ ┌───────────────────────────────────────────────┐ │
│  O2C 銷售 │ │  PageHeader（標題 + 主要動作按鈕，受權限控管）   │ │
│  P2P 採購 │ ├───────────────────────────────────────────────┤ │
│  庫存     │ │                                                 │ │
│  財務     │ │            Content（列表 / 單據 / 報表）         │ │
│  主檔     │ │                                                 │ │
│  系統     │ └───────────────────────────────────────────────┘ │
└──────────┴───────────────────────────────────────────────────┘
```

- **Sider**：可收合（圖示模式），群組可折疊；**選單項目本身受權限過濾**（§3）。
- **Header**：固定置頂。內含**公司切換器**（多公司，切換後 TanStack Query 以 companyId 為 query key 一部分自動換資料）、全域搜尋、語系、深淺切換、通知、使用者選單。
- **Content**：以 `PageHeader`（標題、麵包屑、主要動作）+ 內容卡片。

### 2.5 響應式斷點與圖示

- 採 antd Grid 斷點：`xs<576 / sm≥576 / md≥768 / lg≥992 / xl≥1200 / xxl≥1600`。
- **桌面優先**（ERP 主要在桌面/筆電操作）。`lg` 以下 Sider 自動收合為抽屜；超寬表單明細表在窄屏改為**橫向捲動 + 凍結首欄**（不硬塞 RWD 卡片，以免破壞對帳閱讀）。平板可用（出貨/盤點場景）。
- 圖示統一用 **@ant-design/icons**（取代既有手刻 SVG），語意一致。

---

## 3. 權限驅動 UI 的具體機制

> **核心原則（必須寫死在團隊心智裡）**：**前端的權限控制只是「體驗最佳化」，不是安全邊界。** 隱藏選單、disable 按鈕、遮罩欄位，全部只為了「不要讓使用者看到一個點下去必然 403 的東西」。**真正的 RBAC/ABAC、列級/欄位級權限，一律由後端 enforce**（`03` 的授權模型 + 列級 query filter + 欄位級遮罩）。前端永遠假設「使用者可能繞過 UI 直接打 API」。

### 3.1 權限資料來源與形狀

- 登入後（Keycloak OIDC）向 IAM 取得**目前使用者的權限集合**（建議形狀：`permissions: string[]`，採 `resource:action` 命名，如 `sales.order:create`、`fin.journal:post`、`product.cost:read`）。
- 另含 **ABAC 屬性**供前端輔助呈現（非授權判斷）：`visibleCompanies`、`visibleWarehouses`、`amountLimit` 等——前端可用來「預先提示」（如金額超過上限時提示需簽核），但**核可與否仍由後端決定**。
- 權限集合快取於 `sessionStore`（隨登入/切換公司刷新），並由 TanStack Query 以 `['me','permissions',companyId]` 管理失效。

### 3.2 `usePermission()` hook

```ts
// shared/hooks/usePermission.ts
type Action = string; // e.g. 'sales.order:create'

export function usePermission() {
  const perms = useSessionStore(s => s.permissions);        // string[]
  const has = useCallback(
    (p: Action | Action[], mode: 'all' | 'any' = 'all') => {
      const list = Array.isArray(p) ? p : [p];
      return mode === 'all'
        ? list.every(x => perms.includes(x))
        : list.some(x => perms.includes(x));
    },
    [perms]
  );
  return { has, permissions: perms };
}
```

用法：`const { has } = usePermission(); const canPost = has('fin.journal:post');`

### 3.3 `<Can>` 宣告式元件

```tsx
// shared/auth/Can.tsx
<Can perm="sales.order:create">
  <Button type="primary" onClick={openCreate}>新增銷售單</Button>
</Can>

// 任一權限即可、無權限時顯示替代內容（例如 disable 而非隱藏）
<Can perm={['fin.journal:post', 'fin.journal:approve']} mode="any"
     fallback={<Button disabled>過帳（無權限）</Button>}>
  <Button type="primary">過帳</Button>
</Can>
```

- `Can` 內部呼叫 `usePermission`。預設**無權限即不渲染**；提供 `fallback`（常用於「顯示但 disable + tooltip 說明」）。

### 3.4 三種權限驅動行為（明確規範）

| 行為 | 機制 | 預設策略 |
| --- | --- | --- |
| **選單可見性** | 路由表每個 route 標 `requiredPerm`；Sider 渲染時用 `usePermission` 過濾；路由守衛 `<RequirePermission>` 攔截直接打網址 | **無權限的模組／頁面直接隱藏**（減少視覺雜訊）。直接輸入網址會被守衛導向「無權限頁」。 |
| **按鈕 enable/可見** | 動作按鈕包 `<Can>` | **破壞性/狀態變更動作（過帳、作廢、刪除）→ 顯示但 disable + tooltip 說明「需 XX 權限」**（讓使用者知道功能存在、向誰申請）；**新增類動作 → 無權限直接隱藏**。 |
| **欄位遮罩** | 敏感欄位（成本、毛利、客戶聯絡資料、未來薪資）以 `has('product.cost:read')` 等決定顯示真值或 `••••`／「無檢視權限」 | **前端遮罩僅為體驗**；**後端必須對無權限者根本不回傳該欄位的值**（欄位級遮罩在 API 層 enforce）。前端遮罩是「即使後端誤傳也不顯示」的第二層保險，反之不成立。 |

> **反模式警告**：絕不可「後端照常回傳成本，僅靠前端 CSS 隱藏」。欄位級權限的真相在後端；前端遮罩只是當後端已正確不回傳時，避免畫面出現空洞或 `null`。

### 3.5 與路由整合

```tsx
// router.tsx（節錄）
{
  path: 'sales/orders',
  lazy: () => import('@/features/sales/OrderListPage'),
  handle: { requiredPerm: 'sales.order:read', breadcrumb: '銷售單' },
}
// <RequirePermission> 讀 handle.requiredPerm，無權限 → <Navigate to="/403">
```

---

## 4. 資訊架構與導覽（依三大流程）

導覽以**端到端業務流程**分群，符合使用者的工作心智，而非後端 schema。對映關係：O2C↔`sales`、P2P↔`procure`、庫存↔`inv`、財務↔`fin`、主檔↔`mdm`、系統↔`iam`。

### 4.1 主導覽（Sider）

```
總覽
  └ 儀表板                         (dashboard)

O2C 銷售與收款
  ├ 報價單                         (sales.quote)        〔Phase 2 後段〕
  ├ 銷售訂單 / 出貨                 (sales.order)
  ├ 退貨 / 銷貨折讓                 (sales.return)
  ├ 客戶 / CRM 基本                 (sales.crm)          客戶、聯絡、商機(基本)
  └ 應收帳款 (AR)                   (fin.ar)

P2P 採購與付款
  ├ 請購單                         (procure.requisition) 〔含硬編碼兩階簽核〕
  ├ 採購單                         (procure.po)
  ├ 進貨驗收                       (procure.receipt)
  └ 應付帳款 (AP)                  (fin.ap)

庫存
  ├ 多倉庫存總覽                   (inv.balance)        分倉餘額 + WAC
  ├ 庫存異動明細                   (inv.ledger)
  └ 盤點 / 調整                    (inv.adjust)

財務會計
  ├ 總帳 / 會計分錄                 (fin.journal)
  ├ 試算表                         (fin.trialbalance)   借貸平衡紅綠燈
  ├ 統一發票                       (fin.invoice)        進/銷項、字軌、折讓
  └ 會計期間                       (fin.period)         開立/關閉/鎖定狀態

主檔資料 (MDM)
  ├ 商品 / 料件                    (mdm.product)
  ├ 客戶                           (mdm.customer)
  ├ 供應商                         (mdm.supplier)
  ├ 倉庫                           (mdm.warehouse)
  └ 稅別 / 幣別 / 編號規則         (mdm.config)         （唯讀檢視為主）

系統管理 (IAM)
  ├ 使用者                         (iam.user)
  ├ 角色與權限                     (iam.role)
  └ 組織 / 公司                    (iam.company)

─────────────  導覽 Backlog（停用/灰階，需求驗證後啟用）  ─────────────
  製造 (MRP/BOM) · 專案 · 固定資產 · 人資 · 薪資
  （僅佔位，點擊顯示「規劃中」；不畫高保真畫面）
```

### 4.2 首批交付（Phase 對應，見 §8）

> 依 `01` §12.3 與總計畫 Roadmap，**先交付**：登入、儀表板、主檔（mdm / 商品 / 客戶 / 供應商）、採購、銷售、庫存、財務（總帳 / AR / AP / 試算表 / 發票）。

### 4.3 麵包屑、全域搜尋、空間導覽

- **麵包屑**：由路由 `handle.breadcrumb` 自動生成（流程群組 / 頁面 / 單號）。
- **全域搜尋**（Header）：跨單號 / 商品 / 客戶 / 供應商；輸入單號可直接跳到該單據 Drawer（承襲既有 demo 行為，但改為呼叫後端搜尋 API）。
- **登入頁**：Keycloak 重導為主（OIDC）；本系統提供 landing→「進入系統」觸發登入，回呼後落在儀表板。

---

## 5. ERP 重表格表單的關鍵畫面規範

本節是 ERP 前端的核心。所有畫面以兩個可重用元件為骨幹：**`DataTable`（列表）** 與 **`DocumentForm`（主從單據）**。

### 5.1 列表頁規範（`DataTable` / `useServerTable`）

ERP 資料量大，**一律伺服器端**分頁/排序/篩選，禁止前端全量載入後再篩。

**共用能力**
- **伺服器端分頁**：antd Table + TanStack Query，query key 含 `{page,pageSize,sort,filters,companyId}`；切頁/改篩選即重抓。
- **排序**：欄位 `sorter:true`，把 `sortField/sortOrder` 帶給 API。
- **多條件篩選**：頂部 **FilterBar**（日期區間、狀態、客戶/供應商、倉庫、關鍵字）；篩選條件可同步到 URL query string（可分享、可回上頁保留）。
- **欄位自訂**：右上「欄位設定」可勾選顯示欄位、拖曳排序、調整密度；偏好存使用者層（localStorage + 未來後端使用者偏好）。
- **批次操作**：列可勾選（`rowSelection`），批次動作（批次過帳、批次匯出、批次列印）**受權限與單據狀態雙重控管**（已作廢/已關期者不可選）。
- **匯出**：「匯出 Excel/CSV」按鈕**受 `*.export` 權限控管**（財報外洩風險高，預設僅特定角色可匯出）；大量資料走後端產檔再下載，前端只觸發與輪詢。
- **列操作**：每列尾欄「檢視/編輯/更多」，動作依狀態與權限顯示。
- **空 / 載入 / 錯誤狀態**：見 §7.3。

**線框（列表頁）**

```
┌ PageHeader ─────────────────────────────────────────────────────────┐
│  銷售單                                   [匯出▾(受權限)] [＋ 新增銷售單] │
├ FilterBar ──────────────────────────────────────────────────────────┤
│ [日期 2026/06/01–06/18] [狀態:全部▾] [客戶▾] [倉庫▾] [🔍關鍵字] [重置] [⚙欄位] │
├ Table (size=small, compact) ───────────────────────────────────────┤
│ ☑ 單號     日期     客戶      倉庫   未稅   稅額   含稅金額  狀態   操作 │
│ ☐ SO-0042 06/18  晶華貿易  台北倉 10,000  500  10,500  〔未結〕 ⋯  │
│ ☐ SO-0041 06/17  大同實業  桃園倉  8,000  400   8,400  〔已結清〕⋯ │
│   …(本頁 20 筆)                                                      │
├─────────────────────────────────────────────────────────────────────┤
│  已選 0 筆  [批次過帳] [批次列印]        共 312 筆  ‹ 1 2 3 … 16 ›  20/頁▾ │
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 主從單據規範（`DocumentForm` + `EditableLineTable`）

採購單 / 銷售單 / 發票皆為「**單頭（Header）＋ 多明細（Lines）可編輯表格**」。承襲既有 demo 的 `openSaleForm`（已有 WAC 成本、毛利、即時合計），升級為 React + zod + 即時稅額。

**結構**
- **單頭**：日期、對象（客戶/供應商）、倉庫、幣別、付/收款條件、發票別、備註 → react-hook-form 控制，zod 驗證。
- **明細可編輯表格**：每列＝商品（含 SKU 搜尋下拉）、數量、單價、稅別、折扣、**小計（即時）**；銷售列額外顯示**該品 WAC 成本與毛利**（受 `cost:read` 權限）。
- **合計列即時計算**：未稅小計、**稅額（依稅別即時算）**、含稅總額、（銷售）總成本 WAC、總毛利與毛利率。所有計算**前端即時呈現以利操作，但送出後以後端回算為準**（避免前端浮點與後端不一致；後端為金額真相）。
- **存檔策略**：草稿（Draft）可存可改；**過帳（Post）= 真正寫庫存與分錄**，需二次確認（§5.3）。
- **大表單編輯**：Drawer（側拉）或全頁；明細多時表格區可獨立捲動、合計列 sticky 置底。

**線框（銷售單 — 主從單據）**

```
┌ Drawer：銷售單 SO-0042  狀態〔草稿〕            [關閉]  [存草稿] [送出並過帳] ┐
│ ┌ 單頭 ───────────────────────────────────────────────────────────┐ │
│ │ 出貨日期[2026/06/18] 客戶[晶華貿易▾] 出貨倉[台北倉▾] 幣別[TWD▾]    │ │
│ │ 發票別[三聯式▾] 收款方式[月結/AR▾]  備註[__________________]      │ │
│ │ ⚠ 會計期間 2026-06 為【開立中】，可過帳                            │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ ┌ 明細（可編輯）──────────────────────────────────────────────────┐ │
│ │ 商品(SKU搜尋)       數量  單價    稅別   小計    WAC成本   毛利   ✕ │ │
│ │ 藍牙耳機 BT-100      10  1,000  應稅5%  10,000   620/件  3,800  ✕ │ │
│ │   └ 庫存:85  平均成本 NT$620                                       │ │
│ │ [＋ 新增一行]                                                      │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ ┌ 合計（sticky 即時）────────────────────────────────────────────┐ │
│ │ 未稅小計  10,000   稅額(5%) 500   含稅總額 10,500                 │ │
│ │ 總成本(WAC) 6,200  毛利 3,800 (38.0%)   〔受成本檢視權限〕         │ │
│ └─────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

> 採購單同構：單頭含供應商、進貨倉、**進項稅別**；明細為品項/數量/進價/稅別；合計顯示未稅/進項稅/含稅。**進貨過帳即刷新該倉 WAC**（顯示「過帳後將更新平均成本」提示）。

### 5.3 過帳 / 作廢 / 沖銷 / 退貨折讓（狀態變更動作）

這些是**不可逆或高風險**動作，統一以 `ConfirmAction` 元件處理，三道關卡：**權限檢查 → 期間鎖定檢查 → 二次確認**。

| 動作 | 二次確認內容 | 權限 | 期間檢查 |
| --- | --- | --- | --- |
| **過帳 Post** | 摘要影響（將寫入庫存異動 + N 筆分錄、刷新 WAC），確認 | `*:post` | 目標期間須【開立中】，否則 disable + 提示 |
| **作廢 Void** | 紅字警示「此動作將反向沖銷分錄、回補/回沖庫存」，需輸入原因 | `*:void` | 不可作廢已關期單據；跨期作廢→提示「將以當期沖銷分錄處理」 |
| **沖銷 Reverse** | 顯示原分錄與將產生的沖銷分錄對照 | `fin.journal:reverse` | 同上 |
| **退貨 / 折讓** | 選擇退貨品項與數量、折讓金額；顯示「將開立紅字發票、AR/AP 沖減、庫存回補（WAC 政策）」 | `sales.return:create` | 須在開立期間 |

- **ConfirmAction** 以 antd `Modal.confirm` 或自訂 Modal；破壞性動作按鈕用 `danger`，且**需明確再次點擊**（不用單純 Popconfirm 帶過高風險動作）。
- 動作執行中按鈕 `loading`、防重複提交（mutation pending 期間 disable）。
- 失敗（如後端回 409 期間已關 / 樂觀鎖衝突）→ 明確錯誤提示與後續指引（§7）。

**線框（作廢二次確認）**

```
┌ 確認作廢 SO-0042 ───────────────────────────────┐
│  ⚠ 此操作不可復原                                 │
│  將執行：                                         │
│   • 反向沖銷 4 筆會計分錄（產生紅字分錄）          │
│   • 回補庫存：藍牙耳機 +10（台北倉）               │
│   • AR 沖減 NT$ 10,500                            │
│  作廢原因（必填）：[_____________________________] │
│                              [取消]  [確認作廢(紅)] │
└──────────────────────────────────────────────────┘
```

### 5.4 發票 / 退貨折讓畫面

- **統一發票（fin.invoice）**：列表（進項/銷項分頁、字軌、發票號、買受人、稅額、狀態）＋ 開立/檢視。單張發票畫面顯示**字軌與號碼**、銷售對象、品項、**銷項/進項稅額**、**作廢/折讓**入口。三聯/二聯式切換影響欄位（統編必填與否）。
- **退貨 / 折讓（sales.return）**：由原銷售單帶出可退品項，選數量/金額；產生**紅字發票（折讓單）**，並顯示對 AR/AP 與庫存的影響（WAC 回補政策依 `02` 決策呈現提示）。

---

## 6. 財務正確性的 UI 呈現

財務正確性必須「看得見」。以下為硬性規範。

### 6.1 單據即時財務數字

- **銷售/採購單**即時顯示：**稅額**（依各列稅別計算的合計列）、**含稅總額**、（銷售）**WAC 成本與毛利/毛利率**。
- **金額呈現規範**：右對齊、`tabular-nums`、千分位、幣別前綴（`NT$`）；負數紅字並加括號或負號一致呈現；毛利為負時紅字標示。
- **前端即時 vs 後端真相**：前端即時算只為操作回饋；**送出後以後端回傳金額為準並覆蓋顯示**，避免浮點/捨入不一致（後端為金額真相，見 `02` 稅額/WAC）。

### 6.2 試算表「是否平衡」紅綠燈

承襲既有 `reportTrialBalance`（已有合計與差額顯示），升級為明確紅綠燈：

```
┌ 試算表 (Trial Balance)  期間[2026-06▾]               [匯出(受權限)] ┐
│ 科目代碼  科目        類型   借方        貸方        餘額            │
│ 1130     應收帳款    資產   120,500     —          120,500         │
│ 1310     存貨        資產    88,000     —           88,000         │
│ 2130     銷項稅額    負債    —          15,250      -15,250        │
│ 4100     銷貨收入    收入    —         305,000     -305,000        │
│ …                                                                  │
├────────────────────────────────────────────────────────────────────┤
│ 合計                       420,250     420,250                      │
│             ● 借貸平衡 ✓（綠燈）   　借方 = 貸方                     │
└────────────────────────────────────────────────────────────────────┘
```

- **平衡** → 綠燈 `● 借貸平衡 ✓`；**不平衡** → 紅燈 `● 不平衡 差額 NT$ X`，並提供「檢視差異分錄」連結。
- 紅綠燈用 `colorSuccess`/`colorError`，深淺模式皆高對比；不可僅靠顏色，**附文字**（無障礙）。

### 6.3 會計期間關期狀態（記帳畫面明示）

> 對應 `03` 的會計期間鎖定（Open / Closed / Locked）。前端在**所有記帳/過帳/作廢畫面**明示狀態。

- **狀態徽章**：單頭顯示目標日期所屬期間狀態：
  - 〔開立中 Open〕綠 — 可輸入/過帳。
  - 〔已關閉 Closed〕橘 — **輸入欄位 disable**，過帳/作廢按鈕 disable，顯示提示「2026-05 已關帳，無法過帳到此期間」。
  - 〔已鎖定 Locked〕灰 — 完全唯讀。
- **行為**：選擇的單據日期落在已關/鎖期間時，即時切換為唯讀並提示；嘗試送出時若後端回 409（期間已關），以明確錯誤訊息呈現並阻止。
- **會計期間頁（fin.period）**：列出各期間狀態與開/關/鎖時間（操作受 `fin.period:manage` 權限）。

### 6.4 其他財務呈現

- **AR/AP 帳齡**：應收/應付列表提供帳齡分桶（當期 / 30 / 60 / 90+）視覺化（顏色漸警示）。
- **WAC 成本**：庫存總覽顯示分倉數量與**加權平均成本**；無 `cost:read` 權限者該欄遮罩。
- **儀表板財務 KPI**：庫存價值、AR、AP、本月毛利等以 ECharts + KPI 卡呈現（承襲既有 dashboard KPI 結構）。

---

## 7. 跨切面（Cross-Cutting）

### 7.1 國際化（i18n）

- **react-i18next**，**zh-TW 先行**；所有 UI 文字**外部化**到 `locales/zh-TW/*.json`（依 feature 分檔），**禁止硬編碼字串**。
- **預留 en**：保留 `locales/en/` 空殼與 key 結構；翻譯延後（依 `01` Nice-to-have）。
- 與 antd 整合：`ConfigProvider locale={zhTW}`（日期/表格/分頁等內建文案），隨語系切換。
- 數字/日期/金額格式集中於 `shared/formats`（`Intl.NumberFormat('zh-TW')`），與 i18n 一致。

### 7.2 深色模式
見 §2.2（design system 已涵蓋）。切換偏好持久化於 `uiStore`。

### 7.3 錯誤 / 載入 / 空狀態（統一規範）

| 狀態 | 規範 |
| --- | --- |
| **載入中** | 列表/卡片用 antd `Skeleton`；表格用 `loading`；切頁時保留舊資料 + 局部 spinner（TanStack Query `keepPreviousData`），避免畫面跳動。 |
| **空狀態** | antd `Empty` + 引導動作（如「尚無銷售單，立即新增」按鈕，受權限）。承襲既有 `tableEmpty` 的「圖示 + 標題 + 說明」結構。 |
| **錯誤** | 解析 `ProblemDetails`：表單欄位錯誤 → 對應欄位下方紅字；全域錯誤 → `notification`；403 → 導「無權限頁」；404 → 「找不到資料」頁；網路/500 → 可重試的錯誤卡（重抓按鈕）。 |
| **行級錯誤邊界** | 以 React Error Boundary 包住內容區，單一頁面崩潰不拖垮整個 App（承襲既有 demo try/catch 顯示「頁面發生錯誤」的精神，升級為 ErrorBoundary）。 |

### 7.4 樂觀鎖衝突提示（對應後端 `RowVersion`）

> 對應 `02` 每張表的 `RowVersion`（樂觀並行）。

- 讀取單據時保存 `rowVersion`；送出更新時以 **`If-Match: <rowVersion>`** header 帶回（在 api-client core 注入）。
- 後端偵測衝突回 **409 Conflict**（`ProblemDetails`）。前端攔截後彈出明確提示：

```
┌ 資料已被他人更新 ───────────────────────────────┐
│  此單據在您編輯期間已被其他使用者修改。            │
│  您的變更尚未儲存。                               │
│   [重新載入最新版本]   [檢視差異]   [放棄我的變更] │
└──────────────────────────────────────────────────┘
```

- 預設引導「重新載入最新版本」（重抓並提示使用者重做）；不靜默覆蓋（避免 lost update）。

### 7.5 表單驗證錯誤呈現

- **zod schema** 為單一驗證真相（與 react-hook-form `zodResolver` 整合）；前端做即時/送出時驗證。
- 錯誤呈現：欄位下方紅字 + 欄位紅框（antd `Form.Item validateStatus`）；明細表格列內錯誤就地標示（該格紅框 + tooltip）；送出時若有錯，**捲動並聚焦第一個錯誤欄位**。
- **後端驗證仍為準**：zod 與後端 FluentValidation（`03`）規則盡量對齊，但後端回的 `errors` 仍要能映射回欄位（防止前端規則過時）。

### 7.6 無障礙（A11y）基本要求

- 顏色不單獨承載語意：紅綠燈/狀態**同時附文字或圖示**（色盲友善）。
- 鍵盤可達：表單 Tab 順序合理、明細表可鍵盤新增/移動列；Modal 焦點鎖定與 ESC 關閉（antd 內建）。
- 對比度達 WCAG AA（深淺模式皆驗證，尤其財務紅綠）。
- 圖示按鈕提供 `aria-label`/tooltip；表格欄位有表頭關聯。

---

## 8. 關鍵畫面清單與優先序

> 對映總計畫 Roadmap：**Phase 1**（身分治理）、**Phase 2**（核心進銷存）、**Phase 3**（財務會計+稅務）。Backlog 模組**僅佔位，不畫高保真**。優先級：P0 必做、P1 重要、P2 可後補。

### Phase 1 — 地基與身分治理（前端基礎 + IAM）

| 畫面 | 優先 | 重點 |
| --- | --- | --- |
| App Shell（Sider+Header+Content、主題/深色/i18n、ConfigProvider） | **P0** | 一切的容器 |
| OpenAPI client + TanStack Query + 錯誤模型 + ProblemDetails 處理 | **P0** | 資料層地基 |
| 登入 / OIDC 回呼 / 無權限(403) 頁 | **P0** | Keycloak 串接 |
| `usePermission` / `<Can>` / 路由守衛（權限驅動 UI 骨架） | **P0** | 三大行為機制 |
| 使用者 / 角色與權限 / 組織·公司（IAM 管理） | P1 | 列表+表單，公司切換器 |
| 共用元件：`DataTable`/`useServerTable`、`PageHeader`、`StatusTag`、`MoneyText` | **P0** | 後續所有頁面複用 |

### Phase 2 — 核心進銷存（O2C / P2P / 庫存 + 主檔）

| 畫面 | 優先 | 重點 |
| --- | --- | --- |
| 儀表板（KPI + ECharts 趨勢 + 庫存/帳款卡） | P1 | 承襲既有 dashboard |
| 主檔：商品/料件（列表 + 表單） | **P0** | SKU、分類、單位、定價、狀態 |
| 主檔：客戶 / 供應商（列表 + 表單，含 CRM 基本） | **P0** | 付款條件、聯絡、商機(基本) |
| 主檔：倉庫 | P1 | 多倉前置 |
| `DocumentForm` + `EditableLineTable`（主從單據共用元件） | **P0** | 採購/銷售/發票共用 |
| 採購單（P2P：單頭+明細、進項稅、過帳刷 WAC） | **P0** | 主從單據 |
| 進貨驗收 | P1 | 入庫、WAC 更新 |
| 請購單（含硬編碼兩階簽核 UI） | P2 | 簽核狀態呈現 |
| 銷售訂單/出貨（O2C：明細+WAC 成本+毛利+稅額即時） | **P0** | 旗艦單據畫面 |
| 退貨 / 銷貨折讓（紅字、AR 沖減、庫存回補） | P1 | §5.4 |
| 多倉庫存總覽（分倉餘額 + WAC） | **P0** | 修正「庫存只在商品層」 |
| 庫存異動明細 / 盤點調整 | P1 | 承襲既有 ledger/adjust |
| 報價單 | P2 | O2C 前段 |

### Phase 3 — 財務會計與稅務合規

| 畫面 | 優先 | 重點 |
| --- | --- | --- |
| 總帳 / 會計分錄（列表 + 分錄檢視 + 沖銷） | **P0** | append-only，沖銷對照 |
| 試算表（借貸平衡紅綠燈） | **P0** | §6.2 |
| 應收帳款 AR（列表、收款、帳齡） | **P0** | O2C 收尾 |
| 應付帳款 AP（列表、付款、帳齡） | **P0** | P2P 收尾 |
| 統一發票（進/銷項、字軌、開立/作廢/折讓） | **P0** | 台灣合規底線 |
| 會計期間（開/關/鎖狀態管理） | P1 | §6.3，過帳關期檢查 |
| 過帳/作廢/沖銷的 `ConfirmAction`（三道關卡） | **P0** | §5.3，貫穿財務動作 |
| 財務報表（毛利、商品排行、週轉率，ECharts） | P1 | 承襲既有 reports |

### Backlog（導覽佔位，不畫高保真）
製造（MRP/BOM）、專案、固定資產、人資、薪資 — 僅 Sider 灰階佔位 + 「規劃中」提示，待需求驗證後再設計（避免規劃先於需求）。

---

## 9. 與既有 demo 的對應與替換

| 既有（vanilla JS） | 新（React + AntD） | 處置 |
| --- | --- | --- |
| `assets/js/views.js`（手刻 DOM `el()`、`Views.*`） | `features/**` React 元件 | **整批重寫**取代 |
| `assets/js/app.js`（hash router、`renderSidebar/Topbar/Main`） | `app/router.tsx` + `app/layout/AppShell` | 取代 |
| `openModal` / `toast` / `pill` / `tableEmpty` | antd `Modal`/`Drawer`/`message`/`Tag`/`Empty` + `shared/components` | 取代 |
| `Icons.*`（手刻 SVG） | `@ant-design/icons` | 取代 |
| `openSaleForm`（已有 WAC/毛利/即時合計） | `DocumentForm` + `EditableLineTable` + 即時稅額 | **升級**（保留心智模型，加稅額/權限/期間） |
| `reportTrialBalance`（合計+差額） | 試算表頁 + 紅綠燈 | 升級 |
| `Data`（localStorage） | TanStack Query + OpenAPI client（呼叫後端） | 取代為真後端 |
| `server/InvenFlow.Api/wwwroot`（舊全端 console JS） | 由 Vite build 產物部署（或 ASP.NET 靜態托管 SPA） | 取代 |

---

## 10. 一致性檢查清單（交付前自檢）

- [ ] 所有與後端互動型別來自 `api-client`（OpenAPI 產生），無手寫 DTO。
- [ ] 伺服器狀態在 TanStack Query；Zustand 僅 UI 偏好；表單暫態在 react-hook-form。
- [ ] 每個破壞性動作都過「權限 → 期間 → 二次確認」三關。
- [ ] 敏感欄位前端遮罩存在，**且已確認後端對無權限者不回傳該欄位值**。
- [ ] 列表頁皆伺服器端分頁/排序/篩選，匯出受權限。
- [ ] 單據合計（稅額/含稅/WAC/毛利）即時呈現，且送出後以後端值為準。
- [ ] 試算表紅綠燈、關期狀態徽章、樂觀鎖 409 提示三者皆實作。
- [ ] 文字外部化（zh-TW），深淺模式紅綠對比達 AA。
- [ ] 未存在模組僅 Sider 佔位，無高保真畫面。
```
