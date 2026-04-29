<script setup>
import { onMounted, reactive, ref } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useInfiniteScroll } from "@vueuse/core";
import { useI18n } from "vue-i18n";

const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();

const el = ref(null);
var flag = true;
const submited = ref(false);
const addApprovalerModal = ref(false);
const selectedAppLv = ref();
const ApporovalModal = ref(false);
const isLoading = ref(false);
const Approvals = ref([]);
const Approvalers = ref([]);
const removeApprovalModal = ref(false);
const approvalerPage = ref({
    page: 0,
    rows: 1000,
    total: 0,
});
const Pagable = ref({
    page: 0,
    size: 1000,
    total: 0,
    search: ""
});
const formData = reactive({
    id: 0,
    approvalLevelName: "",
    description: "",
    approvalNumber: 0,
    declineNumber: 0,
    isActive: true,
    approvalLevelLines: [],
});
const clearDataForm = JSON.stringify(formData);
onMounted(() => {
    fetchAllApprovals();
});
useInfiniteScroll(
    el,
    () => {
        if (!isLoading.value) {
            fetchAllApprovalers();
        }
    },
    { distance: 10 }
);
const removeApprovaler = (sp) => {
    sp.data.isCheck = false;
    sp.data.selectedAppLv = false;
    let removeItem = formData.approvalLevelLines.findIndex((el) => el == sp.data);
    if (sp.data.id !== 0) {
        return (sp.data.status = "D");
    }
    return formData.approvalLevelLines.splice(removeItem, 1);
};
const funcClearDataForm = () => {
    Object.assign(formData, JSON.parse(clearDataForm));
    submited.value = false;
    flag = true;
};
const openApprovalDialog = () => {
    funcClearDataForm();
    fetchAllApprovalers();
    ApporovalModal.value = true;
};
const fetchAllApprovalers = async () => {
    if (flag) {
        flag = false;
        isLoading.value = true;
        try {
            isLoading.value = false;
            const res = await API.get(
                `Account/getall?skip=${approvalerPage.value.page}&limit=${approvalerPage.value.rows}&userType=APSP`
            );
            if (res) {
                let data = ConvertData(res.data.item);
                Approvalers.value = data;
                approvalerPage.value.total = res.data.total;
                if (Approvalers.value.length < res.data.total) {
                    flag = true;
                    approvalerPage.value.page += 1;
                } else {
                    flag = false;
                }
                isLoading.value = false;
            }
        } catch (error) { }
    }
};

const ConvertData = (data) => {
    return data.map((el) => {
        return {
            id: el.userId ? el.id : 0,
            fatherId: el.userId ? el.fatherId : 0,
            email: el.email,
            fullName: el.fullName,
            userId: el.userId ? el.userId : el.id,
            status: "",
            isCheck: "",
        };
    });
};

const validateData = () => {
    let status = true;

    if (!formData.approvalLevelName) {
        FunctionGlobal.$notify("E", t("body.systemSetting.approvalLevel.validation.enterApprovalLevelName"), toast);
        return (status = false);
    }
    if (!formData.approvalNumber) {
        FunctionGlobal.$notify("E", t("body.systemSetting.approvalLevel.validation.enterApprovalQuantity"), toast);
        return (status = false);
    }

    if (formData.approvalNumber > formData.approvalLevelLines.filter((el) => el.status != "D").length) {
        FunctionGlobal.$notify(
            "E",
            t("body.systemSetting.approvalLevel.validation.approvalQuantityMustBeLessThanOrEqual"),
            toast
        );
        return (status = false);
    }
    if (!formData.declineNumber) {
        FunctionGlobal.$notify("E", t("body.systemSetting.approvalLevel.validation.enterRejectionQuantity"), toast);
        status = false;
    }
    if (formData.declineNumber > formData.approvalLevelLines.filter((el) => el.status != "D").length) {
        FunctionGlobal.$notify(
            "E",
            t("body.systemSetting.approvalLevel.validation.rejectionQuantityMustBeValid"),
            toast
        );
        status = false;
    }

    return status;
};
const submitForm = async () => {
    submited.value = true;
    if (!validateData()) return;
    formData.approvalLevelLines = formData.approvalLevelLines.map(us => us.userId)
    let ENDPOINT = formData.id ? `ApprovalLevel/${formData.id}` : `ApprovalLevel/`;
    let FUNC_API = formData.id
        ? API.update(ENDPOINT, formData)
        : API.add(ENDPOINT, formData);
    try {
        isLoading.value = true;
        const res = await FUNC_API;
        if (res.data) {
            ApporovalModal.value = false;
            FunctionGlobal.$notify(
                "S",
                !formData.id ? t("body.systemSetting.approvalLevel.messages.addSuccess") : t("body.systemSetting.approvalLevel.messages.updateSuccess"),
                toast
            );
            funcClearDataForm();
            // window.location.reload();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
        isLoading.value = false;
        submited.value = false;
        fetchAllApprovals();
    }
};
const fetchAllApprovals = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(
            `ApprovalLevel?PageSize=${Pagable.value.size}&Page=${Pagable.value.page}&search=${Pagable.value.search}`
        );
        Approvals.value = res.data.result;
        Pagable.value.total = res.data.total;
    } catch (error) {
    } finally {
        isLoading.value = false;
    }
};
const onPageChange = (e) => {
    Pagable.value.size = e.rows;
    Pagable.value.page = e.page;
    fetchAllApprovals();
};
const openDetailApproval = async (data) => {
    funcClearDataForm();
    await fetchAllApprovalers();
    isLoading.value = true;
    try {
        const res = await API.get(`ApprovalLevel/${data.id}`);
        if (res.data) {
            Object.assign(formData, res.data);
            const approvalerMap = new Map();
            Approvalers.value.forEach((e) => {
                approvalerMap.set(e.userId, e);
            });
            Approvalers.value.forEach((e) => {
                e.selectedAppLv = false;
                e.isCheck = false;
            });
            formData.approvalLevelLines = res.data.approvalLevelLines
                .map((el) => {
                    const approvaler = approvalerMap.get(el.approvalUserId);
                    if (approvaler) {
                        approvaler.selectedAppLv = true;
                        approvaler.isCheck = true;
                        approvaler.id = el.id;
                        approvaler.fatherId = el.fatherId;
                        return approvaler;
                    }
                    return null;
                })
                .filter(Boolean);
            Approvalers.value.sort((a, b) => {
                if (a.isCheck === true && b.isCheck === false) return -1;
                if (a.isCheck === false && b.isCheck === true) return 1;
                return 0;
            });
            ApporovalModal.value = true;
        }
    } catch (error) {
    } finally {
        isLoading.value = false;
    }
};
const removeApproval = (data) => {
    Object.assign(formData, data);
    removeApprovalModal.value = true;
};
const confirmRemove = async () => {
    isLoading.value = true;
    try {
        const res = await API.delete(`ApprovalStage/${formData.id}`);
        if (res) {
            fetchAllApprovals();
            FunctionGlobal.$notify("S", t("body.systemSetting.approvalLevel.messages.deleteSuccess"), toast);
            removeApprovalModal.value = false;
            ApporovalModal.value = false;
        }
    } catch (error) {
    } finally {
        isLoading.value = false;
    }
};

const openAddApprovalerDialog = () => {
    addApprovalerModal.value = true;
};
const confirmAddApplr = () => {
    formData.approvalLevelLines = Approvalers.value.filter((el) => {
        el.selectedAppLv = el.isCheck;
        return el.isCheck || el.status == "D";
    });
    fetchAllApprovalers();
    addApprovalerModal.value = false;
};

const unset = (item) => {
    if (item.id != 0 && !item.isCheck) {
        return (item.status = "D");
    }
    if (item.id != 0 && item.isCheck) {
        return (item.status = "");
    }
    if (item.id != 0 && item.isCheck && item.status == "") {
        return (item.id = 0);
    }
    if (item.id == 0 && item.isCheck) {
        item.status = "";
    }
};

const closeAddApplr = () => {
    Approvalers.value.filter((el) => {
        el.isCheck = el.selectedAppLv;
    });
    addApprovalerModal.value = false;
};

</script>
<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <h4 class="font-bold m-0">{{ t('body.systemSetting.approval_level_title') }}</h4>
        <Button :label="t('body.systemSetting.add_new_approval_button')" icon="pi pi-plus"
            @click="openApprovalDialog()" />
    </div>
    <div class="grid mt-3 card p-2">
        <div class="col-12">
            <DataTable paginator lazy :rows="Pagable.size" :page="Pagable.page" :totalRecords="Pagable.total"
                @page="onPageChange($event)" showGridlines v-model:selection="selectedAppLv" :value="Approvals"
                :rowsPerPageOptions="[10, 20, 30]" tableStyle="min-width: 50rem;">
                <template #empty>
                    <div class="text-center p-2">
                        Không tìm thấy cấp phê duyệt tương ứng
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between m-0">
                        <IconField iconPosition="left">
                            <InputText :placeholder="t('body.OrderList.searchPlaceholder')"
                                @change="fetchAllApprovals()" v-model="Pagable.search" />
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                        </IconField>
                        <Button type="button" icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')"
                            severity="danger" outlined @click="clearFilter()" />
                    </div>
                </template>
                <Column selectionMode="multiple"></Column>
                <Column field="approvalLevelName" :header="t('body.systemSetting.approval_level_name_column')"></Column>
                <Column field="description" :header="t('body.systemSetting.description')"></Column>
                <Column field="approvalNumber" :header="t('body.systemSetting.approved_quantity_column')"></Column>
                <Column field="declineNumber" :header="t('body.systemSetting.rejected_quantity_column')"></Column>
                <Column field="actions" :header="t('body.systemSetting.action_column')" style="width: 10rem">
                    <template #body="slotProps">
                        <div class="flex justify-content-center">
                            <Button @click="openDetailApproval(slotProps.data)" text severity="primary"
                                icon="pi pi-pencil" />
                            <Button @click="removeApproval(slotProps.data)" text severity="danger" icon="pi pi-trash" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <Dialog v-model:visible="ApporovalModal" modal
        :header="formData.id ? t('body.promotion.update_button') : t('body.systemSetting.add_new_approval')"
        :style="{ width: '50rem' }">
        <div class="flex flex-column gap-2">
            <div class="flex flex-column gap-2">
                <label for="approvalName" class="font-semibold">{{ t('body.systemSetting.approval_level_name_column') }}
                    <sup class="text-red-500">*</sup></label>
                <InputText :invalid="submited && !formData.approvalLevelName" id="approvalName"
                    v-model="formData.approvalLevelName" autocomplete="off" required />
            </div>
            <div class="flex flex-column gap-2">
                <label for="desc" class="font-semibold">{{ t('body.systemSetting.description') }}</label>
                <Textarea id="desc" rows="3" v-model="formData.description" autocomplete="off"></Textarea>
            </div>
            <div class="grid">
                <div class="col-6">
                    <div class="flex flex-column gap-2">
                        <label class="font-semibold">{{ t('body.systemSetting.approved_quantity_column') }} <sup
                                class="text-red-500">*</sup></label>
                        <InputNumber :invalid="submited && !formData.approvalNumber" v-model="formData.approvalNumber"
                            required />
                    </div>
                </div>
                <div class="col-6">
                    <div class="flex flex-column gap-2">
                        <label class="font-semibold">{{ t('body.systemSetting.rejected_quantity_column') }} <sup
                                class="text-red-500">*</sup></label>
                        <InputNumber :invalid="submited && !formData.declineNumber" v-model="formData.declineNumber"
                            required />
                    </div>
                </div>
            </div>
            <DataTable showGridlines :value="formData.approvalLevelLines">
                <Column class="w-3rem" header="STT">
                    <template #body="slotProps">
                        <div>
                            {{ slotProps.index + 1 }}
                        </div>
                    </template>
                </Column>
                <Column :header="t('body.systemSetting.user_management_title')">
                    <template #body="slotProps">
                        <span>{{ slotProps.data.fullName }}</span>
                    </template>
                </Column>
                <Column style="width: 5rem">
                    <template #body="{ index }">
                        <Button @click="formData.approvalLevelLines.splice(index, 1)" text severity="danger"
                            icon="pi pi-trash" />
                    </template>
                </Column>
            </DataTable>
            <div class="flex align-items-center mt-2">
                <Button icon="pi pi-plus-circle" @click="openAddApprovalerDialog()"
                    :label="t('body.OrderApproval.add_approver')" text />
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button class="px-4" type="button" :label="t('body.OrderList.close')" severity="secondary"
                    @click="ApporovalModal = false" />
                <Button class="px-4" type="submit" :label="t('body.sampleRequest.importPlan.save_button')"
                    @click="submitForm()" />
            </div>
        </template>
    </Dialog>
    <Dialog v-model:visible="removeApprovalModal" modal :header="t('body.OrderApproval.confirm')"
        :style="{ width: '30rem' }">
        <div class="p-2 text-center">Xác nhận xóa ?</div>
        <template #footer>
            <div class="flex gap-2">
                <Button @click="removeApprovalModal = false" :label="t('body.OrderList.close')" icon="pi pi-times"
                    severity="secondary" />
                <Button @click="confirmRemove()" :label="t('body.OrderList.delete')" icon="pi pi-trash"
                    severity="danger" />
            </div>
        </template>
    </Dialog>

    <Dialog v-model:visible="addApprovalerModal" modal :header="t('client.title')" :style="{ width: '700px' }">
        <div class="flex flex-column gap-4">
            <InputText class="w-full" :placeholder="t('body.home.search_placeholder')" />
            <div ref="el" class="card p-2 overflow-y-auto bg-gray-100" style="height: 500px">
                <div v-for="(item, index) in Approvalers" :key="index" class="card flex align-items-center gap-4 m-1">
                    <Checkbox v-model="item.isCheck" binary @change="unset(item)"></Checkbox>
                    <div class="flex flex-column gap-2">
                        <div class="flex gap-2">
                            <strong>{{ t('client.user_name') }}:</strong>
                            <span>{{ item.fullName }}</span>
                        </div>
                        <div class="flex gap-2">
                            <strong>{{ t('client.email_label') }}:</strong>
                            <span>{{ item.email }}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button @click="closeAddApplr()" :label="t('body.OrderList.close')" icon="pi pi-times"
                    severity="secondary" />
                <Button @click="confirmAddApplr()" :label="t('body.OrderApproval.confirm')" icon="pi pi-check" />
            </div>
        </template>
    </Dialog>
    <loading v-if="isLoading"></loading>
</template>

<style scoped></style>