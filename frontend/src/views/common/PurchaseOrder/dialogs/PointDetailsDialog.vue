<template>
    <Dialog v-model:visible="visible" modal :header="t('ChangePoint.detail_point_promoion')" :style="{ width: '80rem' }" 
            :closable="true":resizable="false">
        <DataTable :value="pointDetails" showGridlines stripedRows scrollable scrollHeight="400px">
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="itemName" :header="t('body.home.product_name_column')" style="min-width: 20rem">
                <template #body="{ data }">
                    {{ data.itemName || data.productName }}
                </template>
            </Column>
            <Column field="quantity" :header="t('body.OrderList.quantity_column')" style="width: 8rem">
                <template #body="{ data }">
                    {{ formatNumber(data.quantity || 0) }}
                </template>
            </Column>
            <Column field="points" :header="t('PromotionalItems.PointConversion.point1dv')" style="width: 10rem">
                <template #body="{ data }">
                    {{ formatNumber(data.points || 0) }}
                </template>
            </Column>
            <Column field="totalPoint" :header="t('PromotionalItems.PointConversion.totalPoints')" style="width: 10rem">
                <template #body="{ data }">
                    <strong class="text-green-600">{{ formatNumber(data.totalPoint || 0) }}</strong>
                </template>
            </Column>
            <Column field="pointSetupName" :header="t('body.promotion.promotion_name_label')" style="min-width: 15rem"/>
        </DataTable>
        
        <template #footer>
            <div class="flex justify-content-between align-items-center">  
                <div class="flex align-items-center gap-3">
                    <strong class="text-lg">
                        {{ t('PromotionalItems.PointConversion.totalPoints') }}: 
                        <span class="text-green-600">{{ formatNumber(totalPoints) }}</span>
                        {{ t('PromotionalItems.PromotionalItems.point') }}
                    </strong>
                    <Button :label="t('client.cancel')" icon="pi pi-times" 
                            @click="close" severity="secondary" />
                </div>
            </div>
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const visible = ref(false);
const pointDetails = ref<any[]>([]);

const totalPoints = computed(() => {
    return pointDetails.value.reduce((sum, item) => sum + (item.totalPoint || 0), 0);
});

const formatNumber = (num: number) => {
    if (isNaN(num)) return '0';
    return new Intl.NumberFormat().format(num);
};

const open = (data: any[]) => {
    pointDetails.value = data;
    visible.value = true;
};

const close = () => {
    visible.value = false;
};

defineExpose({
    open,
    close
});
</script>

<style scoped>
:deep(.p-dialog-header) {
    background: linear-gradient(90deg, #10b981, #059669);
    color: white;
}

:deep(.p-dialog-header .p-dialog-title) {
    font-weight: bold;
}

:deep(.p-datatable-header) {
    background: #f8fafc;
    font-weight: 600;
}
</style>
