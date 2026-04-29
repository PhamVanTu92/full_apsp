<template>
    <div>
        <Loading v-if="loading"></Loading>
        <DataTable
            :value="dataTable"
            showGridlines
            tableStyle="min-width: 50rem"
            paginator
            lazy
            :rows="pagination.rows"
            :totalRecords="pagination.total"
            @page="changePage"
            :rowsPerPageOptions="[5, 10, 20, 50, 100]"
            filterDisplay="menu"
            :filterLocale="'vi'"
            :globalFilterFields="['packing']"
            v-model:filters="filters"
            @filter="onFilter"
        >
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText
                            class="w-19rem"
                            :placeholder="t('body.promotion.promotion_search_placeholder')"
                            v-model="filters['global'].value"
                            @keyup.enter="onFilter()"
                        />
                        <InputIcon>
                            <i class="pi pi-search" @click="onFilter()" />
                        </InputIcon>
                    </IconField>
                    <Button
                        type="button"
                        icon="pi pi-filter-slash"
                        :label="t('body.OrderApproval.clear')"
                        outlined
                        @click="clearFilter()"
                    />
                </div>
                <div
                    class="flex align-items-center gap-1 mt-2"
                    v-if="filters.packing.value?.length"
                >
                    <div
                        class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                    >
                        <small>{{ t('body.productManagement.packaging') }}: </small>
                    </div>
                    <div
                        class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                        v-for="item in filters.packing.value"
                        :key="item"
                    >
                        <small>{{
                            PackingContent.find((packing) => packing.id == item)?.name
                        }}</small>
                        <i
                            class="pi pi-times text-red-500"
                            @click="RemoveFilter(item, 'packing')"
                        ></i>
                    </div>
                </div>
            </template>
            <Column :header="t('body.productManagement.image_column')" class="w-1rem">
                <template #body="{ data }">
                    <Image
                        :src="data.itM1[0]?.filePath || 'https://placehold.co/40x40'"
                        width="40px"
                        height="40px"
                        :preview="!!data.itM1[0]?.filePath"
                    ></Image>
                </template>
            </Column>
            <Column field="itemCode" :header="t('body.productManagement.product_code_column')"></Column>
            <Column field="itemName" :header="t('body.productManagement.product_name_column')"></Column>
            <Column field="itemType.name" :header="t('body.productManagement.product_type')"></Column>
            <Column
                field="packing"
                :header="t('body.productManagement.packaging')"
                :showFilterMatchModes="false"
            >
                <template #body="slotProps">
                    {{ slotProps.data.packing.name }}
                </template>
                <template #filter="{ filterModel }">
                    <div class="flex flex-column gap-2">
                        <MultiSelect
                            v-model="filterModel.value"
                            display="chip"
                            :maxSelectedLabels="3"
                            :options="PackingContent"
                            optionLabel="name"
                            filter
                            optionValue="id"
                            :placeholder="t('body.productManagement.packaging_search_placeholder')"
                        />
                    </div>
                </template>
            </Column>
            <Column style="width: 1rem">
                <template #body="{ data }">
                    <div class="flex gap-1">
                        <Button text @click="props.clickEdit(data)" icon="pi pi-pencil" />
                        <Button
                            v-if="0"
                            text
                            @click="props.clickDelete(data)"
                            severity="danger"
                            icon="pi pi-trash"
                        />
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="p-5 text-center">{{ t('body.promotion.no_data_message') }}</div>
            </template>
        </DataTable>
    </div>
</template>

<script setup lang="js">
import { useGlobal } from "@/services/useGlobal";
import { onBeforeMount, onMounted, ref, watch } from "vue";
import API from "@/api/api-main";
import { FilterMatchMode } from "primevue/api";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps(['isFilter', 'clickEdit', 'clickDelete']);
watch(() => props.isFilter, (newVal) => {
  onFilter();
});
const { toast, FunctionGlobal } = useGlobal();
const dataTable = ref([]);
const pagination = ref({
  page: 0,
  rows: 10,
  total: 0,
});
const loading = ref(false);
const loadData = async (filters = "") => {
  loading.value = true;
  try {
    const res = await API.get(`Item?Page=${pagination.value.page + 1}&PageSize=${pagination.value.rows}${filters}&itemType=,16,`);
    if (res.data) {
      dataTable.value = res.data.items;
      pagination.value.total = res.data.total;
    }
  } catch (e) {
    FunctionGlobal.$notify("E", e.response.data.errors, toast);
  }
  finally{
    loading.value = false;
  }
};

onBeforeMount(async () => {
  await loadData();
});

const changePage = (event) => {
  pagination.value.page = event.page;
  pagination.value.rows = event.rows;
  onFilter();
};

//filter
const filters = ref({});

const initFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    packing: { value: null, matchMode: FilterMatchMode.IN },
  };
};
initFilters();

const PackingContent = ref([]);
const GetPacking = async () => {
  try {
    const res = await API.get("Packing/getall");
    if (res.data) {
      PackingContent.value = res.data;
    }
  } catch (e) {
    FunctionGlobal.$notify("E", e.response.data.errors, toast);
  }
};

onBeforeMount(() => {
  onFilter();
  GetPacking();
});

const onFilter = () => {
  const { global, packing } = filters.value;
  let queryString = packing.value ? `&packing=,${packing.value + ','}` : '';
  if (global.value) {
    queryString += `&search=${global.value}`;
  }
  loadData(queryString);
};



const clearFilter = () => {
  initFilters();
  loadData();
};

const RemoveFilter = (i, type) => {
  const resetFilter = {
    packing: () => {
      const filteredPacking = filters.value.packing.value.filter(
        (item) => item !== i
      );
      filters.value.packing.value = filteredPacking.length ? filteredPacking : null;
    },
  };
  if (resetFilter[type]) {
    resetFilter[type]();
    onFilter();
  }
};
</script>
