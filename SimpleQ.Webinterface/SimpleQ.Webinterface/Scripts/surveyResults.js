function switchToMulti() {
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
    $("#singleBtn").removeClass("btn-primary").addClass("btn-secondary");
    $("#trendBtn").removeClass("btn-secondary").addClass("btn-primary");
    $("#singleBtn").attr("disabled", true);
    $("#trendBtn").attr("disabled", false);
    $("#multiResultOptions").hide();
    $("#singleResultOptions").show();
    $("#multiResultPartial").hide();
    $("#singleResultPartial").show();
}

//function changeChart1Multi() {
//    document.getElementById("chart1MultiCol").innerHTML = '<canvas id="chart1Multi" width="800" height="500"></canvas>';
//    var canvas = document.getElementById("chart1Multi");
//    var ctx = canvas.getContext('2d');

//    var myChart = new Chart(ctx, {
//        type: 'line', /*document.getElementById("chart1SelectMulti").value,*/
//        data: {
//            labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
//            datasets: [{
//                label: '# of Votes',
//                data: [113, 192, 333, 45, 112, 23],

//                borderColor: [
//                    'rgba(54, 162, 235, 1)'
//                ],
//                borderWidth: 1
//            }, {
//                label: '# of Votes',
//                data: [130, 80, 200, 112, 112, 200],
//                borderColor: [
//                    'rgba(255,99,132,1)'
//                ],
//                borderWidth: 1
//            }]
//        },
//        options: {
//            scales: {
//                yAxes: [{
//                    ticks: {
//                        beginAtZero: true
//                    }
//                }]
//            }
//        }
//    });
//}
//function changeChart2Multi() {
//    document.getElementById("chart2MultiCol").innerHTML = '<canvas id="chart2Multi" width="800" height="500"></canvas>';
//    var canvas = document.getElementById("chart2Multi");
//    var ctx = canvas.getContext('2d');
//    var myChart2 = new Chart(ctx, {
//        type: document.getElementById("chart2SelectMulti").value,
//        data: {
//            labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
//            datasets: [{
//                label: '# of Votes',
//                data: [12, 19, 3, 5, 2, 3],
//                backgroundColor: [
//                    'rgba(255, 99, 132, 0.2)',
//                    'rgba(54, 162, 235, 0.2)',
//                    'rgba(255, 206, 86, 0.2)',
//                    'rgba(75, 192, 192, 0.2)',
//                    'rgba(153, 102, 255, 0.2)',
//                    'rgba(255, 159, 64, 0.2)'
//                ],
//                borderColor: [
//                    'rgba(255,99,132,1)',
//                    'rgba(54, 162, 235, 1)',
//                    'rgba(255, 206, 86, 1)',
//                    'rgba(75, 192, 192, 1)',
//                    'rgba(153, 102, 255, 1)',
//                    'rgba(255, 159, 64, 1)'
//                ],
//                borderWidth: 1
//            }]
//        }
//    });
//}

//function changeChart1Single() {
//    document.getElementById("chart1Col").innerHTML = '<canvas id="chart1" width="800" height="500"></canvas>';
//    var canvas = document.getElementById("chart1");
//    var ctx = canvas.getContext('2d');
//    var myChart = new Chart(ctx, {
//        type: document.getElementById("chart1SelectSingle").value,
//        data: {
//            labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
//            datasets: [{
//                label: '# of Votes',
//                data: [113, 192, 333, 45, 112, 23],
//                backgroundColor: [
//                    'rgba(255, 99, 132, 0.2)',
//                    'rgba(54, 162, 235, 0.2)',
//                    'rgba(255, 206, 86, 0.2)',
//                    'rgba(75, 192, 192, 0.2)',
//                    'rgba(153, 102, 255, 0.2)',
//                    'rgba(255, 159, 64, 0.2)'
//                ],
//                borderColor: [
//                    'rgba(255,99,132,1)',
//                    'rgba(54, 162, 235, 1)',
//                    'rgba(255, 206, 86, 1)',
//                    'rgba(75, 192, 192, 1)',
//                    'rgba(153, 102, 255, 1)',
//                    'rgba(255, 159, 64, 1)'
//                ],
//                borderWidth: 1
//            }]
//        },
//        options: {
//            scales: {
//                yAxes: [{
//                    ticks: {
//                        beginAtZero: true
//                    }
//                }]
//            }
//        }
//    });
//}
//function changeChart2Single() {
//    document.getElementById("chart2Col").innerHTML = '<canvas id="chart2" width="800" height="500"></canvas>';
//    var canvas = document.getElementById("chart2");
//    var ctx = canvas.getContext('2d');
//    var myChart2 = new Chart(ctx, {
//        type: document.getElementById("chart2SelectSingle").value,
//        data: {
//            labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
//            datasets: [{
//                label: '# of Votes',
//                data: [12, 19, 3, 5, 2, 3],
//                backgroundColor: [
//                    'rgba(255, 99, 132, 0.2)',
//                    'rgba(54, 162, 235, 0.2)',
//                    'rgba(255, 206, 86, 0.2)',
//                    'rgba(75, 192, 192, 0.2)',
//                    'rgba(153, 102, 255, 0.2)',
//                    'rgba(255, 159, 64, 0.2)'
//                ],
//                borderColor: [
//                    'rgba(255,99,132,1)',
//                    'rgba(54, 162, 235, 1)',
//                    'rgba(255, 206, 86, 1)',
//                    'rgba(75, 192, 192, 1)',
//                    'rgba(153, 102, 255, 1)',
//                    'rgba(255, 159, 64, 1)'
//                ],
//                borderWidth: 1
//            }]
//        }
//    });
//}