<script setup>
import { ref, onMounted, reactive } from "vue";
import { useRouter } from "vue-router";
const router = useRouter();
import API from "@/api/api-main";
import { format } from "date-fns";
import { uniq } from "lodash";

const visibleFilter = ref(false);
const visibleDetail = ref(false);
const loading = ref(false);
const currentDate = new Date();
const dateFilter = reactive({
    startDate: new Date(`${currentDate.getFullYear()}-01-01`),
    endDate: currentDate,
    getQuery: () => {
        const startDateStr = format(dateFilter.startDate, "yyyy-MM-dd");
        const endDateStr = format(dateFilter.endDate, "yyyy-MM-dd");
        return `?startDate=${startDateStr}&endDate=${endDateStr}`;
    },
});

const onDateSelected = () => {
    if (dateFilter.startDate && dateFilter.endDate) {
        onClickGetData();
    }
};

const fieldFilters = ["brand", "industry", "itemType", "packaging"];

var taskDoneCount = ref(0);
const fetchReportData = (dateFilter = null) => {
    taskDoneCount.value = 0;
    let url = `report/purchases/top/5/${dateFilter ? dateFilter.getQuery() : ""}`;
    API.get(url)
        .then((res) => {
            const labels = res.data.map((el) => el.itemName);
            const datasets = res.data.map((el) => el.quantity);
            chartData.value = setChartData(labels, datasets);
            taskDoneCount.value++;
        })
        .catch((error) => {});
    API.get(`report/purchases${dateFilter ? dateFilter.getQuery() : ""}`)
        .then((res) => {
            productData.value = sortArray(res.data.lines, "purchasedQuantity", true);
            taskDoneCount.value++;
            filterOptions;
            fieldFilters.forEach((field) => {
                filterOptions[field] = uniq(
                    sortArray(res.data.lines, "purchasedQuantity", true).map(
                        (el) => el[field]
                    )
                );
            });
        })
        .catch((error) => {});
};

const loadingDetail = ref(false);
const product = ref({});
const fetchDetailProduct = (itemCode) => {
    loadingDetail.value = true;
    API.get(`report/purchases/${itemCode}/${dateFilter ? dateFilter.getQuery() : ""}`)
        .then((res) => {
            product.value.invoices = res.data;
            loadingDetail.value = false;
        })
        .catch((error) => {
            loadingDetail.value = false;
        });
};

const onHideDetailDlg = () => {
    product.value = {};
};

const onClickGetData = () => {
    fetchReportData(dateFilter);
};

const openDetail = (data) => {
    visibleDetail.value = true;
    product.value.product = data;
    fetchDetailProduct(data.itemCode);
};

const chartData = ref({});

const productData = ref([]);
const setChartData = (labels, datasets) => {
    const documentStyle = getComputedStyle(document.documentElement);
    return {
        labels: labels || [],
        datasets: [
            {
                label: `Sản phẩm`,
                backgroundColor: documentStyle.getPropertyValue("--cyan-500"),
                borderColor: documentStyle.getPropertyValue("--cyan-500"),
                data: datasets,
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
        indexAxis: "y",
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            title: {
                display: true,
                text: "Sản phẩm mua nhiều nhất",
                fullSize: true,
                font: {
                    size: 24,
                },
            },
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
                    stepSize: 1,
                    beginAtZero: true,
                },
                grid: {
                    display: true,
                    drawBorder: true,
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

onMounted(() => {
    onClickGetData();
});

const formatCurency = (num) => {
    if (num != null || num != undefined)
        return Intl.NumberFormat("vi-VI", {
            style: "decimal",
        }).format(num);
    else return "NaN";
};

const sortArray = (target, field, reverse = false) => {
    if (!target?.length) return [];
    function compareFn(a, b) {
        if (reverse) {
            if (a[field] < b[field]) return 1;
            if (a[field] > b[field]) return -1;
            return 0;
        } else {
            if (a[field] > b[field]) return 1;
            if (a[field] < b[field]) return -1;
            return 0;
        }
    }
    return target.sort(compareFn);
};

const productDataTable = ref();

const exportToCSV = () => {
    productDataTable.value.exportCSV();
};

const onRowClick = ({ data }) => {
    openDetail(data);
};

import { FilterMatchMode, FilterOperator } from "primevue/api";
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    brand: {
        value: null,
        matchMode: FilterMatchMode.IN,
    },
    industry: {
        value: null,
        matchMode: FilterMatchMode.IN,
    },
    itemType: {
        value: null,
        matchMode: FilterMatchMode.IN,
    },
    packaging: {
        value: null,
        matchMode: FilterMatchMode.IN,
    },
});
const filterOptions = reactive({
    brand: [],
    industry: [],
    itemType: [],
    packaging: [],
});
</script>
<template>
    <div class="flex justify-content-center align-items-center mb-3">
        <h4 class="font-bold m-0">Báo cáo mua hàng theo sản phẩm</h4>
    </div>
    <div class="flex align-items-center gap-3">
        <span class="font-bold">Thời gian:</span>
        <span class="flex gap-3">
            <Calendar
                v-model="dateFilter.startDate"
                class="w-3"
                placeholder="Từ ngày"
                formatDate="dd/mm/yyyy"
                :maxDate="dateFilter.endDate"
                @date-select="onDateSelected"
            ></Calendar>
            <Calendar
                v-model="dateFilter.endDate"
                class="w-3"
                placeholder="Đến ngày"
                formatDate="dd/mm/yyyy"
                :minDate="dateFilter.startDate"
                @date-select="onDateSelected"
            ></Calendar>
            <Button v-if="0" label="Áp dụng" @click="onClickGetData"/>
        </span>
    </div>
    <hr />
    <div class="card mb-3">
        <Chart
            type="bar"
            :data="chartData"
            :options="setChartOptions()"
            class="h-30rem card border-noround pt-2"
        />
        <DataTable
            v-model:filters="filters"
            resizableColumns
            columnResizeMode="expand"
            ref="productDataTable"
            :value="productData"
            showGridlines
            scrollable
            filterDisplay="menu"
            scrollHeight="30rem"
            :loading="taskDoneCount == 0"
            selectionMode="single"
            @row-click="onRowClick"
            :globalFilterFields="['itemCode', 'itemName']"
        >
            <template #header>
                <div class="flex justify-content-between align-items-center">
                    <span class="text-xl">Danh sách sản phẩm mua</span>
                    <div class="flex gap-3">
                        <!-- <InputGroup>
                                            <InputGroupAddon> ds </InputGroupAddon>
                                        </InputGroup> -->
                        <InputText
                            v-model="filters['global'].value"
                            placeholder="Tìm kiếm..."
                        ></InputText>
                        <Button
                            icon="pi pi-file-export"
                            outlined
                            severity="info"
                            label="Xuất excel"
                            @click="exportToCSV"
                        />
                    </div>
                </div>
            </template>
            <template #empty>
                <div class="my-5 py-5 text-center">Không có dữ liệu để hiển thị</div>
            </template>
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="itemCode" header="Mã hàng hóa"></Column>
            <Column field="itemName" header="Tên hàng hóa"> </Column>
            <Column
                filterField="brand"
                header="Thương hiệu"
                field="brand"
                :showFilterMatchModes="false"
            >
                <template #filter="{ filterModel }">
                    <MultiSelect
                        placeholder="Thương hiệu"
                        v-model="filterModel.value"
                        :options="filterOptions.brand"
                        class="p-column-filter"
                        style="min-width: 12rem"
                    >
                    </MultiSelect>
                </template>
            </Column>
            <Column
                header="Ngành hàng"
                field="industry"
                filterField="industry"
                :showFilterMatchModes="false"
            >
                <template #filter="{ filterModel }">
                    <MultiSelect
                        placeholder="Ngành hàng"
                        v-model="filterModel.value"
                        :options="filterOptions.industry"
                        class="p-column-filter"
                        style="min-width: 12rem"
                    >
                    </MultiSelect>
                </template>
            </Column>
            <Column
                header="Loại hàng hóa"
                field="itemType"
                filterField="itemType"
                :showFilterMatchModes="false"
            >
                <template #filter="{ filterModel }">
                    <MultiSelect
                        placeholder="Loại hàng hóa"
                        v-model="filterModel.value"
                        :options="filterOptions.itemType"
                        class="p-column-filter"
                        style="min-width: 12rem"
                    >
                    </MultiSelect>
                </template>
            </Column>
            <Column
                header="Quy cách bao bì"
                field="packaging"
                filterField="packaging"
                :showFilterMatchModes="false"
            >
                <template #filter="{ filterModel }">
                    <MultiSelect
                        placeholder="Quy cách bao bì"
                        v-model="filterModel.value"
                        :options="filterOptions.packaging"
                        class="p-column-filter"
                        style="min-width: 12rem"
                    >
                    </MultiSelect>
                </template>
            </Column>
            <Column header="Số lượng mua" field="purchasedQuantity"></Column>
            <Column
                field="promotionalQuantity"
                header="Số lượng được khuyến mại"
            ></Column>
            <Column
                header="Tổng sản lượng được tích lũy (Lít)"
                field="totalAccumulatedVolume"
            ></Column>
            <Column header="Tổng số đơn hàng" field="totalOrders"></Column>
            <Column v-if="0">
                <template #body="{ data }">
                    <span
                        class="pi pi-eye cursor-pointer hover:text-green-700"
                        @click="openDetail(data)"
                    ></span>
                </template>
            </Column>
        </DataTable>
    </div>
    <Dialog
        v-model:visible="visibleFilter"
        modal
        header="Bộ lọc"
        :style="{ width: '45rem' }"
    >
        <div class="card">
            <div class="flex justify-content-between gap-5">
                <div class="w-6">
                    <div class="flex flex-column gap-2">
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">Thời gian</label>
                            <Calendar
                                v-model="dates"
                                selectionMode="range"
                                :manualInput="false"
                                placeholder="Từ ngày - đến ngày"
                            />
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">Thương hiệu</label>
                            <MultiSelect placeholder="Thương hiệu"></MultiSelect>
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">Ngành hàng</label>
                            <MultiSelect placeholder="Ngành hàng"></MultiSelect>
                        </div>
                    </div>
                </div>
                <div class="w-6">
                    <div class="flex flex-column gap-2">
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">Loại hàng hóa</label>
                            <MultiSelect placeholder="Loại hàng hóa"></MultiSelect>
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">Quy cách bao bì</label>
                            <MultiSelect placeholder="Quy cách bao bì"></MultiSelect>
                        </div>
                    </div>
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
    <Dialog
        v-model:visible="visibleDetail"
        modal
        header="Báo cáo chi tiết mua hàng theo sản phẩm"
        :style="{ width: '85rem' }"
        @hide="onHideDetailDlg"
    >
        <DataTable
            :value="product.invoices?.lines"
            showGridlines
            tableStyle="min-width: 50rem"
            :loading="loadingDetail"
        >
            <template #header>
                <div class="flex gap-3 align-items-center">
                    <div>{{ product.product.itemCode }}</div>
                    <div class="h-2rem border-right-1 border-400"></div>
                    <div>{{ product.product.itemName }}</div>
                    <!-- <Button label="Xuất excel" outlined size="small"/> -->
                </div>
            </template>
            <Column header="#">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="orderCode" header="Mã đơn hàng">
                <template #body="{ data }">
                    <router-link
                        target="_blank"
                        :to="{
                            name: 'client-order-detail',
                            params: { id: data.orderId },
                        }"
                        class="text-primary font-semibold hover:underline"
                        >{{ data.orderCode }}</router-link
                    >
                </template>
            </Column>
            <Column field="orderDate" header="Ngày đặt hàng">
                <template #body="{ data }">
                    {{ format(data.orderDate, "dd/MM/yyyy") }}
                </template>
            </Column>
            <Column field="quantityPurchased" header="Số lượng mua">
                <template #body="{ data }">
                    <div class="text-right">
                        {{ data.quantityPurchased }}
                    </div>
                </template>
            </Column>
            <Column field="promotionalQuantity" header="Số lượng KM">
                <template #body="{ data }">
                    <div class="text-right">
                        {{ data.promotionalQuantity }}
                    </div>
                </template>
            </Column>
            <Column field="unitPrice" header="Đơn giá (VNĐ)">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.unitPrice) }}
                    </div>
                </template>
            </Column>
            <Column field="discount" header="Giảm giá (VNĐ)">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.discount) }}
                    </div>
                </template>
            </Column>
            <Column field="taxTotal" header="Thuế suất (VNĐ)">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.taxTotal) }}
                    </div>
                </template>
            </Column>
            <Column field="totalAmount" header="Tổng tiền (VNĐ)">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.totalAmount) }}
                    </div>
                </template>
            </Column>
            <Column field="isIncludedInProduction" header="Được tính sản lượng">
                <template #body="{ data }">
                    {{ data.isIncludedInProduction ? "Có" : "Không" }}
                </template>
            </Column>

            <ColumnGroup type="footer">
                <Row>
                    <Column footer="Tổng:" :colspan="3" footerStyle="text-align:right" />
                    <Column>
                        <template #footer>
                            <div class="text-right">
                                {{ product.invoices?.quantityPurchased }}
                            </div>
                        </template>
                    </Column>
                    <Column>
                        <template #footer>
                            <div class="text-right">
                                {{ product.invoices?.promotionalQuantity }}
                            </div>
                        </template>
                    </Column>
                    <Column>
                        <template #footer>
                            <div class="text-primary text-right"></div>
                        </template>
                    </Column>
                    <Column>
                        <template #footer>
                            <div class="text-primary text-right">
                                {{ formatCurency(product.invoices?.discount) }}
                            </div>
                        </template>
                    </Column>
                    <Column>
                        <template #footer>
                            <div class="text-primary text-right">
                                {{ formatCurency(product.invoices?.taxTotal) }}
                            </div>
                        </template>
                    </Column>
                    <Column>
                        <template #footer>
                            <div class="text-primary text-right">
                                {{ formatCurency(product.invoices?.totalAmount) }}
                            </div>
                        </template>
                    </Column>
                    <Column />
                </Row>
            </ColumnGroup>
        </DataTable>

        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button
                    icon="pi pi-times"
                    type="button"
                    label="Đóng"
                    severity="secondary"
                    @click="visibleDetail = false"
                />
            </div>
        </template>
    </Dialog>
</template>
