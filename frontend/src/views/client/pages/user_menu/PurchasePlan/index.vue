<template>
  <loading v-if="isLoading"></loading>
  <div class="flex justify-content-between align-items-center mb-3">
    <div class="font-bold text-2xl">{{ t('body.sampleRequest.importPlan.title') }}</div>
    <Button icon="pi pi-plus" :label="t('body.sampleRequest.importPlan.create')" @click="visible = true" />
  </div>
  <div class="card pb-0">
    <div class="flex flex-column gap-3">
      <DataTable showGridlines stripedRows paginator lazy :value="data" :rows="paginator.pageSize"
        :totalRecords="paginator.total"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
        :rowsPerPageOptions="[10, 20, 30]" @page="onPage($event)" dataKey="id" filterDisplay="menu"
        v-model:filters="filterStore.filters" :filterLocale="'vi'" @filter="onFilter()">
        <template #header>
          <div class="flex justify-content-between">
            <IconField iconPosition="left">
              <InputText :placeholder="t('body.home.search_placeholder')" v-model="filterStore.filters['global'].value"
                @keyup.enter="onFilter()" @input="debouncedFilter" />
              <InputIcon>
                <i class="pi pi-search" @click="onFilter()" />
              </InputIcon>
            </IconField>
            <Button type="button" icon="pi pi-filter-slash" :label="t('client.clear_filter')" outlined
              @click="clearFilter()" />
          </div>
        </template>
        <Column field="planCode" :header="t('body.sampleRequest.importPlan.code')" class="w-10rem">
          <template #body="{ data }">
            <router-link class="font-bold text-primary underline" :to="`/client/setup/purchase-plan/${data.id}`">{{
              data.planCode }}</router-link>
          </template>
        </Column>
        <Column field="planName" :header="t('body.sampleRequest.importPlan.name')"></Column>
        <Column field="startDate" class="w-12rem" :header="t('body.sampleRequest.importPlan.startDate')"
          filterField="startDate" dataType="date" :filterMatchModeOptions="[
            { label: t('body.report.table_header_month_1'), value: FilterMatchMode.DATE_AFTER },
            { label: t('body.report.table_header_month_2'), value: FilterMatchMode.DATE_BEFORE },
          ]">
          <template #body="{ data }">
            {{ dateFnsFormat(new Date(data.startDate), "dd/MM/yyyy") }}
          </template>
          <template #filter="{ filterModel }">
            <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy"
              :placeholder="t('body.report.from_date_placeholder')" mask="99/99/9999" />
          </template>
        </Column>
        <Column field="endDate" :header="t('body.sampleRequest.importPlan.endDate')" filterField="endDate"
          dataType="date" class="w-12rem" :filterMatchModeOptions="[
            { label: t('body.report.table_header_month_1'), value: FilterMatchMode.DATE_AFTER },
            { label: t('body.report.table_header_month_2'), value: FilterMatchMode.DATE_BEFORE },
          ]">
          <template #body="{ data }">
            {{ dateFnsFormat(new Date(data.endDate), "dd/MM/yyyy") }}
          </template>
          <template #filter="{ filterModel }">
            <Calendar v-model="filterModel.value" dateFormat="dd/mm/yy"
              :placeholder="t('body.report.from_date_placeholder')" mask="99/99/9999" />
          </template>
        </Column>
        <Column field="status" :header="t('body.sampleRequest.importPlan.status')" :showFilterMatchModes="false"
          class="w-10rem">
          <template #body="{ data }">
            <Tag :value="getStatusLabel(data.status).label" :severity="getStatusLabel(data.status).severity"></Tag>
          </template>
          <template #filter="{ filterModel }">
            <MultiSelect v-model="filterModel.value" :options="statuses" optionLabel="name" optionValue="code"
              :placeholder="t('body.promotion.select_status')" class="p-column-filter" showClear>
              <template #option="slotProps">
                <Tag :value="getStatusLabel(slotProps.option.code).label"
                  :severity="getStatusLabel(slotProps.option.code).severity" />
              </template>
            </MultiSelect>
          </template>
        </Column>
        <Column class="w-5rem" :header="t('body.sampleRequest.importPlan.actions')">
          <template #body="sp">
            <div class="flex gap-1">
              <Button :disabled="sp.data.status !== 'P'" icon="pi pi-pencil" text @click="onEdit(sp.data)"
                v-tooltip="t('client.edit')" />
              <Button :disabled="sp.data.status !== 'P'" icon="pi pi-trash" text severity="danger"
                @click="deleteConfirmation(sp.data)" v-tooltip="t('body.OrderList.delete')" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="py-5 my-5 text-center">{{ t('client.no_data') }}</div>
        </template>
      </DataTable>
    </div>
  </div>
  <CreateDialog v-model:visible="visible" :getAllData="onFilter" />
  <UpdateDialog v-model:visible="visibleUpdate" :getAllData="onFilter" :data="dataUpdate" />
  <ConfirmDialog></ConfirmDialog>
</template>

<script setup>
import { onBeforeMount, ref } from "vue";
import { useRouter } from "vue-router";
import CreateDialog from "./components/CreateDialog.vue";
import API from "@/api/api-main";
import { format as dateFnsFormat } from "date-fns";
import { inject } from "vue";
import { FilterStore } from "@/Pinia/Filter/FilterStoreOrderPlanning";
import { FilterMatchMode } from "primevue/api";
import format from "@/helpers/format.helper";
import UpdateDialog from "./components/UpdateDialog.vue";
import { useGlobal } from "@/services/useGlobal";
import { useConfirm } from "primevue/useconfirm";
import { useI18n } from "vue-i18n";
const confirm = useConfirm();

const { toast, FunctionGlobal } = useGlobal();

const { t } = useI18n();

const filterStore = FilterStore();
const conditionHandler = inject("conditionHandler");
const router = useRouter();
const visible = ref(false);
const visibleUpdate = ref(false);
const isLoading = ref(false);
const data = ref([]);
const paginator = ref({
  page: 0,
  pageSize: 10,
  total: 0,
});

const getAllData = async () => {
  const filters = conditionHandler.getQuery(filterStore.filters);
  isLoading.value = true;
  try {
    const queryParam = `?skip=${paginator.value.page}&limit=${paginator.value.pageSize}${filters}`;
    const res = await API.get(`sale-forecast/me${queryParam}`);
    data.value = res.data.plans;
    paginator.value.total = res.data.total;
    router.push(queryParam);
  } catch (err) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

const onPage = (event) => {
  paginator.value.page = event.page;
  paginator.value.pageSize = event.rows;
  getAllData();
};

const labels = {
  P: {
    severity: "info",
    label: t('body.OrderApproval.pending'),
  },
  A: {
    severity: "success",
    label: t('body.OrderList.confirmed'),
  },
  R: {
    severity: "danger",
    label: t('body.OrderList.cancelled'),
  },
};

const getStatusLabel = (str) => {
  return labels[str] || { severity: "info", label: t('body.OrderApproval.pending') };
};
const dataUpdate = ref(null);
const onEdit = (data) => {
  dataUpdate.value = { ...data };
  visibleUpdate.value = true;
};
const deleteConfirmation = (data) => {
  confirm.require({
    message: "Bạn có muốn huỷ kế hoạch nhập hàng này?",
    header: t('client.confirm'),
    icon: "pi pi-info-circle",
    rejectLabel: t('client.cancel'),
    acceptLabel: t('client.confirm'),
    rejectClass: "p-button-secondary p-button-outlined",
    acceptClass: "p-button-danger",
    accept: async () => {
      await onDelete(data);
    },
    reject: () => { },
  });
};

const onDelete = async (data) => {
  try {
    const res = await API.update(`sale-forecast/${data.id}`, {
      ...data,
      status: "R",
      author: null,
    });
    if (res.data) {
      toast.add({
        severity: "success",
        summary: t('body.systemSetting.success_label'),
        detail: "Xóa kế hoạch nhập hàng thành công",
        life: 3000,
      });
    }
    onFilter();
  } catch (error) {
    console.error(error);
  }
};

//Filter
const statuses = ref([
  { code: "P", name: t('body.OrderApproval.pending') },
  { code: "A", name: t('body.OrderList.confirmed') },
  { code: "R", name: t('body.OrderList.cancelled') },
]);
onBeforeMount(() => {
  getAllData();
});

const onFilter = () => {
  paginator.value.page = 0;
  getAllData();
};
let debounceTimeout = null;
const debouncedFilter = () => {
  if (debounceTimeout) {
    clearTimeout(debounceTimeout);
  }
  debounceTimeout = setTimeout(() => {
    onFilter();
  }, 1000);
};
const clearFilter = () => {
  filterStore.resetFilters();
  getAllData();
};
</script>

<style></style>
