<template>
    <div>
        <Dialog v-model:visible="visible" :header="t('body.report.report_title_volume_commitment')" modal style="width: 90rem">
            <DataTable
                :value="Array.from({ length: 10 })"
                show-gridlines
                resizableColumns
                columnResizeMode="fit"
            >
                <Column :header="t('body.report.table_header_stt_3')">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_order_code')"></Column>
                <Column :header="t('body.report.table_header_order_date')"></Column>
                <Column :header="t('body.report.order_volume_column')"></Column>
                <Column :header="t('body.report.accumulated_volume_column')"></Column>
                <Column :header="t('body.report.commitment_discount_volume_column')"></Column>
                <Column :header="t('body.report.commitment_discount_amount_column')"></Column>
                <Column :header="t('body.report.bonus_amount_column')"></Column>
                <Column :header="t('body.report.bonus_type_column')"></Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column :footer="t('body.home.total')" :colspan="3"></Column>
                        <Column></Column>
                        <Column></Column>
                        <Column></Column>
                        <Column></Column>
                        <Column></Column>
                        <Column></Column>
                    </Row>
                </ColumnGroup>
            </DataTable>
            <template #footer>
                <Button
                    @click="visible = false"
                    :label="t('body.OrderList.close')"
                    severity="secondary"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const visible = ref(false);

const open = (data: any) => {
    visible.value = true;
};

defineExpose({
    open,
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
