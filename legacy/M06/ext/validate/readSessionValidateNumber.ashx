<%@ WebHandler Language="C#" Class="readSessionValidateNumber" %>

using System;
using System.Web;
//如何於泛型處理常式 .ashx 中存取工作階段變數(Session Variable)
using System.Web.SessionState;
//參考：http://www.limingchstudio.com/2009/05/ashx-session-variable.html

public class readSessionValidateNumber : IHttpHandler, IRequiresSessionState
{

    public void ProcessRequest (HttpContext context) {
        string Id = context.Request.QueryString["id"];
        context.Response.ContentType = "text/plain";
        string ValidateNumber = (string)context.Session["ValidateNumber" + Id];
        context.Response.Write(ValidateNumber);
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}