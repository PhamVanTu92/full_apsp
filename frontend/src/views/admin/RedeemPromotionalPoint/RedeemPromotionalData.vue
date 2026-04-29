<template>
    <div class="flex gap-3 justify-content-between align-items-center mb-3">
        <div class="flex gap-3">
            <h3 class="font-bold m-0" style="line-height: 33px">{{ t('PromotionalItems.SetupPurchasesPoint.title') }}
            </h3>
        </div>
        <div class="flex gap-3">
            <ExcelImportDlg v-if="0"></ExcelImportDlg>
            <router-link to="promotionalPoints/new">
                <Button :label="t('body.SaleSchannel.add_new_button')" icon="pi pi-plus" />
            </router-link>
        </div>
    </div>
    <DataTable :value="promotions.data" :rows="query.limit" :totalRecords="promotions.count"
        :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)" :first="query.skip * query.limit" lazy
        paginator class="card p-2" showGridlines @page="onPage($event)" filterDisplay="menu"
        v-model:filters="filterStore.filters" @filter="onFilter()">
        <template #header>
            <div class="flex justify-content-between">
                <IconField iconPosition="left">
                    <InputText :placeholder="t('body.OrderList.searchPlaceholder')"
                        v-model="filterStore.filters['global'].value" @input="debounceF" />
                    <InputIcon>
                        <i class="pi pi-search" @click="onFilter()" />
                    </InputIcon>
                </IconField>
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
        <Column field="startDate" :header="t('body.home.from')" filterField="startDate" dataType="date"
            :showFilterMatchModes="true" :filterMatchModeOptions="[
                { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
            ]">
            <template #body="{ data }"> {{ format.DateTime(data.startDate).date }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>
        <Column field="endDate" :header="t('body.home.to')" filterField="endDate" dataType="date"
            :showFilterMatchModes="true" :filterMatchModeOptions="[
                { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
                { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
            ]">
            <template #body="{ data }"> {{ format.DateTime(data.endDate).date }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>
        <Column field="isActive" :header="t('body.promotion.status_column')">
            <template #body="{ data }">
                <Tag :value="data.isActive ? 'Hoạt động' : 'Không hoạt động'"
                    :severity="data.isActive ? 'success' : 'danger'" />
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
    <Loading v-if="loading"></Loading>
</template>
<script setup>
import format from "@/helpers/format.helper";
import ExcelImportDlg from './components/ExcelImportDlg.vue';
import { ref, onBeforeMount, reactive, watch, inject } from 'vue';
import API from '@/api/api-main';
import { useRouter, useRoute } from 'vue-router';
import { debounce } from 'lodash';
import { FilterStore } from '@/Pinia/Filter/FilterPromotionData';
import { useI18n } from 'vue-i18n';
import { FilterMatchMode } from 'primevue/api';

const { t } = useI18n();
const conditionHandler = inject('conditionHandler');
const filterStore = FilterStore();
const router = useRouter();
const route = useRoute();
const loading = ref(false);

const promotions = reactive({
    data: [],
    count: 0
});

const query = reactive({
    skip: 0,
    limit: 10,
    search: null,
    status: null
});

const getApiUrl = (filter = '') => {
    const filters = conditionHandler.getQuery(filterStore.filters);
    let url = `ExchangePoint?Page=${query.skip + 1}&PageSize=${query.limit}&orderBy=id desc`;
    if (filter) url += filter;
    if (filters) url += filters;
    return url;
};

const onFilter = () => {
    query.skip = 0;
    fetchAllPromotions();
};

const debounceF = debounce(onFilter, 1000);

const clearFilter = () => {
    filterStore.resetFilters();
    query.skip = 0;
    fetchAllPromotions();
};

const fetchAllPromotions = async (filter = '') => {
    loading.value = true;
    try {
        const url = getApiUrl(filter);
        const response = await API.get(url);
        promotions.data = response.data.items || [];
        promotions.count = response.data.total || 0;
    } catch (error) {
        console.error('Lỗi tải dữ liệu:', error);
        promotions.data = [];
        promotions.count = 0;
    } finally {
        loading.value = false;
    }
};

const onPage = (event) => {
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

const editPromotion = (id) => {
    router.push({
        name: 'detail-promotionalPoints',
        params: { id }
    });
};

const initializeComponent = async () => {
    if (route.query?.page || route.query?.size) {
        query.skip = Math.max(0, parseInt(route.query.page || 1) - 1);
        query.limit = parseInt(route.query.size || 10);
    }
    await fetchAllPromotions();
};

watch(route, () => {
    initializeComponent();
});

onBeforeMount(async () => {
    await initializeComponent();
});
</script>
