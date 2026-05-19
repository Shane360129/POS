# InvenFlow Full-stack 後端

ASP.NET Core 8 Web API + EF Core + SQL Server，配上一個前端 admin console 走 API。
這個資料夾是「真有後端」版本，根目錄那份是純靜態 demo。

## 架構

```
server/
├── InvenFlow.Api/             # ASP.NET Core 8 Web API
│   ├── Models/                # entity classes
│   │   ├── MasterData.cs      # Product, Supplier, Customer, Warehouse, Account
│   │   ├── Transactions.cs    # Purchase / Sale / Payment / Receipt / StockMovement
│   │   └── Journal.cs         # JournalEntry / JournalLine
│   ├── Data/
│   │   ├── AppDbContext.cs    # EF Core context
│   │   └── SeedData.cs        # 種子資料：12 商品 / 5 客戶 / 4 供應商 / 8 進貨 / 13 銷售
│   ├── Services/
│   │   ├── AccountingService.cs  # WAC 加權平均成本 + 自動借貸分錄
│   │   └── ReportService.cs      # 儀表板、毛利、排行、週轉、試算表
│   ├── Controllers/           # 一個資源一個 controller
│   ├── Dtos/                  # 請求/回應 DTO
│   ├── wwwroot/               # 前端 admin console（fetch 後端 API）
│   │   ├── index.html
│   │   └── assets/{css,js}/
│   ├── Program.cs
│   ├── appsettings.json       # SQL Server 連線字串（預設 sa / InvenFlow#2026）
│   └── Dockerfile
└── docker-compose.yml         # SQL Server + API 一鍵起
```

## 啟動

### 方式 A：docker compose（最簡單，無需安裝 .NET / SQL Server）

```bash
cd server
docker compose up -d
# 等 30 秒讓 SQL Server 啟動 + API migrate + seed
open http://localhost:5080
```

打開瀏覽器：
- **Admin Console**：http://localhost:5080/
- **Swagger API 文件**：http://localhost:5080/swagger（僅 Development 環境）
- **健康檢查**：http://localhost:5080/api/system/health

### 方式 B：在地端用 .NET SDK 跑 + 自備 SQL Server

```bash
cd server/InvenFlow.Api

# 改 appsettings.Development.json 把連線字串指到自己的 SQL Server
dotnet run
# → http://localhost:5080
```

預設 `appsettings.Development.json` 連的是 `(localdb)\\MSSQLLocalDB`，Windows 上有 Visual Studio 即可。

### 方式 C：SQLite 模式（開發便利，無需 SQL Server）

```bash
cd server/InvenFlow.Api
InvenFlow__UseSqlite=true \
  ConnectionStrings__DefaultConnection="Data Source=invenflow.db" \
  dotnet run
```

啟動時會自動建立 `invenflow.db` 並植入種子資料。

## 主要 API

| Method | Path | 說明 |
| --- | --- | --- |
| GET | `/api/products` | 商品清單 |
| POST | `/api/products` | 新增商品 |
| GET | `/api/suppliers` `/api/customers` `/api/warehouses` `/api/accounts` | 主檔 |
| GET | `/api/purchases` | 進貨單列表 |
| POST | `/api/purchases` | 開立進貨 → 自動刷 WAC、入庫、分錄 |
| POST | `/api/purchases/{id}/cancel` | 作廢 → 反向沖銷 |
| GET / POST | `/api/sales` | 銷售（同上邏輯） |
| POST | `/api/sales/{id}/cancel` | 作廢銷售 |
| GET / POST | `/api/payments` `/api/receipts` | 付款 / 收款 |
| GET | `/api/stock-movements` | 庫存異動明細 |
| POST | `/api/stock-movements/adjust` | 盤點調整（盤盈 / 盤虧） |
| GET | `/api/journals` | 會計分錄 |
| GET | `/api/reports/dashboard` | 儀表板指標 |
| GET | `/api/reports/gross-profit?from=&to=` | 毛利分析 |
| GET | `/api/reports/product-ranking?top=10` | 商品排行 |
| GET | `/api/reports/inventory-turnover` | 庫存週轉 |
| GET | `/api/reports/trial-balance?from=&to=` | 試算表（驗證借貸平衡） |
| GET | `/api/system/health` | 健康檢查 |
| POST | `/api/system/reset` | 清空並重新植入種子資料 |

完整 schema 看 Swagger UI。

## 商業邏輯（後端集中）

### 加權平均成本（WAC）

進貨時刷新：
```
new_avg = (old_qty * old_avg + qty * unit_cost) / (old_qty + qty)
```
銷售時鎖定當下平均成本作為 COGS，不影響平均成本。

### 借貸分錄（每筆交易自動產生）

| 事件 | 借 | 貸 |
| --- | --- | --- |
| 進貨（賒帳）| 1310 存貨 | 2100 應付帳款 |
| 進貨（現金）| 1310 存貨 | 1100 現金 |
| 銷售（賒帳）| 1130 應收 + 5100 銷貨成本 | 4100 銷貨收入 + 1310 存貨 |
| 銷售（現金）| 1100 現金 + 5100 銷貨成本 | 4100 銷貨收入 + 1310 存貨 |
| 付款 | 2100 應付 | 1100 現金 |
| 收款 | 1100 現金 | 1130 應收 |
| 盤盈 | 1310 存貨 | 4900 存貨盤盈 |
| 盤虧 | 5900 存貨盤虧 | 1310 存貨 |
| 作廢 | 反向沖銷以上 | 反向沖銷以上 |

「報表分析 → 試算表」即時驗證借貸平衡。

## 連線字串

| 環境 | 來源 | 預設值 |
| --- | --- | --- |
| Production / Docker | `ConnectionStrings__DefaultConnection` 環境變數 | docker-compose 內已配置 |
| Development（本機）| `appsettings.Development.json` | `(localdb)\\MSSQLLocalDB` |
| SQLite | env `InvenFlow__UseSqlite=true` | 由連線字串指定檔名 |

## 開發工具

```bash
dotnet ef migrations add Init    # 建立 migration（預設用 EnsureCreated，可改 migrate）
dotnet build
dotnet run --no-launch-profile --urls http://localhost:5080
```
