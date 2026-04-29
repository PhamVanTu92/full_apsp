<script setup>
import { onBeforeMount, ref, inject } from 'vue';
import API from '@/api/api-main';
import format from '@/helpers/format.helper';
import { useRouter, useRoute } from 'vue-router';
import { FilterMatchMode } from 'primevue/api';
import { FilterStore } from '@/Pinia/Filter/FilterStorePurchaseOrderNET';
import { debounce } from 'lodash';
import { useI18n } from 'vue-i18n';
import { getLabels } from '@/components/Status';
import { exportExcelFile } from '@/helpers/exportFile.helper';
const { t } = useI18n();
const filterStore = FilterStore();
const conditionHandler = inject('conditionHandler');
const router = useRouter();
const route = useRoute();
const loading = ref(false);
const itemMenu = ref([
    {
        label: t('body.OrderList.createOrderBasic'),
        icon: 'pi pi-plus',
        command: () => {
            router.push(`/purchase-order-new`);
        }
    },
    {
        label: t('body.OrderList.createOrderAdvanced'),
        icon: 'pi pi-plus',
        command: () => {
            router.push(`/purchase-order-new-plus`);
        }
    }
]);
const orders = ref([]);
const dataTable = ref({
    limit: route.query.page ? route.query.page : 10,
    skip: route.query.size ? route.query.size : 0
});
const statuses = ref([
    { code: 'DXN', name: t('body.status.DXN') },
    { code: 'HUY', name: t('body.status.HUY') },
    { code: 'DGH', name: t('body.status.DGH') },
    { code: 'DHT', name: t('body.status.DHT') },
    { code: 'CTT', name: t('body.status.CTT') },
    { code: 'CXN', name: t('body.status.CXN') },
    { code: 'DTT', name: t('body.status.DTT') },
    { code: 'DONG', name: t('body.status.DONG') },
    { code: 'DGHR', name: t('body.status.DGHR') }
]);
const labels = {
    DONG: {
        label: t('body.status.DONG'),
        class: 'text-white  bg-gray-500 border-1 border-gray-500'
    },
    DXL: {
        label: t('body.OrderList.processing'),
        class: 'text-yellow-700 bg-yellow-200'
    },
    DXN: {
        class: 'text-teal-700 bg-teal-200',
        label: t('body.OrderList.confirmed')
    },
    HUY: {
        class: 'text-red-500 bg-red-100',
        label: t('body.OrderList.cancelled')
    },
    HUY2: {
        class: 'text-red-500 border-red-500 bg-white border-1',
        label: t('body.OrderList.cancelled')
    },
    DGH: {
        class: 'text-blue-700 bg-blue-200',
        label: t('body.OrderList.inDelivery')
    },
    DGHR: {
        class: 'text-blue-700 bg-blue-200 bg-white border-1',
        label: t('body.OrderList.delivered')
    },
    DHT: {
        class: 'text-green-700 bg-green-200',
        label: t('body.OrderList.completed')
    },
    CTT: {
        class: 'text-yellow-50 bg-yellow-700',
        label: t('body.OrderList.waiting_payment')
    },
    TTN: {
        class: 'text-yellow-700 bg-white border-1 border-yellow-700',
        label: t('body.OrderList.waiting_process')
    },
    // UNC: {
    //     class: "text-green-700 bg-white border-1 border-green-700",
    //     label: "Đã thanh toán",
    // },
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: t('body.OrderList.pendingConfirmation')
    },
    DTT: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: t('body.OrderList.complete_payment')
    }
};

const fetchOrderData = async (queryFilter = null) => {
    loading.value = true;
    try {
        const filters = conditionHandler.getQuery(filterStore.filters);
        const queryParams = `?skip=${dataTable.value.skip}&limit=${dataTable.value.limit}${queryFilter ? '&filter=' + queryFilter : filters}`;
        const { data } = await API.get(`PurchaseOrderNet${queryParams}`);
        orders.value = data.items;
        Object.assign(dataTable.value, {
            limit: data.limit,
            skip: data.skip,
            total: data.total
        });
        router.replace(queryParams);
    } catch (error) {
        console.error('Error fetching order data:', error);
    } finally {
        loading.value = false;
    }
};
const getStatusLabel = (str) => {
    return labels[str] || { severity: '', label: str };
};
const onPageChange = (event) => {
    dataTable.value.skip = event.page;
    dataTable.value.limit = event.rows;
    fetchOrderData();
};
const onFilter = () => {
    dataTable.value.skip = 0;
    fetchOrderData();
};
const clearFilter = () => {
    filterStore.resetFilters();
    dataTable.value.skip = 0;
    fetchOrderData();
};

const exportExcel = async () => {
    loading.value = true; 
    try {
        const filters = conditionHandler.getQuery(filterStore.filters);
        const queryParams = `?skip=0&limit=${dataTable.value.total}${filters}`;
        const { data } = await API.get(`PurchaseOrderNet${queryParams}`);
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
            fileName: `Danh-sach-don-hang-net-${format.DateTime(new Date()).dateTimeFile}.xlsx`,
            sheetName: 'Danh sách đơn hàng',
            columns,
            data: rows
        });
    } catch (error) {
        console.error('Error exporting Excel:', error);
    } finally {
        loading.value = false;
    }
};

const debounceF = debounce(onFilter, 1000);
onBeforeMount(() => {
    if (route.query) {
        dataTable.value.skip = route.query.skip || 0;
        dataTable.value.limit = route.query.limit || 10;
        let filter = route.query.filter;
        fetchOrderData(filter);
    } else {
        fetchOrderData();
    }
});
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <h4 class="font-bold m-0">{{ t('PromotionalItems.OrderNET.title') }}</h4>
        <router-link to="/client/hisPurNET/new">
            <Button :label="t('body.report.create_new_button')" icon="pi pi-plus" />
        </router-link>
        <Menu ref="menu" id="overlay_menu" :model="itemMenu" :popup="true" />
    </div>
    <div class="card p-3">
        <DataTable
            stripedRows
            class="table-main"
            showGridlines
            dataKey="id"
            :value="orders"
            header="surface-200"
            paginator
            :first="dataTable.limit * dataTable.skip"
            :rows="dataTable.limit"
            :page="dataTable.skip"
            :totalRecords="dataTable.total"
            @page="onPageChange($event)"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
            lazy
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} VPKM`"
            filterDisplay="menu"
            v-model:filters="filterStore.filters"
            :filterLocale="'vi'"
            @filter="onFilter"
        >
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText :placeholder="t('body.OrderList.searchPlaceholder')" v-model="filterStore.filters['global'].value" @input="debounceF" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <div class="flex gap-2">
                        <Button type="button" icon="pi pi-filter-slash" outlined severity="danger" @click="clearFilter()" v-tooltip.bottom="t('client.clear_filter')" />
                        <Button type="button" icon="pi pi-file-export" severity="secondary" @click="exportExcel()" label="Export" />
                    </div>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-500 font-italic text-center">
                    {{ t('body.OrderList.noData') }}
                </div>
            </template>
            <Column field="invoiceCode" :header="t('body.OrderList.orderCode')">
                <template #body="slotProps">
                    <router-link class="text-primary hover:underline font-semibold" :to="'/client/hisPurNET/detail/' + slotProps.data.id">
                        <span class="">{{ slotProps.data.invoiceCode }}</span>
                    </router-link>
                </template>
            </Column>
            <!-- <Column field="cardName" :header="t('body.OrderList.customer')">
                <template #body="sp">
                    <router-link class="text-primary hover:underline font-semibold cursor-pointer"
                        :to="'/agen-man/agency-category/' + sp.data.cardId">
                        <span>{{ sp.data.cardName }}</span></router-link>
                </template>
            </Column> -->
            <Column :header="t('body.OrderList.total_order_value')">
                <template #body="sp">
                    <span class="text-right block">{{ format.FormatCurrency(sp.data.total, 2) }}</span>
                </template>
            </Column>
            <Column field="status" :header="t('body.OrderList.orderStatus')" :showFilterMatchModes="false">
                <template #body="slotProps">
                    <Tag :class="getStatusLabel(slotProps.data.status)['class']" :value="getStatusLabel(slotProps.data.status)['label']" />
                </template>
                <template #filter="{ filterModel }">
                    <MultiSelect v-model="filterModel.value" :options="statuses" optionLabel="name" optionValue="code" :placeholder="t('body.OrderList.orderStatus')" class="p-column-filter" showClear>
                        <template #option="slotProps">
                            <Tag :value="getStatusLabel(slotProps.option.code)['label']" :class="getStatusLabel(slotProps.option.code)['class']" />
                        </template>
                    </MultiSelect>
                </template>
            </Column>
            <Column
                field="docDate"
                :header="t('body.OrderList.time')"
                filterField="docDate"
                dataType="date"
                class="w-10rem"
                :filterMatchModeOptions="[
                    {
                        label: t('body.report.from_date_label_2'),
                        value: FilterMatchMode.DATE_AFTER
                    },
                    { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
                ]"
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
        </DataTable>
    </div>
    <Loading v-if="loading"></Loading>
</template>
