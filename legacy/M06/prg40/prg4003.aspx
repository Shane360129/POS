<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg4003.aspx.cs" Inherits="prg4003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .chk-main {border:2px #000 solid;border-radius:3px;padding:20px 14px;font-size:18px;font-weight:bold;}
        .chk-main div, .chk-main span, .chk-main label {font:inherit;}
        .chk-main > div, .chk-main > div div {display:flex;align-items:center;}
        #btn-unSubmit{margin-left:10px;background-color:#FFD1D1;color:#f00!important;border:1px #fee solid;}
        #btn-unSubmit:hover{background-color:#FF7A7A;color:#fff!important;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回上一頁</button>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            }
        };

        function Initial() {
            $("#processing").hide();
            if ($("#isEmpty").val() == "Y") { $("#btn-submit").attr("disabled", true);}
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //接收確定送出
            $("#btn-submit").click(function () {
                $("#processing").show();
                $(this).attr("disabled", true);
                setTimeout(function () {
                    $.ajax({
                        url: "/AjaxOutStk.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: "C13"
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                        },
                        success: function (response) {
                            //console.log(response);
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能接收資料！");
                                    break;
                                case "empty":
                                    alert("查無前台銷售資料，請確認前台是否已上傳資料！");
                                    break;
                                case "Y":
                                    alert("資料接收完成！");
                                    location.reload();
                                    break;
                                default:
                                    console.log(response);
                                    alert("資料接收失敗請聯絡工程人員！");
                            }
                        },
                        complete: function (data) {
                            $(this).attr("disabled", false);
                            $("#processing").hide();
                        }
                    }, 500);
                });
                
            });
        });
    </script>
</asp:Content>
