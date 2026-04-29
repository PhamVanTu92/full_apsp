<template>
    <div class="grid">
        <div class="flex flex-column formgrid col-6">
            <label for="header">{{ t('body.OrderList.customer_label') }}</label>
            <CustomerSelector @item-select="onSelectCustomer" @clear="onClearCustomer" :poStore="poStore" :filted="true"
                :placeholder="t('body.OrderList.select_customer_placeholder')"></CustomerSelector>
        </div>
        <div class="col-3 formgrid">
            <label for="header">{{ t('body.OrderList.order_date_label') }}</label>
            <div class="py-0">
                <Calendar v-model="poStore.model.deliveryTime" showTime hourFormat="24" />
            </div>
        </div>
        <div class="col-3 formgrid text-center" v-if="poStore?.model?.cardName">
            <span>{{ t('PromotionalItems.PointConversion.currentPoints') }}</span><br>
            <b style="font-size: 30px;color: red;"> {{ formatNumber(pointUser) +
                t('PromotionalItems.PromotionalItems.point') }}</b>
        </div>
    </div>
</template>

<script setup lang="ts">
import { PurchaseOrder, Customer } from '../types/entities';
import { usePoStore } from '../store/purchaseStore.store';
import { useI18n } from 'vue-i18n';
import { ref, } from "vue";
const { t } = useI18n();
const poStore = usePoStore();
const onClearCustomer = () => {
    poStore.$reset();
};
const pointUser = ref()
const onSelectCustomer = (event: Customer) => {
    pointUser.value = event.customerPoints.reduce((sum, item) => {
        const dateToCheck = item.expiryDate ? new Date(item.expiryDate) : new Date(item.endDate);
        const isValid = dateToCheck > new Date();
        const pointsToAdd = isValid ? item.remainingPoint : 0;
        return sum + pointsToAdd;
    }, 0);
    poStore.model = new PurchaseOrder(new Customer(event));
    // poStore.fetchCheckPriceMethod();
};
const formatNumber = (num: number) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};

defineExpose({
    onSelectCustomer
});
</script>
<style scoped>
.formgrid {
    line-height: 2rem;
}

.formgrid b {
    color: red;
}
</style>
