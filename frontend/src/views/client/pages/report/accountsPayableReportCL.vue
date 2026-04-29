<template>
    <div>
        <div class="flex justify-content-between align-content-center">
            <h4 class="font-bold m-0">Báo cáo công nợ phải trả</h4>
            <div class="flex gap-2">
                <Button
                    label="In báo cáo"
                    icon="pi pi-print"
                    outlined
                    severity="warning"
                />
                <Button
                    label="Xuất excel"
                    icon="pi pi-file-export"
                    outlined
                    severity="info"
                />
                <Button
                    label="Bộ lọc"
                    icon="pi pi-filter"
                    @click="visibleFilter = true"
                />
            </div>
        </div>
        <div class="grid mx-4 my-5">
            <div class="col-6">
                <div class="flex gap-8">
                    <div>Thời gian:</div>
                    <div>-</div>
                </div>
            </div>
            <div class="col-6">
                <div class="flex gap-8">
                    <div>Hình thức thanh toán:</div>
                    <div>-</div>
                </div>
            </div>
        </div>

        <hr />
        <div class="card">
            <div class="flex flex-column gap-8">
                <div class="card">
                    <Chart
                        type="bar"
                        :data="chartData"
                        :options="chartOptions"
                        class="h-30rem"
                    />
                </div>

                <div>
                    <DataTable
                        :value="[{}, {}, {}]"
                        showGridlines
                        tableStyle="min-width: 50rem"
                    >
                        <Column header="#" style="width: 3rem">
                            <template #body="{ index }">{{ index + 1 }}</template>
                        </Column>
                        <Column header="Tuổi nợ (ngày)" style="width: 10rem"></Column>
                        <Column header="Thanh toán ngay" style="width: 15rem"></Column>
                        <Column header="Công nợ tín chấp" style="width: 10rem"></Column>
                        <Column header="Công nợ bảo lãnh" style="width: 10rem"></Column>
                        <Column header="Tổng" style="width: 10rem"></Column>
                    </DataTable>
                </div>
            </div>
        </div>

        <Dialog v-model:visible="visibleFilter" header="Bộ lọc" style="width: 35%" modal>
            <div class="flex justify-content-between align-content-evenly card">
                <div class="grid w-full">
                    <div class="col-6 flex flex-column gap-2">
                        <label class="font-bold" for="">Thời gian</label>
                        <Calendar
                            v-model="dates"
                            selectionMode="range"
                            :manualInput="false"
                            placeholder="Từ ngày - đến ngày"
                        />
                    </div>

                    <div class="col-6 flex flex-column gap-2">
                        <label class="font-bold" for="">Hình thức thanh toán</label>
                        <MultiSelect placeholder=" Hình thức thanh toán "></MultiSelect>
                    </div>
                </div>
            </div>
            <template #footer>
                <div class="flex justify-content-end gap-2">
                    <Button
                        type="button"
                        label="Bỏ qua"
                        severity="secondary"
                        @click="visibleFilter = false"
                    />
                    <Button
                        type="button"
                        label="Xác nhận"
                        @click="visibleFilter = false"
                    />
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
const chartData = ref();
const chartOptions = ref();

const visibleFilter = ref(true);
const visibleDetail = ref(false);

onMounted(() => {
    chartData.value = setChartData();
    chartOptions.value = setChartOptions();
});

const goBack = () => {
    window.history.back();
};

const setChartData = () => {
    const documentStyle = getComputedStyle(document.documentElement);

    return {
        labels: ["0-30", "31-60", "61-90", "91-120", "121+"],
        datasets: [
            {
                label: "Thanh toán ngay",
                backgroundColor: documentStyle.getPropertyValue("--cyan-600"),
                borderColor: documentStyle.getPropertyValue("--cyan-600"),
                data: [15.0, 12.1, 0, 0, 0],
            },
            {
                label: "Công nợ tín chấp",
                backgroundColor: documentStyle.getPropertyValue("--orange-500"),
                borderColor: documentStyle.getPropertyValue("--orange-500"),
                data: [134.7, 0, 33.2, 4.6],
            },
            {
                label: "Công nợ bảo lãnh",
                backgroundColor: documentStyle.getPropertyValue("--gray-500"),
                borderColor: documentStyle.getPropertyValue("--gray-500"),
                data: [36.02, 5.8, 10.2, 0, 0.402],
            },
        ],
    };
};

const setChartOptions = () => {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue("--text-color");
    const textColorSecondary = documentStyle.getPropertyValue("--text-color-secondary");
    const surfaceBorder = documentStyle.getPropertyValue("--surface-border");

    return {
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            legend: {
                labels: {
                    color: textColor,
                },
            },
        },
        scales: {
            x: {
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
</script>
