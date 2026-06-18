# InvenFlow → 企業級 MIS/ERP 轉型總計畫

> 版本：v1.0（草案，待檢核）
> 目標：將現有 **InvenFlow 進銷存＋會計系統** 升級為 **全功能企業 ERP（MIS）**
> 技術方向：**微服務後端** ＋ **React + TypeScript 前端重構** ＋ **資料庫 per-service**

---

## 1. 願景與目標

把目前單體的「進銷存＋會計」demo，演進為一套可支撐企業營運的 **模組化 ERP/MIS 平台**，涵蓋：採購、銷售、庫存、財務會計、CRM、製造（MRP/BOM）、專案、固定資產、人資與薪資差勤，並以 **權限治理（RBAC/ABAC）、簽核流程、稽核軌跡、BI 報表** 作為跨模組的治理骨幹。

### 成功指標（North Star）
| 面向 | 指標 |
| --- | --- |
| 功能 | 19 個有界上下文（bounded context）全數上線，覆蓋訂單到收款（O2C）、採購到付款（P2P）、計畫到生產（P2P/MRP）三大端到端流程 |
| 正確性 | 跨服務交易最終一致；試算表（trial balance）借貸恆平衡；庫存帳實相符 |
| 安全 | 全 API 經閘道驗證；最小權限；完整稽核軌跡 |
| 維運 | 任一服務可獨立部署；P95 API < 300ms；可觀測性（log/trace/metric）齊備 |
| 體驗 | 響應式、深色模式、i18n（zh-TW 為主）、權限驅動 UI |

---

## 2. 現況評估（As-Is）

### 2.1 既有資產
- **靜態 demo**（根目錄）：vanilla JS + `localStorage`，含 7 大模組 UI。
- **全端版**（`server/InvenFlow.Api`）：ASP.NET Core 8 + EF Core + SQL Server，單體架構。
  - Models：`Product / Supplier / Customer / Warehouse / Account / Purchase / Sale / Payment / Receipt / StockMovement / JournalEntry / JournalLine`
  - 核心商業邏輯：
    - **加權平均成本 WAC**：進貨刷新均價、銷售以當下均價鎖定 COGS。
    - **自動借貸分錄**：每筆交易產生平衡分錄，支援作廢反向沖銷，試算表驗證。
  - Services：`AccountingService`（WAC＋分錄）、`ReportService`（儀表板/毛利/排行/週轉/試算表）。
- **legacy**：原 M06 ASP.NET WebForms 進銷存（封存參考）。

### 2.2 與「企業 ERP」的差距（Gap）
| 缺口 | 嚴重度 | 說明 |
| --- | --- | --- |
| **無身分/權限系統** | 🔴 critical | 目前任何人皆可呼叫任何 API，無登入、無角色、無租戶隔離。 |
| **無稽核軌跡** | 🔴 critical | 無 who/when/what 紀錄，財務系統不合規。 |
| **無簽核流程** | 🟠 high | 採購/請款/調整缺乏審批控制。 |
| **單體耦合** | 🟠 high | WAC 與分錄綁在同一 DB 交易，拆服務後需重新設計一致性。 |
| **缺核心 ERP 模組** | 🟠 high | 無製造、專案、固定資產、HR、薪資、完整 CRM、總帳期間結轉。 |
| **前端不可維護** | 🟡 medium | vanilla JS 手刻 DOM，難以支撐大型表單與權限控管。 |
| **無多公司/多幣別** | 🟡 medium | 企業常見需求未支援。 |
| **無可觀測性/部署自動化** | 🟡 medium | 無 log 聚合、trace、CI/CD pipeline、容器編排。 |

---

## 3. 目標架構（To-Be）

### 3.1 全景圖
```
                         ┌──────────────────────────┐
                         │   React + TS 前端 (SPA)   │
                         │  Web / 平板 / (未來) 行動  │
                         └────────────┬─────────────┘
                                      │ HTTPS / OIDC JWT
                         ┌────────────▼─────────────┐
                         │     API Gateway (YARP)    │ 認證、路由、限流、彙整
                         └────────────┬─────────────┘
        ┌──────────────┬─────────────┼─────────────┬──────────────┐
        ▼              ▼             ▼             ▼              ▼
  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
  │  IAM     │  │ 採購 P2P │  │ 銷售 O2C │  │ 庫存     │  │ 財務會計 │ ...
  │ 身分權限 │  │          │  │          │  │          │  │   GL     │
  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘
       │             │             │             │             │
       ▼             ▼             ▼             ▼             ▼
   (DB per service：每服務獨立 schema/資料庫)
       │             │             │             │             │
       └─────────────┴──── Event Bus（RabbitMQ / Kafka）──────┴──────┐
                            事件驅動、Outbox、Saga 編排                │
                         ┌──────────────────────────────────────────┘
                         ▼
              平台服務：Notification / Audit / Workflow簽核 /
                        File儲存 / BI報表(讀取模型/資料倉儲)
```

### 3.2 有界上下文 / 微服務清單（共 19）

> 此清單為後續 **資料庫設計、後端設計、UI/UX 設計三份文件的共用基準**，命名與邊界以此為準。

**A. 平台 / 跨領域服務（Platform）**
| # | 服務 | 代號 | 職責 |
| --- | --- | --- | --- |
| 1 | Identity & Access | `iam` | 使用者、角色、權限(RBAC/ABAC)、組織單位、登入/SSO(OIDC)、多公司租戶 |
| 2 | API Gateway | `gateway` | 路由、認證強制、限流、聚合 BFF |
| 3 | Workflow / Approval | `workflow` | 可設定簽核流程、待辦、委派 |
| 4 | Notification | `notify` | Email/SMS/站內信、範本、訂閱 |
| 5 | Audit & Activity | `audit` | 全域稽核軌跡、操作日誌（事件消費） |
| 6 | File / Document | `docs-svc` | 附件、檔案儲存（物件儲存）、版本 |
| 7 | Reporting / BI | `bi` | 跨服務讀取模型、儀表板、資料倉儲、匯出 |

**B. 商業領域服務（Domain）**
| # | 服務 | 代號 | 職責 | 與現況關係 |
| --- | --- | --- | --- |
| 8 | Product / Catalog | `product` | 商品/料件主檔、分類、單位、BOM 結構 | 擴充 `Product` |
| 9 | Inventory | `inventory` | 庫存、倉庫、批號/序號、異動、盤點、WAC | 擴充 `StockMovement/Warehouse` |
| 10 | Procurement (P2P) | `procure` | 請購、詢價、採購單、進貨驗收、供應商 | 擴充 `Purchase/Supplier` |
| 11 | Sales / Order (O2C) | `sales` | 報價、訂單、出貨、POS、客戶 | 擴充 `Sale` |
| 12 | CRM | `crm` | 客戶、聯絡人、商機、合約、客訴 | 擴充 `Customer` |
| 13 | Finance / Accounting | `finance` | 總帳GL、AR、AP、會計科目、分錄、期間結轉、多幣別 | 擴充 `Account/Journal/Payment/Receipt` |
| 14 | Manufacturing (MRP) | `mfg` | 工單、BOM 用量、MRP 需求展算、產能 | 新增 |
| 15 | Project | `project` | 專案、任務、工時、專案成本 | 新增 |
| 16 | Fixed Assets | `assets` | 資產登記、折舊、處分 | 新增 |
| 17 | HR | `hr` | 員工、組織、出勤差勤 | 新增 |
| 18 | Payroll | `payroll` | 薪資、加扣項、薪資單、與財務整合 | 新增 |
| 19 | Master Data / Config | `mdm` | 共用代碼、幣別、稅率、行事曆、編號規則 | 新增 |

### 3.3 三大端到端流程（跨服務）
- **O2C（Order-to-Cash）**：`sales` 報價→訂單→`inventory` 出貨扣帳→`finance` 認列收入/AR→`finance` 收款。
- **P2P（Procure-to-Pay）**：`procure` 請購→`workflow` 簽核→採購單→`inventory` 驗收入庫(WAC)→`finance` AP→付款。
- **P2P/MRP（Plan-to-Produce）**：`mfg` 主生產排程→MRP 展算→`procure` 採購＋`mfg` 工單→`inventory` 領料/入庫→`finance` 成本歸集。

---

## 4. 技術選型（Tech Stack）

| 層 | 選型 | 理由 |
| --- | --- | --- |
| 後端框架 | ASP.NET Core 8（每服務一個專案，模組化解決方案）| 沿用既有 .NET 與會計邏輯資產 |
| ORM | EF Core 8（DB per service）| 既有經驗，遷移成本低 |
| 資料庫 | SQL Server 為主；可 polyglot：BI→列式/倉儲、搜尋→OpenSearch | 延續既有；依服務特性選型 |
| 服務間通訊 | 同步：REST/gRPC；非同步：**事件匯流排 RabbitMQ（可升級 Kafka）** | 命令同步、整合事件非同步 |
| 一致性 | **Outbox 模式 + Saga 編排** | 取代單體 DB 交易，達成最終一致 |
| 閘道 | YARP（.NET 原生反向代理）| 與 .NET 整合佳 |
| 身分 | OpenID Connect / OAuth2（Duende IdentityServer 或 Keycloak）| 標準協定、SSO |
| 前端 | React 18 + TypeScript + Vite | 生態成熟、型別安全 |
| 前端 UI 庫 | Ant Design（企業表單/表格成熟）或 MUI；TanStack Query + Zustand；ECharts | ERP 重表單表格 |
| 可觀測性 | OpenTelemetry + Serilog + Prometheus/Grafana + 集中式 trace | 微服務必備 |
| 容器/編排 | Docker + Kubernetes（+ Helm）；本機 docker compose | 獨立部署、彈性擴縮 |
| CI/CD | GitHub Actions（既有 `static.yml` 擴充）| 已在用 |
| API 契約 | OpenAPI（REST）/ Protobuf（gRPC）| 契約優先、前後端解耦 |

---

## 5. 關鍵架構決策（ADR 摘要）

- **ADR-01 模組化單體 → 微服務（漸進式）**：以 **Strangler Fig** 模式，先把現有單體重構為清楚分層的模組化單體，再逐一抽離為服務，避免一次性大爆炸改寫。
- **ADR-02 Database per Service**：各服務獨佔資料庫，禁止跨服務直接讀寫他人資料表；資料同步走事件或 API。
- **ADR-03 跨服務一致性用 Saga**：例如「銷售出貨」涉及 `sales`/`inventory`/`finance`，以編排式 Saga + 補償交易確保最終一致；**會計分錄改由 `finance` 消費整合事件產生**（非單體內同交易）。
- **ADR-04 WAC 成本歸屬於 `inventory`**：成本計算留在庫存服務，出貨時將 COGS 金額隨「出貨完成」事件發佈給 `finance` 入帳，避免跨庫查詢。
- **ADR-05 認證集中、授權分散**：閘道驗證 JWT，各服務依 scope/claims 做細粒度授權（policy-based）。
- **ADR-06 前後端契約優先**：以 OpenAPI 產生 TS client，前端不手寫型別。

---

## 6. 分階段路線圖（Roadmap）

> 原則：**先治理骨幹、再核心流程、後擴充模組**。每階段可獨立交付、可展示。

### Phase 0 — 地基（Foundation）
- 建 mono-repo 解決方案結構（`src/services/*`、`src/gateway`、`src/shared`、`web/`）。
- 共用元件：`BuildingBlocks`（事件匯流排抽象、Outbox、認證中介、錯誤處理、稽核發佈）。
- CI/CD、docker compose 本機全家桶、OpenTelemetry 骨架。
- **產出**：可跑的空殼閘道 + 一個樣板服務 + 前端殼。

### Phase 1 — 身分與治理骨幹
- `iam`（使用者/角色/權限/組織/多公司）、`gateway` 認證、`audit`、`workflow`（簽核引擎）、`notify`。
- 前端：登入、權限驅動選單、組織/角色管理畫面。
- **產出**：登入即受控、所有後續模組可掛簽核與稽核。

### Phase 2 — 核心進銷存上雲（拆既有單體）
- 抽離 `product`、`inventory`、`procure`、`sales`、`crm`、`mdm`。
- 以事件串起 O2C 與 P2P；WAC 留在 `inventory`。
- 前端：主檔、採購、銷售、庫存模組重構為 React。
- **產出**：既有功能在微服務上等價運行 + RBAC + 簽核。

### Phase 3 — 財務會計重構
- `finance`：GL、AR、AP、科目、**事件驅動分錄**、期間結轉、試算表、多幣別。
- 把 `AccountingService` 借貸邏輯遷為「消費整合事件」模型。
- 前端：總帳、應收應付、財務報表（含試算表借貸平衡驗證）。
- **產出**：跨服務交易仍維持借貸平衡與帳實相符。

### Phase 4 — 擴充企業模組
- `mfg`（BOM/工單/MRP）、`project`、`assets`、`hr`、`payroll`、`bi`。
- 前端：對應模組與跨模組 BI 儀表板。
- **產出**：完整全功能 ERP。

### Phase 5 — 強化與上線
- 效能、壓測、災備、資料遷移工具、權限稽核審查、文件與教育訓練。

> 註：本計畫聚焦「規劃與設計」，實作於各階段另行展開。

---

## 7. 跨領域關注點（Cross-Cutting）

| 主題 | 策略 |
| --- | --- |
| 多公司/多租戶 | `CompanyId` 貫穿所有領域資料；IAM 控制可存取公司；報表可合併 |
| 多幣別 | `finance` 管理匯率；交易記原幣＋本位幣 |
| 安全 | OIDC、最小權限、欄位級遮罩、機密以 Secret 管理、傳輸/靜態加密 |
| 稽核與合規 | 不可變稽核軌跡、財務分錄不可刪僅可沖銷、期間鎖定 |
| 可觀測性 | 結構化日誌、分散式追蹤（correlation id 貫穿事件）、健康檢查 |
| 國際化 | 介面文字外部化，預設 zh-TW，可擴充 en |
| 資料遷移 | 既有 InvenFlow 資料以 ETL 匯入新服務（對應表附於 DB 設計） |

---

## 8. 風險與假設

### 風險
| 風險 | 衝擊 | 緩解 |
| --- | --- | --- |
| 微服務複雜度過高（團隊規模/維運） | 高 | 先模組化單體再拆；只在邊界清楚處拆分；保留合併部署選項 |
| 跨服務財務一致性難 | 高 | Outbox + Saga + 對帳批次；分錄集中於 `finance` |
| 範圍過大難收斂 | 高 | 嚴格分階段、每階段可交付；MVP 優先 |
| 資料遷移風險 | 中 | 對應表 + 試運行 + 可回溯 |

### 假設
- 沿用 .NET 與 SQL Server 既有資產；團隊具 C# 能力。
- 初期單一資料中心/雲；K8s 可用。
- zh-TW 為主要語系、台灣會計實務（可調整）。

---

## 9. 後續文件
1. `01-計畫檢核報告.md` — 由獨立 agent 對本計畫進行對抗式檢核。
2. `02-資料庫設計.md` — 各服務資料模型（DB per service）＋既有資料對應。
3. `03-後端微服務設計.md` — 服務邊界、API 契約、事件、Saga、閘道。
4. `04-前端UIUX設計.md` — 設計系統、資訊架構、權限驅動 UI、關鍵畫面。
