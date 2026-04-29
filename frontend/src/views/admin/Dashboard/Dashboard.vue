<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import api from '@/api/api-main';
import { format, startOfMonth, endOfMonth, subMonths, startOfYear, endOfYear } from 'date-fns';
import OrderStatsChart from './components/OrderStatsChart.vue';
import { useI18n } from 'vue-i18n';
import StatsLine from './components/StatsLine.vue';
import Chart from 'primevue/chart';
import { useOrderStatusLabels } from '@/helpers/orderStatus.helper';
import { CHART_COLORS, REGION_CHART_OPTIONS, DASHBOARD_CONFIG } from '@/helpers/dashboardConfig.helper';

// ==================== Refs ====================
const products = ref([]);
const recentOrder = ref([]);
const dataRes = ref();
const dateRangeType = ref('today');
const fromDate = ref(startOfMonth(new Date()));
const toDate = ref(endOfMonth(new Date()));
const isInterCom = ref('domestic');
const regionChartData = ref({
    labels: [],
    datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }]
});
const chartReport = ref(null);

let refreshIntervalId = null;

// ==================== Composables ====================
const { t } = useI18n();
const { getOrderStatus } = useOrderStatusLabels();

// ==================== Constants ====================
const regionChartOptions = ref(REGION_CHART_OPTIONS);

const dateRangeOptions = ref([
    { label: t('body.home.custom'), value: 'custom' },
    { label: t('body.home.today'), value: 'today' },
    { label: t('body.home.lastMonth'), value: 'lastMonth' },
    { label: t('body.home.quarter1'), value: 'q1' },
    { label: t('body.home.quarter2'), value: 'q2' },
    { label: t('body.home.quarter3'), value: 'q3' },
    { label: t('body.home.quarter4'), value: 'q4' },
    { label: t('body.home.firstHalfYear'), value: 'firstHalf' },
    { label: t('body.home.secondHalfYear'), value: 'secondHalf' }
]);

// ==================== Helper Functions ====================

/**
 * Định dạng tiền tệ
 */
const formatCurrency = (value, currency = 'VND') => {
    return Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency,
        currencyDisplay: 'code'
    }).format(value || 0);
};

/**
 * Cập nhật range ngày dựa trên loại khoảng thời gian
 */
const updateDateRange = (rangeType) => {
    const currentYear = new Date().getFullYear();
    const now = new Date();
    const lastMonth = subMonths(now, 1);

    const ranges = {
        today: { from: new Date(), to: new Date() },
        lastMonth: { from: startOfMonth(lastMonth), to: endOfMonth(lastMonth) },
        q1: { from: new Date(currentYear, 0, 1), to: new Date(currentYear, 2, 31) },
        q2: { from: new Date(currentYear, 3, 1), to: new Date(currentYear, 5, 30) },
        q3: { from: new Date(currentYear, 6, 1), to: new Date(currentYear, 8, 30) },
        q4: { from: new Date(currentYear, 9, 1), to: new Date(currentYear, 11, 31) },
        firstHalf: { from: startOfYear(now), to: new Date(currentYear, 5, 30) },
        secondHalf: { from: new Date(currentYear, 6, 1), to: endOfYear(now) }
    };

    const range = ranges[rangeType];
    if (range) {
        fromDate.value = range.from;
        toDate.value = range.to;
    }
};

/**
 * Xử lý dữ liệu region chart
 */
const processRegionChartData = (orderChartData) => {
    if (orderChartData?.regions && orderChartData.regions.length > 0) {
        const colors = CHART_COLORS.pie.slice(0, orderChartData.regions.length);
        regionChartData.value = {
            labels: orderChartData.regions.map((item) => item.regionName),
            datasets: [
                {
                    data: orderChartData.regions.map((item) => item.orderCount),
                    backgroundColor: colors,
                    hoverBackgroundColor: colors
                }
            ]
        };
    } else {
        regionChartData.value = DASHBOARD_CONFIG.DEFAULT_REGION_CHART;
    }
};

/**
 * Fetch dữ liệu từ API
 */
const fetchData = () => {
    const params = new URLSearchParams({
        fromDate: format(fromDate.value, 'yyyy-MM-dd'),
        toDate: format(toDate.value, 'yyyy-MM-dd'),
        isInterCom: isInterCom.value === 'international' ? 'true' : 'false'
    });

    api.get(`dashboard?${params.toString()}`)
        .then((res) => {
            if (res.data) {
                recentOrder.value = res.data.latestOrder || [];
                products.value = res.data.topSaleItem || [];
                dataRes.value = res.data;
                chartReport.value = res.data.chartReport;
                processRegionChartData(res.data.orderChart);
            }
        })
        .catch((error) => {
            console.error('Fetch dashboard error:', error);
        });
};

// ==================== Computed Properties ====================

const totalRegion = computed(() => {
    return regionChartData.value.datasets[0].data.reduce((a, b) => a + b, 0);
});

const formattedDateRange = computed(() => ({
    from: format(fromDate.value, 'dd/MM/yyyy'),
    to: format(toDate.value, 'dd/MM/yyyy')
}));

const statsData = computed(() => {
    const currency = isInterCom.value === 'international' ? 'USD' : 'VND';
    return [
        {
            icon: 'pi pi-money-bill',
            value: formatCurrency(dataRes.value?.currentMonthlyRevenue || 0, currency),
            label: t('body.home.revenue'),
            color: 'purple'
        },
        {
            icon: 'pi pi-shopping-cart',
            value: dataRes.value?.totalOrderInTheMonth || 0,
            label: t('body.home.orders'),
            color: 'green'
        },
        {
            icon: 'pi pi-users',
            value: dataRes.value?.totalCustomers || 0,
            label: t('body.home.distributors'),
            color: 'yellow'
        },
        {
            icon: 'pi pi-user-plus',
            value: dataRes.value?.totalNewCustomerInTheMonth || 0,
            label: t('body.home.newRegistrations'),
            color: 'blue'
        }
    ];
});

// ==================== Watchers ====================

watch(dateRangeType, (newValue) => {
    updateDateRange(newValue);
    if (newValue !== 'custom') {
        fetchData();
    }
});

watch([fromDate, toDate], () => {
    if (dateRangeType.value === 'custom') {
        fetchData();
    }
});

watch(isInterCom, () => {
    fetchData();
});

// ==================== Lifecycle ====================

onMounted(() => {
    fetchData();
    // Refresh data mỗi 60 giây
    refreshIntervalId = setInterval(fetchData, DASHBOARD_CONFIG.REFRESH_INTERVAL);
});

onUnmounted(() => {
    if (refreshIntervalId) {
        clearInterval(refreshIntervalId);
    }
});
</script>

<template>
    <div class="flex justify-content-between align-items-center grid lg:mb-0 mb-3 lg:pl-0 pl-3">
        <h4 class="font-bold m-0 lg:col-3">{{ t('body.home.overview') }}</h4>
        <div class="lg:col-9">
            <div class="flex gap-3 align-items-center lg:justify-content-end flex-wrap w-full">
                <!-- Domestic/International Toggle -->
                <div class="flex align-items-center gap-2 border-1 border-200 border-round p-2">
                    <i :class="isInterCom === 'domestic' ? 'pi pi-home text-primary' : 'pi pi-home text-400'" @click="isInterCom = 'domestic'" class="cursor-pointer"></i>
                    <span :class="isInterCom === 'domestic' ? 'font-semibold text-primary' : 'text-600'" @click="isInterCom = 'domestic'" class="cursor-pointer">
                        {{ t('body.home.domestic') }}
                    </span>
                    <InputSwitch v-model="isInterCom" trueValue="international" falseValue="domestic" />
                    <i :class="isInterCom === 'international' ? 'pi pi-globe text-primary' : 'pi pi-globe text-400'" @click="isInterCom = 'international'" class="cursor-pointer"></i>
                    <span :class="isInterCom === 'international' ? 'font-semibold text-primary' : 'text-600'" @click="isInterCom = 'international'" class="cursor-pointer">
                        {{ t('body.home.international') }}
                    </span>
                </div>

                <!-- Date Range Selector -->
                <div class="flex align-items-center gap-2">
                    <Dropdown v-model="dateRangeType" :options="dateRangeOptions" optionLabel="label" optionValue="value" :placeholder="t('body.home.selectPeriod')" class="w-12rem" />

                    <!-- Custom Date Range -->
                    <div v-if="dateRangeType === 'custom'" class="flex gap-2">
                        <div class="flex align-items-center gap-2">
                            <label>{{ t('body.home.from') }}:</label>
                            <Calendar v-model="fromDate" dateFormat="dd/mm/yy" :showIcon="true" :placeholder="t('body.home.selectDate')" />
                        </div>
                        <div class="flex align-items-center gap-2">
                            <label>{{ t('body.home.to') }}:</label>
                            <Calendar v-model="toDate" dateFormat="dd/mm/yy" :showIcon="true" :placeholder="t('body.home.selectDate')" />
                        </div>
                    </div>

                    <!-- Display Date Range -->
                    <div v-else class="text-sm text-600">
                        <i class="pi pi-calendar mr-2"></i>
                        {{ formattedDateRange.from }} - {{ formattedDateRange.to }}
                    </div>
                </div>
            </div>
        </div>
    </div>
    <StatsLine class="mb-2" :data="statsData" />
    <div class="grid">
        <div class="col-12 lg:col-8">
            <div class="border-1 border-200 bg-white border-round-md h-full flex flex-column">
                <div class="text-xl font-bold p-3">{{ t('body.home.purchaseReport') }}</div>
                <hr class="m-0" />
                <div class="p-3 flex-grow-1">
                    <OrderStatsChart :data="chartReport" />
                </div>
            </div>
        </div>
        <div class="col-12 lg:col-4">
            <div class="border-1 border-200 bg-white border-round-md h-full flex flex-column">
                <div class="text-xl font-bold p-3">{{ t('body.home.regionDistribution') }}</div>
                <hr class="m-0" />
                <div class="p-3 flex-grow-1 relative" style="min-height: 400px">
                    <Chart type="doughnut" :data="regionChartData" :options="regionChartOptions" style="height: 400px" />
                    <div class="chart-center-text pl-2 pt-5">
                        <div class="text-primary font-bold text-4xl pl-1">{{ totalRegion.toLocaleString() }}</div>
                        <span>{{ t('body.home.order') }}</span>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="grid mt-3">
        <div class="col-12 lg:col-8">
            <div class="bg-white border-1 border-200 border-round-md">
                <div class="flex align-items-center px-3 py-2 justify-content-between">
                    <span class="font-bold text-xl mr-2">{{ t('body.home.recentOrders') }}</span>
                    <router-link :to="{ name: 'purchaseList' }">
                        <Button :label="t('body.home.viewAll') " text />
                    </router-link>
                </div>
                <hr class="m-0" />
                <DataTable :value="recentOrder" showGridlines class="p-3">
                    <Column field="invoiceCode" :header="t('body.OrderList.orderCode')">
                        <template #body="slotProps">
                            <router-link class="text-primary hover:underline font-semibold" :to="'/purchase-order/' + slotProps.data.id">
                                <span>{{ slotProps.data.invoiceCode }}</span>
                            </router-link>
                        </template>
                    </Column>
                    <Column field="cardName" :header="t('body.OrderList.customer')"></Column>
                    <Column field="total" :header="t('body.OrderList.orderValue')" class="text-right">
                        <template #body="{ data }">
                            <div class="text-primary">
                                {{ formatCurrency(data?.total || 0, 'VND') }}
                            </div>
                        </template>
                    </Column>
                    <Column field="docDate" :header="t('body.home.order_date_column') " class="">
                        <template #body="{ data }">
                            {{ format(data.docDate, 'dd/MM/yyyy') }}
                        </template>
                    </Column>
                    <Column field="status" :header="t('client.status_header') " class="">
                        <template #body="slotProps">
                            <Tag :class="getOrderStatus(slotProps.data.status)['class']" :value="getOrderStatus(slotProps.data.status)['label']" />
                        </template>
                    </Column>
                </DataTable>
            </div>
        </div>
        <div class="col-12 lg:col-4">
            <div class="bg-white border-1 border-200 align-items-center pb-1 border-round-md">
                <div class="flex justify-content-between align-items-center border-bottom-1 border-200 py-2 pl-3 cus-shadow">
                    <div class="font-bold text-xl">{{ t('body.home.bestSellingProducts') }}</div>
                    <router-link :to="{ name: 'buyByProduct' }">
                        <Button :label="t('body.home.details')" text />
                    </router-link>
                </div>
                <div class="overflow-y-auto" style="max-height: 300px">
                    <div class="flex border-bottom-1 border-200 p-3" v-for="(item, index) in products" :key="index">
                        <div>
                            <img :src="item.item?.itM1?.[0]?.filePath || 'https://placehold.co/60x60'" :alt="item.item.itemName" style="width: 70px; height: 70px" />
                        </div>
                        <div class="flex-grow-1 ml-3 h-full">
                            <div class="flex flex-column gap-2">
                                <div class="font-bold text-md title-prd">
                                    {{ item.item.itemName }}
                                </div>
                                <div>{{ t('body.home.sold') }}: {{ item.quantity }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
@import './components/style.css';
</style>
