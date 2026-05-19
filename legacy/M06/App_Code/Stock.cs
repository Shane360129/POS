using System;
using System.Data;
using static Global;

/// <summary>
/// Stock 進銷存
/// </summary>
public class Stock
{
    /// <summary>
    /// 商品狀態中文化
    /// </summary>
    /// <param name="_status"></param>商品狀態代碼
    /// <returns></returns>
    public string IsSaleName(string _status)
    {
        string name;
        switch (_status)
        {
            case "0": name = "正常進銷貨"; break;
            case "1": name = "只停止進貨"; break;
            case "2": name = "只停止銷貨"; break;
            case "3": name = "停止進銷貨"; break;
            default: name = "未知"; break;
        }
        return name;
    }

    /// <summary>
    /// 商品狀態下拉選項OPTION HTML
    /// </summary>
    /// <param name="_status"></param>商品狀態代碼
    /// <param name="_allYN"></param>是否要出現全部
    /// <returns></returns>
    public string IsSaleOptionHTML(string _status, string _allYN)
    {
        string HTML = _allYN == "Y" ? $"<option value='' {(_status == "ALL" ? "selected" : "")}>全部</option>" : "";
        for(int i = 0; i <= 3; i++)
        {
            HTML += $"<option value='{i}' {(_status == $"{i}" ? "selected" : "")}>{IsSaleName($"{i}")}</option>";
        }
        return HTML;
    }

    /// <summary>
    /// 價格種類中文化
    /// </summary>
    /// <param name="_id"></param>價格種類代碼
    /// <returns></returns>
    public string PriceName(string _id)
    {
        string name;
        switch (_id)
        {
            case "1": name = "標準價格"; break;
            case "2": name = "最低應售價"; break;
            case "3": name = "會員價"; break;
            case "4": name = "大批價"; break;
            default: name = "未知"; break;
        }
        return name;
    }

    /// <summary>
    /// 進貨單號取得
    /// </summary>
    /// <param name="_InStkDate"></param>進貨日期
    /// <returns></returns>
    public string InStkMaxId(string _InStkDate)
    {
        string InStkDate = DateTime.Parse(_InStkDate).ToString("yyyyMMdd");
        string SqlComm = $"SELECT MAX(InStkId) AS MaxId FROM WP_InStock WHERE SUBSTRING(InStkId ,1 , 8)='{InStkDate}'";
        DataTable MaxIdTbl = getTbl.table("WP", SqlComm);

        DataRow row = MaxIdTbl.Rows[0];
        string MaxId = row["MaxId"].ToString();
        int MaxNum = MaxId == "" ? 1 : int.Parse(MaxId.Substring(MaxId.Length - 4, 4)) + 1;
        return $"{InStkDate}{MaxNum.ToString().PadLeft(4, '0')}";
    }

    /// <summary>
    /// 應付銷帳單號取得
    /// </summary>
    /// <param name="_AcctOutDate"></param>銷帳日期
    /// <returns></returns>
    public string AcctOutMaxId(string _AcctOutDate)
    {
        string AcctOutDate = DateTime.Parse(_AcctOutDate).ToString("yyyyMMdd");
        string SqlComm = $"SELECT MAX(acctOutId) AS MaxId FROM WP_AcctOut WHERE SUBSTRING(acctOutId ,1 , 8)='{AcctOutDate}'";
        DataTable MaxIdTbl = getTbl.table("WP", SqlComm);

        DataRow row = MaxIdTbl.Rows[0];
        string MaxId = row["MaxId"].ToString();
        int MaxNum = MaxId == "" ? 1 : int.Parse(MaxId.Substring(MaxId.Length - 4, 4)) + 1;
        return $"{AcctOutDate}{MaxNum.ToString().PadLeft(4, '0')}";
    }

    /// <summary>
    /// 應收銷帳單號取得
    /// </summary>
    /// <param name="_AcctInDate"></param>銷帳日期
    /// <returns></returns>
    public string AcctInMaxId(string _AcctInDate)
    {
        string AcctInDate = DateTime.Parse(_AcctInDate).ToString("yyyyMMdd");
        string SqlComm = $"SELECT MAX(acctInId) AS MaxId FROM WP_AcctIn WHERE SUBSTRING(acctInId ,1 , 8)='{AcctInDate}'";
        DataTable MaxIdTbl = getTbl.table("WP", SqlComm);

        DataRow row = MaxIdTbl.Rows[0];
        string MaxId = row["MaxId"].ToString();
        int MaxNum = MaxId == "" ? 1 : int.Parse(MaxId.Substring(MaxId.Length - 4, 4)) + 1;
        return $"{AcctInDate}{MaxNum.ToString().PadLeft(4, '0')}";
    }

    public string PayTypeName(string _id)
    {
        string name;
        switch (_id)
        {
            case "0": name = "未結"; break;
            case "1": name = "未全結"; break;
            case "2": name = "全結"; break;
            default: name = "未知"; break;
        }
        return name;
    }

    /// <summary>
    /// 出貨單號取得
    /// </summary>
    /// <param name="_OutStkDate"></param>出貨日期
    /// <returns></returns>
    public string OutStkMaxId(string _OutStkDate)
    {
        string OutStkDate = DateTime.Parse(_OutStkDate).ToString("yyyyMMdd");
        string SqlComm = $"SELECT MAX(OutStkId) AS MaxId FROM WP_OutStock WHERE SUBSTRING(OutStkId ,1 , 8)='{OutStkDate}'";
        DataTable MaxIdTbl = getTbl.table("WP", SqlComm);

        DataRow row = MaxIdTbl.Rows[0];
        string MaxId = row["MaxId"].ToString();
        int MaxNum = MaxId == "" ? 1 : int.Parse(MaxId.Substring(MaxId.Length - 4, 4)) + 1;
        return $"{OutStkDate}{MaxNum.ToString().PadLeft(4, '0')}";
    }

    public string OutTypeName(string _id)
    {
        string name;
        switch (_id)
        {
            case "0": name = "未結"; break;
            case "1": name = "未全結"; break;
            case "2": name = "全結"; break;
            default: name = "未知"; break;
        }
        return name;
    }

    /// <summary>
    /// 取得允許進銷貨最小日期(依結帳日判定)
    /// </summary>
    /// <returns></returns>
    public string MinDate()
    {
        string SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY yearMonth DESC";
        DataTable chkoutYMTbl = getTbl.table("WP", SqlComm);

        string mDate;
        if (chkoutYMTbl.Rows.Count == 0)
        {
            mDate = StartYM;
            //mDate = $"{StartYM}01";
        }
        else
        {
            mDate = $"{chkoutYMTbl.Rows[0]["YearMonth"]}";
            mDate = DateTime.Parse($"{mDate.Substring(0, 4)}-{mDate.Substring(4, 2)}-{mDate.Substring(6, 2)}").AddDays(1).ToString("yyyyMMdd");

            //string sYear = $"{chkoutYMTbl.Rows[0]["YearMonth"]}".Substring(0, 4);
            //string sMonth = $"{chkoutYMTbl.Rows[0]["YearMonth"]}".Substring(4, 2);

            //sYear = sMonth == "12" ? $"{int.Parse(sYear) + 1}" : sYear;
            //sMonth = sMonth == "12" ? "01" : $"{int.Parse(sMonth) + 1}".PadLeft(2, '0');
            //mDate = $"{sYear}{sMonth}01";
        }

        return mDate;
    }
}

