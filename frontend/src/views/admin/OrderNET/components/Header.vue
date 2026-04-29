<template>
    <div class="grid">
        <div class="col-2 pr-0 hidden">
            <div class="flex flex-column formgrid field">
                <label for="">{{ t('body.OrderList.orderCode') }}</label>
                <InputText disabled />
            </div>
        </div>
        <div class="col-12 md:col-7 pr-0">
            <div class="flex flex-column formgrid field">
                <label for="">{{ t('body.OrderList.customer_label') }}</label>
                <CustomerSelector @item-select="onSelectCustomer" @clear="onClearCustomer" :filted="true"
                    :cardId="props.cardId" :poStore="poStore"
                    :placeholder="t('body.OrderList.select_customer_placeholder')" />
            </div>
        </div>
        <div class="col-12 md:col-5">
            <div class="grid">
                <div class="col-12 pb-0 field">
                    <label class="" for="">{{ t('body.OrderList.order_date_label') }}</label>
                </div>
                <div class="col-5 py-0">
                    <div class="flex flex-column formgrid field">
                        <Calendar v-model="poStore.model.deliveryTime" showTime hourFormat="24" />
                    </div>
                </div>
                <div class="col py-0 pl-0">
                    <div class="flex flex-column formgrid field mb-0">
                        <div class="flex align-items-center gap-3">
                            <div class="flex gap-2">
                                <Checkbox v-model="poStore.model.isIncoterm" binary readonly />
                                <div>{{ t('body.sampleRequest.customer.incoterm') }} 2020</div>
                            </div>
                            <div class="flex-grow-1">
                                <Dropdown v-model="poStore.model.currency" :options="currencyOption" class="w-full" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { Customer, PurchaseOrder } from "../types/entities";
import { usePoStore } from "../store/purchaseStore.store";
import { useI18n } from 'vue-i18n'; 
const emits = defineEmits(['item-select',]);
const { t } = useI18n();
const poStore = usePoStore();
const currencyOption = ["VND", "USD"];
const onClearCustomer = () => {
    poStore.$reset();
};

const props = defineProps({
    keepProduct: {
        type: Boolean,
        default: false
    },
    cardId: {
        type: Number,
        default: null
    }
});
const onSelectCustomer = (event: Customer) => {
    poStore.model = new PurchaseOrder(new Customer(event));
    emits('item-select', true);
};
const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.field {
    margin-bottom: 0;
}
</style>
