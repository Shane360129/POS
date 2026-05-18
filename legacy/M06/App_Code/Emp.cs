using System;
using System.Collections;
using System.Data;
using static Global;

public class Employee
{
    public bool IsEmp;
    public DataTable EmpTbl, PrgTbl;
    public string EmpId, EmpPw, EmpName, EmpGrpId, EmpIsAdmin, EmpPrgIdGrp;

    public Employee()
    {
        EmpId = stringEncrypt.AesDecryptBase64(cookies.Read("EmpID"), CryptoKey);
        EmpPw = stringEncrypt.AesDecryptBase64(cookies.Read("EmpPW"), CryptoKey);
        IsEmp = ChkEmp(EmpId, EmpPw);
    }

    /// <summary>
    /// 登入初始化
    /// </summary>
    /// <param name="_EmpId"></param>
    /// <param name="_EmpPw"></param>
    /// <returns></returns>
    //public string LoginInit(string _EmpId, string _EmpPw)
    //{
    //    _EmpId = _EmpId.ToLower();  //帳號一律小寫
    //    _EmpPw = _EmpPw.ToLower();  //密碼一律小寫
    //    if (!ChkEmp(_EmpId, _EmpPw)) { return "empty"; }
    //    else
    //    {
    //        try
    //        {
    //            string SqlComm = $"SELECT * FROM WP_Employee WHERE LOWER(empId)='{_EmpId}' AND LOWER(empPw)='{_EmpPw}' AND isStop='N'";
    //            EmpTbl = getTbl.table("WP", SqlComm);

    //            cookies.Update("EmpID", stringEncrypt.AesEncryptBase64(_EmpId, CryptoKey), "10");
    //            cookies.Update("EmpPW", stringEncrypt.AesEncryptBase64(_EmpPw, CryptoKey), "10");
    //            cookies.Update("EmpName", stringEncrypt.AesEncryptBase64(EmpTbl.Rows[0]["empName"].ToString(), CryptoKey), "10");

    //            //SysLogWrite("Login");
    //            return "Y";
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            //Response.Write(ex.Message);
    //            return ex.Message;
    //        }
    //    }
    //}

    public bool LoginInit(string _EmpId, string _EmpPw)
    {
        _EmpId = _EmpId.ToLower();  //帳號一律小寫
        _EmpPw = _EmpPw.ToLower();  //密碼一律小寫
        if (!ChkEmp(_EmpId, _EmpPw)) { return false; }
        else
        {
            try
            {
                string SqlComm = $"SELECT * FROM WP_Employee WHERE LOWER(empId)='{_EmpId}' AND LOWER(empPw)='{_EmpPw}' AND isStop='N'";
                EmpTbl = getTbl.table("WP", SqlComm);

                cookies.Update("EmpID", stringEncrypt.AesEncryptBase64(_EmpId, CryptoKey), "10");
                cookies.Update("EmpPW", stringEncrypt.AesEncryptBase64(_EmpPw, CryptoKey), "10");
                cookies.Update("EmpName", stringEncrypt.AesEncryptBase64(EmpTbl.Rows[0]["empName"].ToString(), CryptoKey), "10");

                SysLogWrite("Login", "");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                //Response.Write(ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// 登出
    /// </summary>
    public void Logout()
    {
        SysLogWrite("Logout", "");
        cookies.Clear("EmpID,EmpPW,EmpName,sDate,eDate,pvSn,pName,isTax");    //sDate,eDate,pvSn,pName,isTax 3002使用
        EmpId = "";
        EmpPw = "";
        IsEmp = false;
    }

    public void SysLogWrite(string _logType, string _memo)
    {
        EmpId = stringEncrypt.AesDecryptBase64(cookies.Read("EmpID"), CryptoKey);
        string SqlComm = $"INSERT INTO WP_SysLog (empId, logType, memo) VALUES ('{EmpId}', '{_logType}', '{_memo}')";
        getTbl.updTbl("WP", SqlComm);

    }

    public bool ChkPrg(string _prgId)
    {
        if (!IsEmp) { return false; }
        else
        {
            string SqlComm = $"SELECT * FROM WP_PrgMenu WHERE prgId='{_prgId}' AND Exist='Y'";
            PrgTbl = getTbl.table("WP", SqlComm);
            return PrgTbl.Rows.Count == 0 ? false : EmpId == "9999" ? true : ((IList)EmpPrgIdGrp.Split(',')).Contains(_prgId);
        }
    }

    private bool ChkEmp(string _empId, string _empPw)    {
        
        if (_empId == "" || _empPw == "") { return false; }
        else
        {
            string _EmpId = _empId.ToLower(),  //帳號一律小寫
                   _EmpPw = _empPw.ToLower();  //密碼一律小寫

            string SqlComm = $"SELECT * FROM WP_vEmployee WHERE LOWER(EmpId)='{_EmpId}' AND isStop='N'";
            EmpTbl = getTbl.table("WP", SqlComm);

            if (EmpTbl.Rows.Count > 0 && EmpTbl.Rows[0]["EmpPW"].ToString().ToLower() == _EmpPw)
            {
                DataRow Row0 = EmpTbl.Rows[0];
                EmpIsAdmin = Row0["isAdmin"].ToString();
                EmpPrgIdGrp = Row0["empPrgIdGrp"].ToString();
                EmpGrpId = Row0["empGrpId"].ToString();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}



