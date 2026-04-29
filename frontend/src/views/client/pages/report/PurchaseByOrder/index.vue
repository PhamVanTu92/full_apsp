<template>
    <div>
        <div class="flex justify-content-center mb-3">
            <h4 class="font-bold m-0">Báo cáo mua hàng theo đơn hàng</h4>
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-bold">Thời gian</span>
            <Calendar
                v-model="query.startDate"
                class="w-10rem"
                placeholder="Từ ngày"
                :maxDate="new Date()"
            />
            <Calendar
                v-model="query.endDate"
                class="w-10rem"
                placeholder="Đến ngày"
                :minDate="query.startDate"
                :maxDate="new Date()"
            />
            <div class="flex gap-3 ml-5" v-if="false">
                <div>
                    <RadioButton
                        v-model="query.isContern"
                        class="mr-2"
                        :value="false"
                        inputId="isIncotermFalse"
                    ></RadioButton>
                    <label for="isIncotermFalse">Đơn hàng trong nước</label>
                </div>
                <div>
                    <RadioButton
                        v-model="query.isContern"
                        class="mr-2"
                        :value="true"
                        inputId="isIncotermTrue"
                    ></RadioButton>
                    <label for="isIncotermTrue">Đơn hàng xuất khẩu</label>
                </div>
            </div>
        </div>
        <hr />
        <div class="card relative">
            <div class="grid mb-3">
                <div class="col-12 lg:col-6">
                    <DataTable
                        showGridlines
                        :value="orderStatus"
                        resizableColumns
                        columnResizeMode="fit"
                    >
                        <Column field="label" header="Trạng thái" class="font-bold">
                            <template #body="{ data }">
                                <span :class="data.class"> {{ data.label }}</span>
                            </template>
                        </Column>
                        <Column
                            field="quantity"
                            class="text-right w-15rem"
                            header="Số lượng đơn hàng"
                        >
                            <template #body="{ data }">
                                <span>{{
                                    Intl.NumberFormat().format(data.quantity)
                                }}</span>
                            </template>
                        </Column>
                        <Column
                            field="value"
                            class="text-right w-15rem"
                            header="Tổng giá trị (VND)"
                        >
                            <template #body="{ data }">
                                <span>{{ Intl.NumberFormat().format(data.value) }}</span>
                            </template>
                        </Column>
                        <ColumnGroup type="footer">
                            <Row>
                                <Column footer="Tổng" class="surface-100"></Column>
                                <Column
                                    class="text-right surface-100"
                                    :footer="Intl.NumberFormat().format(totalQuantity)"
                                />
                                <Column
                                    class="text-right surface-100"
                                    :footer="Intl.NumberFormat().format(totalValue)"
                                />
                            </Row>
                        </ColumnGroup>
                    </DataTable>
                </div>
                <div class="col-12 lg:col-6">
                    <Chart
                        type="bar"
                        :data="setChartData()"
                        :options="setChartOptions()"
                        class="h-full border-1 p-3 border-200"
                    />
                </div>
            </div>
            <DataTable
                :value="orderData.items"
                showGridlines
                stripedRows
                paginator
                lazy
                :first="query.PageSize * query.Page"
                :rows="query.PageSize"
                :totalRecords="orderData.total"
                :rowsPerPageOptions="[10, 20, 30, 40, 50]"
                @page="onChagePage"
                :loading="loading.order"
            >
                <Column header="#" field="">
                    <template #body="{ index }">
                        {{ query.Page * query.PageSize + index + 1 }}
                    </template>
                </Column>
                <Column header="Mã đơn hàng" field="invoiceCode">
                    <template #body="{ data }">
                        <router-link
                            :to="`/purchase-order/${data.id}`"
                            class="text-primary font-bold hover:underline"
                        >
                            <span>{{ data.invoiceCode }}</span>
                        </router-link>
                    </template>
                </Column>
                <Column header="Trạng thái" field="status">
                    <template #body="{ data }">
                        <Tag
                            :value="getStatusTag(data.status).label"
                            :class="getStatusTag(data.status).class"
                        ></Tag>
                    </template>
                </Column>
                <Column header="Ngày đặt hàng" field="docDate">
                    <template #body="{ data }">
                        {{ format(data.docDate, "dd/MM/yyyy") }}
                    </template>
                </Column>
                <Column
                    header="Thành tiền nguyên giá"
                    field="paymentInfo.totalBeforeVat"
                    class="text-right"
                >
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.totalBeforeVat) }}
                    </template>
                </Column>
                <Column
                    header="Thưởng thanh toán ngay"
                    field="paymentInfo.bonusAmount"
                    class="text-right"
                >
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.bonusAmount) }}
                    </template>
                </Column>
                <Column
                    header="Thưởng sản lượng"
                    field="paymentInfo.bonusCommited"
                    class="text-right"
                >
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.bonusCommited) }}
                    </template>
                </Column>
                <Column
                    header="Tổng thanh toán"
                    field="paymentInfo.totalAfterVat"
                    class="text-right"
                >
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.totalAfterVat) }}
                    </template>
                </Column>
                <template #empty>
                    <div class="py-5 my-5 text-center text-500 font-italic">
                        Không có dữ liệu để hiển thị
                    </div>
                </template>
                <ColumnGroup type="footer">
                    <Row>
                        <Column footer="Tổng:" class="surface-100" :colspan="4"></Column>
                        <Column
                            :footer="formatNumber(orderData.footer.totalBeforeVat)"
                            class="text-right surface-100"
                        ></Column>
                        <Column
                            :footer="formatNumber(orderData.footer.bonusAmount)"
                            class="text-right surface-100"
                        ></Column>
                        <Column
                            :footer="formatNumber(orderData.footer.bonusCommited)"
                            class="text-right surface-100"
                        ></Column>
                        <Column
                            :footer="formatNumber(orderData.footer.totalAfterVat)"
                            class="text-right surface-100"
                        ></Column>
                    </Row>
                </ColumnGroup>
            </DataTable>
            <div
                v-if="0"
                style="background-color: rgba(0, 0, 0, 0.4)"
                class="absolute right-0 left-0 top-0 bottom-0 flex justify-content-center align-items-center"
            >
                <i class="pi pi-spin pi-spinner" style="font-size: 4rem"></i>
            </div>
        </div>
        <Loading v-if="loading.global"></Loading>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive, watch } from "vue";
import API from "@/api/api-main";
import { format } from "date-fns";

const currentDate = new Date();
const query = reactive({
    startDate: new Date(`${currentDate.getFullYear()}-01-01`),
    endDate: currentDate,
    Page: 0,
    PageSize: 10,
    status: "",
    isContern: false,
    toQueryStatsString: (): string => {
        return `?Page=${query.Page + 1}&PageSize=${
            query.PageSize
        }&OrderBy=${"id desc"}&startDate=${format(
            query.startDate,
            "yyyy-MM-dd"
        )}&endDate=${format(query.endDate, "yyyy-MM-dd")}&isContern=${query.isContern}`;
    },
});

const setChartData = () => {
    const documentStyle = getComputedStyle(document.documentElement);
    return {
        labels: ["Đang xử lý", "Đã xác nhận", "Đang giao hàng", "Hoàn thành", "Đã hủy"],
        datasets: [
            {
                label: "Đơn hàng",
                backgroundColor: [
                    "rgba(255, 205, 86)",
                    "rgba(75, 192, 192)",
                    "rgba(54, 162, 235)",
                    "rgba(71, 204, 98)",
                    "rgba(255, 70, 50)",
                ],
                data: orderStatus.value.map((item) => item.quantity),
            },
        ],
    };
};

const orderStatus = ref([
    {
        label: "Đang xử lý",
        quantity: 0,
        value: 0,
        class: "text-yellow-500",
        field: "processing",
    },
    {
        label: "Đã xác nhận",
        quantity: 0,
        value: 0,
        class: "text-teal-500",
        field: "confirmed",
    },
    {
        label: "Đang giao hàng",
        quantity: 0,
        value: 0,
        class: "text-blue-500",
        field: "onDelivery",
    },
    {
        label: "Hoàn thành",
        quantity: 0,
        value: 0,
        class: "text-green-500",
        field: "complete",
    },
    {
        label: "Đã hủy",
        quantity: 0,
        value: 0,
        class: "text-red-500",
        field: "cancelled",
    },
]);

const getSum = (field: string) => {
    return orderStatus.value.reduce((acc, curr: any) => acc + curr[field], 0);
};

const totalQuantity = computed(() => getSum("quantity"));
const totalValue = computed(() => getSum("value"));

const loading = reactive({
    global: false,
    order: false,
});

const onChagePage = (event: any) => {
    loading.order = true;
    query.Page = event.page;
    query.PageSize = event.rows;
    fetchStats(query.toQueryStatsString());
};
const fetchStats = (query: string) => {
    API.get("Report/order-state" + query)
        .then((res) => {
            orderStatus.value = orderStatus.value.map((row) => {
                return {
                    ...row,
                    value:
                        res.data?.order[
                            "total" + row.field[0].toUpperCase() + row.field.slice(1)
                        ],
                    quantity: res.data?.order[row.field],
                };
            });
            orderData.items = res.data.item;
            orderData.total = res.data.total;
            Object.keys(orderData.footer).forEach((key) => {
                orderData.footer[key as keyof typeof orderData.footer] = res.data[key];
            });
        })
        .catch()
        .finally(() => {
            loading.global = false;
            loading.order = false;
        });
};

const orderData = reactive({
    items: [],
    total: 0,
    footer: {
        totalBeforeVat: 0,
        bonusAmount: 0,
        bonusCommited: 0,
        totalAfterVat: 0,
    },
});

watch(
    () => {
        return {
            startDate: query.startDate,
            endDate: query.endDate,
            status: query.status,
            isIncoterm: query.isContern,
        };
    },
    (value) => {
        loading.global = true;
        query.Page = 0;
        fetchStats(query.toQueryStatsString());
    }
);

const stt = {
    DXL: {
        label: "Đang xử lý",
        class: "text-yellow-700 bg-yellow-200",
    },
    DXN: {
        class: "text-blue-700 bg-blue-200",
        label: "Đã xác nhận",
    },
    HUY: {
        class: "text-red-700 bg-red-200",
        label: "Đã hủy",
    },
    DGH: {
        class: "text-orange-700 bg-orange-200",
        label: "Đang giao hàng",
    },
    DHT: {
        class: "text-green-700 bg-green-200",
        label: "Hoàn thành",
    },
};
const getStatusTag = (status: string) => {
    return stt[status as keyof typeof stt] || { label: "unknown", severity: "contrast" };
};

const formatNumber = (input: any): string => {
    return Intl.NumberFormat().format(input ?? 0);
};

function initialComponent() {
    // Fetch data or initialize data here
    loading.global = true;
    fetchStats(query.toQueryStatsString());
}

onMounted(() => {
    initialComponent();
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
            legend: {
                // labels: {
                //     color: textColor,
                // },
                display: false,
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

<style scoped>
.warning {
    color: yellow;
}
</style>
