<template>
    <div>
        <div class="flex mb-4">
            <h4 class="font-bold mb-0">{{ t('body.sampleRequest.paymentSettings.title') }}</h4>
        </div>
        <DataTable
            v-model:editingRows="editingRows"
            @row-edit-save="onRowEditSave"
            editMode="row"
            class="card p-3"
            :value="dataTable.data"
            showGridlines
            :loading="dataTable.loading"
        >
            <Column field="name" :header="t('body.sampleRequest.paymentSettings.title')" />
            <Column
                v-for="(col, i) in columns"
                :field="col.field"
                :header="col.header"
                :key="i"
                class="w-15rem text-right"
            >
                <template #body="{ data, field }"> {{ data[field] }}% </template>
                <template #editor="{ data, field }">
                    <InputNumber
                        :pt="{
                            root: { class: 'w-full' },
                            input: { root: { class: 'w-full text-right' } },
                        }"
                        v-model="data[field]"
                        :min="0"
                        :max="100"
                        :minFractionDigits="2"
                        :maxFractionDigits="5"
                        suffix=" %"
                    />
                </template>
            </Column>
            <Column class="w-8rem">
                <template #body="{ editorInitCallback }">
                    <Button icon="pi pi-pencil" @click="editorInitCallback" text/>
                </template>
                <template #editor="{ editorSaveCallback, editorCancelCallback }">
                    <Button icon="pi pi-check" @click="editorSaveCallback" text/>
                    <Button
                        icon="pi pi-times"
                        @click="editorCancelCallback"
                        text
                        severity="danger"
                    />
                </template>
            </Column>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { AxiosResponse } from "axios";
import { useToast } from "primevue/usetoast";
import { ref, reactive, computed, watch, onMounted } from "vue";
import API from "@/api/api-main";
import { useI18n } from "vue-i18n";

interface Model {
    id: number;
    name: string;
    promotionTax: number;
    bonusPaynow: number;
    bonusVolumn: number;
}
const toast = useToast();
const dataTable = reactive({
    data: [] as Array<Model>,
    loading: true,
    error: null,
    total: 0,
});
const editingRows = ref();

const { t } = useI18n();

const columns = [
    { field: "promotionTax", header: t("body.sampleRequest.paymentSettings.promotionTax") },
    { field: "bonusPaynow", header: t("body.sampleRequest.paymentSettings.instantPaymentBonus") },
    { field: "bonusVolumn", header: t("body.sampleRequest.paymentSettings.volumeBonusTax") },
];

const onRowEditSave = ({ newData }: any) => {
    dataTable.loading = true;
    const payload = newData as Model;
    API.update(`PaymentRule/${payload.id}`, payload)
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.systemSetting.update_account_success_message'),
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
        })
        .finally(() => {
            dataTable.loading = false;
        });
};

const fetchData = () => {
    dataTable.loading = true;
    API.get("PaymentRule?Page=1&PageSize=5")
        .then((res: AxiosResponse<{ items: Array<Model>; total: number }>) => {
            dataTable.data = res.data.items;
            dataTable.total = res.data.total;
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
        })
        .finally(() => {
            dataTable.loading = false;
        });
};

const initialComponent = () => {
    fetchData();
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
