<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <h4 class="m-0 font-bold">{{ t('ChangePoint.title') }}</h4>
        <router-link to="/client/setup/promotiondetail/add">
            <Button :label="t('ChangePoint.buttonChangePoint')" icon="pi pi-plus"/>
        </router-link>
    </div>
    <div class="card p-3">
        <DataTable stripedRows showGridlines dataKey="id" :value="PurchaseRequest" tableStyle="min-width: 50rem;"
            header="surface-200" paginator :rows="dataTable.size" :page="dataTable.page"
            :first="dataTable.page * dataTable.size" :totalRecords="dataTable.total_size" @page="onPageChange($event)"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)" lazy filterDisplay="menu"
            :filterLocale="'vi'" v-model:filters="filterStore.filters" :globalFilterFields="['customer', 'status']"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            @filter="onFilter">
            <template #empty>
                <div class="p-2 text-center">{{ t('body.PurchaseRequestList.no_matching_product_message') }}</div>
            </template>
            <template #header>
                <IconField class="items-center" iconPosition="left">
                    <InputText :placeholder="t('body.home.search_placeholder')"
                        v-model="filterStore.filters['global'].value" @input="debounceF" />
                </IconField>
            </template>
            <Column field="id" :header="t('ChangePoint.orderCode')">
                <template #body="slotProps">
                    <router-link :to="{
                        name: 'production-commitment-detail',
                        params: { id: slotProps.data.id }
                    }">
                        <Button icon="fa-solid fa-arrow-right" text class="p-1 text-primary"
                            :label="slotProps.data.invoiceCode"/>
                    </router-link>
                </template>
            </Column>
            <Column field="cardName" :header="t('client.customer_name')" />
            <Column :header="t('ChangePoint.usingPoints')">
                <template #body="slotProps">
                    <span>{{ getPrice(slotProps.data.itemDetail) }}</span>
                </template>
            </Column>
            <Column field="status" :header="t('body.PurchaseRequestList.status')">
                <template #body="slotProps">
                    <Tag :class="getStatusLabel(slotProps.data.status)['class']"
                        :value="getStatusLabel(slotProps.data.status)['label']" />
                </template>
            </Column>
            <Column field="docDate" :header="t('body.OrderList.time')" class="w-10rem" filterField="docDate"
                dataType="date" :filterMatchModeOptions="[
                    { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                    { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
                ]">
                <template #body="slotProps">
                    <span class="border-1 border-300 py-1">
                        <span class="surface-300 px-3 p-1">{{ format.DateTime(slotProps.data.docDate).time }}</span>
                        <span class="px-3 p-1">
                            {{ format.DateTime(slotProps.data.docDate).date }}
                        </span>
                    </span>
                </template>
            </Column>
        </DataTable>
    </div>

    <loading v-if="isLoading"></loading>
</template>
<script setup>
import { onBeforeMount, ref, inject } from "vue";
import API from "@/api/api-main";
import { useRouter, useRoute } from "vue-router";
import { FilterStore } from "@/Pinia/Filter/FilterStorePurchaseRequest";
import format from '@/helpers/format.helper';
import { FilterMatchMode } from 'primevue/api';
import { debounce } from "lodash";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const conditionHandler = inject("conditionHandler");
const router = useRouter();
const route = useRoute();
const filterStore = FilterStore();
const dataTable = ref({
    page: 0,
    size: 10,
    total_size: 0,
});
const isLoading = ref(false);
const PurchaseRequest = ref([]);
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
const getPrice = (items) => {
    if (!items || items.length === 0) return 0;
    return format.FormatCurrency(
        items.reduce((total, item) => total + (item.price || 0) * (item.quantity || 0), 0)
    );
};
const fetchPurchaseReq = async () => {
    isLoading.value = true;
    const filters = conditionHandler.getQuery(filterStore.filters);
    const query = `?Pagesize=${dataTable.value.size}&page=${dataTable.value.page}${filters}`;
    try {
        const res = await API.get(`Redeem${query}`);
        PurchaseRequest.value = res.data.items;
        dataTable.value.total_size = res.data.total;
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
        router.push(`${query}`);
    }
};

onBeforeMount(async () => {
    if (route.query.limit != undefined || route.query.skip != undefined) {
        dataTable.value.size = Number.parseInt(route.query.limit || 10);
        dataTable.value.page = Number.parseInt(route.query.skip || 0);
    }
    fetchPurchaseReq();
});

const onFilter = () => {
    dataTable.value.page = 0;
    fetchPurchaseReq();
};
const onPageChange = (event) => {
    dataTable.value.page = event.page;
    dataTable.value.size = event.rows;
    fetchPurchaseReq();
};

const debounceF = debounce(onFilter, 1000);
</script>
