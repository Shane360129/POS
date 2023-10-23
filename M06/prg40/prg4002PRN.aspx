<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeFile="prg4002PRN.aspx.cs" Inherits="prg4002PRN" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="content-language" content="zh-tw" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1.0, user-scalable=no" />
    <link rel="icon" href="/images/logo/logo.ico" type="image/x-icon" />
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />

    <link href="/ext/css/main.css" rel="stylesheet" />

    <title>水里鄉農會POS系統</title>
    <meta name="description" content="水里鄉農會POS系統" />
    <meta name="keywords" content="水里鄉農會POS系統" />
    <style>
        div, span {font-family:新細明體!important;}
        .rptr-title {font-size:18px!important;font-weight:bold;margin-bottom:10px;text-align:center;text-decoration:underline;text-underline-offset:3.5px;}
        .inStk, .inStk-tbl-reptr span {font-size:14px;}
        .inStk div, .inStk span {font-size:inherit;}
        .inStk-title > div > div {display:inline-block;width:33%;}
        .inStk-title > div > div:nth-child(3) {text-align:right;}
        .inStk-Tbl {font-size:13px!important;}
        .inStk-Tbl {border-collapse:collapse;width:100%;}
        .inStk-tbl-title, .inStk-tbl-footer {border-top:2px #000 solid;border-bottom:2px #000 solid;}
        .inStk-Tbl td {border-bottom:1px #000 solid;padding:2px;border-top:0;white-space:nowrap;overflow:hidden;}
        .inStk-Tbl td:nth-child(1) {text-align:right;max-width:7px;padding-right:4px;padding-left:0;}
        .inStk-Tbl td:nth-child(2) {max-width:50px;}
        .inStk-Tbl td:nth-child(4) {max-width:10px;min-width:10px;text-align:center;}
        .inStk-Tbl td:nth-child(n+5):nth-child(-n+8) {text-align:right;max-width:20px;}
        .inStk-tbl-reptr {margin-top:5px;}
        .inStk-tbl-reptr .rptr-cell:not(:last-child) {margin-right:82px;padding-top:10px;}
        
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <div class="page-main pd-list-main">
                <asp:Label ID="Label2" runat="server"></asp:Label>
            </div>
        </div>
    </form>

    <script src="/Scripts/jquery-3.6.0.min.js"></script>
    <script src="/ext/js/main.js"></script>
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            }
        };


        $(function () {
            $(".page-total").text($("#pageTotal").val());
            setTimeout(function () { window.print();self.close(); }, 500);

        });
    </script>
</body>
</html>
