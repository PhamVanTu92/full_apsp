<template>
  <div class="return-from-order-container">
    <div class="flex align-items-center gap-3 mb-4">
      <Button icon="pi pi-arrow-left" rounded text severity="secondary" class="bg-white shadow-1" @click="goBack" />
      <h3 class="m-0 font-bold text-900">
        {{ t('body.ReturnRequestList.FromOrder.returnTitle', { code: odStore.order?.invoiceCode || '...' }) }}
      </h3>
    </div>

    <div class="card p-4 border-round-xl shadow-1 mb-4 bg-white" v-if="odStore.customer || odStore.order">
      <div class="flex flex-column gap-1">
        <span class="text-xs font-bold text-500 uppercase">{{ t('body.ReturnRequestList.FromOrder.customerLabel') }}</span>
        <span class="text-xl font-bold text-800">{{ odStore.order?.cardName || '...' }}</span>
      </div>
    </div>

    <div class="card p-3 mb-4 border-round-xl shadow-1 bg-white">
      <div class="section-head">
        <h4 class="m-0 font-bold text-800">{{ t('body.ReturnRequestList.Independent.productList') }}</h4>
      </div>

      <DataTable
        :value="orderProducts"
        dataKey="id"
        class="table-main p-datatable-sm custom-return-table"
        responsiveLayout="scroll"
        showGridlines
        :scrollable="orderProducts.length > 5"
        :scrollHeight="orderProducts.length > 5 ? '22rem' : null"
      >
        <template #empty>
          <div class="py-4 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
        </template>

        <Column class="w-3rem">
          <template #header>
            <Checkbox v-model="selectAll" :binary="true" @change="onSelectAllChange" />
          </template>
          <template #body="slotProps">
            <Checkbox v-model="slotProps.data.selected" :binary="true" @change="onRowSelectionChange(slotProps.data)" />
          </template>
        </Column>

        <Column field="name" :header="t('body.ReturnRequestList.FromOrder.productName')">
          <template #body="slotProps">
            <span class="font-medium">{{ slotProps.data.name }}</span>
          </template>
        </Column>

        <Column field="unit" :header="t('body.ReturnRequestList.FromOrder.unit')" style="width: 12%">
          <template #body="slotProps">
            <span class="text-sm uppercase font-semibold">{{ slotProps.data.unit }}</span>
          </template>
        </Column>

        <Column field="openQty" :header="t('body.ReturnRequestList.FromOrder.openQty')" style="width: 10%">
          <template #body="slotProps">
            <span class="font-bold text-900">{{ slotProps.data.openQty }}</span>
          </template>
        </Column>

        <Column field="returnQty" :header="t('body.ReturnRequestList.FromOrder.returnQty')" style="width: 12%">
          <template #body="slotProps">
            <div class="flex flex-column gap-1">
              <InputNumber
                v-model="slotProps.data.returnQty"
                :min="1"
                :max="slotProps.data.openQty"
                inputClass="text-center w-full p-2 border-1 border-200 border-round"
                :class="{ 'p-invalid': slotProps.data.errors?.returnQty }"
                :disabled="!slotProps.data.selected"
                @input="validateReturnRow(slotProps.data)"
              />
              <small class="p-error" v-if="slotProps.data.errors?.returnQty">{{ slotProps.data.errors.returnQty }}</small>
            </div>
          </template>
        </Column>

        <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <span class="font-bold">{{ formatMoney(slotProps.data.returnQty * slotProps.data.price) }}</span>
          </template>
        </Column>

        <Column field="reason" :header="t('body.ReturnRequestList.FromOrder.returnReason')" style="width: 25%">
          <template #body="slotProps">
            <div class="flex flex-column gap-1">
              <InputText
                v-model="slotProps.data.reason"
                :placeholder="t('body.ReturnRequestList.FromOrder.reasonPlaceholder')"
                class="w-full border-1 border-200 border-round p-2"
                :class="{ 'p-invalid': slotProps.data.errors?.reason }"
                :disabled="!slotProps.data.selected"
                @input="validateReturnRow(slotProps.data)"
              />
              <small class="p-error" v-if="slotProps.data.errors?.reason">{{ slotProps.data.errors.reason }}</small>
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <div class="card p-3 mb-4 border-round-xl shadow-1 bg-white">
      <div class="section-head">
        <h4 class="m-0 font-bold text-800">{{ t('body.ReturnRequestList.Independent.exchangeProductList') }}</h4>
        <ProductSelector
          :label="t('body.ReturnRequestList.Independent.addProduct')"
          icon="pi pi-plus"
          severity="primary"
          outlined
          :customer="orderCustomer"
          @confirm="onConfirmAddExchangeProduct"
        />
      </div>

      <DataTable
        :value="exchangeProducts"
        v-model:selection="selectedExchangeProducts"
        dataKey="itemCode"
        class="table-main p-datatable-sm custom-return-table"
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
            <div class="flex flex-column gap-1">
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
                :class="{ 'p-invalid': slotProps.data.errors?.quantity }"
                @input="validateExchangeRow(slotProps.data)"
              />
              <small class="p-error" v-if="slotProps.data.errors?.quantity">{{ slotProps.data.errors.quantity }}</small>
            </div>
          </template>
        </Column>
        <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" style="width: 12%" class="text-right">
          <template #body="slotProps">
            <span class="font-bold">{{ formatMoney(slotProps.data.quantity * slotProps.data.price) }}</span>
          </template>
        </Column>
        <Column field="remarks" :header="t('body.ReturnRequestList.Independent.reason')" style="width: 25%">
          <template #body="slotProps">
            <div class="flex flex-column gap-1">
              <InputText
                v-model="slotProps.data.remarks"
                :placeholder="t('body.ReturnRequestList.Independent.reasonPlaceholder')"
                class="w-full reason-input"
                :class="{ 'p-invalid': slotProps.data.errors?.remarks }"
                @input="validateExchangeRow(slotProps.data)"
              />
              <small class="p-error" v-if="slotProps.data.errors?.remarks">{{ slotProps.data.errors.remarks }}</small>
            </div>
          </template>
        </Column>
      </DataTable>

      <div class="section-actions">
        <Button v-if="selectedExchangeProducts.length > 0" label="Xóa" severity="danger" @click="removeSelectedExchangeProducts" />
      </div>
    </div>

    <div class="summary-wrapper" v-if="canSubmit">
      <div class="summary-card">
        <div class="summary-row">
          <span>{{ t('body.ReturnRequestList.Detail.totalLines') }}</span>
          <b>{{ selectedLinesCount }}</b>
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
    <div class="summary-status" v-if="canSubmit">
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
        :label="t('body.ReturnRequestList.FromOrder.cancel')"
        severity="secondary"
        outlined
        class="px-5 bg-white border-300 text-700"
        @click="goBack"
      />
      <Button
        :label="t('body.ReturnRequestList.FromOrder.submit')"
        icon="pi pi-check-circle"
        severity="success"
        class="px-5 bg-green-600 border-none"
        @click="submitReturn"
        :loading="isSubmitting"
        :disabled="!canSubmit"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useOrderDetailStore } from '../store/orderDetail';
import { useToast } from 'primevue/usetoast';
import PurchaseReturnService from '@/services/purchaseReturn.service';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();
const odStore = useOrderDetailStore();

const isSubmitting = ref(false);
const orderProducts = ref([]);
const exchangeProducts = ref([]);
const selectedExchangeProducts = ref([]);
const selectAll = ref(false);

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

const selectedProductsCount = computed(() => orderProducts.value.filter((product) => product.selected).length);
const canSubmit = computed(() => selectedProductsCount.value > 0 || exchangeProducts.value.length > 0);

const selectedLinesCount = computed(() => {
  return selectedProductsCount.value + exchangeProducts.value.length;
});

const totalQuantity = computed(() => {
  const returnQty = orderProducts.value.filter(p => p.selected).reduce((sum, item) => sum + (Number(item.returnQty) || 0), 0);
  const exchangeQty = exchangeProducts.value.reduce((sum, item) => sum + (Number(item.quantity) || 0), 0);
  return returnQty + exchangeQty;
});

const exchangeValue = computed(() => exchangeProducts.value.reduce((sum, item) => sum + ((Number(item.quantity) || 0) * (Number(item.price) || 0)), 0));
const returnValue = computed(() => orderProducts.value.filter(p => p.selected).reduce((sum, item) => sum + ((Number(item.returnQty) || 0) * (Number(item.price) || 0)), 0));
const totalValue = computed(() => exchangeValue.value + returnValue.value);
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

const orderCustomer = computed(() => {
  if (!odStore.order?.cardId) return null;
  return {
    id: odStore.order.cardId,
    cardCode: odStore.order.cardCode,
    cardName: odStore.order.cardName
  };
});

const onSelectAllChange = () => {
  orderProducts.value.forEach((product) => {
    product.selected = selectAll.value;
    if (!selectAll.value) {
      product.returnQty = 0;
      product.reason = '';
      product.errors = {};
      return;
    }
    product.returnQty = 1;
  });
};

const onRowSelectionChange = (row) => {
  if (!row.selected) {
    row.returnQty = 0;
    row.reason = '';
    row.errors = {};
  } else {
    row.returnQty = 1;
  }
  selectAll.value = orderProducts.value.length > 0 && orderProducts.value.every((product) => product.selected);
};

const validateReturnRow = (row) => {
  if (!row.selected) return true;

  row.errors = {};
  let isValid = true;

  if (!row.returnQty || row.returnQty < 1) {
    row.errors.returnQty = 'SL phải lớn hơn 0';
    isValid = false;
  } else if (row.returnQty > row.openQty) {
    row.errors.returnQty = `Không vượt quá ${row.openQty}`;
    isValid = false;
  }

  if (!row.reason || row.reason.trim() === '') {
    row.errors.reason = 'Bắt buộc nhập lý do';
    isValid = false;
  }

  return isValid;
};

const mapSelectedExchangeProduct = (product) => {
  return {
    itemId: product.id,
    itemCode: product.itemCode,
    itemName: product.itemName,
    uomCode: product.ougp?.ouom?.uomCode || product.uomCode || '',
    uomName: product.ougp?.ouom?.uomName || product.uomName || product.packing?.name || '',
    quantity: Number(product.quantity) > 0 ? Number(product.quantity) : 1,
    price: Number(product.price) || 0,
    remarks: '',
    errors: {}
  };
};

const onConfirmAddExchangeProduct = (selectedProducts) => {
  selectedProducts.forEach((product) => {
    const exists = exchangeProducts.value.find((item) => item.itemCode === product.itemCode);
    if (!exists) {
      exchangeProducts.value.push(mapSelectedExchangeProduct(product));
    }
  });
};

const removeSelectedExchangeProducts = () => {
  const selectedCodes = new Set(selectedExchangeProducts.value.map((item) => item.itemCode));
  exchangeProducts.value = exchangeProducts.value.filter((item) => !selectedCodes.has(item.itemCode));
  selectedExchangeProducts.value = [];
};

const validateExchangeRow = (row) => {
  row.errors = {};
  let isValid = true;

  if (!row.quantity || Number(row.quantity) <= 0) {
    row.errors.quantity = 'SL phải lớn hơn 0';
    isValid = false;
  }

  if (!row.remarks || row.remarks.trim() === '') {
    row.errors.remarks = 'Bắt buộc nhập lý do';
    isValid = false;
  }

  return isValid;
};

const goBack = () => {
  router.push({ name: 'client-return-request-select-order' });
};

const submitReturn = async () => {
  let isFormValid = true;
  const itemsToReturn = [];
  const itemsToExchange = [];

  orderProducts.value.forEach((row) => {
    if (!row.selected) return;
    if (!validateReturnRow(row)) {
      isFormValid = false;
      return;
    }

    itemsToReturn.push({
      baseId: row.baseId,
      baseLineId: row.baseLineId,
      itemId: row.productId,
      type: 'I',
      itemCode: row.productCode,
      itemName: row.name,
      uomCode: row.uomCode || '',
      uomName: row.unit,
      quantity: row.returnQty,
      price: row.price || 0,
      remarks: row.reason.trim()
    });
  });

  exchangeProducts.value.forEach((row) => {
    if (!validateExchangeRow(row)) {
      isFormValid = false;
      return;
    }

    itemsToExchange.push({
      itemId: row.itemId,
      itemCode: row.itemCode,
      itemName: row.itemName,
      type: 'O',
      quantity: Number(row.quantity) || 0,
      price: Number(row.price) || 0,
      uomCode: row.uomCode || '',
      uomName: row.uomName,
      remarks: row.remarks.trim()
    });
  });

  if (!isFormValid) {
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Vui lòng kiểm tra lại các trường bị lỗi.', life: 3000 });
    return;
  }

  if (itemsToReturn.length === 0 && itemsToExchange.length === 0) {
    toast.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Bạn cần chọn ít nhất 1 sản phẩm để gửi yêu cầu.', life: 3000 });
    return;
  }

  const payload = {
    docType: odStore.order?.docType || '',
    cardName: odStore.order?.cardName,
    cardCode: odStore.order?.cardCode,
    cardId: odStore.order?.cardId,
    refInvoiceCode: odStore.order?.invoiceCode,
    docDate: new Date().toISOString(),
    itemDetails: [...itemsToReturn, ...itemsToExchange]
  };

  isSubmitting.value = true;
  try {
    await PurchaseReturnService.add(payload);
    toast.add({ severity: 'success', summary: 'Thành công', detail: 'Tạo yêu cầu trả hàng thành công', life: 3000 });
    setTimeout(() => {
      router.push({ name: 'clientReturnRequest' });
    }, 1000);
  } catch (error) {
    console.error('Error submitting return request:', error);
    toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tạo yêu cầu trả hàng. Vui lòng thử lại sau.', life: 3000 });
  } finally {
    isSubmitting.value = false;
  }
};

watch(
  () => odStore.order,
  (newOrder) => {
    exchangeProducts.value = [];
    selectedExchangeProducts.value = [];

    if (newOrder && newOrder.itemDetail) {
      orderProducts.value = newOrder.itemDetail.map((item) => ({
        id: item.id,
        productId: item.itemId,
        baseId: item.fatherId,
        baseLineId: item.id,
        productCode: item.itemCode,
        name: item.itemName,
        uomCode: item.uomCode,
        unit: item.uomName,
        openQty: item.openQty,
        price: item.price || 0,
        selected: false,
        returnQty: 0,
        reason: '',
        errors: {}
      }));
      selectAll.value = false;
    } else {
      orderProducts.value = [];
    }
  },
  { immediate: true }
);

onMounted(() => {
  const orderId = route.params.id;
  if (orderId && odStore.order?.id !== parseInt(String(orderId))) {
    odStore.fetchStore(parseInt(String(orderId)));
  }
});
</script>

<style scoped lang="scss">
.return-from-order-container {
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

:deep(.custom-return-table .p-datatable-thead > tr > th) {
  background-color: #f8fafc;
  color: #4b5563;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  border-color: #d8e0ea;
  padding: 0.7rem 0.8rem;
}

:deep(.custom-return-table .p-datatable-tbody > tr > td) {
  border-color: #d8e0ea;
  padding: 0.6rem 0.8rem;
  background-color: #fff;
}

:deep(.custom-return-table .p-datatable-wrapper) {
  border: 1px solid #d8e0ea;
  border-radius: 6px;
}

:deep(.custom-return-table .p-checkbox .p-checkbox-box) {
  border-color: #cbd5e1;

  &.p-highlight {
    border-color: #3b82f6;
    background: #3b82f6;
  }
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
