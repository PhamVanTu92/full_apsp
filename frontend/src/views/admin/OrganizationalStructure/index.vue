<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.systemSetting.organization_structure_title') }}</h4>
            <Button @click="onClickOpenOrgStrct()" icon="pi pi-plus" :label="t('body.systemSetting.add_new_button')" />
        </div>
        <div class="card p-3">
            <Splitter
                style="height: 70vh"
                class="border-1 border-200"
                :pt:gutterHandler:style="'width: 6px'"
            >
                <SplitterPanel class="overflow-y-scroll" :size="25" :minSize="10">
                    <div class="w-full">
                        <!-- {{ expandedKeys }} -->
                        <Tree
                            v-model:expandedKeys="expandedKeys"
                            v-model:selectionKeys="selection.cctcKey"
                            :value="data.orgStruct"
                            @node-unselect="handleNodeUnselect"
                            @node-select="handleNodeSelect"
                            selectionMode="single"
                            style="min-width: 20rem"
                            :filter="true"
                            filterMode="lenient"
                            :filterPlaceholder="t('body.systemSetting.tree_search_placeholder')"
                        >
                            <template #default="sp">
                                <span class="py-2">{{ sp.node.name }}</span>
                            </template>
                        </Tree>
                    </div>
                </SplitterPanel>
                <SplitterPanel class="p-3 overflow-y-auto" :size="75" :minSize="50">
                    <div>
                        <div class="border-1 border-200 p-3 mb-3">
                            <div
                                class="flex align-items-center mb-3 justify-content-between"
                            >
                                <div class="font-bold text-xl">
                                    {{ selection.cctcDetail?.name }}
                                </div>
                                <div class="flex gap-2">
                                    <Button
                                        @click="
                                            onClickOpenOrgStrct(selection.cctcDetail?.id)
                                        "
                                        icon="pi pi-pencil"
                                        :disabled="!selection.cctcDetail?.id"
                                    />
                                    <Button
                                        @click="
                                            onClickDeleteOrgStruct(selection.cctcDetail)
                                        "
                                        icon="pi pi-trash"
                                        severity="danger"
                                        :disabled="
                                            selection?.cctcDetail?.children?.length > 0
                                        "
                                    >
                                    </Button>
                                </div>
                            </div>
                            <div class="flex">
                                <div class="col-3 p-0">
                                    <span class="font-semibold mr-3">{{ t('body.systemSetting.unit_code_label') }}:</span>
                                    <span class="">{{ selection.cctcDetail?.code }}</span>
                                </div>
                                <div class="col p-0">
                                    <span class="font-semibold mr-3"
                                        >{{ t('body.systemSetting.parent_unit_label') }}:</span
                                    >
                                    <span class="">{{
                                        selection.cctcDetail?.parent?.name
                                    }}</span>
                                </div>
                                <div class="col p-0" v-if="0">
                                    <span class="font-semibold mr-3">{{ t('body.systemSetting.status_label') }}:</span>
                                    <span class="">{{
                                        selection.cctcDetail?.isActive
                                            ? t('body.sampleRequest.customer.active_status')
                                            : t('body.sampleRequest.customer.inactive_status')
                                    }}</span>
                                </div>
                            </div>
                        </div>
                        <DataTable
                            :value="selection.cctcDetail?.children || []"
                            showGridlines
                            class="mb-3"
                            stripedRows=""
                            scrollable=""
                            scrollHeight="315px"
                        >
                            <Column header="#" class="w-3rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column field="code" :header="t('body.systemSetting.department_code_column')"></Column>
                            <Column field="name" :header="t('body.systemSetting.department_name_column')"></Column>
                            <!-- <Column header="Đơn vị cấp trên"></Column> -->
                            <Column :header="t('body.systemSetting.staff_column')" class="w-8rem text-right">
                                <template #body="{ data }">
                                    {{ data.employeesCount || 0 }}
                                </template>
                            </Column>
                            <Column v-if="0" :header="t('body.systemSetting.status')" class="w-12rem">
                                <template #body="{ data }">
                                    {{
                                        data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status')
                                    }}
                                </template>
                            </Column>
                            <Column class="w-1rem" v-if="false">
                                <template #body="{ data }">
                                    <Button
                                        @click="onClickDeleteOrgStruct(data)"
                                        text
                                        icon="pi pi-trash"
                                        severity="danger"
                                    />
                                </template>
                            </Column>
                            <template #header>
                                <div
                                    class="flex justify-content-between align-items-center"
                                >
                                    <div
                                        style="height: 33px"
                                        class="flex align-items-center text-lg"
                                    >
                                        {{ t('body.systemSetting.list_of_subordinate_units') }}
                                    </div>
                                    <div class="flex gap-2">
                                        <template v-if="!editMode.orgStrct">
                                            <Button
                                                v-if="0"
                                                @click="editMode.orgStrct = true"
                                                icon="pi pi-pencil"
                                                :disabled="!selection.cctcDetail?.id"
                                            />
                                        </template>
                                        <template v-else>
                                            <Button
                                                @click="editMode.orgStrct = false"
                                                icon="pi pi-times"
                                                :label="t('body.OrderList.close')"
                                                severity="secondary"
                                            />
                                            <Button
                                                icon="pi pi-save"
                                                :label="t('body.sampleRequest.importPlan.save_button')"
                                            />
                                        </template>
                                    </div>
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center my-5 py-5 text-500 font-italic">
                                    {{ t('body.systemSetting.no_data_to_display') }}
                                </div>
                            </template>
                        </DataTable>
                        <DataTable
                            :value="
                                editMode.employee
                                    ? dataEdit.employee.filter((epl) => !epl.isDelete)
                                    : selection.cctcDetail?.employees
                            "
                            v-if="0"
                            showGridlines
                            stripedRows=""
                            scrollable=""
                            scrollHeight="315px"
                        >
                            <Column header="#" class="w-3rem">
                                <template #body="sp">
                                    {{ sp.index + 1 }}
                                </template>
                            </Column>
                            <Column field="userName" :header="t('body.systemSetting.account_name') "></Column>
                            <Column field="fullName" :header="t('body.systemSetting.employee_name')"></Column>
                            <Column
                                :header="t('body.systemSetting.direct_manager_column')"
                                class="w-13rem text-center"
                            >
                                <template #body="{ data }">
                                    <!-- {{ data.isLeader }} -->
                                    <RadioButton
                                        v-model="data.isLeader"
                                        :value="true"
                                        :readonly="!editMode.employee"
                                        @change="onChangeLeader(data.id, data.isLeader)"
                                    ></RadioButton>
                                </template>
                            </Column>
                            <!-- <Column header="Nhân viên" class="w-10rem text-center">
                <SplitterPanel class="overflow-y-scroll" :size="25" :minSize="10">
                    <div class="w-full">
                        <Tree
                            v-model:selectionKeys="selection.cctcKey"
                            :value="data.orgStruct"
                            @node-unselect="handleNodeUnselect"
                            @node-select="handleNodeSelect"
                            selectionMode="single"
                            style="min-width: 20rem"
                            :filter="true"
                            filterMode="lenient"
                            filterPlaceholder="Tìm kiếm..."
                        >
                            <template #default="sp">
                                <span class="py-2">{{ sp.node.name }}</span>
                            </template>
                        </Tree>
                    </div>
                </SplitterPanel>
                <SplitterPanel class="p-3 overflow-y-auto" :size="75" :minSize="50">
                    <div>
                        <div class="border-1 border-200 p-3 mb-3">
                            <div
                                class="flex align-items-center mb-3 justify-content-between"
                            >
                                <div class="font-bold text-xl">
                                    {{ selection.cctcDetail?.name }}
                                </div>
                                <div class="flex gap-2">
                                    <Button
                                        @click="
                                            onClickOpenOrgStrct(selection.cctcDetail?.id)
                                        "
                                        icon="pi pi-pencil"
                                        :disabled="!selection.cctcDetail?.id"
                                    />
                                    <Button
                                        @click="
                                            onClickDeleteOrgStruct(selection.cctcDetail)
                                        "
                                        icon="pi pi-trash"
                                        severity="danger"
                                        :disabled="!selection.cctcDetail?.id"
                                    >
                                    </Button>
                                </div>
                            </div>
                            <div class="flex">
                                <div class="col-3 p-0">
                                    <span class="font-semibold mr-3">Mã đơn vị:</span>
                                    <span class="">{{ selection.cctcDetail?.code }}</span>
                                </div>
                                <div class="col p-0">
                                    <span class="font-semibold mr-3"
                                        >Đơn vị cấp trên:</span
                                    >
                                    <span class="">{{
                                        selection.cctcDetail?.parent?.name
                                    }}</span>
                                </div>
                                <div class="col p-0" v-if="0">
                                    <span class="font-semibold mr-3">Trạng thái:</span>
                                    <span class="">{{
                                        selection.cctcDetail?.isActive
                                            ? "Đang hoạt động"
                                            : "Ngừng hoạt động"
                                    }}</span>
                                </div>
                            </div>
                        </div>
                        <DataTable
                            :value="selection.cctcDetail?.children || []"
                            showGridlines
                            class="mb-3"
                            stripedRows=""
                            scrollable=""
                            scrollHeight="315px"
                        >
                            <Column header="#" class="w-3rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column field="code" header="Mã đơn vị"></Column>
                            <Column field="name" header="Tên đơn vị"></Column>
                            <!-- <Column header="Đơn vị cấp trên"></Column> -->
                            <Column header="Thành viên" class="w-8rem text-right">
                                <template #body="{ data }">
                                    {{ data.employeesCount || 0 }}
                                </template>
                            </Column>
                            <Column v-if="0" header="Trạng thái" class="w-12rem">
                                <template #body="{ data }">
                                    {{
                                        data.isActive
                                            ? "Đang hoạt động"
                                            : "Ngừng hoạt động"
                                    }}
                                </template>
                            </Column>
                            <Column class="w-1rem">
                                <template #body="{ data }">
                                    <Button
                                        @click="onClickDeleteOrgStruct(data)"
                                        text
                                        icon="pi pi-trash"
                                        severity="danger"
                                    />
                                </template>
                            </Column>
                            <template #header>
                                <div
                                    class="flex justify-content-between align-items-center"
                                >
                                    <div
                                        style="height: 33px"
                                        class="flex align-items-center text-lg"
                                    >
                                        Danh sách đơn vị trực thuộc
                                    </div>
                                    <div class="flex gap-2">
                                        <template v-if="!editMode.orgStrct">
                                            <Button
                                                v-if="0"
                                                @click="editMode.orgStrct = true"
                                                icon="pi pi-pencil"
                                                :disabled="!selection.cctcDetail?.id"
                                            />
                                        </template>
                                        <template v-else>
                                            <Button
                                                @click="editMode.orgStrct = false"
                                                icon="pi pi-times"
                                                label="Hủy"
                                                severity="secondary"
                                            />
                                            <Button
                                                icon="pi pi-save"
                                                label="Lưu"
                                            />
                                        </template>
                                    </div>
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center my-5 py-5 text-500 font-italic">
                                   {{t('body.sampleRequest.customer.no_data_message')}}
                                </div>
                            </template>
                        </DataTable>
                        <DataTable
                            :value="
                                editMode.employee
                                    ? dataEdit.employee.filter((epl) => !epl.isDelete)
                                    : selection.cctcDetail?.employees
                            "
                            showGridlines
                            stripedRows=""
                            scrollable=""
                            scrollHeight="315px"
                        >
                            <Column header="#" class="w-3rem">
                                <template #body="sp">
                                    {{ sp.index + 1 }}
                                </template>
                            </Column>
                            <Column field="userName" :header="t('body.systemSetting.account_name_column') "></Column>
                            <Column field="fullName" :header="t('body.systemSetting.staff_name_column') "></Column>
                            <Column
                                :header="t('body.systemSetting.direct_manager_column') "
                                class="w-11rem text-center"
                            >
                                <template #body="{ data }">
                                    <!-- {{ data.isLeader }} -->
                                    <RadioButton
                                        v-model="data.isLeader"
                                        :value="true"
                                        :readonly="!editMode.employee"
                                        @change="onChangeLeader(data.id, data.isLeader)"
                                    ></RadioButton>
                                </template>
                            </Column>
                            <!-- <Column header="Nhân viên" class="w-10rem text-center">
                                <template #body="{ data }">
                                    <RadioButton
                                        v-model="data.isLeader"
                                        :value="false"
                                        :readonly="!editMode.employee"
                                    ></RadioButton>
                                </template>
                            </Column> -->
                            <Column class="w-1rem" v-if="editMode.employee">
                                <template #body="{ data }">
                                    <!-- {{ data.isDelete }} -->
                                    <Button
                                        @click="data.isDelete = true"
                                        text
                                        icon="pi pi-trash"
                                        severity="danger"
                                    />
                                </template>
                            </Column>
                            <template #header>
                                <div
                                    class="flex justify-content-between align-items-center"
                                >
                                    <span class="text-lg">{{ t('body.systemSetting.list_of_employees') }}</span>
                                    <div class="flex gap-2">
                                        <template v-if="!editMode.employee">
                                            <Button
                                                icon="pi pi-pencil"
                                                @click="onClickEditEmployee"
                                                :disabled="!selection.cctcDetail?.id"
                                            />
                                            <Button
                                                icon="pi pi-plus"
                                                @click="onClickOpenEmployee"
                                                :disabled="!selection.cctcDetail?.id"
                                            />
                                        </template>
                                        <template v-else>
                                            <Button
                                                @click="editMode.employee = false"
                                                icon="pi pi-times"
                                                :label="t('body.OrderList.close')"
                                                severity="secondary"
                                            />
                                            <Button
                                                @click="onClickSaveEmployee()"
                                                icon="pi pi-save"
                                                :label="t('body.sampleRequest.importPlan.save_button')"
                                            />
                                        </template>
                                    </div>
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center my-5 py-5 text-500 font-italic">
                                    {{ t('body.systemSetting.no_data_to_display') }}
                                </div>
                            </template>
                        </DataTable>
                    </div>
                </SplitterPanel>
            </Splitter>
        </div>
        <OrgStrctDlg
            v-model:visible="visible.orgStrct"
            :options="data.orgStruct"
            :data="orgStructSelect"
            @save="onClickSaveOrgStrct"
            :loading="loading.button"
            :errors="errorMessage"
        />
        <EmployeeDlg
            ref="employeeElement"
            v-model:selection="selection.cctcDetail.employees"
            :employeeIds="selection.cctcDetail?.employees?.map((el) => el.id)"
            @save="onClickAddEmployee"
        />
        <Loading v-if="loading.data" />
        <ConfirmDialog></ConfirmDialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, nextTick, watch } from "vue";
import cloneDeep from "lodash/cloneDeep";
import {
    OrgStruct,
    Employee,
    getOrgStruct,
    getOrgStructDetail,
    addOrgStruct,
    updateOrgStruct,
    deleteOrgStruct,
    saveChoseLeader,
    deleteEmployee,
    addEmployee,
} from "./script";
import OrgStrctDlg from "./components/OrgStrctDlg.vue";
import EmployeeDlg from "./components/EmployeeDlg.vue";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { AxiosError } from "axios";
import { useRouter, useRoute } from "vue-router";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

// -------------- BEGIN FIXED --------------------------------
const employeeElement = ref<InstanceType<typeof EmployeeDlg>>();
const onClickOpenEmployee = (): void => {
    employeeElement.value?.open();
};
// -------------- END FIXED --------------------------------

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const route = useRoute();

const expandedKeys = ref();

const editMode = reactive({
    orgStrct: false,
    employee: false,
});

const dataEdit = reactive({
    orgStrct: [],
    employee: [] as Array<Employee>,
});

const handlerDelete = async (id: number | null): Promise<void> => {
    if (id) {
        try {
            const res = await deleteOrgStruct(id);
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: "Đã xóa phòng ban.",
                life: 3000,
            });
            fetchOrgStruct();
            if (selection.cctcDetail.id === id) {
                fetchOrgStructDetail(data.orgStruct[0].id || 1);
                selection.cctcKey = {
                    [`${data.orgStruct[0].id}`]: true,
                };
            }
        } catch (error) {
            console.error(error);
            toast.add({
                severity: "warn",
                summary: t('body.OrderList.close') || "Thất bại",
                detail:
                    ((error as AxiosError)?.response?.data as {
                        error: string | undefined;
                    }).error || t('body.report.error_occurred_message'),
                life: 3000,
            });
        }
    }
};

const onClickDeleteOrgStruct = async (orgStrct: OrgStruct): Promise<void> => {
    // deleteOrgStruct
    confirm.require({
        message: `Bạn có muốn xóa cơ cấu tổ chức: ${orgStrct.name}`,
        header: "Xác nhận xóa",
        rejectClass: "p-button-secondary p-button",
        rejectIcon: "pi pi-times",
        acceptIcon: "pi pi-check",
        acceptClass: "p-button-danger p-button",
        rejectLabel: t('body.OrderList.close') || "Đóng",
        acceptLabel: "Xóa",
        accept: () => {
            handlerDelete(orgStrct.id);
        },
        reject: () => {},
    });
};

const onClickAddEmployee = async (employeeIds: Array<number>): Promise<void> => {
    try {
        const res = await addEmployee(selection.cctcDetail.id || 0, employeeIds);
        visible.employee = false;
        fetchOrgStructDetail(selection.cctcDetail.id || 0);
        toast.add({
            severity: "success",
            summary: t('body.systemSetting.success_label'),
            detail: "Thêm mới thành công",
            life: 3000,
        });
    } catch (error: any) {
        toast.add({
            severity: "warn",
            summary: t('body.OrderList.close') || "Thất bại",
            detail:
                ((error as AxiosError)?.response?.data as {
                    error: string | undefined;
                }) || t('body.report.error_occurred_message'),
            life: 3000,
        });
    }
};

const onChangeLeader = (id: number, value: boolean) => {
    if (value) {
        dataEdit.employee.forEach((item) => {
            if (item.id == id) {
                item.isLeader = true;
            } else {
                item.isLeader = false;
            }
        });
    }
};

const onClickEditEmployee = (): void => {

    editMode.employee = true;
    dataEdit.employee = cloneDeep(selection.cctcDetail.employees);
};

const onClickSaveEmployee = async (): Promise<void> => {
    const leader = dataEdit.employee.find((epl) => epl.isLeader);
    let errorCount = 0;
    if (leader) {
        try {
            await saveChoseLeader(selection.cctcDetail.id, leader.id);
            fetchOrgStructDetail(selection.cctcDetail.id || 0);
        } catch (error) {
            console.error(error);
            errorCount++;
        }
    }
    const deleteIds = dataEdit.employee
        .filter((epl) => epl.isDelete)
        .map((epl) => epl.id);
    if (deleteIds.length > 0) {
        try {
            await deleteEmployee(selection.cctcDetail.id || 0, deleteIds);
            fetchOrgStructDetail(selection.cctcDetail.id || 0);
        } catch (error) {
            console.error(error);
            errorCount++;
        }
    }
    if (errorCount === 0) {
        toast.add({
            severity: "success",
            summary: t('body.systemSetting.success_label'),
            detail: "Lưu thành công",
            life: 3000,
        });
    } else {
        toast.add({
            severity: "warn",
            summary: t('body.OrderList.close') || "Thất bại",
            detail: t('body.report.error_occurred_message'),
            life: 3000,
        });
    }
    editMode.employee = false;
};

const errorMessage = ref();
const onClickSaveOrgStrct = async (payload: OrgStruct): Promise<void> => {
    if (payload)
        try {
            loading.button = true;
            if (payload.id) {
                const res = await updateOrgStruct(payload.id, payload);
                toast.add({
                    severity: "success",
                    summary: t('body.systemSetting.success_label'),
                    detail: "Cập nhật thành công",
                    life: 3000,
                });
                if (res.data) {
                    fetchOrgStruct();
                    fetchOrgStructDetail(payload.id);
                    visible.orgStrct = false;
                }
            } else {
                const res = await addOrgStruct(payload);
                toast.add({
                    severity: "success",
                    summary: t('body.systemSetting.success_label'),
                    detail: "Thêm mới thành công",
                    life: 3000,
                });
                if (res.data) {
                    fetchOrgStruct();
                    fetchOrgStructDetail(selection.cctcDetail?.id || 1);
                    visible.orgStrct = false;
                }
            }
        } catch (error) {
            console.error(error);
        }
    loading.button = false;
};
const orgStructSelect = ref(new OrgStruct());
const onClickOpenOrgStrct = async (id?: number | null) => {
    if (id) {
        orgStructSelect.value = (await getOrgStructDetail(id)) || new OrgStruct();
    } else {
        const newOrgStrct = new OrgStruct();
        newOrgStrct.parentId = selection.cctcDetail.id;
        orgStructSelect.value = newOrgStrct;
    }
    visible.orgStrct = true;
};

const data = reactive({
    orgStruct: [] as OrgStruct[],
});

const visible = reactive({
    orgStrct: false,
    employee: false,
});

const selection = reactive({
    cctcKey: {},
    cctcDetail: new OrgStruct(),
});

const loading = reactive({
    data: false,
    select: false,
    button: false,
});

const handleNodeUnselect = (e: any): void => {
    nextTick(() => {
        const node = {
            [e.key]: true,
        };
        selection.cctcKey = node;
    });
};

const handleNodeSelect = (e: any): void => {
    fetchOrgStructDetail(e.id);
    editMode.employee = false;
    editMode.orgStrct = false;
};

// watch(
//     () => route.query.id,
//     (id) => {
//         if (id) {
//             // selectNode(Number(id));
//         }
//     }
// );

onMounted(async function () {
    fetchOrgStruct(true);
});

const fetchOrgStruct = async (selectFirst?: boolean) => {
    try {
        loading.data = true;
        data.orgStruct = await getOrgStruct();
        if (selectFirst) {
            fetchOrgStructDetail(data.orgStruct[0].id || 1);
            const cctcKey = {};
            cctcKey[`${data.orgStruct[0].id}`] = true;
            selection.cctcKey = cctcKey;
            expandedKeys.value = cctcKey;
        }
    } catch (error) {
        console.error(error);
        toast.add({
            severity: "error",
            summary: t('body.OrderApproval.clear') || "Lỗi",
            detail: t('body.report.error_occurred_message'),
            life: 3000,
        });
    }
    loading.data = false;
};

const fetchOrgStructDetail = async (id: number) => {
    try {
        loading.select = true;
        selection.cctcDetail = (await getOrgStructDetail(id)) || new OrgStruct();
    } catch (error) {
        toast.add({
            severity: "error",
            summary: t('body.OrderApproval.clear') || "Lỗi",
            detail: error,
            life: 3000,
        });
    }
    loading.select = false;
};
</script>

<style scoped></style>
