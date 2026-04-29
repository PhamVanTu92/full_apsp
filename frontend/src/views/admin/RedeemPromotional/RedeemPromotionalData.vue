<template>
    <div class="flex gap-3 justify-content-between align-items-center mb-3">
        <div class="flex gap-3">
            <h3 class="font-bold m-0" style="line-height: 33px">{{ t('Navbar.menu.setting_point_buy') }}
            </h3>
        </div>
        <div class="flex gap-3">
            <router-link to="predeempromotionals/new">
                <Button :label="t('body.SaleSchannel.add_new_button')" icon="pi pi-plus" />
            </router-link>

        </div>
    </div>
    <DataTable :value="promotions.data" :rows="query.limit" :totalRecords="promotions.count"
        :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)" :first="query.skip * query.limit" lazy
        paginator class="card p-2" showGridlines @page="onPage($event)" :pt="ptTable" filterDisplay="menu"
        v-model:filters="filterStore.filters" @filter="onFilter()">
        <template #header>
            <div class="flex justify-content-between">
                <div>
                    <IconField iconPosition="left">
                        <InputText :placeholder="t('body.OrderList.searchPlaceholder')"
                            v-model="filterStore.filters['global'].value" @input="debouncer" />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <div class="mt-1 text-orange-500">*{{ t('body.OrderList.searchNotification') }}</div>
                </div>

                <Button type="button" icon="pi pi-filter-slash" outlined severity="danger" @click="clearFilter()"
                    v-tooltip.bottom="t('client.clear_filter')" />
            </div>
        </template>
        <Column header="#">
            <template #body="slotProps">
                <div>
                    {{ slotProps.index + 1 }}
                </div>
            </template>
        </Column>
        <Column :header="t('body.promotion.program_code_column')" field="id" />
        <Column :header="t('body.promotion.program_name_column')" field="name" />

        <Column :header="t('body.promotion.from_date_column')" field="fromDate" filterField="fromDate" dataType="date"
            class="w-10rem" :filterMatchModeOptions="[
                { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
            ]">
            <template #body="{ data }">
                {{ format.DateTime(data.fromDate).date }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>

        <Column :header="t('body.promotion.to_date_column')" field="toDate" filterField="toDate" dataType="date"
            class="w-10rem" :filterMatchModeOptions="[
                { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
            ]">
            <template #body="{ data }">
                {{ format.DateTime(data.toDate).date }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>

        <Column :header="t('PromotionalItems.SetupPurchasesPoint.timeOther')" field="extendedToDate"
            filterField="extendedToDate" dataType="date" class="w-10rem" :filterMatchModeOptions="[
                { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
            ]">
            <template #body="{ data }">
                {{ format.DateTime(data.extendedToDate).date }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>

        <Column field="isActive" :header="t('body.promotion.status_column')" filterField="isActive" :options="[
            { label: t('body.sampleRequest.customer.active_status'), value: true },
            { label: t('body.sampleRequest.customer.inactive_status'), value: false }
        ]">
            <template #body="{ data }">
                <Tag :value="data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status')"
                    :severity="data.isActive ? 'success' : 'danger'" />
            </template>
            <template #filter="{ filterModel }">
                <MultiSelect v-model="filterModel.value" :options="[
                    { label: t('body.sampleRequest.customer.active_status'), value: true },
                    { label: t('body.sampleRequest.customer.inactive_status'), value: false }
                ]" optionLabel="label" optionValue="value" :placeholder="t('body.promotion.status_column')"
                    class="p-column-filter" showClear />
            </template>
        </Column>
        <Column field="" class="w-8rem">
            <template #body="{ data }">
                <div class="flex justify-content-center">
                    <Button text icon="pi pi-pencil" @click="editPromotion(data.id)" />
                </div>
            </template>
        </Column>
        <template #empty>
            <div class="p-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
        </template>
    </DataTable>
    <ConfirmDialog class="w-30rem"></ConfirmDialog>
    <Loading v-if="loading"></Loading>
</template>
<script setup>
import { ref, onBeforeMount, reactive, watch, inject } from 'vue';
import API from '@/api/api-main';
import { useRouter, useRoute } from 'vue-router';
import { debounce } from 'lodash';
import { FilterStore } from '@/Pinia/Filter/FilterPromotionDataPay';
import { useI18n } from 'vue-i18n';
import format from "@/helpers/format.helper";
import { FilterMatchMode } from 'primevue/api';
const conditionHandler = inject('conditionHandler');
const filterStore = FilterStore();
const router = useRouter();
const route = useRoute();
const loading = ref(false);
const promotions = reactive({
    data: [],
    count: 0
});

const { t } = useI18n();
const query = reactive({
    skip: 0,
    limit: 10,
    search: null,
    status: null
});

const onSearch = (reset) => {
    let filterQuery = [];
    if (reset) query.skip = 0;
    let search = query.search?.trim();
    if (search) {
        filterQuery.push(`&search=${encodeURIComponent(search)}`);
    }
    fetchAllPromotions(filterQuery.join('&'));
};

const debouncer = debounce(() => onSearch(), 1000);

watch(
    () => [query.status],
    () => {
        debouncer();
    }
);
const onFilter = () => {
    fetchAllPromotions();
};

const editPromotion = (id) => {
    router.push({
        name: 'detail-predeempromotionals',
        params: { id: id }
    });
};
const onPage = async (event) => {
    query.skip = event.page;
    query.limit = event.rows;
    router.push({
        name: route.name,
        query: {
            page: query.skip + 1,
            size: query.limit
        }
    });
};

watch(route, async () => {
    onSearch(false);
});

const fetchAllPromotions = async (filter) => {
    const filters = conditionHandler.getQuery(filterStore.filters);
    let url = `PointSetup?Page=${query.skip + 1}&PageSize=${query.limit}&orderBy=id desc`;
    if (filter) url += filter;
    if (filters) url += filters;
    loading.value = true;
    try {
        const response = await API.get(url);
        promotions.data = response.data['items'];
        promotions.count = response.data['total'];
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const initialComponent = async () => {
    if (route.query) {
        let skip = parseInt(route.query.page);
        skip = skip && skip > 0 ? skip - 1 : 0;
        query.skip = skip;
        query.limit = parseInt(route.query.size) || 10;
    }
    await fetchAllPromotions();
};

onBeforeMount(async () => {
    await initialComponent();
});
const clearFilter = () => {
    filterStore.resetFilters();
    query.skip = 0;
    fetchAllPromotions();
};
const ptTable = {
    rowexpansioncell: {
        class: 'surface-overlay'
    }
};

</script>
