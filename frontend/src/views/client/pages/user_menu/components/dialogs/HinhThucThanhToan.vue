<template>
  <div>
    <Dialog
      v-model:visible="visible"
      class="w-30rem"
      :header="t('paymentMethod.selectPaymentMethod')"
      modal
      @hide="onHide"
    >
      <template #header>
        <div class="flex-grow-1 text-center font-bold text-xl">
          {{ t("paymentMethod.selectPaymentMethod") }}
        </div>
      </template>
      <div class="p-3 border-left-3 border-300 border-round flex flex-column gap-3 mb-3">
        <div class="flex justify-content-between">
          <span>{{ t("paymentMethod.customerCode") }}</span>
          <span> {{ odStore?.order?.cardCode }}</span>
        </div>
        <div class="flex justify-content-between">
          <span>{{ t("paymentMethod.totalInvoice") }}</span>
          <span>{{
            format.FormatCurrency(odStore?.order?.paymentInfo.totalAfterVat || 0)
          }}</span>
        </div>
        <div class="flex justify-content-between">
          <span>{{ t("paymentMethod.instantPaymentBonus") }}</span>
          <span>{{
            format.FormatCurrency(odStore?.order?.paymentInfo.bonusAmount || 0)
          }}</span>
        </div>
        <hr class="my-0" />
        <div class="flex justify-content-between">
          <span>{{ t("paymentMethod.totalPayment") }}</span>
          <span>{{
            format.FormatCurrency(
              (odStore.order?.paymentInfo?.totalAfterVat || 0) -
                (odStore.order?.paymentInfo?.bonusAmount || 0)
            )
          }}</span>
        </div>
      </div>
      <div class="pl-3 py-2 border-left-3 border-300 font-bold surface-100 mb-3">
        {{ t("paymentMethod.selectPaymentMethod") }}
      </div>
      <div class="flex flex-column gap-3">
        <label class="p-3 border-1 border-300 border-round flex">
          <div class="flex-grow-1">
            <div class="flex gap-2 align-items-center mb-2">
              <span>
                <img src="@/assets/payment/vnpay.jpg" width="40px" height="20px" />
              </span>
              <div class="font-bold">{{ t("paymentMethod.onlinePaymentVNPAY") }}</div>
            </div>
            <small>{{ t("paymentMethod.onlinePaymentVNPAYDescription") }}</small>
          </div>
          <div>
            <RadioButton v-model="typePayment" value="OnePay"></RadioButton>
          </div>
        </label>
        <label
          class="p-3 border-1 border-300 border-round flex"
          :class="{ 'opacity-50 cursor-not-allowed surface-100': isVisaOrder }"
        >
          <div class="flex-grow-1">
            <div class="flex gap-2 align-items-center mb-2">
              <span >
                <img src="@/assets/payment/credit-card.png" width="40px" height="40px" />
              </span>
              <div class="font-bold">{{ t("paymentMethod.bankTransfer") }}</div>
            </div>
            <small>{{ t("paymentMethod.bankTransferDescription") }}</small>
          </div>
          <div>
            <RadioButton v-model="typePayment" value="BankWire" :disabled="isVisaOrder"></RadioButton>
          </div>
        </label>
      </div>

      <template #footer>
        <div class="flex gap-2 w-full">
          <Button
            :label="t('paymentMethod.cancel')"
            @click="visible = false"
            severity="secondary"
            class="flex-grow-1"
          />
          <Button
            :label="t('paymentMethod.confirm')"
            @click="ConfirmPayment"
            class="flex-grow-1"
          />
        </div>
      </template>
    </Dialog>

    <Dialog
      v-model:visible="visibleConfirm"
      modal
      :header="t('paymentMethod.confirm')"
      :style="{ width: '29rem' }"
    >
      <span>{{ t("paymentMethod.confirmBankTransfer") }}</span>
      <template #footer>
        <div class="flex justify-content-end gap-2">
          <Button
            :label="t('paymentMethod.cancel')"
            severity="secondary"
            @click="visibleConfirm = false"
          />
          <Button :label="t('paymentMethod.confirm')" @click="PaymenBankWire()" />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from "vue";
  import format from "@/helpers/format.helper";
  import { useOrderDetailStore } from "../store/orderDetailNET";
  import API from "@/api/api-main";
  import { useRouter } from "vue-router";
  import { useToast } from "primevue/usetoast";
  import { useI18n } from "vue-i18n";

  const router = useRouter();
  const odStore = useOrderDetailStore();
  const toast = useToast();
  const { t } = useI18n();
  const typePayment = ref("OnePay");
  const visible = ref(false);

  const onHide = () => {
    typePayment.value = "OnePay";
  };

  const open = () => {
    if (isVisaOrder.value) typePayment.value = "OnePay";
    visible.value = true;
  };
  const visibleConfirm = ref(false);
  defineExpose({ open });

  const initialComponent = () => {
    // code here
  };

  onMounted(function () {
    initialComponent();
  });
  const ConfirmPayment = () => {
    if (isVisaOrder.value && typePayment.value === "BankWire") {
      typePayment.value = "OnePay";
      return;
    }

    switch (typePayment.value) {
      case "OnePay":
        PaymentOnePay();
        break;
      case "BankWire":
        visibleConfirm.value = true;
        break;
    }
  };
  const PaymentOnePay = async () => {
    try {
      if (
        odStore.order?.paymentInfo?.totalDebt ||
        odStore.order?.paymentInfo?.totalDebtGuarantee
      ) {
        toastService(
          "error",
          t("paymentMethod.error"),
          t("paymentMethod.orderNotSupportVNPAY")
        );
        return;
      }

      let link = "";
      switch (odStore.order?.docType) {
        case "NET":
          link = "/NET/" + odStore.order?.id;
          break;
        default:
          link = "/ORDER/" + odStore.order?.id;
          break;
      }

      router.push({
        path: "/client/payment" + link,
      });
    } catch (err) {
      console.error(err);
      toastService(
        "error",
        t("paymentMethod.error"),
        t("paymentMethod.somethingWentWrong")
      );
    }
  };
  const PaymenBankWire = async () => {
    try {
      await API.update(`PurchaseOrder/${odStore.order?.id}/change-status/UNC`);
      toastService(
        "success",
        t("paymentMethod.success"),
        t("paymentMethod.paymentConfirmed")
      );
      visibleConfirm.value = false;
      visible.value = false;
      odStore.fetchStore();
    } catch (error) {
      toastService(
        "error",
        t("paymentMethod.error"),
        t("paymentMethod.somethingWentWrong")
      );
    }
  };
  const toastService = (
    severity: "error" | "secondary" | "success" | "info" | "contrast" | "warn",
    smr: string,
    detail: string,
    life: number = 3000
  ) => {
    toast.add({
      severity: severity,
      detail: detail,
      summary: smr,
      life: life,
    });
  };

  const toNumber = (value: unknown) => {
    const parsedValue = Number(value);
    return Number.isFinite(parsedValue) ? parsedValue : 0;
  };

  const isVisaOrder = computed(() => {
    const order = odStore.order as any;
    const paymentMethods = Array.isArray(order?.paymentMethod)
      ? order.paymentMethod
      : Array.isArray(order?.payments)
        ? order.payments
        : [];
    const itemDetails = Array.isArray(order?.itemDetail) ? order.itemDetail : [];
    const hasVisaPaymentMethod = paymentMethods.some(
      (item: any) => item?.paymentMethodCode === "PayVisa" || item?.payMethod === "PayVisa"
    );
    const hasVisaItem = itemDetails.some((item: any) => item?.paymentMethodCode === "PayVisa");
    const hasVisaTotal = toNumber(order?.paymentInfo?.totalVisa) > 0;

    return hasVisaPaymentMethod || hasVisaItem || hasVisaTotal;
  });
</script>

<style scoped>
  label {
    cursor: pointer;
  }
  label.cursor-not-allowed {
    pointer-events: none;
  }
  label:hover {
    background: var(--primary-50);
  }
  label.cursor-not-allowed:hover {
    background: var(--surface-100);
  }
</style>
