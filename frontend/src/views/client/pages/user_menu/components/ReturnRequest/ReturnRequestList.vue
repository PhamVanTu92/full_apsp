<template>
  <div class="return-request-container">
    <!-- Header -->
    <div class="flex justify-content-between align-items-center mb-3">
      <div>
        <h3 class="m-0 font-bold text-900">{{ t('body.ReturnRequestList.title') }}</h3>
        <p class="return-subtitle m-0 mt-1">{{ t('body.ReturnRequestList.subtitle') }}</p>
      </div>
      <Button 
        :label="t('body.ReturnRequestList.createNew')" 
        icon="pi pi-plus" 
        severity="success" 
        class="bg-green-600 border-none px-4"
        @click="handleCreateNew"
      />
    </div>

    <!-- Table -->
    <div class="card p-3 border-round-xl shadow-1 overflow-hidden bg-white">
      <DataTable
        stripedRows
        showGridlines
        :value="returns"
        :paginator="true"
        :rows="dataTable.limit"
        :totalRecords="dataTable.total"
        :lazy="true"
        @page="onPage($event)"
        :first="dataTable.skip * dataTable.limit"
        dataKey="id"
        :rowHover="true"
        :scrollable="isTableScrollable"
        :scrollHeight="tableScrollHeight"
        filterDisplay="menu"
        v-model:filters="filters"
        @filter="onFilter"
        :filterLocale="'vi'"
        responsiveLayout="scroll"
        class="p-datatable-sm return-table"
        tableStyle="min-width: 50rem"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :currentPageReportTemplate="pageReportTemplate"
        :loading="loading"
      >
        <template #header>
          <div class="flex justify-content-between align-items-center gap-2">
            <IconField iconPosition="left" class="header-search">
              <InputText
                :placeholder="t('body.ReturnRequestList.searchPlaceholder')"
                v-model="searchQuery"
                @input="debouncedSearch"
              />
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
            </IconField>
            <Button
              type="button"
              icon="pi pi-filter-slash"
              :label="t('body.OrderApproval.clear')"
              outlined
              severity="success"
              @click="clearFilter()"
            />
          </div>
        </template>

        <template #empty>
          <div class="py-5 my-5 text-center text-500 font-italic">
            {{ t('body.ReturnRequestList.noData') }}
          </div>
        </template>

        <!-- Mã yêu cầu -->
        <Column field="invoiceCode" :header="t('body.ReturnRequestList.requestCode')" style="width: 16rem">
          <template #body="slotProps">
            <div class="flex flex-column">
              <span class="text-primary font-semibold">{{ slotProps.data.invoiceCode }}</span>
              <span v-if="slotProps.data.refInvoiceCode" class="text-xs text-500 mt-1">Đơn: {{ slotProps.data.refInvoiceCode }}</span>
            </div>
          </template>
        </Column>

        <!-- Khách hàng -->
        <Column field="cardName" :header="t('body.ReturnRequestList.customer')" style="min-width: 22rem">
          <template #body="slotProps">
            <span class="text-800 font-medium">{{ slotProps.data.cardName }}</span>
          </template>
        </Column>

        <!-- Ngày yêu cầu -->
        <Column field="docDate" :header="t('body.ReturnRequestList.requestDate')" class="w-14rem">
          <template #body="slotProps">
            <span class="border-1 border-300 py-1">
              <span class="surface-300 px-3 p-1">{{ formatDateInfo(slotProps.data.docDate).time }}</span>
              <span class="px-3 p-1">{{ formatDateInfo(slotProps.data.docDate).date }}</span>
            </span>
          </template>
        </Column>

        <!-- Loại -->
        <Column
          field="objType"
          :header="t('body.ReturnRequestList.type')"
          class="w-10rem"
          :showFilterMatchModes="false"
        >
          <template #body="slotProps">
            <Tag 
              :value="getTypeInfo(slotProps.data.objType, slotProps.data.refInvoiceCode).label"
              :class="getTypeInfo(slotProps.data.objType, slotProps.data.refInvoiceCode).class"
            />
          </template>
          <template #filter="{ filterModel }">
            <MultiSelect
              v-model="filterModel.value"
              :options="typeOptions"
              optionLabel="name"
              optionValue="code"
              :placeholder="t('body.ReturnRequestList.type')"
              class="p-column-filter"
              showClear
            >
              <template #option="slotProps">
                <Tag
                  :value="getTypeInfo(slotProps.option.code).label"
                  :class="getTypeInfo(slotProps.option.code).class"
                />
              </template>
            </MultiSelect>
          </template>
        </Column>

        <!-- Trạng thái -->
        <Column
          field="status"
          :header="t('body.ReturnRequestList.status')"
          class="w-10rem"
          :showFilterMatchModes="false"
        >
          <template #body="slotProps">
            <Tag 
              :value="getStatusInfo(slotProps.data.status).label"
              :class="getStatusInfo(slotProps.data.status).class"
            />
          </template>
          <template #filter="{ filterModel }">
            <MultiSelect
              v-model="filterModel.value"
              :options="statusOptions"
              optionLabel="name"
              optionValue="code"
              :placeholder="t('body.ReturnRequestList.status')"
              class="p-column-filter"
              showClear
            >
              <template #option="slotProps">
                <Tag
                  :value="getStatusInfo(slotProps.option.code).label"
                  :class="getStatusInfo(slotProps.option.code).class"
                />
              </template>
            </MultiSelect>
          </template>
        </Column>

        <!-- Hành động -->
        <Column :header="t('body.ReturnRequestList.actions')" class="text-center w-8rem">
          <template #body="slotProps">
            <div class="flex justify-content-center gap-2">
              <Button 
                icon="pi pi-eye" 
                rounded 
                text 
                severity="secondary" 
                class="hover:surface-100" 
                v-tooltip.top="t('body.OrderList.viewDetails')"
                @click="viewDetail(slotProps.data)"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- Create Request Dialog -->
    <Dialog 
      v-model:visible="displayCreateDialog" 
      :header="t('body.ReturnRequestList.dialog.title')" 
      :modal="true" 
      :style="{ width: '650px' }" 
      :breakpoints="{ '960px': '75vw', '641px': '100vw' }"
      class="create-return-dialog"
    >
      <div class="flex flex-column md:flex-row gap-4 py-4 px-2">
        <!-- Independent Option -->
        <div 
          class="option-card flex-1 p-4 border-1 border-200 border-round-xl cursor-pointer hover:border-primary transition-colors flex flex-column align-items-center text-center gap-3 bg-white shadow-hover"
          @click="selectType('independent')"
        >
          <div class="icon-wrapper purple-theme flex align-items-center justify-content-center border-round-circle">
            <i class="pi pi-file-plus text-2xl"></i>
          </div>
          <div>
            <h4 class="m-0 text-900 font-bold mb-2">{{ t('body.ReturnRequestList.dialog.independentTitle') }}</h4>
            <p class="m-0 text-600 text-sm line-height-3">{{ t('body.ReturnRequestList.dialog.independentDesc') }}</p>
          </div>
        </div>

        <!-- From Order Option -->
        <div 
          class="option-card flex-1 p-4 border-1 border-200 border-round-xl cursor-pointer hover:border-primary transition-colors flex flex-column align-items-center text-center gap-3 bg-white shadow-hover"
          @click="selectType('from_order')"
        >
          <div class="icon-wrapper blue-theme flex align-items-center justify-content-center border-round-circle">
            <i class="pi pi-shopping-bag text-2xl"></i>
          </div>
          <div>
            <h4 class="m-0 text-900 font-bold mb-2">{{ t('body.ReturnRequestList.dialog.fromOrderTitle') }}</h4>
            <p class="m-0 text-600 text-sm line-height-3">{{ t('body.ReturnRequestList.dialog.fromOrderDesc') }}</p>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-content-end border-top-1 border-100 pt-3 mt-2">
          <Button 
            :label="t('body.ReturnRequestList.dialog.close')" 
            text 
            severity="secondary" 
            class="px-4 font-semibold text-700"
            @click="displayCreateDialog = false"
          />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { computed, inject, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PurchaseReturnService from '@/services/purchaseReturn.service';
import { getLabels } from '@/components/Status';
import { FilterMatchMode } from 'primevue/api';
import { debounce } from 'lodash';
import format from '@/helpers/format.helper';

const router = useRouter();
const { t } = useI18n();
const conditionHandler = inject('conditionHandler');

const searchQuery = ref('');
const displayCreateDialog = ref(false);
const loading = ref(false);
const returns = ref([]);
const filters = ref({
  objType: { value: null, matchMode: FilterMatchMode.EQUALS },
  status: { value: null, matchMode: FilterMatchMode.EQUALS }
});
const dataTable = ref({
  limit: 10,
  skip: 0,
  total: 0
});
const MAX_VISIBLE_ROWS = 20;
const TABLE_ROW_HEIGHT_PX = 56;
const isTableScrollable = computed(() => dataTable.value.limit > MAX_VISIBLE_ROWS);
const tableScrollHeight = computed(() => (
  isTableScrollable.value ? `${MAX_VISIBLE_ROWS * TABLE_ROW_HEIGHT_PX}px` : null
));
const pageReportTemplate = computed(() =>
  t('body.OrderApproval.currentPageReportTemplate', {
    first: '{first}',
    last: '{last}',
    totalRecords: '{totalRecords}'
  })
);
const typeOptions = computed(() => [
  { code: 'I', name: t('body.ReturnRequestList.type_from_order') },
  { code: 'O', name: t('body.ReturnRequestList.type_independent') }
]);
const statusOptions = computed(() => {
  const commonStatusCodes = Object.keys(getLabels(t));
  const extraReturnRequestStatus = ['A', 'P', 'C'];
  const allStatusCodes = [...new Set([...commonStatusCodes, ...extraReturnRequestStatus])];
  return allStatusCodes.map((code) => ({
    code,
    name: getStatusInfo(code).label
  }));
});
const hasColumnFilter = computed(() => {
  return (
    Array.isArray(filters.value.objType.value) && filters.value.objType.value.length > 0
  ) || (
    Array.isArray(filters.value.status.value) && filters.value.status.value.length > 0
  );
});

const fetchReturns = async () => {
  loading.value = true;
  try {
    const filterQuery = hasColumnFilter.value && conditionHandler ? conditionHandler.getQuery(filters.value) : '';
    const params = `skip=${dataTable.value.skip}&limit=${dataTable.value.limit}${searchQuery.value ? '&search=' + encodeURIComponent(searchQuery.value.trim()) : ''}${filterQuery}`;
    const res = await PurchaseReturnService.getAllMe(params);
    returns.value = res.item || res.items || [];
    dataTable.value.total = res.total || 0;
  } catch (error) {
    console.error('Error fetching returns:', error);
  } finally {
    loading.value = false;
  }
};

const onPage = (event) => {
  dataTable.value.skip = event.page;
  dataTable.value.limit = event.rows;
  fetchReturns();
};
const onFilter = () => {
  dataTable.value.skip = 0;
  fetchReturns();
};

const onSearch = () => {
  dataTable.value.skip = 0;
  fetchReturns();
};

const debouncedSearch = debounce(onSearch, 700);

const clearFilter = () => {
  searchQuery.value = '';
  filters.value.objType.value = null;
  filters.value.status.value = null;
  dataTable.value.skip = 0;
  fetchReturns();
};

const handleCreateNew = () => {
  displayCreateDialog.value = true;
};

const selectType = (type) => {
  displayCreateDialog.value = false;
  if (type === 'independent') {
    router.push({ name: 'client-return-request-independent' });
  } else if (type === 'from_order') {
    router.push({ name: 'client-return-request-select-order' });
  }
};


const viewDetail = (item) => {
  if (!item?.id) return;
  router.push({ name: 'client-return-request-detail', params: { id: item.id } });
};

const formatDateInfo = (date) => {
  if (!date) return { time: '', date: '' };
  return format.DateTime(date);
};

const getStatusInfo = (status) => {
  const orderStatusLabels = getLabels(t);
  if (orderStatusLabels[status]) {
    return orderStatusLabels[status];
  }

  if (status === 'A') {
    return {
      label: t('body.ReturnRequestList.status_approved'),
      class: 'text-teal-700 bg-teal-200'
    };
  }

  if (status === 'P') {
    return {
      label: t('body.ReturnRequestList.status_pending'),
      class: 'text-yellow-700 bg-yellow-200'
    };
  }

  if (status === 'C') {
    return {
      label: t('body.status.HUY'),
      class: 'text-red-500 border-red-500 bg-white border-1'
    };
  }

  return {
    label: status || '',
    class: 'text-gray-700 bg-gray-100'
  };
};

const getTypeClass = (type) => {
  if (type === 'from_order') return 'bg-blue-100 text-blue-700 px-3 border-none';
  if (type === 'independent') return 'bg-purple-100 text-purple-700 px-3 border-none';
  return '';
};

const getTypeInfo = (objType, refInvoiceCode) => {
  const isFromOrder = objType === 'I' || !!refInvoiceCode || objType === 'from_order';
  if (isFromOrder) {
    return {
      label: t('body.ReturnRequestList.type_from_order'),
      class: getTypeClass('from_order')
    };
  }
  return {
    label: t('body.ReturnRequestList.type_independent'),
    class: getTypeClass('independent')
  };
};

onMounted(() => {
  fetchReturns();
});
</script>

<style scoped lang="scss">
.return-request-container {
  padding: 1rem 0;
}

:deep(.return-table .p-datatable-header) {
  background: #fff;
  border: none;
  padding: 0 0 0.9rem 0;
}

:deep(.return-table .p-datatable-thead > tr > th) {
  background-color: #f8fafc;
  color: #64748b;
  font-size: 0.95rem;
  font-weight: 700;
  border-color: #e2e8f0;
  padding: 0.9rem 0.8rem;
}

:deep(.return-table .p-datatable-tbody > tr > td) {
  border-color: #e2e8f0;
  padding: 0.9rem 0.8rem;
  background-color: #fff;
}

:deep(.return-table .p-datatable-wrapper) {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
}

:deep(.return-table .p-datatable-tbody > tr:hover > td) {
  background: #fbfdff !important;
}

:deep(.return-table .p-tag) {
  font-size: 0.75rem;
  font-weight: 700;
}

:deep(.header-search .p-inputtext) {
  width: 18rem;
  padding-left: 2.6rem !important;
}

.return-subtitle {
  color: #64748b;
  font-size: 0.875rem;
}

// Dialog styling
:deep(.create-return-dialog) {
  .p-dialog-header {
    border-bottom: 1px solid #f1f5f9;
    padding: 1.5rem;
    
    .p-dialog-title {
      font-weight: 700;
      color: #1e293b;
    }
  }

  .p-dialog-content {
    background-color: #f8fafc;
  }
  
  .p-dialog-footer {
    padding: 0.5rem 1.5rem 1rem;
    border: none;
  }
}

.option-card {
  height: 100%;
  border: 1px solid #e2e8f0;
  
  &:hover {
    border-color: #3b82f6;
    background-color: #f0f7ff;
    transform: translateY(-2px);
  }
}

.shadow-hover {
  transition: all 0.2s ease-in-out;
  &:hover {
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  }
}

.icon-wrapper {
  width: 64px;
  height: 64px;
  
  &.purple-theme {
    background-color: #f5f3ff;
    color: #7c3aed;
  }
  
  &.blue-theme {
    background-color: #eff6ff;
    color: #3b82f6;
  }
}
</style>
