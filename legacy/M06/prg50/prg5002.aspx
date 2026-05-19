<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5002.aspx.cs" Inherits="prg5002" %>

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
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
                $.cookie("pvSn", null);
                $.cookie("acctOutId", null);
                $.cookie("InStkId", null);
                $.cookie("pName", null);
                $.cookie("pNo", null);
                $.cookie("isTax", null);
                $.cookie("dtlPayType", null);
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
        var pvPdJson;    //已選廠商之所有產品
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
                            pvPdJson = jQuery.parseJSON("[]");
                            break;
                        default:
                            pdJson = jQuery.parseJSON(response);
                            //console.log(pdJson);
                            var pvSn = $("#pvSn option:selected").val();
                            pvPdJson = pvSn == "" ? pdJson : pdJson.filter((x) => { return x.pvSn == pvSn });
                            //console.log(pvPdJson);
                            break;
                    }
                }
            });
        }

        function ResetInPdAct() {
            var actPNo = $("#act-pNo").val();
            if (actPNo != "") {
                var chkPvPd = pvPdJson.filter((x) => { return x.pNo == actPNo; })
                if (chkPvPd.length == 0) {
                    $("#pd-filter").val("");
                    $("#act-pNo").val("");
                }
            }
        }

        function Initial() {
            $(".row-main[data-id='']").text("").css("border-top", "0");
            getPd();
            ResetInPdAct();
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
                $.cookie("sDate", $("#sDate").val());
                $.cookie("eDate", $("#eDate").val());
                $.cookie("pvSn", $("#pvSn").val());
                $.cookie("acctOutId", $("#acctOutId").val());
                $.cookie("InStkId", $("#InStkId").val());
                //$.cookie("pName", $("#pName").val());
                $.cookie("pName", $("#pd-filter").val());
                $.cookie("pNo", $("#act-pNo").val());
                $.cookie("isTax", $("#isTax").val());
                $.cookie("dtlPayType", $("#dtlPayType").val());
                location.reload();
            });

            //選擇廠商
            $("#pvSn").change(function () {
                var pvSn = $(this).find("option:selected").val();
                console.log(pvSn);
                if (pvSn == "") { pvPdJson = pdJson; }
                else {
                    pvPdJson = pdJson.filter((x) => { return x.pvSn == pvSn });
                    if (pvPdJson.length == 0)
                        alert("查無該廠商商品資料！");
                }
                ResetInPdAct();
            });

            $("#pd-filter")
                .on('input', function () { $("#act-pNo").val(""); })     //異動時清空pno欄位
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        console.log(str);
                        var tbl = pvPdJson.filter((x) => { return (x.pNameS.indexOf(str) >= 0 || x.pName.indexOf(str) >= 0 || x.pBarcode.indexOf(str) >= 0); })
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
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () { $("#act-pNo").val(""); });
        });
    </script>
</asp:Content>
