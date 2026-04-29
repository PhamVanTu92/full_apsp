<template>
  <div class="return-request-detail-page">
    <div class="flex justify-content-between align-items-center mb-3">
      <h4 class="m-0 font-bold">{{ t('body.ReturnRequestList.Detail.title') }}</h4>
      <Button
        :label="t('body.ReturnRequestList.Detail.back')"
        icon="pi pi-arrow-left"
        severity="secondary"
        outlined
        class="bg-white border-300 text-700"
        @click="goBack"
      />
    </div>

    <div v-if="loading" class="card p-5 text-center">
      <ProgressSpinner style="width: 42px; height: 42px" strokeWidth="5" />
    </div>

    <div v-else-if="!requestDetail" class="card p-5 text-center text-500">
      {{ t('body.ReturnRequestList.Detail.empty') }}
    </div>

    <template v-else>
      <div v-if="showCancellationNotice" class="cancel-notification mb-3">
        <div class="cancel-card">
          <h5 class="cancel-title">{{ t('body.ReturnRequestList.Detail.cancelledTitle') }}</h5>
          <p class="cancel-reason">
            <strong>{{ t('body.ReturnRequestList.Detail.cancelledReason') }}:</strong>
            {{ cancellationReason || '--' }}
          </p>
        </div>
      </div>

      <div class="card p-4 mb-4">
        <div class="grid mt-0">
          <div class="col-12 md:col-6 py-0">
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.customerCode') }}</label>
              <span>{{ requestDetail.cardCode || '--' }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.taxCode') }}</label>
              <span>{{ customerTaxCode }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.customerName') }}</label>
              <span class="font-medium">{{ requestDetail.cardName || '--' }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.requestStatus') }}</label>
              <div class="compact-tag-wrap">
                <Tag
                  :value="getStatusInfo(requestDetail.status).label"
                  :class="getStatusInfo(requestDetail.status).class"
                />
              </div>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.requestType') }}</label>
              <div class="compact-tag-wrap">
                <Tag
                  :value="getTypeInfo(requestDetail.objType, requestDetail.refInvoiceCode).label"
                  :class="getTypeInfo(requestDetail.objType, requestDetail.refInvoiceCode).class"
                />
              </div>
            </div>
          </div>

          <div class="col-12 md:col-6 py-0">
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.requestCode') }}</label>
              <span class="font-semibold text-primary">{{ requestDetail.invoiceCode || '--' }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.referenceOrder') }}</label>
              <span>{{ requestDetail.refInvoiceCode || '--' }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.creator') }}</label>
              <span>{{ getCreatorName(requestDetail) }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.createdAt') }}</label>
              <span>{{ formatDateTime(requestDetail.docDate) }}</span>
            </div>
            <div class="field-row">
              <label>{{ t('body.ReturnRequestList.Detail.updatedAt') }}</label>
              <span>{{ formatDateTime(requestDetail.updatedAt || requestDetail.updateDate || requestDetail.udate) }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="card p-0 mb-4 px-3 pt-3">
        <div class="flex justify-content-between align-items-center mb-3">
          <h5 class="m-0 font-bold text-900">{{ t('body.ReturnRequestList.Independent.productList') }}</h5>
          <Tag v-if="returnItems.length > 0" :value="currencyLabel" class="text-700 bg-gray-100 border-1 border-300 px-3 py-1" />
        </div>

        <DataTable :value="returnItems" class="table-main return-detail-table p-datatable-sm" responsiveLayout="scroll" showGridlines>
          <template #empty>
            <div class="py-4 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
          </template>

          <Column field="index" :header="t('body.ReturnRequestList.Detail.index')" style="width: 4rem">
            <template #body="slotProps">
              {{ slotProps.index + 1 }}
            </template>
          </Column>

          <Column field="itemName" :header="t('body.ReturnRequestList.Detail.productName')" style="min-width: 22rem">
            <template #body="slotProps">
              <div class="flex flex-column">
                <span class="font-medium">{{ slotProps.data.itemName || '--' }}</span>
                <span v-if="slotProps.data.itemCode" class="text-xs text-500 mt-1">{{ slotProps.data.itemCode }}</span>
              </div>
            </template>
          </Column>

          <Column field="uomName" :header="t('body.ReturnRequestList.Detail.unit')" style="min-width: 9rem" />

          <Column field="quantity" :header="t('body.ReturnRequestList.Detail.quantity')" class="text-right" style="min-width: 8rem">
            <template #body="slotProps">
              {{ formatNumber(slotProps.data.quantity, 3) }}
            </template>
          </Column>

          <Column field="price" :header="t('body.ReturnRequestList.Detail.price')" class="text-right" style="min-width: 10rem">
            <template #body="slotProps">
              <div v-if="!isClient && canEditPrice" class="flex justify-content-end">
                <InputNumber v-model="slotProps.data.price" :min="0" :useGrouping="true" :minFractionDigits="2" :maxFractionDigits="2" class="w-full price-edit-input" />
              </div>
              <span v-else>{{ formatMoney(slotProps.data.price) }}</span>
            </template>
          </Column>

          <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" class="text-right" style="min-width: 11rem">
            <template #body="slotProps">
              {{ formatMoney(slotProps.data.quantity * slotProps.data.price) }}
            </template>
          </Column>

          <Column field="remarks" :header="t('body.ReturnRequestList.Detail.reason')" style="min-width: 18rem">
            <template #body="slotProps">
              <span>{{ slotProps.data.remarks || '--' }}</span>
            </template>
          </Column>
        </DataTable>
      </div>

      <div class="card p-0 mb-4 px-3 pt-3">
        <div class="flex justify-content-between align-items-center mb-3">
          <h5 class="m-0 font-bold text-900">{{ t('body.ReturnRequestList.Independent.exchangeProductList') }}</h5>
          <Tag v-if="exchangeItems.length > 0" :value="currencyLabel" class="text-700 bg-gray-100 border-1 border-300 px-3 py-1" />
        </div>

        <DataTable :value="exchangeItems" class="table-main return-detail-table p-datatable-sm" responsiveLayout="scroll" showGridlines>
          <template #empty>
            <div class="py-4 text-center text-500">{{ t('body.ReturnRequestList.noData') }}</div>
          </template>

          <Column field="index" :header="t('body.ReturnRequestList.Detail.index')" style="width: 4rem">
            <template #body="slotProps">
              {{ slotProps.index + 1 }}
            </template>
          </Column>

          <Column field="itemName" :header="t('body.ReturnRequestList.Detail.productName')" style="min-width: 22rem">
            <template #body="slotProps">
              <div class="flex flex-column">
                <span class="font-medium">{{ slotProps.data.itemName || '--' }}</span>
                <span v-if="slotProps.data.itemCode" class="text-xs text-500 mt-1">{{ slotProps.data.itemCode }}</span>
              </div>
            </template>
          </Column>

          <Column field="uomName" :header="t('body.ReturnRequestList.Detail.unit')" style="min-width: 9rem" />

          <Column field="quantity" :header="t('body.ReturnRequestList.Detail.quantity')" class="text-right" style="min-width: 8rem">
            <template #body="slotProps">
              {{ formatNumber(slotProps.data.quantity, 3) }}
            </template>
          </Column>

          <Column field="price" :header="t('body.ReturnRequestList.Detail.price')" class="text-right" style="min-width: 10rem">
            <template #body="slotProps">
              <div v-if="!isClient && canEditPrice" class="flex justify-content-end">
                <InputNumber v-model="slotProps.data.price" :min="0" :useGrouping="true" :minFractionDigits="2" :maxFractionDigits="2" class="w-full price-edit-input" />
              </div>
              <span v-else>{{ formatMoney(slotProps.data.price) }}</span>
            </template>
          </Column>

          <Column field="amount" :header="t('body.ReturnRequestList.Detail.amount')" class="text-right" style="min-width: 11rem">
            <template #body="slotProps">
              {{ formatMoney(slotProps.data.quantity * slotProps.data.price) }}
            </template>
          </Column>

          <Column field="remarks" :header="t('body.ReturnRequestList.Detail.reason')" style="min-width: 18rem">
            <template #body="slotProps">
              <span>{{ slotProps.data.remarks || '--' }}</span>
            </template>
          </Column>
        </DataTable>

        <div class="summary-wrapper">
          <div class="summary-card">
            <div class="summary-row">
              <span>{{ t('body.ReturnRequestList.Detail.totalLines') }}</span>
              <b>{{ editedItems.length }}</b>
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
        <div class="summary-status">
          <Tag
            :value="settlementDirectionLabel"
            :class="settlementDirectionTagClass"
          />
          <span class="summary-status__text">
            {{ settlementDirectionDescription }}
          </span>
        </div>
        <div v-if="statusActionButtons.length" class="detail-actions">
          <div class="detail-actions__inner">
            <Button
              v-if="hasPriceChanges"
              :label="t('Custom.save')"
              severity="warning"
              :loading="actionLoadingKey === 'save_price'"
              @click="savePriceChanges"
            />
            <Button
              v-for="action in statusActionButtons"
              :key="action.key"
              :label="action.label"
              :severity="action.severity"
              :loading="actionLoadingKey === action.key"
              @click="onClickStatusAction(action)"
            />
          </div>
        </div>
      </div>
    </template>

    <Dialog
      v-model:visible="cancelDialogVisible"
      :modal="true"
      :closable="true"
      :draggable="false"
      class="return-cancel-dialog"
      @hide="resetCancelDialog"
    >
      <template #header>
        <div class="cancel-dialog-header">
          <div class="cancel-dialog-header__icon">
            <i class="pi pi-exclamation-triangle text-orange-500"></i>
          </div>
          <div class="cancel-dialog-header__content">
            <h3 class="cancel-dialog-header__title">
              {{ t('body.ReturnRequestList.Detail.cancelDialogTitle') }}
            </h3>
            <p class="cancel-dialog-header__subtitle">
              {{ t('body.ReturnRequestList.Detail.cancelDialogSubtitle') }}
            </p>
          </div>
        </div>
      </template>

      <div class="cancel-dialog-body">
        <label class="cancel-dialog-body__label" for="return-request-cancel-reason">
          <i class="pi pi-comment mr-2"></i>
          {{ t('body.ReturnRequestList.Detail.reason') }}
          <span class="text-red-500 ml-1">*</span>
        </label>
        <Textarea
          id="return-request-cancel-reason"
          v-model="cancelReason"
          rows="4"
          :maxlength="1000"
          class="w-full cancel-reason-input"
          :class="{ 'p-invalid': showCancelReasonError && !cancelReason.trim() }"
          :placeholder="t('body.ReturnRequestList.Detail.cancelPlaceholder')"
        />
        <small class="cancel-dialog-body__count">
          {{ cancelReason.length }}/1000 {{ t('Custom.characters') }}
        </small>
        <small v-if="showCancelReasonError && !cancelReason.trim()" class="cancel-dialog-body__error">
          <i class="pi pi-exclamation-circle mr-1"></i>
          {{ t('body.ReturnRequestList.Detail.cancelReasonRequired') }}
        </small>
      </div>

      <template #footer>
        <div class="flex justify-content-end gap-2 w-full">
          <Button
            :label="t('Notification.cancel')"
            severity="secondary"
            outlined
            icon="pi pi-times"
            @click="cancelDialogVisible = false"
          />
          <Button
            :label="t('Notification.confirm')"
            icon="pi pi-check"
            :loading="actionLoadingKey === 'cancel'"
            @click="submitCancelAction"
          />
        </div>
      </template>
    </Dialog>

    <Dialog
      v-model:visible="paymentDialogVisible"
      class="w-30rem"
      :header="t('body.ReturnRequestList.Detail.paymentDialogTitle')"
      modal
      @hide="resetPaymentDialog"
    >
      <template #header>
        <div class="flex-grow-1 text-center font-bold text-xl">
          {{ t('body.ReturnRequestList.Detail.paymentDialogTitle') }}
        </div>
      </template>

      <div class="p-3 border-left-3 border-300 border-round flex flex-column gap-3 mb-3">
        <div class="flex justify-content-between">
          <span>{{ t('paymentMethod.customerCode') }}</span>
          <span>{{ requestDetail?.cardCode || '--' }}</span>
        </div>
        <div class="flex justify-content-between">
          <span>{{ t('body.ReturnRequestList.Detail.exchangeValue') }}</span>
          <span>{{ formatMoney(exchangeValue) }}</span>
        </div>
        <div class="flex justify-content-between">
          <span>{{ t('body.ReturnRequestList.Detail.returnValue') }}</span>
          <span>{{ formatMoney(returnValue) }}</span>
        </div>
        <hr class="my-0" />
        <div class="flex justify-content-between align-items-center">
          <span>{{ settlementSummaryLabel }}</span>
          <span class="font-bold" :class="settlementAmountClass">
            {{ formatSignedMoney(settlementAmount) }}
          </span>
        </div>
      </div>

      <template v-if="requiresCustomerPayment">
        <div class="pl-3 py-2 border-left-3 border-300 font-bold surface-100 mb-3">
          {{ t('paymentMethod.selectPaymentMethod') }}
        </div>
        <div class="flex flex-column gap-3">
          <label class="payment-option-label p-3 border-1 border-300 border-round flex">
            <div class="flex-grow-1">
              <div class="flex gap-2 align-items-center mb-2">
                <span>
                  <img src="@/assets/payment/vnpay.jpg" width="40" height="20" />
                </span>
                <div class="font-bold">{{ t('paymentMethod.onlinePaymentVNPAY') }}</div>
              </div>
              <small>{{ t('paymentMethod.onlinePaymentVNPAYDescription') }}</small>
            </div>
            <div>
              <RadioButton v-model="typePayment" value="VnPay" />
            </div>
          </label>

          <label class="payment-option-label p-3 border-1 border-300 border-round flex">
            <div class="flex-grow-1">
              <div class="flex gap-2 align-items-center mb-2">
                <span>
                  <img src="@/assets/payment/credit-card.png" width="40" height="40" />
                </span>
                <div class="font-bold">{{ t('paymentMethod.bankTransfer') }}</div>
              </div>
              <small>{{ t('paymentMethod.bankTransferDescription') }}</small>
            </div>
            <div>
              <RadioButton v-model="typePayment" value="BankWire" />
            </div>
          </label>
        </div>
      </template>

      <div v-else class="payment-settlement-note" :class="settlementDirectionPanelClass">
        <i class="pi" :class="settlementDirectionIcon"></i>
        <span>{{ settlementDirectionDescription }}</span>
      </div>

      <template #footer>
        <div class="flex gap-2 w-full">
          <Button
            :label="t('paymentMethod.cancel')"
            @click="paymentDialogVisible = false"
            severity="secondary"
            class="flex-grow-1"
          />
          <Button
            :label="requiresCustomerPayment ? t('paymentMethod.confirm') : t('Notification.confirm')"
            @click="confirmPaymentAction"
            class="flex-grow-1"
            :loading="actionLoadingKey === 'payment'"
          />
        </div>
      </template>
    </Dialog>

    <Dialog
      v-model:visible="paymentConfirmVisible"
      modal
      :header="t('paymentMethod.confirm')"
      :style="{ width: '29rem' }"
    >
      <span>{{ t('paymentMethod.confirmBankTransfer') }}</span>
      <template #footer>
        <div class="flex justify-content-end gap-2">
          <Button
            :label="t('paymentMethod.cancel')"
            severity="secondary"
            @click="paymentConfirmVisible = false"
          />
          <Button
            :label="t('paymentMethod.confirm')"
            :loading="actionLoadingKey === 'payment'"
            @click="confirmBankTransferPayment"
          />
        </div>
      </template>
    </Dialog>

    <ConfirmDialog />
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';

import PurchaseReturnService from '@/services/purchaseReturn.service';
import API from '@/api/api-main';
import { getLabels } from '@/components/Status';
import format from '@/helpers/format.helper';

const props = defineProps({
  isClient: {
    type: Boolean,
    default: false
  }
});

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const confirm = useConfirm();
const toast = useToast();

const loading = ref(false);
const requestDetail = ref(null);
const customerDetail = ref(null);
const actionLoadingKey = ref('');
const cancelDialogVisible = ref(false);
const cancelReason = ref('');
const showCancelReasonError = ref(false);
const paymentDialogVisible = ref(false);
const paymentConfirmVisible = ref(false);
const typePayment = ref('VnPay');

const requestId = computed(() => Number(route.params.id) || 0);
const defaultLineType = computed(() => {
  const isFromOrder = requestDetail.value?.objType === 'I' || !!requestDetail.value?.refInvoiceCode;
  return isFromOrder ? 'I' : 'O';
});

const editedItems = ref([]);
const returnItems = computed(() => editedItems.value.filter((item) => item.type === 'I'));
const exchangeItems = computed(() => editedItems.value.filter((item) => item.type === 'O'));

const initEditedItems = () => {
  const rawItems = requestDetail.value?.itemDetails || requestDetail.value?.itemDetail || [];
  editedItems.value = rawItems.map((item, index) => {
    const rawType = (item.type || item.objType || defaultLineType.value || '').toString().toUpperCase();
    const lineType = rawType === 'O' ? 'O' : 'I';
    const quantity = Number(item.quantity ?? 0) || 0;
    const price = Number(item.price ?? item.unitPrice ?? 0) || 0;

    return {
      id: item.id || `${item.itemCode || 'line'}-${index}`,
      itemId: item.itemId,
      itemCode: item.itemCode || '',
      itemName: item.itemName || item.name || '--',
      uomName: item.uomName || item.uonName || item.uoMName || item.uomCode || '--',
      uomCode: item.uomCode,
      quantity,
      price,
      originalPrice: price,
      amount: quantity * price,
      remarks: item.remarks || item.reason || item.note || '',
      type: lineType
    };
  });
};

const hasPriceChanges = computed(() => {
  return editedItems.value.some((item) => item.price !== item.originalPrice);
});

const canEditPrice = computed(() => {
  const status = (requestDetail.value?.status || '').toString().toUpperCase();
  // Allow editing in Pending (P) or Processing (TTN/DXL) statuses
  return ['P', 'TTN', 'DXL'].includes(status);
});

const savePriceChanges = async () => {
  if (!requestId.value || !requestDetail.value) return;

  actionLoadingKey.value = 'save_price';
  try {
    const updatedItemDetails = editedItems.value.map((item) => ({
      id: item.id,
      itemId: item.itemId,
      itemCode: item.itemCode,
      itemName: item.itemName,
      uomCode: item.uomCode,
      uomName: item.uomName,
      type: item.type,
      quantity: item.quantity,
      price: item.price,
      remarks: item.remarks
    }));

    const docData = {
      ...requestDetail.value,
      itemDetails: updatedItemDetails
    };

    const formData = new FormData();
    // Setting individual fields as per documentation
    formData.append('Id', requestId.value);
    formData.append('ObjType', requestDetail.value.objType || '');
    formData.append('DocType', requestDetail.value.docType || '');
    formData.append('RefInvoiceCode', requestDetail.value.refInvoiceCode || '');
    formData.append('CardId', requestDetail.value.cardId || '');
    formData.append('Currency', requestDetail.value.currency || '');
    formData.append('CardCode', requestDetail.value.cardCode || '');
    formData.append('CardName', requestDetail.value.cardName || '');
    formData.append('DocDate', requestDetail.value.docDate || '');
    formData.append('UserId', requestDetail.value.userId || '');
    
    // Pattern used in OrderDetail.vue: sending the whole object as a JSON string in 'document'
    formData.append('document', JSON.stringify(docData));

    // Also adding ItemDetails individually if needed by the backend multipart binder
    updatedItemDetails.forEach((item, index) => {
      formData.append(`ItemDetails[${index}].id`, item.id);
      formData.append(`ItemDetails[${index}].price`, item.price);
      formData.append(`ItemDetails[${index}].quantity`, item.quantity);
      formData.append(`ItemDetails[${index}].type`, item.type);
      formData.append(`ItemDetails[${index}].itemCode`, item.itemCode);
      formData.append(`ItemDetails[${index}].uomCode`, item.uomCode);
    });

    await PurchaseReturnService.update(requestId.value, formData);
    
    toast.add({
      severity: 'success',
      summary: t('body.systemSetting.success_label'),
      detail: t('body.ReturnRequestList.Detail.changeStatusSuccess'),
      life: 3000
    });
    
    await fetchDetail();
  } catch (error) {
    console.error('Error saving price changes:', error);
    toast.add({
      severity: 'error',
      summary: t('Custom.titleMessageError'),
      detail: t('body.ReturnRequestList.Detail.changeStatusFailed'),
      life: 3000
    });
  } finally {
    actionLoadingKey.value = '';
  }
};

const totalQuantity = computed(() => editedItems.value.reduce((sum, item) => sum + item.quantity, 0));
const totalValue = computed(() => editedItems.value.reduce((sum, item) => sum + (item.quantity * item.price), 0));
const exchangeValue = computed(() => editedItems.value.filter((item) => item.type === 'O').reduce((sum, item) => sum + (item.quantity * item.price), 0));
const returnValue = computed(() => editedItems.value.filter((item) => item.type === 'I').reduce((sum, item) => sum + (item.quantity * item.price), 0));
const settlementAmount = computed(() => normalizeMoneyValue(exchangeValue.value - returnValue.value));
const requiresCustomerPayment = computed(() => settlementAmount.value > 0);
const settlementSummaryLabel = computed(() => {
  if (settlementAmount.value > 0) {
    return t('body.ReturnRequestList.Detail.customerTransferAmount');
  }
  if (settlementAmount.value < 0) {
    return t('body.ReturnRequestList.Detail.companyRefundAmount');
  }
  return t('body.ReturnRequestList.Detail.noSettlementAmount');
});
const settlementDirectionLabel = computed(() => {
  if (settlementAmount.value > 0) {
    return t('body.ReturnRequestList.Detail.customerTransferTag');
  }
  if (settlementAmount.value < 0) {
    return t('body.ReturnRequestList.Detail.companyRefundTag');
  }
  return t('body.ReturnRequestList.Detail.noSettlementTag');
});
const settlementDirectionDescription = computed(() => {
  if (settlementAmount.value > 0) {
    return t('body.ReturnRequestList.Detail.customerTransferNote');
  }
  if (settlementAmount.value < 0) {
    return t('body.ReturnRequestList.Detail.companyRefundNote');
  }
  return t('body.ReturnRequestList.Detail.noSettlementNote');
});
const settlementAmountClass = computed(() => {
  if (settlementAmount.value > 0) {
    return 'text-green-700';
  }
  if (settlementAmount.value < 0) {
    return 'text-orange-600';
  }
  return 'text-700';
});
const settlementDirectionTagClass = computed(() => {
  if (settlementAmount.value > 0) {
    return 'bg-green-100 text-green-700 border-none';
  }
  if (settlementAmount.value < 0) {
    return 'bg-orange-100 text-orange-700 border-none';
  }
  return 'bg-gray-100 text-gray-700 border-none';
});
const settlementDirectionPanelClass = computed(() => {
  if (settlementAmount.value > 0) {
    return 'payment-settlement-note--positive';
  }
  if (settlementAmount.value < 0) {
    return 'payment-settlement-note--negative';
  }
  return 'payment-settlement-note--neutral';
});
const settlementDirectionIcon = computed(() => {
  if (settlementAmount.value > 0) {
    return 'pi-arrow-circle-right';
  }
  if (settlementAmount.value < 0) {
    return 'pi-arrow-circle-left';
  }
  return 'pi-info-circle';
});
const currencyLabel = computed(() => requestDetail.value?.currency || 'VND');
const customerTaxCode = computed(() => (
  customerDetail.value?.licTradNum ||
  customerDetail.value?.taxCode ||
  requestDetail.value?.licTradNum ||
  requestDetail.value?.taxCode ||
  '--'
));
const normalizedStatus = computed(() => (requestDetail.value?.status || '').toString().toUpperCase());
const cancellationReason = computed(() => (
  requestDetail.value?.reasonForCancellation ||
  requestDetail.value?.ReasonForCancellation ||
  requestDetail.value?.cancellationReason ||
  requestDetail.value?.cancelReason ||
  requestDetail.value?.reasonCancel ||
  ''
).toString().trim());
const showCancellationNotice = computed(() => ['HUY2', 'HUY', 'C', 'DH', 'CANCELLED'].includes(normalizedStatus.value));
const statusActionButtons = computed(() => {
  const status = (requestDetail.value?.status || '').toString().toUpperCase();
  const actions = [];

  if (!status) {
    return actions;
  }

  if (!props.isClient && ['TTN', 'CXN', 'DXL', 'CTT'].includes(status)) {
    actions.push({
      key: 'cancel',
      label: t('Notification.cancel'),
      nextStatus: 'HUY2',
      severity: 'danger'
    });
  }

  if (!props.isClient && status === 'DXL') {
    actions.push({
      key: 'approve',
      label: t('Custom.approve'),
      nextStatus: 'CXN',
      severity: 'success'
    });
  }

  if (!props.isClient && ['CXN', 'DTT'].includes(status)) {
    actions.push({
      key: 'confirmToSAP',
      label: t('Custom.confirmToSAP'),
      nextStatus: 'DXN',
      severity: 'success'
    });
  }

  if (!props.isClient && status === 'DGH') {
    actions.push({
      key: 'complete',
      label: t('Custom.complete'),
      nextStatus: 'DHT',
      severity: 'success'
    });
  }

  if (status === 'DGHR') {
    actions.push({
      key: 'complete',
      label: t('Custom.complete'),
      nextStatus: 'DHT',
      severity: 'success'
    });
  }

  if (props.isClient && status === 'CTT') {
    actions.push({
      key: 'payment',
      label: t('Custom.payment'),
      nextStatus: 'DTT',
      severity: 'success'
    });
  }

  return actions;
});

const fetchCustomerDetail = async () => {
  const cardId = requestDetail.value?.cardId;
  customerDetail.value = null;

  if (!cardId) {
    return;
  }

  try {
    const response = await API.get(`customer/${cardId}`);
    customerDetail.value = response?.data || null;
  } catch (error) {
    console.error('Error fetching customer detail:', error);
  }
};

const fetchDetail = async () => {
  if (!requestId.value) {
    requestDetail.value = null;
    customerDetail.value = null;
    return;
  }

  loading.value = true;
  try {
    const response = await PurchaseReturnService.getById(requestId.value);
    requestDetail.value = response?.item || response || null;
    initEditedItems();
    await fetchCustomerDetail();
  } catch (error) {
    requestDetail.value = null;
    customerDetail.value = null;
    toast.add({
      severity: 'error',
      summary: t('body.Custom.titleMessageError'),
      detail: t('body.ReturnRequestList.Detail.fetchFailed'),
      life: 3000
    });
  } finally {
    loading.value = false;
  }
};

const goBack = () => {
  router.push({ name: props.isClient ? 'clientReturnRequest' : 'returnRequestList' });
};

const updateStatus = async (action, options = {}) => {
  if (!action?.nextStatus || !requestId.value) {
    return;
  }

  actionLoadingKey.value = action.key;
  try {
    await PurchaseReturnService.changeStatus(requestId.value, action.nextStatus, options);
    toast.add({
      severity: 'success',
      summary: t('body.systemSetting.success_label'),
      detail: t('body.ReturnRequestList.Detail.changeStatusSuccess'),
      life: 3000
    });
    if (action.key === 'cancel') {
      resetCancelDialog();
    }
    if (action.key === 'payment') {
      resetPaymentDialog();
    }
    await fetchDetail();
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: t('body.Custom.titleMessageError'),
      detail: t('body.ReturnRequestList.Detail.changeStatusFailed'),
      life: 3000
    });
  } finally {
    actionLoadingKey.value = '';
  }
};

const resetCancelDialog = () => {
  cancelDialogVisible.value = false;
  cancelReason.value = '';
  showCancelReasonError.value = false;
};

const resetPaymentDialog = () => {
  paymentDialogVisible.value = false;
  paymentConfirmVisible.value = false;
  typePayment.value = 'VnPay';
};

const openCancelDialog = () => {
  resetCancelDialog();
  cancelDialogVisible.value = true;
};

const openPaymentDialog = () => {
  resetPaymentDialog();
  paymentDialogVisible.value = true;
};

const submitCancelAction = async () => {
  const cancelAction = statusActionButtons.value.find((action) => action.key === 'cancel');

  if (!cancelAction) {
    return;
  }

  if (!cancelReason.value.trim()) {
    showCancelReasonError.value = true;
    return;
  }

  await updateStatus(cancelAction, {
    reasonForCancellation: cancelReason.value.trim()
  });
};

const confirmBankTransferPayment = async () => {
  await updateStatus({
    key: 'payment',
    nextStatus: 'DTT'
  });
};

const confirmPaymentAction = async () => {
  if (!requestId.value) {
    return;
  }

  if (!requiresCustomerPayment.value) {
    await updateStatus({
      key: 'payment',
      nextStatus: 'DTT'
    });
    return;
  }

  if (typePayment.value === 'BankWire') {
    paymentConfirmVisible.value = true;
    return;
  }

  paymentDialogVisible.value = false;
  router.push({
    path: `/client/payment/RETURN/${requestId.value}`
  });
};

const onClickStatusAction = (action) => {
  if (!action?.nextStatus || actionLoadingKey.value) {
    return;
  }

  if (action.key === 'cancel') {
    openCancelDialog();
    return;
  }

  if (action.key === 'payment') {
    openPaymentDialog();
    return;
  }

  if (action.key === 'confirmToSAP') {
    confirm.require({
      message: t('body.ReturnRequestList.Detail.confirmStatusChange'),
      header: t('Notification.confirm'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: t('Notification.confirm'),
      rejectLabel: t('Notification.cancel'),
      accept: async () => {
        await updateStatus(action);
      }
    });
    return;
  }

  updateStatus(action);
};

const formatDateTime = (value) => {
  if (!value) return '--';
  const result = format.DateTime(value);
  if (!result?.date) return '--';
  return result.time ? `${result.time} - ${result.date}` : result.date;
};

const formatNumber = (value, maxFractionDigits = 0) => {
  const parsedValue = Number(value);
  if (!Number.isFinite(parsedValue)) return '0';
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: maxFractionDigits }).format(parsedValue);
};

const formatMoney = (value) => {
  const parsedValue = Number(value);
  if (!Number.isFinite(parsedValue)) return '0';
  return `${new Intl.NumberFormat('vi-VN').format(parsedValue)} ${currencyLabel.value}`;
};

const formatSignedMoney = (value) => {
  const parsedValue = Number(value);
  if (!Number.isFinite(parsedValue)) return `0 ${currencyLabel.value}`;
  const sign = parsedValue > 0 ? '+' : parsedValue < 0 ? '-' : '';
  return `${sign}${new Intl.NumberFormat('vi-VN').format(Math.abs(parsedValue))} ${currencyLabel.value}`;
};

const normalizeMoneyValue = (value) => {
  const parsedValue = Number(value);
  if (!Number.isFinite(parsedValue)) return 0;
  return Math.abs(parsedValue) < 0.000001 ? 0 : parsedValue;
};

const getCreatorName = (data) => {
  return data?.author?.fullName || data?.authorName || data?.createdBy || data?.creatorName || '--';
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
    label: status || '--',
    class: 'text-gray-700 bg-gray-100'
  };
};

const getTypeInfo = (objType, refInvoiceCode) => {
  const typeValue = (objType || '').toString().toUpperCase();
  const isFromOrder = typeValue === 'I' || !!refInvoiceCode || typeValue === 'FROM_ORDER';
  if (isFromOrder) {
    return {
      label: t('body.ReturnRequestList.type_from_order'),
      class: 'bg-blue-100 text-blue-700 px-3 border-none'
    };
  }
  return {
    label: t('body.ReturnRequestList.type_independent'),
    class: 'bg-purple-100 text-purple-700 px-3 border-none'
  };
};

watch(requestId, () => {
  fetchDetail();
});

onMounted(() => {
  fetchDetail();
});
</script>

<style scoped lang="scss">
.return-request-detail-page {
  padding: 1rem;
}

.cancel-notification {
  margin: 0 0 1rem;
}

.cancel-card {
  background: #fecaca;
  border-radius: 8px;
  padding: 0.9rem 1rem;
  border-left: 4px solid #ef4444;
}

.cancel-title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
  font-weight: 700;
  color: #7f1d1d;
}

.cancel-reason {
  margin: 0;
  color: #7f1d1d;
  line-height: 1.5;
  word-break: break-word;
}

.field-row {
  display: grid;
  grid-template-columns: 12rem minmax(0, 1fr);
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid #e2e8f0;
  padding: 0.8rem 0.25rem;

  label {
    font-weight: 700;
    color: #334155;
  }

  span {
    color: #1f2937;
    word-break: break-word;
  }
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

.detail-actions {
  border-top: 1px solid #e2e8f0;
  margin: 0 1rem;
  padding: 0.9rem 0 1rem;
}

.detail-actions__inner {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.cancel-dialog-header {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.cancel-dialog-header__icon {
  width: 3rem;
  height: 3rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  background: linear-gradient(135deg, #fef3c7, #fff7ed);
  border: 2px solid #fbbf24;
}

.cancel-dialog-header__icon i {
  font-size: 1.2rem;
}

.cancel-dialog-header__content {
  flex: 1;
}

.cancel-dialog-header__title {
  margin: 0 0 0.25rem;
  font-size: 1.15rem;
  font-weight: 700;
  color: #111827;
}

.cancel-dialog-header__subtitle {
  margin: 0;
  font-size: 0.9rem;
  color: #64748b;
}

.cancel-dialog-body {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 0.5rem 0 1rem;
}

.cancel-dialog-body__label {
  display: flex;
  align-items: center;
  font-weight: 600;
  color: #334155;
}

.cancel-dialog-body__count {
  text-align: right;
  color: #64748b;
}

.cancel-dialog-body__error {
  color: #dc2626;
  display: flex;
  align-items: center;
}

.cancel-reason-input {
  resize: none;
}

.payment-option-label {
  cursor: pointer;
}

.payment-option-label:hover {
  background: var(--primary-50);
}

.payment-settlement-note {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.payment-settlement-note i {
  font-size: 1.15rem;
  margin-top: 0.1rem;
}

.payment-settlement-note--positive {
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.payment-settlement-note--negative {
  background: #fff7ed;
  color: #c2410c;
  border: 1px solid #fed7aa;
}

.payment-settlement-note--neutral {
  background: #f8fafc;
  color: #475569;
  border: 1px solid #e2e8f0;
}

:deep(.return-detail-table .p-datatable-thead > tr > th) {
  font-size: 0.75rem;
  font-weight: 700;
  color: #475569;
  text-transform: uppercase;
  background: #f8fafc;
}

:deep(.return-detail-table .p-datatable-thead > tr > th:not(:last-child)),
:deep(.return-detail-table .p-datatable-tbody > tr > td:not(:last-child)) {
  border-right: 1px solid #eef2f6;
}

:deep(.return-detail-table .p-datatable-tbody > tr > td) {
  border-color: #e2e8f0;
}

:deep(.compact-tag-wrap) {
  justify-self: start;
}

:deep(.compact-tag-wrap .p-tag) {
  width: auto;
  display: inline-flex;
}

:deep(.return-cancel-dialog) {
  width: 30rem;
  max-width: calc(100vw - 2rem);
}

:deep(.price-edit-input .p-inputnumber-input) {
  text-align: right;
  padding: 0.4rem;
  font-weight: 700;
  color: #1e293b;
  border-color: #3b82f6;
}

:deep(.price-edit-input .p-inputnumber-buttons-stacked .p-button) {
  width: 1.5rem;
}

@media (max-width: 768px) {
  .field-row {
    grid-template-columns: 1fr;
    gap: 0.35rem;
  }

  .summary-status {
    align-items: flex-end;
    flex-direction: column;
  }

  .summary-status__text {
    max-width: 100%;
  }
}
</style>
