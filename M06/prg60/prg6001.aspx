<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg6001.aspx.cs" Inherits="prg6001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .chk-main {border:2px #000 solid;border-radius:3px;padding:20px 14px;font-size:18px;font-weight:bold;}
        .chk-main div, .chk-main span, .chk-main label {font:inherit;}
        .chk-main > div, .chk-main > div div {display:flex;align-items:center;}
        #btn-unSubmit{margin-left:10px;background-color:#FF7A7A;color:#fff!important;border:1px #fee solid;}
        #btn-unSubmit:hover{background-color:#FFD1D1;color:#f00!important;}
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
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $(".chkoutYN").click(function () {
                if ($(this).val() == "Y") {
                    $(".chkoutY-main").show();
                    $(".chkoutN-main").hide();
                } else {
                    $(".chkoutY-main").hide();
                    $(".chkoutN-main").show();
                }
            });

            //日結確定送出
            $("#btn-submit").click(function () {
                if (confirm("確定結帳？")) {
                    $("#processing").show();
                    $(this).attr("disabled", true);
                    setTimeout(function () {
                        $.ajax({
                            url: "/AjaxTrace.aspx",
                            type: "POST",
                            async: false,
                            data: {
                                args0: "T01",
                                sYMD: $("#sYMD").val()
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
                                        alert("非管理者不能結帳！");
                                        break;
                                    case "Y":
                                        alert("結帳成功！");

                                        $.ajax({
                                            url: "/AjaxTrace.aspx",
                                            type: "POST",
                                            async: false,
                                            data: {
                                                args0: "T02",
                                                sYMD: $("#sYMD").val()
                                            },
                                            error: function (xhr) {
                                                alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                                            },
                                            success: function (response) {
                                                if (response == "") {
                                                    location.reload();
                                                }
                                                else {
                                                    console.log(response);
                                                    $("#result").html("<div style='font-weight:bold;margin-top:20px;font-size:20px;'>" + ("庫存為負的商品︰") +
                                                        "</div><div class='empty-data'><i class='fas fa-exclamation-triangle'></i> 請確認進銷貨是否有誤！確認完畢請記得刷新畫面(請按下F5)</div>"
                                                        + response);
                                                }
                                            }
                                        });
                                        //location.reload();
                                        break;
                                    default:
                                        console.log(response);
                                        alert("結帳失敗請聯絡工程人員！");
                                }
                            },
                            complete: function (data) {
                                $(this).attr("disabled", false);
                                $("#processing").hide();
                            }
                        }, 500);
                    });
                }
            });

            //取消日結確定送出
            $("#btn-unSubmit").click(function () {
                if (confirm("確定取消結帳？")) {
                    $("#processing").show();
                    $(this).attr("disabled", true);
                    setTimeout(function () {
                        $.ajax({
                            url: "/AjaxTrace.aspx",
                            type: "POST",
                            async: false,
                            data: {
                                args0: "T01_1",
                                uYMD: $("#uYMD").val()
                            },
                            error: function (xhr) {
                                console.log(xhr.responseText);
                                alert('Ajax request 發生錯誤--btn-unSubmit.click--請洽工程人員');
                            },
                            success: function (response) {
                                //console.log(response);
                                switch (response) {
                                    case "not-emp":
                                        location.replace("/login.aspx");
                                        break;
                                    case "UA":
                                        alert("非管理者不能取銷結帳！");
                                        break;
                                    case "Y":
                                        alert("取消結帳成功！");
                                        location.reload();
                                        break;
                                    default:
                                        console.log(response);
                                        alert("取消結帳失敗請聯絡工程人員！");
                                }
                            },
                            complete: function (data) {
                                $(this).attr("disabled", false);
                                $("#processing").hide();
                            }
                        }, 500);
                    });
                }
            });
        });
    </script>
</asp:Content>
