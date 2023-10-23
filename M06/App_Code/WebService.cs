using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using static Global;

/// <summary>
/// WebService-POS系統前台用
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{
    //Log
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    //南農POS 正式token
    private string token = "D2esIufCHocHKcYXJ496K5JRAbLSc7Om1WTICzCf8iL";
    //測試token
    //private string token = "xOOrsJ8yxBbKQZfCItedo3GNIq15q1syaSppLRKHEQd";

    public WebService()
    {
        //如果使用設計的元件，請取消註解下列一行
        //InitializeComponent(); 
        XmlConfigurator.Configure(new System.IO.FileInfo(Server.MapPath("~/log4net.config")));
    }

    /// <summary>
    /// 檢核驗證資料是否正確
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    public bool ChkCode(string HiddCode)
    {
        string YMD = Global.PosKey + DateTime.Now.ToString("yyyyMMdd");
        if (HiddCode == stringEncrypt.getMd5Method(YMD))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 檢核驗證資料是否正確
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    public bool ChkCode2(string HiddCode)
    {
        string YMD = Global.PosKey;
        if (HiddCode == stringEncrypt.getMd5Method(YMD))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 新增POS銷售檔
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <param name="dtOutStockPos"></param>
    /// <param name="dtOutStockDtlPos"></param>
    /// <param name="Sdate"></param>
    /// <param name="Edate"></param>
    /// <param name="Machine"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable IntoOutStockPos(string HiddCode, DataTable dtOutStockPos, DataTable dtOutStockDtlPos, string Sdate, string Edate, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        Log.Info($"上傳銷售日期:{Sdate}~{Edate} 取得銷售主檔筆數:{dtOutStockPos.Rows.Count} 取得銷售副檔筆數:{dtOutStockDtlPos.Rows.Count}");

        DataTable dtResult = new DataTable();
        dtResult.Clear();
        dtResult.TableName = "dtResult";

        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dtResult;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dtResult;
        }
        //銷售主檔寫入成功筆數
        int MainResult = 0;
        //銷售副檔寫入成功筆數
        int MastResult = 0;
        //有筆數時再執行
        if (dtOutStockPos.Rows.Count > 0 && dtOutStockDtlPos.Rows.Count > 0)
        {
            //刪除POS銷售主檔
            string SqlComm = $"DELETE FROM WP_OutStockPos WHERE LEFT(OutStkId,8)>='{Sdate}' AND LEFT(OutStkId,8)<='{Edate}' AND machId='{MchId}'";
            Log.Info($"刪除POS銷售主檔:{SqlComm}");
            Global.getTbl.updTbl("WP", SqlComm);

            //支付清單
            List<PAY> PayList;
            PAY pay;
            //寫入POS銷售主檔
            foreach (DataRow dr in dtOutStockPos.Rows)
            {
                PayList = new List<PAY>();
                try
                {
                    //日期民國轉西元
                    string sampleDate = dr["SAL_DATE"].ToString();
                    CultureInfo culture = new CultureInfo("zh-TW");
                    culture.DateTimeFormat.Calendar = new TaiwanCalendar();

                    //整理前台支付別 
                    if (dr["HU_PER"].Equals("3"))//賒帳
                    {
                        pay = new PAY();
                        pay.PayKId = "05";
                        pay.PayKName = "賖帳";
                        pay.PayAmt = Convert.ToDecimal(dr["CASH"].ToString());
                        PayList.Add(pay);
                    }
                    else//現金
                    {
                        pay = new PAY();
                        pay.PayKId = "01";
                        pay.PayKName = "現金";
                        pay.PayAmt = Convert.ToDecimal(dr["CASH"].ToString());
                        PayList.Add(pay);
                    }

                    //信用卡
                    pay = new PAY();
                    pay.PayKId = "02";
                    pay.PayKName = "信用卡";
                    pay.PayAmt = Convert.ToDecimal(dr["VISA"].ToString());
                    pay.PayRemark = dr["VISA_NO"].ToString();
                    PayList.Add(pay);

                    //禮券
                    pay = new PAY();
                    pay.PayKId = "03";
                    pay.PayKName = "禮券";
                    pay.PayAmt = Convert.ToDecimal(dr["GIFT_AMT"].ToString());
                    PayList.Add(pay);

                    //儲值金
                    pay = new PAY();
                    pay.PayKId = "04";
                    pay.PayKName = "儲值金";
                    pay.PayAmt = Convert.ToDecimal(dr["ICASH_GIVE"].ToString());
                    PayList.Add(pay);

                    //券&Pay
                    string[] pay_name = dr["pay_name"].ToString().Split(',');
                    string[] pay_amt = dr["pay_text"].ToString().Split(',');
                    for (int i = 0; i < pay_name.Length; i++)
                    {
                        //空白時break
                        if (string.IsNullOrEmpty(pay_name[i]))
                        {
                            break;
                        }

                        pay = new PAY();
                        pay.PayKId = Global.getTbl.table("WP", $"SELECT PayKId FROM WP_PayKind WHERE PayKName='{pay_name[i]}'").Rows[0][0].ToString();
                        pay.PayKName = pay_name[i];
                        pay.PayAmt = Convert.ToDecimal(pay_amt[i]);
                        PayList.Add(pay);
                    }

                    var PayJsonData = JsonConvert.SerializeObject(PayList);
                    Log.Info(PayJsonData);

                    decimal tax = 0;
                    decimal ntax_amt = 0;
                    decimal tax_amt = 0;

                    if (DBNull.Value.Equals(dr["TAX"]))
                    {
                        tax = 0;
                    }
                    else
                    {
                        tax= Convert.ToDecimal(dr["TAX"]);
                    }

                    if (DBNull.Value.Equals(dr["NTAX_AMT"]))
                    {
                        ntax_amt = 0;
                    }
                    else
                    {
                        ntax_amt = Convert.ToDecimal(dr["NTAX_AMT"]);
                    }

                    if (DBNull.Value.Equals(dr["TAX_AMT"]))
                    {
                        tax_amt = 0;
                    }
                    else
                    {
                        tax_amt = Convert.ToDecimal(dr["TAX_AMT"]);
                    }

                    SqlComm = $"INSERT INTO WP_OutStockPos(OutStkId,OutStkDate,amount,tax,amtNoneTax,reciptNo,companyId,isTax,isBack,isDel,empId,timeCreate,timeUpdate,memo,memSn,outType,outLeft,payKind,amtCargo,amtCoupon,OutStkTime,PayList,machId,returnReciptNo,discount,discountShare) VALUES ('{DateTime.Parse(sampleDate, culture).ToString("yyyyMMdd") + dr["SAL_NO"]}','{DateTime.Parse(sampleDate, culture).ToString("yyyy-MM-dd")}','{Convert.ToDecimal(dr["SAL_AMT"])}','{tax}','{ntax_amt + tax_amt}','{dr["CO_NAME"]}','{dr["CO_NO"]}','Y','0','N','{dr["MAN_CODE"]}','{DateTime.Parse(sampleDate, culture).ToString("yyyy-MM-dd")}','{DateTime.Parse(sampleDate, culture).ToString("yyyy-MM-dd")}','','{dr["CST_NO"]}','{(dr["HU_PER"].ToString() == "0" ? "2" : "0")}','0.00','1','0','0','{dr["SAL_TIME"]}',N'{PayJsonData}','{MchId}','{dr["Remark"]}','{dr["DISCOUNT"]}','{dr["DIS_TTT"]}')";
                    Log.Info($"寫入POS銷售主檔:{SqlComm}");
                    Global.getTbl.updTbl("WP", SqlComm);
                    MainResult++;
                }
                catch (Exception ex)
                {
                    Log.Error($"寫入POS銷售主檔異常:{ex.Message}");
                }
            }

            //刪除POS銷售副檔
            SqlComm = $"DELETE FROM WP_OutStockPosDtl WHERE LEFT(OutStkId,8)>='{Sdate}' AND LEFT(OutStkId,8)<='{Edate}' AND machId='{MchId}'";
            Log.Info($"刪除POS銷售副檔:{SqlComm}");
            Global.getTbl.updTbl("WP", SqlComm);

            //寫入POS銷售副檔
            foreach (DataRow dr in dtOutStockDtlPos.Rows)
            {
                try
                {
                    //日期民國轉西元
                    string sampleDate = dr["SAL_DATE"].ToString();
                    CultureInfo culture = new CultureInfo("zh-TW");
                    culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                    
                    //未稅價格
                    double NoneTax = Math.Round(Convert.ToDouble(dr["U_PRICE"]) / 1.05, MidpointRounding.AwayFromZero);

                    SqlComm = $"INSERT INTO WP_OutStockPosDtl(OutStkId,pNo,qty,amount,amtTotal,amtNoneTax,isTax,isDel,timeCreate,timeUpdate,costStd,outType,outLeft,outDate,payKind,machId,salCode,discount,discountPer,discountShare) VALUES('{DateTime.Parse(sampleDate, culture).ToString("yyyyMMdd") + dr["SAL_NO"]}','{Convert.ToInt32(dr["PDC_NO"])}','{Convert.ToDouble(dr["SAL_QTY"])}','{Convert.ToDouble(dr["U_PRICE"])}','{Convert.ToDecimal(dr["AMT"])}','{(dr["TAX_KIND"].Equals("應稅") ? NoneTax : Convert.ToDouble(dr["U_PRICE"]))}','{(dr["TAX_KIND"].Equals("應稅") ? "Y" : "N")}','N','','','0','2','0','','1','{MchId}','{dr["SAL_CODE"]}','{dr["DIS_AMT"]}',{dr["DIS_PER"]},'{dr["DIS_SHARE"]}')";
                    Log.Info($"寫入POS銷售副檔:{SqlComm}");
                    Global.getTbl.updTbl("WP", SqlComm);
                    MastResult++;
                }
                catch (Exception ex)
                {
                    Log.Error($"寫入POS銷售副檔異常:{ex.Message}");
                }
            }
        }

        //回傳處理結果
        dtResult.Columns.Add("ResultName");
        dtResult.Columns.Add("ResultCount");
        dtResult.Rows.Add(new object[] {"銷售主檔取得筆數",dtOutStockPos.Rows.Count});
        dtResult.Rows.Add(new object[] {"銷售副檔取得筆數",dtOutStockDtlPos.Rows.Count});
        dtResult.Rows.Add(new object[] {"銷售主檔上傳成功筆數",MainResult});
        dtResult.Rows.Add(new object[] {"銷售副檔上傳成功筆數",MastResult});
        dtResult.TableName = "dtResult";
        Log.Info($"寫入銷售日期:{Sdate}~{Edate} 寫入銷售主檔成功筆數:{MainResult} 寫入銷售副檔成功筆數:{MastResult}");

        //計算銷售額
        int salAmt = 0;
        foreach (DataRow dr in dtOutStockPos.Rows)
        {
            salAmt += Convert.ToInt32(dr["SAL_AMT"]);
        }

        //取得年營業額
        decimal yearRevenue = Convert.ToDecimal(Global.getTbl.table("WP", $"SELECT sum(amount) AS '年營業額' FROM WP_vMonthSaleAmtPos　WHERE year(OutStkDate)=year(GETDATE())　GROUP BY year(OutStkDate)").Rows[0]["年營業額"].ToString());
        Log.Info($"取得年營業額:{yearRevenue}");

        //Line通知
        try
        {
            Log.Info($"單位:{Global.PageTitle.Replace("POS進銷存管理系統", string.Empty)} 機號:{MchId} 銷售日期:{Sdate}~{Edate} 營業額:{String.Format("{0:C}", salAmt)} 來客數:{dtOutStockPos.Rows.Count} 銷售主檔:{dtOutStockPos.Rows.Count}/{MainResult} 銷售副檔:{dtOutStockDtlPos.Rows.Count}/{MastResult}");
            SendLineText($"單位:{Global.PageTitle.Replace("POS進銷存管理系統", string.Empty)} 機號:{MchId} 銷售日期:{Sdate}~{Edate} 營業額:{String.Format("{0:C}", salAmt)} 來客數:{dtOutStockPos.Rows.Count} 銷售主檔:{dtOutStockPos.Rows.Count}/{MainResult} 銷售副檔:{dtOutStockDtlPos.Rows.Count}/{MastResult}");
            ExportCharts("Chart_001.aspx");
            ExportCharts("Chart_002.aspx");
            SendLinePhoto("Chart_001.jpg", $"{Global.PageTitle.Replace("POS進銷存管理系統", string.Empty)}{DateTime.Now.Year}年每月營收(總營收額:{String.Format("{0:C}", yearRevenue)})");
            SendLinePhoto("Chart_002.jpg", $"{Global.PageTitle.Replace("POS進銷存管理系統", string.Empty)}近30天商品銷量前十名");
        }
        catch (Exception ex)
        {
            Log.Error($"SendLine:{ex.Message}");
        }

        return dtResult;
    }

    /// <summary>
    /// 取得商品資料
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetProduct(string HiddCode, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");

        DataTable dt=new DataTable();
        dt.TableName = "Product";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }
        string SqlComm = $"SELECT * FROM WP_vProductPos WHERE isSale in('0','1')";
        Log.Info($"取得商品資料:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "Product";

        return dt;
    }

    /// <summary>
    /// 取得商品類別
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetPdKind(string HiddCode, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");

        DataTable dt = new DataTable();
        dt.TableName = "PdKind";

        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }
        string SqlComm = $"SELECT * FROM WP_vPdKindPos";
        Log.Info($"取得商品類別:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "PdKind";

        return dt;
    }

    /// <summary>
    /// 取得會員資料
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetMember(string HiddCode, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        dt.TableName = "Member";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }
        string SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N'";
        Log.Info($"取得會員資料:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "Member";

        return dt;
    }

    /// <summary>
    /// 取得員工資料
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetEmployee(string HiddCode, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        dt.TableName = "Employee";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }
        string SqlComm = $"SELECT * FROM WP_Employee WHERE isStop='N'";
        Log.Info($"取得員工資料:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "Employee";

        return dt;
    }

    /// <summary>
    /// 取得促銷資料
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetPromotion(string HiddCode,string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        dt.TableName = "Promotion";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }
        string SqlComm = $"SELECT * FROM WP_Promotion";
        Log.Info($"取得促銷資料:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "Promotion";

        return dt;
    }

    /// <summary>
    /// 取得付款別資料
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public DataTable GetPayKind(string HiddCode, string MchId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        dt.TableName = "PayKind";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return dt;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return dt;
        }

        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N' AND PayKId not in('01','02','03','04','05')  ";
        Log.Info($"取得付款別資料:{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        dt.TableName = "PayKind";

        return dt;
    }

    /// <summary>
    /// 檢查上傳狀態
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <returns></returns>
    [WebMethod]
    public bool ChkUploadStat(string HiddCode, string MchId, string date)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        dt.TableName = "PosToDbDate";
        //驗證是否正確
        if (ChkCode(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            return false;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            return false;
        }

        string SqlComm = $"SELECT sn FROM WP_PosToDbDate WHERE ptdDate='{date}'";
        Log.Info($"檢查上傳狀態({date}):{SqlComm}");
        dt = Global.getTbl.table("WP", SqlComm);
        if (dt.Rows.Count > 0)
        {
            SqlComm = $"SELECT sn FROM WP_PosToDbDate WHERE ptdDate='{date}' AND isDel='N'";
            Log.Info($"檢查上傳狀態({date}):{SqlComm}");
            dt = Global.getTbl.table("WP", SqlComm);
            if (dt.Rows.Count > 0)
            {
                return false;
            }

            return true;
        }

        return true;
    }

    /// <summary>
    /// 查詢儲值金餘額
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <param name="MchId"></param>
    /// <param name="MemId"></param>
    /// <returns></returns>
    [WebMethod]
    public string QueryStoredValueBalance(string HiddCode, string MchId, string MemId)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        bool verify = true;
        dt.TableName = "WP_StoredValue";
        //驗證是否正確
        if (ChkCode2(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            verify = false;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            verify = false;
        }

        //驗證回覆
        if (verify == false)
        {
            var data = new
            {
                memId = MemId,
                Balance = "0",
                IsOK = false,
                Message = "驗證錯誤或機號錯誤"
            };

            return JsonConvert.SerializeObject(data);
        }
        else
        {
            string SqlComm = $"select Top 1 CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Balance)) as 儲值金餘額 from WP_StoredValue where memId = '{SafeString(MemId)}' order by transactionTime desc";
            
            Log.Info($"查詢{MemId}儲值金餘額:{SqlComm.Replace(Global.PosKey , String.Empty)}");
            //Log.Info($"查詢{MemId}儲值金餘額:{SqlComm}");

            dt = Global.getTbl.table("WP", SqlComm);

            if (dt.Rows.Count > 0 && dt.Rows[0]["儲值金餘額"] != DBNull.Value)
            {
                var data = new
                {
                    memId = MemId,
                    Balance = dt.Rows[0]["儲值金餘額"].ToString(),
                    IsOK = true,
                    Message = "成功"
                };

                return JsonConvert.SerializeObject(data);
            }
            else
            {
                var data = new
                {
                    memId = MemId,
                    Balance = "0",
                    IsOK = true,
                    Message = "儲值金餘額不足"
                };

                return JsonConvert.SerializeObject(data);
            }
        }
    }

    /// <summary>
    /// 查詢儲值金交易紀錄(依會員代號)
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <param name="MchId"></param>
    /// <param name="MemId"></param>
    /// <param name="DataRecord"></param>
    /// <returns></returns>
    [WebMethod]
    public string QueryStoredValueRecordByMemId(string HiddCode, string MchId, string MemId, int DataRecord)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        bool verify = true;
        dt.TableName = "WP_StoredValue";
        //驗證是否正確
        if (ChkCode2(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            verify = false;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            verify = false;
        }

        //驗證回覆
        if (verify == false)
        {
            var data = new
            {
                memId = MemId,
                Balance = "0",
                IsOK = false,
                Message = "驗證錯誤或機號錯誤"
            };

            return JsonConvert.SerializeObject(data);
        }
        else
        {
            string SqlComm = $"select Top {DataRecord} transactionNo,transactionTime,memId,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Variety)) as Variety,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Balance)) as Balance,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Code)) as Code, salDate, salNo, machId, CONVERT(varchar(500), DecryptByPassphrase('{Global.PosKey}', Remark)) as Remark from WP_StoredValue where memId = '{SafeString(MemId)}' order by transactionTime desc";

            Log.Info($"查詢{MemId}儲值金交易紀錄:{SqlComm.Replace(Global.PosKey, String.Empty)}");
            //Log.Info($"查詢{MemId}儲值金餘額:{SqlComm}");

            dt = Global.getTbl.table("WP", SqlComm);

            if (dt.Rows.Count > 0)
            {
                StoredValueRecord record  = new StoredValueRecord();
                Details details;
                record.memId = MemId;
                record.IsOK = true;
                record.Message = "成功";
                List<Details> List = new List<Details>();

                foreach (DataRow row in dt.Rows)
                {
                    details = new Details
                    {
                        transactionNo = row["transactionNo"].ToString(),
                        transactionTime = row["transactionTime"].ToString(),
                        memId = row["memId"].ToString(),
                        salDate = row["salDate"].ToString(),
                        salNo = row["salNo"].ToString(),
                        machId = row["machId"].ToString(),
                        Variety = Convert.ToInt32(row["Variety"].ToString()),
                        Balance = Convert.ToInt32(row["Balance"].ToString()),
                        Code = row["Code"].ToString(),
                        Remark = row["Remark"].ToString(),
                    };
                    List.Add(details);
                }
                record.details = List;

                return JsonConvert.SerializeObject(record);
            }
            else
            {
                StoredValueRecord record = new StoredValueRecord();
                record.memId = MemId;
                record.IsOK = true;
                record.Message = "查無交易紀錄";

                return JsonConvert.SerializeObject(record);
            }
        }
    }

    /// <summary>
    /// 查詢儲值金交易紀錄(依日期)
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <param name="MchId"></param>
    /// <param name="MemId"></param>
    /// <param name="SDate"></param>
    /// <param name="EDate"></param>
    /// <returns></returns>
    [WebMethod]
    public string QueryStoredValueRecordByDate(string HiddCode, string MchId, string SDate, string EDate)
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        bool verify = true;
        dt.TableName = "WP_StoredValue";
        //驗證是否正確
        if (ChkCode2(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            verify = false;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            verify = false;
        }

        //驗證回覆
        if (verify == false)
        {
            var data = new
            {
                Balance = "0",
                IsOK = false,
                Message = "驗證錯誤或機號錯誤"
            };

            return JsonConvert.SerializeObject(data);
        }
        else
        {
            string SqlComm = $"select transactionNo,transactionTime,memId,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Variety)) as Variety,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Balance)) as Balance,CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey}', Code)) as Code , salDate, salNo, machId, CONVERT(varchar(500), DecryptByPassphrase('{Global.PosKey}', Remark)) as Remark from WP_StoredValue where transactionTime >= '{SafeString(SDate)}' and transactionTime <= '{SafeString(EDate)}' order by transactionTime desc";

            Log.Info($"查詢儲值金交易紀錄:{SqlComm.Replace(Global.PosKey, String.Empty)}");
            //Log.Info($"查詢{MemId}儲值金餘額:{SqlComm}");

            dt = Global.getTbl.table("WP", SqlComm);

            if (dt.Rows.Count > 0)
            {
                StoredValueRecord record = new StoredValueRecord();
                Details details;
                record.IsOK = true;
                record.Message = "成功";
                List<Details> List = new List<Details>();

                foreach (DataRow row in dt.Rows)
                {
                    details = new Details
                    {
                        transactionNo = row["transactionNo"].ToString(),
                        transactionTime = row["transactionTime"].ToString(),
                        memId = row["memId"].ToString(),
                        salDate = row["salDate"].ToString(),
                        salNo = row["salNo"].ToString(),
                        machId = row["machId"].ToString(),
                        Variety = Convert.ToInt32(row["Variety"].ToString()),
                        Balance = Convert.ToInt32(row["Balance"].ToString()),
                        Code = row["Code"].ToString(),
                        Remark = row["Remark"].ToString(),
                    };
                    List.Add(details);
                }
                record.details = List;

                return JsonConvert.SerializeObject(record);
            }
            else
            {
                StoredValueRecord record = new StoredValueRecord();
                record.IsOK = true;
                record.Message = "查無交易紀錄";

                return JsonConvert.SerializeObject(record);
            }
        }
    }

    /// <summary>
    /// 增減儲值金餘額
    /// </summary>
    /// <param name="HiddCode"></param>
    /// <param name="MchId"></param>
    /// <param name="MemId"></param>
    /// <param name="Value"></param>
    /// <param name="Code"></param>
    /// <param name="Remark"></param>
    /// <returns></returns>
    [WebMethod]
    public string VarietyStoredValueBalance(string HiddCode, string MchId, string MemId,int Value,string Code,string Remark = "", string salDate = "", string salNo = "")
    {
        Log.Info($"來源:{HttpContext.Current.Request.UserHostAddress} {HttpContext.Current.Request.UserHostName} {MchId}");
        DataTable dt = new DataTable();
        bool verify = true;
        dt.TableName = "WP_StoredValue";
        //驗證是否正確
        if (ChkCode2(HiddCode) == false)
        {
            Log.Warn($"驗證錯誤:{HiddCode}");
            verify=false;
        }
        //驗證機號
        if (ChkMachine(MchId) == false)
        {
            Log.Warn($"機號錯誤:{MchId}");
            verify = false;
        }
     
        //驗證回覆
        if (verify==false)
        {
            var data = new
            {
                memId = MemId,
                Variety = "0",
                Balance = "0",
                IsOK = false,
                Code = Code,
                Message = "驗證錯誤或機號錯誤"
            };

            return JsonConvert.SerializeObject(data);
        }
        else
        {
            string SqlComm = $"select Top 1 CONVERT(varchar(10), DecryptByPassphrase('{Global.PosKey }', Balance)) as 儲值金餘額 from WP_StoredValue where memId = '{SafeString(MemId)}' order by transactionTime desc";
            int Balance = 0;//餘額
            
            Log.Info($"查詢儲值金餘額:{SqlComm.Replace(Global.PosKey , String.Empty)}");
            //Log.Info($"查詢儲值金餘額:{SqlComm}");

            dt = Global.getTbl.table("WP", SqlComm);
            if (dt.Rows.Count > 0)
            {
                Balance = Convert.ToInt32(dt.Rows[0]["儲值金餘額"].ToString());
            }
            else
            {
                Balance = 0;
            }

            Log.Info($"查詢{MemId}儲值金 餘額:{Balance} 儲值:{Value}");


            //檢查儲值金小於可用餘額
            if (Balance + Value < 0)
            {
                var data = new
                {
                    memId = MemId,
                    Variety = Value,
                    Balance = Balance,
                    IsOK = true,
                    Code = Code,
                    Message = "儲值金餘額不足"
                };

                Log.Warn($"儲值金餘額不足");

                return JsonConvert.SerializeObject(data);
            }
            else
            {
                try
                {
                    string tranNo = GenerateTransCodeId();
                    int balance = Balance + Value;
                    SqlComm = $"INSERT INTO WP_StoredValue(transactionNo,memId,salDate,salNo,machId,Variety,Balance,Code,Remark) VALUES('{tranNo}','{SafeString(MemId)}','{SafeString(salDate)}','{SafeString(salNo)}','{SafeString(MchId)}',EncryptByPassPhrase('{Global.PosKey}', '{Value}'),EncryptByPassPhrase('{Global.PosKey}', '{balance}'),EncryptByPassPhrase('{Global.PosKey}', '{Code}'),EncryptByPassPhrase('{Global.PosKey}', '{SafeString(Remark)}'))";
                    Log.Info($"寫入儲值金紀錄:{SqlComm.Replace(Global.PosKey, String.Empty)}");
                    
                    Global.getTbl.updTbl("WP", SqlComm);

                    var data = new
                    {
                        memId = MemId,
                        tranNo = tranNo,
                        Variety = Value,
                        Balance = balance,
                        IsOK = true,
                        Code = Code,
                        Message = "交易成功"
                    };

                    Log.Info($"儲值金交易成功--餘額:{balance}");

                    return JsonConvert.SerializeObject(data);
                }
                catch (Exception ex)
                {

                    var data = new
                    {
                        memId = MemId,
                        Variety = Value,
                        Balance = Balance,
                        IsOK = false,
                        Code = Code,
                        Message = "儲值金交易寫入異常"
                    };

                    Log.Error($"儲值金交易寫入異常:{ex.Message}");

                    return JsonConvert.SerializeObject(data);
                }   
            }
  
        }
        
    }

    /// <summary>
    /// 產生交易序號
    /// </summary>
    /// <returns></returns>
    private string GenerateTransCodeId()
    {
        // 取得當前日期
        DateTime now = DateTime.Now;

        // 格式化日期部分，例如 20230511
        string datePart = now.ToString("yyyyMMdd");

        // 產生流水號部分，使用 Guid.NewGuid() 方法生成不重複的隨機字串，
        // 再取其前八個字元作為流水號，例如 f9b23e7c
        string serialPart = Guid.NewGuid().ToString().Substring(0, 8);

        // 組合交易序號
        string transCodeId = datePart + serialPart;

        return transCodeId;
    }

    /// <summary>
    /// 處理特殊符號
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private string SafeString(string input)
    {
        StringBuilder sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            switch (c)
            {
                case '\'':
                    sb.Append(String.Empty); // 替換單引號
                    break;
                case ';':
                    sb.Append(String.Empty); // 替換分號
                    break;
                case '-':
                    sb.Append(String.Empty); // 替換減號
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 發送Line通知(文字)
    /// </summary>
    /// <param name="msg"></param>
    private void SendLineText(string msg)
    {
        HttpClient Client = new HttpClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var content = new Dictionary<string, string>();
        content.Add("message", msg);
        Client.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content));
    }

    /// <summary>
    /// 發送Line通知(圖片)
    /// </summary>
    /// <param name="msg"></param>
    private void SendLinePhoto(string fileName,string msg)
    {
        var upfilebytes = File.ReadAllBytes(System.Web.HttpContext.Current.Server.MapPath("/") + "charts/" + fileName);
        HttpClientHandler handler = new HttpClientHandler();
        HttpClient Client = new HttpClient(handler);
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);    //Token
        MultipartFormDataContent content = new MultipartFormDataContent();
        ByteArrayContent baContent = new ByteArrayContent(upfilebytes);
        content.Add(baContent, "imageFile", fileName);
        string url = @"https://notify-api.line.me/api/notify?message="+ msg;
        Client.PostAsync(url, content);
    }

    /// <summary>
    /// 產生Charts
    /// </summary>
    /// <param name="fileName"></param>
    private void ExportCharts(string fileName)
    {
        HttpClient httpClient = new HttpClient();

        string url = HttpContext.Current.Request.Url.AbsoluteUri.Replace("WebService.asmx",string.Empty) + $"/charts/{fileName}";
        Log.Info(url);

        //發送請求並取得回應內容
        var responseMessage = httpClient.GetAsync(url).Result;

        //檢查回應的伺服器狀態StatusCode是否是200 OK
        if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
        {
            //讀取Content內容
            string responseResult = responseMessage.Content.ReadAsStringAsync().Result;
            Log.Info(responseResult);
        }

    }

    /// <summary>
    /// 檢查機號
    /// </summary>
    /// <param name="mchId"></param>
    /// <returns></returns>
    public bool ChkMachine(string mchId)
    {
        string SqlComm = $"SELECT * FROM WP_Machine WHERE machId='{mchId}' AND isStop='N'";
        DataTable dt = new DataTable();
        dt = Global.getTbl.table("WP", SqlComm);
        if (dt.Rows.Count>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

/// <summary>
/// 支付別
/// </summary>
public class PAY
{
    /// <summary>
    /// 支付編號
    /// </summary>
    public string PayKId { get; set; }
    /// <summary>
    /// 支付名稱
    /// </summary>
    public string PayKName { get; set; }
    /// <summary>
    /// 支付金額
    /// </summary>
    public decimal PayAmt { get; set; }
    /// <summary>
    /// 支付備註
    /// </summary>
    public string PayRemark { get; set; }
}

/// <summary>
/// 儲值金紀錄
/// </summary>
public class StoredValueRecord
{
    public string memId { get; set; }

    public string Balance { get; set; }

    public bool IsOK { get; set; }

    public string Message { get; set; }
    public List<Details> details { get; set; }
}

public class Details
{
    public string transactionNo { get; set; }
    public string transactionTime { get; set; }
    public string memId { get; set; }
    public string salDate { get; set; }
    public string salNo { get; set; }
    public string machId { get; set; }
    public int Variety { get; set; }
    public int Balance { get; set; }
    public string Code { get; set; }
    public string Remark { get; set; }
}