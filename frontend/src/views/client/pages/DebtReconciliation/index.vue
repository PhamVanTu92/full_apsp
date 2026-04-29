<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">Biên bản đối chiếu công nợ</h4>
            <Button
                v-if="0"
                @click="onClickOpenDebtRecDlg()"
                icon="pi pi-plus"
                label="Thêm mới"
            />
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
            <!-- <Column field="customerName" header="Khách hàng"></Column> -->
            <Column field="createdAt" header="Ngày gửi">
                <template #body="{ data }">
                    {{
                        data.sendingDate
                            ? format(data.sendingDate || "", "dd/MM/yyyy")
                            : ""
                    }}
                </template>
            </Column>
            <Column field="createdAt" header="Ngày xác nhận">
                <template #body="{ data }">
                    {{
                        data.confirmationDate
                            ? format(data.confirmationDate || "", "dd/MM/yyyy")
                            : ""
                    }}
                </template>
            </Column>
            <!-- <Column field="creatorName" header="Người gửi"></Column> -->
            <Column field="status" header="Trạng thái">
                <template #body="{ data }">
                    <Tag
                        :value="getStatus(data.status).label"
                        :severity="getStatus(data.status).severity"
                    ></Tag>
                </template>
            </Column>
            <template #header>
                <div class="flex justify-content-between">
                    <InputText @input="onSearch" placeholder="Tìm kiếm..."></InputText>
                </div>
            </template>
            <template #empty>
                <div class="text-center py-5 my-5 text-500 font-italic">
                    Không có dữ liệu để hiển thị
                </div>
            </template>
        </DataTable>
        <DebtRecDlg
            v-model:visible="visible.debtRec"
            :id="selection.DebtRecId"
            @save="onConfirmDebtRec"
            :loading="loading.button"
        />
    </div>
</template>

<script setup lang="ts">
"use strict";
import { ref, reactive, computed, onMounted, watch } from "vue";
// import {
//     getDebtRecData,
//     handleCreate,
//     handleUpdate,
//     handleSaveFiles,
//     getStatus,
// } from "./script";
import { getStatus } from "./script";
import { DebtRecResponse, IDebtRec } from "./entities/DebtRec";
import DebtRecDlg from "./components/DebtRecDlg.vue";
import { useRouter, useRoute } from "vue-router";
import { cloneDeep, debounce } from "lodash";
import { useToast } from "primevue/usetoast";
import { format } from "date-fns";

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
const onConfirmDebtRec = async (event: {
    isValid: boolean;
    model: IDebtRec;
}): Promise<void> => {
    fetchData();
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

const fetchData = async (
    skip?: number,
    limit?: number,
    search?: string
): Promise<void> => {
    try {
        const response = await getDebtRecData(skip, limit, search);
        dataTable.value = response;
    } catch (error) {}
};

onMounted(async function () {
    fetchData();
    const queryId = route.query.id;
    if (queryId && typeof queryId === "string") {
        onClickOpenDebtRecDlg(parseInt(queryId));
    }
});

// API service
import API from "@/api/api-main";
import { AxiosResponse } from "axios";
const getDebtRecData = async (
    skip: number = 0,
    limit: number = 10,
    search?: string
): Promise<DebtRecResponse> => {
    skip ??= 1;
    limit ??= 10;
    search ??= "";
    let uri = `DebtReconciliation?Page=${skip}&PageSize=${limit}&OrderBy=createdAt desc`;
    if (search) {
        uri += `&search=${search?.trim()}`;
    }
    const res = await API.get(uri);
    return res.data;
};
</script>

<style scoped></style>
