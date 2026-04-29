<template>
    <div>
        <Button @click="openDetail()" text :label="t('body.OrderList.viewDetails')"/>

        <Dialog
            v-model:visible="visible"
            modal
            :header="t('client.product_detail')"
            :style="{ width: '100rem' }"
        >
            <DataTable :value="props.data" tableStyle="min-width: 50rem" showGridlines>
                <template #empty>
                    <div class="text-center p-2">{{t('client.please_select_product')}}</div>
                </template>
                <Column header="STT">
                    <template #body="sp">
                        {{ sp.index + 1 }}
                    </template>
                </Column>
                <Column field="itemName" :header="t('client.productName')"></Column>
                <Column field="ougp.ouom.uomName" :header="t('client.unit')" ></Column>
                <Column field="quantity" :header="t('client.quantity')"></Column>
                <Column :header="t('client.output_volume')" style="min-width: 200px">
                    <template #body="sp">
                        {{ formatNumber(sp.data.packing.volumn * sp.data.quantity) }}
                    </template>
                </Column>
                <Column
                    :header="t('client.bonus_volume_liters')"
                    style="min-width: 250px"
                ></Column>
                <Column
                    :header="t('client.discount_on_volume')"
                    style="min-width: 250px"
                ></Column>
            </DataTable>
            <template #footer>
                <div class="flex justify-content-end gap-2">
                    <Button
                        type="button"
                        :label="t('client.cancel')"
                        severity="secondary"
                        @click="visible = false"
                    />
                </div>
            </template>
        </Dialog>
    </div>
</template>
<script setup>
import { ref } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps(["data"]);
const visible = ref(false);

const openDetail = () => {
    visible.value = true;
};

const getVolumn = (data) => {
    if (!data.length) return 0;
    const total = data.reduce((sum, item) => {
        return sum + item.quantity * item.packing.volumn;
    }, 0);

    return total;
};
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};
</script>
<style></style>
