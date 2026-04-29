<template>
    <div class="flex flex-column">
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.sampleRequest.importPlan.title') }}</h4>
            <Button
                icon="pi pi-plus"
                :label="t('body.sampleRequest.importPlan.create')"
                @click="visible = true"
            />
        </div>
        <div class="card p-3">
            <div class="flex flex-column gap-3">
                <DataTable
                    showGridlines
                    stripedRows
                    paginator
                    lazy
                    :value="data"
                    :rows="paginator.pageSize"
                    :totalRecords="paginator.total"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                    :rowsPerPageOptions="
                        Array.from({ length: 10 }, (_, i) => (i + 1) * 10)
                    "
                    @page="onPage($event)"
                    dataKey="id"
                    filterDisplay="menu"
                    v-model:filters="filterStore.filters"
                    :filterLocale="'vi'"
                    @filter="onFilter()"
                >
                    <template #header>
                        <div class="flex justify-content-between">
                            <IconField iconPosition="left">
                                <InputText
                                    :placeholder="t('body.sampleRequest.importPlan.searchPlaceholder')"
                                    v-model="filterStore.filters['global'].value"
                                    @input="debouncedFilter()"
                                />
                                <InputIcon>
                                    <i class="pi pi-search" @click="onFilter()" />
                                </InputIcon>
                            </IconField>
                            <Button
                                type="button"
                                icon="pi pi-filter-slash"
                                v-tooltip.bottom="t('body.OrderApproval.clear')"
                                severity="danger"
                                outlined
                                @click="clearFilter()"
                            />
                        </div>
                    </template>
                    <Column field="planCode" :header="t('body.sampleRequest.importPlan.code')" class="w-9rem">
                        <template #body="{ data }">
                            <router-link
                                class="text-primary font-semibold hover:underline"
                                :to="`/order-planning/detail/${data.id}`"
                            >
                                <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                                <span>{{ data.planCode }}</span>
                            </router-link>
                        </template>
                    </Column>
                    <Column
                        field="planName"
                        :header="t('body.sampleRequest.importPlan.name')"
                        class="w-10rem"
                    ></Column>
                    <Column field="customerName" :header="t('body.sampleRequest.importPlan.customer')" class="w-15rem">
                        <template #body="{ data }">
                            <router-link
                                class="text-primary font-semibold hover:underline"
                                :to="`/agen-man/agency-category/${data.customerId}`"
                            >
                                <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                                <span>{{ data.customerName }}</span>
                            </router-link>
                        </template>
                    </Column>
                    <Column
                        field="startDate"
                        :header="t('body.sampleRequest.importPlan.startDate')"
                        filterField="startDate"
                        dataType="date"
                        class="w-10rem"
                        :filterMatchModeOptions="[
                            { label: t('body.sampleRequest.importPlan.startDate'), value: FilterMatchMode.DATE_AFTER },
                            { label: t('body.sampleRequest.importPlan.endDate'), value: FilterMatchMode.DATE_BEFORE },
                        ]"
                    >
                        <template #body="{ data }">
                            {{ dateFnsFormat(new Date(data.startDate), "dd/MM/yyyy") }}
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
                    <Column
                        field="endDate"
                        :header="t('body.sampleRequest.importPlan.endDate')"
                        filterField="endDate"
                        dataType="date"
                        class="w-10rem"
                        :filterMatchModeOptions="[
                            { label: t('body.sampleRequest.importPlan.startDate'), value: FilterMatchMode.DATE_AFTER },
                            { label: t('body.sampleRequest.importPlan.endDate'), value: FilterMatchMode.DATE_BEFORE },
                        ]"
                    >
                        <template #body="{ data }">
                            {{ dateFnsFormat(new Date(data.endDate), "dd/MM/yyyy") }}
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
                    <Column
                        field="status"
                        :header="t('body.sampleRequest.importPlan.status')"
                        :showFilterMatchModes="false"
                        class="w-8rem"
                    >
                        <template #body="{ data }">
                            <!-- {{ data.status }} -->
                            <Tag
                                :value="getStatusLabel(data.status)['label']"
                                :severity="getStatusLabel(data.status)['severity']"
                            ></Tag>
                        </template>
                        <template #filter="{ filterModel }">
                            <MultiSelect
                                v-model="filterModel.value"
                                :options="statuses"
                                optionLabel="name"
                                optionValue="code"
                                :placeholder="t('body.sampleRequest.importPlan.status')"
                                class="p-column-filter"
                                showClear
                            >
                                <template #option="slotProps">
                                    <Tag
                                        :value="
                                            getStatusLabel(slotProps.option.code)['label']
                                        "
                                        :severity="
                                            getStatusLabel(slotProps.option.code)[
                                                'severity'
                                            ]
                                        "
                                    />
                                </template>
                            </MultiSelect>
                        </template>
                    </Column>
                    <Column class="w-5rem" :header="t('body.sampleRequest.importPlan.actions')">
                        <template #body="sp">
                            <div class="flex gap-1">
                                <Button
                                    :disabled="
                                        sp.data.status !== 'P' ||
                                        sp.data.author.id !==
                                            authStore.user?.appUser?.id
                                    "
                                    icon="pi pi-pencil"
                                    text
                                    @click="onEdit(sp.data)"
                                    v-tooltip="t('body.OrderList.edit')"
                                />
                                <Button
                                    :disabled="
                                        sp.data.status !== 'P' ||
                                        sp.data.author?.userType != 'APSP'
                                    "
                                    icon="pi pi-ban"
                                    text
                                    severity="danger"
                                    @click="deleteConfirmation(sp.data)"
                                    v-tooltip="t('body.PurchaseRequestList.cancel_button')"
                                />
                            </div>
                        </template>
                    </Column>
                    <template #empty>
                        <div class="p-8 m-8 text-center">{{ t('body.OrderList.noData') }}</div>
                    </template>
                </DataTable>
            </div>
        </div>
    </div>
    <ConfirmDialog></ConfirmDialog>
    <CreateDialog v-model:visible="visible" :getAllData="onFilter" :header="t('body.sampleRequest.importPlan.page_title')"/>
    <UpdateDialog
        v-model:visible="visibleUpdate"
        :getAllData="onFilter"
        :data="dataUpdate"
    />
    <loading v-if="isLoading"></loading>
</template>
<script setup>
import { onBeforeMount, ref } from "vue";
import { useRouter } from "vue-router";
import API from "@/api/api-main";
import { format as dateFnsFormat } from "date-fns";
import { inject } from "vue";
import { FilterStore } from "@/Pinia/Filter/FilterStoreOrderPlanning.js";
import { FilterMatchMode } from "primevue/api";
import format from "@/helpers/format.helper";
import CreateDialog from "../../client/pages/user_menu/PurchasePlan/components/CreateDialog.vue";
import UpdateDialog from "../../client/pages/user_menu/PurchasePlan/components/UpdateDialog.vue";
import { useConfirm } from "primevue/useconfirm";
import { useGlobal } from "@/services/useGlobal";
import { useAuthStore } from "@/Pinia/auth";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const authStore = useAuthStore();
const confirm = useConfirm();
const { toast, FunctionGlobal } = useGlobal();
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
        const res = await API.get(`sale-forecast${queryParam}`);
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
        severity: "warning",
        label: t('body.status.CXN'),
    },
    A: {
        severity: "success",
        label: t('body.status.DXN'),
    },
    R: {
        severity: "danger",
        label: t('body.status.DH'),
    },
};

const getStatusLabel = (str) => {
    return labels[str] || { severity: "warning", label: "Unknown" };
};
const dataUpdate = ref(null);
const onEdit = (data) => {
    dataUpdate.value = { ...data };
    visibleUpdate.value = true;
};

const deleteConfirmation = (data) => {
    confirm.require({
        message: "Bạn có muốn huỷ kế hoạch nhập hàng này?",
        header: "Xác nhận huỷ",
        icon: "pi pi-info-circle",
        rejectLabel: t('body.PurchaseRequestList.cancel_button'),
        acceptLabel: t('body.OrderApproval.confirm'),
        rejectClass: "p-button-secondary p-button-outlined",
        acceptClass: "p-button-danger",
        accept: async () => {
            await onDelete(data);
        },
        reject: () => {},
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
        toast.add({
            severity: "error",
            summary: "Thất bại",
            detail: error.response.data.errors,
            life: 3000,
        });
    }
};

//Filter
const statuses = ref([
    { code: "P", name: t('body.status.CXN') },
    { code: "A", name: t('body.status.DXN') },
    { code: "R", name: t('body.status.DH') },
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
