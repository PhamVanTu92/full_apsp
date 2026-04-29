<template>
    <div>
        <div class="flex justify-content-between mb-3">
            <h4 class="font-bold mb-0">{{ t('body.report.report_title_volume_commitment') }}</h4>
            <ButtonGoBack />
        </div>
        <div class="flex gap-5">
            <div class="flex align-items-center gap-3">
                <span>{{ t('body.report.customer_label_1') }}</span>
                <CustomerSelector></CustomerSelector>
            </div>
            <div class="flex align-items-center gap-3">
                <span>{{ t('body.report.year_label') }}</span>
                <Calendar dateFormat="yyyy" view="year" class="w-14rem"></Calendar>
            </div>
        </div>
        <hr />
        <DataTable
            :value="Array.from({ length: 10 })"
            showGridlines
            stripedRows
            class="mb-4"
            resizableColumns
            column-resize-mode="fit"
            selection-mode="single"
            @row-click="onRowClick"
        >
            <template #header>
                <div class="flex align-items-center gap-2">
                    <span>{{ t('body.report.application_method_quarterly') }}</span>
                </div>
            </template>
            <ColumnGroup type="header">
                <Row>
                    <Column :header="t('body.report.table_header_stt_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_category_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_brand_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_product_type_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_index')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_q1')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_q2')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_q3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_q4')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_6_months')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_9_months')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_full_year')" :rowspan="2"> </Column>
                    <Column
                        :header="t('body.report.table_header_exceeding_volume_bonus')"
                        :colspan="2"
                        class="border-bottom-none"
                    >
                    </Column>
                </Row>
                <Row>
                    <Column :header="t('body.report.table_header_milestone_1')"> </Column>
                    <Column :header="t('body.report.table_header_milestone_2')"> </Column>
                </Row>
            </ColumnGroup>
            <Column :header="t('body.report.table_header_stt_3')">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column :header="t('body.report.table_header_category_3')"> </Column>
            <Column :header="t('body.report.table_header_brand_3')"> </Column>
            <Column :header="t('body.report.table_header_product_type_3')"> </Column>
            <Column :header="t('body.report.table_header_index')"> </Column>
            <Column :header="t('body.report.table_header_q1')"> </Column>
            <Column :header="t('body.report.table_header_q2')"> </Column>
            <Column :header="t('body.report.table_header_q3')"> </Column>
            <Column :header="t('body.report.table_header_q4')"> </Column>
            <Column :header="t('body.report.table_header_6_months')"> </Column>
            <Column :header="t('body.report.table_header_9_months')"> </Column>
            <Column :header="t('body.report.table_header_full_year')"> </Column>
            <Column :header="t('body.report.table_header_milestone_1')"> </Column>
            <Column :header="t('body.report.table_header_milestone_2')"> </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :colspan="4" :footer="t('body.home.total')"> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                </Row>
            </ColumnGroup>
        </DataTable>
        <DataTable
            :value="Array.from({ length: 10 })"
            showGridlines
            stripedRows
            class="mb-3"
            resizableColumns
            column-resize-mode="fit"
            selection-mode="single"
            @row-click="onRowClick"
        >
            <template #header>
                <div class="flex align-items-center gap-2">
                    <span>{{ t('body.report.application_method_yearly') }}</span>
                </div>
            </template>
            <ColumnGroup type="header">
                <Row>
                    <Column :header="t('body.report.table_header_stt_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_category_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_brand_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_product_type_3')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_index')" :rowspan="2"> </Column>
                    <template v-for="(monthKey, idx) in monthKeys" :key="idx">
                        <Column :header="t(monthKey)" :rowspan="2"></Column>
                    </template>
                    <Column :header="t('body.report.table_header_3_months')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_6_months')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_9_months')" :rowspan="2"> </Column>
                    <Column :header="t('body.report.table_header_full_year')" :rowspan="2"> </Column>
                    <Column
                        :header="t('body.report.table_header_exceeding_volume_bonus')"
                        :colspan="2"
                        class="border-bottom-none"
                    >
                    </Column>
                </Row>
                <Row>
                    <Column :header="t('body.report.table_header_milestone_1')"> </Column>
                    <Column :header="t('body.report.table_header_milestone_2')"> </Column>
                </Row>
            </ColumnGroup>
            <Column :header="t('body.report.table_header_stt_3')">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column :header="t('body.report.table_header_category_3')"> </Column>
            <Column :header="t('body.report.table_header_brand_3')"> </Column>
            <Column :header="t('body.report.table_header_product_type_3')"> </Column>
            <template v-for="(monthKey, idx) in monthKeys" :key="idx">
                <Column :header="t(monthKey)"></Column>
            </template>
            <Column :header="t('body.report.table_header_3_months')"> </Column>
            <Column :header="t('body.report.table_header_6_months')"> </Column>
            <Column :header="t('body.report.table_header_9_months')"> </Column>
            <Column :header="t('body.report.table_header_full_year')"> </Column>
            <Column :header="t('body.report.table_header_milestone_1')"> </Column>
            <Column :header="t('body.report.table_header_milestone_2')"> </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :colspan="4" :footer="t('body.home.total')"> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                    <Column> </Column>
                </Row>
            </ColumnGroup>
        </DataTable>
        <DetailDialog ref="detailDialogRef"></DetailDialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import DetailDialog from "./dialog/DetailDialog.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const detailDialogRef = ref<InstanceType<typeof DetailDialog>>();

const monthKeys = [
    'body.report.table_header_month_1',
    'body.report.table_header_month_2',
    'body.report.table_header_month_3',
    'body.report.table_header_month_4',
    'body.report.table_header_month_5',
    'body.report.table_header_month_6',
    'body.report.table_header_month_7',
    'body.report.table_header_month_8',
    'body.report.table_header_month_9',
    'body.report.table_header_month_10',
    'body.report.table_header_month_11',
    'body.report.table_header_month_12'
];

const onRowClick = () => {
    detailDialogRef.value?.open("");
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
