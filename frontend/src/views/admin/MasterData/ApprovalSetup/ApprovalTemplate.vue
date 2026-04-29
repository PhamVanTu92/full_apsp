<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import API from '@/api/api-main';
import { useGlobal } from '@/services/useGlobal';
import { useI18n } from 'vue-i18n';
import ApprovalDialog from './ApprovalDialog.vue';
import type { Pagable, DocItem, FormData, ApprovalTemplate, SubmitPayload, ApprovalSampleDetail } from './types';

const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();

const isLoading = ref<boolean>(false);
const pagable = ref<Pagable>({
    rows: 10,
    page: 0,
    totalRecords: 0
});
const confirmRemoveModal = ref<boolean>(false);
const selectedAppLv = ref<ApprovalTemplate[]>();
const visible = ref<boolean>(false);
const ApprovalTemplates = ref<ApprovalTemplate[]>([]);
const submited = ref<boolean>(false);
const formData = reactive<FormData>({
    id: 0,
    approvalSampleName: '',
    description: '',
    isActive: true,
    approvalSampleProcessesLines: [],
    approvalSampleDocumentsLines: [],
    approvalSampleMembersLines: []
});
const Docs = ref<DocItem[]>([]);

onMounted(() => {
    initData();
    fetchAllApprovalTemp();
});

const initData = (): void => {
    Object.assign(formData, {
        id: 0,
        approvalSampleName: '',
        description: '',
        isActive: true,
        approvalSampleProcessesLines: [],
        approvalSampleDocumentsLines: []
    });
    Docs.value = [
        {
            name: t('body.OrderApproval.order'),
            transType: 1,
            status: 'A',
            selected: null
        },
        {
            name: t('body.OrderApproval.pickupRequest'),
            transType: 2,
            status: 'A',
            selected: null
        },
        {
            name: t('body.OrderApproval.sampleRequest'),
            transType: 3,
            status: 'A',
            selected: null
        },
        {
            name: t('body.OrderApproval.return_request'),
            transType: 4,
            status: 'A',
            selected: null
        }
    ];
};
const fetchAllApprovalTemp = async (): Promise<void> => {
    isLoading.value = true;
    try {
        const res = await API.get(`ApprovalSample?page=${pagable.value.page}&pagesize=${pagable.value.rows}`);
        if (res.data) {
            ApprovalTemplates.value = res.data.result;
            pagable.value.totalRecords = res.data.total;
        }
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const validateData = (): boolean => {
    const errors: string[] = [];
    if (!formData.approvalSampleName) {
        errors.push(t('body.systemSetting.approval_template_name'));
    }

    const selectedDocs = formData.approvalSampleDocumentsLines?.length || 0;
    if (selectedDocs < 1) {
        errors.push(t('body.systemSetting.documents'));
    }

    const selectedProcess = formData.approvalSampleProcessesLines?.filter((p) => p.selectedAppLv)?.length || 0;
    if (selectedProcess < 1) {
        errors.push(t('body.systemSetting.process'));
    }

    if (errors.length > 0) {
        FunctionGlobal.$notify('E', t('body.systemSetting.invalid_data_message') + errors.join(', '), toast);
        return false;
    }
    return true;
};
const submitForm = async (): Promise<void> => {
    submited.value = true;
    if (!validateData()) return;
    isLoading.value = true;
    const payload: SubmitPayload = {
        approvalSampleName: formData.approvalSampleName,
        description: formData.description,
        isActive: formData.isActive,
        isDebtLimit: formData.isDebtLimit,
        isOverdueDebt: formData.isOverdueDebt,
        isOther: formData.isOther,
        approvalSampleDocumentsLines: formData.approvalSampleDocumentsLines || [],
        approvalSampleProcessesLines: (formData.approvalSampleProcessesLines || []).filter((p) => p.selectedAppLv).map((p) => p.wtsId),
        approvalSampleMembersLines: formData.approvalSampleMembersLines || []
    };

    const API_ENPOINT: string = formData.id ? `ApprovalSample/${formData.id}` : `ApprovalSample`;

    const FUNC_API = formData.id ? API.update(API_ENPOINT, payload) : API.add(API_ENPOINT, payload);

    try {
        const res = await FUNC_API;
        if (res.data) {
            FunctionGlobal.$notify('S', t('body.systemSetting.success_label'), toast);
            visible.value = false;
        }
    } catch (error) {
        FunctionGlobal.$notify('E', 'Lỗi: ' + (error as any).response?.data?.message || (error as any).message, toast);
        console.error(error);
    } finally {
        fetchAllApprovalTemp();
        isLoading.value = false;
    }
};
const openDetail = async (data: ApprovalTemplate): Promise<void> => {
    initData();
    isLoading.value = true;
    try {
        const res = await API.get(`ApprovalSample/${data.id}`);
        if (res.data) {
            const resData: ApprovalSampleDetail = res.data.result || res.data;
            Object.assign(formData, {
                id: resData.id,
                approvalSampleName: resData.approvalSampleName,
                description: resData.description,
                isActive: resData.isActive,
                isDebtLimit: resData.isDebtLimit,
                isOverdueDebt: resData.isOverdueDebt,
                isOther: resData.isOther
            });
            if (resData.approvalSampleMembersLines && resData.approvalSampleMembersLines.length > 0) {
                formData.approvalSampleMembersLines = resData.approvalSampleMembersLines.map((m) => m.creatorId);
                formData.rUsers = resData.approvalSampleMembersLines.map((m) => ({
                    id: m.creatorId,
                    fullName: m.creator?.fullName || '',
                    email: m.creator?.email || '',
                    role: m.creator?.role || {}
                }));
            } else {
                formData.approvalSampleMembersLines = [];
                formData.rUsers = [];
            }
            if (resData.approvalSampleDocumentsLines && resData.approvalSampleDocumentsLines.length > 0) {
                const documentIds = resData.approvalSampleDocumentsLines.map((d) => d.document);
                formData.approvalSampleDocumentsLines = documentIds;
                Docs.value.forEach((el) => {
                    el.selected = documentIds.includes(el.transType) ? el.transType : null;
                });
            } else {
                formData.approvalSampleDocumentsLines = [];
                Docs.value.forEach((el) => {
                    el.selected = null;
                });
            }
            if (resData.approvalSampleProcessesLines && resData.approvalSampleProcessesLines.length > 0) {
                formData.approvalSampleProcessesLines = resData.approvalSampleProcessesLines.map((p, index) => ({
                    id: p.id,
                    wtsId: p.approvalLevelId,
                    fatherId: p.fatherId,
                    sort: index + 1,
                    selectedAppLv: true,
                    status: '',
                    name: p.approvalLevel?.approvalLevelName || '',
                    description: p.approvalLevel?.description || '',
                    isCheck: true
                }));
            } else {
                formData.approvalSampleProcessesLines = [];
            }

            visible.value = true;
        }
    } catch (error) {
        FunctionGlobal.$notify('E', 'Lỗi: ' + (error as any).response?.data?.message || (error as any).message, toast);
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const confirmRemove = async (): Promise<void> => {
    isLoading.value = true;
    try {
        const res = await API.delete(`ApprovalTemplate/${formData.id}`);
        if (res.data) {
            FunctionGlobal.$notify('S', t('body.systemSetting.success_label') || 'Xóa thành công!', toast);
            confirmRemoveModal.value = false;
        }
    } catch (error) {
        console.error(error);
    } finally {
        fetchAllApprovalTemp();
        initData();
        isLoading.value = false;
    }
};

const addApprovalTemplateDialog = (): void => {
    initData();
    submited.value = false;
    visible.value = true;
};

const handleDialogSubmit = (): void => {
    submitForm();
};

const handleDialogHide = (): void => {
    formData.rUsers = [];
    formData.approvalSampleMembersLines = [];
};

const onPageChange = (event: any): void => {
    pagable.value.page = event.page;
    fetchAllApprovalTemp();
};

const clearFilter = (): void => {
    // Add filter clearing logic here
};
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <div class="text-2xl font-semibold">{{ t('body.systemSetting.approval_template_title') }}</div>
        <Button :label="t('body.systemSetting.add_new_button')" icon="fa-solid fa-plus" @click="addApprovalTemplateDialog()" />
    </div>
    <div class="grid mt-3">
        <div class="col-12 card">
            <DataTable paginator :rows="pagable.rows" :page="pagable.page" @page="onPageChange($event)" :totalRecords="pagable.totalRecords" v-model:selection="selectedAppLv" :value="ApprovalTemplates" showGridlines tableStyle="min-width: 50rem;">
                <template #empty>
                    <div class="text-center p-2">
                        {{ t('body.systemSetting.no_data_to_display') }}
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between m-0">
                        <IconField iconPosition="left">
                            <InputText :placeholder="t('body.OrderList.searchPlaceholder')" />
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                        </IconField>
                        <Button type="button" icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')" severity="danger" outlined @click="clearFilter()" />
                    </div>
                </template>
                <Column header="#" class="w-3rem">
                    <template #body="slotProps">
                        <span>{{ slotProps.index + 1 }}</span>
                    </template>
                </Column>
                <Column field="approvalSampleName" :header="t('body.systemSetting.approval_template_name_column')"> </Column>
                <Column field="description" :header="t('body.systemSetting.description')"></Column>
                <Column field="isActive" :header="t('body.systemSetting.status')" class="w-13rem">
                    <template #body="slotProps">
                        <Tag :severity="slotProps.data.isActive ? 'success' : 'danger'">
                            {{ slotProps.data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status') }}
                        </Tag>
                    </template>
                </Column>
                <Column class="w-1rem">
                    <template #body="slotProps">
                        <div class="flex justify-content-center">
                            <Button @click="openDetail(slotProps.data)" text icon="pi pi-pencil" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <ApprovalDialog :visible="visible" :formData="formData" :Docs="Docs" :submited="submited" @update:visible="visible = $event" @submit="handleDialogSubmit" @hide="handleDialogHide" />

    <Dialog v-model:visible="confirmRemoveModal" modal :header="t('body.OrderApproval.confirm')" :style="{ width: '500px' }">
        <div class="p-2 text-center">
            {{ t('body.OrderApproval.confirm') }} <strong>{{ formData.approvalSampleName }}</strong> ?
        </div>
        <template #footer>
            <div class="flex justify-content-end gap-2 mt-2">
                <Button :label="t('body.OrderList.close')" severity="secondary" @click="confirmRemoveModal = false" />
                <Button :label="t('body.OrderList.delete')" severity="danger" @click="confirmRemove()" />
            </div>
        </template>
    </Dialog>

    <loading v-if="isLoading"></loading>
</template>
