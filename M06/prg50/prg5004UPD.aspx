<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5004UPD.aspx.cs" Inherits="prg5004UPD" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .foot-total td {text-align:right;border-top:1px #000 solid;border-bottom:0;font-weight:bold;}
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

        function pdCalc() {
            var totalDiscnt = parseInt($("#total-discnt").attr("data-val"));
            totalDiscnt == 0 ? $("#total-discnt").hide() : $("#total-discnt").show();
            var totalDtlDiscnt = parseInt($("#total-dtlDiscnt").attr("data-val"));
            totalDtlDiscnt == 0 ? $("#total-dtlDiscnt").hide() : $("#total-dtlDiscnt").show();
            (totalDiscnt == 0 && totalDtlDiscnt == 0) ? $("#total-discnt-row").hide() : $("#total-discnt-row").show();
        }

        function Initial() {
            $(".row-main[data-id='']").text("").css("border-top", "0");
            pdCalc();
        }



        $(function () {
            Initial();

            //整筆刪除
            $("#btn-acctIn-del").click(function () {
                if (confirm("確定整筆刪除？")) {
                    $.ajax({
                        url: "/AjaxOutStk.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: "C14B",
                            sn: $("#sn").val()
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-acctIn-del.click--請洽工程人員');
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
                                    location.href = "/prg50/prg5004.aspx";
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
