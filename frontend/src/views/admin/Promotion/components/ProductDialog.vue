<script setup>
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ProductSelector from '@/components/ProductSelector.vue';

const props = defineProps({
    modelValue: {
        type: Boolean,
        default: false
    },
    isFilterPro: {
        type: Boolean,
        default: false
    },
    itemData: {
        type: Array,
        default: () => []
    },
    itemType: {
        type: Array,
        default: () => []
    },
    selectedItemType: {
        type: Array,
        default: () => []
    }
});

const emit = defineEmits(['update:modelValue', 'confirmProduct', 'confirmItemType', 'updateProduct', 'removeItem']);

const { t } = useI18n();

const localSelectedItemType = ref([]);
watch(
    () => props.selectedItemType,
    (newValue) => {
        localSelectedItemType.value = newValue;
    },
    { deep: true, immediate: true }
);

const closeModal = () => {
    emit('update:modelValue', false);
};

const onConfirmProduct = () => {
    emit('confirmProduct', props.itemData);
};

const onConfirmItemType = () => {
    emit('confirmItemType', localSelectedItemType.value);
};

const onUpdateProduct = (event) => {
    emit('updateProduct', event);
};

const onRemoveItem = (index) => {
    emit('removeItem', index);
};
</script>

<template>
    <Dialog :visible="modelValue" @update:visible="closeModal" modal :header="t('body.sampleRequest.sampleProposal.choose_product_button')" :style="{ width: '45vw' }">
        <TabView :activeIndex="selectedItemType.length ? 1 : 0">
            <TabPanel :header="t('body.home.product_label')">
                <DataTable :value="itemData">
                    <Column header="#">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column field="itemName" :header="t('body.home.product_name_column')"></Column>
                    <Column>
                        <template #body="{ index }">
                            <Button icon="pi pi-trash" text severity="danger" @click="onRemoveItem(index)" />
                        </template>
                    </Column>
                </DataTable>
                <div class="flex justify-content-between w-full mt-5">
                    <div>
                        <ProductSelector icon="pi pi-plus" :label="t('body.PurchaseRequestList.find_product_button')" outlined :isFilterPro="isFilterPro" @confirm="onUpdateProduct($event)" :disabledHeader="true" />
                    </div>
                    <div class="flex justify-content-end gap-2">
                        <Button type="button" :label="t('body.home.cancel_button')" severity="secondary" @click="closeModal" />
                        <Button type="button" :label="t('body.home.confirm_button')" @click="onConfirmProduct" />
                    </div>
                </div>
            </TabPanel>
            <TabPanel :header="t('body.productManagement.product_type')" v-if="isFilterPro">
                <DataTable :value="itemType" v-model:selection="localSelectedItemType" dataKey="itemId" scrollable scrollHeight="400px">
                    <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                    <Column field="itemName" :header="t('body.productManagement.typeName')"></Column>
                </DataTable>
                <div class="flex justify-content-end gap-2 mt-5">
                    <Button type="button" :label="t('body.home.cancel_button')" severity="secondary" @click="closeModal" />
                    <Button type="button" :label="t('body.home.confirm_button')" @click="onConfirmItemType" />
                </div>
            </TabPanel>
        </TabView>
    </Dialog>
</template>
