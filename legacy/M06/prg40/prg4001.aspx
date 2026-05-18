<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg4001.aspx.cs" Inherits="prg4001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .outStk-input-main {border:2px #000 solid;border-radius:3px;padding:20px 14px;padding-bottom:0;}
        .outStk-input-main > div {display:flex;flex-wrap:wrap;}
        .input-sub {width:auto;margin-bottom:20px;display:flex;align-items:center;}

        #pay-list-grp{display:flex;align-items:center;}
        #pay-list-grp i {margin-left:5px;cursor:pointer;margin-right:20px;}

        #pk-list-add {display:none;}

        .pd-row {display:flex;align-items:center;}
        .pd-row:not(:nth-child(1)) {border-top:1px #c9c9c9 dashed;}
        .pd-row-title > div {background-color:#3DBA8E;color:#fff;margin-right:1px;}
        .pd-row-title > div:last-child {margin-right:0;}
        .pd-row > div {text-align:right;width:7.7%;padding:5px;}
        .pd-row > div:first-child {width:24.9%;text-align:left;}
        .pd-row > div:nth-child(2), .pd-row > div:nth-child(3) {width:14.5%;min-width:170px;}
        .pd-row > div:nth-child(5), .pd-row > div:nth-child(7) {width:11.5%;}

        .pd-row > div.txt-center {text-align:center;}
        .pd-list-total {width:100%;text-align:right;padding:5px;padding-right:27%;font-weight:bold;}
        .pd-list-total span {font-size:18px;color:#080;}
    
        .edit-zone {display:flex;justify-content:flex-end;}
        .edit-zone .edit-btn {margin:0;margin-left:5px;padding:0 4px;font-size:15px;}

        .pdLimitDate {width:105px!important;padding:1px 5px;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <div class="page-main outStk-add-main" style="margin-top:10px;">
            <div id="add-pd-main">
                <span style="font-weight:bold;font-size:18px;"><i class="fas fa-caret-square-right"></i> 銷貨商品︰</span>
                <div class="del-txt-group">
                    <input type="text" class="form-control del-txt-input" placeholder="請輸入商品名稱或條碼" maxlength="42" style="width:266px;" disabled id="pd-filter"  />
                    <div class="del-txt-button"><i class="fas fa-times-circle"></i></div>
                    <input type="hidden" id="act-pNo" />
                </div>
                <button id="pd-add-submit" class="btn-submit btn-admin" style="margin-left:10px;" disabled><i class="fas fa-check-circle"></i> 加入清單</button>
                <input type="checkbox" id="back-inStk" /><label for="back-inStk" id="lbl-backStk">退貨</label>
            </div>
            <div style="border-radius:3px;border:1px #080 solid;">
                <div class="pd-row pd-row-title"><div>商品名稱</div><div>含稅單價</div><div>數量</div><div>單位</div><div>小計</div><div class="txt-center">稅別</div><div class="txt-center">保存期限</div><div class="txt-center">刪除</div></div>
                <div class="outStkPd-main">
                    <div class="empty-pd empty-data">尚無商品！</div>
                    <div id="pd-list-main"></div>
                </div>
            </div>
            <div class="pd-list-total"><span>總計︰</span><span id="outStk-total">0</span></div>
            <div id="pd-append" style="display:none;">
                <div class="pd-row">
                    <div class="pNameS"></div>
                    <div class="dtlAmt edit-single-main" data-id="dtlAmt" style="justify-content:flex-end;">
                        <div class="edit-tag"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                        <div class="edit-zone" style="display: none;">
                            <input type="text" class="form-control align-r edit-txt edit-dtlAmt chk-input" data-func="real_number" data-alt="1" maxlength="10" style="width:70px;" value="1">
                            <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                            <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                        </div>
                    </div>
                    <div class="pd-qty edit-single-main" data-id="pd-qty" style="justify-content:flex-end;">
                        <div class="edit-tag"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                        <div class="edit-zone" style="display: none;">
                            <input type="text" class="form-control align-r edit-txt edit-qty chk-input" data-func="number" data-alt="1" maxlength="6" style="width:70px;" value="1">
                            <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                            <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                        </div>
                    </div>
                    <div class="pUName"></div>
                    <div class="pd-sub-total">0</div>
                    <div class="txt-center isTax"></div>
                    <div class="txt-center"><input type="text" readonly class="pdLimitDate" /></div>
                    <div class="txt-center"><button class="btn-del pd-del"><i class='fas fa-trash'></i>刪除</button></div>
                </div>
            </div>
        </div>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回上一頁</button>
            <button class="btn-submit btn-page btn-admin" id="btn-add-outStk" data-alt="pd-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/ext/js/PayKind.js"></script>
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
        $('#OutStkDate').datepicker({
            dateFormat: 'yy/mm/dd',
            changeYear: true,
            changeMonth: true,
            yearRange: "-100:+1",
            minDate: new Date($("#minDate").val().substr(0,4), $("#minDate").val().substr(4,2) - 1, $("#minDate").val().substr(6,2))
        });

        function intlPdNew(_$obj, _$JSON) {                     //新增商品後，初始化商品資料
            var back = $("#back-inStk").prop("checked");        //是否為進貨退回
            _$obj.attr("data-id", _$JSON["pNo"]).attr("data-back", back);
            _$obj.find(".pNameS").html((back ? "<span class='back-dot'>退</span>" : "") +
                _$JSON["pNameS"] + "(" + _$JSON["pBarcode"] + ")");
            var priceKindId = $("#memSn option:selected").attr("data-priceKind");
            var priceKind = priceKindId == "2" ? "priceLow" : priceKindId == "3" ? "priceMem" : priceKindId == "4" ? "priceBat" : "priceStd";
            var dtlAmt = roundToPoint(_$JSON[priceKind], pointQty);

            _$obj.find(".dtlAmt span").text(dtlAmt);
            _$obj.find(".dtlAmt .edit-txt").val(dtlAmt);
            _$obj.find(".dtlAmt .edit-txt").attr("data-alt", dtlAmt);

            _$obj.find(".pUName").text(_$JSON["pUName"]);
            _$obj.find(".isTax").text(_$JSON["isTax"] == "Y" ? "應稅" : "免稅");
            _$obj.find(".pd-qty span").text(back ? -1 : 1);

            back ? _$obj.find(".pdLimitDate").show() : _$obj.find(".pdLimitDate").hide();
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

        //計算商品小計及總計
        function pdCalc() {
            var pdTotal, listPdTotal = 0;
            $("#pd-list-main").find(".pd-row").each(function () {
                pdTotal = Math.round(parseFloat($(this).find(".dtlAmt span").text()) * parseInt($(this).find(".pd-qty span").text()));
                $(this).find(".pd-sub-total").text(pdTotal);
                listPdTotal += pdTotal;
            });
            $("#outStk-total").text(listPdTotal);
            CalcPayKind();
        }

        //處理資料更新按鈕
        function chkEditBtn(_$obj, _act) {
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

                    _$obj.find(".edit-txt").attr("data-alt", new_value);
                    _$obj.find(".edit-tag span").text(new_value * (id != "dtlAmt" ? back : 1));

                    pdCalc();
                    break;
            };
            _$obj.find(".edit-zone").hide();
            _$obj.find(".edit-tag").show();
        }

        var pdJson;      //所有產品(含已下架)
        function getPd() {
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03",
                    kind: "OUT"
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--getPd()--請洽工程人員');
                },
                success: function (response) {
                    switch (response) {
                        case "not-emp":
                            location.replace("/login.aspx");
                            break;
                        case "N":
                            pdJson = jQuery.parseJSON("[]");
                            break;
                        default:
                            pdJson =  jQuery.parseJSON(response);
                            break;
                    }
                }
            });
        }

        function closePdFilter(_bool) {
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

        function ResetInPdAct() {
            $("#act-pNo").val("");
            $("#pd-add-submit").prop("disabled", true);
            closePdFilter(true);
        }

        function ChkUpdDisable() {
            if ($("#OutStkDate").val().replace(/\//g, '') < $("#minDate").val()) {
                $("#OutStkDate").val($("#minDate").val().substr(0, 4) + "/" + $("#minDate").val().substr(4, 2) + "/" + $("#minDate").val().substr(6, 2));
            }
        }

        function CalcPayKind() {
            $obj = $("#pay-list-grp");
            if ($obj.find(".pay-row").length == 1) {
                $obj.find(".pay-amt").val($("#outStk-total").text());
            }
        }

        function datepicker(_$obj) {        //啟動保存期限日曆
            _$obj.datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-5:+5"
            });
        }

        function Initial() {
            getPd();
            ResetInPdAct();
            ChkUpdDisable();
        }

        $(function () {
            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-1:+0"
            });

            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //商品確定新增至進貨清單
            $("#pd-add-submit").click(function () {
                if ($("#memSn").val() == "") { return error_focus("請先選擇會員！", $("#memSn")); }

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

            //商品從進貨清單移除
            $(".pd-del").click(function () {
                if (confirm("確定刪除本商品？")) {
                    $(this).closest(".pd-row").remove();
                    if ($("#pd-list-main").find(".pd-row").length == 0) { $(".empty-pd").show(); }
                    pdCalc();
                }
            });

            //確定送出進貨單
            $("#btn-add-outStk").click(function () {
                if ($("#memSn").val() == "") { return error_focus("請先選擇會員！", $("#memSn")); }
                if ($("#pd-list-main").find(".pd-row").length == 0) { return error_focus("請先加入商品！", $("#pd-select")); };

                var pd_list = "";
                $("#pd-list-main").find(".pd-row").each(function () {
                    var $this = $(this);
                    pd_list += (pd_list == "" ? "" : ",") +
                        "{'pNo':'" + $this.attr("data-id") + "'," +
                        "'pName':'" + $this.find("pNames").val() + "'," +
                        "'amount':'" + $this.find(".dtlAmt .edit-txt").val() + "'," +
                        "'qty':'" + $this.find(".pd-qty span").text() + "'," +
                        "'amtTotal':'" + $this.find(".pd-sub-total").text() + "'," +
                        "'pdLimitDate':'" + $this.find(".pdLimitDate").val() + "'}";
                });
                pd_list = "[" + pd_list + "]";
                console.log(pd_list);
                
                CalcPayKind();
                $obj = $("#pay-list-grp");
                payAmtTotal = 0;
                $obj.find(".pay-amt").each(function () {
                    if ($(this).val() != "") { payAmtTotal += parseFloat($(this).val()); }
                });
                if (payAmtTotal != parseFloat($("#outStk-total").text())) {
                    return error_focus("付款方式總額不等於銷貨單緦計！", $obj);
                } else {
                    var payJSON = "";
                    $obj.find(".pay-row").each(function () {
                        payJSON += (payJSON == "" ? "" : ",") + "{\"PAYID\":\"" + $(this).find("option:selected").val() + "\",\"PAYAMT\":" + roundToPoint($(this).find(".pay-amt").val(), pointQty) + "}"
                    });
                    payJSON = "[" + payJSON + "]";
                };

                if (confirm("確定新增銷貨單？")) {

                    var data = "args0=C12&pdList=" + pd_list + "&payJSON=" + payJSON + "&" + $(".outStk-input-main").find(":input").serialize();
                    console.log(data);
                    $.ajax({
                        url: "/AjaxOutStk.aspx",
                        type: "POST",
                        async: false,
                        data: data,
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
                                    alert("非管理者不能新增！");
                                    break;
                                case "Z":
                                    alert("查無商品！");
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg40/prg4001.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
            });

            //開啟變更商品清單的商品單價及數量
            $(".edit-tag").click(function () {
                $("#pd-list-main .pd-row").find(".edit-zone:visible").each(function () {
                    $obj = $(this).closest(".edit-single-main");
                    chkEditBtn($obj, "abort");
                });
                $obj = $(this).closest(".edit-single-main");
                $(this).hide()
                $obj.find(".edit-zone").show();
                $obj.find(".edit-txt").focus(function () { $(this).select(); }).focus();

            });

            //處理商品清單的商品單價及數量變更按鈕
            $(".edit-btn").click(function () {
                var id = $(this).attr("data-id");   //abort:放棄 / submit:確定變更
                $obj = $(this).closest(".edit-single-main");
                chkEditBtn($obj, id);
            });

            $("#memFilter").change(function () {
                if ($("#pd-list-main").find(".pd-row").length > 0) {
                    if (!confirm("變更會員，目前銷貨商品清單將清空，是否繼續？")) {
                        $(this).val($.data(this, "current"));     //變更前的值
                        return false;
                    }
                }
                ResetInPdAct();
                $("#pd-list-main").empty();
                $("#inStk-total").text("0");
                if ($(this).val() != "") {
                    closePdFilter(false);
                }

                $.data(this, "current", $(this).val());     //變更後的值
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
                        var str = request.term
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        var tbl = pdJson.filter((x) => { return (x.pNameS.indexOf(str) >= 0 || x.pName.indexOf(str) >= 0 || x.pBarcode.indexOf(str) >= 0); })
                        response($.map(tbl, function (item) { // 此處是將返回資料轉換為 JSON物件
                            return {
                                label: item.pBarcode + "．" + item.pNameS, // 下拉項顯示內容
                                value: item.pBarcode + "．" + item.pNameS,  // 下拉項對應數值
                                actPno: item.pNo,
                                qtyNow: item.qtyNow,
                                type: item.isUpdStk

                                //另外可以自定義其它引數
                            }
                        }));
                    },
                    select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                        var back = $("#back-inStk").prop("checked");
						if (parseInt(ui.item.qtyNow) <= 0 && (ui.item.type) == "Y" && !back) {
                            alert("目前該商品無庫存，無法銷貨！");
                            $("#pd-filter").val("");
                            $("#act-pNo").val("");
                            return false;
                        } else {
                            $("#pd-filter").val(ui.item.value);
                            $("#act-pNo").val(ui.item.actPno);
                            $("#pd-add-submit").prop("disabled", false);
                            return false;
                        }
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    if ($(this).closest(".del-disabled").length == 0) {
                        $("#memFilter").val("");
                        $("#act-pNo").val("");
                        $("#pd-add-submit").prop("disabled", true);
                    }
                });
        });

        $('#memFilter').autocomplete({
            source: <%= ProviderNamesJson %>,
             autoFocus: true,
             minLength: 1,
             delay: 300,
             select: function (event, ui) {
                 $('#memFilter').val(ui.item.label);
                 $('#memSn').val(ui.item.value);
                 return false;
             },
         });

        function Base64ToBytes(base64) {
            var s = window.atob(base64);
            var bytes = new Uint8Array(s.length);
            for (var i = 0; i < s.length; i++) {
                bytes[i] = s.charCodeAt(i);
            }
            return bytes;
        };

    </script>
</asp:Content>
