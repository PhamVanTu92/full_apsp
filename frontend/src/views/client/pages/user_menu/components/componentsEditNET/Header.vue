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
                {{
                  odStore.order?.payments && odStore.order?.payments.length > 0
                    ? formatPaymentType(odStore.order?.payments)
                    : ""
                }}
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
                {{
                  paymentMethod == "VnPay"
                    ? convertStatusVNPay(odStore.order?.vnPayStatus)
                    : ""
                }}
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
          class="w-full"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { format } from "date-fns";
  import { onMounted , ref} from "vue";
  import { formatStatus } from "../dialogs/script";
  import { useOrderDetailStore } from "../store/orderDetailNET";
  import router from "@/router";
  import { useI18n } from "vue-i18n";

  const { t } = useI18n();
  const paymentMethod = ref("");
  const odStore = useOrderDetailStore();

  const onClickCustomerDetail = () => {
    router.push({
      name: "agencyCategory-detail",
      params: {
        id: odStore.order?.cardId,
      },
    });
  }; 

  const convertStatusVNPay = (status: boolean | undefined) => {
    if(paymentMethod.value == "VnPay"){
      switch (status) {
        case true:
          return t("paymentPage.payment_success_status");
        case false:
          return t("paymentPage.payment_failure_status");
        default:
          return "";
      }
    }
    return "";
  };

  const formatPaymentType = (payment: any) => {  
    paymentMethod.value =
      payment.reduce((max: any, item: any) => (item.id > max.id ? item : max))
        .paymentMethodName == "Bank"
        ? "Bank"
        : "VnPay";
    return paymentMethod.value && paymentMethod.value == "Bank" ? "UNC/Chuyển khoản ngân hàng" : "VnPay";
  };

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
