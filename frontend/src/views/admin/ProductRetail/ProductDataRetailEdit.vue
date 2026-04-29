<script setup>
  import { ref, onBeforeMount } from "vue";
  import API from "@/api/api-main";
  import { useRouter, useRoute } from "vue-router";
  import { useGlobal } from "@/services/useGlobal";
  import { useI18n } from "vue-i18n";
  import format from "@/helpers/format.helper";
  import SelectDistributor from "./components/SelectDistributor.vue";
  const { t } = useI18n();
  const visible = ref(false);
  const { toast } = useGlobal();
  const ItemData = ref([]);
  const Customers = ref();
  const router = useRouter();
  const route = useRoute();
  const isFilterPro = ref(false);
  const submited = ref(false);
  const loading = ref(false);
  const selectedItemType = ref([]);
  const IntermediateVariable = ref();
  const fileInput = ref(null);
  const discountTypeOptions = [
    { label: "USD", value: "USD" },
    { label: "VND", value: "VND" },
  ];
  const dataCallApi = ref({
    id: 0,
    priceListName: "",
    createdDate: format.formatDate(new Date()),
    effectDate: format.formatDate(new Date()),
    expriedDate: format.formatDate(new Date()),
    isAllCustomer: true,
    isRetail: true,
    isActive: true,
    note: "",
    priceListLine: [],
  });

  const confirmDataProduct = () => {
    const existingItemIds = dataCallApi.value.priceListLine.map((line) => line.itemCode);
    const newItems = ItemData.value.filter(
      (item) => !existingItemIds.includes(item.itemCode)
    );
    dataCallApi.value.priceListLine.unshift(...newItems);
    visible.value = false;
  };

  const getById = async (id) => {
    try {
      const res = await API.get(`PriceList/${id}`);
      if (!res.data) return;
      const {
        effectDate,
        expriedDate,
        customerId,
        customerName,
        customerGroupId,
        customerGroupName,
        ...restData
      } = res.data;
      dataCallApi.value = {
        ...restData,
        effectDate: format.formatDate(effectDate),
        expriedDate: format.formatDate(expriedDate),
      };
      if (customerId) {
        Customers.value = [
          {
            type: "C",
            items: [{ id: customerId, cardName: customerName }],
          },
        ];
      } else if (customerGroupId) {
        Customers.value = [
          {
            type: "G",
            items: [{ id: customerGroupId, groupName: customerGroupName }],
          },
        ];
      }
    } catch (error) {
      console.error("Error fetching price list:", error);
      toast.add({
        severity: "error",
        summary: t("body.report.error_occurred_message"),
        life: 3000,
      });
    }
  };

  const updateSelectedProduct = (data) => {
    data.forEach((el) => {
      ItemData.value.push({
        id: 0,
        fatherId: 0,
        itemId: el.itemId,
        itemCode: el.itemCode,
        itemName: el.itemName,
        packingId: el.packing.id,
        packingName: el.packing.name,
        image: el.itM1[0].filePath || "",
        point: 0,
        currency: "VND",
      });
    });
  };
  const convertDateCallApi = (date) => {
    const [d, m, y] = date.split("/");
    return new Date(`${y}-${m}-${d}`).toISOString().slice(0, 10);
  };

  const validate = (dataValid) => {
    submited.value = true;
    if (!dataValid.priceListName) {
      toast.add({
        severity: "error",
        summary: t("validation.priceListName_required"),
        life: 3000,
      });
      return false;
    }

    if (!dataValid.effectDate || !dataValid.expriedDate) {
      toast.add({
        severity: "error",
        summary: t("validation.effectDate_required"),
        life: 3000,
      });
      return false;
    }
    if (!dataValid) {
      if (!Customers.value?.items?.length) {
        toast.add({
          severity: "warn",
          summary: t("validation.applicable_object_required"),
          life: 3000,
        });
        return false;
      }
      if (Customers.value.items.length > 1) {
        toast.add({
          severity: "error",
          summary: t("validation.applicable_object_required2"),
          life: 3000,
        });
        return false;
      }
      const customer = Customers.value.items[0];
      const isCustomer = Customers.value.type === "C";
      if (dataValid.isAllCustomer) Customers.value = null;

      if (isCustomer) {
        dataValid.customerId = customer.id;
        dataValid.customerName = customer.cardName;
      } else {
        dataValid.customerGroupId = customer.id;
        dataValid.customerGroupName = customer.groupName;
      }
    }
    dataValid.effectDate = convertDateCallApi(dataValid.effectDate);
    dataValid.expriedDate = convertDateCallApi(dataValid.expriedDate);
    if (dataValid.priceListLine.length === 0) {
      toast.add({
        severity: "error",
        summary: t("validation.products_required2"),
        life: 3000,
      });
      return false;
    }
    const invalidPriceItems = dataValid.priceListLine.filter(
      (item) => !item.price || item.price <= 0
    );
    if (invalidPriceItems.length > 0) {
      toast.add({
        severity: "error",
        summary: t("validation.products_required3"),
        life: 3000,
      });
      return false;
    }
    if (!dataValid.id) dataValid.createdDate = convertDateCallApi(dataValid.createdDate);
    else dataValid.createdDate = dataValid.createdDate.substring(0, 10);

    return true;
  };

  const SavePromotion = async () => {
    let dataValid = { ...dataCallApi.value };
    if (!validate(dataValid)) return;
    try {
      const res = dataValid.id
        ? await API.update(`PriceList/${dataValid.id}`, dataValid)
        : await API.add("PriceList", dataValid);
      if (res) {
        toast.add({
          severity: "success",
          summary: dataValid.id
            ? t("client.update_success")
            : t("PromotionalItems.SetupPurchases.addnewSuccess"),
          life: 3000,
        });
      }
      router.push(`/list-product-retail`);
    } catch (error) {
      console.error(error);
      if (error.status == 403) {
        toast.add({
          severity: "error",
          summary: t("PromotionalItems.SetupPurchases.youDontAction"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("body.report.error_occurred_message"),
          life: 3000,
        });
      }
    } finally {
      loading.value = false;
    }
  };

  const confirmDataTypeItem = (data) => {
    for (let i = IntermediateVariable.value.length - 1; i >= 0; i--) {
      IntermediateVariable.value.splice(i, 1);
    }
    ItemData.value = [];
    data.forEach((el) => {
      IntermediateVariable.value.push({
        id: 0,
        fatherId: 0,
        itemType: "G",
        itemId: el.itemId,
        itemCode: el.itemCode,
        itemName: el.itemName,
        packing: el.packing,
        status: "A",
      });
    });
    visible.value = false;
  };

  const onDownloadTemplate = () => {
    const link = document.createElement("a");
    link.href = "/files/Mẫu_sản_phẩm_bán_lẻ.xlsx";
    link.download = "Mẫu_sản_phẩm_bán_lẻ.xlsx";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const onFileSelected = (event) => {
    const file = event.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = async (e) => {
      try {
        const XLSX = await import("xlsx");
        const workbook = XLSX.read(e.target.result, { type: "array" });
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];
        const data = XLSX.utils.sheet_to_json(worksheet, { range: 1 });

        const processedData = [];
        for (const row of data) {
          if (row["Mã sản phẩm"]) {
            processedData.push({
              itemCode: row["Mã sản phẩm"] || "",
              // itemName: row['Tên sản phẩm'] || '',
              // itemUnit: row['ĐVT'] || '',
              price: row["Đơn giá"] || 0,
              currency: row["Đơn vị tiền tệ"] || "VND",
            });
          }
        }
        let res = await API.add("PriceList/import", processedData);
        if (res) {
          dataCallApi.value.priceListLine = [
            ...dataCallApi.value.priceListLine,
            ...res.data,
          ];
        }
      } catch (error) {
        console.error("Error reading Excel file:", error);
      }
    };
    reader.readAsArrayBuffer(file);
    event.target.value = "";
  };

  onBeforeMount(async () => {
    try {
      loading.value = true;
      if (route?.params?.id) await getById(route?.params?.id);
    } catch (error) {
      console.error(error);
      toast.add({
        severity: "error",
        summary: t("body.report.error_occurred_message"),
        life: 3000,
      });
    } finally {
      loading.value = false;
    }
  });
</script>
<template>
  <div class="flex justify-content-between align-items-center sticky top-0 mb-3">
    <h4 class="font-bold m-0">
      {{
        dataCallApi.id
          ? t("PromotionalItems.SetupPurchasesPoint.update_retail_page_title")
          : t("PromotionalItems.SetupPurchasesPoint.create_retail_page_title")
      }}
    </h4>
    <div class="flex gap-2">
      <Button
        icon="pi pi-download"
        label="Tải mẫu in"
        severity="info"
        outlined
        @click="onDownloadTemplate"
      />
      <Button
        icon="pi pi-upload"
        label="Nhập Excel"
        severity="success"
        outlined
        @click="() => fileInput.click()"
      />
      <Button
        :label="t('body.systemSetting.save_button')"
        icon="pi pi-save"
        @click="SavePromotion()"
      />
      <Button
        @click="router.back()"
        :label="t('body.promotion.back_button')"
        icon="pi pi-arrow-left"
        severity="secondary"
      />
      <input
        ref="fileInput"
        type="file"
        accept=".xlsx,.xls"
        style="display: none"
        @change="onFileSelected"
      />
    </div>
  </div>
  <div>
    <div class="grid">
      <div class="col-8">
        <div class="card">
          <h6 class="m-0 font-bold">{{ t("body.promotion.info_section_title") }}</h6>
          <div class="grid mt-2">
            <div class="col-12 flex flex-column gap-3">
              <div class="flex justify-content-between">
                <span
                  >{{ t("body.promotion.promotion_name_label") }}
                  <sup class="text-red-500">*</sup></span
                >
                <InputText
                  :invalid="submited && !dataCallApi.priceListName"
                  v-model="dataCallApi.priceListName"
                  class="w-8"
                />
              </div>
              <div class="flex justify-content-between">
                <span
                  >{{ t("PromotionalItems.PromotionalItems.priceRetail") }}
                  <sup class="text-red-500">*</sup></span
                >
                <div class="flex w-8 align-items-center">
                  <Checkbox v-model="dataCallApi.isRetail" :binary="true" />
                  <label class="ml-2">{{
                    t("PromotionalItems.PromotionalItems.isRetail")
                  }}</label>
                </div>
              </div>
              <div class="flex justify-content-between">
                <span>{{ t("body.promotion.status_column") }}</span>
                <div class="flex w-8 gap-2">
                  <div class="flex align-items-center">
                    <RadioButton
                      inputId="A"
                      v-model="dataCallApi.isActive"
                      :value="true"
                    />
                    <label for="A" class="ml-2">{{
                      t("body.sampleRequest.customer.active_status")
                    }}</label>
                  </div>
                  <div class="flex align-items-center">
                    <RadioButton
                      inputId="I"
                      v-model="dataCallApi.isActive"
                      :value="false"
                    />
                    <label for="I" class="ml-2">{{
                      t("body.sampleRequest.customer.inactive_status")
                    }}</label>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="grid">
            <div class="col-12 flex flex-column gap-3">
              <h6 class="m-0 font-bold">
                {{ t("body.promotion.applicable_object_section_title") }}
              </h6>
              <div class="flex justify-content-between">
                <div class="flex align-items-center">
                  <RadioButton
                    inputId="All"
                    v-model="dataCallApi.isAllCustomer"
                    :value="true"
                    name="pizza"
                  />
                  <label for="All" class="ml-2">{{
                    t("body.promotion.applicable_object_all")
                  }}</label>
                </div>
              </div>
              <div class="flex">
                <div class="flex w-4 align-items-center">
                  <RadioButton
                    inputId="Cus"
                    v-model="dataCallApi.isAllCustomer"
                    :value="false"
                    name="pizza"
                  />
                  <label for="Cus" class="ml-2">{{
                    t("body.promotion.applicable_object_custom")
                  }}</label>
                </div>
                <div class="flex w-8 flex-column gap-2">
                  <SelectDistributor
                    v-if="!loading"
                    v-model:selection="Customers"
                    :disabled="dataCallApi.isAllCustomer"
                  />
                </div>
              </div>
            </div>
            <div class="col-12 flex flex-column gap-3">
              <h6 class="m-0 font-bold">
                {{ t("body.promotion.applicable_time_section_title") }}
              </h6>
              <div class="align-items-center flex">
                <span class="w-4"
                  >{{ t("body.promotion.start_date_label")
                  }}<sup class="text-red-500">*</sup></span
                >
                <Calendar
                  showIcon
                  :invalid="submited && !dataCallApi.effectDate"
                  :minDate="new Date()"
                  v-model="dataCallApi.effectDate"
                  class="w-8"
                  :placeholder="t('body.promotion.start_date_placeholder')"
                  :dateFormat="'dd/mm/yy'"
                  @update:modelValue="
                    (val) => (dataCallApi.effectDate = format.formatDate(val))
                  "
                >
                </Calendar>
              </div>
              <div class="align-items-center flex">
                <span class="w-4"
                  >{{ t("body.promotion.end_date_label")
                  }}<sup class="text-red-500">*</sup></span
                >
                <Calendar
                  showIcon
                  :invalid="submited && !dataCallApi.expriedDate"
                  :minDate="new Date()"
                  v-model="dataCallApi.expriedDate"
                  :placeholder="t('body.promotion.end_date_placeholder')"
                  :dateFormat="'dd/mm/yy'"
                  class="w-8"
                  @update:modelValue="
                    (val) => (dataCallApi.expriedDate = format.formatDate(val))
                  "
                >
                </Calendar>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4">
        <div class="card">
          <h6 class="m-0 font-bold mb-3">
            {{ t("PromotionalItems.SetupPurchasesPoint.summary_section_title") }}
          </h6>
          <div style="line-height: 2">
            <span
              >{{ t("body.promotion.promotion_name_label") }} :
              {{ dataCallApi.priceListName }}</span
            >
            <br />
            <span
              >{{ t("body.sampleRequest.customer.applicable_object_column") }} :
              {{
                dataCallApi.isAllCustomer == true
                  ? t("body.promotion.applicable_object_all")
                  : t("body.promotion.applicable_object_custom")
              }}</span
            ><br />
            <span
              >{{ t("body.promotion.start_date_label") }} :
              {{ dataCallApi.effectDate }}</span
            ><br />
            <span
              >{{ t("body.promotion.end_date_label") }} :
              {{ dataCallApi.expriedDate }}</span
            >
          </div>
        </div>
        <div class="card">
          <h6 class="m-0 font-bold">{{ t("body.PurchaseRequestList.note_label") }}</h6>
          <div class="flex justify-content-between mt-3">
            <Textarea v-model="dataCallApi.note" rows="2" class="w-full" />
          </div>
        </div>
      </div>

      <div class="col-12">
        <div class="card">
          <div class="flex justify-content-between align-items-center mb-3">
            <div></div>
            <Button
              icon="pi pi-plus"
              label="Thêm sản phẩm"
              @click="() => (visible = true)"
            />
          </div>
          <DataTable
            :value="dataCallApi.priceListLine"
            dataKey="id"
            stripedRows
            responsiveLayout="scroll"
            paginator
            :rows="10"
            :rowsPerPageOptions="[5, 10, 20, 50]"
            :totalRecords="dataCallApi.priceListLine.length"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :currentPageReportTemplate="`${t(
              'body.productManagement.display'
            )} {first} - {last} ${t(
              'body.productManagement.total_of'
            )} {totalRecords} ${t('body.systemSetting.orders')}`"
          >
            <template #empty>
              <div class="text-center p-4 text-gray-500">
                {{ t("body.OrderList.noData") }}
              </div>
            </template>
            <Column header="#">
              <template #body="{ index }">
                {{ index + 1 }}
              </template>
            </Column>
            <Column
              field="itemCode"
              :header="t('body.home.product_code_column') + ' *'"
            />
            <Column :header="t('body.home.product_name_column') + ' *'">
              <template #body="slotProps">
                <div class="flex align-items-center gap-2">
                  <img
                    v-if="slotProps.data.image"
                    :src="slotProps.data.image"
                    alt=""
                    class="w-3rem h-3rem object-cover border-round"
                  />
                  <span>{{ slotProps.data.itemName }}</span>
                </div>
              </template>
            </Column>
            <Column field="packingName" :header="t('client.unit')" />
            <Column :header="t('PromotionalItems.PromotionalItems.priceRetail')">
              <template #body="slotProps">
                <div class="flex">
                  <InputGroup>
                    <InputNumber
                      v-model="slotProps.data.price"
                      :min="0"
                      placeholder="0"
                      :minFractionDigits="slotProps.data['currency'] == 'USD' ? 2 : 0"
                      :maxFractionDigits="slotProps.data['currency'] == 'USD' ? 2 : 0"
                      class="w-7rem"
                    />
                    <Dropdown
                      v-model="slotProps.data['currency']"
                      class="surface-50 hover:bg-green-50" 
                      :options="discountTypeOptions"
                      option-label="label"
                      option-value="value"
                      pt:trigger:class="hidden"
                      style="margin-left: -1px; min-width: 5rem"
                    />
                  </InputGroup>
                </div>
              </template>
            </Column>

            <Column :header="t('body.systemSetting.action')">
              <template #body="{ index }">
                <Button
                  icon="pi pi-trash"
                  class="p-button-rounded p-button-text"
                  severity="danger"
                  :title="t('body.OrderList.delete')"
                  @click="dataCallApi.priceListLine.splice(index, 1)"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="visible"
    modal
    :header="t('body.sampleRequest.sampleProposal.choose_product_button')"
    :style="{ width: '45rem' }"
  >
    <TabView :activeIndex="selectedItemType.length ? 1 : 0">
      <TabPanel :header="t('body.home.product_label')">
        <DataTable :value="ItemData">
          <Column header="#">
            <template #body="slotProps">
              {{ slotProps.index + 1 }}
            </template>
          </Column>
          <Column field="itemName" :header="t('body.home.product_name_column')"></Column>
          <Column>
            <template #body="{ index }">
              <Button
                icon="pi pi-trash"
                text
                severity="danger"
                @click="() => ItemData.splice(index, 1)"
              />
            </template>
          </Column>
        </DataTable>
        <div class="flex justify-content-between w-full mt-5">
          <div>
            <ProductSelector
              icon="pi pi-plus"
              :label="t('body.PurchaseRequestList.find_product_button')"
              outlined
              :isFilterPro="isFilterPro"
              @confirm="updateSelectedProduct($event)"
            />
          </div>
          <div class="flex justify-content-end gap-2">
            <Button
              type="button"
              :label="t('body.home.cancel_button')"
              severity="secondary"
              @click="visible = false"
            />
            <Button
              type="button"
              :label="t('body.home.confirm_button')"
              @click="confirmDataProduct"
            />
          </div>
        </div>
      </TabPanel>
      <TabPanel :header="t('body.productManagement.product_type')" v-if="isFilterPro">
        <DataTable
          :value="ItemType"
          v-model:selection="selectedItemType"
          dataKey="itemId"
          scrollable
          scrollHeight="400px"
        >
          <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
          <Column
            field="itemName"
            :header="t('body.productManagement.typeName')"
          ></Column>
        </DataTable>
        <div class="flex justify-content-end gap-2 mt-5">
          <Button
            type="button"
            :label="t('body.home.cancel_button')"
            severity="secondary"
            @click="visible = false"
          />
          <Button
            type="button"
            :label="t('body.home.confirm_button')"
            @click="confirmDataTypeItem(selectedItemType)"
          />
        </div>
      </TabPanel>
    </TabView>
  </Dialog>
  <Loading v-if="loading"></Loading>
</template>

<style>
  .p-highlight::before {
    background: var(--green-200);
  }

  .p-highlight > .p-button-label {
    color: var(--green-800);
  }
</style>

<style scoped>
  :deep .p-inputnumber-input {
    width: 2rem;
  }
</style>
