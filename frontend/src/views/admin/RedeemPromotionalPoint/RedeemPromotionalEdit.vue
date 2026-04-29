<script setup>
  import { ref, onBeforeMount } from "vue";
  import API from "@/api/api-main";
  import SelectDistributor from "./components/SelectDistributor.vue";
  import { useRouter, useRoute } from "vue-router";
  import { useGlobal } from "@/services/useGlobal";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  const { toast } = useGlobal();
  import format from "@/helpers/format.helper";
  const ConvertCustomerLocal = async (data) => {
    if (Array.isArray(data) && data.length > 0) {
      const customer = [
        {
          type: data[0]?.type || "",
          items: data.map((e) => ({
            id: e.customerId,
            cardCode: e.customerCode,
            cardName: e.customerName,
          })),
        },
      ];

      return customer;
    }
    return [{ type: "", items: [] }];
  };

  const dataCallApi = ref({
    id: null,
    name: "",
    note: "",
    startDate: format.formatDate(new Date()),
    endDate: format.formatDate(new Date()),
    isActive: true,
    isAllCustomer: true,
    lines: [],
    customers: [],
  });

  const router = useRouter();
  const route = useRoute();
  const visible = ref(false);
  const isFilterPro = ref(false);
  const submited = ref(false);

  const loading = ref(false);
  const ItemData = ref([]);
  const ItemType = ref([]);
  const selectedItemType = ref([]);

  const getById = async (id) => {
    try {
      const res = await API.get(`ExchangePoint/${id}`);
      if (res.data) {
        dataCallApi.value = res.data.items;
        dataCallApi.value.startDate = format.formatDate(dataCallApi.value.startDate);
        dataCallApi.value.endDate = format.formatDate(dataCallApi.value.endDate);
        res.data.items.customers = await ConvertCustomerLocal(res.data.items.customers);
      }
    } catch (error) {
      toast.add({
        severity: "error",
        summary: t("Custom.errorOccurred"),
        life: 3000,
      });
      console.error(error);
    }
  };

  const updateSelectedProduct = (data) => {
    data.forEach((el) => {
      ItemData.value.push({
        itemId: el.itemId,
        itemCode: el.itemCode,
        itemName: el.itemName,
        packingId: el.packing.id,
        packingName: el.packing.name,
        point: 0,
      });
    });
  };

  const confirmDataProduct = () => {
    const existingItemIds = new Set(dataCallApi.value.lines.map((line) => line.itemCode));
    const newItems = ItemData.value.filter((item) => !existingItemIds.has(item.itemCode));
    dataCallApi.value.lines.push(...newItems);
    visible.value = false;
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

  const IntermediateVariable = ref();
  const RemoveItem = (index) => {
    ItemData.value.splice(index, 1);
    for (let i = IntermediateVariable.value.length - 1; i >= 0; i--)
      IntermediateVariable.value.splice(i, 1);
    ItemData.value.forEach((el) => {
      IntermediateVariable.value.push({
        id: 0,
        fatherId: 0,
        itemType: "I",
        itemId: el.itemId,
        itemCode: el.itemCode,
        itemName: el.itemName,
        status: "",
      });
    });
  };
  const ConvertCustomer = (data) => {
    if (data?.length) data = data[0];
    if (data?.items?.length) {
      return data?.items.map((el) => ({
        type: data.type,
        customerId: el.id,
        CustomerCode: el.cardCode || "",
        customerName: el.cardName || el.groupName,
        status: "A",
      }));
    }
    return [];
  };
  const convertDateCallApi = (date) => {
    const [d, m, y] = date.split("/");
    return new Date(`${y}-${m}-${d}`).toISOString().slice(0, 10);
  };

  const validate = () => {
    if (
      !dataCallApi.value.name ||
      !dataCallApi.value.startDate ||
      !dataCallApi.value.endDate ||
      dataCallApi.value.lines.some((line) => line.point < 0)
    ) {
      submited.value = true;
      toast.add({
        severity: "error",
        summary: t("Notification.input_required"),
        life: 3000,
      });
      return false;
    }
    dataCallApi.value.startDate = convertDateCallApi(dataCallApi.value.startDate);
    dataCallApi.value.endDate = convertDateCallApi(dataCallApi.value.endDate);
    return true;
  };
  const SavePromotion = async () => {
    if (!validate()) return;
    let Customers = ConvertCustomer(dataCallApi.value.customers);
    let dataCall = {
      ...dataCallApi.value,
      customers: Customers,
    };
    try {
      const res = dataCallApi.value.id
        ? await API.update(`ExchangePoint/${dataCallApi.value.id}`, dataCall)
        : await API.add("ExchangePoint", dataCall);
      if (res) {
        toast.add({
          severity: "success",
          summary: dataCallApi.value.id
            ? t("client.update_success")
            : t("PromotionalItems.SetupPurchases.addnewSuccess"),
          life: 3000,
        });
      }
      router.push(`/promotionalPoints`);
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

  const actionLines = (index, type) => {
    if (type == "remove") dataCallApi.value.lines.splice(index, 1);
    else {
      let dataCopy = { ...dataCallApi.value.lines[index] };
      dataCallApi.value.lines.push(dataCopy);
    }
  };

  const changeAllCustomer = () => {
    if (dataCallApi.value.isAllCustomer) {
      dataCallApi.value.customers = [];
    }
  };
</script>
<template>
  <div class="flex justify-content-between align-items-center mb-4 sticky top-0">
    <h4 class="font-bold m-0">
      {{
        dataCallApi.id
          ? t("PromotionalItems.SetupPurchasesPoint.update_promotion_page_title")
          : t("PromotionalItems.SetupPurchasesPoint.create_promotion_page_title")
      }}
    </h4>
    <div class="flex gap-2">
      <Button
        @click="router.back()"
        :label="t('body.promotion.back_button')"
        icon="pi pi-arrow-left"
        severity="secondary"
      />
      <Button
        :label="t('body.systemSetting.save_button')"
        icon="pi pi-save"
        @click="SavePromotion()"
      />
    </div>
  </div>
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
                :invalid="submited && !dataCallApi.name"
                v-model="dataCallApi.name"
                class="w-8"
              >
              </InputText>
            </div>
          </div>
          <div class="col-12 flex flex-column gap-3">
            <div class="flex justify-content-between">
              <span>{{ t("body.promotion.status_column") }}</span>
              <div class="flex w-8 gap-6">
                <div class="flex align-items-center">
                  <RadioButton inputId="A" v-model="dataCallApi.isActive" :value="true" />
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
        <div class="grid mt-2">
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
                  @change="changeAllCustomer"
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
                  v-model:selection="dataCallApi.customers"
                  :disabled="dataCallApi.isAllCustomer"
                />
              </div>
            </div>
          </div>
          <div class="col-12">
            <h6 class="m-0 font-bold">
              {{ t("body.promotion.applicable_time_section_title") }}
            </h6>
            <div class="flex justify-content-between mt-3">
              <div class="gap-2 align-items-center flex">
                <span
                  >{{ t("body.promotion.start_date_label")
                  }}<sup class="text-red-500">*</sup></span
                >
                <Calendar
                  showIcon
                  :invalid="submited && !dataCallApi.startDate"
                  :minDate="new Date()"
                  v-model="dataCallApi.startDate"
                  :placeholder="t('body.promotion.start_date_placeholder')"
                  :dateFormat="'dd/mm/yy'"
                  @update:modelValue="
                    (val) => (dataCallApi.startDate = format.formatDate(val))
                  "
                >
                </Calendar>
              </div>
              <div class="gap-2 align-items-center flex">
                <span
                  >{{ t("body.promotion.end_date_label")
                  }}<sup class="text-red-500">*</sup></span
                >
                <Calendar
                  showIcon
                  :invalid="submited && !dataCallApi.endDate"
                  :minDate="new Date()"
                  v-model="dataCallApi.endDate"
                  :placeholder="t('body.promotion.end_date_placeholder')"
                  :dateFormat="'dd/mm/yy'"
                  @update:modelValue="
                    (val) => (dataCallApi.endDate = format.formatDate(val))
                  "
                >
                </Calendar>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-4">
      <div class="card">
        <h6 class="m-0 font-bold mb-3">{{ t("body.PurchaseRequestList.note_label") }}</h6>
        <Textarea v-model="dataCallApi.note" rows="12" class="w-full" />
      </div>
    </div>
    <div class="col-12">
      <div class="card">
        <div class="flex align-items-center justify-content-between">
          <h5 class="m-0 font-bold">{{ t("body.promotion.promotion") }}</h5>
          <Button
            icon="pi pi-plus"
            class="p-button-rounded p-button-text"
            style="background-color: #0d733d; color: #fff"
            @click="() => (visible = true)"
          />
        </div>
        <DataTable
          :value="dataCallApi.lines"
          dataKey="id"
          stripedRows
          responsiveLayout="scroll"
        >
          <template #empty>
            <div class="text-center p-4 text-gray-500">
              {{ t("body.OrderList.noData") }}
            </div>
          </template>
          <Column header="#">
            <template #body="slotProps">
              {{ slotProps.index + 1 }}
            </template>
          </Column>
          <Column :header="t('body.home.product_code_column') + ' *'">
            <template #body="slotProps">
              {{ slotProps.data.itemCode }}
            </template>
          </Column>
          <Column :header="t('body.home.product_name_column') + ' *'">
            <template #body="slotProps">
              {{ slotProps.data.itemName }}
            </template>
          </Column>
          <Column :header="t('body.home.packaging_column')">
            <template #body="slotProps">
              {{ slotProps.data.packingName }}
            </template>
          </Column>
          <Column :header="t('PromotionalItems.SetupPurchases.promotion_type')">
            <template #body="slotProps">
              <InputNumber
                v-model="slotProps.data.point"
                :invalid="submited && !slotProps.data.point"
                :min="0"
              />
            </template>
          </Column>
          <Column :header="t('body.systemSetting.action')">
            <template #body="{ index }">
              <Button
                icon="pi pi-trash"
                class="p-button-rounded p-button-text"
                severity="danger"
                :title="t('body.OrderList.delete')"
                @click="actionLines(index, 'remove')"
              />
            </template>
          </Column>
        </DataTable>
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="visible"
    modal
    :header="t('body.sampleRequest.sampleProposal.choose_product_button')"
    :style="{ width: '45vw' }"
  >
    <TabView :activeIndex="selectedItemType.length ? 1 : 0">
      <TabPanel :header="t('body.home.product_label')">
        <DataTable :value="ItemData">
          <Column header="#">
            <template #body="{ index }">{{ index + 1 }}</template>
          </Column>
          <Column field="itemName" :header="t('body.home.product_name_column')"></Column>
          <Column>
            <template #body="{ index }">
              <Button
                icon="pi pi-trash"
                text
                severity="danger"
                @click="RemoveItem(index)"
              />
            </template>
          </Column>
        </DataTable>
        <div class="flex justify-content-between w-full mt-5">
          <div class="flex gap-2">
            <ProductSelector
              icon="pi pi-plus"
              :label="t('PromotionalItems.SetupPurchases.choose_product_button')"
              outlined
              :isFilterPro="isFilterPro"
              @confirm="updateSelectedProduct($event)"
            />
            <ProductSelector
              :andPointApi="'Item/ItemPromotions'"
              icon="pi pi-plus"
              :label="t('PromotionalItems.SetupPurchases.add_product_button')"
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
  <Loading v-if="loading" />
</template>

<style>
  .p-highlight::before {
    background: var(--green-200);
  }

  .p-highlight > .p-button-label {
    color: var(--green-800);
  }
</style>
