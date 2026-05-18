<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2004Add.aspx.cs" Inherits="prg2004Add" %>

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
            <div class="page-main emp-add-main">
                <div class="input-row">
                    <div class="input-title">登入帳號</div>
                    <div class="input-text" style="display:flex;align-items:center;">
                        <input type="text" name="empId" class="form-control" data-alt="MI_登入帳號" maxlength="10" style="width:112px;margin-left:4px; margin-right:35px;" />
                        <div class="emp-pw">
                            <span class="input-tag">密碼︰</span>
                            <input type="password" name="empPw" class="form-control" data-alt="MI_密碼" maxlength="10" style="width:112px;margin-left:4px;margin-right:35px;" />
                        </div>
                        <div style="display:flex; align-items:center;">
                            <span class="input-tag">停用︰</span>
                            <input type="radio" name="isStop" id="isStopN" class="form-control" value="N" checked style="margin-left: 2px;" /><label for="isStopN">否</label>
                            <input type="radio" name="isStop" id="isStopY" class="form-control" value="Y" /><label for="isStopY">是</label>
                        </div>
                    </div>
                </div>
                <div class="input-row" style="">
                    <div class="input-title">帳號名稱</div>
                    <div class="input-text">
                        <input type="text" name="empName" class="form-control" data-alt="MI_帳號名稱" maxlength="50" style="width:260px;margin-left:4px;margin-right:74px;" />
                    </div>
                </div>
                <div class="input-row" style="align-items:center;">
                    <div class="input-title">管理人員</div>
                    <div class="input-text" style="display:flex;align-items:center;">
                        <input type="radio" name="isAdmin" id="isAdminN" class="form-control" value="N" checked style="margin-left: 2px;" /><label for="isAdminN">否</label>
                        <input type="radio" name="isAdmin" id="isAdminY" class="form-control" value="Y" /><label for="isAdminY">是</label>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">群　　組</div>
                    <div class="input-text">
                        <asp:Label ID="Label2" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">備　　註</div>
                    <div class="input-text">
                        <textarea name="memo" maxlength="500" class="form-control" rows="2" style="width: 688px;"></textarea>
                    </div>
                </div>
            </div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回帳號列表</button>
                <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="emp-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定新增</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script>
        function ChkFinish(_id) {
            if ($("[name='memTel']").val() == "" && $("[name='memMobil']").val() == ""){ return error_focus("電話及手機請至少擇一輸入！", $("[name='memTel']")); }
            return noneEmpty(_id);
        }

        function Initial() {
            //$(".pvk-input-main").hide();
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    var data = "args0=C08&" + $obj.find(":input").serialize();
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
                                    alert("非管理者不能新增！");
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg20/prg2004.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
                                    break;
                            }
                        }
                    });
                }

            });

            $(".dittoMemAddr").click(function () {
                if ($(this).val() == "Y") { $("#ord-addr-main").hide(); } else { $("#ord-addr-main").show(); };
            });

        });
    </script>
</asp:Content>
