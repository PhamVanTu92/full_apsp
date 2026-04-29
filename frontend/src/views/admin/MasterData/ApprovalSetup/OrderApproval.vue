<script setup>
import { onMounted, ref, computed, inject } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import debounce from "lodash/debounce";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useMeStore } from "@/Pinia/me";
import format from "@/helpers/format.helper";
import { FilterStore } from "@/Pinia/Filter/filterStoreApproval";
import { FilterMatchMode } from "primevue/api";
const conditionHandler = inject("conditionHandler");
const router = useRouter();
const route = useRoute();
const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();
const meStore = useMeStore();
const me = ref(null);
const isLoading = ref(false);
const expandedRows = ref({});
const approvalOrder = ref([]);
const processData = ref({});
const status_approval = ref({});
const keySearchOrders = ref("");
const filterStore = FilterStore();
const dataTable = ref({
  page: 0,
  size: 10,
  total_size: 0,
});

const statusLabels = computed(() => ({
  0: { severity: "warning", label: t("body.OrderApproval.pending") },
  1: { severity: "success", label: t("body.OrderApproval.approved") },
  2: { severity: "danger", label: t("body.OrderApproval.rejected") },
  3: { severity: "info", label: t("body.OrderList.cancelled") },
}));
const statusesFilter = ref([
  { code: 0, name: t("body.OrderApproval.pending") },
  { code: 1, name: t("body.OrderApproval.approved") },
  { code: 2, name: t("body.OrderApproval.rejected") },
  { code: 3, name: t("body.OrderList.cancelled") },
]);
const getStatusLabel = (status) => {
  return statusLabels.value[status] || { severity: "warning", label: "Không xác định" };
};

const approvalOptions = computed(() => [
  { name: t("body.OrderApproval.approval"), code: 1 },
  { name: t("body.OrderApproval.rejected"), code: 2 },
]);

const rowsPerPageOptions = computed(() =>
  Array.from({ length: 10 }, (_, i) => (i + 1) * 10)
);

const buildQueryString = () => {
  const params = new URLSearchParams();
  if (keySearchOrders.value) params.append("search", keySearchOrders.value);
  params.append("Page", dataTable.value.page + 1);
  params.append("PageSize", dataTable.value.size);
  if (route.params.id) params.append("filter", `id=${route.params.id}`);
  params.append("OrderBy", "id desc");
  return params.toString();
};

// ===== API Calls =====
const fetchApprovalOrder = async () => {
  isLoading.value = true;
  try {
    const filters = conditionHandler.getQuery(filterStore.filters);
    const res = await API.get(
      `ApprovalWorkFlow?${buildQueryString()}` + (route.params.id ? "" : filters)
    );
    if (res?.data) {
      approvalOrder.value = res.data.result || [];
      dataTable.value.total_size = res.data.total || 0;
    }
  } catch (error) {
    console.error("Error fetching approval orders:", error);
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    isLoading.value = false;
  }
};

const fetchApprovalDetails = async (id) => {
  try {
    const res = await API.get(`ApprovalWorkFlow/${id}`);
    return res?.data?.approvalWorkFlowLines || [];
  } catch (error) {
    console.error(`Error fetching approval details for ID ${id}:`, error);
    return [];
  }
};

const onPageChange = (event) => {
  dataTable.value.page = event.page;
  dataTable.value.size = event.rows;
  fetchApprovalOrder();
};

const onRowExpand = async (event) => {
  const rowId = event.data.id;
  if (!processData.value[rowId]) {
    const lines = await fetchApprovalDetails(rowId);
    processData.value[rowId] = lines;
    status_approval.value[rowId] = false;
  }
};

const onSearch = debounce(() => {
  dataTable.value.page = 0;
  fetchApprovalOrder();
}, 500);

const isUserApprover = (line) => {
  return me.value?.user?.id === line.approvalUserId && !line.updatedAt;
};

const updateApproval = async (rowId) => {
  isLoading.value = true;
  try {
    const lines = processData.value[rowId] || [];
    const approvalPromises = lines.filter(isUserApprover).map((item) =>
      API.add("ApprovalWorkFlow/approve", {
        approvalDecisionId: rowId,
        approvalDecisionLineId: item.id,
        status: item.status,
        note: item.note || "",
      })
    );
    await Promise.all(approvalPromises);
    processData.value[rowId] = await fetchApprovalDetails(rowId);
    status_approval.value[rowId] = false;
    FunctionGlobal.$notify(
      "S",
      t("body.systemSetting.approvalLevel.messages.updateSuccess"),
      toast
    );
    await fetchApprovalOrder();
  } catch (error) {
    console.error("Error updating approval:", error);
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    isLoading.value = false;
  }
};

const canEditApproval = (lineData) => {
  return me.value?.user?.id === lineData.approvalUser?.id && !lineData.updatedAt;
};

const handleApprovalChange = (rowId) => {
  status_approval.value[rowId] = true;
};

// ===== Navigation =====
const backToList = () => {
  router.push({ name: "orderApproval" });
  route.params.id = "";
  fetchApprovalOrder();
};

const loadApprovalById = async (id) => {
  const lines = await fetchApprovalDetails(id);
  if (lines.length > 0) {
    processData.value[id] = lines;
    expandedRows.value = { [id]: true };
  }
};

onMounted(async () => {
  me.value = await meStore.getMe();
  await fetchApprovalOrder();
  if (route.params.id) {
    await loadApprovalById(route.params.id);
  }
});
</script>

<template>
  <div class="flex justify-content-between align-items-center mb-4">
    <h4 class="font-bold m-0">
      {{
        route.params.id ? t("Feedback.approvalTitleDetail") : t("Feedback.approvalTitle")
      }}
    </h4>
  </div>

  <div class="grid mt-3 card p-2">
    <div class="col-12">
      <DataTable
        v-model:expandedRows="expandedRows"
        :value="approvalOrder"
        dataKey="id"
        class="table-main"
        showGridlines
        paginator
        lazy
        :rows="dataTable.size"
        :page="dataTable.page"
        :totalRecords="dataTable.total_size"
        :rowsPerPageOptions="rowsPerPageOptions"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :currentPageReportTemplate="
          t('Feedback.currentPageReportTemplate') +
          ' {first} - {last} ' +
          t('Feedback.upTotal') +
          ' {totalRecords} ' +
          t('Feedback.request')
        "
        v-model:filters="filterStore.filters"
        @filter="fetchApprovalOrder"
        filterDisplay="menu"
        tableStyle="min-width: 50rem"
        @page="onPageChange"
        @rowExpand="onRowExpand"
      >
        <template #empty>
          <div class="py-5 my-5 text-center">
            {{ t("body.OrderList.noData") }}
          </div>
        </template>
        <template #header>
          <div class="flex justify-content-between align-items-center">
            <IconField iconPosition="left">
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText
                v-model="keySearchOrders"
                :placeholder="t('body.OrderApproval.searchPlaceholder')"
                @input="onSearch"
              />
            </IconField>

            <Button
              v-if="route.params.id"
              type="button"
              icon="pi pi-list"
              :label="t('Evaluate.backToList')"
              outlined
              @click="backToList"
            />
          </div>
        </template>
        <Column expander style="width: 3rem" />
        <Column header="#" style="width: 3rem">
          <template #body="{ index }">
            {{ index + 1 + dataTable.page * dataTable.size }}
          </template>
        </Column>

        <Column :header="t('body.OrderApproval.documentName')">
          <template #body="{ data }">
            <router-link
              :to="{
                name:
                  data.approvalWorkFlowDocumentLines?.[0]?.docObj?.objType == 22
                    ? 'order-detail'
                    : 'DetailPurchaseRequest',
                params: { id: data.approvalWorkFlowDocumentLines?.[0]?.docObj?.id },
              }"
            >
              <span class="text-primary cursor-pointer text-bold">
                {{ data.approvalWorkFlowDocumentLines?.[0]?.docObj?.invoiceCode || "-" }}
              </span>
            </router-link>
          </template>
        </Column>
        <Column field="creator.fullName" :header="t('body.OrderApproval.creator')" />
        <Column :header="t('body.OrderApproval.approvalTemplate')">
          <template #body="{ data }">
            <span>
              {{ data.approvalSample?.approvalSampleName || "-" }}
            </span>
          </template>
        </Column>
        <Column field="description" :header="t('body.OrderApproval.note')" />
        <Column
          :header="t('body.OrderApproval.status')"
          :showFilterMatchModes="false"
          field="approvalStatus"
          class="w-10rem"
        >
          <template #body="{ data }">
            <Tag
              :severity="getStatusLabel(data.approvalStatus).severity"
              :value="getStatusLabel(data.approvalStatus).label"
            />
          </template>
          <template #filter="{ filterModel }">
            <MultiSelect
              v-model="filterModel.value"
              :options="statusesFilter"
              optionLabel="name"
              optionValue="code"
              :placeholder="t('body.OrderList.orderStatus')"
              class="p-column-filter"
              showClear
            >
              <template #option="slotProps">
                <Tag
                  :severity="getStatusLabel(slotProps.option.code).severity"
                  :value="getStatusLabel(slotProps.option.code).label"
                />
              </template>
            </MultiSelect>
          </template>
        </Column>
        <Column
          :header="t('body.OrderApproval.createDate')"
          bodyClass="w-5rem"
          field="createdAt"
          filterField="createdAt"
          dataType="date"
          :filterMatchModeOptions="[
            {
              label: t('body.report.from_date_label_2'),
              value: FilterMatchMode.DATE_AFTER,
            },
            {
              label: t('body.report.to_date_label_2'),
              value: FilterMatchMode.DATE_BEFORE,
            },
          ]"
        >
          <template #body="{ data }">
            <span class="border-1 border-300 py-1">
              <span class="surface-300 px-3 p-1">
                {{ format.DateTimePlusUTC(data.createdAt).time }}
              </span>
              <span class="px-3 p-1">
                {{ format.DateTimePlusUTC(data.createdAt).date }}
              </span>
            </span>
          </template>
          <template #filter="{ filterModel }">
            <Calendar
              v-model="filterModel.value"
              dateFormat="dd/mm/yy"
              placeholder="dd/mm/yy"
              mask="99/99/9999"
            />
          </template>
        </Column>
        <!-- Expansion Row -->
        <template #expansion="{ data: rowData }">
          <div class="p-3">
            <DataTable
              v-if="processData[rowData.id]"
              :value="processData[rowData.id]"
              class="table-main"
              showGridlines
              tableStyle="min-width: 50rem"
            >
              <Column
                field="approvalLevel.approvalLevelName"
                :header="t('body.OrderApproval.process')"
                style="width: 15rem"
              />

              <Column
                field="approvalUser.fullName"
                :header="t('body.OrderApproval.approver')"
                style="width: 20rem"
              />

              <Column :header="t('body.OrderApproval.approval')" style="width: 12rem">
                <template #body="{ data: lineData }">
                  <Dropdown
                    v-model="lineData.status"
                    :options="approvalOptions"
                    optionLabel="name"
                    optionValue="code"
                    :placeholder="t('body.OrderApproval.result')"
                    :disabled="!canEditApproval(lineData)"
                    class="w-full"
                    @change="handleApprovalChange(rowData.id)"
                  />
                </template>
              </Column>

              <Column :header="t('body.OrderApproval.result')" style="width: 10rem">
                <template #body="{ data: lineData }">
                  <Tag
                    :value="getStatusLabel(lineData.status).label"
                    :severity="getStatusLabel(lineData.status).severity"
                  />
                </template>
              </Column>

              <Column :header="t('body.OrderApproval.note')" style="width: 12rem">
                <template #body="{ data: lineData }">
                  <Textarea
                    v-model="lineData.note"
                    autoResize
                    :disabled="!canEditApproval(lineData)"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('body.OrderApproval.approvalDate')" style="width: 12rem">
                <template #body="{ data: lineData }">
                  <span v-if="lineData.updatedAt">
                    {{ format.formatDate(lineData.updatedAt) }}
                  </span>
                  <span v-else class="text-muted">-</span>
                </template>
              </Column>
            </DataTable>

            <div class="flex justify-content-end mt-3">
              <Button
                v-if="status_approval[rowData.id]"
                :label="t('body.OrderApproval.confirm')"
                @click="updateApproval(rowData.id)"
              />
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>

  <loading v-if="isLoading" />
</template>
