<template>
    <div class="">
        <Chart
            type="bar"
            :data="setPieChartData()"
            class="flex-grow-1 mb-3 border-1 border-200 p-3"
            :options="pieChartOptions"
            :plugins="[ChartDataLabels]"
        />

        <div class="border-1 border-200">
            <div class="p-3 border-bottom-1 border-200 font-bold surface-200">
                Số Dư Hiện Tại
            </div>
            <div class="p-3">
                Công nợ tín chấp:
                <span class="font-semibold ml-3" :class="dataset.credit.severity">{{
                    dataset.credit.currentLabel
                }}</span>
                /
                <span class="text-primary font-semibold">{{
                    dataset.credit.limitLabel
                }}</span>
            </div>
            <div class="p-3 pt-0">
                Công nợ bảo lãnh:
                <span class="font-semibold ml-3" :class="dataset.guaranteed.severity">{{
                    dataset.guaranteed.currentLabel
                }}</span>
                /
                <span class="text-primary font-semibold">{{
                    dataset.guaranteed.limitLabel
                }}</span>
            </div>
        </div>
        <!-- {{ dataset }} -->
    </div>
</template>

<script setup>
import { ref, onMounted, reactive } from "vue";
import ChartDataLabels from "chartjs-plugin-datalabels";
import formater from "../../../../../../helpers/format.helper";
import { useMeStore } from "../../../../../../Pinia/me";

const meStore = useMeStore();
const dataset = reactive({
    credit: {
        label: "Công nợ tín chấp",
        code: "PayCredit",
        limit: 0,
        current: 0,
        balance: 0,
        limitLabel: "",
        currentLabel: "",
        balanceLabel: "",
        severity: "text-primary",
        //---------------------------
        remainPercent: 0,
        curentPercent: 0,
    },
    guaranteed: {
        label: "Công nợ bảo lãnh",
        code: "PayGuarantee",
        limit: 0,
        current: 0,
        balance: 0,
        limitLabel: "",
        currentLabel: "",
        balanceLabel: "",
        severity: "text-primary",
        //---------------------------
        remainPercent: 0,
        curentPercent: 0,
    },
    init() {
        this.credit.limitLabel = formater.FormatCurrency(this.credit.limit);
        this.credit.currentLabel = formater.FormatCurrency(this.credit.current);
        this.credit.balance = this.credit.limit - this.credit.current;
        if (this.credit.balance < 0) {
            this.credit.severity = "text-red-500";
        }
        this.credit.balanceLabel = formater.FormatCurrency(this.credit.balance);
        this.credit.curentPercent = calcPer(this.credit.current, this.credit.limit);
        this.credit.remainPercent = 100 - this.credit.curentPercent;

        this.guaranteed.limitLabel = formater.FormatCurrency(this.guaranteed.limit);
        this.guaranteed.currentLabel = formater.FormatCurrency(this.guaranteed.current);
        this.guaranteed.balance = this.guaranteed.limit - this.guaranteed.current;
        if (this.guaranteed.balance < 0) {
            this.guaranteed.severity = "text-red-500";
        }
        this.guaranteed.balanceLabel = formater.FormatCurrency(this.guaranteed.balance);
        this.guaranteed.curentPercent = calcPer(
            this.guaranteed.current,
            this.guaranteed.limit
        );
        this.guaranteed.remainPercent = 100 - this.guaranteed.curentPercent;
    },
});
const calcPer = (value, limit) => {
    const result = Math.floor((value * 100) / limit);
    return result;
};

const rh = (value) => {
    if (value < 0) {
        return 0;
    }
    return value;
};

const setPieChartData = () => {
    const documentStyle = getComputedStyle(document.body);

    return {
        labels: ["Công nợ tín chấp", "Công nợ bảo lãnh"],
        datasets: [
            {
                label: "Công nợ hiện tại (%)",
                data: [
                    rh(dataset.credit.curentPercent),
                    rh(dataset.guaranteed.curentPercent),
                ],
                backgroundColor: "rgba(75, 192, 192, 0.8)",
            },
            {
                label: "Còn lại (%)",
                data: [
                    rh(dataset.credit.remainPercent),
                    rh(dataset.guaranteed.remainPercent),
                ],
                backgroundColor: "rgba(200, 200, 200, 0.8)",
            },
        ],
    };
};

const pieChartOptions = ref({
    indexAxis: "y",
    responsive: true,
    plugins: {
        datalabels: {
            display: true,
            color: "#000",
            formatter: (value, data) => {

                return `${value}%`;
            },
            font: { weight: "bold" },
        },
        legend: { position: "top" },
    },
    scales: {
        x: {
            stacked: true,
            beginAtZero: true,
            max: 100,
            // ticks: { callback: (value) => `${value}%` },
        },
        y: { stacked: true },
    },
});

onMounted(async () => {
    const me = await meStore.getMe();
    const data = me?.user?.bpInfo.crD3;

    // Công nợ tín chấp
    dataset.credit.limit = data.find(
        (item) => item.paymentMethodCode == dataset.credit.code
    )?.balanceLimit;
    dataset.credit.current = data.find(
        (item) => item.paymentMethodCode == dataset.credit.code
    )?.balance;

    // Công nợ bảo lãnh
    dataset.guaranteed.limit = data.find(
        (item) => item.paymentMethodCode == dataset.guaranteed.code
    )?.balanceLimit;
    dataset.guaranteed.current = data.find(
        (item) => item.paymentMethodCode == dataset.guaranteed.code
    )?.balance;

    dataset.init();
});
</script>

<style></style>
