<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2002Add.aspx.cs" Inherits="prg2002Add" %>

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
            <div class="page-main pv-add-main">
                <div class="input-row">
                    <div class="input-title">編　　號</div>
                    <div class="input-text">
                        <input type="text" name="pvId" class="form-control" data-alt="MI_廠商編號" maxlength="10" style="width:109px;" />
                    </div>
                </div>
                <div class="input-row">
                    <div class="input-title">名　　稱</div>
                    <div class="input-text">
                        <span class="input-tag">全名︰</span>
                        <input type="text" name="pvName" class="form-control" data-alt="MI_廠商全名" maxlength="50" style="width: 335px; margin-right: 20px;" />
                        <span class="input-tag">簡稱︰</span>
                        <input type="text" name="pvNameS" class="form-control" data-alt="MI_廠商簡稱" maxlength="10" style="width: 227px;" />
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">分　　類</div>
                    <div class="input-text">
                        <asp:Label ID="Label2" runat="server" Style="margin-right: 35px;"></asp:Label>
                        <span class="input-tag">狀態︰</span>
                        <select name="isStop" class="form-control">
                            <option value="N">正常往來</option>
                            <option value="Y">停止往來</option>
                        </select>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">公　　司</div>
                    <div class="input-text">
                        <div>
                            <span class="input-tag">負責人︰</span>
                            <input type="text" name="pvBoss" maxlength="50" class="form-control" style="width: 230px; margin-right: 35px;" />
                            <span class="input-tag">電話︰</span>
                            <input type="text" name="pvTel" maxlength="50" class="form-control" style="width: 300px; margin-right: 67px;" />
                        </div>
                        <div style="display:flex;align-items:center;margin-top:10px;">
                            <span class="input-tag">傳　真︰</span>
                            <input type="text" name="fax" maxlength="50" class="form-control" style="width: 230px; margin-right:33px;margin-left:4px;" />
                            <span class="input-tag">Email︰</span>
                            <input type="text" name="email" maxlength="50" class="form-control" style="width: 300px; margin-right: 67px;" />
                        </div>
                        <div style="display:flex;align-items:center;margin-top:10px;">
                            <span class="input-tag">地　址︰</span>
                            <div class="city-change" style="margin-left:4px;">
                                <select class="form-control city-id" name="pvCityId"></select>
                                <select class="form-control zone-id" name="pvZoneId"></select>
                                <input type="text" name="pvAddr" maxlength="150" class="form-control" style="width: 402px; margin-right: 67px;" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">聯絡人員</div>
                    <div class="input-text">
                        <div>
                            <span class="input-tag">姓　名︰</span>
                            <input type="text" name="ctactName" data-alt="MI_聯絡人姓名" maxlength="50" class="form-control" style="width: 230px; margin-right: 35px;" />
                            <span class="input-tag">電話︰</span>
                            <input type="text" name="ctactTel" data-alt="MI_聯絡人電話" maxlength="50" class="form-control" style="width: 300px; margin-right: 67px;" />
                        </div>
                        <div style="display:flex;align-items:center;margin-top:10px;">
                            <div class="input-tag">地　址︰</div>
                            <div class="input-text">
                                <div class="city-change" style="margin-left:4px;">
                                    <select class="form-control city-id" name="ctactCityId"></select>
                                    <select class="form-control zone-id" name="ctactZoneId"></select>
                                    <input type="text" name="ctactAddr" maxlength="150" class="form-control" style="width: 402px; margin-right: 67px;" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">發　　票</div>
                    <div class="input-text">
                        <span class="input-tag">統一編號︰</span>
                        <input type="text" name="taxId" maxlength="8" class="form-control" style="width: 100px; margin-right: 35px;" />
                        <span class="input-tag">發票抬頭︰</span>
                        <input type="text" name="invoTitle" maxlength="80" class="form-control" style="width: 381px; margin-right: 67px;" />
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">往來銀行</div>
                    <div class="input-text">
                        <div>
                            <span class="input-tag">銀行代號︰</span>
                            <input type="text" name="bankId" maxlength="20" class="form-control" style="width: 100px; margin-right: 35px;" />
                            <span class="input-tag">銀行名稱︰</span>
                            <input type="text" name="bankName" maxlength="50" class="form-control" style="width: 381px; margin-right: 67px;" />
                        </div>
                        <div style="display:flex;align-items:center;margin-top:10px;">
                            <span class="input-tag">銀行帳號︰</span>
                            <input type="text" name="bankAccount" maxlength="50" class="form-control" style="width: 180px; margin-right: 35px;margin-left:4px;" />
                            <span class="input-tag">戶　　名︰</span>
                            <input type="text" name="bankAcctName" maxlength="80" class="form-control" style="width: 309px; margin-right: 67px;" />
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">備　　註</div>
                    <div class="input-text">
                        <textarea name="memo" maxlength="500" class="form-control" rows="2" style="width:688px;height:60px;"></textarea>
                    </div>
                </div>
            </div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回廠商列表</button>
                <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pv-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定新增</button>
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
            //$(".pvk-input-main").hide();
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    var data = "args0=C06&" + $obj.find(":input").serialize();
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
                                case "A":
                                    error_focus("商品編號已存在，請重新輸入編號！", $("[name='pvId']"));
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg20/prg2002.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }

            });

        });
    </script>
</asp:Content>
