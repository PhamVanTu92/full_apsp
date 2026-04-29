t<template>
    <div>
        <div class="flex justify-content-between mb-4">
            <h4 class="font-bold m-0">{{ t('body.productManagement.product_group_title') }}</h4>
            <Button
                icon="pi pi-plus"
                :label="t('body.productManagement.add_new_button')"
                class="mr-3"
                @click="onClickOpenDlg()"
            />
        </div>
        <DataTable
            class="card p-3"
            :value="productGroupData.itemGroup"
            lazy
            showGridlines
            paginator
            :rowsPerPageOptions="[10, 20, 30, 40, 50]"
            :rows="query.limit"
            :totalRecords="productGroupData.total"
        >
            <Column field="itmsGrpName" :header="t('body.productManagement.product_group_name_column')"></Column>
            <Column field="description" :header="t('body.productManagement.description_column')"></Column>
            <Column field="isActive" :header="t('body.productManagement.status_column')" class="w-10rem">
                <template #body="{ data }">
                    <Tag
                        :severity="data.isActive ? 'primary' : 'danger'"
                        :value="data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status')"
                    ></Tag>
                </template>
            </Column>
            <Column header="" class="w-1rem">
                <template #body="{ data }">
                    <div class="flex">
                        <Button
                            @click="onClickOpenDlg(data.id)"
                            icon="pi pi-pencil"
                            text
                        />
                        <Button
                            v-if="0"
                            @click="onClickDelete(data.id)"
                            icon="pi pi-trash"
                            text
                            severity="danger"
                        />
                    </div>
                </template>
            </Column>
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputText
                            @input="onSearch"
                            v-model="query.search"
                            :placeholder="t('body.productManagement.search_placeholder')"
                        ></InputText>
                        <InputIcon>
                            <i class="pi pi-search" />
                        </InputIcon>
                    </IconField>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-center text-500 font-italic">
                    {{ t('body.systemSetting.no_data_to_display') }}
                </div>
            </template>
        </DataTable>
        <ProductGroupDialog ref="dialogRef" @refresh-data="onRefreshData" />
        <ConfirmDialog></ConfirmDialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import ProductGroupDialog from "./components/ProductGroupDialog.vue";
import {
    IProductGroup,
    IProductGroupResponse,
    fetchProductGroup,
    deleteProductGroupById,
} from "./entities/ProductGroup";
import { AxiosError } from "axios";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { debounce } from "lodash";
import { useI18n } from "vue-i18n";

const confirm = useConfirm();
const toast = useToast();

const { t } = useI18n();

const onSearch = debounce(() => {
    query.value.skip = 0;
    fetchData(query.value);
}, 500);

const handleDelete = (id: number) => {
    deleteProductGroupById(id)
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.productManagement.product_group_title') + ' ' + t('body.systemSetting.update_account_success_message') /* fallback style */,
                life: 3000,
            });
            fetchData();
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
            console.error(error.message);
        });
};

const onClickDelete = (id: number) => {
    confirm.require({
        message: t('body.productManagement.delete') || "Are you sure to delete this product group?",
        header: t('body.OrderApproval.confirm') || "Confirm",
        rejectClass: "p-button-secondary",
        acceptClass: "p-button-danger",
        rejectLabel: t('body.PurchaseRequestList.cancel_button') || "Cancel",
        acceptLabel: t('body.SaleSchannel.delete') || "Delete",
        accept: () => {
            handleDelete(id);
        },
    });
};

const dialogRef = ref<InstanceType<typeof ProductGroupDialog>>();
const onClickOpenDlg = (id?: number): void => {
    dialogRef.value?.openDialog(id);
};

const query = ref({
    search: "",
    limit: 10,
    skip: 0,
});

const productGroupData = ref<IProductGroupResponse>({
    itemGroup: [] as Array<IProductGroup>,
    limit: 10,
    skip: 0,
    total: 0,
});

const onRefreshData = (res) => {
    query.value.skip = 0;
    fetchData();
};

const fetchData = (_query?: { skip: number; limit: number; search: string | null }) => {
    if (_query) {
        fetchProductGroup(_query.skip, _query.limit, _query.search)
            .then((res) => {
                productGroupData.value = res.data;
            })
            .catch((error: AxiosError) => {
                toast.add({
                    severity: "error",
                    summary: t('body.report.error_occurred_message'),
                    detail: t('body.report.error_occurred_message'),
                    life: 3000,
                });
                console.error(error.message);
            });
    } else {
        fetchProductGroup()
            .then((res) => {
                productGroupData.value = res.data;
            })
            .catch((error: AxiosError) => {
                toast.add({
                    severity: "error",
                    summary: t('body.report.error_occurred_message'),
                    detail: t('body.report.error_occurred_message'),
                    life: 3000,
                });
                console.error(error.message);
            });
    }
};

onMounted(function () {
    fetchData();
});
</script>

<style scoped></style>
