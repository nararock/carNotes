﻿//при открытии страницы Статистика выполняется:
document.addEventListener("DOMContentLoaded", goToCommonStatistic);

/**
 * переменная для хранения объекта для построения графика*/
var myChart;

/**
 * активирует календарь на странице Статистика*/
function activeStatisticCalendar(vehicleId) {
    moment.locale('ru');
    $('input[name="daterangeFuelFlow"]').daterangepicker({
        ranges: {
            'Сегодня': [moment(), moment()],
            'Вчера': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'За неделю': [moment().subtract(6, 'days'), moment()],
            'За месяц': [moment().subtract(29, 'days'), moment()],
            'Этот месяц': [moment().startOf('month'), moment().endOf('month')],
            'Прошлый месяц': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        },
        startDate: moment().subtract(90, 'days'),
        endDate: moment(),
        "alwaysShowCalendars": true
    }, function (start, end, label) {
        goToFuelFlowStatistic(vehicleId, start.format("DD.MM.YYYY"), end.format("DD.MM.YYYY"));
        /*document.location = "/Statistic/GetDataForFuelFlowStatistic?vehicleId=" + vehicleId + "&dateFrom=" + start + "&dateTo=" + end;*/
        /*console.log("A new date selection was made: " + start.format('YYYY-MM-DD') + ' to ' + end.format('YYYY-MM-DD'));*/
    });
};


/**
 * Метод для получения данных о расходах (заправки, ремонты, иные расходы) для построения круговой диаграммы "Общая статистика".
 * Вызывается при загрузке страницы "Статистика".*/
function goToCommonStatistic() {
    var elementReference = document.getElementsByClassName("CommonStatistic");
    var anotherElementReference = document.getElementsByClassName("FuelFlowStatistic");
    if (!elementReference[0].classList.contains("active")) {
        elementReference[0].classList.add("active");
        anotherElementReference[0].classList.remove("active");
    }

    var elementDaterange = document.getElementsByClassName("daterangeFuelFlow");
    elementDaterange[0].style.display = "none";

    fetch("/Statistic/GetDataForCommonStatistic?vehicleId=" + vehicleId)
        .then(response => response.json())
            .then((allExpenseData) => {
                const labels = [];
                const dataPie = [];
                const color = [];
                for (var i = 0; i < allExpenseData.length; i++) {
                    labels.push(allExpenseData[i].Name);
                    dataPie.push(allExpenseData[i].Cost);
                    color.push(allExpenseData[i].Color);
                }
                const data = {
                    labels: labels,
                    datasets: [{
                        label: 'Все расходы',
                        backgroundColor: color,
                        data: dataPie,
                    }]
                };
                const config = {
                    type: 'pie',
                    data: data,
                    options: {
                        plugins: {
                            legend: {
                                labels: {
                                    font: {
                                        size: 14
                                    }
                                }
                            }
                        }
                    }
                };
                var parentCanvas = document.getElementById('ChartWrapper');
                parentCanvas.classList.add('pieChart');//присваиваем класс для контроля размером в css стилях
                if (parentCanvas.classList.contains('barChart')) {
                    parentCanvas.classList.remove('barChart')
                }
                var canvas = document.getElementById('Chart');
                if (typeof myChart != 'undefined') {//при каждом переходе к новому графику нужно удалять данные переменной chart и чистить canvas
                    const context = canvas.getContext('2d');
                    context.clearRect(0, 0, canvas.width, canvas.height);
                    var w = canvas.width;
                    canvas.width = 1;
                    canvas.width = w;
                    myChart.destroy();
                }
                 myChart = new Chart(
                    canvas,
                    config
                );
            })
}

function goToFuelFlowStatistic(vehicleId, dateFrom, dateTo)
{
    var elementReference = document.getElementsByClassName("FuelFlowStatistic");
    var anotherElementReference = document.getElementsByClassName("CommonStatistic");
    if (!elementReference[0].classList.contains("active")) {
        elementReference[0].classList.add("active");
        anotherElementReference[0].classList.remove("active");
    }

    var elementDaterange = document.getElementsByClassName("daterangeFuelFlow");
    elementDaterange[0].style.display = "inline";
    activeStatisticCalendar(vehicleId);

    fetch("/Statistic/GetDataForFuelFlowStatistic?vehicleId=" + vehicleId + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo)
        .then(response => response.json())
        .then((dataFuelFlowStatistic) => {
            const labels = [];
            const dataBar = [];
            for (var i = 0; i < dataFuelFlowStatistic.length; i++) {
                labels.push(dataFuelFlowStatistic[i].Date);
                dataBar.push(dataFuelFlowStatistic[i].Cost);
            }
            const data = {
                labels: labels,
                datasets: [{
                    barThickness: 25,
                    data: dataBar,
                    backgroundColor: "#a333c8"
                }]
            };

            const config = {
                type: 'bar',
                data: data,
                options: {
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    indexAxis: 'x',
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                },
            };
            var parentCanvas = document.getElementById('ChartWrapper');
            parentCanvas.classList.add('barChart');
            if (parentCanvas.classList.contains('pieChart')) {
                parentCanvas.classList.remove('pieChart')
            }
            var canvas = document.getElementById('Chart');
            const context = canvas.getContext('2d');
            context.clearRect(0, 0, canvas.width, canvas.height);
            var w = canvas.width;
            canvas.width = 1;
            canvas.width = w;
            myChart.destroy();
            myChart = new Chart(
                canvas,
                config
            );
        })
}
