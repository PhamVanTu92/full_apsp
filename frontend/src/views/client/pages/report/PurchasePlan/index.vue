<template>
    <div>
        <div class="mb-3 flex justify-content-center">
            <h4 class="font-bold mb-0">{{ t('body.importPlan.title') }}</h4>
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-semibold">{{ t('body.report.year_label') }}</span>
            <Calendar
                v-model="selected.time"
                :placeholder="t('body.report.from_date_placeholder')"
                dateFormat="yy"
                view="year"
            ></Calendar>
            <span class="font-semibold">{{ t('body.importPlan.plan_name_label') }}</span>
            <Dropdown
                v-model="selected.plan"
                class="w-20rem"
                :options="data.plans"
                optionLabel="hddtCode"
            >
            </Dropdown>
        </div>
        <hr />
        <DataTable
            class="card p-3"
            :value="[]"
            showGridlines
            stripedRows
            resizableColumns
            columnResizeMode="fit"
        >
            <ColumnGroup type="header">
                <Row>
                    <Column header="#" :rowspan="2" />
                    <Column :header="t('body.report.table_header_product_code_1')" :rowspan="2" />
                    <Column :header="t('body.report.table_header_product_name_1')" :rowspan="2" />
                    <Column :header="t('body.report.table_header_packaging_1')" :rowspan="2" />
                    <Column :header="t('body.report.table_header_brand_1')" :rowspan="2" />
                    <Column :header="t('body.report.table_header_category_3')" :rowspan="2" />
                    <Column :header="t('body.report.table_header_product_type_1')" :rowspan="2" />
                    <Column :header="`${t('body.report.table_header_month_1')} ${new Date(selected.time).getFullYear()}`" :colspan="3" />
                    <Column :header="`${t('body.report.table_header_month_2')} ${new Date(selected.time).getFullYear()}`" :colspan="3" />
                    <Column :header="`${t('body.report.table_header_month_3')} ${new Date(selected.time).getFullYear()}`" :colspan="3" />
                </Row>
                <Row>
                    <template v-for="i in [1, 2, 3]" :key="i">
                        <Column :header="t('body.home.quantity')" />
                        <Column :header="t('body.report.table_header_total_orders')" />
                        <Column header="Tỷ lệ chính xác" />
                    </template>
                </Row>
            </ColumnGroup>
            <Column
                v-for="i in [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]"
                :key="i"
            />
            <template #empty>
                <div class="py-5 my-5 text-center">{{ t('client.no_data') }}</div>
            </template>
        </DataTable>
    </div>
</template>
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const selected = reactive({
    time: new Date(),
    plan: null,
});
const data = reactive({
    plans: [
        { date: new Date(), billNo: "HD001", billValue: 1000000, hddtCode: "KH001" },
        { date: new Date(), billNo: "HD002", billValue: 2000000, hddtCode: "KH002" },
        { date: new Date(), billNo: "HD003", billValue: 3000000, hddtCode: "KH003" },
    ],
});

onMounted(function () {});
</script>

<style scoped></style>
