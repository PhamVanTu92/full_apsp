<template>
    <div class="flex gap-3 justify-content-between align-items-center mb-3">
        <div class="flex gap-3">
            <h3 class="font-bold m-0" style="line-height: 33px">{{ t('Navbar.menu.productListretail') }}
            </h3>
            <div class="flex gap-3 align-items-center">
                <IconField iconPosition="left">
                    <InputIcon class="pi pi-search"> </InputIcon>
                    <InputText v-model="query.search" @input="debouncer()" class="w-30rem"
                        :placeholder="t('body.promotion.promotion_search_placeholder')" />
                </IconField>
            </div>
        </div>
        <div class="flex gap-3">
            <ExcelImportDlg v-if="0"></ExcelImportDlg>
            <router-link to="list-product-retail/new">
                <Button :label="t('body.SaleSchannel.add_new_button')" icon="pi pi-plus" />
            </router-link>
        </div>
    </div>
    <DataTable :value="promotions.data" :rows="query.limit" :totalRecords="promotions.count"
        :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)" :first="query.skip * query.limit" lazy
        paginator class="card p-2" showGridlines @page="onPage($event)" :pt="ptTable" filterDisplay="menu"
        v-model:filters="filterStore.filters" @filter="onFilter()">
        <Column header="#">
            <template #body="slotProps">
                <div>
                    {{ slotProps.index + 1 }}
                </div>
            </template>
        </Column>
        <Column :header="t('body.promotion.program_code_column')" field="id" />
        <Column :header="t('body.promotion.program_name_column')" field="priceListName" />
        <Column :header="t('body.promotion.from_date_column')">
            <template #body="{ data }">
                {{ format.formatDate(data.createdDate) }}
            </template>
        </Column>
        <Column :header="t('body.promotion.to_date_column')">
            <template #body="{ data }">
                {{ format.formatDate(data.expriedDate) }}
            </template>
        </Column>
        <Column field="isActive" :header="t('body.promotion.status_column')" :showFilterMatchModes="false">
            <template #body="{ data }">
                <Tag :value="data.isActive ? 'Hoạt động' : 'Không hoạt động'"
                    :severity="data.isActive ? 'success' : 'danger'" />
            </template>
        </Column>
        <Column field="" class="w-8rem">
            <template #body="{ data }">
                <div class="flex justify-content-center">
                    <Button text icon="pi pi-pencil" @click="router.push(`/list-product-retail/detail/${data.id}`);"/>
                </div>
            </template>
        </Column>
        <template #empty>
            <div class="p-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
        </template>
    </DataTable>
    <ConfirmDialog class="w-30rem"></ConfirmDialog>
    <Loading v-if="loading"></Loading>
    <OverlayPanel ref="op" class="w-25rem surface-100">
        <DataTable :value="doiTuongData.data" showGridlines scrollable scrollHeight="200px" size="small">
            <Column field="customerName" :pt:headerCell:style="'display:none'">
                <template #header>
                    <div class="flex justify-content-center flex-grow-1">
                        {{ doiTuongData.type }}
                    </div>
                </template>
                <template #body="{ data }">
                    <router-link class="text-green-700 hover:underline"
                        :to="`/agen-man/agency-category/${data.customerId}`">{{ data.customerName }}</router-link>
                </template>
            </Column>
        </DataTable>
    </OverlayPanel>
</template>
<script setup>
import format from "@/helpers/format.helper";
import ExcelImportDlg from './components/ExcelImportDlg.vue';
import { ref, onBeforeMount, reactive, watch, inject } from 'vue';
import API from '@/api/api-main';
import { useRouter, useRoute } from 'vue-router';
import { debounce } from 'lodash';
import { FilterStore } from '../../../Pinia/Filter/FilterPromotionData';
import { useI18n } from 'vue-i18n';

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
const op = ref();
const doiTuongData = reactive({
    type: null,
    data: []
});
 
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
    let url = `PriceList?Page=${query.skip + 1}&PageSize=${query.limit}&orderBy=id desc`;
    if (filter) url += filter;
    if (filters) url += filters;

    loading.value = true;
    try {
        const response = await API.get(url);
        promotions.data = response.data.data;
        promotions.count = response.data.total;
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

// static variables
const ptTable = {
    rowexpansioncell: {
        class: 'surface-overlay'
    }
};

</script>
