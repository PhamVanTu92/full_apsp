<script setup>
  import { ref, onBeforeMount, watch } from "vue";
  import { useRoute } from "vue-router";
  import API from "@/api/api-main";
  import format from "@/helpers/format.helper";
  import { useGlobal } from "@/services/useGlobal";
  import router from "@/router";
  import ModalEditAddress from "./ModalEditAddress.vue";
  import { useI18n } from "vue-i18n";
  import { useConfirm } from "primevue/useconfirm";
  import { useUserInfoStore } from "@/Pinia/user_info/userInfo";
  import Notes from "./Notes.vue";
  import DanhGiaDonHang from "../../common/Order/components/DanhGiaDonHang.vue";
  const userInfoStore = useUserInfoStore();
  const { t } = useI18n();
  const editCTGH = ref(false);
  const { toast, FunctionGlobal } = useGlobal();
  const statusDebt = ref(false);
  const visible = ref(false);
  const Route = useRoute();
  const Customer = ref({});
  const DataProduct = ref({});
  const isLoading = ref(false);
  const confirm = useConfirm();
  const cancelReason = ref("");
  const disabledNote = ref(true);
  const visibleModal = ref(false);
  const odStore = ref();
  const ratingDialogVisible = ref(false);
  onBeforeMount(() => {
    fetchOrderReqDetail();
  });
  const fetchOrderReqDetail = async () => {
    isLoading.value = true;
    try {
      const res = await API.get(`PurchaseRequest/${Route.params.id}`);
      odStore.value = res.data.item;
      if (res.data) {
        DataProduct.value = res.data.item;
        isLoading.value = false;
        fetchCustomer();
      }
    } catch (error) {
      console.error(error);
    }
  };
  const fetchCustomer = async () => {
    try {
      const res = await API.get(`Customer/${DataProduct.value.cardId}`);
      Customer.value = res.data;
    } catch (error) {
      console.error(error);
    }
  };

  const ApproveOrder = async (id) => {
    try {
      const res = await API.add(`ApprovalWorkFlow`, {
        docId: id,
        docType: 2, // 1: Đơn hàng mua 2: YCLHG
      });
      if (res.status === 200 && res.data.id) {
        router.push(`/approval-setup/order-approval/${res.data.id}`);
      } else {
        FunctionGlobal.$notify("E", res.data.errors || res.data.error, toast);
      }
    } catch (error) {
      FunctionGlobal.$notify("E", error.response?.data?.error || error.message, toast);
    }
  };
  const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
  };
  let stt = {
    DXL: {
      class: "text-yellow-500",
      label: t("body.status.DXL"),
    },
    DXN: {
      class: "text-green-500",
      label: t("body.status.DXN"),
    },
    HUY: {
      class: "text-red-500",
      label: t("body.status.HUY2"),
    },
    DGH: {
      class: "text-blue-500",
      label: t("body.status.DGH"),
    },
    DHT: {
      class: "text-green-700",
      label: t("body.status.DHT"),
    },
    CXN: {
      class: "text-orange-700 bg-white",
      label: t("body.status.CXN"),
    },
  };
  const formatStatus = (data) => {
    return (
      stt[data] || {
        class: "text-yellow-500",
        label: t("body.status.ERR"),
      }
    );
  };
  const onClickConfirmOrder = (stt) => {
    confirm.require({
      message: t("body.status.confirm_pickup_request"),
      header: t("body.home.confirm_button"),
      rejectClass: "p-button-secondary",
      rejectlabel: t("body.status.DONG"),
      acceptLabel: t("body.home.confirm_button"),
      accept: () => {
        ConfirmOrder("DXN");
      },
    });
  };
  const ConfirmOrder = async (stt) => {
    try {
      const res = await API.update(
        `PurchaseOrder/${Route.params.id}/change-status/${stt}`
      );
      if (res.status == 200) {
        FunctionGlobal.$notify("S", "Xác nhận thành công!", toast);
        fetchOrderReqDetail();
      }
    } catch (error) {
      FunctionGlobal.$notify("E", error, toast);
    }
  };
  const userLocation = (user) => {
    if (!user?.address || user.address.length === 0) {
      return "";
    }
    const locationData = user.address.find((loc) => loc.type === "S");
    if (!locationData) return "";
    return `${locationData.address || ""} - ${locationData.locationName || ""} - ${
      locationData.areaName || ""
    }`;
  };
  watch(Route, () => {
    if (Route.params.id) {
      fetchOrderReqDetail();
    }
  });

  const isExceedDebt = ref(false);
  const getInvoices = () => {
    let result = [];
    let code = DataProduct.value.invoiceCode;
    if (DataProduct.value?.paymentInfo?.totalDebt) {
      result.push({
        invoiceNumber: code,
        paymentMethodName: "Công nợ - Tín chấp",
        amountOverdue: DataProduct.value?.paymentInfo?.totalDebt,
        isInvoice: false,
        status: DataProduct.value?.status,
      });
    }
    if (DataProduct.value?.paymentInfo?.totalDebtGuarantee) {
      result.push({
        invoiceNumber: code,
        paymentMethodName: "Công nợ - Bảo lãnh",
        amountOverdue: DataProduct.value?.paymentInfo?.totalDebtGuarantee,
        isInvoice: false,
        status: DataProduct.value?.status,
      });
    }
    return result;
  };
  // >------------------------------------------------------------------------------<
  const filesInput = ref(null);
  const filesAllow = ["jpg", "jpeg", "png", "pdf", "doc", "docx", "xls", "xlsx", "txt"];
  const onClickFilesChungTuGiaoHang = () => {
    filesInput.value.click();
  };
  const chungChiGiaoHangs = ref([]);
  const onChangeFilesChungTuGiaoHang = async (e) => {
    if (e.target.files.length < 1) return;
    const file = e.target.files[0];
    const fileExtention = file.name.split(".").pop().toLowerCase();
    if (!filesAllow.includes(fileExtention)) {
      FunctionGlobal.$notify(
        "E",
        "File đã chọn không đúng định dạng, vui lòng chọn file khác!",
        toast
      );
      return;
    }
    chungChiGiaoHangs.value.push({
      name: file.name,
      file: file,
      authorName: userInfoStore?.userInfo?.user?.fullName || "",
      uploadFileAt: new Date().toISOString(),
    });
    DataProduct.value.attDocuments.push({
      fileName: file.name,
      authorName: userInfoStore?.userInfo?.user?.fullName || "",
      uploadFileAt: new Date().toISOString(),
      file: file,
    });
    filesInput.value.value = null;
  };

  const onClickGiaoHang = async () => {
    if (chungChiGiaoHangs.value.length < 1) {
      FunctionGlobal.$notify("W", "Vui lòng thêm tài liệu chứng từ giao hàng!", toast);
      return;
    }
    try {
      const body = new FormData();
      if (chungChiGiaoHangs.value.length < 1) {
        FunctionGlobal.$notify("W", "Hãy nhập file chứng từ", toast);
        return;
      }
      chungChiGiaoHangs.value.forEach((el) => {
        body.append("AttachFile", el.file);
      });
      // await API.add(`PurchaseOrder/${Route.params.id}/AttDocuments`, body);
      // await API.get(`PurchaseOrder/${Route.params.id}/change-status/DGH`);
      await API.update(`PurchaseRequest/${Route.params.id}/change-status/DGH`, body);
      fetchOrderReqDetail();
    } catch (error) {
      FunctionGlobal.$notify("E", "Đã có lỗi xảy ra", toast);
    }
  };

  const onClickHoanThanh = async () => {
    confirm.require({
      message: t("Custom.confirm_complete_order"),
      header: t("body.home.confirm_button"),
      icon: "pi pi-exclamation-triangle",
      rejectClass: "p-button-secondary",
      rejectLabel: t("Logout.cancel"),
      acceptLabel: t("body.home.confirm_button"),
      accept: async () => {
        try {
          const body = new FormData();
          chungChiGiaoHangs.value.forEach((el) => {
            body.append("AttachFile", el.file);
          });
          isLoading.value = true;
          await API.update(`PurchaseRequest/${Route.params.id}/change-status/DGH`, body);
          await API.update(`PurchaseRequest/${Route.params.id}/change-status/DHT`);
          FunctionGlobal.$notify("S", t("Custom.complete_order_success"), toast);
          await fetchOrderReqDetail();
          chungChiGiaoHangs.value = [];
        } catch (error) {
          console.error("Error completing order:", error);
          FunctionGlobal.$notify(
            "E",
            error?.response?.data?.message || t("Custom.complete_order_error"),
            toast
          );
        } finally {
          isLoading.value = false;
        }
      },
    });
  };

  const onClickDownloadFile = (file) => {
    var a = document.createElement("a");
    a.href = file.filePath;
    a.target = "_blank";
    a.download = file.fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  };
  const changeStatusModal = () => {
    visibleModal.value = !visibleModal.value;
  };

  const ConfirmCancel = async () => {
    try {
      if (!cancelReason.value) {
        FunctionGlobal.$notify("E", t("Custom.reason_required_enter"), toast);
        return;
      }
      await API.update(
        `${"PurchaseRequest/"}${
          Route?.params?.id
        }/change-status/HUY?reasonForCancellation=${cancelReason.value}`
      );
      FunctionGlobal.$notify("S", t("Custom.success_cancel_oder"), toast);
      fetchOrderReqDetail();
      visible.value = false;
    } catch (error) {
      console.error(error);
      FunctionGlobal.$notify("E", t("Custom.error_cancel_oder"), toast);
    } finally {
      loading.value = false;
    }
  };
  const uploadFileATT = async () => {
    try {
      const body = new FormData();
      for (const el of DataProduct.value.attDocuments) {
        if (!el.id) body.append("files", el.file);
      }
      let res = await API.add(`PurchaseOrder/${Route.params.id}/AttDocuments`, body);
      if (res) FunctionGlobal.$notify("S", t("Notification.update_success"), toast);
      else FunctionGlobal.$notify("E", t("Notification.update_error"), toast);

      editCTGH.value = false;
    } catch (error) {
      console.error(error);
    }
  };

  const onClickSaveNote = async (note) => {
    try {
      let body = { sellerNote: note };
      await API.patch(`PurchaseRequest/${DataProduct.value.id}`, body);
      toast.add({
        severity: "success",
        summary: t("body.systemSetting.success_label"),
        detail: t("Notification.update_success_note"),
        life: 3000,
      });
      disabledNote.value = true;
    } catch (error) {
      console.error(error);
      toast.add({
        severity: "error",
        summary: t("body.report.failures_card"),
        detail: t("Notification.update_error_note"),
        life: 3000,
      });
    } finally {
      loading.value = false;
    }
  };

  const deleteRow = async (data) => {
    try {
      await API.delete(`PurchaseRequest/${DataProduct.value.id}/AttDocuments`, [data.id]);
      toast.add({
        severity: "success",
        summary: t("body.systemSetting.success_label"),
        detail: t("Custom.delete_delivery_document_success"),
        life: 3000,
      });
      fetchOrderReqDetail();
    } catch (error) {
      console.error(error);
      toast.add({
        severity: "error",
        summary: t("body.report.failures_card"),
        detail: t("Custom.delete_delivery_document_error"),
        life: 3000,
      });
    }
  };
</script>
<template>
  <div v-if="DataProduct">
    <div class="grid align-items-center">
      <div class="col-12">
        <div class="flex gap-2 justify-content-between">
          <h4 class="m-0 font-bold">{{ t("Custom.shipmentDetails") }}</h4>
          <div class="flex gap-2">
            <Button
              :label="t('Evaluate.title')"
              icon="pi pi-star"
              severity="success"
              @click="ratingDialogVisible = true"
              v-if="
                DataProduct.status == 'DHT' &&
                DataProduct.ratings?.length == 0 &&
                Route.name != 'DetailPurchaseRequest'
              "
            />
            <ButtonGoBack />
          </div>
        </div>
      </div>
    </div>
    <div v-if="DataProduct.status === 'HUY'" class="cancel-notification mb-2">
      <div class="cancel-card bg-red-100">
        <div class="cancel-header">
          <h3 class="cancel-title">{{ t("CancelOrder.title") }}</h3>
        </div>
        <div class="cancel-content">
          <p class="reason-text">
            <strong>{{ t("ChangePoint.reason") }}:</strong>
            <i>{{ DataProduct.reasonForCancellation }}</i>
          </p>
        </div>
      </div>
    </div>
    <div class="grid">
      <div class="col-12">
        <div class="card">
          <div class="grid justify-content-between">
            <div class="col-6">
              <div class="flex flex-column gap-3">
                <div class="flex align-items-center gap-3">
                  <span>{{ t("body.OrderList.orderCode") }}:</span>
                  <strong>{{ DataProduct.invoiceCode }}</strong>
                </div>
                <div class="flex align-items-center gap-3">
                  <span>{{ t("body.sampleRequest.customer.customer_code_label") }}:</span>
                  <strong>{{
                    DataProduct.cardCode ? DataProduct.cardCode : "--"
                  }}</strong>
                </div>
                <div class="flex align-items-center gap-3">
                  <span>{{ t("body.PurchaseRequestList.customer_label") }}:</span>
                  <span class="text-blue-500">{{
                    DataProduct.cardName ? DataProduct.cardName : "--"
                  }}</span>
                  <DebtCheck
                    ref="debtRef"
                    v-if="false"
                    @checkDebt="statusDebt = $event"
                    :DebtData="Customer?.crD4"
                    :TotalDebtData="Customer?.crD3"
                    type="icon"
                    :paymentMethod="'PayNow'"
                    :TotalPayment="0"
                    :ItemDebt="null"
                  >
                  </DebtCheck>
                  <KiemTraCongNo
                    v-model:isExceedDebt="isExceedDebt"
                    :bpId="DataProduct.cardId"
                    :payCredit="DataProduct?.paymentInfo?.totalDebt || 0"
                    :payGuarantee="DataProduct?.paymentInfo?.totalDebtGuarantee || 0"
                    :invoices="getInvoices()"
                  />
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="flex flex-column gap-3">
                <div class="flex gap-3">
                  <span>{{ t("body.PurchaseRequestList.pickup_time_label") }}:</span
                  ><strong
                    >{{
                      DataProduct.deliveryTime
                        ? format.DateTime(DataProduct.deliveryTime).time +
                          " " +
                          format.DateTime(DataProduct.deliveryTime).date
                        : "--"
                    }}
                  </strong>
                </div>
                <div class="flex gap-3">
                  <span>{{ t("body.PurchaseRequestList.time") }}:</span 
                  ><strong>{{
                    format.DateTime(DataProduct.docDate).time +
                    " " +
                    format.DateTime(DataProduct.docDate).date
                  }}</strong>
                </div>
                <div class="flex gap-3">
                  <span>{{ t("body.OrderApproval.creator") }}:</span
                  ><strong>{{
                    DataProduct.author?.fullName ? DataProduct.author?.fullName : "--"
                  }}</strong>
                </div>
                <div class="flex gap-3">
                  <span>{{ t("body.PurchaseRequestList.status") }}:</span
                  ><span :class="formatStatus(DataProduct.status).class">{{
                    formatStatus(DataProduct.status).label
                  }}</span>
                </div>

                <div class="flex gap-3">
                  <!-- <span>Người liên hệ:</span><span>--</span> -->
                </div>
              </div>
            </div>

            <div class="col-12">
              <DataTable
                showGridlines
                :value="DataProduct.itemDetail"
                tableStyle="min-width: 50rem"
              >
                <Column header="#">
                  <template #body="sp">
                    <span>{{ sp.index + 1 }}</span>
                  </template>
                </Column>
                <Column
                  field="itemCode"
                  :header="t('body.productManagement.product_code_column')"
                >
                </Column>
                <Column
                  field="itemName"
                  :header="t('body.productManagement.product_name_column')"
                >
                </Column>
                <Column
                  field="uomName"
                  :header="t('body.productManagement.unit_name_column')"
                ></Column>
                <Column :header="t('body.PurchaseRequestList.quantity_column')">
                  <template #body="sp">
                    <span>{{ formatNumber(sp.data.quantity) }}</span>
                  </template>
                </Column>
                <Column field="onHand" header="Lượng hàng gửi kho" v-if="0"></Column>
                <!-- <Column header="Đơn giá">
                  <template #body="sp">
                    <span>{{ formatNumber(sp.data.price) }}đ</span>
                  </template>
                </Column>
                <Column header="Giảm giá">
                  <template #body="sp">
                    <span>{{ sp.data.discount }}%</span>
                  </template>
                </Column>
                <Column header="Thuế">
                  <template #body="sp">
                    <span>{{ formatNumber(sp.data.vatAmount) }}đ</span>
                  </template>
                </Column>
                <Column header="Thành tiền">
                  <template #body="sp">
                    <span>{{ formatNumber(sp.data.lineTotal) }}đ</span>
                  </template>
                </Column> -->
              </DataTable>
            </div>
            <!-- <div class="col-6">
                            <h6>{{ t('body.PurchaseRequestList.note_label') }}</h6>
                            <Textarea v-model="DataProduct.note" class="w-full" :disabled="disabledNote"></Textarea>
                            <div class="flex justify-content-end gap-2" v-if="DataProduct.status != 'DHT'">
                                <Button v-if="disabledNote == true" :label="t('body.OrderList.edit')"
                                    @click="editNode(false)" />
                                <div v-else>
                                    <Button :label="t('body.PurchaseRequestList.cancel_button')" @click="editNode(true)"
                                        severity="secondary" />
                                    <Button :label="t('body.PurchaseRequestList.confirm_button')"
                                        @click="onClickSaveNote(DataProduct.note)" class="ml-2" />
                                </div>
                            </div>
                        </div> -->
            <div class="col-12 md:col-8 flex flex-column gap-3">
              <Notes :odStore="odStore" />
            </div>
            <div class="col-12" v-if="DataProduct.status == 'DXN'">
              <DataTable :value="chungChiGiaoHangs" showGridlines>
                <template #header>
                  <div class="font-bold text-lg my-2">
                    {{ t("body.OrderApproval.documentName") }}
                  </div>
                </template>
                <template #footer>
                  <Button
                    @click="onClickFilesChungTuGiaoHang"
                    icon="pi pi-plus"
                    :label="t('body.PurchaseRequestList.add_new_button')"
                  />
                  <input
                    @change="onChangeFilesChungTuGiaoHang"
                    ref="filesInput"
                    type="file"
                    class="hidden"
                  />
                </template>
                <Column header="#" class="w-3rem">
                  <template #body="sp">
                    <span>{{ sp.index + 1 }}</span>
                  </template>
                </Column>
                <Column :header="t('body.OrderApproval.documentName')">
                  <template #body="sp">
                    {{ sp.data.name }}
                  </template>
                </Column>
                <Column
                  :header="t('body.OrderApproval.creator')"
                  class="w-20rem"
                  field="authorName"
                >
                </Column>
                <Column :header="t('body.OrderApproval.createDate')" class="w-20rem">
                  <template #body="sp">
                    {{ format.DateTime(sp.data.uploadFileAt)?.dateTime }}
                  </template>
                </Column>
                <Column class="w-3rem">
                  <template #body="sp">
                    <Button
                      @click="chungChiGiaoHangs.splice(sp.index, 1)"
                      icon="pi pi-trash"
                      severity="danger"
                      text
                    />
                  </template>
                </Column>
                <template #empty>
                  <div class="p-5 my-5 text-center">
                    {{ t("client.delivery_documents_doc") }}
                  </div>
                </template>
              </DataTable>
            </div>
            <div class="col-12" v-if="['DGH', 'DHT'].includes(DataProduct.status)">
              <DataTable :value="DataProduct.attDocuments || []" showGridlines>
                <Column header="#" class="w-3rem">
                  <template #body="sp">
                    <span>{{ sp.index + 1 }}</span>
                  </template>
                </Column>
                <Column header="Tệp đính kèm" field="fileName" />
                <Column field="authorName" header="Người tạo" class="w-20rem"></Column>
                <Column header="Ngày tạo" class="w-10rem">
                  <template #body="sp">
                    {{ format.DateTime(sp.data.uploadFileAt)?.date }}
                  </template>
                </Column>
                <Column class="w-3rem">
                  <template #body="sp">
                    <div class="flex gap-2">
                      <Button
                        :disabled="!sp.data.filePath"
                        @click="onClickDownloadFile(sp.data)"
                        icon="pi pi-download"
                        text
                      />
                      <Button
                        icon="pi pi-trash"
                        text
                        severity="danger"
                        v-if="DataProduct.status != 'DHT' && editCTGH"
                        @click="deleteRow(sp.data)"
                      />
                    </div>
                  </template>
                </Column>
                <template #empty>
                  <div class="p-5 my-5 text-center">
                    {{ t("client.delivery_documents_doc") }}
                  </div>
                </template>
                <template #header>
                  <div class="flex justify-content-between align-items-center">
                    <span class="my-2 text-lg font-bold">
                      {{ t("client.delivery_documents") }}
                    </span>
                    <div v-if="DataProduct.status != 'DHT'">
                      <Button
                        icon="pi pi-times"
                        :label="t('client.cancel')"
                        severity="secondary"
                        @click="editCTGH = false"
                        v-if="editCTGH"
                        class="mr-2"
                      />
                      <Button
                        icon="pi pi-pencil"
                        :label="t('client.edit')"
                        severity="secondary"
                        @click="editCTGH = true"
                        v-if="!editCTGH"
                      />
                      <Button
                        icon="pi pi-save"
                        :label="t('client.save')"
                        v-if="editCTGH"
                        @click="uploadFileATT()"
                      />
                    </div>
                  </div>
                </template>
                <template #footer v-if="editCTGH">
                  <div>
                    <Button
                      icon="pi pi-upload"
                      :label="t('body.PurchaseRequestList.add_new_button')"
                      severity="primary"
                      @click="onClickFilesChungTuGiaoHang"
                    />
                    <input
                      @change="onChangeFilesChungTuGiaoHang"
                      ref="filesInput"
                      type="file"
                      class="hidden"
                    />
                  </div>
                </template>
              </DataTable>
            </div>
          </div>
          <hr v-if="DataProduct.status != 'DHT' && DataProduct.status != 'HUY'" />
          <div v-if="DataProduct.status != 'DHT' && DataProduct.status != 'HUY'">
            <div class="flex gap-3 justify-content-end">
              <AdditionalRequest
                v-if="['DXL'].includes(DataProduct.status)"
                :disabled="DataProduct.approval?.status == 'A'"
                @onUpdate="fetchOrderReqDetail()"
                :endpoint="`PurchaseOrder/${Route.params.id}`"
              />
              <!-- (!DataProduct.attachFile?.length > 0 ||
                                        !DataProduct.attachFile?.some((el) => el.filePath)) || -->
              <Button
                v-if="['DXL'].includes(DataProduct.status)"
                :disabled="
                  (isExceedDebt &&
                    !DataProduct.attachFile?.some((item) => item.filePath)) ||
                  DataProduct.approval?.status == 'A'
                "
                @click="ApproveOrder(DataProduct.id)"
                label="Phê duyệt"
              >
              </Button>
              <!-- <Button label="Huỷ YCLH" severity="danger" /> -->
              <Button
                v-if="DataProduct.status == 'DXL' || DataProduct.status == 'CXN'"
                @click="visible = true"
                severity="danger"
                :label="t('Custom.cancelOrder')"
              />
              <Button
                @click="onClickConfirmOrder()"
                v-if="['CXN'].includes(DataProduct?.status)"
                :disabled="DataProduct.approval && DataProduct.approval?.status !== 'A'"
                :label="t('body.PurchaseRequestList.confirm_sap')"
              />
              <!-- ----------------------------------------------- -->
              <!-- <Button @click="onClickGiaoHang" v-if="['DXN'].includes(DataProduct.status)"
                                icon="fa-solid fa-truck-fast" severity="info" label="Giao hàng"
                                :disabled="!chungChiGiaoHangs.length"/> -->
              <Button
                @click="onClickHoanThanh"
                v-if="
                  ['DGH'].includes(DataProduct.status) ||
                  ['DXN'].includes(DataProduct.status)
                "
                icon="fa-solid fa-check"
                severity="primary"
                label="Hoàn thành"
                :disabled="!DataProduct.attDocuments.length"
              />
              <!-- ----------------------------------------------- -->
            </div>
          </div>
        </div>
      </div>
      <div class="col-12">
        <DocumentsComp
          @callAPI="fetchOrderReqDetail()"
          :data="DataProduct"
        ></DocumentsComp>
      </div>

      <div class="col-12" v-if="DataProduct.ratings?.length > 0">
        <div class="card">
          <DanhGiaDonHang
            type="preview"
            :orderId="DataProduct.id"
            :initialData="DataProduct.ratings"
            docType="YCLHG"
          />
        </div>
      </div>

      <div class="col-12">
        <div class="card p-0 h-full">
          <div class="text-xl font-bold pt-3 px-3">{{ t("client.customer_info") }}</div>
          <hr />
          <div class="grid px-3">
            <div class="col-6">
              <div class="card m-0 flex flex-column p-0">
                <div class="p-3 font-bold">
                  {{ t("body.PurchaseRequestList.customer_label") }}
                </div>
                <hr class="m-0" />
                <div class="flex flex-column gap-3 p-3">
                  <div class="flex gap-3">
                    <span>{{ t("body.PurchaseRequestList.customer_label") }}:</span
                    ><strong>{{ Customer.cardName ? Customer.cardName : "--" }}</strong>
                  </div>
                  <div class="flex gap-3">
                    <span>{{ t("body.sampleRequest.customer.tax_code_label") }}:</span
                    ><strong
                      >{{ Customer.licTradNum ? Customer.licTradNum : "--" }}
                    </strong>
                  </div>
                  <div class="flex gap-3">
                    <span>{{ t("body.sampleRequest.customer.phone_label") }}:</span
                    ><strong>
                      {{ Customer.phone ? Customer.phone : "--" }}
                    </strong>
                  </div>
                  <div class="flex gap-3">
                    <span>{{ t("body.sampleRequest.customer.email_label") }}:</span
                    ><strong> {{ Customer.email ? Customer.email : "--" }}Ư </strong>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="card p-0 h-full">
                <div class="p-3 flex justify-content-between">
                  <div class="font-bold">
                    {{ t("body.PurchaseRequestList.shipping_info_tab") }}
                  </div>
                  <i
                    class="pi pi-pencil cursor-pointer text-green-500"
                    v-tooltip.top="t('client.edit')"
                    @click="changeStatusModal()"
                    v-if="DataProduct.status != 'HUY' && DataProduct.status != 'DHT'"
                  />
                </div>
                <hr class="m-0" />
                <div>
                  <div class="flex flex-column gap-1 p-3">
                    <div class="flex gap-3">
                      <span
                        >{{
                          t("body.sampleRequest.customer.contact_person_name_column")
                        }}:</span
                      >
                      <strong>
                        {{
                          DataProduct?.address?.filter((el) => el.type == "S")[0]?.person
                        }}
                      </strong>
                    </div>
                    <div class="flex gap-3">
                      <span
                        >{{
                          t("body.sampleRequest.customer.phone_number_1_column")
                        }}:</span
                      ><strong>
                        {{
                          DataProduct?.address?.filter((el) => el.type == "S")[0]?.phone
                        }}
                      </strong>
                    </div>
                    <div class="flex gap-3">
                      <!-- <pre>{{DataProduct?.address}}</pre> -->
                      <span>{{ t("body.sampleRequest.customer.id_card_column") }}: </span
                      ><strong>
                        {{
                          DataProduct?.address?.filter((el) => el.type == "S")[0]?.cccd
                        }}
                      </strong>
                    </div>
                    <div class="flex gap-3">
                      <span
                        >{{
                          t("body.sampleRequest.customer.license_plate_column")
                        }}:</span
                      ><strong>
                        {{
                          DataProduct?.address?.filter((el) => el.type == "S")[0]
                            ?.vehiclePlate
                        }}
                      </strong>
                    </div>
                    <div class="flex gap-3">
                      <span>{{ t("body.sampleRequest.customer.address_column") }}:</span
                      ><strong>
                        {{ userLocation(DataProduct) }}
                      </strong>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="visible"
    class="w-30rem"
    modal
    @hide="onHide"
    :closable="true"
    :draggable="false"
  >
    <template #header>
      <div class="dialog-header">
        <div class="header-icon">
          <i class="pi pi-exclamation-triangle text-orange-500"></i>
        </div>
        <div class="header-content">
          <h3 class="header-title">{{ t("Custom.confirm_cancel_oder_YCLHG") }}</h3>
          <p class="header-subtitle">{{ t("Custom.enter_reason") }}</p>
        </div>
      </div>
    </template>

    <div class="dialog-content">
      <div class="form-group">
        <label for="cancelReason" class="form-label">
          <i class="pi pi-comment mr-2"></i>
          {{ t("Custom.reason_yclhg") }}
          <span class="required-star">*</span>
        </label>
        <Textarea
          id="cancelReason"
          v-model="cancelReason"
          class="cancel-reason-input"
          :placeholder="t('Custom.reason_required_enter')"
          rows="4"
          :autoResize="false"
          :maxlength="1000"
        />
        <small class="character-count"
          >{{ cancelReason.length }}/1000 {{ t("Custom.characters") }}</small
        >
        <!-- <small v-if="!cancelReason" class="error-message">
                    <i class="pi pi-exclamation-circle mr-1"></i>
                    {{ t('Custom.reason_required') }}
                </small> -->
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2 w-full">
        <Button
          :label="t('Logout.cancel')"
          @click="visible = false"
          icon="pi pi-times"
          severity="secondary"
        />
        <Button :label="t('client.confirm')" icon="pi pi-check" @click="ConfirmCancel" />
      </div>
    </template>
  </Dialog>
  <Dialog
    v-model:visible="ratingDialogVisible"
    modal
    :header="t('Evaluate.evaluateOrder')"
    :style="{ width: '50rem' }"
  >
    <DanhGiaDonHang :type="'vote'" :orderId="DataProduct.id" :docType="'YCLHG'" />
  </Dialog>
  <ConfirmDialog></ConfirmDialog>
  <ModalEditAddress
    :dataAddress="DataProduct"
    :visibleModal="visibleModal"
    :paramID="Route.params.id"
    @change="changeStatusModal()"
  ></ModalEditAddress>
  <loading v-if="isLoading"></loading>
</template>

<style scoped>
  .cancel-notification {
    margin: 12px 0;
  }

  .cancel-card {
    background: #fecaca;
    border-radius: 8px;
    padding: 12px;
    border-left: 4px solid #ef4444;
  }

  .cancel-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
  }

  .cancel-title {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: #7f1d1d;
  }

  .cancel-content {
    margin: 0;
  }

  .cancel-invoice-dialog {
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  }

  .dialog-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 0.5rem 0;
  }

  .header-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 3rem;
    height: 3rem;
    background: linear-gradient(135deg, #fef3cd, #fff3cd);
    border-radius: 50%;
    border: 2px solid #fbbf24;
  }

  .header-icon i {
    font-size: 1.25rem;
  }

  .header-content {
    flex: 1;
  }

  .header-title {
    margin: 0 0 0.25rem 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--text-color);
  }

  .header-subtitle {
    margin: 0;
    font-size: 0.875rem;
    color: var(--text-color-secondary);
    line-height: 1.4;
  }

  .dialog-content {
    padding: 1.5rem 0;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  .form-label {
    display: flex;
    align-items: center;
    font-weight: 500;
    color: var(--text-color);
    font-size: 0.9rem;
  }

  .required-star {
    color: #ef4444;
    margin-left: 0.25rem;
  }

  .cancel-reason-input {
    border-radius: 8px;
    border: 2px solid var(--surface-border);
    transition: all 0.2s ease;
    font-size: 0.9rem;
    padding: 0.75rem;
    resize: none;
  }

  .cancel-reason-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb), 0.1);
  }

  .cancel-reason-input.p-invalid {
    border-color: #ef4444;
    background-color: #fef2f2;
  }

  .cancel-reason-input.p-invalid:focus {
    border-color: #ef4444;
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
  }

  .character-count {
    text-align: right;
    color: var(--text-color-secondary);
    font-size: 0.75rem;
    margin-top: -0.5rem;
  }

  .error-message {
    color: #ef4444;
    font-size: 0.8rem;
    display: flex;
    align-items: center;
    margin-top: -0.5rem;
  }

  .dialog-footer {
    display: flex;
    gap: 0.75rem;
    padding: 0.5rem 0 0 0;
  }

  .cancel-button {
    flex: 1;
    border-radius: 8px;
    padding: 0.75rem 1.5rem;
    font-weight: 500;
    transition: all 0.2s ease;
  }

  .cancel-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }

  .confirm-button {
    flex: 1;
    border-radius: 8px;
    padding: 0.75rem 1.5rem;
    font-weight: 500;
    background: linear-gradient(135deg, #ef4444, #dc2626);
    border: none;
    color: white;
    transition: all 0.2s ease;
  }

  .confirm-button:hover:not(:disabled) {
    background: linear-gradient(135deg, #dc2626, #b91c1c);
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
  }

  .confirm-button:disabled {
    background: var(--surface-300);
    color: var(--text-color-secondary);
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
  }

  /* Animation cho dialog */
  :deep(.p-dialog) {
    animation: dialogShow 0.3s ease-out;
  }

  @keyframes dialogShow {
    from {
      opacity: 0;
      transform: scale(0.9) translateY(-20px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }

  /* Responsive */
  @media (max-width: 768px) {
    .cancel-invoice-dialog {
      width: 95vw !important;
      margin: 1rem;
    }

    .dialog-header {
      flex-direction: column;
      text-align: center;
      gap: 0.75rem;
    }

    .header-icon {
      align-self: center;
    }

    .dialog-footer {
      flex-direction: column;
    }
  }
</style>
