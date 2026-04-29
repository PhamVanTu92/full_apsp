<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <h4 class="m-0 font-bold">{{ t('ChangePoint.title') }}</h4>
        <router-link to="/promotional-request/add">
            <Button :label="t('ChangePoint.buttonChangePoint')" icon="pi pi-plus" />
        </router-link>
    </div>
    <div class="card p-3">
        <DataTable
            stripedRows
            showGridlines
            dataKey="id"
            :value="PurchaseRequest"
            tableStyle="min-width: 50rem;"
            header="surface-200"
            paginator
            :rows="dataTable.size"
            :page="dataTable.page"
            :first="dataTable.page * dataTable.size"
            :totalRecords="dataTable.total_size"
            @page="onPageChange($event)"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
            lazy
            filterDisplay="menu"
            :filterLocale="'vi'"
            v-model:filters="filterStore.filters"
            :globalFilterFields="['customer', 'status']"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            @filter="onFilter"
        >
            <template #empty>
                <div class="p-2 text-center">{{ t('validation.no_matching_product_message') }}</div>
            </template>
            <template #header>
                <div class="flex justify-content-between">
                    <IconField class="items-center" iconPosition="left">
                        <InputText :placeholder="t('body.home.search_placeholder')" v-model="filterStore.filters['global'].value" @input="debounceF" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>

                    <Button type="button" icon="pi pi-filter-slash" outlined severity="danger" @click="clearFilter()" v-tooltip.bottom="t('client.clear_filter')" />
                </div>
            </template>
            <Column field="invoiceCode" :header="t('ChangePoint.orderCode')">
                <template #body="slotProps">
                    <router-link class="text-primary hover:underline font-semibold" :to="'/promotional-request/' + slotProps.data.id">
                        <span class="">{{ slotProps.data.invoiceCode }}</span>
                    </router-link>
                </template>
            </Column>
            <Column field="cardName" :header="t('client.customer_name')" />
            <Column :header="t('ChangePoint.usingPoints')">
                <template #body="slotProps">
                    <span>{{ slotProps.data.itemDetail?.reduce((total, line) => total + line.price * line.quantity, 0) }}</span>
                </template>
            </Column>
            <Column field="status" :header="t('body.PurchaseRequestList.status')" :showFilterMatchModes="false">
                <template #body="slotProps">
                    <Tag :class="getStatusLabel(slotProps.data.status)['class']" :value="getStatusLabel(slotProps.data.status)['label']" />
                </template>
                <template #filter="{ filterModel }">
                    <MultiSelect v-model="filterModel.value" :options="statuses" optionLabel="name" optionValue="code" :placeholder="t('body.OrderList.orderStatus')" showClear>
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
                    { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
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

    <loading v-if="isLoading"></loading>
</template>
<script setup>
import { onBeforeMount, ref, inject } from 'vue';
import API from '@/api/api-main';
import { useRouter, useRoute } from 'vue-router';
import { FilterStore } from '@/Pinia/Filter/FilterStorePurchaseRequest';
import format from '@/helpers/format.helper';
import { FilterMatchMode } from 'primevue/api';
import { debounce } from 'lodash';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const conditionHandler = inject('conditionHandler');
const router = useRouter();
const route = useRoute();
const filterStore = FilterStore();
const dataTable = ref({
    page: 0,
    size: 10,
    total_size: 0
});
const isLoading = ref(false);
const PurchaseRequest = ref([]);
const statuses = ref([
    { code: 'TTN', name: t('body.status.TTN') },
    { code: 'CTT', name: t('body.status.CTT') },
    { code: 'CXN', name: t('body.status.CXN') },
    { code: 'DXL', name: t('body.status.DXL') },
    { code: 'DXN', name: t('body.status.DXN') },
    { code: 'DGH', name: t('body.status.DGH') },
    { code: 'DGHR', name: t('body.status.DGHR') },
    { code: 'DHT', name: t('body.status.DHT') },
    { code: 'HUY2', name: t('body.status.DH') },
    { code: 'DONG', name: t('body.status.DONG') },
    { code: 'DTT', name: t('body.status.DTT') }
]);
const labels = {
    DONG: {
        label: t('body.status.DONG'),
        class: 'text-white  bg-gray-500 border-1 border-gray-500'
    },
    DXL: {
        label: t('body.status.Processing'),
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
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: t('body.OrderList.pendingConfirmation')
    },
    DTT: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: t('body.OrderList.complete_payment')
    }
};

const getStatusLabel = (str) => {
    return labels[str] || { severity: '', label: str };
};

const onFilter = () => {
    dataTable.value.page = 0;
    fetchPurchaseReq();
};
const onPageChange = (event) => {
    dataTable.value.page = event.page;
    dataTable.value.size = event.rows;
    fetchPurchaseReq();
};
const clearFilter = () => {
    filterStore.resetFilters();
    dataTable.value.page = 0;
    fetchPurchaseReq();
};

const hasListQuery = () => {
    return route.query.limit != undefined || route.query.skip != undefined || route.query.filter != undefined;
};

const fetchPurchaseReq = async () => {
    isLoading.value = true;
    const filters = conditionHandler.getQuery(filterStore.filters);
    const query = `?limit=${dataTable.value.size}&skip=${dataTable.value.page}${filters}&orderBy=id desc`;
    try {
        const res = await API.get(`Redeem${query}`);
        PurchaseRequest.value = res.data.items;
        dataTable.value.total_size = res.data.total;
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
        router.replace(`${query}`);
    }
};

onBeforeMount(async () => {
    if (hasListQuery()) {
        dataTable.value.size = parseInt(route.query.limit || 10);
        dataTable.value.page = parseInt(route.query.skip || 0);
    } else {
        filterStore.resetFilters();
        dataTable.value.size = 10;
        dataTable.value.page = 0;
    }
    fetchPurchaseReq();
});

const debounceF = debounce(onFilter, 1000);
</script>
