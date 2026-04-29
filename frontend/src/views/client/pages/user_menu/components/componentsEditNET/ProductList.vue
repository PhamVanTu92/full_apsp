<template>
  <DataTable
    :value="odStore.order?.itemDetail"
    resizableColumns
    columnResizeMode="fit"
    show-gridlines
    scrollable
    scroll-height="600px"
  >
    <Column field="" header="#">
      <template #body="{ index }">{{ index + 1 }}</template>
    </Column>
    <Column field="itemName" :header="t('client.productName')"></Column>
    <Column field="uomName" :header="t('client.unit')"></Column>
    <Column field="quantity" :header="t('client.quantity')" class="text-right"
      ><template #body="{ data, field }">
        {{ fnum(data[field]) }}
      </template></Column
    >
    <Column field="price" :header="t('client.unitPrice')" class="text-right"
      ><template #body="{ data, field }">
        {{ fnum(data[field]) }}
      </template></Column
    >
    <Column field="discount" :header="t('client.discount')" class="text-right">
      <template #body="{ data, field }">
        {{ fnum(data[field]) }}
        {{ data.discountType == "P" ? "%" : odStore?.order?.currency }}
      </template>
    </Column>
    <Column
      field="priceAfterDist"
      :header="t('client.discountedPrice')"
      class="text-right"
      ><template #body="{ data, field }">
        {{ fnum(data[field]) }}
      </template></Column
    >
    <Column field="vat" :header="t('client.taxRate')" class="text-right"
      ><template #body="{ data, field }">
        {{ fnum(data[field]) }}
      </template></Column
    >
    <Column field="" :header="t('client.subtotal')" class="text-right">
      <template #body="{ data }">
        {{ fnum(data["priceAfterDist"] * data["quantity"]) }}
      </template>
    </Column>
    <Column
      field="paymentMethodCode"
      :header="t('client.payment_methods')"
      class="text-right"
    >
      <template #body="{ data, field }">
        {{ getPaymentLabel(data[field]) }}
      </template>
    </Column>
    <template #footer>
      <div class="font-normal">
        {{ t("client.total_output") }}
        <span class="font-bold">{{ fnum(volumn || 0, 0, t("client.liter")) }}</span>
      </div>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue";
import { fnum } from "../dialogs/script";
import { useOrderDetailStore } from "../store/orderDetailNET";

import { useI18n } from "vue-i18n";
const { t } = useI18n();

const odStore = useOrderDetailStore();

const volumn = computed(() => {
  return odStore.order?.itemDetail.reduce(
    (sum, item) => item.itemVolume * item.quantity + sum,
    0
  );
});

const paymentMethodCode = {
  PayNow: t("client.pay_immediately"),
  PayCredit: t("client.debt_balance"),
  PayGuarantee: t("client.installment_debt"),
};

const getPaymentLabel = (key: string) => {
  return paymentMethodCode[key as keyof typeof paymentMethodCode] || "";
};

const initialComponent = () => {
  // code here
};

onMounted(function () {
  initialComponent();
});
</script>

<style scoped></style>
