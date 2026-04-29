<script setup>
import { onBeforeMount, ref, inject } from 'vue';
import API from '@/api/api-main';
import format from '@/helpers/format.helper';
import { exportExcelFile } from '@/helpers/exportFile.helper';
import { useRouter, useRoute } from 'vue-router';
import { FilterMatchMode } from 'primevue/api';
import { FilterStore } from '@/Pinia/Filter/FilterStorePurchaseOrderVPKM';
import { debounce } from 'lodash';
import { useI18n } from 'vue-i18n';
import { getStatuses, getLabels } from '@/components/Status';

const { t } = useI18n();
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
const statuses = ref(getStatuses(t));
const labels = getLabels(t);
const filterStore = FilterStore();
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
const fetchOrderData = async (queryFilter = null) => {
    loading.value = true;
    try {
        const filters = conditionHandler.getQuery(filterStore.filters);
        const queryParams = `?skip=${dataTable.value.skip}&limit=${dataTable.value.limit}${queryFilter ? `&filter=${queryFilter}` : filters}`;
        const { data } = await API.get(`PurchaseOrderVPKM${queryParams}`);
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
const CustomCurrency = (data) => {
    const total = format.FormatCurrency((data?.paymentInfo?.totalAfterVat || 0) - (data?.paymentInfo?.bonusAmount || 0));
    return `${total} ${data?.currency}`;
};
const exportExcel = async () => {
    loading.value = true;
    try { 
        let queryFilter = route.query.filter;

        const filters = conditionHandler.getQuery(filterStore.filters);

        const queryParams = `?skip=0&limit=${dataTable.value.total}${queryFilter ? `&filter=${queryFilter}` : filters}`;
        const { data } = await API.get(`PurchaseOrderVPKM${queryParams}`);
        const statusLabelRevert = getLabels(t);
        const rows = data.items.map((order, index) => ({
            stt: index + 1,
            invoiceCode: order.invoiceCode,
            cardName: order.cardName,
            status: statusLabelRevert[order.status]?.label || order.status,
            total: CustomCurrency(order),
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
            fileName: `Danh-sach-don-hang-tang-vpkm-${format.DateTime(new Date()).dateTimeFile}.xlsx`,
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
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <h4 class="font-bold m-0">{{ t('PromotionalItems.PromotionalItems.title') }}</h4>
        <router-link to="/gift-request/detail">
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
            tableStyle="min-width: 50rem;"
            header="surface-200"
            paginator
            lazy
            :first="dataTable.limit * dataTable.skip"
            :rows="dataTable.limit"
            :page="dataTable.skip"
            :totalRecords="dataTable.total"
            @page="onPageChange($event)"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
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
                    <router-link class="text-primary hover:underline font-semibold" :to="'/purchase-order/' + slotProps.data.id">
                        <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                        <span class="">{{ slotProps.data.invoiceCode }}</span>
                    </router-link>
                </template>
            </Column>
            <Column field="cardName" :header="t('body.OrderList.customer')" style="min-width: 30rem">
                <template #body="sp">
                    <router-link class="text-primary hover:underline font-semibold cursor-pointer" :to="'/agen-man/agency-category/' + sp.data.cardId">
                        <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                        <span>{{ sp.data.cardName }}</span></router-link
                    >
                </template>
            </Column>
            <Column field="status" :header="t('body.OrderList.orderStatus')" style="width: 15rem" :showFilterMatchModes="false">
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
    <Loading v-if="loading" />
</template>
