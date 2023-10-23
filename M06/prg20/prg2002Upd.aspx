<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2002Upd.aspx.cs" Inherits="prg2002Upd" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <article>
        <div class="page-body prg-menu">
            <uc1:inc_Prg runat="server" ID="inc_Prg" />
        </div>
        <div class="page-body prg-content">
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <asp:Label ID="Label2" runat="server"></asp:Label>
            <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回廠商列表</button>
                <button class="btn-submit btn-page btn-admin" id="btn-upd"><i class="fas fa-check-circle"></i> 修改資料</button>
                <button class="btn-abort btn-page" id="btn-upd-abort"><i class="fas fa-times-circle"></i> 放棄修改</button>
                <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pv-upd-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/ext/cityChg/cityChg.js"></script>
    <script>
        function ChkFinish(_id) {
            return noneEmpty(_id);
        }

        function Initial() {
            $(".pv-upd-main").find(":input").attr("disabled", true);
            $("#btn-upd-abort, #btn-submit").hide();
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-upd").click(function () {
                $(".pv-upd-main").find(":input").attr("disabled", false);
                $(this).hide();
                $("#btn-upd-abort, #btn-submit").show();
            });

            $("#btn-upd-abort").click(function () {
                location.reload();
            });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    var data = "args0=C06A&" + $obj.find(":input").serialize();
                    $.ajax({
                        url: "/AjaxMain.aspx",
                        type: "POST",
                        async: false,
                        data: data,
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                        },
                        success: function (response) {
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能修改！");
                                    break;
                                case "A":
                                    error_focus("商品編號已存在，請重新輸入編號！", $("[name='pvId']"));
                                    break;
                                case "Y":
                                    alert("修改完成！");
                                    location.reload();
                                    break;
                                default:
                                    console.log(response);
                                    alert("修改失敗請聯絡工程人員！");
                            }
                        }
                    });
                }

            });

        });
    </script>
</asp:Content>
