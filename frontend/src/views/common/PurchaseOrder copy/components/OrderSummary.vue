<template>
  <div class="border-1 border-green-700 p-3 border-round bg-white">
    <VoucherSelect></VoucherSelect>
    <template v-for="(item, i) in lists" :key="i">
      <div class="flex justify-content-between align-items-center my-3">
        <span v-html="item.label"></span>
        <InputNumber
          @change=""
          v-if="item.paynow"
          :min="0"
          v-model="item.value"
        ></InputNumber>

        <span v-else>{{ fnum(item.value, 0) }}</span>
      </div>
      <Divider v-if="item.divider" />
    </template>
    <div class="flex justify-content-between my-3">
      <span class="">{{ t("body.OrderList.total_order_value") }}</span>
      <span class="">{{ fnum(poStore.orderSummary.totalAfterVat, 0) }}</span>
    </div>
    <Divider />
    <div class="mt-5">
      <div class="flex justify-content-between text-lg">
        <span class="font-bold">{{ t("body.OrderList.total_payment") }}</span>
        <span class="font-bold">{{
          fnum(poStore.orderSummary.totalAfterVat - poStore.orderSummary.bonusAmount, 0)
        }}</span>
      </div>
      <div class="mb-3">
        <Button
          @click="paymentDetailDialogRef?.open()"
          label="Chi tiết thanh toán"
          text
          severity="info"
          class="p-0 font-light"
          size="small"
          v-if="poStore.model.itemDetail.length"
        />
        <div v-else class="mb-5 pb-1"></div>
      </div>
      <div class="flex gap-2 mb-3" v-if="props.isClient">
        <Checkbox
          v-model="isConfirmed"
          @change="onChangeConfirm"
          inputId="confirmTerm"
          binary
        ></Checkbox>
        <label for="confirmTermx">
          {{ t("client.agree_read_and_accept") }}
          <span @click="onClickTerm" class="text-blue-500 cursor-pointer hover:underline">
            {{ t("client.personal_data_notice") }}</span
          >
          {{ t("client.company_suffix") }}
        </label>
      </div>
    </div>
    <Button
      :loading="loading"
      :disabled="
        !poStore.model.cardId ||
        !poStore.model.itemDetail.length ||
        poStore.disableOrderButton ||
        (!isConfirmed && props.isClient)
      "
      iconPos="right"
      label="Đặt hàng"
      icon="fa-solid fa-cart-shopping"
      class="w-full p-3 text-xl"
      @click="onCreateOrder"
    />

    <PaymentDetailDialog ref="paymentDetailDialogRef"></PaymentDetailDialog>
    <PDFView
      v-model:visible="visibleTerm"
      v-if="visibleTerm"
      :url="termData?.filePath"
      :header="termData?.name"
    >
    </PDFView>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, watch, onMounted, nextTick } from "vue";
  import { fnum, createPurchaseOrder, clearCartItem, checkPayment } from "../script";
  import { usePoStore } from "../store/purchaseStore.store";
  import { useToast } from "primevue/usetoast";
  import { useRouter } from "vue-router";
  import API from "@/api/api-main";
  //Components
  import PaymentDetailDialog from "../dialogs/PaymentDetailDialog.vue";
  const paymentDetailDialogRef = ref<InstanceType<typeof PaymentDetailDialog>>();
  import VoucherSelect from "./VoucherSelect.vue";
  import PDFView from "@/components/PDFViewer/PDFView.vue";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();

  const props = defineProps({
    isClient: {
      type: Boolean,
      default: false,
    },
  });

  const toast = useToast();
  const router = useRouter();
  const poStore = usePoStore();
  const isConfirmed = ref(false);

  const termData = ref<any>({});
  const visibleTerm = ref(false);
  const onClickTerm = () => {
    API.get("article?Page=1&PageSize=1&Filter=status=A")
      .then((res) => {
        termData.value = res.data.item?.[0] || {};
        visibleTerm.value = true;
      })
      .catch((error) => {});
  };

  const onChangeConfirm = () => {
    localStorage.setItem("ACCEPT-PURCHASE-POLICY", JSON.stringify(isConfirmed.value));
  };

  const loading = ref(false);
  const onCreateOrder = () => {
    nextTick(() => {
      try {
        let bonus = poStore.orderSummary.bonusPercent * 100;
        let bonusAmount = poStore.orderSummary.bonusAmount;
        let total = poStore.orderSummary.totalAfterVat; //- bonus
        let totalBeforeVatnumber = poStore.orderSummary.totalBeforeVat;
        let vatAmount = poStore.orderSummary.vatAmount;
        const payload = poStore.model.toPayload(
          bonus,
          bonusAmount,
          total,
          totalBeforeVatnumber,
          vatAmount
        );

        if (payload.address.length < 2) {
          poStore.activeIndexTab = 1;
          toast.add({
            severity: "warn",
            detail: t("Custom.order_warning"),
            summary: t("Custom.warning"),
            life: 3000,
          });
          return;
        }
        loading.value = true;
        createPurchaseOrder(payload)
          .then((res) => {
            toast.add({
              severity: "success",
              summary: t("body.systemSetting.success_label"),
              detail: `${t("Custom.order_success")}`,
              life: 5000,
            });
            // Sau khi tạo đơn hàng thành công thì
            if (props.isClient) {
              // nếu là bên khách hàng thì xóa giỏ hàng đi
              clearCartItem();
              router.replace({
                name: "hisPur",
              });
              return; // Lúc tạo xong không gọi payment nữa
              const paynowAfterDiscount =
                poStore.orderSummary.totalPayNow - poStore.orderSummary.bonusAmount; // Số tiền thanh toán ngay sau khi trừ thưởng TTN
              // Nếu đơn hàng có thanh toán ngay thì điều hướng đến OnePay
              if (paynowAfterDiscount > 0) {
                // Lấy thông tin thanh toán
                const payData = {
                  docId: res.data.id,
                  paymentAmount: paynowAfterDiscount + "00",
                  paymentMethodId: 0,
                };
                checkPayment(payData)
                  .then((res) => {
                    // chuyển hướng đến trang thanh toán
                    const redirectLink = res.data.redirectLink;
                    if (redirectLink) {
                      // window.open(redirectLink, "_self");
                    } else {
                      toast.add({
                        severity: "error",
                        summary: "Đã có lỗi xảy ra",
                        detail: `Không thể chuyển hướng đến trang thanh toán, vui lòng liên hệ với quản trị viên để được hỗ trợ.`,
                        life: 10000,
                      });
                    }
                  })
                  .catch((error) => {
                    toast.add({
                      severity: "error",
                      summary: "Đã có lỗi xảy ra",
                      detail: `Không thể thực hiện thanh toán, vui lòng thử lại sau.`,
                      life: 10000,
                    });
                  });
              }
              // nếu không thì quay trở lại danh sách đơn hàng
              router.replace({
                name: "hisPur",
              });
            } else {
              // nếu là bên admin thì router back lại danh sách đơn hàng
              router.replace({
                name: "purchaseList",
              });
            }
          })
          .catch((error) => {
            toast.add({
              severity: "error",
              summary: t("Custom.error"),
              detail: `${t("Custom.error_occurred")}`,
              life: 5000,
            });
            console.error(error);
          })
          .finally(() => {
            loading.value = false;
          });
      } catch (error) {
        toast.add({
          severity: "error",
          summary: t("Custom.error"),
          detail: `${t("Custom.error_occurred")}`,
          life: 5000,
        });
        console.error(error);
      }
    });
  };

  const lists = ref([
    {
      label: t("body.OrderList.order_value_before_discount"),
      value: 0,
      field: "totalBeforeVat",
    },
    {
      label: t("body.OrderList.other_promotions_value"),
      value: 0,
      field: "TotalDiscount",
    },
    {
      label: t("body.OrderList.commitment_bonus"),
      value: 0,
      field: "bonusCommited",
    },
    {
      label: t("body.OrderList.payment_bonus"),
      value: 0,
      field: "bonusAmount",
      divider: true,
      paynow: true,
    },
    {
      label: t("body.OrderList.order_value_after_discount"),
      value: 0,
      field: "totalBeforeVat_bonusCommited",
    },
    {
      label: t("body.OrderList.total_tax_amount"),
      value: 0,
      field: "vatAmount",
      divider: true,
    },
    {
      label: t("body.OrderList.cash_payment_option"),
      value: 0,
      field: "totalPayNow",
    },
    {
      label: t("body.OrderList.credit_debt_option"),
      value: 0,
      field: "totalDebt",
    },
    {
      label: t("body.OrderList.guaranteed_debt_option"),
      value: 0,
      field: "totalDebtGuarantee",
      divider: true,
    },
  ]);

  const roundNumber = (num: number) => {
    return Math.round(num * 100) / 100;
  };

  watch(
    () => JSON.stringify([poStore.orderSummary]),
    () => {
      lists.value.map((item) => {
        if (item.field == "totalBeforeVat_bonusCommited") {
          item.value = roundNumber(
            poStore.orderSummary["totalBeforeVat"] - poStore.orderSummary["bonusCommited"]
          );
        } else {
          item.value = roundNumber(
            poStore.orderSummary[item.field as keyof typeof poStore.orderSummary] || 0
          );
        }
        return item;
      });
    }
  );

  const initialComponent = () => {
    // code here
    localStorage.getItem("ACCEPT-PURCHASE-POLICY") &&
      (isConfirmed.value = JSON.parse(
        localStorage.getItem("ACCEPT-PURCHASE-POLICY") || "false"
      ));
  };

  onMounted(function () {
    initialComponent();
  });
</script>

<style scoped></style>
