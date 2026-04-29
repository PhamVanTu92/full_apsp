<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.systemSetting.role_title') }}</h4>
            <Button @click="onClickOpenRoleDlg(null)" icon="pi pi-plus" :label="t('body.systemSetting.add_new_button')" />
        </div>
        <DataTable
            class="card p-3"
            :value="data.roles"
            showGridlines
            rowHover
            stripedRows
            paginator
            :rows="10"
            :rowsPerPageOptions="[10, 20, 50]"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
           :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
        >
            <template #empty>
                <div class="my-5 py-5 text-center">{{t('body.sampleRequest.customerGroup.no_data_message')}}</div>
            </template>
            <Column field="name" :header="t('body.systemSetting.role_name_column')"></Column>
            <Column field="countUserInRole" :header="t('body.systemSetting.account_title_for_role')" class="w-10rem text-right"></Column>
            <Column field="isActive" :header="t('body.systemSetting.status')" class="w-10rem">
                <template #body="{ data }">
                    <Tag :value="data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status')" :severity="data.isActive ? '' : 'secondary'"></Tag>
                </template>
            </Column>
            <Column field="notes" :header="t('body.systemSetting.description')"></Column>
            <Column field="" header="" class="w-5rem">
                <template #body="{ data }">
                    <div class="flex">
                        <Button @click="onClickOpenRoleDlg(data)" icon="pi pi-pencil" severity="" text v-tooltip="t('body.OrderList.edit')"/>
                        <Button @click="onClickCopy(data)" icon="pi pi-copy" severity="info" text v-tooltip="t('body.promotion.copy_button')"/>
                        <Button @click="onClickConfirmDelete(data)" icon="pi pi-trash" severity="danger" text v-tooltip="t('body.OrderList.delete')" v-if="![34, 35, 39, 1029, 1046, 1053].includes(data.id || 0)"/>
                    </div>
                </template>
            </Column>
        </DataTable>
        <Dialog v-model:visible="visible" modal :header="isUpdate ? t('body.systemSetting.update_role') : t('body.systemSetting.add_new_role')" class="w-8">
            <div class="surface-50 p-4 border-round mb-3">
                <div class="grid">
                    <div class="col-6">
                        <div class="field">
                            <label for="" class="font-semibold">{{ t('body.systemSetting.role_name') }}</label>
                            <InputText v-model="payload.name" id="role-name" class="w-full" :disabled="[34, 35, 39, 1029, 1046, 1053].includes(payload.id || 0)"></InputText>
                        </div>
                        <div class="field mb-0">
                            <label for="" class="font-semibold">{{ t('body.systemSetting.description') }}</label>
                            <Textarea v-model="payload.notes" :rows="3" class="w-full"></Textarea>
                        </div>
                    </div>

                    <div class="col-6">
                        <div class="field flex gap-3">
                            <RadioButton v-model="payload.isSaleRole" input-id="nvkd" :value="true"></RadioButton>
                            <label for="nvkd">{{ t('body.systemSetting.is_business_employee') }}</label>
                        </div>
                        <div class="field flex gap-3">
                            <RadioButton v-model="payload.isSaleRole" input-id="nkh" :value="false" @change="payload.isFillCustomerGroup = !payload.isSaleRole"></RadioButton>
                            <div class="flex-grow-1">
                                <label for="nkh">{{ t('body.systemSetting.can_view_customer_groups') }}</label>
                                <div v-if="!payload.isSaleRole" class="mt-3">
                                    <MultiSelect v-model="payload.roleFillCustomerGroups" :placeholder="t('Custom.select_customer_group')" class="w-30rem" :options="nhomKH" option-label="groupName" option-value="id">
                                        <template #empty>
                                            <div class="text-center text-500 my-5">{{ t('body.promotion.no_data_message') }}</div>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                        </div>
                        <!-- <div class="field flex gap-3">
                            <RadioButton v-model="payload.isSaleRole" input-id="kpq" :value="false"></RadioButton>
                            <label for="kpq">Không phân quyền </label>
                        </div> -->
                        <div class="field flex gap-3 mt-5">
                            <label for="" class="w-10rem font-semibold">{{ t('body.systemSetting.status') }}</label>
                            <InputSwitch v-model="payload.isActive" :true-value="true" :false-value="false"></InputSwitch>
                        </div>
                    </div>
                </div>
            </div>
            <ClaimsComponent class="card p-3" :claims="data.claims" expandFirst v-model="payload.selectClaims"></ClaimsComponent>
            <!-- {{ payload }} -->
            <template #footer>
                <div class="flex justify-content-end gap-2">
                    <Button :label="t('body.OrderList.close')" severity="secondary" @click="visible = false"/>
                    <Button :loading="loading.role" :label="t('body.sampleRequest.importPlan.save_button')" @click="onClickSaveRole()"/>
                </div>
            </template>
        </Dialog>
        <ConfirmDialog></ConfirmDialog>
    </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onBeforeMount } from 'vue';
import { AxiosResponse } from 'axios';
import API from '@/api/api-main';
import { Role, RoleClaim } from './role.model';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';

import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const status = ref(true);

const nhomKH = ref([]);
const fetchNhomKH = () => {
    API.get('CustomerGroup?skip=0&limit=10000&filter=(isActive=true)').then((res) => {
        nhomKH.value = res.data['bpGroup'];
    });
};

const data = reactive({
    roles: [] as Role[],
    claims: []
});

const loading = reactive({
    role: false,
    roleData: false
});

const toast = useToast();
const confirm = useConfirm();
const visible = ref(false);
const payload = ref<Role>(new Role());

const fetchRoles = (): void => {
    API.get('role/getrole')
        .then((res: AxiosResponse) => {
            data.roles = res.data;
        })
        .catch((error) => {});
};

const fetchClaims = (): void => {
    API.get('Privileges/getall')
        .then((res: AxiosResponse) => {
            data.claims = res.data;
        })
        .catch();
};
var isUpdate = ref(false);
const onClickOpenRoleDlg = (role: Role | null, isCopy?: boolean): void => {
    payload.value = new Role();
    if (role) {
        API.get(`role/${role.id}`).then((res: AxiosResponse) => {
            payload.value = new Role(res.data);
            if (isCopy) {
                payload.value.name += ' - Copy';
                isUpdate.value = false;
            } else {
                isUpdate.value = true;
            }
        });
    } else {
        isUpdate.value = false;
        payload.value = new Role();
    }
    visible.value = true;
};

const onClickSaveRole = (): void => {
    loading.role = true;
    const dataBody = payload.value.toJSON(); 
    // return;
    if (!isUpdate.value) {
        dataBody.id = 0;
        API.add('role/add', dataBody)
            .then((res) => {
                data.roles.push(res.data);
                toast.add({
                    severity: 'success',
                    summary: t('body.systemSetting.success_label'),
                    detail: 'Thêm mới thành công',
                    life: 3000
                });
                loading.role = false;
                visible.value = false;
                fetchRoles();
            })
            .catch((error) => {
                toast.add({
                    severity: 'error',
                    summary: 'Lỗi',
                    detail: t('body.report.error_occurred_message'),
                    life: 3000
                });
                loading.role = false;
                visible.value = false;
            });
    } else {
        API.update(`role/${dataBody.id}`, dataBody)
            .then((res) => {
                toast.add({
                    severity: 'success',
                    summary: t('body.systemSetting.success_label'),
                    detail: 'Cập nhật thành công',
                    life: 3000
                });
                loading.role = false;
                visible.value = false;
                fetchRoles();
            })
            .catch((error) => {
                toast.add({
                    severity: 'error',
                    summary: 'Lỗi',
                    detail: t('body.report.error_occurred_message'),
                    life: 3000
                });
                loading.role = false;
                visible.value = false;
            });
        loading.role = false;
        visible.value = false;
    }
};

const onClickCopy = (data: Role): void => {
    const roleCopy = new Role(data);
    roleCopy.name += ' (Copy)';

    onClickOpenRoleDlg(roleCopy, true);
};

const onClickConfirmDelete = (data: Role): void => {
    confirm.require({
        message: 'Bạn có muốn xóa vai trò này không',
        header: t('body.systemSetting.update_role') || 'Xác nhận xóa',
        rejectClass: 'p-button-secondary',
        acceptClass: 'p-button-danger',
        rejectLabel: t('body.OrderList.close') || 'Hủy',
        acceptLabel: t('body.OrderList.delete') || 'Xóa',
        accept: () => {
            deleteRole(data.id);
        },
        reject: () => {}
    });
};

const deleteRole = (id: number | null): void => {
    API.delete(`role/${id}`)
        .then(() => {
            fetchRoles();
            toast.add({
                severity: 'success',
                summary: t('body.systemSetting.success_label'),
                detail: 'Xóa thành công',
                life: 3000
            });
        })
        .catch((error) => {
            console.error(error);
            toast.add({
                severity: 'error',
                summary: 'Lỗi',
                detail: error.response?.data?.errors || t('body.report.error_occurred_message'),
                life: 3000
            });
        });
};

onBeforeMount(() => {
    fetchRoles();
    fetchClaims();
    fetchNhomKH();
});
</script>

<style></style>
