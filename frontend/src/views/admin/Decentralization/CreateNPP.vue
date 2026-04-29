<template>
    <div>
        <div class="mb-3 flex justify-content-between">
            <h4 class="mb-0 font-bold">{{ t('body.systemSetting.create_customer_account') }}</h4>
            <ButtonGoBack/>
        </div>
        <DataTable v-if="0" v-model:selection="selectedCustomer" :value="data.customers" selectionMode="multiple"
            class="card p-3" showGridlines paginator lazy @page="onPageChange" :totalRecords="data.total"
            :rows="params.limit" :rowsPerPageOptions="[10, 30, 50]">
            <template #header>
                <div class="flex">
                    <IconField iconPosition="left">
                        <InputIcon class="pi pi-search"> </InputIcon>
                        <InputText :placeholder="t('body.home.search_placeholder')" class=""> </InputText>
                    </IconField>
                </div>
            </template>
            <template #footer>
                <div class="flex gap-3 justify-content-end" style="height: 33px">
                    <div class="my-auto font-normal" v-if="selectedCustomer.length">
                        {{ t('body.productManagement.selected_label') }}
                        <span class="font-bold mx-1">{{ selectedCustomer.length }}</span>
                        {{ t('OrderList.customer') }}
                    </div>
                    <Button v-if="selectedCustomer.length" :disabled="!selectedCustomer.length"
                        :label="t('body.systemSetting.create_account_button')" icon="pi pi-user"
                        @click="onClickOnpenDlg" :loading="loading.dialog"/>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
            </template>
            <Column selectionMode="multiple" headerStyle="width: 3rem" />
            <Column field="cardCode" :header="t('body.systemSetting.table_header_customer_code')"
                style="max-width: 10rem" />
            <Column field="cardName" :header="t('body.systemSetting.full_name_column')" />
            <Column field="email" :header="t('body.systemSetting.table_header_email')" />
        </DataTable>

        <DataTable v-model:editingRows="editingRows" v-model:selection="selectionRows" editMode="row" :value="customers"
            show-gridlines class="card p-3 mb-5" @row-edit-save="onRowEditSave" scrollable scroll-height="60vh">
            <Column selection-mode="multiple" class="w-3rem"></Column>
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="cardCode" :header="t('body.systemSetting.table_header_customer_code')" class="w-20rem">
            </Column>
            <Column field="cardName" :header="t('body.systemSetting.full_name_column')"></Column>
            <Column :header="t('body.systemSetting.table_header_confirmation_status')" class="w-20rem">
                <template #body="{ data }">
                    {{ data.userInfo }}
                </template>
            </Column>
            <Column field="email" :header="t('body.systemSetting.table_header_email')" class="w-30rem">
                <template #editor="{ data, field, index }">
                    <InputText :invalid="!!errorMsg[index]" v-model="data[field]" class="w-full" autofocus></InputText>
                    <small class="text-red-500">{{ errorMsg[index] }}</small>
                </template>
            </Column>

            <Column rowEditor class="w-7rem">
                <template #body="{ data, index, editorInitCallback }">
                    <div class="flex">
                        <Button @click="onEditorInit(data, editorInitCallback)" icon="pi pi-pencil" text/>
                        <Button @click="onClickRemoveCustomer(index)" icon="pi pi-trash" text
                            severity="danger"/>
                    </div>
                </template>
                <template #editor="{ data, index, editorSaveCallback, editorCancelCallback }">
                    <div class="flex">
                        <Button @click="onRowSaveChange(data, index, editorSaveCallback)" icon="pi pi-check"
                            text/>
                        <Button @click="onEditorCancel(index, editorCancelCallback)" icon="pi pi-times" text
                            severity="danger"/>
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="flex justify-content-center align-items-center" style="height: 50vh">
                    <div class="my-5 py-5 text-center">
                        <div class="mb-3">
                            {{ t("Custom.add_new_customer") }}
                        </div>
                    </div>
                </div>
            </template>
            <template #footer>
                <div class="flex gap-2 justify-content-between">
                    <div>
                        <Button @click="customerSelectDialogRef?.open()" :label="t('Custom.addCustomer')" outlined />
                        <Button @click="onClickDeleteRows" v-if="selectionRows.length"
                            :label="t('body.OrderList.delete')" severity="danger" class="ml-2" />
                    </div>
                    <Button :disabled="customers.length < 1" @click="visibleComfirm = true"
                        :label="t('body.systemSetting.create_account_button')"/>
                </div>
            </template>
        </DataTable>
        <ConfirmDialog></ConfirmDialog>
        <!-- <Loading v-if="loading.data" /> -->
        <Dialog v-model:visible="visibleComfirm" :header="t('body.OrderApproval.confirm')" modal class="w-30rem">
            <div>
                <template v-if="editingRows.length">
                    {{ t('Custom.messNosave') }}
                </template>
                <template v-else>
                    {{ t('Custom.messNosave1') }}
                    <span class="font-bold">{{ customers.length }}</span> {{ t('Custom.messNosave2') }}
                </template>
            </div>
            <template #footer>
                <Button @click="visibleComfirm = false" :label="t('body.OrderList.close')"
                    severity="secondary"/>
                <Button @click="onCreateNPPAccount" :loading="createLoading"
                    :label="t('body.OrderApproval.confirm')"/>
            </template>
        </Dialog>
        <CustomerSelectDialog @item-select="onSelectCustomer" :exception-ids="selectedIds"
            ref="customerSelectDialogRef">
        </CustomerSelectDialog>
    </div>
</template>

<script lang="ts" setup>
import { useToast } from "primevue/usetoast";
import { reactive, ref, onMounted, computed } from "vue";
import { useConfirm } from "primevue/useconfirm";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
// Components
interface Customer {
    id: number;
    email: string;
    cardCode: string;
    cardName: string;
}

const editingRows = ref<any[]>([]);
const selectionRows = ref<any[]>([]);
const customers = ref<Customer[]>([]);
const selectedIds = computed(() => customers.value.map((u) => u.id));
const visibleComfirm = ref(false);

const onClickDeleteRows = () => {
    customers.value = customers.value.filter((item, index) => {
        const row = selectionRows.value.find((sItem) => sItem.id === item.id);
        if (row) {
            errorMsg[index] = "";
        }
        return !row;
    });
    selectionRows.value = [];
};
const createLoading = ref(false);
const onCreateNPPAccount = () => {
    const payload = customers.value.map((kh) => ({
        cardId: kh.id,
        email: kh.email,
    }));
    createLoading.value = true;
    API.add("account/register-businesspartner", payload)
        .then((res) => { 
            for (let resq of res.data) {
                if (resq.errorMessage) {
                    toast.add({
                        severity: "error",
                        summary: "Lỗi",
                        detail: `Tạo tài khoản cho ${resq.bpCardCode} thất bại: ${resq.errorMessage}`,
                        life: 5000,
                    });
                } else {
                    toast.add({
                        severity: "success",
                        summary: t('body.systemSetting.success_label') || "Thành công",
                        detail: `Đã gửi email tài khoản cho  ${payload.length} nhà phân phối`,
                        life: 3000,
                    });
                }
            }
            editingRows.value = [];
            selectionRows.value = [];
            customers.value = []; 
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra.",
                life: 3000,
            });
        })
        .finally(() => {
            visibleComfirm.value = false;
            createLoading.value = false;
        });
};

import { PATTERN } from "@/helpers/validate";
const errorMsg = reactive<{ [key: string]: string }>({});

const onRowSaveChange = (data: any, index: number, saveCallback: Function) => {
    errorMsg[`${index}`] = "";
    if (!PATTERN.email.test(data.email)) {
        errorMsg[`${index}`] = data.email
            ? t('body.systemSetting.invalid_email_message') || "Email không đúng định dạng"
            : "Vui lòng nhập email";
        return;
    } else {
        saveCallback();
    }
};

const onEditorInit = (data: any, callback: Function) => {
    editingRows.value = [];
    callback();
};

const onEditorCancel = (index: number, cancelCallback: Function) => {
    errorMsg[index] = "";
    cancelCallback();
};

const onRowEditSave = (event: any) => {
    const { newData, index } = event;
    customers.value[index] = newData;
};

const onSelectCustomer = (items: any[]) => {

    for (let item of items) {
        if (customers.value.findIndex((cus) => cus.id === item.id) == -1) {
            customers.value.push({
                id: item.id,
                cardCode: item.cardCode,
                cardName: item.cardName,
                email: item.email,
            });
        }
    }
};

const onClickRemoveCustomer = (index: number) => {
    delete errorMsg[index];
    customers.value.splice(index, 1);
};

import CustomerSelectDialog from "./dialog/CustomerSelectDialog.vue";
const customerSelectDialogRef = ref<InstanceType<typeof CustomerSelectDialog>>();

//============================================================================================
const toast = useToast();
const confirm = useConfirm();

const data = reactive({
    customers: Array<any>([]),
    total: 0,
});

const selectedCustomer = ref([]);

const onClickOnpenDlg = (): void => {
    confirm.require({
        message: `Bạn có muốn tạo tài khoản cho ${selectedCustomer.value.length} khách hàng này không?`,
        header: "Tạo tài khoản khách hàng",
        rejectClass: "p-button-secondary p-button-secondary",
        rejectLabel: "Hủy",
        rejectIcon: "pi pi-times",
        acceptIcon: "pi pi-check",
        acceptLabel: "Xác nhận",
        accept: () => {
            doCreate();
        },
        reject: () => { },
    });
};

const loading = reactive({
    data: false,
    dialog: false,
});
const doCreate = (): void => {
    const cusIds = selectedCustomer.value.map((cus: any) => {
        return cus.id;
    });
    loading.dialog = true;
    API.add("account/register-businesspartner", cusIds)
        .then((res: AxiosResponse) => {
            loading.dialog = false;
            toast.add({
                severity: "success",
                summary: "Thông báo",
                detail: "Yêu cầu đã được gửi thành công",
                life: 3000,
            });
        })
        .catch((err: AxiosError) => {
            loading.dialog = false;
            console.error(err.message);
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                life: 3000,
            });
        });
};

import { AxiosError, AxiosResponse } from "axios";
const params = {
    page: 0,
    limit: 10,
    total: 0,
};
const fetchCustomerUnregisted = (skip?: number): void => {
    loading.data = true;
    API.get(
        `customer?skip=${skip || 0}&limit=${params.limit
        }&filter=(portalRegistrationStatus=N)`
    )
        .then((res: AxiosResponse) => {
            loading.data = false;
            data.customers = res.data?.items;

            data.total = res.data?.total;
        })
        .catch((error) => {
            loading.data = false;
        });
};

const onPageChange = (event: any): void => {

    params.page = event.page;
    params.limit = event.rows;
    fetchCustomerUnregisted(params.page);
};

onMounted(() => {
    fetchCustomerUnregisted();
});
</script>

<style></style>
