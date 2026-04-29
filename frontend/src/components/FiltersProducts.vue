<template>
  <div class="w-full flex gap-2">
    <MultiSelect
      v-model="query.brand"
      :placeholder="t('body.home.brand_column')"
      :options="DataGlobal.dataBrand"
      optionLabel="name"
      optionValue="id"
      filter
      style="width: 25%"
      @change="GetAllProduct()"
    />
    <MultiSelect
      v-model="query.industry"
      :placeholder="t('body.home.category_column')"
      :options="DataGlobal.dataTypeGoods"
      optionLabel="name"
      optionValue="id"
      filter
      style="width: 25%"
      @change="GetAllProduct()"
    />
    <MultiSelect
      v-model="query.itemtype"
      :placeholder="t('client.product_type')"
      :options="DataGlobal.dataTypeProduct"
      optionLabel="name"
      optionValue="id"
      filter
      style="width: 25%"
      @change="GetAllProduct()"
    />
    <MultiSelect
      v-model="query.packing"
      :placeholder="t('body.home.packaging_column')"
      :options="DataGlobal.dataOUOM"
      optionLabel="name"
      optionValue="id"
      filter
      style="width: 25%"
      @change="GetAllProduct()"
    />
    <InputText
      :placeholder="t('common.placeholder_enter_keyword')"
      v-model="query.search"
      @keyup="SearchProducts()"
      style="width: 25%"
    >
    </InputText>
    <div class="flex-grow-0 p-0">
      <!-- <Button icon="pi pi-refresh" @click="onClickRefresh" /> -->
      <Button
        icon="pi pi-times"
        @click="onClickClearFilter"
        severity="danger"
        text
        v-tooltip="t('common.btn_clear_filter')"
      />
    </div>
  </div>
  <!-- <div class="flex align-items-start">
    <Button @click="resetQuery" v-if="!checkQuery()" label="Xóa điều kiện lọc"/>
  </div> -->
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import API from "../api/api-main";
const { t } = useI18n();
import { debounce } from "lodash";
const emits = defineEmits(["getQueryString", "SearchProduct"]);
onMounted(() => {
  GetAllBrand();
  GetAllTypeGoods();
  GetAllTypeProduct();
  GetAllOUOM();
});
const DataGlobal = ref({});
const query = ref({
  search: "",
  brand: "",
  industry: "",
  packing: "",
  itemtype: "",
});

const onClickClearFilter = () => {
  Object.keys(query.value).forEach((key) => (query.value[key] = ""));
  GetAllProduct();
};
const resetQuery = () => {
  Object.keys(query.value).forEach((key) => (query.value[key] = ""));
  GetAllProduct();
};
const GetAllBrand = async () => {
  try {
    const res = await API.get(`Brand/getall`);
    if (res.data) DataGlobal.value.dataBrand = res.data;
  } catch (error) {}
};

const GetAllTypeGoods = async () => {
  try {
    const res = await API.get(`Industry/getall`);
    if (res.data) DataGlobal.value.dataTypeGoods = res.data;
  } catch (error) {}
};

const GetAllTypeProduct = async () => {
  try {
    const res = await API.get(`ItemType/getall`);
    if (res.data) DataGlobal.value.dataTypeProduct = res.data;
  } catch (error) {}
};
const GetAllOUOM = async () => {
  try {
    const res = await API.get(`Packing/getall`);
    if (res.data) DataGlobal.value.dataOUOM = res.data;
  } catch (error) {}
};
const GetAllProduct = async () => {
  const fields = ["brand", "industry", "itemtype", "packing", "search"];
  let queryString = fields
    .filter((field) => query.value[field]?.length || query.value[field] !== "")
    .map((field) => {
      const value = Array.isArray(query.value[field])
        ? query.value[field].join(",")
        : query.value[field];

      if (field === "search") {
        return `${field}=${value || ""}`;
      }
      return `${field}=${value ? `,${value},` : ""}`;
    })
    .join("&");
  emits("getQueryString", queryString);
};
const debouncedFetchProduct = debounce(GetAllProduct, 1000);

const SearchProducts = () => {
  debouncedFetchProduct();
};
const checkQuery = () => {
  return Object.values(query.value).every(
    (value) => value === null || value === undefined || value === ""
  );
};
</script>
<style></style>
