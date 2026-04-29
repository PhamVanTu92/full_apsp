<template>
    <div class="flex justify-content-center">
        <Chart
            type="pie"
            :data="setPieChartData()"
            :options="pieChartOptions"
            class="w-30rem h-30rem"
            :plugins="[ChartDataLabels]"
        />
    </div>
</template>

<script setup>
import { ref } from "vue";
const congNoTheoTrangThaiDatasets = ref([
    {
        name: "Trong hạn",
        value: 540,
        amount: "15,000,000 VNĐ",
    },
    {
        name: "Sắp đến hạn",
        value: 325,
        amount: "10,000,000 VNĐ",
    },
    {
        name: "Quá hạn",
        value: 250,
        amount: "8,000,000 VNĐ",
    },
]);
const setPieChartData = () => {
    return {
        labels: congNoTheoTrangThaiDatasets.value.map((el) => el.name),
        datasets: [
            {
                data: congNoTheoTrangThaiDatasets.value.map((el) => el.value),
                backgroundColor: [
                    "rgb(52, 121, 40)",
                    "rgb(77,180,77)",
                    "rgb(200, 0, 54)",
                ],
                hoverBackgroundColor: [
                    "rgba(32,102,38,0.7)",
                    "rgba(77,180,77,0.7)",
                    "rgba(200, 0, 54, 0.7)",
                ],
            },
        ],
    };
};

import ChartDataLabels from "chartjs-plugin-datalabels";
const pieChartOptions = {
    plugins: {
        datalabels: {
            color: "#fff",
            font: {
                weight: "bold",
                size: 14,
            },
            formatter: (value, context) => {

                const index = context.dataIndex;
                const percentage = (
                    (value /
                        context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0)) *
                    100
                ).toFixed(2);
                const label = context.chart.data.labels[context.dataIndex];
                return `${label}:\n${percentage}%`;
            },
        },
        legend: {
            labels: {
                render: "percentage",
                fontColor: function (data) {
                    var rgb = hexToRgb(data.dataset.backgroundColor[data.index]);
                    var threshold = 140;
                    var luminance = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                    return luminance > threshold ? "black" : "white";
                },
                precision: 2,
            },
        },
        tooltip: {
            callbacks: {
                label: function (tooltipItem) {
                    // const index = tooltipItem.dataIndex;
                    // const status = pieData.statuses[index];
                    const value =
                        congNoTheoTrangThaiDatasets.value[tooltipItem.dataIndex].value;
                    const amount =
                        congNoTheoTrangThaiDatasets.value[tooltipItem.dataIndex].amount;
                    return [`Số đơn hàng: ${value}`, `Tổng tiền: ${amount}`];
                },
            },
        },
    },
    options: {
        indexAxis: "y",
        responsive: true,
        plugins: {
            tooltip: {
                callbacks: {
                    label: function (tooltipItem) {
                        return `${tooltipItem.dataset.label}: ${tooltipItem.raw}%`;
                    },
                },
            },
            datalabels: {
                display: true,
                color: "#000",
                formatter: (value) => `${1}%`,
                font: { weight: "bold" },
            },
            legend: { position: "top" },
        },
        scales: {
            x: {
                stacked: true,
                beginAtZero: true,
                max: 100,
                ticks: { callback: (value) => `${value}%` },
            },
            y: { stacked: true },
        },
    },
};
</script>

<style></style>
