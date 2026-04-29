<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.report.debt_reconciliation_report_title') }}</h4>
            <Button @click="onClickOpenDebtRecDlg()" icon="pi pi-plus" :label="t('body.productManagement.add_new_button')" />
        </div>
        <DataTable
            paginator
            :rows="query.limit"
            :value="dataTable.items || []"
            showGridlines
            class="card p-3"
            :rowsPerPageOptions="[10, 20, 30]"
            lazy
            @page="onPage"
            :totalRecords="dataTable.total"
        >
            <Column field="code" header="Mã biên bản">
                <template #body="slotProps">
                    <div
                        @click="onClickOpenDebtRecDlg(slotProps.data.id)"
                        class="text-primary hover:underline font-semibold cursor-pointer"
                    >
                        {{ slotProps.data.code }}
                    </div>
                </template>
            </Column>
            <Column field="name" header="Tên biên bản"></Column>
            <Column field="customerName" :header="t('body.report.table_header_customer_name_2')"></Column>
            <Column field="createdAt" :header="t('body.report.document_date')">
                <template #body="{ data }">
                    {{
                        data.sendingDate
                            ? format(data.sendingDate || "", "dd/MM/yyyy")
                            : ""
                    }}
                </template>
            </Column>
            <Column field="" header="Ngày xác nhận">
                <template #body="{ data }">
                    {{
                        data.confirmationDate
                            ? format(data.confirmationDate, "dd/MM/yyyy")
                            : ""
                    }}
                </template>
            </Column>
            <Column field="creatorName" header="Người gửi"></Column>
            <Column field="status" :header="t('body.OrderApproval.status')">
                <template #body="{ data }">
                    <Tag
                        :value="getStatus(data.status).label"
                        :severity="getStatus(data.status).severity"
                    ></Tag>
                </template>
            </Column>
            <template #header>
                <div class="flex justify-content-between">
                    <InputText @input="onSearch" :placeholder="t('body.report.search_placeholder_2')"></InputText>
                </div>
            </template>
            <template #empty>
                <div class="text-center py-5 my-5 text-500 font-italic">
                    {{ t('body.systemSetting.no_data_to_display') }}
                </div>
            </template>
        </DataTable>
        <DebtRecDlg
            v-model:visible="visible.debtRec"
            :id="selection.DebtRecId"
            @save="onSaveDebtRec"
            @resent="onResentDebtRec"
            @changeStt="onChangeStt"
            :loading="loading.button"
        />
    </div>
    <Loading v-if="loadingDataTable"></Loading>
</template>

<script setup lang="ts">
"use strict";
import { ref, reactive, computed, onMounted, watch } from "vue";
import {
    getDebtRecData,
    handleCreate,
    handleUpdate,
    handleSaveFiles,
    handleChangeStatus,
    getStatus,
} from "./script";
import { DebtRecResponse, IDebtRec } from "./entities/DebtRec";
import DebtRecDlg from "./components/DebtRecDlg.vue";
import { useRouter, useRoute } from "vue-router";
import { cloneDeep, debounce } from "lodash";
import { useToast } from "primevue/usetoast";
import { format } from "date-fns";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const doSearch = async (e) => {
    query.search = e.data;
    query.skip = 1;
    dataTable.value = await getDebtRecData(query.skip, query.limit, query.search);
};
const onSearch = debounce(doSearch, 200);

const router = useRouter();
const route = useRoute();
const toast = useToast();

const query = reactive({
    limit: 10,
    skip: 0,
    search: "",
});
const onPage = async (event: any): Promise<void> => {
    query.skip = event.page;
    query.limit = event.rows;
    dataTable.value = await getDebtRecData(query.skip + 1, query.limit, query.search);
};

const dataTable = ref<DebtRecResponse>({
    items: [],
    pageSize: 0,
    page: 0,
    total: 0,
});

const visible = reactive({
    debtRec: false,
});

const selection = reactive({
    DebtRecId: null as number | null,
});
const loading = reactive({
    button: false,
});

const onChangeStt = () => {
    fetchData();
};

const handleDelete = (id: number, ids: Array<number>): void => {
    if (ids.length) {
        API.delete(`DebtReconciliation/${id}/attachments`, ids)
            .then((res) => res)
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Xóa tài liệu đính kèm thất bại!",
                    life: 3000,
                });
            });
    }
};

import { AxiosResponse } from "axios";
const onResentDebtRec = (data: {
    isValid: boolean;
    model: IDebtRec;
    deleteFiles: Array<number>;
}) => {
    if (data.isValid) {
        loading.button = true;
        const payload = cloneDeep(data.model) as IDebtRec;
        payload.status = "P";
        delete payload.attachments;
        const newFiles = data.model.attachments
            ?.filter((el) => !el.id)
            .map((el) => el.file);
        handleUpdate(payload)
            .then((res: AxiosResponse<IDebtRec>) => {
                loading.button = false;
                visible.debtRec = false;
                handleDelete(payload.id || 0, data.deleteFiles);
                handleSaveFiles(payload.id || 0, newFiles || [])
                    .then((_res) => {})
                    .catch((error) => {
                        toast.add({
                            severity: "error",
                            summary: "Lỗi",
                            detail: "Xảy ra lỗi trong quá trình tải lên tài liệu!",
                            life: 3000,
                        });
                    });
                handleChangeStatus(payload.id || 0, "P")
                    .then((res) => {
                        toast.add({
                            severity: "success",
                            summary: "Thành công",
                            detail: "Đã gửi lại biên bản đối chiếu công nợ!",
                            life: 3000,
                        });
                        fetchData();
                    })
                    .catch((error) => {
                        toast.add({
                            severity: "error",
                            summary: "Lỗi",
                            detail:
                                "Đã xảy ra lỗi trong quá trình gửi lại biên bản đối chiếu công nợ!",
                            life: 3000,
                        });
                    });
                loading.button = false;
            })
            .catch((error) => {
                loading.button = false;
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail:
                        "Đã xảy ra lỗi trong quá trình gửi lại biên bản đối chiếu công nợ!",
                    life: 3000,
                });
            });
    }
};

const onSaveDebtRec = async (event: {
    isValid: boolean;
    model: IDebtRec;
}): Promise<void> => {
    try {
        loading.button = true;
        if (event.isValid) {
            try {
                const payload = cloneDeep(event.model) as IDebtRec;
                const httpMethod = payload.id ? handleUpdate : handleCreate;
                delete payload.attachments;
                payload.status = "P";
                const saveFormResult = await httpMethod(payload);

                if (saveFormResult.data.id) {
                    const newFiles = event.model.attachments?.map((row) => row.file);
                    const saveFileResult = await handleSaveFiles(
                        saveFormResult.data.id,
                        newFiles || []
                    );

                } else {
                    toast.add({
                        severity: "error",
                        summary: "Lỗi",
                        detail: "Đã có lỗi xảy ra trong quá trình tải lên tài liệu",
                        life: 3000,
                    });
                }

                //Finish
                toast.add({
                    severity: "success",
                    summary: "Thành công",
                    detail: "Đã lưu dữ liệu thành công!",
                    life: 3000,
                });
                // Refresh data table
                fetchData();
                // Close dialog
                visible.debtRec = false;
            } catch (error) {
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Đã có lỗi xảy ra!",
                    life: 3000,
                });
            }
        }
    } catch (error) {
        console.error(error);
    }
    loading.button = false;
};

const onClickOpenDebtRecDlg = (id?: number): void => {
    selection.DebtRecId = null;
    if (id) {
        selection.DebtRecId = id;
        router.replace({ query: { id: `${id}` } });
    } else {
        router.replace({ name: route.name });
    }
    visible.debtRec = true;
};

watch(
    () => route.query.id,
    (id) => {
        if (typeof id === "string") {
            onClickOpenDebtRecDlg(parseInt(id));
        }
    }
);

const loadingDataTable = ref(false);
const fetchData = async (skip?: number, limit?: number, text?: string): Promise<void> => {
    loadingDataTable.value = true;
    try {
        const response = await getDebtRecData(skip, limit, text);
        dataTable.value = response;
    } catch (error) {}
    loadingDataTable.value = false;
};

onMounted(async function () {
    fetchData();
    const queryId = route.query.id;
    if (queryId && typeof queryId === "string") {
        onClickOpenDebtRecDlg(parseInt(queryId));
    }
});
</script>

<style scoped></style>
