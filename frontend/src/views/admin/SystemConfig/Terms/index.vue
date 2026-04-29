<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.systemSetting.term_info_title') }}</h4>
            <Button @click="onClickUpdate" icon="pi pi-plus" :label="t('body.systemSetting.add_new_button')" />
        </div>
        <DataTable :value="dataTable.data" paginator :totalRecords="dataTable.total" class="card" :rows="paramQuery.pageSize" showGridlines @page="onPage">
            <Column field="name" :header="t('body.systemSetting.term_name_column')"></Column>
            <Column field="note" :header="t('body.systemSetting.note_column')"></Column>
            <Column field="createDate" :header="t('body.systemSetting.update_date_column')">
                <template #body="{ data, field }">
                    {{ fDate(data[field]) }}
                </template>
            </Column>
            <Column field="creator" :header="t('body.systemSetting.updater_column')"></Column>
            <Column field="status" :header="t('body.systemSetting.status')">
                <template #body="{ data, field }">
                    <Tag :value="getStatusLabel(data[field])['label']" :severity="getStatusLabel(data[field])['severity']"></Tag>
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.document_title')" class="w-7rem text-center">
                <template #body="{ data }">
                    <Button @click="onClickViewDoc(data)" icon="pi pi-eye" text/>
                </template>
            </Column>
            <template #empty>
                <div class="p-5 m-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
            </template>
        </DataTable>

        <TermDlg @success="fetchData()" ref="termDlgRef"></TermDlg>
        <PDFView v-model:visible="visible" v-if="visible" :url="pdfUrl" ref="pdfViewRef"></PDFView>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';
import TermDlg from './TermDlg.vue';
import API from '@/api/api-main';
import { getURLQuery } from '@/utils/url';
import { fDate } from '../../../../utils/format';
import PDFView from '../../../../components/PDFViewer/PDFView.vue';
import { VuePDF } from '@tato30/vue-pdf';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const pdfViewRef = ref<InstanceType<typeof PDFView>>();
const pdfUrl = ref<string>('');
const visible = ref(false);

const onClickViewDoc = (data: any) => {
    if (!data.filePath) {
        console.error('No PDF path');
        return;
    }

    pdfUrl.value = data.filePath;

    visible.value = true;
};

const termDlgRef = ref<InstanceType<typeof TermDlg>>();
const onClickUpdate = () => {
    termDlgRef.value?.open();
};

const paramQuery = reactive({
    page: 1,
    pageSize: 10,
    orderBy: 'desc id'
});

const status = reactive({
    A: {
        label: t('body.sampleRequest.customer.active_status'),
        severity: 'primary'
    },
    D: {
        label: t('body.sampleRequest.customer.inactive_status'),
        severity: 'danger'
    }
});

const getStatusLabel = (key: string) => {
    return (
        status[key as keyof typeof status] || {
            label: t('body.report.undefined_label'),
            severity: 'secondary'
        }
    );
};

const dataTable = reactive({
    data: [],
    page: 0,
    size: 0,
    total: 0
});

const onPage = (event: any) => {
    paramQuery.page = event.page + 1;
    fetchData();
};

const fetchData = () => {
    API.get(`article?${getURLQuery(paramQuery)}`).then((res) => {

        dataTable.data = res.data.item;
    });
};

const initialComponent = () => {
    // code here
    fetchData();
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
