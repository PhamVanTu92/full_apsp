<template>
  <div class="create-independent-container">
    <div class="flex align-items-center gap-3 mb-4">
      <Button icon="pi pi-arrow-left" rounded text severity="secondary" class="bg-white shadow-1" @click="goBack" />
      <h3 class="m-0 font-bold text-900">{{ t('body.ReturnRequestList.Independent.title') }}</h3>
    </div>

    <div class="card p-4 border-round-xl shadow-1 mb-4 bg-white">
      <div class="flex flex-column gap-2">
        <label for="customerName" class="text-xs font-bold text-700 uppercase">
          {{ t('body.ReturnRequestList.Independent.customerName') }}
        </label>
        <CustomerSelector
          v-model="selectedCustomer"
          width="100%"
          :placeholder="t('body.ReturnRequestList.Independent.customerPlaceholder')"
          @item-select="onSelectCustomer"
        />
      </div>
    </div>

    <div class="card p-3 mb-4">
      <div class="section-head">
        <h4 class="m-0 font-bold text-800">{{ t('body.ReturnRequestList.Independent.productList') }}</h4>
        <ProductSelector
          :label="t('body.ReturnRequestList.Independent.addProduct')"
          icon="pi pi-plus"
          severity="primary"
          outlined
          :customer="selectedCustomer"
          @confirm="onConfirmAddReturnProduct"
        />
      </div>

      <DataTable
        :value="returnProducts"
        v-model:selection="selectedReturnProducts"
        dataKey="itemCode"
        class="table-main p-datatable-sm custom-edit-table"
        responsiveLayout="scroll"
        showGridlines
        :scrollable="returnProducts.length > 5"
        :scrollHeight="returnProducts.length > 5 ? '22rem' : null"
      >
        <template #empty>
          <div class="py-4 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
        </template>
        <Column selectionMode="multiple" headerStyle="width: 3rem" style="width: 3rem"></Column>
        <Column field="itemName" :header="t('body.ReturnRequestList.Independent.productName')">
          <template #body="slotProps">
            <span class="font-medium">{{ slotProps.data.itemName }}</span>
          </template>
        </Column>
        <Column field="uomName" :header="t('body.ReturnRequestList.Independent.unit')" style="width: 12%">
          <template #body="slotProps">
            <span>{{ slotProps.data.uomName }}</span>
          </template>
        </Column>
        <Column field="price" :header="t('body.ReturnRequestList.Independent.price')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <InputNumber
              v-model="slotProps.data.price"
              :min="0"
              :useGrouping="true"
              :minFractionDigits="2"
              :maxFractionDigits="2"
              showButtons
              buttonLayout="stacked"
              incrementButtonIcon="pi pi-angle-up"
              decrementButtonIcon="pi pi-angle-down"
              class="w-full price-input"
            />
          </template>
        </Column>
        <Column field="quantity" :header="t('body.ReturnRequestList.Independent.quantity')" style="width: 12%">
          <template #body="slotProps">
            <InputNumber
              v-model="slotProps.data.quantity"
              :min="1"
              :useGrouping="true"
              :minFractionDigits="3"
              :maxFractionDigits="3"
              showButtons
              buttonLayout="stacked"
              class="w-full custom-number-input"
              incrementButtonIcon="pi pi-angle-up"
              decrementButtonIcon="pi pi-angle-down"
            />
          </template>
        </Column>
        <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <span class="font-bold">{{ formatMoney(slotProps.data.quantity * slotProps.data.price) }}</span>
          </template>
        </Column>
        <Column field="remarks" :header="t('body.ReturnRequestList.Independent.reason')" style="width: 25%">
          <template #body="slotProps">
            <InputText
              v-model="slotProps.data.remarks"
              :placeholder="t('body.ReturnRequestList.Independent.reasonPlaceholder')"
              class="w-full reason-input"
            />
          </template>
        </Column>
      </DataTable>

      <div class="section-actions">
        <Button
          v-if="selectedReturnProducts.length > 0"
          label="Xóa"
          severity="danger"
          @click="removeReturnSelected"
        />
      </div>
    </div>

    <div class="card p-3 mb-4">
      <div class="section-head">
        <h4 class="m-0 font-bold text-800">{{ t('body.ReturnRequestList.Independent.exchangeProductList') }}</h4>
        <ProductSelector
          :label="t('body.ReturnRequestList.Independent.addProduct')"
          icon="pi pi-plus"
          severity="primary"
          outlined
          :customer="selectedCustomer"
          @confirm="onConfirmAddExchangeProduct"
        />
      </div>

      <DataTable
        :value="exchangeProducts"
        v-model:selection="selectedExchangeProducts"
        dataKey="itemCode"
        class="table-main p-datatable-sm custom-edit-table"
        responsiveLayout="scroll"
        showGridlines
        :scrollable="exchangeProducts.length > 5"
        :scrollHeight="exchangeProducts.length > 5 ? '22rem' : null"
      >
        <template #empty>
          <div class="py-4 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
        </template>
        <Column selectionMode="multiple" headerStyle="width: 3rem" style="width: 3rem"></Column>
        <Column field="itemName" :header="t('body.ReturnRequestList.Independent.productName')">
          <template #body="slotProps">
            <span class="font-medium">{{ slotProps.data.itemName }}</span>
          </template>
        </Column>
        <Column field="uomName" :header="t('body.ReturnRequestList.Independent.unit')" style="width: 12%">
          <template #body="slotProps">
            <span>{{ slotProps.data.uomName }}</span>
          </template>
        </Column>
        <Column field="price" :header="t('body.ReturnRequestList.Independent.price')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <InputNumber
              v-model="slotProps.data.price"
              :min="0"
              :useGrouping="true"
              :minFractionDigits="2"
              :maxFractionDigits="2"
              showButtons
              buttonLayout="stacked"
              incrementButtonIcon="pi pi-angle-up"
              decrementButtonIcon="pi pi-angle-down"
              class="w-full price-input"
            />
          </template>
        </Column>
        <Column field="quantity" :header="t('body.ReturnRequestList.Independent.quantity')" style="width: 12%">
          <template #body="slotProps">
            <InputNumber
              v-model="slotProps.data.quantity"
              :min="1"
              :useGrouping="true"
              :minFractionDigits="3"
              :maxFractionDigits="3"
              showButtons
              buttonLayout="stacked"
              class="w-full custom-number-input"
              incrementButtonIcon="pi pi-angle-up"
              decrementButtonIcon="pi pi-angle-down"
            />
          </template>
        </Column>
        <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <span class="font-bold">{{ formatMoney(slotProps.data.quantity * slotProps.data.price) }}</span>
          </template>
        </Column>
        <Column field="remarks" :header="t('body.ReturnRequestList.Independent.reason')" style="width: 25%">
          <template #body="slotProps">
            <InputText
              v-model="slotProps.data.remarks"
              :placeholder="t('body.ReturnRequestList.Independent.reasonPlaceholder')"
              class="w-full reason-input"
            />
          </template>
        </Column>
      </DataTable>

      <div class="section-actions">
        <Button
          v-if="selectedExchangeProducts.length > 0"
          label="Xóa"
          severity="danger"
          @click="removeExchangeSelected"
        />
      </div>
    </div>

    <div class="summary-wrapper" v-if="returnProducts.length > 0 || exchangeProducts.length > 0">
      <div class="summary-card">
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.totalLines') }}</span>
          <b>{{ returnProducts.length + exchangeProducts.length }}</b>
        </div>
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.totalQuantity') }}</span>
          <b>{{ formatNumber(totalQuantity, 3) }}</b>
        </div>
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.totalValue') }}</span>
          <b>{{ formatMoney(totalValue) }}</b>
        </div>
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.exchangeValue') }}</span>
          <b>{{ formatMoney(exchangeValue) }}</b>
        </div>
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.returnValue') }}</span>
          <b>{{ formatMoney(returnValue) }}</b>
        </div>
        <div class="summary-row summary-row--settlement">
          <span>{{ settlementSummaryLabel }}</span>
          <b :class="settlementAmountClass">
            {{ formatSignedMoney(settlementAmount) }}
          </b>
        </div>
      </div>
    </div>
    <div class="summary-status" v-if="returnProducts.length > 0 || exchangeProducts.length > 0">
      <Tag
        :value="settlementDirectionLabel"
        :class="settlementDirectionTagClass"
      />
      <span class="summary-status__text">
        {{ settlementDirectionDescription }}
      </span>
    </div>

    <div class="flex justify-content-end gap-3 mb-5">
      <Button
        :label="t('body.ReturnRequestList.Independent.cancel')"
        severity="secondary"
        outlined
        class="px-5 bg-white border-300 text-700"
        @click="goBack"
      />
      <Button
        :label="t('body.ReturnRequestList.Independent.submit')"
        icon="pi pi-check-circle"
        severity="success"
        class="px-5 bg-green-600 border-none"
        @click="submitRequest"
        :loading="isSubmitting"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';

import PurchaseReturnService from '@/services/purchaseReturn.service';

const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const formatMoney = (value) => {
  if (value === null || value === undefined) return '--';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatNumber = (value, decimals = 0) => {
  if (value === null || value === undefined) return '--';
  return new Intl.NumberFormat('vi-VN', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals
  }).format(value);
};

const formatSignedMoney = (value) => {
  if (value === null || value === undefined) return '--';
  const prefix = value > 0 ? '+' : '';
  const formatted = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  return prefix + formatted;
};

const normalizeMoneyValue = (val) => {
  return Math.round(val);
};

const selectedCustomer = ref(null);
const isSubmitting = ref(false);
const returnProducts = ref([]);
const exchangeProducts = ref([]);
const selectedReturnProducts = ref([]);
const selectedExchangeProducts = ref([]);

const onSelectCustomer = (customer) => {
  if (selectedCustomer.value?.id && selectedCustomer.value.id !== customer?.id) {
    returnProducts.value = [];
    exchangeProducts.value = [];
    selectedReturnProducts.value = [];
    selectedExchangeProducts.value = [];
  }
  selectedCustomer.value = customer;
};

const mapSelectedProduct = (product) => {
  return {
    itemId: product.id,
    itemCode: product.itemCode,
    itemName: product.itemName,
    uomCode: product.ougp?.ouom?.uomCode || product.uomCode || '',
    uomName: product.ougp?.ouom?.uomName || product.uomName || product.packing?.name || '',
    quantity: product.quantity || 1,
    price: Number(product.price) || 0,
    currency: product.currency || '',
    remarks: ''
  };
};

const addProductsToList = (selectedProducts, targetList) => {
  selectedProducts.forEach((product) => {
    const exists = targetList.value.find((item) => item.itemCode === product.itemCode);
    if (!exists) {
      targetList.value.push(mapSelectedProduct(product));
    }
  });
};

const onConfirmAddReturnProduct = (selectedProducts) => {
  addProductsToList(selectedProducts, returnProducts);
};

const onConfirmAddExchangeProduct = (selectedProducts) => {
  addProductsToList(selectedProducts, exchangeProducts);
};

const removeReturnSelected = () => {
  const items = selectedReturnProducts.value ?? [];
  const selectedCodes = new Set(items.map((item) => item.itemCode));
  returnProducts.value = (returnProducts.value ?? []).filter((item) => !selectedCodes.has(item.itemCode));
  selectedReturnProducts.value = [];
};

const removeExchangeSelected = () => {
  const items = selectedExchangeProducts.value ?? [];
  const selectedCodes = new Set(items.map((item) => item.itemCode));
  exchangeProducts.value = (exchangeProducts.value ?? []).filter((item) => !selectedCodes.has(item.itemCode));
  selectedExchangeProducts.value = [];
};

const goBack = () => {
  router.push({ name: 'returnRequestList' });
};

const isProductListValid = (items) => {
  return items.every((product) => {
    return product.itemName && product.uomName && product.remarks && product.remarks.trim() && Number(product.quantity) > 0;
  });
};

const mapItemDetails = (items, type) => {
  return items.map((product) => ({
    itemId: product.itemId,
    type,
    itemCode: product.itemCode,
    itemName: product.itemName,
    uomCode: product.uomCode,
    uomName: product.uomName,
    quantity: Number(product.quantity) || 0,
    price: Number(product.price) || 0,
    remarks: product.remarks.trim()
  }));
};

const submitRequest = async () => {
  if (!selectedCustomer.value) {
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Vui lòng chọn khách hàng', life: 3000 });
    return;
  }

  if (returnProducts.value.length === 0 && exchangeProducts.value.length === 0) {
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Vui lòng thêm ít nhất một sản phẩm', life: 3000 });
    return;
  }

  if (!isProductListValid(returnProducts.value) || !isProductListValid(exchangeProducts.value)) {
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Vui lòng nhập đầy đủ lý do cho tất cả sản phẩm', life: 3000 });
    return;
  }

  const payload = {
    docType: selectedCustomer.value.docType,
    cardName: selectedCustomer.value.cardName,
    cardCode: selectedCustomer.value.cardCode,
    cardId: selectedCustomer.value.id,
    refInvoiceCode: null,
    docDate: new Date().toISOString(),
    itemDetails: [
      ...mapItemDetails(returnProducts.value, 'I'),
      ...mapItemDetails(exchangeProducts.value, 'O')
    ]
  };

  isSubmitting.value = true;
  try {
    await PurchaseReturnService.add(payload);
    toast.add({ severity: 'success', summary: 'Thành công', detail: 'Tạo yêu cầu trả hàng thành công', life: 3000 });
    setTimeout(() => {
      router.push({ name: 'returnRequestList' });
    }, 1000);
  } catch (error) {
    console.error('Error submitting return request:', error);
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tạo yêu cầu trả hàng. Vui lòng thử lại sau.', life: 3000 });
  } finally {
    isSubmitting.value = false;
  }
};

const totalQuantity = computed(() => {
  const returnQty = returnProducts.value.reduce((sum, item) => sum + (Number(item.quantity) || 0), 0);
  const exchangeQty = exchangeProducts.value.reduce((sum, item) => sum + (Number(item.quantity) || 0), 0);
  return returnQty + exchangeQty;
});

const totalValue = computed(() => {
  const returnVal = returnProducts.value.reduce((sum, item) => sum + ((Number(item.quantity) || 0) * (Number(item.price) || 0)), 0);
  const exchangeVal = exchangeProducts.value.reduce((sum, item) => sum + ((Number(item.quantity) || 0) * (Number(item.price) || 0)), 0);
  return returnVal + exchangeVal;
});

const exchangeValue = computed(() => exchangeProducts.value.reduce((sum, item) => sum + ((Number(item.quantity) || 0) * (Number(item.price) || 0)), 0));
const returnValue = computed(() => returnProducts.value.reduce((sum, item) => sum + ((Number(item.quantity) || 0) * (Number(item.price) || 0)), 0));
const settlementAmount = computed(() => normalizeMoneyValue(exchangeValue.value - returnValue.value));

const settlementSummaryLabel = computed(() => {
  if (settlementAmount.value > 0) return t('body.ReturnRequestList.Detail.customerTransferAmount');
  if (settlementAmount.value < 0) return t('body.ReturnRequestList.Detail.companyRefundAmount');
  return t('body.ReturnRequestList.Detail.noSettlementAmount');
});

const settlementDirectionLabel = computed(() => {
  if (settlementAmount.value > 0) return t('body.ReturnRequestList.Detail.customerTransferTag');
  if (settlementAmount.value < 0) return t('body.ReturnRequestList.Detail.companyRefundTag');
  return t('body.ReturnRequestList.Detail.noSettlementTag');
});

const settlementDirectionDescription = computed(() => {
  if (settlementAmount.value > 0) return t('body.ReturnRequestList.Detail.customerTransferNote');
  if (settlementAmount.value < 0) return t('body.ReturnRequestList.Detail.companyRefundNote');
  return t('body.ReturnRequestList.Detail.noSettlementNote');
});

const settlementAmountClass = computed(() => {
  if (settlementAmount.value > 0) return 'text-green-700';
  if (settlementAmount.value < 0) return 'text-orange-600';
  return 'text-700';
});

const settlementDirectionTagClass = computed(() => {
  if (settlementAmount.value > 0) return 'bg-green-100 text-green-700 border-none';
  if (settlementAmount.value < 0) return 'bg-orange-100 text-orange-700 border-none';
  return 'bg-gray-100 text-gray-700 border-none';
});

</script>

<style scoped lang="scss">
.create-independent-container {
  padding: 1rem 0;
}

.section-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.25rem 0 0.75rem;
}

.section-actions {
  display: flex;
  justify-content: flex-start;
  margin-top: 0.75rem;
}

:deep(.custom-edit-table .p-datatable-thead > tr > th) {
  background-color: #f8fafc;
  color: #4b5563;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  border-color: #d8e0ea;
  padding: 0.7rem 0.8rem;
}

:deep(.custom-edit-table .p-datatable-tbody > tr > td) {
  border-color: #d8e0ea;
  padding: 0.6rem 0.8rem;
  background-color: #fff;
}

:deep(.custom-edit-table .p-datatable-wrapper) {
  border: 1px solid #d8e0ea;
  border-radius: 6px;
}

:deep(.custom-number-input .p-inputnumber-input),
:deep(.price-input .p-inputnumber-input) {
  text-align: right;
  padding-right: 2.2rem;
}

:deep(.reason-input) {
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  padding: 0.5rem 0.65rem;
}

.summary-wrapper {
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid #e2e8f0;
  padding: 0.9rem 1rem;
}

.summary-card {
  width: 100%;
  max-width: 26rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 0.7rem 0.9rem;
  background: #f8fafc;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.35rem 0;
  color: #334155;

  &:not(:last-child) {
    border-bottom: 1px solid #e2e8f0;
  }
}

.summary-row--settlement {
  font-weight: 700;
}

.summary-status {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 0.75rem;
  padding: 0 1rem 1rem;
  color: #475569;
}

.summary-status__text {
  max-width: 26rem;
  font-size: 0.92rem;
  text-align: right;
}

@media (max-width: 768px) {
  .summary-status {
    align-items: flex-end;
    flex-direction: column;
  }

  .summary-status__text {
    max-width: 100%;
  }
}
</style>
