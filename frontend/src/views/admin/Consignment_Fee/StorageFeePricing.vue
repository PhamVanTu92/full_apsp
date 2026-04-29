<script setup>
import { ref, onBeforeMount } from "vue";
import API from "@/api/api-main";
import { format } from "date-fns";
import { useRouter, useRoute } from "vue-router";
import { FilterStore } from "@/Pinia/Filter/FilterStore";
import { FilterMatchMode } from "primevue/api";
import { inject } from "vue";
import { debounce } from "lodash";
import { useI18n } from "vue-i18n";

const conditionHandler = inject("conditionHandler");
const filterStore = FilterStore();
const router = useRouter();
const route = useRoute();
const DataFee = ref([]);
const loading = ref(false);
const dataTable = ref({
    limit: route.query.page ? route.query.page : 10,
    skip: route.query.pageSize ? route.query.size : 0,
});

const { t } = useI18n();

onBeforeMount(() => {
    GetFee();
});

const GetFee = async () => {
    try {
        loading.value = true;
        const filters = conditionHandler.getQuery(filterStore.filters);
        const queryParams = `?page=${dataTable.value.skip}&pageSize=${dataTable.value.limit}${filters}`;
        const res = await API.get(`Fee${queryParams}`);
        if (res.data) {
            DataFee.value = res.data.items;
            Object.assign(dataTable.value, {
                // limit: res.data.limit,
                // skip: res.data.skip,
                total: res.data.total,
            });
            router.push(queryParams);
        }
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const onPageChange = (event) => {
    dataTable.value.skip = event.page;
    dataTable.value.limit = event.rows;
    GetFee();
};

const onFilter = () => {
    dataTable.value.skip = 0;
    GetFee();
};
const debounceF = debounce(onFilter, 1000);

const clearFilter = () => {
    filterStore.resetFilters();
    GetFee();
};
</script>
<template>
    <div class="mb-3 flex justify-content-between align-items-center">
        <h4 class="font-bold m-0">{{ t('body.sampleRequest.warehouseFee.title') }}</h4>

        <div class="flex gap-2">
            <router-link to="/storage-fee-pricing/new"><Button icon="pi pi-plus"
                    :label="t('body.systemSetting.add_new_button')" /></router-link>
        </div>
    </div>
    <div class="card p-2">
        <DataTable stripedRows class="table-main" showGridlines dataKey="id" tableStyle="min-width: 50rem;"
            header="surface-200" paginator :rows="dataTable.limit" :page="dataTable.skip"
            :totalRecords="dataTable.total" @page="onPageChange($event)" :rowsPerPageOptions="[10, 20, 30]" lazy
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
            filterDisplay="menu" v-model:filters="filterStore.filters" :globalFilterFields="['customer', 'status']"
            :filterLocale="'vi'" @filter="onFilter" :value="DataFee">
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText :placeholder="t('body.sampleRequest.warehouseFee.searchPlaceholder')"
                            v-model="filterStore.filters['global'].value" @input="debounceF()" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <Button type="button" icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')"
                        severity="danger" outlined @click="clearFilter()" />
                </div>
            </template>
            <Column header="#">
                <template #body="{ index }">
                    <span>{{ index + 1 }}</span>
                </template>
            </Column>
            <Column field="name" :header="t('body.sampleRequest.warehouseFee.price_list_name_label')"> </Column>
            <Column field="fromDate" :header="t('body.sampleRequest.warehouseFee.from_date_label')"
                filterField="fromDate" dataType="date" :filterMatchModeOptions="[
                    { label: t('body.sampleRequest.warehouseFee.from_date_label'), value: FilterMatchMode.DATE_AFTER },
                    { label: t('body.sampleRequest.warehouseFee.to_date_label'), value: FilterMatchMode.DATE_BEFORE },
                ]">
                <template #body="{ data }">
                    <span>{{ format(data.fromDate, "dd/MM/yyyy") }}</span>
                </template>
                <template #filter="{ filterModel }">
                    <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy"
                        mask="99/99/9999" />
                </template>
            </Column>
            <Column field="dueDate" :header="t('body.sampleRequest.warehouseFee.to_date_label')" filterField="dueDate"
                dataType="date" :filterMatchModeOptions="[
                    { label: t('body.sampleRequest.warehouseFee.from_date_label'), value: FilterMatchMode.DATE_AFTER },
                    { label: t('body.sampleRequest.warehouseFee.to_date_label'), value: FilterMatchMode.DATE_BEFORE },
                ]">
                <template #body="{ data }">
                    <span>{{ format(data.toDate, "dd/MM/yyyy") }}</span>
                </template>
                <template #filter="{ filterModel }">
                    <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy"
                        mask="99/99/9999" />
                </template>
            </Column>
            <Column field="status" :header="t('body.sampleRequest.warehouseFee.status_label')"
                :showFilterMatchModes="false">
                <template #body="{ data }">
                    <Tag :value="data.status ? t('body.sampleRequest.warehouseFee.status_active') : t('body.sampleRequest.warehouseFee.status_inactive')"
                        :severity="data.status ? 'primary' : 'danger'"></Tag>
                </template>
                <template #filter="{ filterModel }">
                    <MultiSelect v-model="filterModel.value" :options="[
                        { name: t('body.sampleRequest.warehouseFee.status_active'), code: true },
                        { name: t('body.sampleRequest.warehouseFee.status_inactive'), code: false },
                    ]" optionLabel="name" optionValue="code" :placeholder="t('body.OrderApproval.clear')"
                        class="p-column-filter" showClear>
                    </MultiSelect>
                </template>
            </Column>
            <Column field="createDate" :header="t('body.OrderApproval.createDate')" filterField="createDate"
                dataType="date" :filterMatchModeOptions="[
                    { label: t('body.sampleRequest.warehouseFee.from_date_label'), value: FilterMatchMode.DATE_AFTER },
                    { label: t('body.sampleRequest.warehouseFee.to_date_label'), value: FilterMatchMode.DATE_BEFORE },
                ]">
                <template #body="{ data }">
                    <span>{{ format(data.createDate, "dd/MM/yyyy") }}</span>
                </template>
                <template #filter="{ filterModel }">
                    <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy"
                        mask="99/99/9999" />
                </template>
            </Column>
            <Column field="userId" :header="t('body.systemSetting.creator')">
                <template #body="{ data }">
                    <span>{{ data.appUser.fullName }}</span>
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.warehouseFee.actions')">
                <template #body="{ data }">
                    <div class="flex gap-2">
                        <router-link :to="'/storage-fee-pricing/update/' + data.id">
                            <Button text icon="pi pi-eye"></Button></router-link>
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="p-2 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
            </template>
        </DataTable>
    </div>
    <Loading v-if="loading"></Loading>
</template>

<style scoped>
.row_gap>div:last-child {
    margin: 0 !important;
    padding-bottom: 0px !important;
}

.row_gap>div:not(:last-child) {
    border-bottom: 1px var(--surface-border) solid;
}
</style>
