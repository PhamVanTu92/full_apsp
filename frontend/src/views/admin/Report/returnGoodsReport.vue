<template>
    <div>
        <div class="flex justify-content-between align-content-center mb-3">
            <h4 class="font-bold m-0">{{ t('ReturnedGoods.title') }}</h4>
            <ButtonGoBack />
        </div>
        <div class="flex align-items-center gap-3">
            <div>{{ t('body.report.customer_label_1') }}</div>
            <CustomerSelector selectionMode="multiple" :placeholder="t('body.report.area_placeholder')" />
            <span class="font-bold">{{ t('body.home.time_label') }} : </span>
            <span class="flex gap-3">
                <Calendar v-model="dateFilter.startDate" class="w-3"
                    :placeholder="t('body.report.from_date_placeholder')" formatDate="dd/mm/yyyy"
                    :maxDate="dateFilter.endDate" @date-select="onDateSelected"></Calendar>
                <Calendar v-model="dateFilter.endDate" class="w-3" :placeholder="t('body.report.to_date_placeholder')"
                    formatDate="dd/mm/yyyy" :minDate="dateFilter.startDate" @date-select="onDateSelected"></Calendar>
                <Button v-if="0" :label="t('body.report.apply_button_2')" @click="onClickGetData"/>
            </span>
        </div>
        <hr />
        <div class="card p-3">
            <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem" stripedRows
                v-model:expandedRows="expandedRows" dataKey="id">
                <Column expander style="width: 3rem" />
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column :header="t('ReturnedGoods.sapCode')" />
                <Column :header="t('ReturnedGoods.orderCode')" />
                <Column :header="t('ReturnedGoods.buyDate')" />
                <Column :header="t('ReturnedGoods.returnDate')" />
                <Column :header="t('client.note')" />

                <template #expansion="">
                    <div class="p-4 surface-100">
                        <DataTable :value="[{}, {}, {}]" showGridlines>
                            <Column header="#" style="width: 3rem">
                                <template #body="{ index }">{{ index + 1 }}</template>
                            </Column>
                            <Column :header="t('body.report.table_header_product_code_2')" />
                            <Column :header="t('body.report.table_header_product_name_2')" />
                            <Column :header="t('client.unit')" />
                            <Column :header="t('client.quantity')" />
                            <Column :header="t('client.note')" style="width: 10rem" />
                        </DataTable>
                    </div>
                </template>

                <template #header>
                    <div class="flex justify-content-between align-items-center">
                        <span class="text-xl" />
                        <div class="flex gap-3">
                            <InputText :placeholder="t('body.report.search_placeholder_2')"></InputText>
                            <Button icon="pi pi-file-export" outlined severity="info"
                                :label="t('body.report.export_excel_button_2')"/>
                        </div>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const timeNow = new Date();
const dateFilter = reactive({
    startDate: new Date(`${timeNow.getFullYear()}/01/01`),
    endDate: timeNow,
});

const expandedRows = ref([]);

const onDateSelected = () => {
    // Handle date selection logic here

};
</script>
