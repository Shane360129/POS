<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg800A.aspx.cs" Inherits="prg800A" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
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
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //確定送出銷貨單
            $("#btn-submit").click(function () {
                $.ajax({
                    url: "/AjaxTrace.aspx",
                    type: "POST",
                    async: false,
                    data: {
                        args0: "T00"
                    },
                    error: function (xhr) {
                        console.log(xhr.responseText);
                        //alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                    },
                    success: function (response) {
                        console.log(response);
                        $("#result").html("<div style='font-weight:bold;margin-top:20px;font-size:20px;'>" + (response == "" ? "無庫存不符商品！" : "庫存為負商品︰") + "</div>" + response);
                    }
                });
            });
        });
    </script>
</asp:Content>
