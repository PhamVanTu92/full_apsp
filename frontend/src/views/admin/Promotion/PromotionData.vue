<template>
    <div class="grid">
        <!-- Header Section -->
        <div class="col-12 flex justify-content-between">
            <h3 class="font-bold m-0">
                {{ t('body.promotion.promotion_title') }}
            </h3>
            <div class="flex gap-3">
                <router-link :to="'promotion/new'">
                    <Button icon="pi pi-plus" :label="t('body.promotion.add_promotion_button')" />
                </router-link>
            </div>
        </div>
    </div>

    <!-- Data Table -->
    <DataTable :value="promotions.data" :rows="pagination.limit" :totalRecords="promotions.count"
        :rowsPerPageOptions="ROWS_PER_PAGE_OPTIONS" :first="pagination.skip * pagination.limit" lazy paginator
        class="card p-2" showGridlines @page="handlePageChange" :pt="TABLE_PT_CONFIG" filterDisplay="menu"
        v-model:filters="filterStore.filters" @filter="handleFilter">
        <!-- Table Header -->
        <template #header>
            <div class="flex justify-content-between">
                <IconField iconPosition="left">
                    <InputText :placeholder="t('body.OrderList.searchPlaceholder')"
                        v-model="filterStore.filters['global'].value" @input="debouncedFilter" />
                    <InputIcon>
                        <i class="pi pi-search" @click="handleFilter" />
                    </InputIcon>
                </IconField>
                <div class="flex align-items-center gap-2">
                    <!-- <Calendar class="w-10rem" v-model="filterStore.filters['fromDate'].value" :placeholder="t('body.promotion.from_date_column')" @date-select="handleFilter" dateFormat="dd/mm/yy" /> -
                    <Calendar class="w-10rem" v-model="filterStore.filters['toDate'].value" :placeholder="t('body.promotion.to_date_column')" @date-select="handleFilter" dateFormat="dd/mm/yy" /> -->
                    <Button type="button" icon="pi pi-filter-slash" outlined severity="danger"
                        @click="handleClearFilter" v-tooltip.bottom="t('client.clear_filter')" />
                </div>
            </div>
        </template>
        <Column field="promotionCode" :header="t('body.promotion.program_code_column')" />
        <Column field="promotionName" :header="t('body.promotion.program_name_column')" />
        <Column :header="t('body.promotion.from_date_column')" filterField="fromDate" dataType="date"
            :filterMatchModeOptions="filterFromDate">
            <template #body="{ data }">
                {{ formatDate(data.fromDate) }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>
        <Column :header="t('body.promotion.to_date_column')" filterField="toDate" dataType="date"
            :filterMatchModeOptions="filterToDate">
            <template #body="{ data }">
                {{ formatDate(data.toDate) }}
            </template>
            <template #filter="{ filterModel }">
                <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy" placeholder="dd/mm/yy" mask="99/99/9999" />
            </template>
        </Column>
        <Column :header="t('body.promotion.form_column')">
            <template #body="{ data }">
                {{ getFormTypeLabel(data.promotionType) }}
            </template>
        </Column>
        <Column :header="t('body.promotion.applicable_object_column')">
            <template #body="{ data }">
                <div v-if="getApplicableObjectInfo(data).isAll">
                    {{ t('body.promotion.applicable_object_all') }}
                </div>
                <Button v-else outlined size="small" :severity="getApplicableObjectInfo(data).severity"
                    @click="handleShowApplicableObjects($event, data.promotionCustomer)">
                    {{ getApplicableObjectInfo(data).message }}
                </Button>
            </template>
        </Column>
        <Column field="promotionStatus" :header="t('body.promotion.status_column')" :showFilterMatchModes="false">
            <template #filter="{ filterModel }">
                <MultiSelect v-model="filterModel.value" :options="STATUS_OPTIONS" optionLabel="name" optionValue="code"
                    :placeholder="t('body.promotion.select_status')" class="p-column-filter" showClear />
            </template>
            <template #body="{ data }">
                <Tag :severity="getStatusInfo(data.promotionStatus)?.severity"
                    :value="getStatusInfo(data.promotionStatus)?.label" />
            </template>
        </Column>
        <Column class="w-8rem">
            <template #body="{ data }">
                <div class="flex justify-content-center">
                    <Button text icon="pi pi-pencil" @click="handleEditPromotion(data.id)" />
                </div>
            </template>
        </Column>
        <template #empty>
            <div class="p-5 text-center">
                {{ t('body.systemSetting.no_data_to_display') }}
            </div>
        </template>
    </DataTable>
    <ConfirmDialog class="w-30rem" />
    <Loading v-if="loading" />

    <OverlayPanel ref="overlayPanelRef" class="w-25rem surface-100">
        <DataTable :value="applicableObjectsData.data" showGridlines scrollable scrollHeight="200px" size="small">
            <Column field="customerName" :pt:headerCell:style="'display:none'">
                <template #header>
                    <div class="flex justify-content-center flex-grow-1">
                        {{ applicableObjectsData.type }}
                    </div>
                </template>
                <template #body="{ data }">
                    <router-link class="text-green-700 hover:underline"
                        :to="`/agen-man/agency-category/${data.customerId}`">
                        {{ data.customerName }}
                    </router-link>
                </template>
            </Column>
        </DataTable>
    </OverlayPanel>
</template>

<script setup>
import { ref, reactive, onBeforeMount, watch, inject, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';
import { debounce } from 'lodash';
import { format } from 'date-fns';
import API from '@/api/api-main';
import { FilterStore } from '@/Pinia/Filter/FilterPromotionDataNEW';
import { FilterMatchMode } from 'primevue/api';
// ============================================================================
// Constants
// ============================================================================

const ROWS_PER_PAGE_OPTIONS = Array.from({ length: 10 }, (_, i) => (i + 1) * 10);

const TABLE_PT_CONFIG = {
    rowexpansioncell: {
        class: 'surface-overlay'
    }
};

const FORM_TYPE_LABELS = {
    1: 'Hoá đơn',
    2: 'Hàng hoá',
    3: 'Hoá đơn & hàng hoá',
    4: 'Ngày sinh nhật'
};

const STATUS_LABELS = {
    A: {
        label: computed(() => t('body.promotion.status_active')),
        severity: 'primary'
    },
    I: {
        label: computed(() => t('body.promotion.status_inactive')),
        severity: 'secondary'
    }
};

// ============================================================================
// Composables & Services
// ============================================================================

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const conditionHandler = inject('conditionHandler');
const filterStore = FilterStore();

// ============================================================================
// Reactive State
// ============================================================================
const filterFromDate = [
    { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
    { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
];
const filterToDate = [
    { label: t('body.report.from_date_label_2'), value: FilterMatchMode.DATE_AFTER },
    { label: t('body.report.to_date_label_2'), value: FilterMatchMode.DATE_BEFORE }
];
const loading = ref(false);
const overlayPanelRef = ref();

const promotions = reactive({
    data: [],
    count: 0
});

const pagination = reactive({
    skip: 0,
    limit: 10
});

const applicableObjectsData = reactive({
    type: null,
    data: []
});

const STATUS_OPTIONS = computed(() => [
    { name: t('body.promotion.status_inactive'), code: 'I' },
    { name: t('body.promotion.status_active'), code: 'A' }
]);

// ============================================================================
// Helper Functions
// ============================================================================

const formatDate = (dateString) => {
    return format(new Date(dateString), 'dd/MM/yyyy');
};

const getFormTypeLabel = (type) => {
    return FORM_TYPE_LABELS[type] ?? 'Unknown';
};

const getStatusInfo = (status) => {
    return STATUS_LABELS[status] ?? null;
};

const getApplicableObjectInfo = (data) => {
    const result = {
        isAll: data.isAllCustomer,
        message: null,
        severity: null
    };

    if (data.isAllCustomer) {
        result.message = t('body.promotion.applicable_object_all');
        return result;
    }

    const count = data.promotionCustomer?.length || 0;
    const ignorePrefix = data.isIgnore ? 'Tất cả trừ' : '';
    const customerType = data.promotionCustomer?.[0]?.type;

    if (customerType === 'C') {
        result.severity = data.isIgnore ? 'danger' : 'primary';
        result.message = `${ignorePrefix} ${count} khách hàng`;
    } else if (customerType === 'G') {
        result.severity = data.isIgnore ? 'danger' : 'info';
        result.message = `${ignorePrefix} ${count} nhóm khách hàng`;
    }

    return result;
};

// ============================================================================
// API Functions
// ============================================================================

const fetchPromotions = async () => {
    const queryFilters = JSON.parse(JSON.stringify(filterStore.filters));
    if (queryFilters.fromDate?.value) {
        queryFilters.fromDate.value = format(new Date(queryFilters.fromDate.value), 'yyyy-MM-dd');
    }
    if (queryFilters.toDate?.value) {
        queryFilters.toDate.value = format(new Date(queryFilters.toDate.value), 'yyyy-MM-dd');
    }

    const filters = conditionHandler.getQuery(queryFilters);

    let url = `promotion?skip=${pagination.skip}&limit=${pagination.limit}`;

    if (filters) {
        url += filters;
    }

    loading.value = true;

    try {
        const response = await API.get(url);
        promotions.data = response.data.items;
        promotions.count = response.data.total;
    } catch (error) {
        console.error('Error fetching promotions:', error);
        toast.add({
            severity: 'error',
            summary: t('body.systemSetting.error_label'),
            detail: 'Đã có lỗi xảy ra khi tải dữ liệu',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

// ============================================================================
// Event Handlers
// ============================================================================

const handlePageChange = async (event) => {
    pagination.skip = event.page;
    pagination.limit = event.rows;

    router.push({
        name: route.name,
        query: {
            page: pagination.skip + 1,
            size: pagination.limit
        }
    });
};

const handleFilter = () => {
    fetchPromotions();
};

const handleClearFilter = () => {
    filterStore.resetFilters();
    fetchPromotions();
};

const handleShowApplicableObjects = (event, data) => {
    applicableObjectsData.data = data;
    applicableObjectsData.type = data[0]?.type === 'C' ? 'Danh sách khách hàng' : 'Danh sách nhóm';
    overlayPanelRef.value.toggle(event);
};

const handleEditPromotion = (id) => {
    router.push({
        name: 'new_promotion_detail',
        params: { id }
    });
};

// ============================================================================
// Debounced Functions
// ============================================================================

const debouncedFilter = debounce(() => {
    handleFilter();
}, 1000);

// ============================================================================
// Watchers
// ============================================================================

watch(route, async () => {
    await fetchPromotions();
});

// ============================================================================
// Lifecycle Hooks
// ============================================================================

const initializeComponent = async () => {
    if (route.query.page) {
        const page = parseInt(route.query.page);
        pagination.skip = page > 0 ? page - 1 : 0;
    }

    if (route.query.size) {
        pagination.limit = parseInt(route.query.size);
    }

    await fetchPromotions();
};

onBeforeMount(async () => {
    await initializeComponent();
});
</script>

<style scoped>
.p-10px {
    padding: 10px 0px;
    margin: 0px;
}

.form-group {
    padding: 10px 0px;
    margin: 0px;
    border-bottom: 1px solid var(--surface-200);
}

.form-group span:first-child {
    padding: 0px;
}

.form-group span:last-child {
    padding: 0px;
}
</style>
