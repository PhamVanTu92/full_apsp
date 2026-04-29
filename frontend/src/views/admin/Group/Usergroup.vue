<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <h4 class="font-bold m-0">{{ t('systemSetting.user_management_title') }}</h4>
        <div class="flex gap-2">
            <Button :label="t('body.customerGroup.addNew')" icon="fa-solid fa-plus" @click="NewItem()"/>
        </div>
    </div>

    <div class="grid mt-3 card p-2">
        <div class="col-12 h-screen bg-white">
            <DataTable
                class="table-main"
                showGridlines
                dataKey="id"
                :value="Groups"
                stripedRows
                selectionMode="single"
                tableStyle="min-width: 50rem;"
                header="surface-200"
                paginator
                :rows="dataTable.rows"
                :page="dataTable.page"
                :totalRecords="dataTable.totalRecords"
                @page="onPageChange($event)"
                :rowsPerPageOptions="[10, 20, 30]"
                lazy
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} nhóm nhà phân phối"
                @filter="onFilter"
                :filterLocale="'vi'"
                filterDisplay="menu"
                v-model:filters="filterStore.filters"
                :globalFilterFields="['name']"
            >
                <template #header>
                    <div class="flex justify-content-between">
                        <IconField iconPosition="left">
                            <InputText
                                :placeholder="t('body.customerGroup.enter_keyword_placeholder')"
                                @keyup.enter="onFilter()"
                            />
                            <InputIcon>
                                <i class="pi pi-search" @click="onFilter()" />
                            </InputIcon>
                        </IconField>
                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            :label="t('client.clear_filter')"
                            outlined
                            @click="clearFilter()"
                        />
                    </div>
                    <div class="mt-2">
                        <div
                            class="flex align-items-center gap-1 mb-2"
                            v-if="filterStore.filters.isActive?.value !== null"
                        >
                            <div
                                class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                            >
                                <small>{{ t('client.status') }}: </small>
                            </div>
                            <div
                                class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                            >
                                <small>{{
                                    filterStore.filters.isActive?.value
                                        ? t('client.active_status')
                                        : t('client.inactive_status')
                                }}</small>
                                <i
                                    class="pi pi-times text-red-500 cursor-pointer"
                                    @click="
                                        RemoveFilter(
                                            filterStore.filters.isActive?.value,
                                            'isActive'
                                        )
                                    "
                                ></i>
                            </div>
                        </div>
                        <div
                            class="flex align-items-center gap-1 mb-2"
                            v-if="filterStore.filters.createdAt?.constraints[0].value"
                        >
                            <div
                                class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                            >
                                <small
                                    >{{
                                        setLabelContidition(
                                            filterStore.filters.createdAt?.constraints[0]
                                                .matchMode
                                        ) +
                                        " " +
                                        format.DateTime(
                                            filterStore.filters.createdAt?.constraints[0]
                                                .value
                                        ).date
                                    }}
                                    {{
                                        filterStore.filters.createdAt?.constraints[1]
                                            ?.value
                                            ? setLabelContidition(
                                                  filterStore.filters.createdAt?.operator
                                              ) +
                                              " " +
                                              setLabelContidition(
                                                  filterStore.filters.createdAt
                                                      ?.constraints[1].matchMode
                                              ) +
                                              " " +
                                              format.DateTime(
                                                  filterStore.filters.createdAt
                                                      ?.constraints[1].value
                                              ).date
                                            : ""
                                    }}</small
                                >
                                <i
                                    class="pi pi-times text-red-500 cursor-pointer"
                                    @click="
                                        RemoveFilter(
                                            filterStore.filters.createdAt?.constraints[0]
                                                .value,
                                            'createdAt'
                                        )
                                    "
                                ></i>
                            </div>
                        </div>
                        <div
                            class="flex align-items-center gap-1 mb-2"
                            v-if="filterStore.filters.updatedAt?.constraints[0].value"
                        >
                            <div
                                class="p-2 border-round bg-gray-100 flex align-items-center justify-content-between gap-2"
                            >
                                <small
                                    >{{
                                        setLabelContidition(
                                            filterStore.filters.updatedAt?.constraints[0]
                                                .matchMode
                                        ) +
                                        " " +
                                        format.DateTime(
                                            filterStore.filters.updatedAt?.constraints[0]
                                                .value
                                        ).date
                                    }}
                                    {{
                                        filterStore.filters.updatedAt?.constraints[1]
                                            ?.value
                                            ? setLabelContidition(
                                                  filterStore.filters.updatedAt?.operator
                                              ) +
                                              " " +
                                              setLabelContidition(
                                                  filterStore.filters.updatedAt
                                                      ?.constraints[1].matchMode
                                              ) +
                                              " " +
                                              format.DateTime(
                                                  filterStore.filters.updatedAt
                                                      ?.constraints[1].value
                                              ).date
                                            : ""
                                    }}</small
                                >
                                <i
                                    class="pi pi-times text-red-500 cursor-pointer"
                                    @click="
                                        RemoveFilter(
                                            filterStore.filters.updatedAt?.constraints[0]
                                                .value,
                                            'updatedAt'
                                        )
                                    "
                                ></i>
                            </div>
                        </div>
                    </div>
                </template>
                <template #empty>
                    <div class="text-center">
                        <div class="my-5 py-5 text-center">{{ t('client.no_data_to_display') }}</div>
                    </div>
                </template>
                <Column header="#" :style="{ width: '3%' }">
                    <template #body="slotProps">
                        {{ slotProps.index + 1 }}
                    </template>
                </Column>
                <Column field="name" :header="t('body.customerGroup.name')" :style="{ width: '25%' }">
                </Column>
                <Column field="description" :header="t('body.customerGroup.description')" :style="{ width: '15%' }">
                    <template #body="slotProps">
                        <div>{{ slotProps.data.description }}</div>
                    </template>
                </Column>
                <Column
                    field="isActive"
                    :header="t('client.status')"
                    :style="{ width: '12%' }"
                    :showFilterMatchModes="false"
                >
                    <template #body="slotProps">
                        <div>
                            {{
                                slotProps.data.isActive ? t('client.active_status') : t('client.inactive_status')
                            }}
                        </div>
                    </template>
                    <template #filter="{ filterModel }">
                        <Dropdown
                            v-model="filterModel.value"
                            :options="statusOptions"
                            optionLabel="label"
                            optionValue="value"
                            @change="onFilter"
                            :placeholder="t('client.select_status')"
                            class="p-column-filter"
                            showClear
                        >
                            <template #option="slotProps">
                                <span>{{ slotProps.option.label }}</span>
                            </template>
                        </Dropdown>
                    </template>
                </Column>
                <Column
                    field="createdAt"
                    filterField="createdAt"
                    :header="t('client.created_date')"
                    :style="{ width: '12%' }"
                    dataType="date"
                    :filterMatchModeOptions=" [
                        { label: t('body.report.from_date_placeholder'), value: FilterMatchMode.DATE_AFTER },
                        { label: t('body.report.to_date_placeholder'), value: FilterMatchMode.DATE_BEFORE },
                    ]"
                >
                    <template #body="slotProps">
                        <span class="border-1 border-300 py-1">
                            <span class="surface-300 px-3 p-1">{{
                                format.DateTime(slotProps.data.createdAt).time
                            }}</span>
                            <span class="px-3 p-1">{{
                                format.DateTime(slotProps.data.createdAt).date
                            }}</span>
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
                <Column
                    field="updatedAt"
                    filterField="updatedAt"
                    :header="t('systemSetting.update_date_column')"
                    :style="{ width: '12%' }"
                    dataType="date"
                    :filterMatchModeOptions=" [
                        { label: t('body.report.from_date_placeholder'), value: FilterMatchMode.DATE_AFTER },
                        { label: t('body.report.to_date_placeholder'), value: FilterMatchMode.DATE_BEFORE },
                    ]"
                >
                    <template #body="slotProps">
                        <span class="border-1 border-300 py-1">
                            <span class="surface-300 px-3 p-1">{{
                                format.DateTime(slotProps.data.updatedAt).time
                            }}</span>
                            <span class="px-3 p-1">{{
                                format.DateTime(slotProps.data.updatedAt).date
                            }}</span>
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
                <Column :header="t('body.importPlan.actions')" :style="{ width: '10%' }">
                    <template #body="slotProps">
                        <div>
                            <Button
                                class="ml-1"
                                icon="pi pi-pencil"
                                text
                                @click="UpdateData(slotProps.data)"
                                :tooltip="t('client.edit')"
                            />
                            <Button
                                @click="deleteItem(slotProps.data)"
                                severity="danger"
                                icon="pi pi-trash"
                                text
                                :tooltip="t('body.OrderList.delete')"
                            />
                            <Button
                                class="ml-1"
                                icon="fa-solid fa-ellipsis-vertical"
                                text
                                @click="ActionCopy($event, slotProps.data)"
                            />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <Dialog
        v-model:visible="dialogItem"
        modal
        :draggable="false"
        :header="
            payload.id
                ? t('body.customerGroup.update') || 'CẬP NHẬT NHÓM NGƯỜI DÙNG'
                : payload.copy == true
                ? t('body.customerGroup.copy') || 'SAO CHÉP NHÓM NGƯỜI DÙNG'
                : t('body.customerGroup.add_new_customer_group') || 'THÊM MỚI NHÓM NGƯỜI DÙNG'
        "
        :style="{ width: '55%' }"
        class="p-fluid"
        :pt="{
            header: { style: 'border-bottom: 1px solid #e5e7eb;' },
        }"
    >
        <div class="grid p-3 mt-1 align-items-top">
            <div class="col-6">
                <div class="flex flex-column gap-2 border-1 border-solid border-200 p-3">
                    <div class="field">
                        <label for="group_name">{{ t('body.customerGroup.name') }}</label>
                        <InputText v-model="payload.name" :placeholder="t('body.customerGroup.enter_group_name_placeholder')" />
                    </div>
                    <div class="field">
                        <label for="description">{{ t('body.customerGroup.description') }}</label>
                        <Textarea
                            rows="5"
                            v-model="payload.description"
                            :placeholder="t('body.customerGroup.description')"
                            autoResize
                        />
                    </div>
                    <div class="flex gap-5">
                        <div>
                            <RadioButton
                                v-model="payload.isActive"
                                inputId="Check"
                                :value="true"
                            />
                            <label for="Check" class="ml-2">{{ t('client.active_status') }}</label>
                        </div>
                        <div>
                            <RadioButton
                                v-model="payload.isActive"
                                inputId="Check1"
                                :value="false"
                            />
                            <label for="Check1" class="ml-2">{{ t('client.inactive_status') }}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-6">
                <ListUser v-model="payload.listUsers" :idGroup="payload.id" />
            </div>
        </div>

        <template #footer>
            <div class="flex justify-end gap-2">
                <Button
                    type="button"
                    :label="t('client.cancel')"
                    severity="secondary"
                    @click="dialogItem = false"
                />
                <Button type="button" :label="t('client.save')" @click="SaveItem()"/>
            </div>
        </template>
    </Dialog>
    <Dialog
        v-model:visible="dialogDeleteItem"
        modal
        position="center"
        :draggable="false"
        :header="t('client.confirm')"
        :style="{ width: '35%' }"
        class="p-fluid"
    >
        <div
            class="flex flex-column align-items-center w-full gap-3 border-bottom-1 surface-border pb-0"
        >
            <i class="pi pi-info-circle text-6xl text-red-500"></i>
            <p>
                {{ t('body.OrderList.delete') }}?
                <span class="font-bold text-red-500">{{ dataEdit.dataDelete.name }}</span>
            </p>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    :label="t('client.cancel')"
                    outlined
                    severity="secondary"
                    @click="confirmDelete(false)"
                />
                <Button :label="t('body.OrderList.delete')" @click="confirmDelete(true)" severity="danger" />
            </div>
        </template>
    </Dialog>

    <OverlayPanel ref="opcopy" appendTo="body">
        <div class="flex flex-column gap-3">
            <Button icon="fa-solid fa-copy" text @click="CopyGroup()"/>
        </div>
    </OverlayPanel>
    <Loading v-if="loading" />
</template>

<script setup>
import { ref, onBeforeMount } from "vue";
import API from "@/api/api-main";
import format from "@/helpers/format.helper";
import { merge, omit } from "lodash";
import { useGlobal } from "@/services/useGlobal";
import { FilterMatchMode } from "primevue/api";
import { FilterStore } from "@/Pinia/Filter/FilterStoreUsergroup.js";
import { useRouter } from "vue-router";
import ListUser from "./ListUser.vue";
import { inject } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const conditionHandler = inject("conditionHandler");
const router = useRouter();
const Groups = ref([]);
const loading = ref(false);
const { toast, FunctionGlobal } = useGlobal();
const dataTable = ref({
    page: 0,
    rows: 10,
    totalRecords: 0,
});
const payload = ref({
    description: "",
    group_code: "",
    name: "",
    isActive: true,
    listUsers: [],
    id: 0,
});
const statusOptions = ref([
    { label: t('client.active_status'), value: true },
    { label: t('client.inactive_status'), value: false },
]);
const dataClear = JSON.stringify(payload.value);
const dialogItem = ref(false);
const dialogDeleteItem = ref(false);
const dataEdit = ref({ submited: false });
const opcopy = ref("");
const idCopy = ref("");
const filterStore = FilterStore();
onBeforeMount(() => {
    fetchDataGroups();
});

const onFilter = () => {
    dataTable.value.page = 0;
    fetchDataGroups();
};

const setLabelContidition = (con) => {
    const locale = {
        startsWith: "bắt đầu với",
        contains: "chứa",
        notContains: "không chứa",
        endsWith: "kết thúc với",
        equals: "bằng",
        notEquals: "không bằng",
        noFilter: "không có bộ lọc",
        dateIs: "Ngày là",
        dateIsNot: "Ngày không phải là",
        dateBefore: "Đến ngày",
        dateAfter: "Từ ngày",
        clear: "Xóa",
        apply: "Áp dụng",
        matchAll: "Tất cả",
        matchAny: "Bất kỳ",
        addRule: "Thêm quy tắc",
        removeRule: "Xóa quy tắc",
        accept: "Đồng ý",
        reject: "Hủy",
        choose: "Chọn",
        upload: "Tải lên",
        cancel: "Hủy",
        and: "và",
        or: "hoặc",
    };
    return locale[con];
};

const clearFilter = () => {
    filterStore.resetFilters();
    fetchDataGroups();
};
const RemoveFilter = (i, type) => {
    const resetFilter = {
        isActive: () => {
            filterStore.filters.isActive = {
                value: null,
                matchMode: FilterMatchMode.EQUALS,
            };
        },
        createdAt: () => {
            filterStore.filters.createdAt = {
                operator: "and",
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
            };
        },
        updatedAt: () => {
            filterStore.filters.updatedAt = {
                operator: "and",
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
            };
        },
    };
    if (resetFilter[type]) {
        resetFilter[type]();
        onFilter();
    }
};

const fetchDataGroups = async () => {
    loading.value = true;
    const filters = conditionHandler.getQuery(filterStore.filters);
    try {
        let queryParams = `?skip=${dataTable.value.page}&limit=${dataTable.value.rows}${filters}`;
        const res = await API.get(`UserGroup${queryParams}`);
        Groups.value = res.data.groups;
        Groups.value.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        dataTable.value.totalRecords = res.data.total;
        router.push(queryParams);
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

const onPageChange = (event) => {
    dataTable.value.page = event.page;
    dataTable.value.rows = event.rows;
    fetchDataGroups();
};
const NewItem = () => {
    ClearData();
    dialogItem.value = true;
};
const UpdateData = async (data) => {
    try {
        loading.value = true;
        const res = await API.get(`UserGroup/${data.id}`);
        if (res.data) payload.value = res.data;
        dialogItem.value = true;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

const SaveItem = async () => {
    if (!ValidateData()) return;
    loading.value = true;
    const data = { ...payload.value };
    const FUNAPI = data.id
        ? API.update(`UserGroup/${data.id}`, omit(data, "listUsers"))
        : API.add(`UserGroup`, omit(data, "listUsers"));
    try {
        const res = await FUNAPI;
        if (res.data) {
            const dataUser = payload.value.listUsers
                .filter((item) => !item.groupId)
                .map((item) => item.id);
            API.add(`UserGroup/${res.data.id}/users`, dataUser);
            const message = payload.value.id
                ? "Cập nhật nhóm người dùng thành công"
                : "Thêm nhóm người dùng mới thành công";
            FunctionGlobal.$notify("S", message, toast);
            ClearData();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};
const ClearData = () => {
    payload.value = JSON.parse(dataClear);
    dialogItem.value = false;
    fetchDataGroups();
};

const deleteItem = (data) => {
    dataEdit.value.dataDelete = data;
    dialogDeleteItem.value = true;
};

const confirmDelete = async (status) => {
    if (status) {
        try {
            loading.value = true;
            const res = await API.delete(`UserGroup/${dataEdit.value.dataDelete.id}`);
            if (res.data) FunctionGlobal.$notify("S", "Đã xoá thành công", toast);
        } catch (error) {
            FunctionGlobal.$notify("E", error, toast);
        } finally {
            loading.value = false;
            dialogDeleteItem.value = false;
            fetchDataGroups();
        }
    } else {
        dialogDeleteItem.value = false;
        dataEdit.value.dataDelete = {};
    }
};

const ValidateData = () => {
    let status = true;
    const fields = [
        {
            value: payload.value.name,
            message: "Vui lòng nhập tên nhóm",
        },
        {
            value: payload.value.isActive !== null,
            message: "Vui lòng nhập trạng thái nhóm",
        },
    ];

    fields.forEach((field) => {
        if (!field.value) {
            FunctionGlobal.$notify("E", field.message, toast);
            status = false;
        } else if (field.validate && !field.validate(field.value)) {
            FunctionGlobal.$notify("E", field.invalidMsg, toast);
            status = false;
        }
    });
    const checkDuplicateName = Groups.value.some(
        (group) => group.name === payload.value.name && group.id !== payload.value.id
    );
    if (checkDuplicateName) {
        FunctionGlobal.$notify(
            "E",
            "Tên nhóm người dùng đã tồn tại, vui lòng chọn tên khác",
            toast
        );
        status = false;
    }
    return status;
};

const removeNullValues = (obj) => {
    const result = {};
    Object.keys(obj).forEach((key) => {
        if (obj[key] !== null) {
            result[key] = obj[key];
        }
    });
    return result;
};

const ActionCopy = (event, data) => {
    opcopy.value.toggle(event);
    idCopy.value = data.id;
};
const CopyGroup = async () => {
    try {
        loading.value = true;
        const res = await API.get(`UserGroup/${idCopy.value}`);
        if (res.data)
            payload.value = merge({}, payload.value, removeNullValues(res.data));
        payload.value.id = 0;
        payload.value.copy = true;
        payload.value.name = "Nhân bản từ " + payload.value.name;
        payload.value.group_code = "copy_" + payload.value.group_code;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
        dialogItem.value = true;
    }
};
</script>
<style scoped>
.Numberclass {
    background: #f9fafb;
    color: #374151;
    border: 1px solid #e5e7eb;
    border-width: 0 0 1px 0;
    padding: 1rem 1rem;
    font-weight: 700;
}

.hoevr_ :hover {
    border-left-style: 1px solid #000000;
}
</style>
