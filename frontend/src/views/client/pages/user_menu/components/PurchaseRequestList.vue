<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <strong class="text-2xl">{{t('body.PurchaseRequestList.pickupRequestList')}}</strong>
        <AddOrdersShipComp @fetchData="fetchPurchaseReq"></AddOrdersShipComp>
    </div>
    <div class="grid mt-3 card p-2">
        <div class="col-12">
            <DataTable
                stripedRows
                class="table-main"
                showGridlines
                :value="PurchaseRequest"
                tableStyle="min-width: 50rem;"
                header="surface-200"
                paginator
                :rows="dataTable.size || 0"
                :page="dataTable.page || 0"
                :totalRecords="dataTable.total_size || 0"
                @page="onPageChange($event)"
                :rowsPerPageOptions="[10, 20, 30]"
                lazy
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                 :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                filterDisplay="menu"
                :filterLocale="'vi'"
                v-model:filters="filterStore.filters"
                @filter="onFilter"
            >
                <template #empty>
                    <div class="py-5 my-5 text-center">
                        {{t('client.no_matching_request')}}
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between">
                        <IconField iconPosition="left">
                            <InputText
                                class="w-19rem"
                                :placeholder="t('client.search_pickup_request')"
                                v-model="filterStore.filters['global'].value"
                                @input="debouncedFilter"
                            />
                            <InputIcon>
                                <i class="pi pi-search" @click="onFilter()" />
                            </InputIcon>
                        </IconField>
                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            :label="t('body.OrderApproval.clear')"
                            outlined
                            @click="clearFilter()"
                        />
                    </div>
                </template>
                <Column field="invoiceCode" :header="t('client.request_code')">
                    <template #body="slotProps">
                        <router-link :to="`purchase-request-client/${slotProps.data.id}`">
                            <span class="flex align-items-center font-semibold">
                                <Button
                                    icon="fa-solid fa-arrow-right"
                                    text
                                    class="text-primary"
                                />
                                <span class="text-primary">{{
                                    slotProps.data.invoiceCode
                                }}</span>
                            </span>
                        </router-link>
                    </template>
                </Column>

                <Column
                    field="status"
                    :header="t('body.promotion.status_label')"
                    style="width: 10rem"
                    :showFilterMatchModes="false"
                >
                    <template #body="slotProps">
                        <Tag
                            :severity="getStatusLabel(slotProps.data.status)['severity']"
                            :value="getStatusLabel(slotProps.data.status)['label']"
                        />
                    </template>
                    <template #filter="{ filterModel }">
                        <MultiSelect
                            v-model="filterModel.value"
                            :options="statuses"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn trạng thái"
                            class="p-column-filter"
                            showClear
                        >
                            <template #option="slotProps">
                                <Tag
                                    :value="getStatusLabel(slotProps.option.code).label"
                                    :severity="
                                        getStatusLabel(slotProps.option.code).severity
                                    "
                                />
                            </template>
                        </MultiSelect>
                    </template>
                </Column>
                <Column
                    field="docDate"
                    :header="t('client.creation_time')"
                    style="width: 10rem"
                    filterField="docDate"
                    dataType="date"
                >
                    <template #body="slotProps">
                        <span class="border-1 border-300 py-1">
                            <span class="surface-300 px-3 p-1">{{
                                format.DateTime(slotProps.data.docDate).time
                            }}</span>
                            <span class="px-3 p-1">{{
                                format.DateTime(slotProps.data.docDate).date
                            }}</span>
                        </span>
                    </template>
                    <template #filter="{ filterModel }">
                        <Calendar
                            v-model="filterModel.value"
                            dateFormat="dd/mm/yy"
                            placeholder="dd/mm/yy"
                            mask="99/99/9999"
                        />
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
    <ConfirmDialog style="min-width: 30rem">
        <template #message="sp">
            <div class="w-full">
                <div class="field grid">
                    <span class="col-5 font-bold text-right">Mã đơn hàng:</span>
                    <span class="col">{{ sp.message.message["order_tracking_id"] }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">Khách hàng:</span>
                    <span class="col">{{
                        sp.message.message["buyer"]["buyer_name"]
                    }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">Trạng thái:</span>
                    <span class="col">{{ sp.message.message["status"] }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">Số lượng sản phẩm:</span>
                    <span class="col">{{ sp.message.message["products"].length }}</span>
                </div>
                <div class="field grid">
                    <span class="col-5 font-bold text-right">Tổng đơn giá:</span>
                    <span class="col">{{
                        format.FormatCurrency(sp.message.message["total_amount"])
                    }}</span>
                </div>
            </div>
        </template>
    </ConfirmDialog>
    <loading v-if="isLoading"></loading>
</template>
<style scoped></style>
<script setup>
import { onBeforeMount, ref, watch } from "vue";
import API from "@/api/api-main";
import { useRouter } from "vue-router";
import format from "@/helpers/format.helper";
import { FilterStore } from "@/Pinia/Filter/FilterStore";
import { FilterMatchMode } from "primevue/api";
import { inject } from "vue";

import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const conditionHandler = inject("conditionHandler");
const router = useRouter();
const dataTable = ref({
    page: 0,
    size: 10,
    total_size: 0,
});
const filterStore = FilterStore();
const isLoading = ref(false);
const PurchaseRequest = ref([]);

const fetchPurchaseReq = async (filters = "") => {
    isLoading.value = true;
    try {
        const queryParam = `?limit=${dataTable.value.size}&skip=${dataTable.value.page}${filters}`;
        const res = await API.get(`me/PurchaseRequest${queryParam}`);
        PurchaseRequest.value = res.data.items?.length ? res.data.items : [];
        dataTable.value.total_size = res.data.total;
        router.push(queryParam);
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};

const labels = {
    DXL: {
        severity: "warning",
        label: "Đang xử lý",
    },
    DXN: {
        severity: "info",
        label: "Đã xác nhận",
    },
    HUY: {
        severity: "danger",
        label: "Đã hủy",
    },
    DGH: {
        severity: "secondary",
        label: "Giao hàng",
    },
    DHT: {
        severity: "success",
        label: "Hoàn thành",
    },
};
const statuses = ref([
    { code: "DXL", name: "Đang xử lý" },
    { code: "DXN", name: "Đã xác nhận" },
    { code: "HUY", name: "Đã hủy" },
    { code: "DGH", name: "Đang giao hàng" },
    { code: "DHT", name: "Đã hoàn thành" },
]);
const getStatusLabel = (str) => {
    return labels[str] || { severity: "warning", label: "Đang xử lý" };
};

// Filter
const onPageChange = (event) => {
    dataTable.value.page = event.page;
    dataTable.value.size = event.rows;
    onFilter();
};

onBeforeMount(() => {
    onFilter();
});

const onFilter = () => {
    const queryString = conditionHandler.getQuery(filterStore.filters);
    fetchPurchaseReq(queryString);
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
    fetchPurchaseReq();
};
</script>
