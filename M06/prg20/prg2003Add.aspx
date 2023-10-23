<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2003Add.aspx.cs" Inherits="prg2003Add" %>

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
            <div class="page-main mem-add-main">
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">會員分類</div>
                    <asp:Label ID="Label2" runat="server" Style="margin-right:94px;"></asp:Label>
                    <div class="input-text">
                        <span class="input-tag">狀態︰</span>
                        <select name="isStop" class="form-control" style="margin-right:115px;"><option value="N">正常往來</option><option value="Y">停止往來</option></select>
                        <span class="input-tag">預設售價︰</span>
                        <select name="priceKind" class="form-control"><option value="1">標準售價</option><option value="2">最低應售價</option><option value="3">會員價</option><option value="4">大批價</option></select>
                    </div>
                </div>
                <div class="input-row">
                    <div class="input-title">會員資料</div>
                    <div class="input-text">
                        <div style="display:flex;align-items:center;">
                            <span class="input-tag">會員編號︰</span>
                            <input type="text" id="memId" name="memId" class="form-control" data-alt="MI_會員編號" maxlength="10" style="width:109px;margin-left:4px;margin-right:29px;" />
                            <span class="input-tag">姓名︰</span>
                            <input type="text" name="memName" class="form-control" data-alt="MI_會員姓名" maxlength="50" style="width:260px;margin-left:4px;margin-right:175px;" />
                        </div>
                        <div style="display: flex; align-items: center; margin-top: 10px;">
                            <span class="input-tag">性別︰</span>
                            <input type="radio" name="gender" id="genderF" class="form-control" value="F" checked style="margin-left: 2px;" /><label for="genderF">女</label>
                            <input type="radio" name="gender" id="genderM" class="form-control" value="M" /><label for="genderM" style="margin-right:78px;">男</label>
                            <span class="input-tag">生日︰</span>
                            <input type='text' class='form-control open-datepicker' name='birthday' size='10' readonly style='width:auto;margin-left:4px;margin-right:93px;' />
                            <span class="input-tag">身份證字號︰</span>
                            <input type="text" name="idNo" maxlength="10" class="form-control" style="width:118px;margin-left:4px;" />
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">聯絡資訊</div>
                    <div class="input-text">
                        <div>
                            <span class="input-tag">電話︰</span>
                            <input type="text" name="memTel" maxlength="40" class="form-control" style="width:259px;margin-right:63px;" />
                            <span class="input-tag">手機︰</span>
                            <input type="text" name="memMobil" maxlength="40" class="form-control" style="width:259px;" />
                        </div>
                        <div style="display:flex;align-items:center;margin-top:10px;">
                            <span class="input-tag">傳真︰</span>
                            <input type="text" name="memFax" maxlength="40" class="form-control" style="width:259px;margin-right:57px;margin-left:4px;" />
                            <span class="input-tag">Email︰</span>
                            <input type="text" name="memEmail" maxlength="50" class="form-control" style="width:259px;margin-left:4px;" />
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">通訊地址</div>
                    <div class="input-text">
                        <div class="city-change">
                            <select class="form-control city-id" name="memCityId"></select>
                            <select class="form-control zone-id" name="memZoneId"></select>
                            <input type="text" name="memAddr" maxlength="150" class="form-control" style="width:469px; margin-right: 67px;" />
                        </div>
                    </div>
                </div>
                <div class="input-row" style="align-items: flex-start;">
                    <div class="input-title">送貨地址</div>
                    <div class="input-text">
                        <div style="height:33px;display:flex;align-items:center;">
                            <span class="input-tag">同通訊地址︰</span>
                            <input type="radio" name="dittoMemAddr" id="dittoMemAddrY" class="form-control dittoMemAddr" value="Y" checked style="margin-left:2px;"/><label for="dittoMemAddrY">是</label>
                            <input type="radio" name="dittoMemAddr" id="dittoMemAddrN" class="form-control dittoMemAddr" value="N" /><label for="dittoMemAddrN">否</label>
                        </div>
                        <div class="city-change" id="ord-addr-main" style="margin-top:8px;display:none;">
                            <select class="form-control city-id" name="ordCityId"></select>
                            <select class="form-control zone-id" name="ordZoneId"></select>
                            <input type="text" name="ordAddr" maxlength="150" class="form-control" style="width:469px; margin-right: 67px;" />
                        </div>
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
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回會員列表</button>
                <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="mem-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定新增</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/ext/cityChg/cityChg.js"></script>
    <script>
        //設定日期選擇器為中文語系
        $.datepicker.regional['zh-TW'] = {
            dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
            dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"],
            monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            monthNamesShort: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            prevText: "上月",
            nextText: "次月",
            weekHeader: "週"
        };
        $.datepicker.setDefaults($.datepicker.regional["zh-TW"]);

        function ChkFinish(_id) {
            if ($("[name='memTel']").val() == "" && $("[name='memMobil']").val() == ""){ return error_focus("電話及手機請至少擇一輸入！", $("[name='memTel']")); }
            return noneEmpty(_id);
        }

        function Initial() {
            //$(".pvk-input-main").hide();
        }



        $(function () {
            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-100:+0"
            });

            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    var data = "args0=C07&" + $obj.find(":input").serialize();
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
                                    error_focus("會員編號已存在，請重新輸入！", $("#memId"));
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg20/prg2003.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
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
