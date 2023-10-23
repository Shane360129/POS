//新增付款方式
function PayKindAdd() {
    var a = [];
    $("#pay-list-grp .pay-kind").each(function () {
        $this = $(this);
        $.each(a, function (key, value) {
            $this.find("option[value='" + value + "']").remove();
        });

        if ($this.find("option").length == 0) {
            $this.closest(".pay-row").remove();
        } else {
            a.push($(this).val());
        }
    });

    ChkPayKind();
}

//重整付款方式選項
function ChkPayKind() {
    var a = [];
    $objGrp = $("#pay-list-grp");
    $objGrp.find(".pay-kind option:selected").each(function (index) {
        a.push($(this).val());
    });

    var oa = [];
    var $objAdd = $("#pk-list-add");
    $objAdd.find("option").each(function () {
        oa.push($(this).val());
    });

    $objGrp.find(".pay-kind").each(function () {
        var add_oa = oa.filter(x => !a.includes(x));
        var $this = $(this);
        var val = $this.find("option:selected").val();
        add_oa.push(val);
        $this.empty();

        $.each(add_oa.sort(), function (key, value) {
            $this.append($objAdd.find("option[value='" + value + "']").clone());
        });
        $this.find("option[value='" + val + "']").prop("selected", true);
    });

    if ($objGrp.find(".pay-row").length == 1) {
        $objGrp.find(".pay-amt").hide();
    } else {
        $objGrp.find(".pay-amt").show();
    }
}

$(function () {
    ChkPayKind();

    $(".btn-pay-add").click(function () {
        if (!$(this).hasClass("pay-disabled")) {
            var $obj = $("#pay-list-grp");
            var extObj = $('#pk-list-add').children().clone(true);
            $obj.append(extObj);
            PayKindAdd();
        }
    });

    $(".btn-pay-del").click(function () {
        if (!$(this).hasClass("pay-disabled")) {
            $(this).closest(".pay-row").remove();
            ChkPayKind();
        }
    });

    $(".pay-kind").change(function () {
        ChkPayKind();
    });
});