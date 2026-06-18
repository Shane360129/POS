# InvenFlow — 智慧進銷存與會計管理平台

> 📐 **企業級 MIS/ERP 轉型規劃進行中** — 本專案正規劃升級為全功能企業 ERP（模組化單體優先 → 漸進微服務、React + TS 前端重構）。完整設計文件（總計畫／檢核報告／資料庫／後端／前端）見 [`docs/`](./docs/README.md)。

兩種跑法，同一套模組架構（主檔 / 進貨 / 銷售 / 庫存 / 應收應付 / 報表 / 設定）：

| 版本 | 位置 | 後端 | 用途 |
| --- | --- | --- | --- |
| **靜態 demo** | 根目錄 | 無（瀏覽器 `localStorage`）| 部署 GitHub Pages 直接體驗 |
| **Full-stack** | [`server/`](./server/) | ASP.NET Core 8 + EF Core + SQL Server | 真實後端、可開發延伸 |

原 M06 ASP.NET WebForms 進銷存後台已歸檔到 `legacy/` 供參考。

## 靜態 demo 啟動方式

### 本機開發

直接以靜態伺服器服務本資料夾，例如：

```bash
python3 -m http.server 8000      # 然後開 http://localhost:8000/
# 或
npx http-server . -p 8000
```

無需後端、無需資料庫。所有資料儲存於瀏覽器 `localStorage`，
首次載入會自動植入 12 商品、4 供應商、5 客戶、8 進貨、13 銷售、
3 付款、4 收款的示範資料。

進入 `index.html` 為產品介紹頁，點「啟動 Demo」進入 `app.html`。

### 部署到 GitHub Pages

repo → Settings → Pages：
- Source：`Deploy from a branch`
- Branch：`master` / `(root)` → 儲存

部署後網址：`https://<user>.github.io/POS/`

## Full-stack 版啟動

完整 ASP.NET Core 8 + SQL Server 後端在 [`server/`](./server/)，docker compose 一鍵起：

```bash
cd server
docker compose up -d        # 起 SQL Server + API
open http://localhost:5080  # Admin Console
# http://localhost:5080/swagger     ← Swagger API 文件
```

不想裝 docker / SQL Server 也可用 SQLite 模式：
```bash
cd server/InvenFlow.Api
InvenFlow__UseSqlite=true \
  ConnectionStrings__DefaultConnection="Data Source=invenflow.db" \
  dotnet run
```

詳細 API 表、entity 關聯、商業邏輯說明見 [`server/README.md`](./server/README.md)。

## 模組對應（原 → 新）

| 原 prg | 模組 | InvenFlow 對應 |
| --- | --- | --- |
| prg10 | 系統設定 | 系統設定 / 會計科目 |
| prg20 | 主檔 | 商品 / 供應商 / 客戶 / 倉庫 |
| prg30 | 進貨入庫 | 進貨作業 |
| prg40 | 銷貨出貨 | 銷售作業 |
| prg50 | 應付 / 應收 / 付款 / 收款 | 應付帳款 / 應收帳款 |
| prg60 | 庫存盤點 | 庫存管理 (查詢 / 異動 / 調整) |
| prg70 | 報表 | 報表分析 (毛利 / 排行 / 週轉 / 試算) |
| prg80 | 月結 | (示範略) |
| prg90 | 系統管理 | 系統設定 |

## 商業邏輯重點

### 加權平均成本 (Weighted Average Cost, WAC)

**進貨時** 重新計算平均成本：
```
new_avg = (old_qty × old_avg + qty × unit_cost) / (old_qty + qty)
```

**銷售時** 以當下平均成本鎖定銷貨成本：
```
COGS = qty × current_avg
```
平均成本本身不因銷貨而變動，直到下一次進貨才會被刷新。

### 會計分錄（自動產生、借貸平衡）

| 事件 | 借方 (Dr) | 貸方 (Cr) |
| --- | --- | --- |
| 進貨 (賒帳) | 1310 存貨 | 2100 應付帳款 |
| 進貨 (現金) | 1310 存貨 | 1100 現金 |
| 銷售 (賒帳) | 1130 應收帳款 + 5100 銷貨成本 | 4100 銷貨收入 + 1310 存貨 |
| 銷售 (現金) | 1100 現金 + 5100 銷貨成本 | 4100 銷貨收入 + 1310 存貨 |
| 付款 | 2100 應付帳款 | 1100 現金 |
| 收款 | 1100 現金 | 1130 應收帳款 |
| 盤盈 | 1310 存貨 | 4900 存貨盤盈 |
| 盤虧 | 5900 存貨盤虧 | 1310 存貨 |
| 作廢回沖 | 反向沖銷以上分錄 | 反向沖銷以上分錄 |

報表分析 → 試算表 可即時驗證借貸平衡。

## 檔案結構

```
.
├── index.html              # 產品介紹 / Landing（靜態 demo）
├── app.html                # 主應用 SPA shell（靜態 demo）
├── assets/
│   ├── css/style.css
│   └── js/{data,accounting,views,app}.js
├── server/                 # Full-stack 版（ASP.NET Core + SQL Server）
│   ├── InvenFlow.Api/
│   │   ├── Models/         # entity
│   │   ├── Data/           # DbContext + Seed
│   │   ├── Services/       # AccountingService (WAC) + ReportService
│   │   ├── Controllers/
│   │   ├── Dtos/
│   │   ├── wwwroot/        # admin console（fetch API 版前端）
│   │   └── Program.cs
│   ├── docker-compose.yml  # SQL Server + API
│   └── README.md
└── legacy/                 # 原 M06 ASP.NET WebForms 專案（封存）
```
