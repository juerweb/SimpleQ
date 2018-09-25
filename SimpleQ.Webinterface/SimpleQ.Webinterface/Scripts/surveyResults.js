function switchToMulti() {
    //changeChartMulti('chart1SelectMulti', 'chart1Col', 'chart1');
    //changeChartMulti('chart2SelectMulti', 'chart2Col', 'chart2');
    $("#trendBtn").removeClass("btn-primary").addClass("btn-secondary");
    $("#singleBtn").removeClass("btn-secondary").addClass("btn-primary");
    $("#trendBtn").attr("disabled", true);
    $("#singleBtn").attr("disabled", false);
    $("#singleResultOptions").hide();
    $("#multiResultOptions").show();
    $("#singleResultPartial").hide();
    $("#multiResultPartial").show();
}

function switchToSingle() {
    //changeChartMulti('chart1SelectSingle', 'chart1Col', 'chart1');
    //changeChartMulti('chart2SelectSingle', 'chart2Col', 'chart2');
    $("#singleBtn").removeClass("btn-primary").addClass("btn-secondary");
    $("#trendBtn").removeClass("btn-secondary").addClass("btn-primary");
    $("#singleBtn").attr("disabled", true);
    $("#trendBtn").attr("disabled", false);
    $("#multiResultOptions").hide();
    $("#singleResultOptions").show();
    $("#multiResultPartial").hide();
    $("#singleResultPartial").show();
}
