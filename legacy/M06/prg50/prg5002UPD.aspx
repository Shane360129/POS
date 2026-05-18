<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5002UPD.aspx.cs" Inherits="prg5002UPD" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .search-main {border:1px #080 solid;border-radius:3px;margin-bottom:20px;padding:20px 10px;padding-bottom:0;}
        .search-main > div {display:flex;align-items:center;flex-wrap:wrap;margin-bottom:10px;}
        .search-main .search-sub {display:flex;align-items:center;margin-bottom:10px;margin-right:40px;}

        .list-main {width:100%;margin-top:20px;}
        .list-main td {border-top:2px #c9c9c9 dotted;padding:5px;border-right:1px #fff solid;}
        .list-main .list-title td {border-top:0;background-color:#3DBA8E;color:#fff;font-weight:bold;}
        .list-main tr:last-child td {border-bottom:2px #c9c9c9 dotted;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main pd-list-main">            
            <asp:Label ID="Label2" runat="server"></asp:Label>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            }
        };

        //計算商品小計及總計
        function pdCalc() {
            var pdTotal, listPdTotal = 0;
            $(".inStk-list-main").find(".inStkAmtTotal").each(function () {
                listPdTotal += parseFloat($(this).text());
            });
            $("#acctOut-total").text(listPdTotal.toFixed(2));
        }

        function Initial() {
            $(".row-main[data-id='']").text("").css("border-top", "0");
            pdCalc();
        }



        $(function () {
            Initial();

            //整筆刪除
            $("#btn-acctOut-del").click(function () {
                if (confirm("確定整筆刪除？")) {
                    $.ajax({
                        url: "/AjaxInStk.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: "C13B",
                            sn: $("#sn").val()
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-acctOut-del.click--請洽工程人員');
                        },
                        success: function (response) {
                            //console.log(response);
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能刪除！");
                                    break;
                                case "Y":
                                    alert("刪除成功！");
                                    location.href = "/prg50/prg5002.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("刪除失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
            });
        });
    </script>
</asp:Content>
