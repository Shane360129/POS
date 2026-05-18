<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg4002UPD.aspx.cs" Inherits="prg4002UPD" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .outStk-input-main {border:1px #000 solid;border-radius:3px;padding:10px 0;}
        .pd-row {display:flex;align-items:center;}
        #pd-list-main .pd-row:not(:first-child) {border-top:1px #c9c9c9 dashed;}
        .pd-row > div {text-align:right;width:6.98%;padding:5px;}
        .pd-row-title > div {background-color:#3DBA8E;color:#fff;margin-right:1px;}
        .pd-row-title > div:last-child {margin-right:0;}
        .pd-row > div:first-child {width:30.5%;text-align:left;}
        .pd-row > div:nth-child(2), .pd-row > div:nth-child(3) {width:13.5%;}

        .pd-row > div.txt-center {text-align:center;}
        .pd-list-total {text-align:right;margin-right:21%;font-weight:bold;padding:5px;padding-right:15px;color:#080;}
        .pd-list-total span {color:#080;}

        .edit-single-main {justify-content:flex-end;padding-right:5px;margin-left:0;}
        .notice-txt {color:#F60087;font-size:18px;}

        .pdLimitDate {width:105px!important;padding:1px 5px;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <div class="page-main outStk-upd-main" style="margin-top:10px;">
            <div id="add-pd-main">
                <span style="font-weight:bold;font-size:18px;"><i class="fas fa-caret-square-right"></i> 銷貨商品︰</span>
                <div class="del-txt-group">
                    <input type="text" class="form-control del-txt-input" placeholder="請輸入商品名稱或條碼" maxlength="42" style="width:266px;" id="pd-filter" />
                    <input type="hidden" id="act-pNo" />
                    <div class="del-txt-button"><i class="fas fa-times-circle"></i></div>
                </div>
                <button id="pd-add-submit" class="btn-submit btn-admin" style="margin-left:10px;" disabled><i class="fas fa-check-circle"></i> 加入清單</button>
                <input type="checkbox" id="back-inStk" /><label for="back-inStk" id="lbl-backStk">退貨</label>
            </div>
            <div style="border-radius:3px;border:1px #080 solid;">
                <div class="pd-row pd-row-title"><div>商品名稱</div><div>含稅單價</div><div>數量</div><div>單位</div><div>單折</div><div>小計</div><div class="txt-center">稅別</div><div class="txt-center">保存期限</div><div class="txt-center">刪除</div></div>
                <div class="outStkPd-main">
                    <div class="empty-pd empty-data" style="display:none;">尚無商品！</div>
                    <div id="pd-list-main"><asp:Label ID="Label3" runat="server"></asp:Label></div>
                </div>
            </div>
            <div class="pd-list-total"><span>總計︰</span><span id="outStk-total">0</span></div>
            <div id="pd-append" style="display:none;">
                <div class="pd-row" data-sn="new">
                    <div class="pNameS"></div>
                    <div class="dtlAmt edit-single-main" data-id="dtlAmt">
                        <div class="edit-tag" style="justify-content:flex-end;"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                        <div class="edit-zone" style="display: none;">
                            <input type="text" class="form-control edit-txt edit-dtlAmt chk-input" data-func="real_number" data-id="1" maxlength="10" style="width:70px;" value="1">
                            <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                            <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                        </div>
                    </div>
                    <div class="pd-qty edit-single-main" data-id="pdQty">
                        <div class="edit-tag" style="justify-content:flex-end;"><i class="fas fa-edit btn-admin"></i><span>1</span></div>
                        <div class="edit-zone" style="display: none;">
                            <input type="text" class="edit-txt edit-qty chk-input" data-func="number" data-id="1" maxlength="6" style="width:70px;" value="1">
                            <button class="btn-abort edit-btn" data-id="abort"><i class="fas fa-times-circle"></i>放棄</button>
                            <button class="btn-submit edit-btn btn-admin" data-id="submit"><i class="fas fa-check-circle"></i>確定</button>
                        </div>
                    </div>
                    <div class="pUName"></div>
                    <div class="pd-discnt">0</div>
                    <div class="pd-sub-total">0</div>
                    <div class="txt-center isTax"></div>
                    <div class="txt-center"><input type="text" readonly class="pdLimitDate" /></div>
                    <div class="txt-center"><button class="btn-del pd-del"><i class='fas fa-trash'></i>刪除</button></div>
                </div>
            </div>
        </div>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回上一頁</button>
            <button class="btn-submit btn-page btn-admin" id="btn-upd-outStk" data-alt="pd-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
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

        //新增商品後，初始化商品資料
        function intlPdNew(_$obj, _$JSON) {
            var back = $("#back-inStk").prop("checked");        //是否為進貨退回
            _$obj.attr("data-id", _$JSON["pNo"]).attr("data-back", back);
            _$obj.find(".pNameS").html((back ? "<span class='back-dot'>退</span>" : "") +
                _$JSON["pNameS"] + "(" + _$JSON["pBarcode"] + ")");
            var dtlAmt = accurateDecimal(_$JSON["priceStd"], pointQty);
            _$obj.find(".dtlAmt span").text(dtlAmt);
            _$obj.find(".dtlAmt .edit-txt").val(dtlAmt);
            _$obj.find(".dtlAmt .edit-txt").attr("data-id", dtlAmt);

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
            var pdTotal, listPdTotal = 0, discntDtlTotal = 0;
            $("#pd-list-main").find(".pd-row").each(function () {
                pdTotal = Math.round(parseFloat($(this).find(".dtlAmt span").text()) * parseInt($(this).find(".pd-qty span").text()));
                discntDtlTotal += parseInt($(this).find(".pd-discnt").text());
                $(this).find(".pd-sub-total").text(pdTotal);
                listPdTotal += pdTotal;
            });
            var discnt = parseInt($("#discnt").val());
            var discntStr = discnt == "0" ? "" : ("全折:-" + $("#discnt").val());
            var discntDtlStr = discntDtlTotal == 0 ? "" : ("單折:" + discntDtlTotal);
            var discntTotalStr = (discntStr == "" && discntDtlStr == "") ? "" : ("(" + (discntStr == "" ? discntDtlStr : (discntStr + (discntDtlStr == "" ? "" : ("/" + discntDtlStr)))) + ")");
            $("#outStk-total").text(discntTotalStr + (listPdTotal + discntDtlTotal - discnt));
            CalcPayKind();
        }

        //處理資料更新按鈕
        function chkEditBtn(_$obj, _id) {
            switch (_id) {
                case "abort":
                    var orgnl_value = _$obj.find(".edit-txt").attr("data-id");
                    _$obj.find(".edit-txt").val(orgnl_value);
                    break;
                case "submit":
                    var new_value = _$obj.find(".edit-txt").val();
                    if (new_value == "") { return error_focus("請填入數字！", _$obj.find(".edit-txt")); }
                    var $objRow = _$obj.closest(".pd-row"),
                        id = _$obj.attr("data-id"),
                        back = $objRow.attr("data-back") == "true" ? -1 : 1;        //是否為退貨
                    _$obj.find(".edit-txt").attr("data-id", new_value);
                    _$obj.find(".edit-tag span").text(id == "dtlAmt" ? parseFloat(new_value).toFixed(2) : (new_value * back));     //更新含稅單價要小數兩位，數量為整數(且處理退貨為負)
                    pdCalc();
                    break;
            };
            _$obj.find(".edit-zone").hide();
            _$obj.find(".edit-tag").show();
        }

        function chkUpdBtn() {
            if ($("#OutStkDate").val().replace(/\//g, '') < $("#minDate").val()) {
                $(".pd-row .edit-tag").removeClass("edit-tag");
                $(".pd-row i:not(.fa-trash)").hide();
                $(".pd-row .pd-del").prop("disabled", true);
                closePdFilter(true);
                $("#btn-outStk-del").prop("disabled", true).css({ "color": "#FFD1D1", "cursor": "not-allowed" });
                $("#notice-upd").html("<i class='fas fa-exclamation-circle'> 本單已日結無法修改</i>");
            }
            var isUpd = $("#isUpd").val();
            if (isUpd != "Y") {
                $(".pd-row .edit-tag").removeClass("edit-tag");
                $(".pd-row i:not(.fa-trash)").hide();
                $(".pd-row .pd-del").prop("disabled", true);
                $(".pay-group").prop("disabled", true).addClass("pay-disabled").css({'color':'#aaa', 'cursor':'not-allowed'});

                closePdFilter(true);
                var msg = isUpd == "N" ? "已銷帳" : "為前台資料";
                $("#notice-pay-upd").html("<i class='fas fa-exclamation-circle'> 本單" + msg + "無法修改</i>");
            }
            
        }


        var pdJson;      //所有產品(含已下架)
        function getPd() {
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03"
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
                            pdJson = jQuery.parseJSON(response);
                            //console.log(pdJson);
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
            $("#pd-filter").val("");
            $("#act-pNo").val("");
            $("#pd-add-submit").prop("disabled", true);
        }

        function CalcPayKind() {
            $obj = $("#pay-list-grp");
            if ($obj.find(".pay-row").length == 1) {
                $obj.find(".pay-amt").val($("#outStk-total").text());
            }
        }

        function initlDatepicker() {
            $(".outStkPd-main").find(".pdLimitDate").each(function () {
                var id = "pdLimitDate_" + $(this).attr("data-id");
                $(this).attr("id", id);
                datepicker($("#" + id));
            });
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
            pdCalc();
            chkUpdBtn();
            initlDatepicker();
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //商品確定新增至銷貨清單
            $("#pd-add-submit").click(function () {
                var pNo = $("#act-pNo").val();
                if (pNo == "") {
                    return error_focus("請先選擇商品！", $("#pd-filter"));
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
                                    $(".empty-pd").hide();
                                    jsonPd = JSON.parse(response);
                                    $JSON = jsonPd[0];
                                    var extendObject = $("#pd-append").children().clone(true);
                                    $("#pd-list-main").append(extendObject);
                                    intlPdNew($("#pd-list-main").find(".pd-row").last(), $JSON);
                                    ResetInPdAct();
                                    break;
                            }
                        }
                    });
                };
            });

            //商品從銷貨清單移除
            $(".pd-del").click(function () {
                if (confirm("確定刪除本商品？")) {
                    $(this).closest(".pd-row").remove();
                    if ($("#pd-list-main").find(".pd-row").length == 0) { $(".empty-pd").show(); }
                    pdCalc();
                }
            });

            //確定送出修改銷貨單
            $("#btn-upd-outStk").click(function () {
                if ($("#pd-list-main").find(".pd-row").length == 0) {
                    return error_focus("請先加入商品！", $("#pd-select"));
                } else {
                    var pd_list = "";
                    $("#pd-list-main").find(".pd-row").each(function () {
                        var $this = $(this);
                        pd_list += (pd_list == "" ? "" : ",") +
                            "{'dtlSn':'" + $this.attr("data-sn") + "'," +
                            "'pNo':'" + $this.attr("data-id") + "'," +
                            "'amount':'" + $this.find(".dtlAmt .edit-txt").val() + "'," +
                            "'qty':'" + $this.find(".pd-qty span").text() + "'," +
                            "'amtTotal':'" + $this.find(".pd-sub-total").text() + "'," +
                            "'pdLimitDate':'" + $this.find(".pdLimitDate").val() + "'}";
                    });
                    pd_list = "[" + pd_list + "]";
                }

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

                var data = "args0=C12A&sn=" + $("#sn").val() + "&pdList=" + pd_list + "&payJSON=" + payJSON + "&" + $(".outStk-input-main").find(":input").serialize();
                if (confirm("確定送出？")) {
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
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能修改！");
                                    break;
                                case "Z":
                                    alert("商品清單有誤！");
                                    break;
                                case "Y":
                                    alert("修改成功！");
                                    $.ajax({
                                        url: "/AjaxTrace.aspx",
                                        type: "POST",
                                        async: false,
                                        data: {
                                            args0: "T00"
                                        },
                                        error: function (xhr) {
                                            console.log(xhr.responseText);
                                        },
                                        success: function (response) {
                                            console.log(response);
                                            location.reload();
                                        }
                                    });
                                    break;
                                default:
                                    console.log(response);
                                    alert("修改失敗請聯絡工程人員！");
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
                $(".edit-tag").show();
                $(".edit-zone").hide();
                $(this).hide();

                $obj = $(this).closest(".edit-single-main");
                $obj.find(".edit-txt").val($obj.find(".edit-txt").attr("data-id"));
                $obj.find(".edit-zone").show();
                $obj.find(".edit-txt").focus(function () { $(this).select(); }).focus();

            });

            //處理商品清單的商品單價及數量變更按鈕
            $(".edit-btn").click(function () {
                var id = $(this).attr("data-id");   //abort:放棄 / submit:確定變更
                $obj = $(this).closest(".edit-single-main");
                chkEditBtn($obj, id);
            });

            //整筆刪除
            $("#btn-outStk-del").click(function () {
                if (confirm("確定整筆刪除？")) {
                    $.ajax({
                        url: "/AjaxOutStk.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: "C12B",
                            sn: $("#sn").val()
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-outStk-del.click--請洽工程人員');
                        },
                        success: function (response) {
                            //console.log(response);
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能刪除！");
                                    break;
                                case "Y":
                                    alert("刪除成功！");
                                    location.href = "/prg40/prg4002.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("刪除失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
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
                        console.log(str);
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
                    ResetInPdAct();
                });
        });
    </script>
</asp:Content>
