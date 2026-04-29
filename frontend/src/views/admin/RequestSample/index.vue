
<template>
    <div class="flex justify-content-between mb-4">
        <h4 class="m-0 font-semibold">{{ t('body.sampleRequest.sampleProposal.title') }}</h4>
        <router-link :to="{ name: 'request-sample-create' }">
            <Button :label="t('body.sampleRequest.sampleProposal.addNew')" icon="pi pi-plus"/>
        </router-link>
    </div>

    <DataTable
        :value="dataSample"
        class="card p-3"
        showGridlines
        resizableColumns
        columnResizeMode="fit"
        lazy
        paginator
        :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
        @page="onPageChange($event)"
        :rows="paginator.pageSize"
        :page="paginator.page"
        :totalRecords="paginator.total"
        :emptyMessage="t('body.sampleRequest.sampleProposal.noData')"
    >
        <template #header>
            <div class="flex">
                <IconField iconPosition="left">
                    <InputText
                        @input="onSearchDebounce"
                        v-model="paginator.filter"
                        :placeholder="t('body.sampleRequest.sampleProposal.searchPlaceholder')"
                    />
                    <InputIcon>
                        <i class="pi pi-search" />
                    </InputIcon>
                </IconField>
            </div>
        </template>
        <Column header="#" class="w-1rem">
            <template #body="{ index }">
                {{ index + 1 }}
            </template>
        </Column>

        <Column :header="t('body.sampleRequest.sampleProposal.code')" field="code">
            <template #body="{ data }">
                <router-link
                    :to="{ name: 'request-sameple-edit', params: { id: data.id } }"
                >
                    <span class="text-primary hover:underline cursor-pointer">
                        <i class="pi pi-arrow-right mr-2"></i>
                        {{ data.invoiceCode }}
                    </span>
                </router-link>
            </template>
        </Column>
        <Column :header="t('body.sampleRequest.sampleProposal.customerName')" field="cardName"></Column>
        <Column :header="t('body.sampleRequest.sampleProposal.creator')" field="userName"></Column>
        <Column :header="t('body.sampleRequest.sampleProposal.createdAt')" field="docDate">
            <template #body="{ data }">
                <span v-if="data.docDate">{{ format(data.docDate, "dd/MM/yyyy") }}</span>
            </template>
        </Column>
        <Column :header="t('body.sampleRequest.sampleProposal.status')" field="status" style="width: 10rem">
            <template #body="{ data }">
                <Tag
                    :value="getSeverity(data.status, 'label')"
                    :severity="getSeverity(data.status, 'severity')"
                />
            </template>
        </Column>
        <Column style="width: 1rem">
            <template #body="{ data }">
                <Button disabled icon="pi pi-trash" text severity="danger"/>
            </template>
        </Column>
        <template #empty>
            <div class="text-center py-5">{{ t('body.sampleRequest.sampleProposal.noData') }}</div>
        </template>
    </DataTable>
</template>

<script setup>
import { ref, onMounted } from "vue";
import API from "@/api/api-main";
import { format } from "date-fns";
import { debounce, filter } from "lodash";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const dataSample = ref([]);

const paginator = ref({
    page: 1,
    pageSize: 30,
    total: 0,
    filter: "",
});

onMounted(() => {
    GetTestSample();
});
const GetTestSample = async () => {
    try {
        let url = `RequestOfExample?Page=${paginator.value.page}&PageSize=${paginator.value.pageSize}&OrderBy=id desc`;
        if (paginator.value.filter) url += `&Search=${paginator.value.filter}`;
        const { data } = await API.get(url);
        if (data) {
            dataSample.value = data.items;
            paginator.value.total = data.total;
        }
    } catch (error) {
        console.error(error);
    }
};

const stts = {
    DXL: {
        severity: "warning",
        label: t('body.home.processing')
    },
    DXN: {
        severity: "info",
        label: t('body.home.confirmed')
    },
    HT: {
        severity: "success",
        label: t('body.sampleRequest.sampleProposal.Complete')
    },
    HUY: {
        severity: "danger",
        label: t('body.status.HUY')
    },
    NHAP: {
        severity: "secondary",
        label: t('body.sampleRequest.sampleProposal.NHAP')
    },
};
const getSeverity = (status, prop) => {
    return stts[status]?.[prop] || "";
};

const onSearchDebounce = debounce(() => {
    GetTestSample();
}, 400);

const onPageChange = (e) => {
    paginator.value.pageSize = e.rows;
    paginator.value.page = e.page + 1;
    GetTestSample();
};
</script>
