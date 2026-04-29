<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <h4 class="m-0 font-bold">{{ t('body.PurchaseRequestList.pickupRequestList') }}</h4>
        <AddOrdersShipComp @fetchData="fetchPurchaseReq"></AddOrdersShipComp>
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
            :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
            @filter="onFilter"
        >
            <template #empty>
                <div class="p-2 text-center">
                    {{ t('body.PurchaseRequestList.no_matching_product_message') }}
                </div>
            </template>
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText :placeholder="t('body.PurchaseRequestList.searchOrderPlaceholder')" v-model="filterStore.filters['global'].value" @input="debounceF" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <Button type="button" icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')" severity="danger" outlined @click="clearFilter()" />
                </div>
            </template>
            <Column field="invoiceCode" :header="t('body.PurchaseRequestList.requestCode')" class="w-13rem">
                <template #body="slotProps">
                    <router-link class="text-primary hover:underline font-semibold" :to="`/purchase-request/detail/${slotProps.data.id}`">
                        <span>{{ slotProps.data.invoiceCode }}</span>
                    </router-link>
                </template>
            </Column>
            <Column field="cardName" :header="t('body.PurchaseRequestList.customer')">
                <template #body="sp">
                    <router-link class="text-primary hover:underline font-semibold" :to="'/agen-man/agency-category/' + sp.data.cardId">
                        <span>{{ sp.data.cardName }}</span>
                    </router-link>
                </template>
            </Column>

            <Column field="status" :header="t('body.PurchaseRequestList.status')" style="width: 10rem" :showFilterMatchModes="false">
                <template #body="slotProps">
                    <Tag :severity="getStatusLabelPromotion(slotProps.data.status)['severity']" :value="getStatusLabelPromotion(slotProps.data.status)['label']" :class="getStatusLabelPromotion(slotProps.data.status)['class'] || ''" />
                </template>
                <template #filter="{ filterModel }">
                    <MultiSelect v-model="filterModel.value" :options="statuses" optionLabel="name" optionValue="code" :placeholder="t('body.PurchaseRequestList.status')" class="p-column-filter" showClear>
                        <template #option="slotProps">
                            <Tag :value="getStatusLabelPromotion(slotProps.option.code).label" :severity="getStatusLabelPromotion(slotProps.option.code).severity" :class="getStatusLabelPromotion(slotProps.option.code)['class'] || ''" />
                        </template>
                    </MultiSelect>
                </template>
            </Column>
            <Column
                field="docDate"
                :header="t('body.PurchaseRequestList.time')"
                filterField="docDate"
                dataType="date"
                :filterMatchModeOptions="[
                    {
                        label: t('body.report.from_date_label_2'),
                        value: FilterMatchMode.DATE_AFTER
                    },
                    { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
                ]"
                class="w-10rem"
            >
                <template #body="slotProps">
                    <span class="border-1 border-300 py-1">
                        <span class="surface-300 px-3 p-1">{{ format.DateTime(slotProps.data?.docDate || '')?.time }}</span>
                        <span class="px-3 p-1">
                            {{ format.DateTime(slotProps.data?.docDate || '')?.date }}
                        </span>
                    </span>
                </template>
                <template #filter="{ filterModel }">
                    <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
                </template>
            </Column>
        </DataTable>
    </div>
    <ConfirmDialog style="min-width: 30rem">
        <template #message="sp">
            <div class="w-full">
                <div class="field grid">
                    <span class="col-5 font-bold text-right">{{ t('body.PurchaseRequestList.requestCode') }}:</span>
                    <span class="col">{{ sp.message.message['order_tracking_id'] }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">{{ t('body.PurchaseRequestList.customer') }}:</span>
                    <span class="col">{{ sp.message.message['buyer']?.['buyer_name'] }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">{{ t('body.PurchaseRequestList.status') }}:</span>
                    <span class="col">{{ sp.message.message['status'] }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">{{ t('body.PurchaseRequestList.quantity_column') }}:</span>
                    <span class="col">{{ sp.message.message['products'].length }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">{{ t('body.OrderList.totalAmount') }}:</span>
                    <span class="col">{{ format.FormatCurrency(sp.message.message['total_amount']) }}</span>
                </div>
            </div>
        </template>
    </ConfirmDialog>
    <loading v-if="isLoading" />
</template>
<script setup>
import { onBeforeMount, ref } from 'vue';
import API from '@/api/api-main';
import { useRouter, useRoute } from 'vue-router';
import format from '@/helpers/format.helper';
import { getStatusLabel, getStatusLabelPromotion } from '@/helpers/statusLabels.helper';
import { FilterStore } from '@/Pinia/Filter/FilterStorePurchaseRequest';
import { FilterMatchMode } from 'primevue/api';
import { inject } from 'vue';
import { debounce } from 'lodash';
import { useI18n } from 'vue-i18n';
import { getStatuses, getStatusesPromotion } from '@/components/Status';

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
const statuses = ref(getStatusesPromotion(t));
const onFilter = () => {
    dataTable.value.page = 0;
    fetchPurchaseReq();
};
const debounceF = debounce(onFilter, 1000);
const clearFilter = () => {
    filterStore.resetFilters();
    dataTable.value.page = 0;
    fetchPurchaseReq();
};
const onPageChange = (event) => {
    dataTable.value.page = event.page;
    dataTable.value.size = event.rows;
    fetchPurchaseReq();
};

const hasListQuery = () => {
    return route.query.limit != undefined || route.query.skip != undefined || route.query.filter != undefined;
};

const fetchPurchaseReq = async () => {
    isLoading.value = true;
    const filters = conditionHandler.getQuery(filterStore.filters);
    const query = `?limit=${dataTable.value.size}&skip=${dataTable.value.page}${filters}`;
    try {
        const res = await API.get(`PurchaseRequest${query}`);
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

</script>
