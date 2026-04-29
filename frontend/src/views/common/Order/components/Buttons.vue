<template>
  <div>
    <hr v-if="buttons.some((item) => item.visible)" />
    <div class="flex gap-2 justify-content-end">
      <template v-for="(btn, i) in buttons.filter((btn) => btn.visible)" :key="i">
        <Button
          :icon="btn.icon"
          :label="btn.label"
          :severity="btn.severity"
          :disabled="btn.disabled"
          @click="
            async () => {
              await onButtonClick(i, btn.onClick);
            }
          "
          :loading="loading[i]"
        />
      </template>
    </div>
    <YCBSDialog ref="YCBSDialogRef" />
    <HinhThucThanhToanDialog ref="htttDialogRef" type="ORDER" />
    <CancelInvoice ref="CancelInvoiceRef" />
    <Dialog
      v-model:visible="confirmDialogVisible"
      :header="t('Custom.orderConfirmation')"
      :modal="true"
      :closable="false"
      :style="{ width: '500px' }"
    >
      <div class="confirmation-content">
        <div class="flex align-items-center gap-3 mb-4">
          <i
            class="pi pi-exclamation-triangle text-yellow-500"
            style="font-size: 3rem"
          ></i>
          <span class="font-semibold text-xl">{{ t("Custom.confirmOrderMessage") }}</span>
        </div>
        <div class="ml-6">
          <div class="mb-3">
            <span class="font-semibold">{{ t("Custom.orderCode") }}:</span>
            <span class="ml-2">{{ odStore.order?.invoiceCode }}</span>
          </div>
          <div class="mb-3">
            <span class="font-semibold">{{ t("body.OrderList.customer") }}:</span>
            <span class="ml-2">{{ odStore.order?.cardName }}</span>
          </div>
          <div class="mb-3">
            <span class="font-semibold">{{ t("Custom.totalPayment") }}:</span>
            <span class="ml-2 text-green-600 font-bold">
              {{
                formatCurrency(
                  (odStore.order?.paymentInfo?.totalAfterVat || 0) -
                    (odStore.order?.paymentInfo?.bonusAmount || 0)
                )
              }}
            </span>
          </div>
        </div>
        <div class="mt-4 p-3 bg-yellow-50 border-round">
          <p class="m-0 text-sm text-yellow-900">
            <i class="pi pi-info-circle mr-2"></i>
            {{ t("Custom.cannotUndoWarning") }}
          </p>
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('Custom.cancel')"
          icon="pi pi-times"
          @click="confirmDialogVisible = false"
          severity="secondary"
          outlined
        />
        <Button
          :label="t('Custom.confirm')"
          icon="pi pi-check"
          @click="confirmOrder"
          :loading="confirmLoading"
          severity="success"
        />
      </template>
    </Dialog>
    <Dialog
      v-model:visible="confirmPaidDialogVisible"
      :header="t('Custom.confirmPaid')"
      :modal="true"
      :closable="false"
      :style="{ width: '36rem' }"
    >
      <div class="confirmation-content">
        <div class="flex align-items-start gap-3">
          <i
            class="pi pi-check-circle text-green-500"
            style="font-size: 3rem"
          ></i>
          <p class="m-0 line-height-3">
            {{
              t("Custom.confirmPaidMessage", {
                customerName: confirmedPaidCustomerName,
                orderCode: confirmedPaidOrderCode,
                paymentMethod: confirmedPaidMethod,
              })
            }}
          </p>
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('Custom.cancel')"
          icon="pi pi-times"
          @click="confirmPaidDialogVisible = false"
          severity="secondary"
          outlined
        />
        <Button
          :label="t('Custom.confirm')"
          icon="pi pi-check"
          @click="confirmPaid"
          :loading="confirmPaidLoading"
          severity="success"
        />
      </template>
    </Dialog>
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from "vue";
  import { useOrderDetailStore } from "../store/orderDetail";
  import API from "@/api/api-main";
  import { useToast } from "primevue/usetoast";
  import { useRouter } from "vue-router";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  import YCBSDialog from "../dialogs/YCBS.vue";
  import { useConfirm } from "primevue/useconfirm";
  const confirm = useConfirm();
  const YCBSDialogRef = ref<InstanceType<typeof YCBSDialog>>();
  import HinhThucThanhToanDialog from "../dialogs/HinhThucThanhToan.vue";
  const htttDialogRef = ref<InstanceType<typeof HinhThucThanhToanDialog>>();
  import CancelInvoice from "../dialogs/CancelInvoice.vue";
  const CancelInvoiceRef = ref<InstanceType<typeof CancelInvoice>>();
  const odStore = useOrderDetailStore();
  const toast = useToast();

  const props = defineProps({
    isClient: {
      default: false,
    },
  });
  const router = useRouter();

  const loading = ref<{ [key: string]: boolean }>({});
  const confirmDialogVisible = ref(false);
  const confirmLoading = ref(false);
  const confirmPaidDialogVisible = ref(false);
  const confirmPaidLoading = ref(false);

  type ButtonSeverity =
    | "danger"
    | "secondary"
    | "success"
    | "info"
    | "warning"
    | "help"
    | "contrast";

  interface Button {
    label: string;
    severity?: ButtonSeverity;
    onClick?: Function;
    visible?: boolean;
    disabled?: boolean;
    icon?: string;
  }

  // Format currency
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(value);
  };

  const confirmOrder = async () => {
    confirmLoading.value = true;
    try {
      await API.add(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/send-payment`
      );
      toastService(
        "success",
        t("body.systemSetting.success_label"),
        t("Custom.OrderConfirmed")
      );
      confirmDialogVisible.value = false;
      odStore.fetchStore();
    } catch (err) {
      console.error(err);
    } finally {
      confirmLoading.value = false;
    }
  };

  const confirmPaid = async () => {
    confirmPaidLoading.value = true;
    try {
      await API.update(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/change-status/UNC`
      );
      toastService(
        "success",
        t("paymentMethod.success"),
        t("paymentMethod.paymentConfirmed")
      );
      confirmPaidDialogVisible.value = false;
      odStore.fetchStore();
    } catch (err) {
      toastService(
        "error",
        t("paymentMethod.error"),
        t("paymentMethod.somethingWentWrong")
      );
    } finally {
      confirmPaidLoading.value = false;
    }
  };

  // Button click handlers
  const huyDonHang = async () => {
    try {
      await API.update(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/change-status/HUY2`
      );
      toastService(
        "success",
        t("body.systemSetting.success_label"),
        "Đã cập nhật trạng thái đơn hàng"
      );
      odStore.fetchStore();
    } catch (err) {
      toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
    }
  };

  const xacNhanDonHang = async () => {
    confirmDialogVisible.value = true;
  };

  const xacNhanDaThanhToan = () => {
    confirmPaidDialogVisible.value = true;
  };

  const yeuCauBoSung = () => {
    YCBSDialogRef.value?.open();
  };

  const pheDuyet = async () => {
    try {
      const res = await API.add(`ApprovalWorkFlow`, {
        docId: odStore.order?.id,
        docType: 1, // 1: Đơn hàng mua 2: YCLHG
      });

      if (res.status === 200 && res.data.id) {
        router.push(`/approval-setup/order-approval/${res.data.id}`);
      } else {
        toastService("error", t("Custom.error"), res.data.errors || res.data.error);
      }
    } catch (error) {
      toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
    }
  };
  const xacNhan = async () => {
    confirm.require({
      message: t("Notification.confirmOrderMessage"),
      header: t("Notification.confirm"),
      icon: "pi pi-exclamation-triangle",
      acceptLabel: t("Notification.confirm"),
      rejectLabel: t("Notification.cancel"),
      accept: async () => {
        try {
          await API.update(
            `${
              odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"
            }${odStore.order?.id}/change-status/DXN`
          );
          toastService(
            "success",
            t("body.systemSetting.success_label"),
            t("Custom.OrderConfirmed")
          );
          odStore.fetchStore();
        } catch (err) {
          toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
        }
      },
    });
  };
  const giaoHang = async () => {
    try {
      // Không qua bước đang giao nữa mà trực tiếp thành trạng th ái hoàn thành
      const body = new FormData();
      odStore.order?.attDocuments.forEach((el) => {
        if (el._file) body.append("AttachFile", el._file);
      });
      await API.update(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/change-status/DHT`,
        body
      );
      toastService(
        "success",
        t("body.systemSetting.success_label"),
        t("Custom.deliveryConfirmed")
      );
      odStore.fetchStore();
    } catch (err) {
      toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
    }
  };
  const daNhanHang = async () => {
    try {
      await API.update(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/change-status/DHT`
      );
      toastService(
        "success",
        t("body.systemSetting.success_label"),
        t("Custom.OrderConfirmed")
      );
      odStore.fetchStore();
    } catch (err) {
      toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
    }
  };
  const daGiaoHang = async () => {
    try {
      await API.update(
        `${odStore?.order?.docType == "NET" ? "PurchaseOrderNet/" : "PurchaseOrder/"}${
          odStore.order?.id
        }/change-status/DGHR`
      );
      toastService(
        "success",
        t("body.systemSetting.success_label"),
        t("Custom.deliverySuccessful")
      );
      odStore.fetchStore();
    } catch (err) {
      toastService("error", t("Custom.error"), t("Custom.errorOccurred"));
    }
  };
  const thanhToan = async () => {
    htttDialogRef.value?.open();
    return;
  };
  const cancelInvoice = async () => {
    CancelInvoiceRef.value?.open();
  };

  const confirmedPaidCustomerName = computed(() => {
    return odStore.order?.cardName || odStore.customer?.cardName || "";
  });

  const confirmedPaidOrderCode = computed(() => {
    return odStore.order?.invoiceCode || "";
  });

  const confirmedPaidMethod = "UNC/Chuyển khoản ngân hàng";

  const toNumber = (value: unknown) => {
    const parsedValue = Number(value);
    return Number.isFinite(parsedValue) ? parsedValue : 0;
  };

  const isInstantPaymentOrder = computed(() => {
    const order = odStore.order as any;
    const paymentInfo = order?.paymentInfo || {};
    const paymentMethods = Array.isArray(order?.paymentMethod)
      ? order.paymentMethod
      : Array.isArray(order?.payments)
        ? order.payments
        : [];
    const itemDetails = Array.isArray(order?.itemDetail) ? order.itemDetail : [];
    const paymentCodes = [
      ...paymentMethods.map((item: any) => item?.paymentMethodCode || item?.payMethod),
      ...itemDetails.map((item: any) => item?.paymentMethodCode),
    ].filter(Boolean);

    const hasOnlyPayNowCode =
      paymentCodes.length > 0 && paymentCodes.every((code) => code === "PayNow");
    const hasPayNowTotal = toNumber(paymentInfo.totalPayNow) > 0;
    const hasOtherPaymentTotal =
      toNumber(paymentInfo.totalDebt) > 0 ||
      toNumber(paymentInfo.totalDebtGuarantee) > 0 ||
      toNumber(paymentInfo.totalVisa) > 0;

    return (hasOnlyPayNowCode || hasPayNowTotal) && !hasOtherPaymentTotal;
  });

  const buttons = computed<Button[]>(() => {
    return [
      {
        label: t("Custom.cancelOrder"),
        severity: "danger",
        visible: isVisible(["TTN", "CXN", "DXL", "CTT"], !props.isClient),
        onClick: cancelInvoice,
      },
      {
        label: t("Custom.confirmPaid"),
        severity: "success",
        icon: "pi pi-check-circle",
        visible: isVisible(["CTT"], !props.isClient && isInstantPaymentOrder.value),
        onClick: xacNhanDaThanhToan,
      },
      {
        label: t("Custom.confirmOrder"),
        visible: isVisible(["TTN"], !props.isClient),
        onClick: xacNhanDonHang,
      },
      {
        label: t("Custom.supplementRequest"),
        severity: "help",
        visible: isVisible(["DXL"], odStore.isExceedDebt && !props.isClient),
        disabled: odStore.order?.approval?.status == "A",
        onClick: yeuCauBoSung,
      },
      {
        label: t("Custom.approve"),
        visible: isVisible(["DXL"], odStore.isExceedDebt && !props.isClient),
        disabled:
          odStore.order?.attachFile.some((row) => !row.filePath) ||
          (odStore.order?.attachFile.length || 0) < 1,
        onClick: pheDuyet,
      },
      {
        label: t("Custom.confirmToSAP"),
        visible: isVisible(["DTT", "CXN"]) && !props.isClient,
        disabled: odStore.order?.attachFile.some((row) => !row.filePath),
        onClick: xacNhan,
      },
      {
        label: t("Custom.complete"),
        // severity: 'success',
        // icon: 'fa-solid fa-truck-fast',
        visible: isVisible(["DGH"], !props.isClient),
        onClick: giaoHang,
        disabled: (odStore.order?.attDocuments.length || 0) < 1,
      },
      {
        label: t("Custom.payment"),
        visible: isOrderStatusVisible(["CTT"], props.isClient),
        icon: "fa-solid fa-money-check-dollar",
        onClick: thanhToan,
      },
      {
        label: t("Custom.complete"),
        visible: isVisible(["DGHR"]),
        onClick: daNhanHang,
      },
    ] as Button[];
  });

  const onButtonClick = async (index: number, callback: Function | undefined) => {
    Object.keys(loading.value).forEach((key) => {
      loading.value[key] = false;
    });
    if (callback) {
      loading.value[index] = true;
      await callback();
      loading.value[index] = false;
    }
  };

  // stt: Trạng thái đơn hàng mà nút sẽ được hiện ra
  // except: trạng thái bổ sung
  const isVisible = (stt: string[], except = true): boolean => {
    return stt.includes(odStore.order?.status || "") && except;
  };

  const isOrderStatusVisible = (stt: string[], except = true): boolean => {
    return stt.includes(odStore.order?.status || "") && except;
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

  const initialComponent = () => {
    // code here
  };

  onMounted(function () {
    initialComponent();
  });
</script>

<style scoped>
  .confirmation-content {
    padding: 1rem;
  }
</style>
