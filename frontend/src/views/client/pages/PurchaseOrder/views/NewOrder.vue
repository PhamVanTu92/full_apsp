<template>
    <PurchaseOrderComponent :isClient="true" />
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import PurchaseOrderComponent from "../../../../common/PurchaseOrder/index.vue";
import { usePoStore } from "../../../../common/PurchaseOrder/store/purchaseStore.store";
import { useMeStore } from "../../../../../Pinia/me";
import { Customer, PurchaseOrder } from "@/views/common/PurchaseOrder/types/entities"; 
const poStore = usePoStore();
const meStore = useMeStore(); 
onMounted(async function () { 
    await meStore.getMe();
    poStore.isClient = true;
    const customer = meStore?.me?.user?.bpInfo;
    if (customer) {
        poStore.model = new PurchaseOrder(new Customer(customer));
        poStore.fetchCheckPriceMethod();
    }
});
</script> 
