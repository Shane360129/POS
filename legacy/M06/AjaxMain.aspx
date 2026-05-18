    <%@ Page Language="C#" %>

<%@ Import Namespace="System.Data" %>
<%@ Assembly Name="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Response.Clear();
            Employee emp = new Employee();
            Stock stock = new Stock();
            ToDouble toDouble = new ToDouble();
            string SqlComm = "";
            DataTable pdTbl, memTbl;
            try
            {
                switch (Request.Form["args0"])
                {
                    #region A00 修改Cookies內容---  args1:Cookies名稱 args2:修改內容
                    case "A00":
                        Global.cookies.Update(Request.Form["args1"], Request.Form["args2"], "");
                        break;
                    #endregion
                    #region A01 員工登入
                    case "A01":
                        Response.Write(emp.LoginInit(Request.Form["EmpId"].ToLower(), Request.Form["EmpPW"].ToLower()) ? "Y" : "N");
                        break;
                    #endregion
                    default:
                        if (emp.IsEmp)
                        {
                            switch (Request.Form["args0"])
                            {
                                #region B00 取資料庫轉JSON string
                                case "B00":
                                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                                    SqlComm = Request.Form["SqlComm"];
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count > 0)
                                    {
                                        Dictionary<string, object> row;
                                        foreach (DataRow dr in pdTbl.Rows)
                                        {
                                            row = new Dictionary<string, object>();
                                            foreach (DataColumn col in pdTbl.Columns)
                                            {
                                                row.Add(col.ColumnName, dr[col]);
                                            }
                                            rows.Add(row);
                                        }
                                    };
                                    Response.Write(serializer.Serialize(rows));
                                    break;
                                #endregion
                                #region B001 取資料庫轉JSON string
                                case "B001":
                                    serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    rows = new List<Dictionary<string, object>>();

                                    SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND payType IN (0,1) AND pvSn='{Request.Form["pvSn"]}' ORDER BY InStkId, sn, dtlSn";
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count > 0)
                                    {
                                        Dictionary<string, object> row;
                                        foreach (DataRow dr in pdTbl.Rows)
                                        {
                                            row = new Dictionary<string, object>();
                                            foreach (DataColumn col in pdTbl.Columns)
                                            {
                                                row.Add(col.ColumnName, dr[col]);
                                            }
                                            rows.Add(row);
                                        }
                                    };
                                    Response.Write(serializer.Serialize(rows));
                                    break;
                                #endregion
                                #region B01 員工登出
                                case "B01":
                                    emp.Logout();
                                    Response.Write("Y");
                                    break;
                                #endregion
                                #region B02 取得分類JSON
                                case "B02":
                                    SqlComm = "SELECT * FROM WP_vPdKind ORDER BY pKLId, pKMId, pKSId";
                                    DataTable PKTbl = Global.getTbl.table("WP", SqlComm);

                                    string JSON = "";
                                    foreach (DataRow row in PKTbl.Rows)
                                    {
                                        JSON += $"{(JSON == "" ? "" : ",")}{{\"pKLId\":\"{row["pKLId"]}\",\"pKLName\":\"{row["pKLName"]}\",\"pKMId\":\"{row["pKMId"]}\",\"pKMName\":\"{row["pKMName"]}\",\"pKSId\":\"{row["pKSId"]}\",\"pKSName\":\"{row["pKSName"]}\"}}";
                                    }
                                    JSON = $"[{JSON}]";
                                    Response.Write(JSON);
                                    break;
                                #endregion
                                #region B03 取廠商商品清單
                                case "B03":
                                    string pvSn = Request.Form["pvSn"],
                                        pdFilter = Request.Form["pdFilter"];

                                    switch (Request.Form["kind"])       //isSale銷售狀態(0:正常進銷貨 / 1:只停止進貨 / 2:只停止銷貨 / 3:停止進銷貨)
                                    {
                                        case "IN":
                                            SqlComm = $"SELECT * FROM WP_Product WHERE isSale IN (0,2) " +
                                                    $"{(pdFilter=="" ? "": $"AND (pNameS LIKE '%{pdFilter}%' OR pName LIKE '%{pdFilter}%' OR pBarcode LIKE '%{pdFilter}%')")} ORDER BY PName";
                                            break;
                                        case "OUT":
                                            SqlComm = $"SELECT * FROM WP_Product WHERE isSale IN (0,1) " +
                                                    $"{(pdFilter=="" ? "": $"AND (pNameS LIKE '%{pdFilter}%' OR pName LIKE '%{pdFilter}%' OR pBarcode LIKE '%{pdFilter}%')")} ORDER BY PName";
                                            break;
                                        case "IO":
                                            SqlComm = $"SELECT  * FROM WP_Product WHERE isSale IN (0,1,2) {(pvSn=="" ? "" : $"AND pvSn='{pvSn}'")} " +
                                                    $"{(pdFilter=="" ? "": $"AND (pNameS LIKE '%{pdFilter}%' OR pName LIKE '%{pdFilter}%' OR pBarcode LIKE '%{pdFilter}%')")} ORDER BY PName";
                                            break;
                                        default:
                                            SqlComm = $"SELECT * FROM WP_Product WHERE isSale NOT IN (3) ORDER BY PName";
                                            break;
                                    }
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    string pdJSON = "", pBarcode = "";
                                    foreach (DataRow row in pdTbl.Rows)
                                    {
                                        pBarcode = row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString();
                                        pdJSON += $"{(pdJSON == "" ? "" : ",")}{{\"pvSn\":\"{row["pvSn"]}\",\"pNo\":\"{row["pNo"]}\",\"pNameS\":\"{row["pNameS"]}\",\"pName\":\"{row["pName"]}\",\"pBarcode\":\"{pBarcode}\",\"qtyNow\":\"{row["qtyNow"]}\",\"isUpdStk\":\"{row["isUpdStock"]}\"}}";
                                    };

                                    if (pdJSON == "") { Response.Write("N"); }
                                    else
                                    {
                                        pdJSON = $"[{pdJSON}]";
                                        Response.Write(pdJSON);
                                    }
                                    break;
                                #endregion
                                #region B03_1 取廠商或商品清單
                                case "B03_1":
                                    string _json, _kind,
                                        json = "";
                                    pBarcode = "";
                                    DataTable jsonTbl;
                                    _json = Request.Form["json"];
                                    _kind = Request.Form["kind"];
                                    if (_json == "pdJson")  //取商品JSON
                                    {
                                        SqlComm = _kind == "IN"
                                            ? "SELECT * FROM WP_Product WHERE isSale IN (0,2) ORDER BY PName"
                                            : _kind == "OUT"
                                                ? "SELECT * FROM WP_Product WHERE isSale IN (0,1) ORDER BY PName"
                                                : "SELECT * FROM WP_Product ORDER BY PName";
                                        jsonTbl = Global.getTbl.table("WP", SqlComm);
                                        json = "";
                                        pBarcode = "";
                                        foreach (DataRow row in jsonTbl.Rows)
                                        {
                                            pBarcode = $"{row["pBarcode"]}" == "" ? $"{row["pCode"]}" : $"{row["pBarcode"]}";
                                            json += $"{(json == "" ? "" : ",")}{{\"pvSn\":\"{row["pvSn"]}\",\"pNo\":\"{row["pNo"]}\",\"pNameS\":\"{row["pNameS"]}\",\"pName\":\"{row["pName"]}\",\"pBarcode\":\"{pBarcode}\",\"qtyNow\":\"{row["qtyNow"]}\"}}";
                                        };
                                    }
                                    else  //取廠商JSON
                                    {
                                        SqlComm = _kind == "ALL"
                                            ? "SELECT * FROM WP_Provider ORDER BY pvId"
                                            : "SELECT * FROM WP_Provider WHERE isStop='N' ORDER BY pvId";
                                        jsonTbl = Global.getTbl.table("WP", SqlComm);
                                        json = "";
                                        foreach (DataRow row in jsonTbl.Rows)
                                        {
                                            json += $"{(json == "" ? "" : ",")}{{\"pvSn\":\"{row["sn"]}\",\"pvId\":\"{row["pvId"]}\",\"pvName\":\"{row["pvName"]}\",\"pvNameS\":\"{row["pvNameS"]}\"}}";
                                        };
                                    }

                                    if (json == "") { Response.Write("N"); }
                                    else
                                    {
                                        json = $"[{json}]";
                                        Response.Write(json);
                                    }
                                    break;
                                #endregion
                                #region B03_2 取會員清單
                                case "B03_2":
                                    string memFilter = Request.Form["memFilter"],
                                        memJSON = "";

                                    SqlComm = $"SELECT * FROM WP_Member WHERE isStop = 'N' " +
                                        $"{(memFilter=="" ? "": $"AND (memId LIKE '%{memFilter}%' OR memName LIKE '%{memFilter}%')")} ORDER BY memName";
                                    memTbl = Global.getTbl.table("WP", SqlComm);
                                    foreach (DataRow row in memTbl.Rows)
                                    {
                                        memJSON += $"{(memJSON == "" ? "" : ",")}{{\"memSn\":\"{row["sn"]}\",\"memId\":\"{row["memId"]}\",\"memName\":\"{row["memName"]}\"}}";
                                    };

                                    if (memJSON == "")
                                        Response.Write("N");
                                    else
                                    {
                                        memJSON = $"[{memJSON}]";
                                        Response.Write(memJSON);
                                    }
                                    break;
                                #endregion
                                #region B04 取商品資料
                                case "B04":
                                    //string pdJSON;
                                    SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo='{Request.Form["pNo"]}'";
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count == 0) { Response.Write("N"); }
                                    else
                                    {
                                        pdJSON = "";
                                        DataRow row0 = pdTbl.Rows[0];
                                        pdJSON = $"[{{" +
                                            $"\"pNo\":\"{row0["pNo"]}\"," +
                                            $"\"pNameS\":\"{row0["pNameS"]}\"," +
                                            $"\"pBarcode\":\"{(row0["pBarcode"].ToString() == "" ? row0["pCode"].ToString() : row0["pBarcode"].ToString())}\"," +
                                            $"\"costStd\":\"{row0["costStd"]}\"," +
                                            $"\"pUName\":\"{row0["pUName"]}\"," +
                                            $"\"isTax\":\"{row0["isTax"]}\"," +
                                            $"\"priceStd\":\"{row0["priceStd"]}\"," +
                                            $"\"priceLow\":\"{row0["priceLow"]}\"," +
                                            $"\"priceMem\":\"{row0["priceMem"]}\"," +
                                            $"\"priceBat\":\"{row0["priceBat"]}\"," +
                                            $"\"priceBad\":\"{row0["priceBad"]}\"," +
                                            $"\"pvSn\":\"{row0["pvSn"]}\"," +
                                            $"\"pvNameS\":\"{($"{row0["pvNameS"]}" == "" ? $"{row0["pvName"]}" : $"{row0["pvNameS"]}")}\"" +                                        $"}}]";
                                        Response.Write(pdJSON);
                                    };
                                    break;
                                #endregion
                                default:
                                    if (emp.EmpIsAdmin == "N") { Response.Write("UA"); }    //非管理者不能變更
                                    else
                                    {
                                        switch (Request.Form["args0"])
                                        {
                                            #region C01 商品類別新增
                                            case "C01":
                                                switch (Request.Form["pkind"])
                                                {
                                                    case "pkL": //主分類
                                                        SqlComm = $"SELECT * FROM WP_PdKindL WHERE pKLId='{Request.Form["pkLId"]}'";
                                                        DataTable pKLTbl = Global.getTbl.table("WP", SqlComm);
                                                        if (pKLTbl.Rows.Count == 0)
                                                        {
                                                            SqlComm = $"INSERT INTO WP_PdKindL (pKLId, pKLName) VALUES ('{Request.Form["pkLId"]}', N'{Request.Form["pkLName"]}')";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                            Response.Write("Y");
                                                        }
                                                        else
                                                        {
                                                            DataRow row = pKLTbl.Rows[0];
                                                            if (row["pKLExist"].ToString() == "Y") { Response.Write("A"); }    //已存在該分類編號
                                                            else
                                                            {
                                                                SqlComm = $"UPDATE WP_PdKindL SET pKLName=N'{Request.Form["pkLName"]}', pKLExist='Y' WHERE pKLId='{row["pkLId"]}'";
                                                                Global.getTbl.updTbl("WP", SqlComm);
                                                                Response.Write("Y");
                                                            }
                                                        }
                                                        break;
                                                    case "pkM": //次分類
                                                        SqlComm = $"SELECT * FROM WP_PdKindM WHERE pKLId='{Request.Form["pkLId"]}' AND pKMId='{Request.Form["pkLId"]}{Request.Form["pkMId"]}'";
                                                        DataTable pKMTbl = Global.getTbl.table("WP", SqlComm);
                                                        if (pKMTbl.Rows.Count == 0)
                                                        {
                                                            SqlComm = $"INSERT INTO WP_PdKindM (pKLId, pKMId, pKMName) VALUES ('{Request.Form["pkLId"]}', '{Request.Form["pkLId"]}{Request.Form["pkMId"]}', N'{Request.Form["pKMName"]}')";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                            Response.Write("Y");
                                                        }
                                                        else
                                                        {
                                                            DataRow row = pKMTbl.Rows[0];
                                                            if (row["pKMExist"].ToString() == "Y") { Response.Write("A"); }    //已存在該分類編號
                                                            else
                                                            {
                                                                SqlComm = $"UPDATE WP_PdKindM SET pKMName=N'{Request.Form["pkMName"]}', pKMExist='Y' WHERE sn='{row["sn"]}'";
                                                                Global.getTbl.updTbl("WP", SqlComm);
                                                                Response.Write("Y");
                                                            }
                                                        }
                                                        break;
                                                    case "pkS": //小分類
                                                        SqlComm = $"SELECT * FROM WP_PdKindS WHERE pKLId='{Request.Form["pkLId"]}' AND pKMId='{Request.Form["pkMId"]}' AND pKSId='{Request.Form["pkMId"]}{Request.Form["pkSId"]}'";
                                                        DataTable pKSTbl = Global.getTbl.table("WP", SqlComm);
                                                        if (pKSTbl.Rows.Count == 0)
                                                        {
                                                            SqlComm = $"INSERT INTO WP_PdKindS (pKLId, pKMId, pKSId, pKSName) VALUES ('{Request.Form["pkLId"]}', '{Request.Form["pkMId"]}', '{Request.Form["pkMId"]}{Request.Form["pkSId"]}', N'{Request.Form["pKSName"]}')";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                            Response.Write("Y");
                                                        }
                                                        else
                                                        {
                                                            DataRow row = pKSTbl.Rows[0];
                                                            if (row["pKSExist"].ToString() == "Y") { Response.Write("A"); }    //已存在該分類編號
                                                            else
                                                            {
                                                                SqlComm = $"UPDATE WP_PdKindS SET pKSName=N'{Request.Form["pkSName"]}', pKSExist='Y' WHERE sn='{row["sn"]}'";
                                                                Global.getTbl.updTbl("WP", SqlComm);
                                                                Response.Write("Y");
                                                            }
                                                        }
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C01A 商品分類修改刪除
                                            case "C01A":
                                                string pKind = Request.Form["pKind"];
                                                //string updTbl = pKind == "pKL" ? "WP_PdKindL" : pKind == "pKM" ? "WP_PdKindM" : "";
                                                switch (Request.Form["updKind"])
                                                {
                                                    case "submit": //修改
                                                        SqlComm = $"UPDATE WP_PdKind{pKind} SET pK{pKind}Name=N'{Request.Form["pKName"]}' WHERE pK{pKind}Id='{Request.Form["pKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                    case "del": //刪除
                                                        string correct = "Y";
                                                        if (pKind != "S")
                                                        {
                                                            switch (pKind)
                                                            {
                                                                case "L":
                                                                    SqlComm = $"SELECT * FROM WP_PdKindM WHERE pKLId='{Request.Form["pKId"]}' AND pKMExist='Y'";
                                                                    break;
                                                                case "M":
                                                                    SqlComm = $"SELECT * FROM WP_PdKindS WHERE pKMId='{Request.Form["pKId"]}' AND pKSExist='Y'";
                                                                    break;
                                                                default:
                                                                    correct = "N";
                                                                    break;
                                                            }
                                                            if (correct == "Y")
                                                            {
                                                                DataTable pKDtlTbl = Global.getTbl.table("WP", SqlComm);
                                                                correct = pKDtlTbl.Rows.Count == 0 ? "Y" : "A";
                                                            }
                                                        }

                                                        if (correct == "Y")
                                                        {
                                                            SqlComm = $"UPDATE WP_PdKind{pKind} SET pK{pKind}Exist='N' WHERE pK{pKind}Id='{Request.Form["pKId"]}'";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                            Response.Write("Y");
                                                        }
                                                        else
                                                        {
                                                            Response.Write(correct);
                                                        }
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C02 廠商分類新增
                                            case "C02":
                                                SqlComm = $"SELECT * FROM WP_PvKind WHERE pvKId='{Request.Form["pvKId"]}'";
                                                DataTable pvKTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pvKTbl.Rows.Count == 0)
                                                {
                                                    SqlComm = $"INSERT INTO WP_PvKind (pvKId, pvKName) VALUES ('{Request.Form["pvKId"]}', N'{Request.Form["pvKName"]}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                    Response.Write("Y");
                                                }
                                                else
                                                {
                                                    DataRow row = pvKTbl.Rows[0];
                                                    if (row["pvExist"].ToString() == "Y") { Response.Write("A"); }    //已存在該分類編號
                                                    else
                                                    {
                                                        SqlComm = $"UPDATE WP_PvKind SET pvKName=N'{Request.Form["pvKName"]}', pvExist='Y' WHERE pvKId='{row["pvKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                    }
                                                }
                                                break;
                                            #endregion
                                            #region C02A 廠商分類修改刪除
                                            case "C02A":
                                                switch (Request.Form["updKind"])
                                                {
                                                    case "submit": //修改
                                                        SqlComm = $"UPDATE WP_PvKind SET pvKName=N'{Request.Form["pvKName"]}' WHERE pvKId='{Request.Form["pvKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                    case "del": //刪除
                                                        SqlComm = $"UPDATE WP_PvKind SET pvExist='N' WHERE pvKId='{Request.Form["pvKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C03 會員分類新增
                                            case "C03":
                                                SqlComm = $"SELECT * FROM WP_MemKind WHERE memKId='{Request.Form["memKId"]}'";
                                                DataTable memKTbl = Global.getTbl.table("WP", SqlComm);
                                                if (memKTbl.Rows.Count == 0)
                                                {
                                                    SqlComm = $"INSERT INTO WP_MemKind (memKId, memKName) VALUES ('{Request.Form["memKId"]}', N'{Request.Form["memKName"]}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                    Response.Write("Y");
                                                }
                                                else
                                                {
                                                    DataRow row = memKTbl.Rows[0];
                                                    if (row["memKExist"].ToString() == "Y") { Response.Write("A"); }    //已存在該分類編號
                                                    else
                                                    {
                                                        SqlComm = $"UPDATE WP_MemKind SET memKName=N'{Request.Form["memKName"]}', memKExist='Y' WHERE memKId='{row["memKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                    }
                                                }
                                                break;
                                            #endregion
                                            #region C03A 會員分類修改刪除
                                            case "C03A":
                                                switch (Request.Form["updKind"])
                                                {
                                                    case "submit": //修改
                                                        SqlComm = $"UPDATE WP_MemKind SET memKName=N'{Request.Form["memKName"]}' WHERE memKId='{Request.Form["memKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                    case "del": //刪除
                                                        SqlComm = $"UPDATE WP_MemKind SET memKExist='N' WHERE memKId='{Request.Form["memKId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C04 商品單位新增
                                            case "C04":
                                                SqlComm = $"SELECT * FROM WP_PdUnit WHERE pUExist='Y' AND pUName='{Request.Form["pUName"]}'";
                                                DataTable pUNameTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pUNameTbl.Rows.Count > 0) { Response.Write("A"); }
                                                else
                                                {
                                                    SqlComm = $"SELECT * FROM WP_PdUnit WHERE pUExist='N'";
                                                    DataTable pUTbl = Global.getTbl.table("WP", SqlComm);
                                                    if (pUTbl.Rows.Count == 0)
                                                    {
                                                        SqlComm = $"INSERT INTO WP_PdUnit (pUName) VALUES (N'{Request.Form["pUName"]}')";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                    }
                                                    else
                                                    {
                                                        DataRow row = pUTbl.Rows[0];
                                                        SqlComm = $"UPDATE WP_PdUnit SET pUName=N'{Request.Form["pUName"]}', pUExist='Y' WHERE pUId='{row["pUId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                    }
                                                }
                                                break;
                                            #endregion
                                            #region C04A 商品單位修改刪除
                                            case "C04A":
                                                switch (Request.Form["updKind"])
                                                {
                                                    case "submit": //修改
                                                        SqlComm = $"UPDATE WP_PdUnit SET pUName=N'{Request.Form["pUName"]}' WHERE pUId='{Request.Form["pUId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                    case "del": //刪除
                                                        SqlComm = $"UPDATE WP_PdUnit SET pUExist='N' WHERE pUId='{Request.Form["pUId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C05 商品新增
                                            case "C05":
                                                SqlComm = $"SELECT * FROM WP_Product ";
                                                SqlComm += Request.Form["pBarcode"] != ""
                                                        ? $"WHERE pBarcode='{Request.Form["pBarcode"]}' " +
                                                            $"{(Request.Form["pCode"] != "" ? $"OR pCode='{Request.Form["pCode"]}'" : "")}"
                                                        : $"{(Request.Form["pCode"] != "" ? $"WHERE pCode='{Request.Form["pCode"]}'" : "")}";

                                                pdTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pdTbl.Rows.Count == 0)
                                                {
                                                    //新增產品只輸入期初成本及期初庫存，標準成本及平均成本為期初成本，目前庫也為期初庫存
                                                    string cost = Request.Form["costInitial"],
                                                        qty = Request.Form["qtyInitial"];

                                                    SqlComm = "INSERT INTO WP_Product " +
                                                            "(pName, pNameS, pBarcode, pCode, pUnit, priceStd, " +
                                                                "priceLow, priceMem, priceBat, isUpdStock, " +
                                                                "isSale, isTax, costStd, costAvg, costInitial, " +
                                                                "pvSn, qtyNow, qtyInitial) " +
                                                            $"VALUES (N'{Request.Form["pName"]}', N'{Request.Form["pNameS"]}', '{Request.Form["pBarcode"]}', '{Request.Form["pCode"]}', '{Request.Form["pUnit"]}', '{Request.Form["priceStd"]}', " +
                                                                $"'{Request.Form["priceLow"]}', '{Request.Form["priceMem"]}', '{Request.Form["priceBat"]}', '{Request.Form["isUpdStock"]}', " +
                                                                $"'{Request.Form["isSale"]}', '{Request.Form["isTax"]}', '{cost}', '{cost}', '{cost}', " +
                                                                $"'{Request.Form["pvSn"]}', '{qty}', '{qty}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);

                                                    SqlComm = "SELECT Max(pNo) AS NewPNo FROM WP_Product";
                                                    DataTable newPdTbl = Global.getTbl.table("WP", SqlComm);

                                                    string newPNo = newPdTbl.Rows[0]["NewPNo"].ToString();
                                                    //新增商品分類
                                                    updPKind(newPNo, Request.Form["PKJson"]);

                                                    Response.Write("Y");
                                                }
                                                else
                                                {
                                                    DataRow row0 = pdTbl.Rows[0];
                                                    Response.Write($"{(row0["pBarcode"].ToString() != "" && (row0["pBarcode"].ToString() == Request.Form["pBarcode"]) ? "B" : "C")}");  //B:已存在國際編號 / C:已存在店內編號
                                                }
                                                break;
                                            #endregion
                                            #region C05A 商品修改
                                            case "C05A":
                                                string PNo = Request.Form["pNo"].ToString();

                                                SqlComm = $"SELECT * FROM WP_Product WHERE pNo<>'{PNo}' ";
                                                SqlComm += Request.Form["pBarcode"] != ""
                                                        ? $"AND ( pBarcode='{Request.Form["pBarcode"]}' " +
                                                            $"{(Request.Form["pCode"] != "" ? $"OR pCode='{Request.Form["pCode"]}')" : ")")}"
                                                        : $"{(Request.Form["pCode"] != "" ? $"AND pCode='{Request.Form["pCode"]}'" : "")}";

                                                pdTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pdTbl.Rows.Count == 0)
                                                {
                                                    SqlComm = $"SELECT * FROM WP_Product WHERE pNo='{PNo}' ";
                                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                                    DataRow pdRow0 = pdTbl.Rows[0];

                                                    insertPdPriceUpd(emp.EmpId, pdRow0, int.Parse(Request.Form["priceStd"]), int.Parse(Request.Form["priceLow"]), int.Parse(Request.Form["priceMem"]), int.Parse(Request.Form["priceBat"]));       //寫入價格異動檔

                                                    SqlComm = $"UPDATE WP_Product SET " +
                                                            $"pName=N'{Request.Form["pName"]}', pNameS=N'{Request.Form["pNameS"]}', pBarcode='{Request.Form["pBarcode"]}', pCode='{Request.Form["pCode"]}', pUnit='{Request.Form["pUnit"]}', priceStd='{Request.Form["priceStd"]}', " +
                                                                $"priceLow='{(Request.Form["priceLow"] == "" ? Request.Form["priceStd"] : Request.Form["priceLow"])}', " +
                                                                $"priceMem='{(Request.Form["priceMem"] == "" ? Request.Form["priceStd"] : Request.Form["priceMem"])}', " +
                                                                $"priceBat='{(Request.Form["priceBat"] == "" ? Request.Form["priceStd"] : Request.Form["priceBat"])}', " +
                                                            $"isUpdStock='{Request.Form["isUpdStock"]}', " +
                                                            $"isSale='{Request.Form["isSale"]}', isTax='{Request.Form["isTax"]}', costStd='{Request.Form["costStd"]}', " +
                                                                $"costAvg='{(Request.Form["costAvg"] == "" ? Request.Form["costStd"] : Request.Form["costAvg"])}', " +
                                                                $"costInitial='{(Request.Form["costInitial"] == "" ? Request.Form["costStd"] : Request.Form["costInitial"])}', " +
                                                            $"pvSn='{Request.Form["pvSn"]}', " +
                                                            $"qtyNow = '{Request.Form["qtyNow"]}', qtyInitial = '{Request.Form["qtyInitial"]}'" +
                                                            $"WHERE pNo='{PNo}'";
                                                    Global.getTbl.updTbl("WP", SqlComm);

                                                    //更新商品分類
                                                    updPKind(PNo, Request.Form["PKJson"]);
                                                    Response.Write("Y");
                                                }
                                                else
                                                {
                                                    DataRow row0 = pdTbl.Rows[0];
                                                    Response.Write($"{(row0["pBarcode"].ToString() != "" && (row0["pBarcode"].ToString() == Request.Form["pBarcode"]) ? "B" : "C")}");  //B:已存在國際編號 / C:已存在店內編號
                                                }
                                                break;
                                            #endregion
                                            #region C06 廠商新增
                                            case "C06":
                                                SqlComm = $"SELECT * FROM WP_Provider WHERE pvId='{Request.Form["pvId"]}'";
                                                DataTable pvTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pvTbl.Rows.Count > 0) { Response.Write("A"); }
                                                else
                                                {
                                                    SqlComm = $"INSERT INTO WP_Provider " +
                                                            $"(pvId, pvName, pvNameS, pvKId, pvBoss, pvTel, " +
                                                            $"pvCityId, pvZoneId, pvAddr, ctactName, ctactTel, " +
                                                            $"ctactCityId, ctactZoneId, ctactAddr, fax, email, " +
                                                            $"taxId, isStop, invoTitle, bankId, bankName, " +
                                                            $"bankAccount, bankAcctName, memo, sortId) " +
                                                            $"VALUES ('{Request.Form["pvId"]}', N'{Request.Form["pvName"]}', N'{Request.Form["pvNameS"]}', '{Request.Form["pvKId"]}', N'{Request.Form["pvBoss"]}', '{Request.Form["pvTel"]}', " +
                                                            $"'{Request.Form["pvCityId"]}', '{Request.Form["pvZoneId"]}', N'{Request.Form["pvAddr"]}', N'{Request.Form["ctactName"]}', '{Request.Form["ctactTel"]}', " +
                                                            $"'{Request.Form["ctactCityId"]}', '{Request.Form["ctactZoneId"]}', N'{Request.Form["ctactAddr"]}', '{Request.Form["fax"]}', '{Request.Form["email"]}', " +
                                                            $"'{Request.Form["taxId"]}', '{Request.Form["isStop"]}', N'{Request.Form["invoTitle"]}', '{Request.Form["bankId"]}', N'{Request.Form["bankName"]}', " +
                                                            $"'{Request.Form["bankAccount"]}', N'{Request.Form["bankAcctName"]}', N'{Request.Form["memo"]}', '{Request.Form["sortId"]}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);

                                                    Response.Write("Y");
                                                }
                                                break;
                                            #endregion
                                            #region C06A 廠商修改
                                            case "C06A":
                                                string sn = Request.Form["sn"].ToString();

                                                SqlComm = $"SELECT * FROM WP_vProvider WHERE sn<>'{sn}' AND pvId='{Request.Form["pvId"]}'";
                                                pvTbl = Global.getTbl.table("WP", SqlComm);
                                                if (pvTbl.Rows.Count > 0) { Response.Write("A"); }
                                                else
                                                {
                                                    SqlComm = $"UPDATE WP_Provider SET " +
                                                            $"pvId=N'{Request.Form["pvId"]}', pvName=N'{Request.Form["pvName"]}', pvNameS=N'{Request.Form["pvNameS"]}', pvKId='{Request.Form["pvKId"]}', pvBoss=N'{Request.Form["pvBoss"]}', pvTel='{Request.Form["pvTel"]}', " +
                                                            $"pvCityId='{Request.Form["pvCityId"]}', pvZoneId='{Request.Form["pvZoneId"]}', pvAddr=N'{Request.Form["pvAddr"]}', ctactName=N'{Request.Form["ctactName"]}', ctactTel='{Request.Form["ctactTel"]}', " +
                                                            $"ctactCityId='{Request.Form["ctactCityId"]}', ctactZoneId='{Request.Form["ctactZoneId"]}', ctactAddr=N'{Request.Form["ctactAddr"]}', fax='{Request.Form["fax"]}', email='{Request.Form["email"]}', " +
                                                            $"taxId='{Request.Form["taxId"]}', isStop='{Request.Form["isStop"]}', invoTitle=N'{Request.Form["invoTitle"]}', bankId='{Request.Form["bankId"]}', bankName=N'{Request.Form["bankName"]}', " +
                                                            $"bankAccount='{Request.Form["bankAccount"]}', bankAcctName=N'{Request.Form["bankAcctName"]}', memo=N'{Request.Form["memo"]}', sortId='{Request.Form["sortId"]}' " +
                                                            $"WHERE sn='{sn}'";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                    Response.Write("Y");
                                                }
                                                break;
                                            #endregion
                                            #region C07 會員新增
                                            case "C07":
                                                SqlComm = $"SELECT * FROM WP_Member WHERE memId='{Request.Form["memId"]}'";
                                                memTbl = Global.getTbl.table("WP", SqlComm);
                                                if (memTbl.Rows.Count > 0) { Response.Write("A"); }
                                                else
                                                {
                                                    string ordCityId = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memCityId"] : Request.Form["ordCityId"],
                                                        ordZoneId = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memZoneId"] : Request.Form["ordZoneId"],
                                                        ordAddr = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memAddr"] : Request.Form["ordAddr"],
                                                        birthday = Request.Form["birthday"] == "" ? "1911-01-01" : Request.Form["birthday"];

                                                    SqlComm = $"INSERT INTO WP_Member " +
                                                            $"(memId, memName, idNo, memKId, birthday, " +
                                                            $"isStop, priceKind, gender, memTel, memMobil, " +
                                                            $"memFax, memEmail, memCityId, memZoneId, memAddr, " +
                                                            $"dittoMemAddr, ordCityId, ordZoneId, ordAddr, memo, " +
                                                            $"bankId, bankName, bankAccount, bankAcctName) " +
                                                            $"VALUES ('{Request.Form["memId"]}', N'{Request.Form["memName"]}', '{Request.Form["idNo"]}', '{Request.Form["memKId"]}', '{birthday}', " +
                                                            $"'{Request.Form["isStop"]}', '{Request.Form["priceKind"]}', '{Request.Form["gender"]}', '{Request.Form["memTel"]}', '{Request.Form["memMobil"]}', " +
                                                            $"'{Request.Form["memFax"]}', '{Request.Form["memEmail"]}', '{Request.Form["memCityId"]}', '{Request.Form["memZoneId"]}', N'{Request.Form["memAddr"]}', " +
                                                            $"'{Request.Form["dittoMemAddr"]}', '{ordCityId}', '{ordZoneId}', N'{ordAddr}', N'{Request.Form["memo"]}', " +
                                                            $"'{Request.Form["bankId"]}', N'{Request.Form["bankName"]}', '{Request.Form["bankAccount"]}', N'{Request.Form["bankAcctName"]}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);

                                                    Response.Write("Y");
                                                }
                                                break;
                                            #endregion
                                            #region C07A 會員修改
                                            case "C07A":
                                                sn = Request.Form["sn"].ToString();
                                                SqlComm = $"SELECT * FROM WP_Member WHERE sn<>'{sn}' AND memId='{Request.Form["memId"]}'";
                                                memTbl = Global.getTbl.table("WP", SqlComm);
                                                if (memTbl.Rows.Count > 0) { Response.Write("A"); }
                                                else
                                                {
                                                    string ordCityId = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memCityId"] : Request.Form["ordCityId"],
                                                        ordZoneId = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memZoneId"] : Request.Form["ordZoneId"],
                                                        ordAddr = Request.Form["dittoMemAddr"] == "Y" ? Request.Form["memAddr"] : Request.Form["ordAddr"],
                                                        birthday = Request.Form["birthday"] == "" ? "1911-01-01" : Request.Form["birthday"];

                                                    SqlComm = $"UPDATE WP_Member SET " +
                                                            $"memId='{Request.Form["memId"]}', memName=N'{Request.Form["memName"]}', idNo='{Request.Form["idNo"]}', memKId='{Request.Form["memKId"]}', birthday='{birthday}', " +
                                                            $"isStop='{Request.Form["isStop"]}', priceKind='{Request.Form["priceKind"]}', gender='{Request.Form["gender"]}', memTel='{Request.Form["memTel"]}', memMobil='{Request.Form["memMobil"]}', " +
                                                            $"memFax='{Request.Form["memFax"]}', memEmail='{Request.Form["memEmail"]}', memCityId='{Request.Form["memCityId"]}', memZoneId='{Request.Form["memZoneId"]}', memAddr=N'{Request.Form["memAddr"]}', " +
                                                            $"dittoMemAddr='{Request.Form["dittoMemAddr"]}', ordCityId='{ordCityId}', ordZoneId='{ordZoneId}', ordAddr=N'{ordAddr}', memo=N'{Request.Form["memo"]}', " +
                                                            $"bankId='{Request.Form["bankId"]}', bankName=N'{Request.Form["bankName"]}', bankAccount='{Request.Form["bankAccount"]}', bankAcctName=N'{Request.Form["bankAcctName"]}' " +
                                                            $"WHERE sn='{sn}'";
                                                    Global.getTbl.updTbl("WP", SqlComm);

                                                    Response.Write("Y");
                                                }
                                                break;
                                            #endregion
                                            #region C08 登入員工新增
                                            case "C08":
                                                SqlComm = $"INSERT INTO WP_Employee " +
                                                        $"(empId, empPw, empName, empGrpId, isStop, memo, isAdmin) " +
                                                        $"VALUES ('{Request.Form["empId"]}', '{Request.Form["empPw"]}', N'{Request.Form["empName"]}', '{Request.Form["empGrpId"]}','{Request.Form["isStop"]}', N'{Request.Form["memo"]}', '{Request.Form["isAdmin"]}')";
                                                Global.getTbl.updTbl("WP", SqlComm);

                                                Response.Write("Y");
                                                break;
                                            #endregion
                                            #region C08A 登入員工修改
                                            case "C08A":
                                                sn = Request.Form["sn"].ToString();
                                                SqlComm = $"UPDATE WP_Employee SET " +
                                                        $"empId='{Request.Form["empId"]}', empName=N'{Request.Form["empName"]}', empGrpId='{Request.Form["empGrpId"]}', isStop='{Request.Form["isStop"]}', memo=N'{Request.Form["memo"]}', isAdmin='{Request.Form["isAdmin"]}' " +
                                                        //$"empId='{Request.Form["empId"]}', empPw='{Request.Form["empPw"]}', empName=N'{Request.Form["empName"]}', empGrpId='{Request.Form["empGrpId"]}', isStop='{Request.Form["isStop"]}', memo=N'{Request.Form["memo"]}', isAdmin='{Request.Form["isAdmin"]}' " +
                                                        $"WHERE sn='{sn}'";
                                                Global.getTbl.updTbl("WP", SqlComm);

                                                Response.Write("Y");
                                                break;
                                            #endregion
                                            #region C09 角色群組新增
                                            case "C09":
                                                SqlComm = $"SELECT * FROM WP_EmpGrp WHERE empGrpId='{Request.Form["empGrpId"]}'";
                                                DataTable empGrpTbl = Global.getTbl.table("WP", SqlComm);
                                                if (empGrpTbl.Rows.Count == 0)
                                                {
                                                    SqlComm = $"INSERT INTO WP_EmpGrp (empGrpId, empGrpName) VALUES ('{Request.Form["empGrpId"]}', N'{Request.Form["empGrpName"]}')";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                    Response.Write("Y");
                                                }
                                                else
                                                {
                                                    DataRow row = empGrpTbl.Rows[0];
                                                    if (row["isStop"].ToString() == "N") { Response.Write("A"); }    //已存在該角色群組
                                                    else
                                                    {
                                                        SqlComm = $"UPDATE WP_EmpGrp SET empGrpName=N'{Request.Form["empGrpName"]}', isStop='N' WHERE empGrpId='{row["empGrpId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                    }
                                                }
                                                break;
                                            #endregion
                                            #region C09A 角色群組修改刪除
                                            case "C09A":
                                                switch (Request.Form["updKind"])
                                                {
                                                    case "submit": //修改
                                                        SqlComm = $"UPDATE WP_EmpGrp SET empGrpName=N'{Request.Form["empGrpName"]}' WHERE empGrpId='{Request.Form["empGrpId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                    case "del": //刪除
                                                        SqlComm = $"UPDATE WP_EmpGrp SET isStop='Y' WHERE empGrpId='{Request.Form["empGrpId"]}'";
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                        Response.Write("Y");
                                                        break;
                                                }
                                                break;
                                            #endregion
                                            #region C10 角色群組權限變更
                                            case "C10":
                                                SqlComm = $"UPDATE WP_EmpGrp SET empPrgIdGrp=N'{Request.Form["empPrgIdGrp"]}' WHERE empGrpId='{Request.Form["empGrpId"]}'";
                                                Global.getTbl.updTbl("WP", SqlComm);
                                                Response.Write("Y");
                                                break;
                                                #endregion
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            Response.Write("not-emp");
                        }
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                Response.Write(ex.Message);
            }
            Response.End();
        }

    }

    /// <summary>
    /// 更新商品分類
    /// </summary>
    /// <param name="_PNo">商品編號</param>
    /// <param name="_PKJson">商品類別資料</param>
    private void updPKind(string _PNo, string _PKJson)
    {
        string SqlComm = $"UPDATE WP_PdKindProd SET pKExist='N' WHERE pNo={_PNo}";
        Global.getTbl.updTbl("WP", SqlComm);

        SqlComm = $"SELECT * FROM WP_PdKindProd WHERE pKExist='N'";
        DataTable PKPdTbl = Global.getTbl.table("WP", SqlComm);
        int PKPRecNo = 0;

        JArray json = JArray.Parse(_PKJson);

        string PKMId, PKSId;
        foreach (DataRow row in PKPdTbl.Rows)
        {
            if (PKPRecNo < json.Count)
            {
                PKMId = json[PKPRecNo]["PKM"].ToString() == "null" ? "0" : json[PKPRecNo]["PKM"].ToString();
                PKSId = json[PKPRecNo]["PKS"].ToString() == "null" ? "0" : json[PKPRecNo]["PKS"].ToString();
                SqlComm = $"UPDATE WP_PdKindProd SET pNo={_PNo}, pKLId='{json[PKPRecNo]["PKL"]}', pKMId='{PKMId}', pKSId='{PKSId}', pKExist='Y' WHERE sn={row["sn"]}";
                Global.getTbl.updTbl("WP", SqlComm);
                PKPRecNo++;
            }
            else
            {
                break;
            }

        }

        for (int i = PKPRecNo; i < json.Count; i++)
        {
            PKMId = json[i]["PKM"].ToString() == "null" ? "0" : json[i]["PKM"].ToString();
            PKSId = json[i]["PKS"].ToString() == "null" ? "0" : json[i]["PKS"].ToString();
            SqlComm = $"INSERT INTO WP_PdKindProd ( pNo, pKLId, pKMId, pKSId, pKExist) VALUES ({_PNo}, '{json[i]["PKL"]}', '{PKMId}', '{PKSId}', 'Y')";
            Global.getTbl.updTbl("WP", SqlComm);
        }
    }

    /// <summary>
    /// 寫入價格異動表
    /// </summary>
    /// <param name="_empId"></param>員工代碼
    /// <param name="_pdRow"></param>商品資料
    /// <param name="_priceStd"></param>異動後標準售價
    /// <param name="_priceLow"></param>異動後最低應售價
    /// <param name="_priceMem"></param>異動後會員價
    /// <param name="_priceBat"></param>異動後大批價
    private void insertPdPriceUpd(string _empId, DataRow _pdRow, int _priceStd, int _priceLow, int _priceMem, int _priceBat)
    {
        if (int.Parse(_pdRow["priceStd"].ToString()) != _priceStd || int.Parse(_pdRow["priceLow"].ToString()) != _priceLow || int.Parse(_pdRow["priceMem"].ToString()) != _priceMem || int.Parse(_pdRow["priceBat"].ToString()) != _priceBat)
        {
            string SqlComm = $"SELECT * FROM WP_pdPriceUpd WHERE PNo='{_pdRow["PNo"]}'";
            DataTable pdTbl = Global.getTbl.table("WP", SqlComm);
            if (pdTbl.Rows.Count == 0)      //無異動過，先將原始的價格寫入
            {
                SqlComm = $"INSERT INTO WP_pdPriceUpd (pNo, empId, priceStd, priceLow, priceMem, priceBat) " +
                        $"VALUES ('{_pdRow["PNo"]}', '{_empId}', '{_pdRow["priceStd"]}', '{_pdRow["priceLow"]}', '{_pdRow["priceMem"]}', '{_pdRow["priceBat"]}')";
                Global.getTbl.updTbl("WP", SqlComm);

            }
            SqlComm = $"INSERT INTO WP_pdPriceUpd (pNo, empId, priceStd, priceLow, priceMem, priceBat) " +
                    $"VALUES ('{_pdRow["PNo"]}', '{_empId}', '{_priceStd}', '{_priceLow}', '{_priceMem}', '{_priceBat}')";
            Global.getTbl.updTbl("WP", SqlComm);
        }
    }

    /// <summary>
    /// 取得最後進貨成本
    /// </summary>
    /// <param name="_pNo"></param>商品編號
    /// <param name="_pdCostInitl"></param>商品期初成本
    private double pdCostStd(string _pNo, double _pdCostInitl)
    {
        string SqlComm = $"SELECT TOP(1) * FROM WP_InStockDtl WHERE pNo = '{_pNo}' AND isDel='N' ORDER BY InStkId DESC, Sn DESC";
        DataTable lastInStkAmtTbl = Global.getTbl.table("WP", SqlComm);
        return lastInStkAmtTbl.Rows.Count == 0 ? _pdCostInitl : double.Parse($"{lastInStkAmtTbl.Rows[0]["amount"]}");

    }

</script>

