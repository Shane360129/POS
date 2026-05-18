<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5003.aspx.cs" Inherits="prg5003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .search-main { border: 1px #080 solid;border-radius: 3px;margin-bottom: 20px;padding: 20px 10px;padding-bottom: 0; }
        .search-main > div {display:flex;align-items:center;flex-wrap:wrap;margin-bottom:10px;}
        .search-main .search-sub {display:flex;align-items:center;margin-bottom:10px;margin-right:40px;}

        .acctIn-main {margin-top:15px;display:flex;align-items:center;}
        .acctIn-main label {cursor:pointer;}
        .mem-select {margin-left:10px;border:1px #ccc solid;padding:5px 10px;border-radius:4px;background-color:#eee;}

        .prg-content .list-main .ckbox { margin-left:0;margin-right:5px;width:16px;height:16px;}
        .prg-content .list-main .ckbox:checked { color:#000;background-color:#444;}

        .amt-chk-total, #outStk-chk-total {color:#E90080;}

        .list-main {width:100%;font-size:12px;}

        .list-title > div {background-color:#e9e9e9;font-weight:bold;padding:5px;border-right:1px #fff solid;}
        .list-title > div:nth-child(1){width:16%;}
        .list-title > div:nth-child(2){width:11.5%;}
        .list-title > div:nth-child(3){width:9.5%;}
        .list-title > div:nth-child(4){width:5.5%;}
        .list-title > div:nth-child(5){width:5.5%;}
        .list-title > div:nth-child(6){width:5.5%;}
        .list-title > div:nth-child(7){width:23%;}
        .list-title > div:nth-child(8){width:6.5%;}
        .list-title > div:nth-child(9){width:4%;}
        .list-title > div:nth-child(10){width:4%;}
        .list-title > div:nth-child(11){width:4%;}
        .list-title > div:nth-child(12){width:4.8%;}

        .node-mem, .mem-dtl-row, .id-dtl-row {display:flex;}
        .node-mem:not(:nth-child(1)),
        .mem-dtl-row:not(:nth-child(1)),
        .id-dtl > div:not(:nth-child(1)) {border-top:2px #c9c9c9 dotted;}
        .node-mem > div {padding:5px;}
        .node-mem > div:nth-child(1){width:16%;}
        .node-mem > div:nth-child(2){width:83.8%;padding:0;}
        .mem-select {display:none;}

        .mem-dtl-row > div {padding:5px;}
        .mem-dtl-row > div:nth-child(1) {width:13.82%;}
        .mem-dtl-row > div:nth-child(2) {width:11.31%;}
        .mem-dtl-row > div:nth-child(3) {width:6.59%;}
        .mem-dtl-row > div:nth-child(4) {width:6.59%;}
        .mem-dtl-row > div:nth-child(5) {width:6.59%;}
        .mem-dtl-row > div:nth-child(6) {width:54.9%;padding:0;}

        .id-dtl-row > div {padding:5px;}
        .id-dtl-row > div:nth-child(1) {width:49.98%;}
        .id-dtl-row > div:nth-child(2) {width:13.95%;}
        .id-dtl-row > div:nth-child(3) {width:8.77%;}
        .id-dtl-row > div:nth-child(4) {width:8.77%;}
        .id-dtl-row > div:nth-child(5) {width:8.77%;}
        .id-dtl-row > div:nth-child(6) {width:9.74%;}
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回上一頁</button>
            <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pd-add-main" disabled><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
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

        //計算商品小計及總計
        function pdCalc() {
            var pdTotal = 0, PdChkTotal = 0, memTotal = 0, memChkTotal = 0, amt;
            if ($(".rd-acctIn:checked").val() == "A") {                
                $(".kind-A").find(".node-mem").each(function () {
                    $(this).find(".mem-dtl-row").each(function () {
                        amt = parseInt($(this).find(".outLeft-amt").attr("data-val"));
                        memTotal += amt;
                        if ($(this).find(".prg-ckbox:not(:disabled)").prop("checked")) {
                            memChkTotal += amt;
                        }
                    });
                    $(this).find(".amt-total").text(memTotal);
                    $(this).find(".amt-chk-total").text(memChkTotal == 0 ? "" : "/" + memChkTotal);
                    pdTotal += memTotal;
                    PdChkTotal += memChkTotal;
                    memTotal = 0;
                    memChkTotal = 0;
                });

                $("#outStk-total").text(pdTotal);
                $("#outStk-chk-total").text(PdChkTotal == 0 ? "" : "/" + PdChkTotal);
                //$("#btn-submit").prop("disabled", $(".prg-ckbox:visible:checked").length == 0);    //無任何勾選則隱藏送出按鈕
            } else {
                var id = $("#mem").val();
                $(".kind-B .mem-" + id).find(".outLeft-amt").each(function () {
                    memTotal += parseInt($(this).attr("data-val"));
                });
                $("#outStk-total").text(memTotal);
                $("#outStk-chk-total").text("");
                //$("#btn-submit").prop("disabled", memTotal == 0);
            }
            SwitchSubmit();
        }

        function ChkAllChkbox() {
            $("#all-ckbox").prop("checked", $(".prg-ckbox:visible:not(:checked)").length == 0);
        }

        function SwitchKind() {
            if ($(".rd-acctIn:checked").val() == "A") {
                $(".kind-A").show();
                $(".kind-B").hide();
                pdCalc();
                MakePrintURL();
            } else {
                $(".kind-A").hide();
                $(".kind-B").show();
                SwitchMem();
            }
        }

        function SwitchMem() {
            var id = $("#mem").val();
            $("#mem-amt").val("");
            $(".kind-B").find(".node-mem").hide();
            $(".kind-B").find(".mem-" + id).show();
            pdCalc();
            MakePrintURL();
        }

        function SwitchSubmit() {
            var isClose = true;
            if ($(".rd-acctIn:checked").val() == "A") {
                isClose = $(".prg-ckbox:visible:checked").length == 0;
            } else {
                var memAmt = $("#mem-amt").val();
                isClose = $("#outStk-total").text() == ""
                    ? true
                    : (memAmt == "" || memAmt == "0");
            }
             $("#btn-submit").prop("disabled", isClose);    //無任何勾選則隱藏送出按鈕
        }

        function MakePrintURL() {
            var kind = $(".rd-acctIn:checked").val();
            var url = "prg5003PRN.aspx?kind=" + kind;
            if (kind == "B") {
                url += "&mem=" + $("#mem").val();
            }
            $("#btn-print").attr("href", url);
        }

        function Initial() {
            SwitchKind();
            pdCalc();
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

            //--------------勾選控制--[START]--------------
            $("#all-ckbox").click(function () {
                $(".ckbox").prop("checked", $(this).prop("checked"));
                pdCalc();
            });

            $(".node-ckbox").click(function () {
                $(this).closest(".node-main").find(".prg-ckbox").prop("checked", $(this).is(":checked"));
                pdCalc();
                ChkAllChkbox()
            });

            $(".prg-ckbox").click(function () {
                var $obj = $(this).closest(".node-main");
                $obj.find(".node-ckbox").prop("checked", $obj.find(".prg-ckbox:visible:not(:checked)").length == 0);

                pdCalc();
                ChkAllChkbox();
            });
            //--------------勾選控制--[END]--------------

            $(".rd-acctIn").change(function () {
                SwitchKind();
            });

            $("#mem").change(function () {
                SwitchMem();
            });

            $("#mem-amt").blur(function () {
                var total = $("#outStk-total").text();
                var val = $(this).val();
                var regex = /^[0-9]*$/;
                var isClose = true;
                isClose = (total == "" || val == "")
                    ? true
                    : (parseInt(val) == 0 || !regex.exec(val))
                        ? true
                        : parseInt(val) > parseInt(total)
                            ? true
                            : false;
                $("#btn-submit").prop("disabled", isClose);    //開啟送出按鈕
                if (parseInt(val) == 0) {
                    error_focus("銷帳金額不可為 0 !", $(this));
                }else if (parseInt(val) > parseInt(total)) {
                    error_focus("銷帳金額不可大於" + total + "!", $(this));
                }

            });

            //確定送出銷貨單
            $("#btn-submit").click(function () {
                var ouStk_list = "";
                var kind= $(".rd-acctIn:checked").val();
                if (kind == "A") {
                    $("#outStk-list-main .mem-dtl-row").find(".prg-ckbox:not(:disabled):checked").each(function () {
                        var $this = $(this);
                        ouStk_list += (ouStk_list == "" ? "" : ",") +
                            "{'sn':'" + $this.attr("data-id") + "'}";
                    });
                    ouStk_list = "[" + ouStk_list + "]";
                    console.log(ouStk_list);
                } else {

                }

                $.ajax({
                    url: "/AjaxOutStk.aspx",
                    type: "POST",
                    async: false,
                    data: {
                        args0: "C14",
                        AcctInDate: $("#acctInDate").val(),
                        ouStkList: ouStk_list,
                        kind: $(".rd-acctIn:checked").val(),
                        mem: $("#mem").val(),
                        memAmt: $("#mem-amt").val()
                    },
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
                                alert("銷貨清單有誤！");
                                break;
                            case "Y":
                                alert("新增成功！");
                                //location.href = "/prg50/prg5004.aspx";
                                location.reload();
                                break;
                            default:
                                console.log(response);
                                alert("新增失敗請聯絡工程人員！");
                        }
                    }
                });
            });

            //$("#btn-prn").click(function () {
            //    //window.open("prg5003PRN.aspx");
            //    var title = "<div class='rptr-title'>應收帳款明細表</div>" +
            //        "<div style='text-align:right;font-size:14px;width:100%;'>列印日期︰" + $.date(new Date()) + "</div>";
            //    $(".list-main").printThis(
            //        {
            //            printContainer: true,
            //            importCSS: true,
            //            loadCSS: "/ext/css/5003print.css",
            //            header: title
            //        });
            //});
        });
    </script>

        <script type="text/javascript">
            function DownloadFile() {
                $.ajax({
                    type: "POST",
                    url: "prg5003.aspx/SaveExcel",
                    data: "",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (r) {
                        //Convert Base64 string to Byte Array.
                        var bytes = Base64ToBytes(r.d);

                        //Convert Byte Array to BLOB.
                        var blob = new Blob([bytes], { type: "application/octetstream" });

                        //Check the Browser type and download the File.
                        var isIE = false || !!document.documentMode;
                        if (isIE) {
                            window.navigator.msSaveBlob(blob, "5003應收帳款銷帳.xlsx");
                        } else {
                            var url = window.URL || window.webkitURL;
                            link = url.createObjectURL(blob);
                            var a = $("<a />");
                            a.attr("download", "5003應收帳款銷帳.xlsx");
                            a.attr("href", link);
                            $("body").append(a);
                            a[0].click();
                            $("body").remove(a);
                        }
                    }
                });
            };
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
