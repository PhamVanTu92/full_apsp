<template>
    <div>
        <div class="flex justify-content-between align-content-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.report.report_title_average_price') }}</h4>
            <ButtonGoBack />
        </div>
        <div class="flex align-items-center gap-3">
            <span class="font-bold">{{ t('body.home.time_label') }}</span>
            <span class="flex gap-3">
                <Calendar
                    v-model="dateFilter.startDate"
                    class="w-3"
                    :placeholder="t('body.report.from_date_placeholder')"
                    formatDate="dd/mm/yyyy"
                    :maxDate="dateFilter.endDate"
                    @date-select="onDateSelected"
                ></Calendar>
                <Calendar
                    v-model="dateFilter.endDate"
                    class="w-3"
                    :placeholder="t('body.report.to_date_placeholder')"
                    formatDate="dd/mm/yyyy"
                    :minDate="dateFilter.startDate"
                    @date-select="onDateSelected"
                ></Calendar>
                <Button v-if="0" :label="t('body.report.apply_button_2')" @click="onClickGetData"/>
            </span>
        </div>

        <hr />
        <div class="card p-3">
            <DataTable
                :value="[{}, {}, {}]"
                showGridlines
                tableStyle="min-width: 50rem"
                selectionMode="single"
                stripedRows=""
                @row-click="onRowClick"
            >
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column :header="t('body.report.table_header_product_code_2')" style="width: 8rem"></Column>
                <Column :header="t('body.report.table_header_product_name_2')" style="width: 10rem"></Column>
                <Column :header="t('body.report.table_header_brand_2')" style="width: 10rem"></Column>
                <Column :header="t('body.report.table_header_category_2')" style="width: 10rem"></Column>
                <Column :header="t('body.report.table_header_product_type_2')" style="width: 10rem"></Column>
                <Column :header="t('body.report.table_header_packaging_2')" style="width: 10rem"></Column>
                <Column :header="t('body.report.table_header_average_price')" style="width: 10rem"></Column>
                <template #header>
                    <div class="flex justify-content-between align-items-center">
                        <span class="text-xl">{{ t('body.report.purchased_product_list_title_2') }}</span>
                        <div class="flex gap-3">
                            <InputText :placeholder="t('body.report.search_placeholder_2')"></InputText>
                            <Button
                                icon="pi pi-file-export"
                                outlined
                                severity="info"
                                :label="t('body.report.export_excel_button_2')"
                            />
                        </div>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>

    <!-- Chi tiết từng báo cáo  -->
    <Dialog
        v-model:visible="visibleDetail"
        :header="t('body.report.report_title_average_price')"
        modal
    >
        <div class="card p-3">
            <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem">
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column :header="t('body.report.table_header_order_date')" style="width: 10rem"></Column>
                <Column :header="t('body.report.unit_price_column') + ' (VND)'" style="width: 12rem"></Column>
                <Column :header="t('body.report.unit_price_column') + ' (VND)'" style="width: 10rem"></Column>
                <Column header="Chênh lệch (VND)" style="width: 10rem"></Column>
            </DataTable>
        </div>
    </Dialog>
</template>

<script setup>
import { ref, reactive } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const timeNow = new Date();
const dateFilter = reactive({
    from: new Date(`${timeNow.getFullYear()}/01/01`),
    to: timeNow,
});
const visibleDetail = ref(false);

const goBack = () => {
    window.history.back();
};
const onRowClick = () => {
    visibleDetail.value = true;
};
</script>
