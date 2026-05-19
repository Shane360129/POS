/*
 *  使用時 class 務必以範例結構命名 data-id 為如有預設值，沒有可省略
 *  
 * *** 使用範例 ***
 <div class="city-change">
     <select class="city-id" data-id="A"></select>
     <select class="zone-id" data-id="104"></select>
 </div> 
 * 
 * cityChg.js 要 link 到使用頁面
 * path_city 路徑要配合實際路徑修改
 */

var jsonCity;
var path_city = "/ext/cityChg/";    //程式絕對路徑
function loadCityJson() {
    $.ajax({
        url: path_city + "AjaxCityChg.aspx",
        type: "POST",
        async: false,
        dataType: "json",
        //如果連線成功        	   
        success: function (data) {
            jsonCity = data;
        },
        error: function (xhr) {
            console.log(xhr.responseText);
            alert('Ajax request 發生錯誤--loadCityJson()--請洽工程人員');
        },
    });
}

function chang_city(_$obj) {
    var city_id = _$obj.val();
    var $obj_zone = _$obj.closest(".city-change").find(".zone-id");
    var zone_id = $obj_zone.attr("data-id");

    $obj_zone.empty();
    for (i = 0; i < jsonCity.length; i++) {
        if (city_id == jsonCity[i].cityId) {
            var selected = jsonCity[i].zoneId == zone_id ? "selected" : "";
            $obj_zone.append("<option value='" + jsonCity[i].zoneId + "' " + selected + ">" + jsonCity[i].zoneName + "</option>");
        }
    }
}

function initlCity() {
    loadCityJson();
    $(".city-change").each(function () {
        var $obj = $(this);
        var $obj_city = $obj.find(".city-id");
        var city_id = $obj_city.attr("data-id");
        $obj_city.empty();
        var preCityId = "";
        for (i = 0; i < jsonCity.length; i++) {
            if (preCityId != jsonCity[i].cityId) {
                var selected = jsonCity[i].cityId == city_id ? "selected" : "";
                $obj_city.append("<option value='" + jsonCity[i].cityId + "' " + selected + ">" + jsonCity[i].cityName + "</option>");
                preCityId = jsonCity[i].cityId;
            }
        }
        chang_city($obj_city);

    });
}

$(function () {
    initlCity();
    $(".city-id").change(function () { chang_city($(this)); });
});