///NuGet 需下載 linq.js
document.write('<script src="/scripts/linq.min.js"></script>');
var jsonObj;
function loadPkJson() {
    $.ajax({
        url: "/AjaxMain.aspx",
        type: "POST",
        async: false,
        data: { args0: "B02" },
        dataType: "json",
        //如果連線成功        	   
        success: function (data) {
            jsonObj = data;
        },
        error: function (xhr) {
            console.log(xhr.responseText);
            alert('Ajax request 發生錯誤--loadPkJson()--請洽工程人員');
        },
    });
}

function chk_add_btn() {    //判斷分類是否已選擇，是則出現新增button
    $(".PKind:visible option[value=0]:selected").length > 0 ? $(".btn-PK-add").hide() : $(".btn-PK-add").show();
}

function chang_Pkind(_$obj) {
    var $objGrp = _$obj.closest(".pk-select-grp");
    var pKind = _$obj.attr("data-id");
    var value = _$obj.val();
    switch (pKind) {
        case "PKL":
            var $obj = $objGrp.find(".PKMPd");
            $obj.empty();
            if (value == "0") {
                $obj.hide();
            } else {
                var linqPKM = linqResult("PKM", value);
                if (linqPKM.length == 0) {
                    $obj.hide();
                } else {
                    $obj.show();
                    $obj.append("<OPTION value='0'>請選擇</OPTION>");
                    $.each(linqPKM, function (i, item) {
                        $obj.append("<option value='" + item.Id + "'>" + item.Id + " " + item.Name + "</option>");
                    });
                }
            }
            $objGrp.find(".PKSPd").empty().hide();
            break;
        case "PKM":
            var $obj = $objGrp.find(".PKSPd");
            $obj.empty();
            if (value == "0") {
                $obj.hide();
            } else {
                var linqPKM = linqResult("PKS", value);
                if (linqPKM.length == 0) {
                    $obj.hide();
                } else {
                    $obj.show();
                    $obj.append("<OPTION value='0'>請選擇</OPTION>");
                    $.each(linqPKM, function (i, item) {
                        $obj.append("<option value='" + item.Id + "'>" + item.Id + " " + item.Name + "</option>");
                    });
                }
            }
            break;
    }
    chk_add_btn();
}

function addPKind() {
    var $obj = $(".pk-grp-main");
    //---判斷Add-btn
    var emptyPK = $obj.find(".PKind option[value=0]:selected").length;      //是否有商品分類尚未選取
    if (emptyPK != 0) {
        var $objFocus = $obj.find(".PKind option[value=0]:selected").first().closest("select");
        error_focus("請先完成分類選擇！", $objFocus);
    }
    else {
        var extendObject = $('#append-pkind').children().clone(true);
        $obj.append(extendObject);
        makeSelectPK($obj.find(".pk-select-grp").last());
        chang_Pkind($obj.find(".PKLPd").last());
    }
    chk_add_btn();
}

function delPKind(_obj) {
    var $objMain = $(".pk-grp-main");
    var $obj = _obj.closest(".pk-select-grp");
    if ($objMain.find(".btn-PK-del").length == 1) {
        $obj.find(".PKLPd").val("0");
        $obj.find(".PKMPd").empty().hide();
        $obj.find(".PKSPd").empty().hide();
    } else {
        $obj.remove();
    }
    chk_add_btn();
}

function makePKGrpToJson() {
    var jsonString = "";
    $(".pk-grp-main").find(".pk-select-grp").each(function () {
        jsonString += jsonString == "" ? "" : ",";
        jsonString += "{'PKL':'" + $(this).find(".PKLPd").val() + "','PKM':'" + $(this).find(".PKMPd").val() + "','PKS':'" + $(this).find(".PKSPd").val() + "'}";
    });
    $("#pkind-json").val("[" + jsonString + "]");
}

function initPkind() {
    loadPkJson();
    $(".pk-grp-main").find(".pk-select-grp").each(function () {
        makeSelectPK($(this));
    });
    chk_add_btn();
}

function makeSelectPK(_$obj) {
    var $obj = _$obj.find(".PKLPd");
    var id_L = $obj.attr("data-alt");
    var linqPKL = linqResult("PKL", "");
    $obj.empty().append("<option value='0'>請選擇</option>");
    $.each(linqPKL, function (i, item) {
        var selected = item.Id == id_L ? "selected" : "";
        $obj.append("<option value='" + item.Id + "' " + selected + ">" + item.Id + " " + item.Name + "</option>");
    });

    var $obj = _$obj.find(".PKMPd");
    var id_M = $obj.attr("data-alt");
    if (id_M == "0") {
        _$obj.find(".PKMPd").hide();
        _$obj.find(".PKSPd").hide();
    } else {
        var linqPKM = linqResult("PKM", id_L);
        $obj.empty().append("<option value='0'>請選擇</option>");
        $.each(linqPKM, function (i, item) {
            var selected = item.Id == id_M ? "selected" : "";
            $obj.append("<option value='" + item.Id + "' " + selected + ">" + item.Id + " " + item.Name + "</option>");
        });

        var $obj = _$obj.find(".PKSPd");
        var id_S = $obj.attr("data-alt");
        if (id_S == "0") {
            _$obj.find(".PKSPd").hide();
        } else {
            var linqPKS = linqResult("PKS", id_M);
            $obj.empty().append("<option value='0'>請選擇</option>");
            $.each(linqPKS, function (i, item) {
                var selected = item.Id == id_S ? "selected" : "";
                $obj.append("<option value='" + item.Id + "' " + selected + ">" + item.Id + " " + item.Name + "</option>");
            });
        }
    }

}

function linqResult(_pKind, _id) {
    var QueryResult = Enumerable.From(jsonObj);
    switch (_pKind) {
        case "PKL":
            var QueryResult = QueryResult
                //.OrderBy(function (o) { o.pKLId })
                .Distinct(function (d) { return d.pKLId })
                .Select(function (s) {
                    viewData = { Name: s.pKLName, Id: s.pKLId };
                    return viewData
                }).ToArray();
            break;
        case "PKM":
            var QueryResult = QueryResult
                .Where(function (w) { return (w.pKLId == _id && w.pKMId != "0") })
                //.OrderBy(function (o) { o.pKMId })
                .Distinct(function (d) { return d.pKMId })
                .Select(function (s) {
                    viewData = { Name: s.pKMName, Id: s.pKMId };
                    return viewData
                }).ToArray();
            break;
        case "PKS":
            var QueryResult = QueryResult
                .Where(function (w) { return (w.pKMId == _id && w.pKSId != "0") })
                //.OrderBy(function (o) { o.pKSId })
                .Distinct(function (d) { return d.pKSId })
                .Select(function (s) {
                    viewData = { Name: s.pKSName, Id: s.pKSId };
                    return viewData
                }).ToArray();
            break;
    }
    return QueryResult;
}


$(function () {
    initPkind();

    $(".PKind").change(function () { chang_Pkind($(this));})

    $(".btn-PK-add").click(function () { addPKind();})

    $(".btn-PK-del").click(function () { delPKind($(this));})

});