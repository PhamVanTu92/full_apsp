<script setup>
import { ref, onBeforeMount } from "vue";
import API from "@/api/api-main";
import { useAuthStore } from "@/Pinia/auth";
import { useGlobal } from "@/services/useGlobal";
import { useRouter, useRoute } from "vue-router";
import SettingStorageFeePricingDlg from "./SettingStorageFeePricingDlg.vue";
import router from "../../../../router";
import slugify from "slugify";
import { useI18n } from "vue-i18n";

const route = useRoute();
const { toast, FunctionGlobal } = useGlobal();
const DataFeeLine = ref();
const payLoad = ref();
const loading = ref(false);
const authStore = useAuthStore();
const userId = authStore.user?.appUser?.id ?? 0;
const submited = ref(false);
const dt = ref();
const visibleSetting = ref(false);

const { t} = useI18n();

onBeforeMount(() => {
  initData();
  if (route.name != "UpdateWarehouseFee") getFeeLine();
  if (route.name == "UpdateWarehouseFee") getById(route.params.id);
});

const initData = () => {
  payLoad.value = {
    name: "",
    description: "",
    fromDate: "",
    toDate: "",
    userId: userId,
    status: true,
    feeLine: [],
  };
};
const getFeeLine = async () => {
  try {
    loading.value = true;
    const { data } = await API.get("Fee/feeLine");
    DataFeeLine.value = data.items;
    DataFeeLine.value.forEach((el) => {
      el.status = "A";
    });
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

const getById = async (id) => {
  loading.value = true;
  try {
    const { data } = await API.get(`Fee/${id}`);
    payLoad.value = {
      ...data.items,
      fromDate: new Date(data.items.fromDate),
      toDate: new Date(data.items.toDate),
    };
    payLoad.value.feeLine.sort((a, b) => a.ugpId - b.ugpId);
    payLoad.value.feeLine.forEach((el) => {
      el.status = "U";
    });
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
  }
};

const SaveFee = async () => {
  try {
    submited.value = true;
    if (validate()) {
      return FunctionGlobal.$notify("E", "Vui lòng nhập đủ thông tin", toast);
    }
    loading.value = true;
    if (!payLoad.value.id) {
      payLoad.value.feeLine = DataFeeLine.value.map(
        ({ ugpId, feeLevelId, feePrice, feeWAT, status }) => ({
          ugpId,
          feeLevelId,
          feePrice,
          feeWAT,
          status,
        })
      );
    }
    const url = payLoad.value.id ? `Fee/${payLoad.value.id}` : "Fee/add";
    const ENDPOINT = payLoad.value.id
      ? API.update(url, payLoad.value)
      : API.add(url, payLoad.value);
    const res = await ENDPOINT;

    if (res.data) {
      FunctionGlobal.$notify(
        "S",
        !payLoad.value.id ? "Thêm mới thành công" : "Cập nhật thành công",
        toast
      );
      if (!payLoad.value.id) initData();
      router.push({ name: "storage-fee-pricing-list" });
    }
  } catch (error) {
    FunctionGlobal.$notify("E", error.message || error, toast);
  } finally {
    submited.value = false;
    loading.value = false;
  }
};

const validate = () => {
  const { name, fromDate, toDate } = payLoad.value;
  if (!name.trim()) return true;
  if (!fromDate) return true;
  if (!toDate) return true;
  return false;
};

const exportCSV = () => {
  if (!dt.value) {
    return FunctionGlobal.$notify("E", "Dữ liệu chưa được khởi tạo", toast);
  }

  dt.value.exportCSV({ filename: "Mẫu bảng giá tính phí lưu kho" });
};

const importCSV = (event) => {
  const file = event.files[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onload = (e) => {
    const text = e.target.result;
    parseCSV(text);
  };
  reader.readAsText(file);
  event.files = []; // Reset the file input
};

const parseCSV = (csvText) => {
  const rows = csvText
    .split("\n")
    .map((row) => row.split(","))
    .slice(1);

  rows.forEach((e, index) => {
    const item = DataFeeLine.value.find(
      (el) =>
        el.ougp?.ugpName.trim() === e[0].trim() &&
        el.feeLevel?.name.trim() === e[2].trim()
    );

    if (!!item) {
      item.feePrice = Number(e[3]?.trim()) || 0;
      item.feeWAT = Number(e[4]?.trim()) || 0;
    } else {

    }
  });
};
</script>

<template>
  <div class="mb-3 flex justify-content-between align-items-center">
    <h4 class="font-bold m-0">
      {{
        route.name != "UpdateWarehouseFee"
          ? t('body.sampleRequest.warehouseFee.page_title')
          :  t('body.sampleRequest.warehouseFee.update_page_title')
      }}
    </h4>
    <div class="flex gap-2">
      <Button
        icon="fa-solid fa-gear"
        :label="t('body.sampleRequest.warehouseFee.setup_button')"
        @click="visibleSetting = true"
      />
    </div>
  </div>
  <div class="card">
    <div class="grid">
      <div class="col-6">
        <div class="flex flex-column gap-3">
          <div class="flex flex-column gap-2 w-6">
            <label>{{ t('body.sampleRequest.warehouseFee.price_list_name_label') }} <sup class="text-red-500">*</sup></label>
            <InputText
              :placeholder="t('body.sampleRequest.warehouseFee.price_list_name_placeholder')"
              v-model="payLoad.name"
              :invalid="submited && !payLoad.name"
            />
            <small v-if="submited && !payLoad.name" class="text-red-500"
              >Vui lòng nhập tên bảng giá</small
            >
          </div>
          <div class="flex flex-column gap-2 w-6">
            <label>{{ t('body.sampleRequest.warehouseFee.price_list_description_label') }}</label>
            <InputText :placeholder="t('body.sampleRequest.warehouseFee.price_list_description_placeholder')" v-model="payLoad.description" />
          </div>
        </div>
      </div>
      <div class="col-6">
        <div class="flex flex-column gap-6">
          <div class="flex gap-3">
            <div class="flex flex-column gap-2 w-6">
              <label>
                {{ t('body.sampleRequest.warehouseFee.from_date_label') }}
                <i
                  class="pi pi-info-circle text-primary"
                  v-tooltip="{ value: 'Thời gian áp dụng' }"
                ></i>
                <sup class="text-red-500">*</sup>
              </label>
              <div class="flex flex-column gap-2 w-full">
                <Calendar
                  :placeholder="t('body.sampleRequest.warehouseFee.from_date_placeholder')"
                  class="w-full"
                  v-model="payLoad.fromDate"
                  :invalid="submited && !payLoad.fromDate"
                />
                <small v-if="submited && !payLoad.fromDate" class="text-red-500"
                  >Vui lòng chọn thời gian</small
                >
              </div>
            </div>
            <div class="flex flex-column gap-2 w-6">
              <label>{{ t('body.sampleRequest.warehouseFee.to_date_label') }} <sup class="text-red-500">*</sup></label>
              <Calendar
                :placeholder="t('body.sampleRequest.warehouseFee.to_date_placeholder')"
                class="w-full"
                :minDate="payLoad.fromDate"
                v-model="payLoad.toDate"
                :invalid="submited && !payLoad.toDate"
              />
              <small v-if="submited && !payLoad.toDate" class="text-red-500"
                >Vui lòng chọn thời gian</small
              >
            </div>
          </div>
          <div>
            <div class="flex align-items-center gap-3">
              <label>{{ t('body.sampleRequest.warehouseFee.status_label') }}</label>
              <div class="flex gap-3">
                <div class="flex align-items-center">
                  <RadioButton v-model="payLoad.status" :value="true" />
                  <label class="ml-2">{{ t('body.sampleRequest.warehouseFee.status_active') }}</label>
                </div>
                <div class="flex align-items-center">
                  <RadioButton v-model="payLoad.status" :value="false" />
                  <label class="ml-2">{{ t('body.sampleRequest.warehouseFee.status_inactive') }}</label>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div class="card p-2">
    <DataTable
      tableStyle="min-width: 50rem"
      :value="!route.params.id ? DataFeeLine : payLoad.feeLine"
      scrollable
      ref="dt"
      scrollHeight="450px"
      rowGroupMode="rowspan"
      :groupRowsBy="['ougp.ugpName']"
      showGridlines
      exportFilename="Mẫu bảng giá tính phí lưu kho"
    >
      <template #header>
        <div class="flex justify-content-end gap-2">
          <Button
            icon="pi pi-external-link"
            :label="t('body.sampleRequest.warehouseFee.export_button')"
            severity="warning"
            @click="exportCSV($event)"
          />
          <FileUpload
            mode="basic"
            @select="importCSV"
            :auto="false"
            :maxFileSize="1000000"
            :label="t('body.sampleRequest.warehouseFee.import_button')"
            severity="info"
            :chooseLabel="t('body.sampleRequest.warehouseFee.import_button')"
          />
        </div>
      </template>
      <Column header="#" field="">
        <template #body="{ index }">
          <span>{{ index + 1 }}</span>
        </template>
      </Column>
      <Column :header="t('body.sampleRequest.warehouseFee.table_header_unit_group')" field="ougp.ugpName"></Column>
      <Column :header="t('body.sampleRequest.warehouseFee.table_header_description')" field="ougp.ugpName"></Column>
      <Column :header="t('body.sampleRequest.warehouseFee.table_header_fee_milestone')" field="feeLevel.name"></Column>
      <Column :header="t('body.sampleRequest.warehouseFee.table_header_price')" field="feePrice">
        <template #body="{ data }">
          <InputNumber v-model="data.feePrice" :min="0" />
        </template>
      </Column>
      <Column :header="t('body.sampleRequest.warehouseFee.table_header_tax')" field="feeWAT">
        <template #body="{ data }">
          <InputNumber v-model="data.feeWAT" :min="0" :max="100" />
        </template>
      </Column>
    </DataTable>
    <div class="flex justify-content-end mt-5 mb-2 mr-3">
      <div class="flex gap-3">
        <router-link to="/storage-fee-pricing/list">
          <Button :label="t('body.home.back_button')" severity="secondary"/>
        </router-link>
        <Button :label="t('body.systemSetting.save_button')" @click="SaveFee"/>
      </div>
    </div>
  </div>

  <SettingStorageFeePricingDlg
    v-model:visible="visibleSetting"
    @updateData="
      route.name == 'UpdateWarehouseFee' ? getById(route.params.id) : getFeeLine()
    "
  ></SettingStorageFeePricingDlg>

  <Loading v-if="loading"></Loading>
</template>
<style scoped>
.row_gap > div:last-child {
    margin: 0 !important;
    padding-bottom: 0px !important;
}

.row_gap > div:not(:last-child) {
    border-bottom: 1px var(--surface-border) solid;
    padding-bottom: 0.5rem;
}
</style>
