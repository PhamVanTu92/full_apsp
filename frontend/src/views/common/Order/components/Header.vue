<template>
  <div class="card">
    <div class="grid">
      <div class="col-12 md:col-10 pb-0">
        <div class="grid mt-0">
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{
                t("body.report.table_header_customer_code_1")
              }}</label>
              <div class="col">
                <Button
                  @click="onClickCustomerDetail"
                  :label="odStore.order?.cardCode"
                  text
                  class="p-0 font-bold"
                />
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("body.OrderList.orderCode") }}</label>
              <div class="col">{{ odStore.order?.invoiceCode }}</div>
            </div> 
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("client.tax_code") }}</label>
              <div class="col">{{ odStore.customer?.licTradNum }}</div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("client.creator") }}</label>
              <div class="col">{{ odStore.order?.author.fullName }}</div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("client.customerName") }}</label>
              <div class="col">{{ odStore.order?.cardName }}</div>
            </div>
          </div>

          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("client.created_date") }}</label>
              <div class="col">
                {{
                  odStore.order?.docDate
                    ? format(odStore.order?.docDate, "HH:mm - dd/MM/yyyy")
                    : ""
                }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("body.OrderList.orderStatus") }}</label>
              <div class="col">
                <Tag
                  :class="formatStatus(odStore.order?.status || '')?.class"
                  :value="formatStatus(odStore.order?.status || '')?.label"
                />
              </div>
            </div>
          </div>

          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{ t("body.OrderList.order_date_label") }}</label>
              <div class="col">
                {{
                  odStore.order?.deliveryTime
                    ? format(odStore.order?.deliveryTime, "HH:mm - dd/MM/yyyy")
                    : ""
                }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{
                t("body.OrderList.payment_method_label")
              }}</label>
              <div class="col">
                {{ paymentMethodLabel }}
              </div>
            </div>
          </div>
          <div class="col-12 md:col-6 py-0">
            <div class="field grid">
              <label class="col-fixed">{{
                t("body.OrderList.payment_status_label")
              }}</label>
              <div
                :class="
                  odStore.order?.vnPayStatus == true
                    ? 'text-green-500 col'
                    : 'text-red-500 col'
                "
              >
                {{ onlinePaymentStatusLabel }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-12 md:col-2 pb-0">
        <KiemTraCongNo
          v-model:isExceedDebt="odStore.isExceedDebt"
          style="width: 12rem"
          :bpId="odStore.order?.cardId"
          :bpName="odStore.order?.cardName"
          :payCredit="odStore.order?.paymentInfo?.totalDebt"
          :payGuarantee="odStore.order?.paymentInfo?.totalDebtGuarantee"
          :statusOrder="odStore.order?.status"
          class="w-full"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { format } from "date-fns";
  import { computed, onMounted } from "vue";
  import { formatStatus } from "../script";
  import { useOrderDetailStore } from "../store/orderDetail";
  import router from "@/router";
  import { useI18n } from "vue-i18n";

  const { t } = useI18n();
  const odStore = useOrderDetailStore();
  const paymentLabelByCode: Record<string, string> = {
    PayNow: t("client.pay_immediately"),
    PayCredit: t("client.debt_balance"),
    PayGuarantee: t("client.installment_debt"),
    PayVisa: t("client.visa_credit_card"),
  };

  const onClickCustomerDetail = () => {
    router.push({
      name: "agencyCategory-detail",
      params: {
        id: odStore.order?.cardId,
      },
    });
  };

  const isWaitingPayment = computed(() => odStore.order?.status === "CTT");
  const paidPayments = computed(() => {
    if (isWaitingPayment.value) return [];
    return (odStore.order?.payments || []).filter((payment: any) => {
      return !payment?.status || payment.status === "A";
    });
  });
  const hasPaymentTransactions = computed(() => !!paidPayments.value.length);

  const normalizePaymentLabel = (payment: any, preferPaymentName = false) => {
    const code = payment?.paymentMethodCode || payment?.payMethod || payment?.methodCode || "";
    const name = payment?.paymentMethodName || payment?.paymentType || payment?.methodName || "";
    const normalizedName = name.toString().toLowerCase();

    if (preferPaymentName && name) {
      if (normalizedName === "bank" || normalizedName.includes("bankwire")) return "UNC/Chuyển khoản ngân hàng";
      if (normalizedName.includes("visa") || normalizedName.includes("card") || normalizedName.includes("credit")) return t("client.visa_credit_card");
      if (normalizedName.includes("onepay")) return "OnePay";
      if (normalizedName.includes("vnpay")) return "VnPay";
    }

    if (code === "PayVisa") return t("client.visa_credit_card");
    if (code && paymentLabelByCode[code]) return paymentLabelByCode[code];
    if (normalizedName === "bank" || normalizedName.includes("bankwire")) return "UNC/Chuyển khoản ngân hàng";
    if (normalizedName.includes("visa") || normalizedName.includes("card") || normalizedName.includes("credit")) return t("client.visa_credit_card");
    if (normalizedName.includes("onepay")) return "OnePay";
    if (normalizedName.includes("vnpay")) return "VnPay";
    return name || "";
  };

  const paymentSource = computed(() => {
    return paidPayments.value;
  });

  const paymentMethodLabel = computed(() => {
    const labels = paymentSource.value
      .map((payment: any) => normalizePaymentLabel(payment, hasPaymentTransactions.value))
      .filter(Boolean);
    return [...new Set(labels)].join(", ");
  });

  const hasOnlinePayment = computed(() => {
    return paymentSource.value.some((payment: any) => {
      const code = payment?.paymentMethodCode || "";
      const name = (payment?.paymentMethodName || payment?.paymentType || "").toString().toLowerCase();
      return code === "PayVisa" || name.includes("vnpay") || name.includes("onepay") || name.includes("visa") || name.includes("card");
    });
  });

  const onlinePaymentStatusLabel = computed(() => {
    if (hasOnlinePayment.value) {
      switch (odStore.order?.vnPayStatus) {
        case true:
          return t("paymentPage.payment_success_status");
        case false:
          return t("paymentPage.payment_failure_status");
        default:
          return "";
      }
    }
    return "";
  });

  const initialComponent = () => {
    // code here
  };
  onMounted(function () {
    initialComponent();
  });
</script>

<style scoped>
  .field.grid {
    border-bottom: 1px solid var(--surface-200);
    margin-left: 4px;
  }

  label {
    width: 15rem;
    font-weight: bold;
  }
</style>
