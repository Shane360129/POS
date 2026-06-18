# InvenFlow → 企業級 MIS/ERP 轉型總計畫

> 版本：**v2.0（已依 `01-計畫檢核報告.md` 修訂）**
> 目標：將現有 **InvenFlow 進銷存＋會計系統** 升級為 **全功能企業 ERP（MIS）**
> 架構方向：**模組化單體（Modular Monolith）優先、預留拆分縫 → 漸進抽離為微服務**
> 前端：**React 18 + TypeScript 重構**　|　後端：**ASP.NET Core 8 模組化單體**

> **v2.0 修訂重點（相對 v1.0）**
> 1. 改正 As-Is 關鍵誤述：既有 `AccountingService` **並無交易保證**（兩次 `SaveChangesAsync`、無 `BeginTransaction`），是潛在帳實不符 bug。
> 2. 架構由「一步到位 19 微服務」降為「**模組化單體優先、6–8 核心模組、預留拆分縫**」；Kafka/K8s/gRPC/資料倉儲/YARP/Saga 整批延後並設量化觸發條件。
> 3. 會計分錄**維持本地、同交易**產生（不在單體階段事件化）。
> 4. 新增一級主題：**稅務/統一發票、退貨折讓、會計期間鎖定、GL 維度、多倉庫存、測試策略、編號併發**。
> 5. `mdm` 提前到地基；`workflow` 先硬編碼；`product`+`inventory` 合併；`crm` 基本功能併入 `sales`；身分用 **Keycloak**；前端 UI 庫定為 **Ant Design**。

---

## 1. 願景與目標

把目前單體的「進銷存＋會計」demo，演進為一套可支撐企業營運的 **模組化 ERP/MIS 平台**，涵蓋採購、銷售、庫存、財務會計、CRM、製造（MRP/BOM）、專案、固定資產、人資與薪資差勤，並以 **權限治理（RBAC/ABAC）、簽核、稽核軌跡、稅務合規、BI 報表** 作為跨模組治理骨幹。

> **架構與範圍是兩件事**：採「模組化單體」不代表縮減功能範圍——**全功能 ERP 的範圍維持不變**，只是先以「單一部署單元、清楚模組邊界」交付，待量化觸發條件成立再把模組抽離為獨立服務。

### 成功指標（North Star）
| 面向 | 指標 |
| --- | --- |
| 功能 | 涵蓋三大端到端流程：訂單到收款（O2C）、採購到付款（P2P）、計畫到生產（Plan-to-Produce/MRP） |
| 正確性 | **每筆分錄寫入時借貸恆等（DB 約束保證）**；庫存帳實相符；關期後不可竄改；皆有自動化測試覆蓋 |
| 安全 | 全 API 經認證授權；列級/欄位級權限可驗證；稽核軌跡不可竄改 |
| 合規 | 台灣統一發票（進/銷項稅、字軌、折讓）、會計期間鎖定、憑證保存 |
| 維運 | 單一部署可上線；可觀測性（log/trace/metric）齊備；模組邊界清楚、可演進 |
| 體驗 | 響應式、深色模式、i18n（zh-TW 為主）、權限驅動 UI |

---

## 2. 現況評估（As-Is）— 已對照程式碼校正

### 2.1 既有資產
- **靜態 demo**（根目錄）：vanilla JS + `localStorage`。
- **全端版**（`server/InvenFlow.Api`）：ASP.NET Core 8 + EF Core + SQL Server，**單體、單一資料庫、單一 `.csproj`、12 張表**。
  - Models（已核對）：`Product / Supplier / Customer / Warehouse / Account / Purchase / Sale / Payment / Receipt / StockMovement / JournalEntry / JournalLine`。
  - `AccountingService`：WAC 加權平均成本、自動借貸分錄、作廢反向沖銷。
  - `ReportService`：儀表板/毛利/排行/週轉/試算表。
- **legacy**：原 M06 ASP.NET WebForms 進銷存（封存參考）。

### 2.2 與「企業 ERP」的差距（Gap）— 含檢核發現的既有 bug
| 缺口 / 既有問題 | 嚴重度 | 說明（含程式碼證據） |
| --- | --- | --- |
| **分錄無交易保證（既有 bug）** | 🔴 | `ApplyPurchaseAsync` 等先 `SaveChangesAsync()` 寫庫存、**再**一次 `SaveChangesAsync()` 寫分錄，全程無 `BeginTransaction`。兩次之間崩潰 → 庫存已動、分錄未生，帳實不符。**轉型第一步即補上顯式交易。** |
| **借貸平衡靠程式碼「剛好寫對」** | 🟠 | `TrialBalanceAsync` 僅查詢時加總比對；DB 無 CHECK，繞過 service 即破壞。需**寫入時硬約束**。 |
| **單號併發重號（既有 bug）** | 🟠 | `NextNumberAsync` 用 `Count()+1`；併發或作廢後會重號。改 **DB sequence / Hi-Lo**。 |
| **無身分/權限** | 🔴 | `Program.cs` 無 `AddAuthentication/Authorization`、無 `[Authorize]`；**CORS `AllowAnyOrigin` 全開**。 |
| **無稽核軌跡** | 🔴 | 無 who/when 欄位、無稽核表。 |
| **無會計期間鎖定** | 🔴 | `Cancel*/ApplyAdjustment` 用 `DateTime.UtcNow` 可寫任意日期分錄，不檢查期間。 |
| **無稅務/統一發票** | 🔴 | `Purchase/Sale/Item` 無稅額欄位；科目表無進/銷項稅科目。台灣營運必備。 |
| **無退貨/折讓** | 🔴 | 只有「整單作廢」，無部分退貨、紅字折讓。 |
| **零測試** | 🔴 | 全 repo 無測試專案；財務系統不可接受。 |
| **多倉是假的** | 🟠 | 庫存餘額/均價只在 `Product` 層；`StockMovement` 雖記 `WarehouseId` 但扣帳只改 `Product.Stock`。 |
| **GL 無維度** | 🟠 | 科目無部門/成本中心/專案維度，出不了部門/專案損益。 |
| **作廢不還原 WAC** | 🟠 | 進貨作廢刻意不回沖均價（`AccountingService` 行 221 註解），均價會失真。 |
| **無多公司/多幣別** | 🟠 | 所有表無 `CompanyId`、無幣別；屬橫切改造（每表每查詢），非單一新模組。 |
| **前端不可維護** | 🟡 | vanilla JS 手刻 DOM，難支撐大型表單與權限。 |
| **無可觀測性/CI-CD/容器編排** | 🟡 | 無 log 聚合、trace、pipeline。 |

---

## 3. 目標架構（To-Be）

### 3.1 全景圖（模組化單體 + 拆分縫）
```
                    ┌─────────────────────────────┐
                    │   React 18 + TS 前端 (SPA)    │  Web / 平板
                    └──────────────┬──────────────┘
                                   │ HTTPS / OIDC JWT
                    ┌──────────────▼──────────────┐
                    │   ASP.NET Core 8 後端（單一部署單元）        │
                    │   認證中介(JWT) · 授權(RBAC/ABAC) · 稽核攔截 │
                    │ ┌──────────────────────────────────────┐ │
                    │ │  模組（各自 schema，禁跨模組直接 JOIN）   │ │
                    │ │  iam · mdm · 物料庫存 · procure · sales   │ │
                    │ │  finance · audit · (workflow)            │ │
                    │ │  ── 模組間僅經 in-process 介面呼叫 ──     │ │
                    │ │  IInventoryService / ISalesService ...    │ │
                    │ │  ── 領域事件：MediatR + Outbox 表 ──       │ │
                    │ └──────────────────────────────────────┘ │
                    └──────────────┬──────────────┘
                                   │ 1 個 DB（多 schema 隔離）
                    ┌──────────────▼──────────────┐
                    │  SQL Server（或 PostgreSQL）  │
                    └─────────────────────────────┘

        ── 拆分縫（日後觸發條件成立才啟用）──
        in-process 介面 → 遠端代理 / MediatR → RabbitMQ / 單 DB → DB-per-service
        外部身分：Keycloak（一開始就獨立，因安全邊界）
```

### 3.2 有界上下文 / 模組清單（共用基準）

> 此清單為 **資料庫、後端、前端三份設計文件的共用基準**。
> **形態**：以下皆為「**模組化單體內的模組**」（各自 schema、in-process 介面、禁止跨模組直接 JOIN），而非獨立部署的服務。`Phase` 欄標示落地順序。

**A. 平台 / 跨領域**
| 模組 | 代號(schema) | 職責 | Phase | 與 v1.0 差異 |
| --- | --- | --- | --- | --- |
| Identity & Access | `iam` | 使用者、角色、權限(RBAC/ABAC)、組織、多公司租戶、登入(委由 Keycloak) | 1 | 身分改用 **Keycloak**，不自建 IdentityServer |
| Master Data & Config | `mdm` | 共用代碼、幣別、稅率、行事曆、**編號規則(sequence)**、客戶/供應商主檔 | **0–1（提前）** | 由第 19 提前至地基；併入客戶/供應商主檔 |
| Audit & Activity | `audit` | 稽核軌跡（**EF SaveChanges 攔截器、同庫稽核表**） | 1 | 不做成異步消費服務，改同源攔截器 |
| Workflow / Approval | `workflow` | 簽核：**先硬編碼兩階**，需求穩定後再抽象引擎 | 2（精簡） | 通用引擎延後，避免過度設計 |
| Notification | `notify` | Email/站內信、範本 | 2 | — |
| Document / File | `docs-svc` | 附件、檔案儲存 | 3 | — |
| Reporting / BI | `bi` | 報表、儀表板（**起步用讀取複本/物化檢視 + ECharts**，非資料倉儲） | 3+ | 資料倉儲延後 |

**B. 商業領域**
| 模組 | 代號(schema) | 職責 | Phase | 與 v1.0 差異 |
| --- | --- | --- | --- | --- |
| 物料與庫存 | `inv`(product+inventory) | 料件主檔、分類、單位、BOM、**多倉庫存餘額/成本**、WAC、批號/序號、盤點 | 2 | **product 與 inventory 合併**（WAC 強耦合，避免拆成 Saga） |
| Procurement (P2P) | `procure` | 請購、採購單、進貨驗收（含進項稅）、供應商往來 | 2 | — |
| Sales / Order (O2C) | `sales` | 報價、訂單、出貨、退貨折讓、POS、**CRM 基本**（客戶/聯絡/商機） | 2 | **CRM 基本併入**；新增退貨折讓 |
| Finance / Accounting | `fin` | 總帳GL（**含維度**）、AR、AP、科目、**同交易分錄**、期間鎖定、稅務、多幣別 | 3 | 分錄**維持同步同交易**；新增維度/期間/稅 |
| Manufacturing (MRP) | `mfg` | 工單、BOM 用量、MRP 展算、產能 | **Backlog（需求未驗證）** | 不納入前 4 Phase 架構承諾 |
| Project | `project` | 專案、任務、工時、專案成本 | **Backlog** | 同上 |
| Fixed Assets | `assets` | 資產登記、折舊、處分 | **Backlog** | 同上 |
| HR | `hr` | 員工、組織、出勤差勤 | **Backlog** | 同上 |
| Payroll | `payroll` | 薪資、加扣項、薪資單、與財務整合 | **Backlog** | 同上 |

> **第一階段真正要做的核心 6–8 模組**：`iam`、`mdm`、`inv`（物料庫存）、`procure`、`sales`（含 CRM 基本）、`fin`、`audit`、`workflow`（精簡）。其餘為 backlog，設計文件會涵蓋邊界但不畫高保真細節。

### 3.3 三大端到端流程
- **O2C**：`sales` 報價→訂單→`inv` 出貨扣帳（WAC 鎖 COGS）→`fin` **同交易**認列收入/AR/稅→收款。
- **P2P**：`procure` 請購→`workflow` 簽核→採購單→`inv` 驗收入庫（刷 WAC）→`fin` AP/進項稅→付款。
- **Plan-to-Produce（backlog）**：`mfg` MPS→MRP 展算→`procure`+`mfg` 工單→`inv` 領料/入庫→`fin` 成本歸集（依 GL 維度）。

---

## 4. 技術選型（Tech Stack）— 已依檢核精簡

| 層 | 選型（初期） | 拆分後（觸發條件成立才升級） | 理由 |
| --- | --- | --- | --- |
| 後端 | ASP.NET Core 8（單一專案、模組資料夾/類別庫分層） | 模組抽為獨立服務 | 沿用 .NET 資產 |
| ORM | EF Core 8（**多 schema 單一 DB**、補顯式交易、借貸 CHECK） | DB-per-service | 低遷移成本 |
| 資料庫 | **SQL Server**（**並評估 PostgreSQL 省授權成本**） | 各服務獨立庫 | 延續既有，成本可選 |
| 模組間通訊 | **in-process 介面**（`IInventoryService`） | 換遠端代理 / REST | 保留拆分縫、零分散式稅 |
| 領域事件 | **MediatR in-process + Outbox 表**（先建，含 `EventId` 冪等鍵） | 換 RabbitMQ | 契約不變、可平滑拆分 |
| 一致性 | **本地 DB 交易**（同交易產生分錄） | Outbox+Saga+**每日對帳**+關期前置檢查 | 先用 RDBMS 最擅長的 ACID |
| 身分 | **Keycloak**（或雲 IdP），OIDC/OAuth2 | 同 | 免自建、免授權費 |
| 閘道 | 暫無（ASP.NET 中介軟體） | YARP | 單體不需閘道 |
| 前端 | **React 18 + TS + Vite + Ant Design** + react-hook-form + zod + TanStack Query + Zustand + ECharts | 同 | 重表單表格 ERP 的成熟組合 |
| API 契約 | **OpenAPI → 產生 TS client**（不手寫型別） | + 版本管理 | 前後端解耦 |
| 可觀測性 | Serilog + OpenTelemetry → Seq/Grafana Cloud | + Prometheus/Grafana 全套 | 單體階段從簡 |
| 容器/部署 | **docker compose / PaaS** | Kubernetes + Helm | 避免 K8s 黑洞 |
| CI/CD | GitHub Actions（擴充既有 `static.yml`） | 多服務 pipeline | 已在用 |
| 測試 | xUnit + FluentAssertions + property-based + Playwright(E2E) | + 契約/對帳測試 | North Star 要求正確性 |

> **整批延後（設量化觸發條件）**：Kafka、Kubernetes+Helm、gRPC、資料倉儲、OpenSearch、YARP 閘道、通用 workflow 引擎、DB-per-service、Saga。觸發條件示例：模組 build/部署成為瓶頸、需多團隊獨立部署節奏、單一模組需獨立擴縮、單庫成為效能瓶頸。

---

## 5. 關鍵架構決策（ADR 摘要）— 已修訂

- **ADR-01（修訂）模組化單體優先**：以 Strangler Fig，先把單體重構為**清楚分層的模組化單體**（含 Phase 1.5：補交易、補借貸 CHECK、補 audit 攔截器），**在此停留夠久**，再依量化觸發條件逐一抽離。Roadmap 與此一致（修正 v1.0 矛盾）。
- **ADR-02 模組 schema 隔離**：同一 DB 內以 schema 隔離，**禁止跨模組直接 JOIN / FK 到他人核心表**；跨模組存取走 in-process 介面或領域事件。
- **ADR-03（修訂）會計分錄維持本地同交易**：`sales`/`procure` 在**同一 DB 交易**內呼叫 `fin` 產生分錄（沿用並補強 `AccountingService`：顯式交易 + 借貸平衡硬約束）。**不在單體階段事件化分錄**。僅當未來確定拆 `fin`，才採「事件 + 冪等 + 每日對帳 + 關期前置檢查」四件套。
- **ADR-04（修訂）成本政策歸 finance/會計治理**：`inv` 提供數量與單位成本快照，**會計政策（成本流動假設）由 `fin` 治理**，避免兩處各有「成本真相」。
- **ADR-05 認證集中、授權分散**：Keycloak 發 JWT，中介軟體驗證；各模組 policy-based 授權 + **全域 query filter 做列級過濾**（依使用者可見公司/倉/部門）。
- **ADR-06 契約優先**：OpenAPI 產生 TS client，前端不手寫型別。
- **ADR-07（新增）會計期間鎖定**：`AccountingPeriod`（Open/Closed/Locked）；所有寫分錄路徑（含 Cancel/Adjustment）統一經期間開放檢查。
- **ADR-08（新增）分錄不可竄改**：分錄 append-only；應用帳號無 UPDATE/DELETE 分錄權；更正一律以沖銷分錄處理。
- **ADR-09（新增）編號以 sequence/Hi-Lo**：取代 `Count()+1`，由 `mdm` 統一字軌規則。
- **ADR-10（新增）多公司/多幣別為橫切**：所有表加 `CompanyId`、交易記原幣+本位幣；先做 Spike C 量測成本。

---

## 6. 分階段路線圖（Roadmap）— 已修訂

### Phase 0 — 地基
- Mono-repo 結構（`src/Modules/*`、`src/Host`、`src/BuildingBlocks`、`web/`）。
- `BuildingBlocks`：交易/單元工作、Outbox（含 `EventId`）、MediatR、認證中介、稽核攔截器、ProblemDetails 錯誤模型、FluentValidation。
- **`mdm` 先行**：編號 sequence、幣別、稅率、共用代碼。
- CI/CD、docker compose、Serilog+OTel 骨架、測試專案骨架。

### Phase 1 — 身分與治理骨幹
- `iam`（使用者/角色/權限/組織/多公司，接 Keycloak）、認證授權中介、**列級 query filter**、`audit` 攔截器、`notify`。
- 前端：登入、權限驅動選單、組織/角色/權限管理。

### Phase 1.5 —（新增）既有單體模組化 + 修 bug
- 把現有進銷存重構為模組（`inv`、`procure`、`sales`）；**補顯式交易**、**借貸平衡 CHECK**、**多倉庫存餘額表**、**編號 sequence**、**收斂 CORS + 輸入驗證**。
- 補既有商業邏輯的單元測試（WAC、借貸平衡 property test）。

### Phase 2 — 核心進銷存 + 流程治理
- `inv`（物料+多倉庫存+批號/序號）、`procure`、`sales`（含退貨折讓、CRM 基本）；O2C/P2P 串接；`workflow` 精簡兩階簽核。
- 前端：主檔、採購、銷售、庫存模組（React）。

### Phase 3 — 財務會計 + 稅務合規
- `fin`：GL（**含維度**）、AR、AP、**同交易分錄**、期間鎖定、**台灣統一發票/進銷項稅/折讓**、多幣別；`bi`（讀取複本/物化檢視）、`docs-svc`。
- 前端：總帳、應收應付、發票、財務報表（試算表借貸紅綠燈、關期狀態）。

### Phase 4 — 擴充企業模組（backlog，需求驗證後）
- `mfg`/`project`/`assets`/`hr`/`payroll` 視需求逐一導入。

### Phase 5 — 強化與（條件性）拆分
- 效能/壓測/DR；**達觸發條件**才把 1–2 個低耦合模組（如 `iam`、`notify`）抽離為服務，導入 RabbitMQ + 對帳。

### Spike（正式實作前的技術探針）
- **Spike A 跨模組財務一致性**：驗證 Outbox/對帳能抓出重複/亂序/補償失敗（決定未來是否拆 `fin`）。
- **Spike B 期間鎖定 + 跨期沖銷**。
- **Spike C 多公司/多幣別橫切成本量測**。
- **Spike D 台灣統一發票/折讓模型化**（對 Sale/Journal 的侵入度）。

> 資料遷移：舊 demo 資料量小且模型差異大，**直接以新模型 re-seed**，不另做 ETL 工具。

---

## 7. 跨領域關注點（Cross-Cutting）— 已強化機制

| 主題 | 機制（非僅名詞） |
| --- | --- |
| 多公司/多租戶 | 所有表 `CompanyId`；全域 query filter 依使用者可見公司過濾；報表可合併 |
| 多幣別 | `fin` 管匯率；交易記原幣 + 本位幣金額 |
| 授權 | RBAC（角色-權限-資源矩陣）+ ABAC（屬性：公司/倉/部門/金額上限）；評估點＝中介軟體 + 資源層 filter；**列級過濾**用全域 query filter；**欄位級遮罩**（薪資/成本）後端 enforce、前端僅體驗 |
| 稽核不可竄改 | EF SaveChanges 攔截器寫 `AuditLog`；分錄 append-only + DB 權限收斂 |
| 會計期間鎖定 | `AccountingPeriod` 狀態機；寫入/沖銷前檢查 |
| 稅務/發票 | 進/銷項稅科目與稅額欄位；統一發票字軌、紅字折讓、401/403 申報欄位 |
| 退貨/折讓 | 部分退貨回補庫存（WAC 政策明確）、紅字發票、AR/AP 沖減 |
| 編號 | DB sequence/Hi-Lo，字軌規則集中於 `mdm` |
| 可觀測性 | 結構化日誌、correlation id、健康檢查、（拆分後）分散式追蹤 |
| 國際化 | 文字外部化，預設 zh-TW，可擴 en |
| 測試 | WAC 單元、借貸平衡 property test、關期回歸、E2E；（拆分後）契約與對帳測試 |

---

## 8. 風險與假設

### 風險
| 風險 | 衝擊 | 緩解 |
| --- | --- | --- |
| 範圍過大難收斂 | 高 | 核心 6–8 模組優先；backlog 模組需求驗證後再做；每階段可交付 |
| 財務正確性（跨模組） | 高 | 維持本地同交易分錄 + 借貸 CHECK；拆分前先做 Spike A 與每日對帳 |
| 過早拆分導致複雜度爆炸 | 高 | 模組化單體優先；拆分設量化觸發條件 |
| 稅務/發票合規深度被低估 | 中 | Spike D 先驗證模型侵入度 |
| 多公司/多幣別橫切改造 | 中 | Spike C 量測；地基期即納入欄位 |

### 假設
- 沿用 .NET；DB 採 SQL Server 或 PostgreSQL（待定，預設前者、評估後者）。
- 小團隊（約 1–3 人），單一資料中心/PaaS 起步。
- zh-TW 與台灣會計/稅務實務為主，可調整。

---

## 9. 後續文件
1. ✅ `01-計畫檢核報告.md` — 獨立 agent 對抗式檢核（已完成，本 v2.0 據此修訂）。
2. `02-資料庫設計.md` — 模組 schema 設計、共用欄位、分錄硬約束、期間/稅/發票/退貨/維度/多倉/Outbox/稽核表、既有資料對應。
3. `03-後端模組化設計.md` — 模組邊界、in-process 介面、MediatR/Outbox、**兩套一致性（單體本地交易 / 未來拆分）**、授權模型、期間鎖定、編號、API 契約、Spike。
4. `04-前端UIUX設計.md` — 設計系統（AntD）、資訊架構（依三大流程）、權限驅動 UI、ERP 重表單表格規範、財務正確性呈現。
