<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-4">
            <h4 class="font-bold m-0">{{ t('body.sampleRequest.commitment.title') }}</h4>
            <div>
                <Button :label="t('body.sampleRequest.commitment.addNew')" @click="committedDlgComp?.open()"/>
            </div>
        </div>
        <DataTable
            :value="DATA_TABLE.data"
            showGridlines
            lazy
            paginator
            :totalRecords="DATA_TABLE.total || 0"
            class="card p-3"
            :rows="query.limit"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
            @page="onPageChange"
        >
            <Column header="#" class="w-1rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="committedCode" :header="t('body.sampleRequest.commitment.code')">
                <template #body="{ data }">
                    <span
                        @click="committedDlgComp?.open(data.id)"
                        class="text-primary font-semibold hover:underline cursor-pointer"
                        >{{ data.committedCode }}</span
                    >
                </template>
            </Column>
            <Column field="committedName" :header="t('body.sampleRequest.commitment.name')"></Column>
            <Column :header="t('body.sampleRequest.commitment.customerName')">
                <template #body="{ data }">
                    <router-link
                        class="text-primary font-semibold hover:underline"
                        :to="{
                            name: 'agencyCategory-detail',
                            params: {
                                id: data.cardId,
                            },
                        }"
                    >
                        {{ data.cardName }}
                    </router-link>
                </template>
            </Column>
            <Column field="committedYear" :header="t('body.sampleRequest.commitment.time')">
                <template #body="{ data }">
                    {{ new Date(data.committedYear)?.getFullYear() }}
                </template>
            </Column>
            <Column field="creator.fullName" :header="t('body.sampleRequest.commitment.createdBy')"></Column>
            <Column field="docStatus" :header="t('body.sampleRequest.commitment.status')">
                <template #body="{ data }">
                    <Tag
                        :value="getStatusLabel(data.docStatus).label"
                        :severity="getStatusLabel(data.docStatus).severity"
                    ></Tag>
                </template>
            </Column>
            <Column header="" class="w-1rem">
                <template #body="{ data }">
                    <Button
                        @click="onClickDelete(data)"
                        v-if="data.docStatus == 'D'"
                        icon="pi pi-trash"
                        text
                        severity="danger"
                    />
                </template>
            </Column>
            <template #header>
                <IconField iconPosition="left">
                    <InputIcon class="pi pi-search"> </InputIcon>
                    <InputText
                        v-model="query.search"
                        @input="onSearch"
                        :placeholder="t('body.home.search_placeholder')"
                    />
                </IconField>
            </template>
            <template #empty>
                <div class="text-500 font-italic py-5 my-5 text-center">
                    {{ t('body.systemSetting.no_data_to_display') }}
                </div>
            </template>
        </DataTable>
        <CommittedDlg
            ref="committedDlgComp"
            @refresh="onRefreshData"
            :showPayload="false"
        ></CommittedDlg>
        <Loading v-if="loading" />
        <Dialog v-model:visible="visibleConfirm" :header="t('body.OrderApproval.confirm')" modal>
            <div>
                Bạn có chắc chắn muốn xóa cam kết có mã:
                <span class="font-bold">{{ deleterow?.committedCode }}</span
                >?
            </div>
            <template #footer>
                <Button
                    :label="t('body.PurchaseRequestList.cancel_button')"
                    @click="visibleConfirm = false"
                    severity="secondary"
                />
                <Button
                    :label="t('body.SaleSchannel.delete')"
                    @click="deleteCommitted(deleterow.id)"
                    severity="danger"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import API from "@/api/api-main";
import { Axios, AxiosResponse } from "axios";
import { CommittedResponse, Committed, Creator } from "./entities/Committed";
import { useRoute, useRouter } from "vue-router";
import CommittedDlg from "./components/CommittedDlg.vue";
import { debounce } from "lodash";
import { useToast } from "primevue/usetoast";
import { watch } from "vue";
import { useI18n } from "vue-i18n";

const { t  } = useI18n();

const committedDlgComp = ref<InstanceType<typeof CommittedDlg>>();
const URI = "commited";
const router = useRouter();
const route = useRoute();
const loading = ref(false);
const query = reactive({
    skip: 0,
    limit: 10,
    search: "",
    OrderBy: "id desc",
    toQueryString: () => {
        let result: Array<string> = [];
        Object.keys(query).forEach((key) => {
            if (key != "toQueryString") result.push(`${key}=${query[key]}`);
        });
        return "?" + result.join("&");
    },
});


const deleteCommitted = (id) => {
    API.delete(`${URI}/${id}`)
        .then((res: AxiosResponse<any>) => {

            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: "Cam kết đã được xóa.",
                life: 3000,
            });
            visibleConfirm.value = false;
            onRefreshData();
        })
        .catch((error) => {
            console.error("Error", error);
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: error.response?.data?.errors || t('body.report.error_occurred_message'),
                life: 3000,
            });
        });
};

const visibleConfirm = ref(false);
const deleterow = ref<any>(null);
const onClickDelete = (row) => {
    visibleConfirm.value = true;
    deleterow.value = row;
};

const DATA_TABLE = ref<CommittedResponse>({
    data: [],
    total: 0,
    limit: 0,
    skip: 1,
});

const onSearch = debounce(() => {
    query.skip = 0;
    fetchData(query.toQueryString());
}, 500);

function onPageChange(event: any) {

    query.skip = event.page;
    query.limit = event.rows;
    fetchData(query.toQueryString());
}
function onRefreshData() {
    fetchData(query.toQueryString());
}

function fetchData(_query?: string, isPushToRouter = true): void {
    if (isPushToRouter)
        router.push({
            query: {
                ...route.query,
                limit: query.limit,
                skip: query.skip,
            },
        });
    loading.value = true;
    API.get(URI + _query)
        .then((res: AxiosResponse<CommittedResponse>) => {
            DATA_TABLE.value = res.data;
        })
        .catch((error) => {})
        .finally(() => {
            loading.value = false;
        });
}

const status = {
    P: {
        label: t('body.status.pending'),
        severity: "warning",
    },
    D: {
        label: t('body.status.draft'),
        severity: "info",
    },
    C: {
        label: t('body.status.DH'),
        severity: "danger",
    },
    A: {
        label: t('body.status.DXN'),
        severity: "primary",
    },
    R: {
        label: t('body.status.HUY'),
        severity: "danger",
    },
};

watch(
    () => route.query.id,
    (id) => {
        if (id) {
            committedDlgComp.value?.open(Number(id));
        }
    }
);

function getStatusLabel(str: string): any {
    return (
        status[str as keyof typeof status] || { label: t('body.report.undefined_label'), severity: "contrast" }
    );
}

onMounted(function () {
    fetchData(query.toQueryString(), false);
    if (route.query.id) {
        committedDlgComp.value?.open(Number(route.query.id));
    }
});
</script>

<style scoped></style>
