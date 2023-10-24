using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using static Global;



/// <summary>
/// StringEncrypt AES 演算法為字串加密解密
/// ex:加密  YourString  = new StringEncrypt().AesEncryptBase64( YourString , BuyGlobal.CryptoKey);
/// ex:解密  YourString  = new StringEncrypt().AesDecryptBase64( YourString , BuyGlobal.CryptoKey);
/// </summary>
public class StringEncrypt
{
    /// <summary>
    /// 字串加密(非對稱式)
    /// </summary>
    /// <param name="SourceStr">加密前字串</param>
    /// <param name="CryptoKey">加密金鑰</param>
    /// <returns>加密後字串</returns>
    public string AesEncryptBase64(string SourceStr, string CryptoKey)
    {
        string encrypt = "";
        try
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Encoding.UTF8.GetBytes(SourceStr);
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
        }
        return encrypt;
    }

    /// <summary>
    /// 字串解密(非對稱式)
    /// </summary>
    /// <param name="SourceStr">解密前字串</param>
    /// <param name="CryptoKey">解密金鑰</param>
    /// <returns>解密後字串</returns>
    public string AesDecryptBase64(string SourceStr, string CryptoKey)
    {
        string decrypt = "";
        try
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Convert.FromBase64String(SourceStr);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    decrypt = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
        }
        return decrypt;
    }

    //32位加密：ComputeHash
    public string getMd5Method(string input)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] myData = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i <= myData.Length - 1; i++)
        {
            sBuilder.Append(myData[i].ToString("x2"));
        }
        return string.Format("ComputeHash(32)：{0}", sBuilder.ToString());
    }
}

public class GetTbl
{
    /// <summary>
    /// GetTbl  取得Tabel
    /// </summary>
    /// <param name="_data"></param>    Bomb:讀取bomb table /   Buy:讀取buy table
    /// <param name="_pSqlComm"></param>
    /// <returns></returns>
    public DataTable table(string _data, string _pSqlComm)
    {
        string connString;
        connString = ConfigurationManager.ConnectionStrings[_data + "Rd"].ConnectionString;

        using (var conn = new SqlConnection(connString))
        {
            SqlCommand com = new SqlCommand(_pSqlComm, conn);
            if (conn.State != ConnectionState.Open)
                conn.Open();
            var reader = com.ExecuteReader(CommandBehavior.CloseConnection);
            DataTable tb = new DataTable();
            tb.Load(reader);

            return tb;
        }
    }

    public bool updTbl(string _data, string _pSqlComm)
    {
        string connString;
        connString = ConfigurationManager.ConnectionStrings[_data].ConnectionString;

        using (var conn = new SqlConnection(connString))
        {
            SqlCommand com = new SqlCommand(_pSqlComm, conn);
            if (conn.State != ConnectionState.Open)
                conn.Open();
            int SqlResult = com.ExecuteNonQuery();
            conn.Close();

            return (SqlResult == 1) ? true : false;
        }
    }

    public JObject ToJSON(DataTable _table)
    {
        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        if (_table.Rows.Count > 0)
        {
            Dictionary<string, object> row;
            foreach (DataRow dr in _table.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in _table.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
        };
        return JObject.Parse(serializer.Serialize(rows));
    }
}


/// <summary>
/// Cookies 處理Cookies
/// </summary>
public class Cookies
{
    /// <summary>
    /// 清除cookies
    /// </summary>
    /// <param name="_CookiesNameGrp"></param>  cookies名稱集合ex: "MemId,MemName"
    /// <returns></returns>
    public bool Clear(string _CookiesNameGrp)
    {
        foreach (string Name in _CookiesNameGrp.Split(','))
        {
            HttpCookie NewCookies = new HttpCookie(Name) { Path = "/" };
            NewCookies.Value = "";
            NewCookies.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(NewCookies);
        }

        return true;
    }

    /// <summary>
    /// 更新cookies
    /// </summary>
    /// <param name="_Name"></param>            _Name:Cookies名稱
    /// <param name="_Value"></param>           _Value:Cookies內容(空白代表刪除cookies)
    /// <param name="_LimitHours"></param>      _LimitHours:Cookies保存時間(分鐘;空白代表1年)
    /// 
    public bool Update(string _Name, string _Value, string _LimitHours)
    {
        HttpCookie NewCookies = new HttpCookie(_Name) { Path = "/" };
        NewCookies.Expires = (_Value == "")
            ? DateTime.Now.AddDays(-1)      // 更改內容空字串代表清除Cookies
            : NewCookies.Expires = (_LimitHours == "")
                ? DateTime.Now.AddDays(365)
                : DateTime.Now.AddHours(double.Parse(_LimitHours));

        NewCookies.Value = _Value;
        HttpContext.Current.Response.Cookies.Add(NewCookies);
        return true;

    }

    /// <summary>
    /// 讀取cookies
    /// </summary>
    /// <param name="_Name"></param>            _Name:Cookies名稱
    public string Read(string _Name)
    {
        HttpCookie RdCookies = HttpContext.Current.Request.Cookies[_Name];
        return (RdCookies == null) ? "" : RdCookies.Value;
    }
}


/// <summary>
/// 字串集合加入及刪除
/// </summary>
public class ArrayGrp
{
    public string In(string _group, string _cell)
    {
        _group += _group == "" ? _cell : ((IList)_group.Split(',')).Contains(_cell) ? "" : $",{_cell}";
        return _group;
    }
    public string Out(string _group, string _cell)
    {
        _group = _group == "" ? "" : string.Join(",", _group.Split(',').Where((source, index) => source != _cell).ToArray());
        return _group;
    }
}


/// <summary>
/// WsString    字串處理
/// </summary>
public class WsString
{
    ///<summary>
    ///以*右側隱藏字串
    /// </summary>
    /// <param name="_Str"></param> 處理字串
    /// <param name="Qty"></param> 隱藏數量
    public string HideRight(string _Str, int _Qty)
    {
        string hideString = (_Str.Length == 0 || _Qty <= 0)
            ? "overflow"
            : (_Str.Length <= _Qty)
                ? string.Concat(Enumerable.Repeat("*", _Str.Length))
                : $"{_Str.Substring(0, _Str.Length - _Qty)}{string.Concat(Enumerable.Repeat("*", _Qty))}";
        return hideString;
    }

    ///<summary>
    ///保留左側字串，其餘以*隱藏字串
    /// </summary>
    /// <param name="_Str"></param> 處理字串
    /// <param name="Qty"></param> 顯示數量
    public string ShowLeft(string _Str, int _Qty)
    {
        string hideString = (_Str.Length == 0 || _Qty <= 0)
            ? "overflow"
            : (_Str.Length == 1)
                ? _Str
                : $"{_Str.Substring(0, _Str.Length <= _Qty ? 2 : _Qty)}{string.Concat(Enumerable.Repeat("*", _Str.Length - (_Str.Length <= _Qty ? 2 : _Qty)))}";
        return hideString;
    }

    /// <summary>
    /// 隱藏字串中間以符號代替
    /// </summary>
    /// <param name="_Str"></param>  原始字串
    /// <param name="_Sign"></param>  替代字符號
    public string HideMiddle(string _Str, string _Sign)
    {
        string reString;
        switch (_Str.Length)
        {
            case 0:
            case 1:
                reString = _Str;
                break;
            case 2:
                reString = $"{_Str.Substring(0, 1)}{_Sign}";
                break;
            default:
                reString = $"{_Str.Substring(0, 1)}{_Sign}{_Str.Substring(_Str.Length - 1, 1)}";
                break;
        }
        return reString;
    }


    /// <summary>
    /// 判斷是否符合email
    /// </summary>
    public bool IsEmail(string _Email)
    {
        return Regex.IsMatch(_Email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }

    /// <summary>
    /// 判斷是否英數字
    /// </summary>
    public bool IsAlphaNum(string _Str)
    {
        Regex regex = new Regex(@"^[A-Za-z0-9]");
        return regex.IsMatch(_Str);
    }


    /// <summary>
    /// 是否符合電話(中文字、英數字组成以及底線、減號、空白、小數點、#)
    /// 
    /// </summary>
    public bool IsTelephone(string _Str)
    {
        Regex regex = new Regex(@"^[\u4e00-\u9fa5_a-zA-Z0-9_.# ]+$");
        return regex.IsMatch(_Str);
    }


    /// <summary>
    /// 判斷是否日期格式
    /// </summary>
    /// <param name="_date"></param>
    /// <returns></returns>
    public bool IsDate(string _date)
    {
        try
        {
            DateTime.Parse(_date); //转换成功,肯定是日期型了
            return true;
        }
        catch
        {
            //转换不成功,肯定不是日期型了.
            return false;
        }


    }

    /// <summary>
    /// IsInDate    判斷日期區間內 result:Y:日期內 B:還沒到 A:過了
    /// </summary>
    //------
    public class IsInDate
    {
        /// <param name="_Date"></param>     判斷日期
        /// <param name="_SDate"></param>    開始日期
        /// <param name="_EDate"></param>    截止日期
        /// <returns></returns>
        public string Compare(DateTime _Date, DateTime _SDate, DateTime _EDate)
        {
            return DateTime.Compare(_Date, _SDate) >= 0 && DateTime.Compare(_Date, _EDate) <= 0 ? "Y" : DateTime.Compare(_SDate, _Date) > 0 ? "B" : "A";
        }
    }
}

/// <summary>
/// IsInDate    判斷日期區間內 result:Y:日期內 B:還沒到 A:過了
/// </summary>
//------
public class IsInDate
{
    /// <param name="_Date"></param>     判斷日期
    /// <param name="_SDate"></param>    開始日期
    /// <param name="_EDate"></param>    截止日期
    /// <returns></returns>
    public string Compare(DateTime _Date, DateTime _SDate, DateTime _EDate)
    {
        return DateTime.Compare(_Date, _SDate) >= 0 && DateTime.Compare(_Date, _EDate) <= 0 ? "Y" : DateTime.Compare(_SDate, _Date) > 0 ? "B" : "A";
    }
}

/// <summary>
/// PayMethod 付款方式中文化
/// </summary>
public class PayMethod
{
    public string name;
    public PayMethod(string _method)
    {
        switch (_method)
        {
            case "1": name = "ATM/滙款"; break;
            case "2": name = "貨到付款"; break;
            case "3": name = "信用卡付款"; break;
            default: name = "未知"; break;
        }
    }
}

/// <summary>
/// PayKind 付款方式
/// </summary>
public class PayKind
{
    string name, html;
    string[] kind = { "付現", "賒帳"};
    public string Name(string _kind)
    {
        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N' AND PayKId='{_kind}'";
        DataTable payTbl = getTbl.table("WP", SqlComm);
        //name = int.TryParse(_kind, out _) ? _kind == "99" ? "其他" : int.Parse(_kind) < kind.Length ? kind[int.Parse(_kind)] : "未知" : "未知";
        name = payTbl.Rows.Count == 0 ? "未知" : $"{payTbl.Rows[0]["PayKName"]}";
        return name;
    }

    /// <summary>
    /// 產生付款選擇的HTML
    /// </summary>
    /// <param name="_kind"></param>指定的種類
    /// <param name="_unKind"></param>排除的種類
    /// <returns></returns>
    public string HTML(string _kind, string _unKind)
    {
        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N' {(_unKind == "" ? "" : $"AND PayKId<>'{_unKind}'")} ORDER BY PayKId";
        DataTable payTbl = getTbl.table("WP", SqlComm);
        html = "";
        foreach(DataRow row in payTbl.Rows)
        {
            html += $"<option {($"{row["PayKId"]}" == _kind ? "selected" : "")} value='{row["PayKId"]}'>{row["PayKName"]}</option>";
        }
        return html;
    }

    public string HTML2(string _JSON)
    {
        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N' ORDER BY PayKId";
        DataTable payTbl = getTbl.table("WP", SqlComm);
        string payList = $"<select class='pay-group pay-kind' class='form-control' style='margin-right:5px;'>{HTML("0", "")}</select>" +
        $"<input type='text' class='form-control chk-input pay-group pay-amt' data-func='real_number' maxlength='6' style='width:75px;' />";
        html = "";
        if (_JSON == "")
        {
            html = "<div id='pay-list-grp' style='display:flex;align-items:center'>" +
                "<div class='pay-row'>" +
                    $"{payList}<i class='fas fa-plus-circle btn-pay btn-pay-add' style='color:#080;margin:0 20px 0 5px;'></i>" +
                "</div>" +
            "</div>" +
            $"<div id='pk-list-add' style='display:none;'><div class='pay-row'>{payList}<i class='fas fa-minus-circle btn-pay btn-pay-del' style='color:#f00;margin:0 20px 0 5px;'></i></div></div>";
        }
        else
        {
            try
            {
                JArray payJSON = JArray.Parse(_JSON);
                int recNo = 0;
                html = "<div id='pay-list-grp' style='display:flex;align-items:center'>";
                    foreach (var pay in payJSON)
                    {
                        html += $"<div class='pay-row'>" +
                            $"<select class='pay-group pay-kind' class='form-control' style='margin-right:5px;'>{HTML($"{pay["PAYID"]}", "")}</select>" +
                            $"<input type='text' class='form-control chk-input pay-group pay-amt' data-func='real_number' maxlength='6' style='width:75px;' value='{pay["PAYAMT"]}' />";
                            html += recNo == 0
                                ? $"<i class='fas fa-plus-circle pay-group btn-pay btn-pay-add' style='color:#080;margin:0 20px 0 5px;cursor:pointer;'></i>"
                                : $"<i class='fas fa-minus-circle pay-group btn-pay btn-pay-del' style='color:#f00;margin:0 20px 0 5px;cursor:pointer;'></i>";
                        html += "</div>";
                        recNo++;
                    }
                html += "</div>" +
                $"<div id='pk-list-add' style='display:none;'><div class='pay-row'>{payList}<i class='fas fa-minus-circle pay-group btn-pay btn-pay-del' style='color:#f00;margin:0 20px 0 5px;'></i></div></div>";

            }
            catch (InvalidOperationException ex)
            {
                return(ex.Message);
            }
        }
        return html;
    }

}


public class ToDouble
{
    public double Numer(double orgnlDbl, int point)
    {
        return decimal.ToDouble(Math.Round(Convert.ToDecimal(orgnlDbl), point));
        //return decimal.ToDouble(Math.Round(Convert.ToDecimal(orgnlDbl), point, MidpointRounding.AwayFromZero));
    }
}


/// <summary>
/// 數字三位數一逗點
/// </summary>
public class ThreeDot
{
    public string To3Dot(string _str)
    {
        string[] strAry;
        string dotStr = "", dotBE = "", str;
        int anchor = 0;
        str = _str.Substring(0, 1) == "-" ? _str.Substring(1) : _str;
        strAry = str.Split('.');
        #region 處理小數點前
        if (strAry[0].Length <= 3) { dotBE = strAry[0]; }
        else
        {
            anchor = strAry[0].Length % 3;
            dotBE = strAry[0].Substring(0, anchor);
            for (int i = anchor; i < strAry[0].Length; i = i + 3)
            {
                dotBE += $"{(dotBE == "" ? "" : ",")}{strAry[0].Substring(i, 3)}";
            }
        }
        #endregion
        string dotAF = strAry.Length > 1 ? $".{strAry[1]}" : "";    //處理小數點後
        dotStr = $"{dotBE}{dotAF}";
        return $"{(_str.Substring(0, 1) == "-" ? "-" : "")}{dotStr}";
    }
}

namespace test.lib
{
    public class function
    {
        public static bool isJson(String value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return false;
                object aa = JsonConvert.DeserializeObject(value);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}


