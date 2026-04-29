<template>
    <DataTable :value="odStore.order?.itemDetail" resizableColumns columnResizeMode="fit" show-gridlines scrollable
        scroll-height="600px">
        <Column field="" header="#">
            <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column field="itemName" :header="t('ChangePoint.nameProduct')"></Column>
        <Column field="price" :header="t('ChangePoint.point')" class="text-right">
            <template #body="{ data, field }">
                {{ fnum(data[field]) }}
            </template>
        </Column>
        <Column field="quantity" :header="t('client.quantity')" class="text-right">
            <template #body="{ data, field }">
                {{ fnumInt(data[field]) }}
            </template>
        </Column>
        <Column :header="t('ChangePoint.totalPoint')" class="text-right">
            <template #body="{ data }">
                {{ fnum(data.price * data.quantity) }}
            </template>
        </Column>
        <ColumnGroup type="footer">
            <Row>
                <Column :footer="t('body.home.total')" :colspan="3" /> 
                <Column :footer="fnumInt(sumQuantity || 0)" class="text-right" />
                <Column :footer="fnum(volumn || 0)" class="text-right" />
            </Row>
        </ColumnGroup>
    </DataTable>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { fnum, fnumInt } from "../../PurchaseOrder/script";
import { useOrderDetailStore } from "../store/orderDetail";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const odStore = useOrderDetailStore();
const volumn = computed(() => {
    return odStore.order?.itemDetail.reduce(
        (sum, item) => item.price * item.quantity + sum,
        0
    );
});

const sumQuantity = computed(() => {
    return odStore.order?.itemDetail.reduce(
        (sum, item) => item.quantity + sum,
        0
    );
});
</script>
