<template>
    <div>  
        <Dialog v-model:visible="visible" style="width: 70vw; max-width: 100rem !important" modal :header="t('common.volume_info')" @hide="initialComponent">
            <DataTable :value="poStore.model.itemDetail">
                <Column :header="t('common.stt')">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column>
                <Column field="itemName" :header="t('common.product_name')" />
                <Column field="uomName" :header="t('common.unit')" />
                <Column field="quantity" :header="t('common.quantity')" class="text-right" />
                <Column field="_volumn" :header="t('common.volume_liters')" class="text-right">
                    <template #body="{ data }">
                        {{ fnum(data._volumn) }}
                    </template>
                </Column>
                <Column :header="t('common.reward_volume_liters')" class="text-right">
                    <template #body="{ data }">
                        {{ fnum(data._volumn * data.quantity) }}
                    </template>
                </Column>
                <Column :header="t('common.discount_volume_eligible')" />
                <ColumnGroup type="footer">
                    <Row>
                        <Column :footer="t('common.total')" :colspan="4" />
                        <Column :footer="volumns" class="text-right" />
                        <Column :footer="reachVolumns" class="text-right" />
                        <Column footer="-" class="text-right" />
                    </Row>
                </ColumnGroup>
            </DataTable>
            <template #footer>
                <Button @click="visible = false" :label="t('common.close')" severity="secondary" />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { usePoStore } from '../store/purchaseStore.store';
import { fnum } from '../script';
import { useI18n } from 'vue-i18n';

const visible = ref(false);
const poStore = usePoStore();
const { t } = useI18n(); // Destructure t from useI18n

const volumns = computed(() => {
    return poStore.model.itemDetail.reduce((sum, item) => (sum += item._volumn), 0).toLocaleString(undefined, { style: 'decimal' });
});
const reachVolumns = computed(() => {
    return poStore.model.itemDetail.reduce((sum, item) => (sum += item._volumn * item.quantity), 0).toLocaleString(undefined, { style: 'decimal' });
});

const open = () => {
    visible.value = true;
};

defineExpose({
    open
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>