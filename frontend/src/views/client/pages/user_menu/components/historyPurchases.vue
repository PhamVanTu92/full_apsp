<template>
    <div>
        <h4 class="font-bold mb-3">{{ t('client.orderHistory') }}</h4>
        <DataTable
            class="card p-3"
            showGridlines
            stripedRows
            paginator
            lazy
            @page="onPageChange"
            :page="orders_data.page"
            :first="orders_data.size * orders_data.page"
            :rowsPerPageOptions="[10, 20, 50]"
            :totalRecords="orders_data.total_count"
            :rows="orders_data.size"
            :value="orders_data.orders"
            header="surface-200"
            :rowStyleClass="getRowClass"
            tableStyle="min-width: 50rem"
            :globalFilterFields="['productCode', 'status', 'orderDate', 'total']"
            filterDisplay="menu"
            :filterLocale="'vi'"
            v-model:filters="filterStore.filters"
            @filter="onFilter"
        >
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText :placeholder="t('body.OrderList.searchPlaceholder')" v-model="filterStore.filters['global'].value" @keyup.enter="onFilter()" @input="debouncedFilter" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <div class="flex gap-2">
                        <Button type="button" icon="pi pi-filter-slash" :label="t('body.OrderApproval.clear')" outlined @click="clearFilter()" />
                        <Button type="button" icon="pi pi-file-export" severity="secondary" @click="exportExcel()" label="Export" />
                    </div>
                </div>
            </template>
            <Column field="invoiceCode" :header="t('body.OrderList.orderCode')" style="font-size: 14px">
                <template #body="slotProps">
                    <router-link
                        :to="{
                            name: 'client-order-detail',
                            params: { id: slotProps.data.id }
                        }"
                    >
                        <Button icon="fa-solid fa-arrow-right" text class="p-1 text-primary" :label="slotProps.data.invoiceCode" />
                    </router-link>
                </template>
            </Column>

            <Column field="status" :header="t('body.OrderList.orderStatus')" style="font-size: 14px" :showFilterMatchModes="false">
                <template #body="slotProps">
                    <Tag :class="getStatusLabel(slotProps.data.status)?.class" :value="getStatusLabel(slotProps.data.status)?.label"></Tag>
                </template>
                <template #filter="{ filterModel }">
                    <MultiSelect v-model="filterModel.value" :options="status" optionLabel="name" optionValue="code" :placeholder="t('body.promotion.select_status')" class="p-column-filter" showClear>
                        <template #option="slotProps">
                            <Tag :value="getStatusLabel(slotProps.option.code).label" :class="getStatusLabel(slotProps.option.code).class" />
                        </template>
                    </MultiSelect>
                </template>
            </Column>

            <Column field="total" :header="t('Custom.totalPayment')" style="width: 14rem" filterField="total" dataType="numeric" :showFilterMatchModes="false">
                <template #body="slotProps">
                    <div class="text-right">
                        {{ format.FormatCurrency(slotProps.data.total - slotProps.data.bonusAmount || 0, slotProps.data.currency == 'VND' ? 0 : 2) }}
                    </div>
                </template>
                <template #filter="{ filterModel }">
                    <div class="flex flex-column gap-2">
                        <Dropdown
                            v-model="filterModel.matchMode"
                            :options="[
                                { code: 'gte', name: t('body.OrderList.greater_or_equal') },
                                { code: 'lte', name: t('body.OrderList.less_or_equal') }
                            ]"
                            optionLabel="name"
                            optionValue="code"
                        ></Dropdown>
                        <InputNumber v-model="filterModel.value" />
                    </div>
                </template>
            </Column>
            <Column
                field="docDate"
                :header="t('body.OrderList.time')"
                style="font-size: 14px"
                filterField="docDate"
                dataType="date"
                :filterMatchModeOptions="[
                    { label: 'Từ ngày', value: FilterMatchMode.DATE_AFTER },
                    { label: 'Đến ngày', value: FilterMatchMode.DATE_BEFORE }
                ]"
                class="w-14rem"
            >
                <template #body="slotProps">
                    <span class="border-1 border-300 py-1">
                        <span class="surface-300 px-3 p-1">{{ format.DateTime(slotProps.data.docDate).time }}</span>
                        <span class="px-3 p-1">
                            {{ format.DateTime(slotProps.data.docDate).date }}
                        </span>
                    </span>
                </template>
                <template #filter="{ filterModel }">
                    <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
                </template>
            </Column>
            <template #empty>
                <div class="text-center py-5 my-5">{{ t('body.OrderList.noData') }}</div>
            </template>
        </DataTable>
    </div>
    <loading v-if="isLoading"></loading>
</template>

<script setup>
import { ref, reactive, onBeforeMount } from 'vue';
import { FilterMatchMode } from 'primevue/api';
import API from '@/api/api-main';
import format from '@/helpers/format.helper';
import { exportExcelFile } from '@/helpers/exportFile.helper';
import { FilterStore } from '@/Pinia/Filter/FilterStorePurchaseOrder';
import { useRoute, useRouter } from 'vue-router';
import { inject } from 'vue';
import { useI18n } from 'vue-i18n';
import {  getLabels } from '@/components/Status';
const { t } = useI18n();
const conditionHandler = inject('conditionHandler');
const router = useRouter();
const isLoading = ref(false);
const filterStore = FilterStore();
const route = useRoute();

const orders_data = reactive({
    orders: [],
    size: 10,
    page: route.query.skip || 0,
    total_count: 0
});
const fetchDebtHistory = async () => {
    const filters = conditionHandler.getQuery(filterStore.filters);
    isLoading.value = true;
    try {
        const queryParam = `?skip=${orders_data.page}&limit=${orders_data.size}${filters}`;
        const res = await API.get(`me/PurchaseOrder${queryParam}`);
        orders_data.orders = res.data.items;
        orders_data.total_count = res.data.total;
        router.push(queryParam);
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const onPageChange = (e) => {
    orders_data.page = e.page;
    orders_data.size = e.rows;
    fetchDebtHistory();
};
const labels = {
    DXL: { label: t('body.status.DXL'), class: 'text-yellow-700 bg-yellow-200' },
    DXN: { label: t('body.status.DXN'), class: 'text-teal-700 bg-teal-200' },
    HUY: { label: t('body.status.DH'), class: 'text-red-700 bg-red-200' },
    HUY2: { label: t('body.status.DH'), class: 'text-red-700 bg-red-200' },
    DGH: { label: t('body.status.DGH'), class: 'text-blue-700 bg-blue-200' },
    DHT: { label: t('body.status.DHT'), class: 'text-green-700 bg-green-200' },
    CTT: { label: t('body.status.CTT'), class: 'text-yellow-50 bg-yellow-700' },
    TTN: { label: t('body.status.TTN'), class: 'text-yellow-700 bg-white border-1 border-yellow-700' },
    CXN: { label: t('body.status.CXN'), class: 'text-orange-700 bg-white border-1 border-orange-700' },
    UNC: { label: t('body.status.DTT'), class: 'text-green-700 bg-white border-1 border-green-700' },
    DONG: { label: t('body.status.DONG'), class: 'text-gray-700 bg-gray-200 border-1 border-gray-700' }
};

const getStatusLabel = (str) => {
    return labels[str] || { class: '', label: '' };
};

const getRowClass = (rowData) => {
    if (rowData.status === 'Đang giao hàng') {
        return 'completed-row'; // CSS class name
    } else {
        return '';
    }
};

const status = ref([
    { code: 'DXL', name: t('body.status.DXL') },
    { code: 'TTN', name: t('body.status.TTN') },
    { code: 'CTT', name: t('body.status.CTT') },
    { code: 'CXN', name: t('body.status.CXN') },
    { code: 'DXN', name: t('body.status.DXN') },
    { code: 'DGH', name: t('body.status.DGH') },
    { code: 'DHT', name: t('body.status.DHT') },
    { code: 'HUY', name: t('body.status.DH') }
]);

// Filter

onBeforeMount(() => {
    if (route.query.status && status.value.find((item) => item.code == route.query.status)) {
        filterStore.filters.status.value = route.query.status;
        orders_data.page = 0;
    }
    fetchDebtHistory();
});

const onFilter = () => {
    orders_data.page = 0;
    fetchDebtHistory();
};
let debounceTimeout = null;
const debouncedFilter = () => {
    if (debounceTimeout) {
        clearTimeout(debounceTimeout);
    }
    debounceTimeout = setTimeout(() => {
        onFilter();
    }, 1000);
};
const clearFilter = () => {
    filterStore.resetFilters();
    fetchDebtHistory();
}; 
const exportExcel = async () => {
    isLoading.value = true;

    try {
        const filters = conditionHandler.getQuery(filterStore.filters);
        const queryParams = `?skip=0&limit=${orders_data.total_count}${filters}`;
        const { data } = await API.get(`me/PurchaseOrder${queryParams}`);
        const statusLabelRevert = getLabels(t); 
        
        const rows = data.items.map((order, index) => ({
            stt: index + 1,
            invoiceCode: order.invoiceCode,
            cardName: order.cardName,
            status: statusLabelRevert[order.status]?.label || order.status,
            total: order.currency == 'VND' ? format.FormatCurrency((order.total || 0) - (order.bonusAmount || 0), 0) : format.FormatCurrency((order.total || 0) - (order.bonusAmount || 0), 2),
            docDate: `${format.DateTime(order.docDate).time} ${format.DateTime(order.docDate).date}`
        }));

        const columns = [
            { header: 'STT', key: 'stt', width: 10, style: { alignment: { horizontal: 'center' } } },
            { header: t('body.OrderList.orderCode'), key: 'invoiceCode', width: 20 },
            { header: t('body.OrderList.customer'), key: 'cardName', width: 40 },
            { header: t('body.OrderList.orderStatus'), key: 'status', width: 25 },
            { header: t('Custom.totalPayment'), key: 'total', width: 25, style: { alignment: { horizontal: 'right' } } },
            { header: t('body.OrderList.time'), key: 'docDate', width: 20, style: { alignment: { horizontal: 'center' } } }
        ];

        await exportExcelFile({
            fileName: `Danh-sach-don-hang-${format.DateTime(new Date()).dateTimeFile}.xlsx`,
            sheetName: 'Danh sách đơn hàng',
            columns,
            data: rows
        });
    } catch (error) {
        console.error('Error exporting Excel:', error);
    } finally {
        isLoading.value = false;
    }
};
</script>
