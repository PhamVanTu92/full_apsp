<template>
    <div>
        <DataTable v-model:expanded-rows="expandedRows" v-model:selection="expandedRows" :value="dataTable.data" lazy
            :rows="query.limit" :total-records="dataTable.total" showGridlines striped-rows paginator @page="onPage"
            selection-mode="single" @row-select="onRowSelect" @row-unselect="onRowUnselect">
            <template #header>
                <div class="flex justify-content-between gap-2">
                    <InputText v-model="query.search" @input="onSearch"
                        :placeholder="t('body.home.search_placeholder')"></InputText>
                    <div class="flex gap-2">
                        <Button icon="pi pi-filter-slash" severity="danger" outlined/>
                        <Button @click="router.push({ name: 'add-customer' })"
                            :label="t('body.systemSetting.create_account_button')"/>
                    </div>
                </div>
            </template>
            <Column field="userName" :header="t('body.systemSetting.table_header_customer_code')" class="w-18rem">
            </Column>
            <Column field="email" :header="t('body.systemSetting.email_label')" class="w-25rem"></Column>
            <Column field="fullName" :header="t('body.systemSetting.full_name_column')"></Column>
            <template #expansion="{ data }">
                <div class="card p-3">
                    <div class="grid">
                        <div class="col-2">
                            <div class="field">
                                <label class="font-semibold" for="">{{
                                    t('body.systemSetting.table_header_customer_code') }}</label>
                                <div class="value">{{ data.userName }}</div>
                            </div>
                        </div>
                        <div class="col-5">
                            <div class="field">
                                <label class="font-semibold" for="">{{ t('body.systemSetting.full_name_column')
                                }}</label>
                                <InputText v-if="!~!updatePayload" v-model="updatePayload.fullName" class="w-full">
                                </InputText>
                                <div v-else class="value">{{ data.fullName }}</div>
                            </div>
                        </div>
                        <div class="col-3">
                            <div class="field">
                                <label class="font-semibold" for="">{{ t('body.systemSetting.email_label') }}</label>
                                <InputText v-if="!!updatePayload" v-model="updatePayload.email" class="w-full">
                                </InputText>
                                <div v-else class="value">{{ data.email }}</div>
                            </div>
                        </div>
                        <div class="col-2">
                            <div class="field flex flex-column h-full">
                                <label class="font-semibold" for="">{{ t('body.systemSetting.status_label') }}</label>
                                <div class="flex-grow-1 flex align-items-center">
                                    <Tag v-if="data.status == 'A'">{{
                                        t('body.sampleRequest.customerGroup.active_status_option') }}</Tag>
                                    <Tag v-else severity="danger">{{
                                        t('client.deactivate') }}</Tag>
                                </div>
                            </div>
                        </div>
                    </div>
                    <Divider></Divider>
                    <div class="flex justify-content-end align-items-center gap-2">
                        <Button v-if="!updatePayload" @click="updatePayload = cloneDeep(data)"
                            :label="t('body.OrderList.edit')"/>
                        <template v-else>
                            <Button :label="t('body.systemSetting.reset_password')" icon="pi pi-sync" severity="info"
                                @click="onClickResetPw(updatePayload)"/>
                            <Button
                                :label="updatePayload.status == 'A' ? t('body.systemSetting.deactivate_account') : t('body.systemSetting.activate_deactivate_account')"
                                :icon="updatePayload.status == 'A' ? 'pi pi-lock' : 'pi pi-unlock'"
                                :severity="updatePayload.status == 'A' ? 'danger' : ''"
                                @click="onClickStatus(updatePayload)"/>
                            <div class="h-2rem border-left-1 border-300"></div>
                            <Button @click="updatePayload = null" :label="t('body.home.cancel_button')"
                                severity="secondary"/>
                            <Button :loading="loading" @click="onSaveCus"
                                :label="t('body.systemSetting.save_button')"/>
                        </template>
                    </div>
                </div>
            </template>
            <Column field="status" :header="t('body.PurchaseRequestList.status')">
                <template #body="{ data }">
                    <Tag v-if="data.status == 'A'">
                        {{ t('body.sampleRequest.customerGroup.active_status_option') }}
                    </Tag>
                    <Tag v-else severity="danger">
                        {{ t('client.deactivate') }}
                    </Tag>
                </template>
            </Column>
            <template #empty>
                <div class="flex justify-content-center align-items-center" style="height: 50vh">
                    <div>{{ t('body.systemSetting.no_data_to_display') }}</div>
                </div>
            </template>
        </DataTable>

        <ResetPasswordDialog ref="resetPasswordDialogRef"></ResetPasswordDialog>
    </div>
</template>

<script setup lang="ts">
import API from "@/api/api-main";
import { ref, reactive, onMounted } from "vue";
import { useRouter } from "vue-router";
import { cloneDeep, debounce } from "lodash";
import { useToast } from "primevue/usetoast";
import ResetPasswordDialog from "../dialog/ResetPasswordDialog.vue";
import { useI18n } from "vue-i18n"; 
const { t } = useI18n(); 
const resetPasswordDialogRef = ref<InstanceType<typeof ResetPasswordDialog>>(); 
const handleStatus = (id: number, status: "A" | "I"): void => {
    API.add(`account/DeActive?id=${id}&Status=${status}`)
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.save_button'),
                detail: t('body.systemSetting.update_account_success_message') || "Cập nhật tài khoản thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.clear') || "Lỗi",
                detail: t('body.report.error_occurred_message') || "Cập nhật tài khoản thất bại",
                life: 3000,
            });
            console.error(error);
        });
};

const onClickStatus = (user: any) => {
    let status: "I" | "A" = user.status == "A" ? "I" : "A";
    handleStatus(user.id, status);
};

const onClickResetPw = (user: any) => {
    resetPasswordDialogRef.value?.open(user);
};

const router = useRouter();
const query = reactive({
    skip: 0,
    limit: 10,
    search: "",
    userType: "NPP",
    OrderBy: "id desc",
});
const dataTable = reactive({
    data: [] as any[],
    total: 0,
});

const updatePayload = ref<any>();
const expandedRows = ref<any[]>([]);
const toast = useToast();
const loading = ref(false);

const onSaveCus = () => {
    loading.value = true;
    API.update(`account/${updatePayload.value.id}`, updatePayload.value)
        .then((res) => {
            updatePayload.value = null;
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.save_button'),
                detail: t('body.systemSetting.update_account_success_message') || "Cập nhật thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.clear') || "Lỗi",
                detail: t('body.report.error_occurred_message') || "Đã có lỗi xảy ra",
                life: 3000,
            });
        })
        .finally(() => {
            loading.value = false;
            fetchCustomers();
        });
};

const onRowUnselect = (e: any) => {
    expandedRows.value = [];
    updatePayload.value = null;
};
const onRowSelect = (e: any) => {
    expandedRows.value = [e.data];
};

const getQuery = () => {
    const urlSearchParam = new URLSearchParams(
        Object.fromEntries(
            Object.entries(query)
                .filter((item) => item[1] != null)
                .map(([key, value]) => [key, String(value)])
        )
    );
    return urlSearchParam.toString();
};

const fetchCustomers = () => {
    API.get(`account/getall?${getQuery()}`)
        .then((res) => {
            dataTable.data = res.data["item"];
            dataTable.total = res.data["total"];
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.clear') || "Lỗi",
                detail: t('body.report.error_occurred_message') || "Đã có lỗi xảy ra",
                life: 3000,
            });
        });
};
const debouncer = debounce(() => {
    query.skip = 0;
    fetchCustomers();
}, 500);
const onSearch = () => {
    debouncer();
};

const onPage = (e: any) => {
    query.skip = e.page;
    query.limit = e.rows;
    fetchCustomers();
};

const initialComponent = () => {
    // code here
    fetchCustomers();
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.field {
    margin-bottom: 0;
}

.value {
    line-height: 33px;
}
</style>
