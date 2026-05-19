<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg4002.aspx.cs" Inherits="prg4002" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .search-main {border:1px #080 solid;border-radius:3px;margin-bottom:20px;padding:20px 10px;padding-bottom:0;}
        .search-main > div {display:flex;align-items:center;flex-wrap:wrap;margin-bottom:10px;}
        .search-main .search-sub {display:flex;align-items:center;margin-bottom:10px;margin-right:40px;}

        .list-main {width:100%;}
        .list-main td {border-top:2px #c9c9c9 dotted;padding:5px;border-right:1px #fff solid;}
        .list-main .list-title td {border-top:0;background-color:#e9e9e9;font-weight:bold;}
        .list-main tr:last-child td {border-bottom:2px #c9c9c9 dotted;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main pd-list-main">            
            <asp:Label ID="Label2" runat="server"></asp:Label>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/ext/js/Filter.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
                $.cookie("memSn", null);
                $.cookie("outStkId", null);
                $.cookie("pName", null);
                $.cookie("reciptNo", null);
                $.cookie("pNo", null);
                $.cookie("isTax", null);
                $.cookie("outType", null);
                window.location.reload();
            }
        };

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

        var pdJson;      //所有產品(含已下架)
        function getPd() {
            var str = $("#pd-filter").val();
            str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03",
                    kind: "OUT",
                    pdFilter: str
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

        function CalcTotal() {
            $obj = $(".list-main");
            var totalQty = 0, totalDiscnt = 0, totalDtlDiscnt = 0, totalAmt = 0;
            $obj.find(".dtl-qty").each(function () {
                totalQty += parseInt($(this).text());
            });
            $("#total-qty").text(totalQty);
            //$obj.find(".discnt").each(function () {
            //    totalDiscnt += parseInt($(this).attr("data-val"));
            //    console.log(totalDiscnt);
            //});
            totalDiscnt = parseInt($("#total-discnt").attr("data-val"));
            totalDiscnt == 0
                ? $("#total-discnt").hide()
                : $("#total-discnt").show();
            $obj.find(".dtl-discnt").each(function () {
                totalDtlDiscnt += parseInt($(this).attr("data-val"));
            });
            totalDtlDiscnt == 0
                ? $("#total-dtlDiscnt").hide()
                : $("#total-dtlDiscnt").find(".amt").text(to3dot(totalDtlDiscnt));
            (totalDiscnt == 0 && totalDtlDiscnt == 0) ? $("#total-discnt-row").hide() : $("#total-discnt-row").show();

            $obj.find(".dtl-amt").each(function () {
                totalAmt += parseInt($(this).attr("data-val"));
            });
            $("#total-amt").text(to3dot(totalAmt+totalDiscnt));
        }

        function Initial() {
            getPd();
            $("#processing").hide();
            $(".row-main[data-id='']").text("").css("border-top", "0");
            var outType = $("#outType option:selected").val();
            (outType == "0" || outType == "1")
                ? $("#btn-prn").show()
                : $("#btn-prn").hide();
            CalcTotal();
        }



        $(function () {
            Initial();

            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "2021:+0",
                setDate: "today"
            });

            $("#btn-search").click(function () {
                if ($("#sDate").val() == "" || $("#eDate").val() == "") {
                    return error_focus("查詢區間不可為空白！", $("#sDate").val() == "" ? $("#sDate") : $("#eDate"));
                }
                $("#processing").show();
                $(this).attr("disabled", true);
                $.cookie("sDate", $("#sDate").val());
                $.cookie("eDate", $("#eDate").val());
                $.cookie("memSn", $("#memSn").val());
                $.cookie("outStkId", $("#outStkId").val());
                $.cookie("reciptNo", $("#reciptNo").val());
                $.cookie("pName", $("#pd-filter").val());
                $.cookie("pNo", $("#act-pNo").val());
                $.cookie("isTax", $("#isTax").val());
                $.cookie("outType", $("#outType").val());
                location.reload();
            });

            $("#pd-filter")
                .on('input', function () {      //異動時清空pno欄位
                    $("#act-pNo").val("");
                })
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        //var str = request.term;
                        //str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        //var tbl = pdJson.filter((x) => { return (x.pNameS.indexOf(str) >= 0 || x.pName.indexOf(str) >= 0 || x.pBarcode.indexOf(str) >= 0); })
                        getPd();
                        response($.map(pdJson, function (item) { // 此處是將返回資料轉換為 JSON物件
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
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    if ($(this).closest(".del-disabled").length == 0) {
                        $("#act-pNo").val("");
                    }
                });

        });
    </script>
</asp:Content>
