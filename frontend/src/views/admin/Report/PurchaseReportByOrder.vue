<template>
    <div>
        <div class="flex justify-content-between mb-3">
            <h4 class="font-bold m-0">{{ t('body.report.report_title_order') }}</h4>
            <ButtonGoBack />
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-bold">{{ t('body.home.time_label') }}</span>
            <Calendar v-model="query.startDate" class="w-10rem" :placeholder="t('body.report.from_date_placeholder')"
                :maxDate="new Date()" />
            <Calendar v-model="query.endDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')"
                :minDate="query.startDate" :maxDate="new Date()" />
            <div class="flex gap-3 ml-5">
                <div>
                    <RadioButton v-model="query.isContern" class="mr-2" :value="false" inputId="isIncotermFalse">
                    </RadioButton>
                    <label for="isIncotermFalse">{{ t('body.report.domestic_orders_option') }}</label>
                </div>
                <div>
                    <RadioButton v-model="query.isContern" class="mr-2" :value="true" inputId="isIncotermTrue">
                    </RadioButton>
                    <label for="isIncotermTrue">{{ t('body.report.export_orders_option') }}</label>
                </div>
            </div>
        </div>
        <hr />
        <div class="card relative">
            <div class="grid mb-3">
                <div class="col-12 lg:col-6">
                    <DataTable showGridlines :value="orderStatus" resizableColumns columnResizeMode="fit">
                        <Column field="label" :header="t('body.report.table_header_status_1')" class="font-bold">
                            <template #body="{ data }">
                                <span :class="data.class"> {{ data.label }}</span>
                            </template>
                        </Column>
                        <Column field="quantity" class="text-right w-15rem"
                            :header="t('body.report.table_header_order_quantity')">
                            <template #body="{ data }">
                                <span>{{ Intl.NumberFormat().format(data.quantity) }}</span>
                            </template>
                        </Column>
                        <Column field="value" class="text-right w-15rem"
                            :header="t('body.report.table_header_total_value')">
                            <template #body="{ data }">
                                <span>{{ Intl.NumberFormat().format(data.value) }}</span>
                            </template>
                        </Column>
                        <ColumnGroup type="footer">
                            <Row>
                                <Column :footer="t('body.home.total')" class="surface-100"></Column>
                                <Column class="text-right surface-100"
                                    :footer="Intl.NumberFormat().format(totalQuantity)" />
                                <Column class="text-right surface-100"
                                    :footer="Intl.NumberFormat().format(totalValue)" />
                            </Row>
                        </ColumnGroup>
                    </DataTable>
                </div>
                <div class="col-12 lg:col-6">
                    <Chart type="bar" :data="setChartData()" :options="setChartOptions()"
                        class="h-full border-1 p-3 border-200" />
                </div>
            </div>
            <DataTable :value="orderData.items" showGridlines stripedRows paginator lazy
                :first="query.PageSize * query.Page" :rows="query.PageSize" :totalRecords="orderData.total"
                :rowsPerPageOptions="[10, 20, 30, 40, 50]" @page="onChagePage" :loading="loading.order">
                <Column :header="t('body.report.table_header_stt_1')" field="">
                    <template #body="{ index }">
                        {{ query.Page * query.PageSize + index + 1 }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_order_code')" field="invoiceCode">
                    <template #body="{ data }">
                        <router-link :to="`/purchase-order/${data.id}`" class="text-primary font-bold hover:underline">
                            <span>{{ data.invoiceCode }}</span>
                        </router-link>
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_order_status')" field="status">
                    <template #body="{ data }">
                        <Tag :value="getStatusTag(data.status)['label']" :class="getStatusTag(data.status)['class']" />
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_order_date')" field="docDate">
                    <template #body="{ data }">
                        {{ format(data.docDate, 'dd/MM/yyyy') }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_original_amount')" field="paymentInfo.totalBeforeVat"
                    class="text-right">
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.totalBeforeVat) }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_immediate_payment_bonus')" field="paymentInfo.bonusAmount"
                    class="text-right">
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.bonusAmount) }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_volume_bonus')" field="paymentInfo.bonusCommited"
                    class="text-right">
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.bonusCommited) }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_total_payment')" field="paymentInfo.totalAfterVat"
                    class="text-right">
                    <template #body="{ data }">
                        {{ formatNumber(data.paymentInfo?.totalAfterVat) }}
                    </template>
                </Column>
                <template #empty>
                    <div class="py-5 my-5 text-center text-500 font-italic">{{ t('body.promotion.no_data_message') }}
                    </div>
                </template>
                <ColumnGroup type="footer">
                    <Row>
                        <Column :footer="t('body.home.total')" class="surface-100" :colspan="4"></Column>
                        <Column :footer="formatNumber(orderData.footer.totalBeforeVat)" class="text-right surface-100">
                        </Column>
                        <Column :footer="formatNumber(orderData.footer.bonusAmount)" class="text-right surface-100">
                        </Column>
                        <Column :footer="formatNumber(orderData.footer.bonusCommited)" class="text-right surface-100">
                        </Column>
                        <Column :footer="formatNumber(orderData.footer.totalAfterVat)" class="text-right surface-100">
                        </Column>
                    </Row>
                </ColumnGroup>
            </DataTable>
            <div v-if="0" style="background-color: rgba(0, 0, 0, 0.4)"
                class="absolute right-0 left-0 top-0 bottom-0 flex justify-content-center align-items-center">
                <i class="pi pi-spin pi-spinner" style="font-size: 4rem"></i>
            </div>
        </div>
        <Loading v-if="loading.global"></Loading>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive, watch } from 'vue';
import API from '@/api/api-main';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const currentDate = new Date();
const query = reactive({
    startDate: new Date(`${currentDate.getFullYear()}-01-01`),
    endDate: currentDate,
    Page: 0,
    PageSize: 10,
    status: '',
    isContern: false,
    toQueryStatsString: (): string => {
        return `?Page=${query.Page + 1}&PageSize=${query.PageSize}&OrderBy=${'id desc'}&startDate=${format(query.startDate, 'yyyy-MM-dd')}&endDate=${format(query.endDate, 'yyyy-MM-dd')}&isContern=${query.isContern}`;
    }
});

const setChartData = () => {
    return {
        labels: [
            t('body.status.TTN'),
            t('body.status.CTT'),
            t('body.status.DTT'),
            t('body.status.CXN'),
            t('body.status.DXN'),
            t('body.status.DGH'),
            t('body.status.DGHR'),
            t('body.status.DHT'),
            t('body.status.HUY2'),
            t('body.status.DONG'),
        ],
        datasets: [
            {
                label: t('body.report.table_header_order_quantity'),
                backgroundColor: orderStatus.value.map((item) => {
                    switch (item.field) {
                        case 'processing': return 'rgba(251, 191, 36, 0.8)'; // yellow
                        case 'inPaying': return 'rgba(34, 197, 94, 0.8)'; // green
                        case 'paid': return 'rgba(74, 222, 128, 0.8)'; // light green
                        case 'pending': return 'rgba(249, 115, 22, 0.8)'; // orange
                        case 'confirmed': return 'rgba(59, 130, 246, 0.8)'; // blue
                        case 'onDelivery': return 'rgba(251, 146, 60, 0.8)'; // light orange
                        case 'delivied': return 'rgba(168, 85, 247, 0.8)'; // purple
                        case 'complete': return 'rgba(20, 184, 166, 0.8)'; // teal
                        case 'cancelled': return 'rgba(239, 68, 68, 0.8)'; // red
                        case 'closed': return 'rgba(248, 113, 113, 0.8)'; // light red
                        default: return 'rgba(156, 163, 175, 0.8)'; // gray
                    }
                }),
                data: orderStatus.value.map((item) => item.quantity)
            }
        ]
    };
};

const orderStatus = ref([
    {
        label: t('body.status.TTN'),
        quantity: 0,
        value: 0,
        class: 'text-yellow-700',
        field: 'processing'
    },
    {
        label: t('body.status.CTT'),
        quantity: 0,
        value: 0,
        class: 'text-green-700',
        field: 'inPaying'
    },
    {
        label: t('body.status.DTT'),
        quantity: 0,
        value: 0,
        class: 'text-green-500',
        field: 'paid'
    },
    {
        label: t('body.status.CXN'),
        quantity: 0,
        value: 0,
        class: 'text-orange-700',
        field: 'pending'
    },
    {
        label: t('body.status.DXN'),
        quantity: 0,
        value: 0,
        class: 'text-blue-700',
        field: 'confirmed'
    },
    {
        label: t('body.status.DGH'),
        quantity: 0,
        value: 0,
        class: 'text-orange-500',
        field: 'onDelivery'
    },
    {
        label: t('body.status.DGHR'),
        quantity: 0,
        value: 0,
        class: 'text-purple-700',
        field: 'delivied'
    },
    {
        label: t('body.status.DHT'),
        quantity: 0,
        value: 0,
        class: 'text-teal-700',
        field: 'complete'
    },
    {
        label: t('body.status.HUY2'),
        quantity: 0,
        value: 0,
        class: 'text-red-700',
        field: 'cancelled'
    },
    {
        label: t('body.status.DONG'),
        quantity: 0,
        value: 0,
        class: 'text-red-500',
        field: 'closed'
    }
]);

const getSum = (field: string) => {
    return orderStatus.value.reduce((acc, curr: any) => acc + curr[field], 0);
};

const totalQuantity = computed(() => getSum('quantity'));
const totalValue = computed(() => getSum('value'));

const loading = reactive({
    global: false,
    order: false
});

const onChagePage = (event: any) => {
    loading.order = true;
    query.Page = event.page;
    query.PageSize = event.rows;
    fetchStats(query.toQueryStatsString());
};
const fetchStats = (queryStr: string) => {
    API.get('Report/order-state' + queryStr)
        .then((res) => {
            orderStatus.value = orderStatus.value.map((row) => {
                return {
                    ...row,
                    value: res.data?.order['total' + row.field[0].toUpperCase() + row.field.slice(1)],
                    quantity: res.data?.order[row.field]
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
        totalAfterVat: 0
    }
});

watch(
    () => {
        return {
            startDate: query.startDate,
            endDate: query.endDate,
            status: query.status,
            isIncoterm: query.isContern
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
        label: t('body.status.DXL'),
        class: 'text-yellow-700 bg-yellow-200'
    },
    DXN: {
        class: 'text-blue-700 bg-blue-200',
        label: t('body.status.DXN')
    },
    HUY: {
        class: 'text-red-700 bg-red-200',
        label: t('body.status.HUY')
    },
    HUY2: {
        class: 'text-red-700 bg-red-200',
        label: t('body.status.HUY2')
    },
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: t('body.status.CXN')
    },
    TTN: {
        class: 'text-yellow-700 bg-white border-1 border-yellow-700',
        label: t('body.status.TTN')
    },
    DGH: {
        class: 'text-orange-700 bg-orange-200',
        label: t('body.status.DGH')
    },
    DHT: {
        class: 'text-green-700 bg-green-200',
        label: t('body.status.DHT')
    },
    CTT: {
        class: 'text-green-700 bg-green-200',
        label: t('body.status.CTT')
    },
    DONG: {
        class: 'text-green-700 bg-green-200',
        label: t('body.status.DONG')
    },
    DGHR: {
        class: 'text-purple-700 bg-purple-200',
        label: t('body.status.DGHR')
    }

};
const getStatusTag = (status: string) => {
    return stt[status as keyof typeof stt] || { label: 'unknown', severity: 'contrast' };
};

const formatNumber = (input: any): string => {
    return Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(input ?? 0);
};

function initialComponent() {
    loading.global = true;
    fetchStats(query.toQueryStatsString());
}

onMounted(() => {
    initialComponent();
});
const setChartOptions = () => {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');
    return {
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            legend: {
                display: false
            }
        },
        scales: {
            x: {
                ticks: {
                    color: textColorSecondary,
                    font: {
                        weight: 500
                    }
                },
                grid: {
                    display: false,
                    drawBorder: false
                }
            },
            y: {
                ticks: {
                    color: textColorSecondary
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false
                }
            }
        }
    };
};
</script>