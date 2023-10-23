<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg3001.aspx.cs" Inherits="prg3001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="/Content/jquery-ui.css" rel="stylesheet" />
    <link href="/Content/jquery-ui.theme.css" rel="stylesheet" />
    <style>
        .input-main {border: 2px #000 solid;border-radius: 3px;padding: 20px 14px;padding-bottom: 0;display: flex;flex-wrap: wrap;}
        .input-sub {width: auto;margin-bottom: 20px;}
        #back-inStk:disabled{cursor:not-allowed;}

        .pd-row {display: flex;align-items: center;}
        .pd-row:not(:nth-child(1)) {border-top: 1px #c9c9c9 dashed;}
            .pd-row > div {width: 11.5%;padding: 5px;}
                .pd-row > div:not(.align-c) {text-align: right;}

        .pd-row-title > div {background-color: #3DBA8E;color: #fff;margin-right: 1px;}
        .pd-row-title > div:last-child {margin-right: 0;}
        .pd-row > div:first-child {width: 25%;text-align: left;}
        .pd-row > div:nth-child(2), .pd-row > div:nth-child(3) {width: 14.5%;}
        .pd-row > div:first-child {width: 25%;text-align: left;}

        .pd-list-total {text-align: right;margin-right: 31%;font-weight: bold;padding: 5px;color: #080;}
        .pd-list-total span {font-size: 18px;color: #080;}

        .edit-single-main {justify-content: flex-end;padding-right: 5px;margin-left: 0;}
        .edit-zone {z-index: 5;background-color: #fff;margin-right: -125px;}
        .prg-content .pd-del {font-size: 15px;}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <article>
        <div class="page-body prg-menu">
            <uc1:inc_Prg runat="server" ID="inc_Prg" />
        </div>
        <div class="page-body prg-content">
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <asp:Label ID="Label2" runat="server"></asp:Label>
            <div class="page-main inStk-add-main" style="margin-top: 10px;">
                <div id="add-pd-main">
                    <span style="font-weight: bold; font-size: 18px;"><i class="fas fa-caret-square-right"></i>進貨商品︰</span>
                    <div class="del-txt-group">
                        <input type="text" class="form-control del-txt-input" placeholder="請輸入商品名稱或條碼" maxlength="42" style="width: 266px;" disabled id="pd-filter" />
                        <div class="del-txt-button"><i class="fas fa-times-circle"></i></div>
                        <input type="hidden" id="act-pNo" />
                    </div>
                    <button id="pd-add-submit" class="btn-submit btn-admin" disabled style="margin-left: 10px;"><i class="fas fa-check-circle"></i>加入清單</button>
                    <input type="checkbox" id="back-inStk" /><label for="back-inStk" id="lbl-backStk">退貨</label>
                </div>
                <div style="border-radius: 3px; border: 1px #080 solid;">
                    <div class="pd-row pd-row-title">
                        <div>商品名稱</div>
                        <div>含稅單價</div>
                        <div>數量</div>
                        <div>單位</div>
                        <div>小計</div>
                        <div class="align-c">稅別</div>
                        <div class="align-c">保存期限</div>
                        <div class="align-c">刪除</div>
                    </div>
                    <div class="inStkPd-main">
                        <div class="empty-pd empty-data">尚無商品！</div>
                        <div id="pd-list-main"></div>
                    </div>
                </div>
                <div class="pd-list-total"><span>總計︰</span><span id="inStk-total">0</span></div>
                <div id="pd-append" style="display: none;">
                    <div class="pd-row">
                        <div class="pNameS"></div>
                        <div class="costStd edit-single-main" data-id="costStd" style="justify-content: flex-end;">
                            <div class="edit-tag"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                            <div class="edit-zone" style="display: none;">
                                <input type="text" class="form-control align-r edit-txt edit-costStd chk-input" data-func="real_number" data-alt="1" maxlength="10" style="width: 103px;" value="1">
                                <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                                <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                            </div>
                        </div>
                        <div class="pd-qty edit-single-main" data-id="pd-qty" style="justify-content: flex-end;">
                            <div class="edit-tag"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                            <div class="edit-zone" style="display: none;">
                                <input type="text" class="form-control align-r edit-txt edit-qty chk-input" data-func="number" data-alt="1" maxlength="6" style="width: 70px;" value="1">
                                <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                                <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                            </div>
                        </div>
                        <div class="pUName"></div>
                        <div class="subTotal edit-single-main" data-id="subTotal" style="justify-content: flex-end;">
                            <div class="edit-tag"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                            <div class="edit-zone" style="display: none;">
                                <input type="text" class="form-control align-r edit-txt edit-subTotal chk-input" data-func="number" data-alt="1" maxlength="10" style="width: 70px;" value="1">
                                <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                                <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                            </div>
                        </div>
                        <div class="align-c isTax"></div>
                        <div class="align-c">
                            <input type="text" class="form-control open-datepicker pdLimitDate" readonly /></div>
                        <div class="align-c">
                            <button class="btn-del pd-del"><i class='fas fa-trash'></i>刪除</button></div>
                    </div>
                </div>
            </div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i>回上一頁</button>
                <button class="btn-submit btn-page btn-admin" id="btn-add-inStk" data-alt="pd-add-main"><i class="fas fa-arrow-alt-circle-right"></i>確定送出</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        var pointQty = parseInt($("#pointQty").val());
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
        $('#InStkDate').datepicker({        //啟動進貨日期日曆
            dateFormat: 'yy/mm/dd',
            changeYear: true,
            changeMonth: true,
            yearRange: "-100:+1",
            minDate: new Date($("#minDate").val().substr(0, 4), $("#minDate").val().substr(4, 2) - 1, $("#minDate").val().substr(6, 2))
        });

        function datepicker(_$obj) {        //啟動保存期限日曆
            _$obj.datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-5:+5"
            });
        }

        function intlPdNew(_$obj, _$JSON) {        //新增商品後，初始化商品資料
            var back = $("#back-inStk").prop("checked");        //是否為進貨退回
            _$obj.attr("data-id", _$JSON["pNo"]).attr("data-back", back);
            _$obj.find(".pNameS").html((back ? "<span class='back-dot'>退</span>" : "") +
                _$JSON["pNameS"] + "(" + _$JSON["pBarcode"] + ")" +
                ($("#pvSn").val() == _$JSON["pvSn"] ? "" : ("<span class='diff-pv' style='white-space:nowrap'>(" + _$JSON["pvNameS"] + ")</span>")));
            var costStd = accurateDecimal(_$JSON["costStd"], pointQty, "Y");
            _$obj.find(".costStd span").text(costStd);
            _$obj.find(".costStd .edit-txt").val(costStd);
            _$obj.find(".costStd .edit-txt").attr("data-alt", costStd);     //原始值，放棄更改時恢復用

            _$obj.find(".pd-qty span").text(back ? -1 : 1);

            _$obj.find(".subTotal span").text(Math.round(costStd * (back ? -1 : 1)));            //小計初始值，因數量初始為1，故初始值即為標準成本
            _$obj.find(".subTotal .edit-txt").val(Math.round(costStd));
            _$obj.find(".subTotal .edit-txt").attr("data-alt", Math.round(costStd));     //原始值，放棄更改時恢復用

            _$obj.find(".pUName").text(_$JSON["pUName"]);
            _$obj.find(".isTax").text(_$JSON["isTax"] == "Y" ? "應稅" : "免稅");

            //#region 設定保存期限的
            var maxId = 0
            _$obj.closest("#pd-list-main").find(".pdLimitDate").each(function () {
                var dataId = $(this).attr("data-id");
                maxId = (typeof (dataId) == "undefined") ? maxId : parseInt(dataId) > maxId ? parseInt(dataId) : maxId;
            });
            _$obj.find(".pdLimitDate").attr({ "id": "pdLimitDate_" + (maxId + 1), "data-id": (maxId + 1) });
            datepicker($("#pdLimitDate_" + (maxId + 1)));
            //#endregion

            pdCalc();
        }

        function pdCalc() {        //計算商品小計及總計
            var pdTotal, listPdTotal = 0;
            $("#pd-list-main").find(".pd-row").each(function () {
                pdTotal = parseInt($(this).find(".subTotal span").text());
                listPdTotal += pdTotal;
            });
            $("#inStk-total").text(listPdTotal);
        }

        function chkEditBtn(_$obj, _act) {        //處理資料更新按鈕
            switch (_act) {
                case "abort":
                    var orgnl_value = _$obj.find(".edit-txt").attr("data-alt");
                    _$obj.find(".edit-txt").val(orgnl_value);
                    break;
                case "submit":
                    var $objRow = _$obj.closest(".pd-row"),
                        id = _$obj.attr("data-id"),
                        back = $objRow.attr("data-back") == "true" ? -1 : 1,        //是否為退貨
                        new_value = _$obj.find(".edit-txt").val();
                    if (new_value == "") { return error_focus("請填入數字！", _$obj.find(".edit-txt")); }
                    new_value = (id == "costStd" ? accurateDecimal(new_value, pointQty, "Y") : new_value);

                    _$obj.find(".edit-txt").attr("data-alt", new_value);
                    _$obj.find(".edit-tag span").text(new_value * (id != "costStd" ? back : 1));

                    if (id == "subTotal") {     //商品小計更動，計算並寫回含稅單價
                        var costStd = accurateDecimal(parseFloat($objRow.find(".subTotal .edit-txt").attr("data-alt")) / parseInt($objRow.find(".pd-qty .edit-txt").attr("data-alt")), pointQty, "Y");
                        $objRow.find(".costStd .edit-txt").val(Math.abs(costStd)).attr("data-alt", costStd);
                        $objRow.find(".costStd .edit-tag span").text(costStd);
                    } else {        //數量或含稅單價異動，計算並寫入小計
                        var subTotal = Math.round(parseFloat($objRow.find(".costStd .edit-txt").attr("data-alt")) * parseInt($objRow.find(".pd-qty .edit-txt").attr("data-alt")));
                        $objRow.find(".subTotal .edit-txt").val(Math.abs(subTotal)).attr("data-alt", subTotal);
                        $objRow.find(".subTotal .edit-tag span").text(subTotal * back);
                    }

                    pdCalc();
                    break;
            };
            _$obj.find(".edit-zone").hide();
            _$obj.find(".edit-tag").show();
        }

        var pdJson;      //所有產品(可進貨)
        var pvJson;      //所有廠商
        function getJson(_json, _kind) {        //取得商品資料
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03_1",
                    json: _json,
                    kind: _kind
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--getJson()--請洽工程人員');
                },
                success: function (response) {
                    if (response == "not-emp") {
                        location.replace("/login.aspx");
                    } else {
                        response == "N"
                            ? _json == "pdJson"
                                ? pdJson = jQuery.parseJSON("[]")
                                : pvJson = jQuery.parseJSON("[]")
                            : _json == "pdJson"
                                ? pdJson = jQuery.parseJSON(response)
                                : pvJson = jQuery.parseJSON(response);
                    }
                }
            });
        }

        function chkUpdZone() {     //修改小區域設定
            var updchk = true;
            $("#pd-list-main .pd-row").find(".edit-zone:visible").each(function () {
                var $obj = $(this).find(".edit-txt");
                if ($obj.val() != $obj.attr("data-alt")) { updchk = false; return error_focus("你修改後尚未按下確定！", $obj) }
                else { chkEditBtn($(this).closest(".edit-single-main"), "abort"); }
            });
            return updchk;
        }

        function closePdFilter(_bool) {     //商品搜尋開啟/關閉
            var $obj = $("#add-pd-main");
            if (_bool) {
                $obj.find(".del-txt-group").addClass("del-disabled");
                $obj.find("#pd-filter").val("").prop("disabled", true);
                $obj.find("#back-inStk").prop("disabled", true);
                $obj.find("#lbl-backStk").addClass("dis-lbl-backStk");
            } else {
                $obj.find(".del-txt-group").removeClass("del-disabled");
                $obj.find("#pd-filter").val("").prop("disabled", false);
                $obj.find("#back-inStk").prop("disabled", false);
                $obj.find("#lbl-backStk").removeClass("dis-lbl-backStk");
            }
        }

        function ResetInPdAct() {   //商品搜尋重設
            $("#act-pNo").val("");
            $("#pd-add-submit").prop("disabled", true);
            closePdFilter(true);
        }

        function ChkUpdDisable() {      //進貨日期預設
            if ($("#InStkDate").val().replace(/\//g, '') < $("#minDate").val()) {
                $("#InStkDate").val($("#minDate").val().substr(0, 4) + "/" + $("#minDate").val().substr(4, 2) + "/" + $("#minDate").val().substr(6, 2));
            }
        }

        function ChgProvider(_pvType) {   //選擇廠商時處理  _pvType 1:廠商下拉式 / 2:autocomplete
            var $obj = _pvType == "1" ? $("#pvSn") : $("#act-pvSn")
            $obj.change(function () {
                if (_pvType == "1" && $("#pd-list-main").find(".pd-row").length > 0) {
                    if (!confirm("變更廠商，目前進貨商品清單將清空，是否繼續？")) {
                        $(this).val($.data(this, "current"));     //變更前的值
                        return false;
                    }
                }

                ResetInPdAct();
                $("#pd-list-main").empty();
                $("#inStk-total").text("0");

                $.data(this, "current", $(this).val());     //變更後的值
                var isNoneEmpty = _pvType == "1"
                    ? $(this).find("option:selected").val() != "0"
                    : $(this).val() != "";

                if (isNoneEmpty)
                    closePdFilter(false);

                return true;
            });
        }

        function setUserID(myValue) {
            $('#act-pvSn').val(myValue)
                .trigger('change');
        }

        function Initial() {    //起始設定
            getJson("pdJson", "IN");
            if ($("#pvType").val() == "2") { getJson("pvJson", "") };   //廠商是autocomplete
            ResetInPdAct();
            ChkUpdDisable();
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });    //回上一頁

            ChgProvider($("#pvType").val());    //選擇廠商異動

            $("#pd-add-submit").click(function () {            //商品確定列入至進貨清單
                var pNo = $("#act-pNo").val();
                if (pNo == "") {
                    return error_focus("請先選擇商品！", $("#act-pNo"));
                } else {
                    $.ajax({
                        url: "/AjaxMain.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: "B04",
                            pNo: pNo
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--pd-add-submit.click--請洽工程人員');
                        },
                        success: function (response) {
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "N":
                                    alert("查無該商品資料");
                                    break;
                                default:
                                    ResetInPdAct();
                                    closePdFilter(false);
                                    $(".empty-pd").hide();
                                    jsonPd = JSON.parse(response);
                                    $JSON = jsonPd[0];
                                    var extendObject = $("#pd-append").children().clone(true);
                                    $("#pd-list-main").append(extendObject);
                                    intlPdNew($("#pd-list-main").find(".pd-row").last(), $JSON);
                                    break;
                            }
                        }
                    });
                };
            });

            $(".pd-del").click(function () {            //商品從進貨清單移除
                if (confirm("確定刪除本商品？")) {
                    $(this).closest(".pd-row").remove();
                    if ($("#pd-list-main").find(".pd-row").length == 0) { $(".empty-pd").show(); }
                    pdCalc();
                }
            });

            $("#btn-add-inStk").click(function () {            //確定送出進貨單
                if ($("#pd-list-main").find(".pd-row").length == 0) {
                    return error_focus("請先加入商品！", $("#pd-filter"));
                } else {
                    if (!chkUpdZone()) { return false; }
                    else {
                        var pd_list = "";
                        $("#pd-list-main").find(".pd-row").each(function () {
                            var $this = $(this);
                            pd_list += (pd_list == "" ? "" : ",") +
                                "{'pNo':'" + $this.attr("data-id") + "'," +
                                "'amount':'" + $this.find(".costStd .edit-txt").val() + "'," +
                                "'qty':'" + $this.find(".pd-qty span").text() + "'," +
                                "'amtTotal':'" + $this.find(".subTotal span").text() + "'," +
                                "'pdLimitDate':'" + $this.find(".pdLimitDate").val() + "'}";
                        });
                        pd_list = "[" + pd_list + "]";
                    }
                }

                if (confirm("確定新增進貨單？")) {
                    var ajaxData = "args0=C11&" + $(".input-main :input").serialize() + "&pdList=" + pd_list;
                    $.ajax({
                        url: "/AjaxInStk.aspx",
                        type: "POST",
                        async: false,
                        data: ajaxData,
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
                                case "Z":
                                    alert("商品清單有誤！");
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg30/prg3001.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
            });

            //開啟變更商品清單的商品單價及數量或小計
            $(".edit-tag").click(function () {
                if (chkUpdZone()) {
                    $obj = $(this).closest(".edit-single-main");
                    $(this).hide()
                    $obj.find(".edit-zone").show();
                    $obj.find(".edit-txt").focus(function () { $(this).select(); }).focus();
                }
            });

            //處理商品清單的商品單價及數量變更按鈕
            $(".edit-btn").click(function () {
                var act = $(this).attr("data-id");   //abort:放棄 / submit:確定變更
                var $obj = $(this).closest(".edit-single-main");
                chkEditBtn($obj, act);
            });

            $("#pd-filter")
                .on('input', function () {      //異動時清空pno欄位
                    $("#act-pNo").val("");
                    $("#pd-add-submit").prop("disabled", true);
                })
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        var tbl = pdJson.filter((x) => { return (x.pNameS.indexOf(str) >= 0 || x.pName.indexOf(str) >= 0 || x.pBarcode.indexOf(str) >= 0); })
                        response($.map(tbl, function (item) { // 此處是將返回資料轉換為 JSON物件
                            return {
                                label: item.pBarcode + "．" + item.pNameS, // 下拉項顯示內容
                                value: item.pBarcode + "．" + item.pNameS,  // 下拉項對應數值
                                actPno: item.pNo
                                //另外可以自定義其它引數
                            }
                        }));
                    },
                    select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                        $("#pd-filter").val(ui.item.value);
                        $("#act-pNo").val(ui.item.actPno);
                        $("#pd-add-submit").prop("disabled", false);
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    if ($(this).closest(".del-disabled").length == 0) {
                        $("#act-pNo").val("");
                        $("#pd-add-submit").prop("disabled", true);
                    }
                });

            $("#pv-filter")
                .on('input', function () {      //異動時清空pno欄位
                    if ($("#pd-list-main").find(".pd-row").length > 0) {
                        if (!confirm("變更廠商，目前進貨商品清單將清空，是否繼續？")) {
                            $(this).val($(this).attr("data-val"));     //變更前的值
                            return false;
                        }
                    }
                    setUserID("");
                })
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        var tbl = pvJson.filter((x) => { return (x.pvName.indexOf(str) >= 0 || x.pvNameS.indexOf(str) >= 0 || x.pvId.indexOf(str) >= 0); })
                        response($.map(tbl, function (item) { // 此處是將返回資料轉換為 JSON物件
                            return {
                                label: item.pvId + "．" + item.pvNameS, // 下拉項顯示內容
                                value: item.pvId + "．" + item.pvNameS,  // 下拉項對應數值
                                actPvSn: item.pvSn
                                //另外可以自定義其它引數
                            }
                        }));
                    },
                    select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                        $("#pv-filter").val(ui.item.value).attr("data-val", ui.item.value);
                        setUserID(ui.item.actPvSn)
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    if ($("#pd-list-main").find(".pd-row").length > 0) {
                        if (!confirm("變更廠商，目前進貨商品清單將清空，是否繼續？")) {
                            $("#pv-filter").val($("#pv-filter").attr("data-val"));     //變更前的值
                            return false;
                        }
                    }
                    setUserID("");
                    $("#pv-filter").attr("data-val", "");
                });

        });
    </script>
</asp:Content>
