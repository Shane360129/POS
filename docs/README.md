# InvenFlow → 企業級 MIS/ERP 轉型設計文件

本資料夾是把現有 **InvenFlow 進銷存＋會計系統** 升級為 **全功能企業 ERP（MIS）** 的規劃與設計文件。
依使用者指定流程產出：**①列計畫 → ②另一個 agent 對抗式檢核 → ③據以規劃資料庫 / 後端 / 前端**。

## 已定案方向
| 項目 | 決定 |
| --- | --- |
| 範圍 | **全功能企業 ERP**（採購/銷售/庫存/財務會計/CRM＋backlog：製造MRP/專案/固定資產/HR/薪資） |
| 架構 | **模組化單體（Modular Monolith）優先、預留拆分縫 → 漸進抽離為微服務**（檢核後由「一步到位微服務」調整而來） |
| 前端 | React 18 + TypeScript + Vite + **Ant Design** + react-hook-form + zod + TanStack Query + Zustand + ECharts |
| 後端 | ASP.NET Core 8 模組化單體（EF Core、多 schema 單一 DB）、in-process 介面 + MediatR/Outbox |
| 資料庫 | SQL Server 為主（設計可移植 PostgreSQL） |
| 身分 | Keycloak（OIDC/OAuth2）、RBAC + ABAC、列級/欄位級權限 |
| 一致性 | **會計分錄維持本地同交易**；未來拆 `fin` 才採 Outbox+Saga+每日對帳+關期前置檢查 |

## 文件清單（建議閱讀順序）
| # | 文件 | 內容 |
| --- | --- | --- |
| 00 | [轉型總計畫](./00-MIS-ERP-轉型總計畫.md) | 願景、現況差距、目標架構、模組清單、技術選型、ADR、路線圖、風險（**v2.0 已依檢核修訂**） |
| 01 | [計畫檢核報告](./01-計畫檢核報告.md) | 獨立 agent 對抗式檢核：找出既有 bug、評估過度設計、Must/Should/Nice-fix 清單（評 B−） |
| 02 | [資料庫設計](./02-資料庫設計.md) | 9 個 schema、共用欄位、分錄三道硬約束、期間/稅/發票/退貨/維度/多倉/Outbox/稽核、DDL 草稿、舊→新對應 |
| 03 | [後端模組化設計](./03-後端模組化設計.md) | 模組邊界與拆分觸發條件、**兩套一致性**、修正既有交易 bug、RBAC+ABAC、期間鎖定、編號、稽核、API、測試、Spike |
| 04 | [前端 UI/UX 設計](./04-前端UIUX設計.md) | 設計系統與深色模式、權限驅動 UI、依三大流程的資訊架構、ERP 重表單表格規範、財務正確性呈現 |

## 檢核帶來的關鍵修正（v1.0 → v2.0）
1. 改正 As-Is 誤述：既有 `AccountingService` **無交易保證**（雙 `SaveChangesAsync`、無 `BeginTransaction`），是潛在帳實不符 bug。
2. 架構降為模組化單體優先；Kafka/K8s/gRPC/資料倉儲/YARP/Saga 整批延後並設量化觸發條件。
3. 會計分錄維持本地同交易（不在單體階段事件化）。
4. 新增一級主題：稅務/統一發票、退貨折讓、會計期間鎖定、GL 維度、多倉庫存、測試策略、編號併發。
5. `mdm` 提前、`workflow` 精簡、`product`+`inventory` 合併、`crm` 併入 `sales`、身分用 Keycloak、UI 定 Ant Design。

## 三大端到端流程
- **O2C**：`sales` 報價→訂單→`inv` 出貨扣帳（WAC 鎖 COGS）→`fin` 同交易認列收入/AR/稅→收款。
- **P2P**：`procure` 請購→`wf` 簽核→採購單→`inv` 驗收入庫（刷 WAC）→`fin` AP/進項稅→付款。
- **Plan-to-Produce（backlog）**：`mfg` MPS→MRP→`procure`+`mfg` 工單→`inv` 領料/入庫→`fin` 成本歸集（依 GL 維度）。

## 命名對齊註記（三份文件間）
為避免實作時混淆，於此一處收斂三份文件的輕微命名差異（**以 `02` 資料庫設計為資料模型最終依據**）：
- **workflow 模組 schema**：資料模型 schema 名為 **`wf`**（見 `02` §3.8）；`03` 行文以「`workflow` 模組」稱之，兩者同指一物。
- **`notify` schema 與通知表**：`notify` 模組屬 **Phase 2**，`03` 已述其介面；`02` 目前聚焦 Phase 1–3 核心交易表，通知相關表於 Phase 2 補。
- **編號表**：名為 **`mdm.NumberSequence`**（`02` §3.2/§7.5）；即 `03` 所述「編號服務」之資料來源。
- **拆分階段才出現的表**：`ProcessedEvent`（消費端冪等去重）、`DeadLetter`（補償失敗佇列）為 **未來拆分 `fin` 時** 才新增（`03` §4 列為拆分驗收條件）；單體階段僅先建 `app.Outbox`（`02` §2.1）。

## 後續
本套文件聚焦「規劃與設計」。實作建議依 `00` 的 Roadmap（Phase 0 地基 → 1 治理骨幹 → 1.5 模組化+修 bug → 2 進銷存 → 3 財務稅務）推進，並先完成 `03` 的 Spike A/B/C/D 再投入大規模實作。
