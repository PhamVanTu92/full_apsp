<template>
  <div class="select-order-container">
    <div class="flex align-items-center gap-3 mb-4">
      <Button icon="pi pi-arrow-left" rounded text severity="secondary" class="bg-white shadow-1" @click="goBack" />
      <h3 class="m-0 font-bold text-900">{{ t('body.ReturnRequestList.SelectOrder.title') }}</h3>
    </div>

    <div class="card p-3 border-round-xl shadow-1 overflow-hidden bg-white">
      <DataTable
        :value="orders"
        dataKey="id"
        responsiveLayout="scroll"
        class="p-datatable-sm select-order-table"
        tableStyle="min-width: 50rem"
        :rowHover="true"
        :loading="loading"
        :first="(page - 1) * pageSize"
        :rows="pageSize"
        :totalRecords="total"
        paginator
        lazy
        stripedRows
        :scrollable="isTableScrollable"
        :scrollHeight="tableScrollHeight"
        :rowsPerPageOptions="rowsPerPageOptions"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :currentPageReportTemplate="pageReportTemplate"
        showGridlines
        @page="onPageChange($event)"
        @row-click="onRowClick"
      >
        <template #header>
          <div class="flex justify-content-between align-items-center gap-2">
            <IconField iconPosition="left" class="header-search">
              <InputText
                v-model="searchQuery"
                :placeholder="t('body.OrderList.searchPlaceholder')"
                @input="onSearch"
              />
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
            </IconField>
            <div class="flex gap-2">
              <Button
                type="button"
                icon="pi pi-filter-slash"
                :label="t('body.OrderApproval.clear')"
                outlined
                severity="success"
                @click="clearFilter"
              />
              <Button
                type="button"
                icon="pi pi-file-export"
                severity="secondary"
                label="Export"
                @click="exportOrders"
              />
            </div>
          </div>
        </template>

        <template #empty>
          <div class="py-5 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
        </template>

        <Column field="invoiceCode" :header="t('body.OrderList.orderCode')" style="width: 22%">
          <template #body="slotProps">
            <div class="flex align-items-center gap-2">
              <span class="invoice-code">{{ slotProps.data.invoiceCode }}</span>
              <Tag :value="t('body.ReturnRequestList.SelectOrder.completedTag')" class="tag-completed" />
            </div>
          </template>
        </Column>

        <Column field="cardName" :header="t('body.OrderList.customer')" style="width: 34%">
          <template #body="slotProps">
            <span class="text-900 font-bold">{{ slotProps.data.cardName }}</span>
          </template>
        </Column>

        <Column field="docDate" :header="t('body.OrderList.time')" style="width: 24%" class="w-14rem">
          <template #body="slotProps">
            <span class="border-1 border-300 py-1">
              <span class="surface-300 px-3 p-1">{{ getTime(slotProps.data.docDate) }}</span>
              <span class="px-3 p-1">{{ getDate(slotProps.data.docDate) }}</span>
            </span>
          </template>
        </Column>

        <Column header="Sản phẩm" style="width: 14%">
          <template #body="slotProps">
            <div class="text-right">
              <div class="item-count">{{ getItemCount(slotProps.data) }} mặt hàng</div>
            </div>
          </template>
        </Column>

      </DataTable>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PurchaseReturnService from '@/services/purchaseReturn.service';
import format from '@/helpers/format.helper';
import { exportExcelFile } from '@/helpers/exportFile.helper';
import { debounce } from 'lodash';
import { useToast } from 'primevue/usetoast';

const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const searchQuery = ref('');
const orders = ref([]);
const loading = ref(false);
const page = ref(1);
const pageSize = ref(10);
const total = ref(0);
const MAX_VISIBLE_ROWS = 20;
const TABLE_ROW_HEIGHT_PX = 56;
const rowsPerPageOptions = computed(() => Array.from({ length: 10 }, (_, i) => (i + 1) * 10));
const isTableScrollable = computed(() => pageSize.value > MAX_VISIBLE_ROWS);
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

const fetchOrders = async () => {
  loading.value = true;
  try {
    let filterParam = '';
    if (searchQuery.value) {
      filterParam = `&Filter=invoiceCode=${searchQuery.value}`;
    }
    const params = `Page=${page.value}&PageSize=${pageSize.value}${filterParam}`;
    const data = await PurchaseReturnService.getOrder(params);
    orders.value = data.item || data.items || [];
    total.value = data.total || 0;
  } catch (error) {
    console.error('Error fetching orders:', error);
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tải danh sách đơn hàng', life: 3000 });
  } finally {
    loading.value = false;
  }
};

const onPageChange = (event) => {
  page.value = event.page + 1;
  pageSize.value = event.rows;
  fetchOrders();
};

const getItemCount = (order) => {
  return (order?.itemDetails || order?.itemDetail || []).length;
};

const getTime = (docDate) => (docDate ? format.DateTime(docDate).time : '');
const getDate = (docDate) => (docDate ? format.DateTime(docDate).date : '');

const onRowClick = (event) => {
  if (!event?.data) return;
  selectOrder(event.data);
};

const clearFilter = () => {
  searchQuery.value = '';
  page.value = 1;
  fetchOrders();
};

const exportOrders = async () => {
  try {
    if (!orders.value.length) {
      toast.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Không có dữ liệu để xuất.', life: 3000 });
      return;
    }

    const rows = orders.value.map((order, index) => ({
      stt: index + 1,
      invoiceCode: order.invoiceCode || '',
      cardName: order.cardName || '',
      docDate: `${getTime(order.docDate)} ${getDate(order.docDate)}`.trim(),
      itemCount: `${getItemCount(order)} mặt hàng`
    }));

    const columns = [
      { header: 'STT', key: 'stt', width: 10 },
      { header: t('body.OrderList.orderCode'), key: 'invoiceCode', width: 22 },
      { header: t('body.OrderList.customer'), key: 'cardName', width: 42 },
      { header: t('body.OrderList.time'), key: 'docDate', width: 22 },
      { header: 'Sản phẩm', key: 'itemCount', width: 18 }
    ];

    await exportExcelFile({
      fileName: `Danh-sach-don-hang-hoan-thanh-${format.DateTime(new Date()).dateTimeFile}.xlsx`,
      sheetName: 'Đơn hàng hoàn thành',
      columns,
      data: rows
    });
  } catch (error) {
    console.error('Error exporting orders:', error);
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể xuất dữ liệu.', life: 3000 });
  }
};

const goBack = () => {
  router.push({ name: 'clientReturnRequest' });
};

const onSearch = debounce(() => {
  page.value = 1;
  fetchOrders();
}, 500);

const selectOrder = (order) => {
  router.push({ 
    name: 'client-return-request-from-order', 
    params: { id: order.id } 
  });
};

onMounted(() => {
  fetchOrders();
});
</script>

<style scoped lang="scss">
.select-order-container {
  padding: 1rem 0;
}

:deep(.select-order-table .p-datatable-header) {
  background: #fff;
  border: none;
  padding: 0 0 0.9rem 0;
}

.invoice-code {
  color: #2563eb;
  font-weight: 700;
}

.tag-completed {
  background-color: #ecfdf5 !important;
  color: #065f46 !important;
  padding: 0.175rem 0.5rem !important;
  font-weight: 700 !important;
  font-size: 0.675rem !important;
}

.item-count {
  font-weight: 700;
  color: #475569;
}

:deep(.select-order-table .p-datatable-thead > tr > th) {
  background-color: #f8fafc;
  color: #64748b;
  font-size: 0.95rem;
  font-weight: 700;
  border-color: #e2e8f0;
  padding: 1.05rem 0.8rem;
}

:deep(.select-order-table .p-datatable-tbody > tr > td) {
  border-color: #e2e8f0;
  padding: 1.05rem 0.8rem;
  background-color: #fff;
}

:deep(.select-order-table .p-datatable-wrapper) {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
}

:deep(.select-order-table .p-datatable-tbody > tr) {
  cursor: pointer;
}

:deep(.select-order-table .p-datatable-tbody > tr:hover > td) {
  background: #fbfdff !important;
}

:deep(.header-search .p-inputtext) {
  width: 16rem;
  padding-left: 2.6rem !important;
}
</style>
