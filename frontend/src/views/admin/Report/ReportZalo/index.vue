<template>
    <div class="flex justify-content-between align-content-center mb-3">
        <h4 class="font-bold m-0">{{ t('body.report.title_report_zalo') }}</h4>
        <div class="flex gap-2">
            <ButtonGoBack />
        </div>
    </div>
    <div class="card">
        <DataTable :value="dataTable" dataKey="cardCode" paginator :rowsPerPageOptions="[5, 10, 20, 50]" lazy
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :rows="params.pageSize" :page="params.page" :totalRecords="params.total"
            :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('Notification.title')}`">
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="invoiceCode" :header="t('body.report.invoice_code')">
                <template #body="{ data }">
                    <RouterLink
                        :to="data.objType == 22 ? `/purchase-order/${data.docId}` : (data.objType == 12 ? `/promotional-request/${data.docId}` : 'purchase-request/detail/' + data.docId)">
                        <span style="color:green">
                            {{ data.invoiceCode }}
                        </span>
                    </RouterLink>
                </template>
            </Column>
            <Column :header="t('body.report.notification_confirm')" field="zaloMess">
                <template #body="slotProps">
                    <div v-if="slotProps.data.zaloMess">
                        {{ slotProps.data.zaloMess }}
                    </div>
                </template>
            </Column>
            <Column header="" field="zaloMess">
                <template #body="slotProps">
                    <div v-if="slotProps.data.zaloMess">
                        <Button :label="t('Login.buttons.sendBack')" severity="info" outlined
                            @click="sendMessageZalo(slotProps.data.docId, slotProps.data.typeMess)" />
                    </div>
                </template>
            </Column>
            <Column :header="t('body.report.notification_complete')" field="zaloMess">
                <template #body="slotProps">
                    <span v-if="slotProps.data.zaloMess1">
                        {{ slotProps.data.zaloMess1 }}
                    </span>
                </template>
            </Column>
            <Column header="" field="zaloMess">
                <template #body="slotProps">
                    <span v-if="slotProps.data.zaloMess1">
                        <Button :label="t('Login.buttons.sendBack')" severity="info" outlined
                            @click="sendMessageZalo(slotProps.data.docId, slotProps.data.typeMess1)" />
                    </span>
                </template>
            </Column>
            <template #empty>
                <div class="py-5 my-5 text-center text-500 font-italic">
                    {{ t('body.report.no_data_to_display_message_1') }}
                </div>
            </template>
        </DataTable>
        <Loading v-if="loading.global" />
    </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue';
import API from '@/api/api-main';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';

const toast = useToast()
const { t } = useI18n();
const dataTable = ref([]);

const loading = reactive({
    global: false,
    export: false
});

const params = reactive({
    page: 1,
    pageSize: 10,
    total: 0,
});

const fetchData = async () => {
    loading.global = true;
    try {
        const res = await API.get(`Report/zalo?page=${params.page}&pageSize=${params.pageSize}`);
        dataTable.value = res.data.zalo;
        params.total = res.data.total;
    } catch (error) {
        console.error('Error fetching report data:', error);
    } finally {
        loading.global = false;
    }

};

const sendMessageZalo = async (docId: number, typeMess: string) => {
    try {
        await API.add('Zalo?DocId=' + docId + '&TypeMess=' + typeMess);
        toast.add({ severity: 'success', summary: t('Custom.titleMessageInfo'), detail: t('body.report.send_message_complete'), life: 3000 });
        await fetchData();
    } catch (error) {
        console.error('Error sending Zalo message:', error);
        toast.add({ severity: 'error', summary: t('Custom.titleMessageInfo'), detail: t('body.report.send_message_failed'), life: 3000 });
    }
};

onMounted(() => {
    fetchData();
});
</script>

<style scoped lang="css">
small {
    color: var(--red-500);
}

/* Style cho expand row */
:deep(.p-datatable-row-expansion) {
    background-color: var(--surface-50);
}

:deep(.p-datatable-row-expansion > td) {
    padding: 0 !important;
}

:deep(.p-datatable-row-expansion .p-3) {
    background-color: var(--surface-0);
}
</style>
