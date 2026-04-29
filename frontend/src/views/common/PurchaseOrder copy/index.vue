<template>
  <div>
    <div
      v-if="!poStore.isClient && !props.isClient"
      class="flex justify-content-between mb-3"
    >
      <h4 class="mb-0 font-bold">Tạo mới đơn hàng</h4>
      <ButtonGoBack />
    </div>
    <div class="grid">
      <div class="col-9">
        <Header v-if="!poStore.isClient && !props.isClient"></Header>
        <TabView
          v-model:activeIndex="poStore.activeIndexTab"
          class="card p-0 overflow-hidden"
          style="min-height: 575px"
        >
          <TabPanel header="Sản phẩm">
            <ProductList :isClient="props.isClient" class="mb-3"></ProductList>
            <PromotionList></PromotionList>
            <div class="field">
              <label for="">Ghi chú</label>
              <Textarea v-model="poStore.model.note" class="w-full" :rows="4" />
            </div>
          </TabPanel>
          <TabPanel :disabled="!poStore.model.cardId" header="Khách hàng">
            <CustomerInfo></CustomerInfo>
          </TabPanel>
        </TabView>
      </div>
      <div class="col-3">
        <div class="grid">
          <div class="col-12 pb-0">
            <div class="flex flex-column formgrid field">
              <label v-if="!props.isClient">&nbsp;</label>
              <KiemTraCongNo
                :bpId="poStore.model._customer?.id"
                :bpName="poStore.model._customer?.cardName"
                :payCredit="poStore.orderSummary.totalDebt"
                :payGuarantee="poStore.orderSummary.totalDebtGuarantee"
              />
            </div>
          </div>
          <div class="col-12 py-0">
            <OrderSummary class="mb-3" :isClient="props.isClient"></OrderSummary>
            <Committed></Committed>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { ItemDetail, PurchaseOrder } from "./types/entities";
// Components
import Header from "./components/Header.vue";
import ProductList from "./components/ProductList.vue";
import PromotionList from "./components/PromotionList.vue";
import OrderSummary from "./components/OrderSummary.vue";
import CustomerInfo from "./components/CustomerInfo.vue";
import Committed from "./components/Committed.vue";
import { isArray } from "lodash";

import { usePoStore } from "./store/purchaseStore.store";
import { useCartStore } from "@/Pinia/cart";

const poStore = usePoStore();
const cartStore = useCartStore();

const props = defineProps({
  isClient: {
    type: Boolean,
    default: false,
  },
});

const initialComponent = async () => {
  poStore.$reset();
  await new Promise((result: any) => setTimeout(() => result(), 10));
  if (props.isClient) {
    await cartStore.getCart();
    if (isArray(cartStore.getItems)) {
      for (const item of cartStore.getItems) {
        poStore.model.itemDetail.push(new ItemDetail(item["item"]));
      }
    }
  }
};

onMounted(async function () {
  await initialComponent();
});
</script>

<style scoped></style>
