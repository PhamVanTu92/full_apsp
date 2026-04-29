<template>
    <div class="h-full"> 
        <Chart type="bar" :data="chartData" :options="chartOptions" class="h-full" />
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, PropType } from "vue";

import { useI18n } from 'vue-i18n';
const { t } = useI18n();

interface ChartSeries {
    name: string;
    data: number[];
}

interface ChartReport {
    title: string;
    categories: string[];
    series: ChartSeries[];
}

const chartData = ref();
const chartOptions = ref();
const props = defineProps({
    data: {
        required: true,
        type: Object as PropType<ChartReport>,
    },
});

const setChartOptions = () => {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue("--text-color");
    const textColorSecondary = documentStyle.getPropertyValue("--text-color-secondary");
    const surfaceBorder = documentStyle.getPropertyValue("--surface-border");

    return {
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            tooltips: {
                mode: "index",
                intersect: false,
            },
            legend: {
                labels: {
                    color: textColor,
                },
            },
        },
        scales: {
            x: {
                // stacked: true,
                ticks: {
                    color: textColorSecondary,
                    font: {
                        weight: 500,
                    },
                },
                grid: {
                    display: false,
                    drawBorder: false,
                },
            },
            y: {
                // stacked: true,
                ticks: {
                    color: textColorSecondary,
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false,
                },
            },
        },
    };
};

type DataSet = {
    label: string;
    data: number[];
    backgroundColor: string;
    borderColor: string;
};
const setChartData = (labels: string[], datasets: DataSet[]) => {
    return {
        labels: labels,
        datasets: datasets,
    };
};

const barColors = {
    ["Đang giao hàng"]: "--blue-700",
    ["Đã hoàn thành"]: "--green-200",
    ["Đang xử lý"]: "--yellow-200",
    ["Đã xác nhận"]: "--teal-200",
    ["Hủy"]: "--red-200",
    ["Chờ thanh toán"]: "--yellow-700",
    ["Đóng"]: "--gray-500",
    ["Chờ xác nhận"]: "--orange-700",
    ["Chờ xử lý"]: "--yellow-700",

};

const initialComponent = () => {
    // code here
    const documentStyle = getComputedStyle(document.documentElement);
    let datasets: DataSet[] = [];
    if (props.data) {
        datasets = props.data.series.map((item) => {
            return {
                label: item.name,
                data: item.data,
                backgroundColor: documentStyle.getPropertyValue(
                    barColors[item.name as keyof typeof barColors]
                ),
                borderColor: documentStyle.getPropertyValue(
                    barColors[item.name as keyof typeof barColors]
                ),
            };
        });
    }
    chartData.value = setChartData(props.data?.categories || [], datasets);
    chartOptions.value = setChartOptions();
};

watch(
    () => JSON.stringify(props.data),
    () => {
        initialComponent();
    }
);

onMounted(function () {
    initialComponent();
});
</script> 
