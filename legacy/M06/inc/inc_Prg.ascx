<%@ Control Language="C#" ClassName="AdminLeft" AutoEventWireup="true" CodeFile="inc_Prg.ascx.cs" Inherits="inc_inc_Prg" %>
<style>
    .pageL-title {background-color:#3DBA8E;font-size:18px;color:#FFF;text-align:center;font-weight:bold;padding:3px;border-bottom:3px #ADD599 solid;border-radius:5px 5px 0 0;}
    .pageL-detail {margin-bottom:25px;}
    .detail-link {border-bottom: 2px #A5A5A5 dashed;padding:5px;}

    a:hover, a > div:hover{
        color: #629062;
        text-decoration: none;
    }
</style>
<asp:Label ID="Label1" runat="server"></asp:Label>

<script src="/Scripts/jquery-3.6.0.min.js"></script>
<script type="text/javascript">
    function empLogout() {
        $.ajax({
            url: "/AjaxMain.aspx",
            type: "POST",
            async: false,
            data: { args0: 'B01' },
            error: function (xhr) {
                console.log(xhr.responseText);
                alert('Ajax request 發生錯誤--empLogout()--請洽工程人員');
            },
            success: function (response) {
                if (response == "Y") {
                    alert("已登出！");
                    location.replace("/login.aspx");
                } else {
                    alert("登出失敗，請聯絡工程人員！");
                }
            }
        });
    }

    $(function () {
        $(".pageL-detail").each(function () {
            $(this).text() == "" && $(this).closest(".pageL-node").hide();            
        });
    });


</script>