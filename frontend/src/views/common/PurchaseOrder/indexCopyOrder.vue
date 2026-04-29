<template>
  <div>
    <div class="flex justify-content-between mb-3">
      <h4 class="mb-0 font-bold">{{ t("body.OrderList.create_new_order_title") }}</h4>
      <ButtonGoBack />
    </div>
    <div class="grid">
      <div class="col-9">
        <Header
          ref="getCustomers"
          @item-select-done="handleItemSelectDone"
          :isClient="!isClient"
        />
        <TabView
          v-model:activeIndex="poStore.activeIndexTab"
          class="card p-0 overflow-hidden"
          style="min-height: 575px"
        >
          <TabPanel :header="t('body.OrderList.products_tab')">
            <ProductList :isClient="isClient" class="mb-3" />
            <PromotionList />
            <div class="field">
              <label for="">{{ t("body.systemSetting.note_column") }}</label>
              <Textarea v-model="poStore.model.note" class="w-full" :rows="4" />
            </div>
          </TabPanel>
          <TabPanel
            :disabled="!poStore.model.cardId"
            :header="t('body.OrderList.customer_tab')"
          >
            <CustomerInfo />
          </TabPanel>
        </TabView>
      </div>
      <div class="col-3 px-0">
        <div class="grid">
          <div class="col-12 pb-0">
            <div class="flex flex-column formgrid field">
              <label v-if="!isClient">&nbsp;</label>
              <KiemTraCongNo
                :bpId="poStore.model._customer?.id"
                :bpName="poStore.model._customer?.cardName"
                :payCredit="poStore.orderSummary.totalDebt"
                :payGuarantee="poStore.orderSummary.totalDebtGuarantee"
                :currency="poStore.model.currency"
                @update:isExceedDebt="handleExceedDebt"
              />
            </div>
          </div>
          <div class="col-12 py-0">
            <OrderSummary class="mb-3" :isClient="isClient" />
            <Committed />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from "vue";
  import Header from "./components/Header.vue";
  import ProductList from "./components/ProductList.vue";
  import PromotionList from "./components/PromotionList.vue";
  import OrderSummary from "./components/OrderSummary.vue";
  import CustomerInfo from "./components/CustomerInfo.vue";
  import Committed from "./components/Committed.vue";
  import { usePoStore } from "./store/purchaseStore.store";
  import { useI18n } from "vue-i18n";
  import API from "@/api/api-main";
  import { useRoute } from "vue-router";

  const route = useRoute();
  const handleExceedDebt = (val: boolean) => {
    poStore.isExceedDebt = val;
  };
  const { t } = useI18n();
  const poStore = usePoStore();
  const isClient = ref(route.name == "purchase-order-copy-admin" ? false : true);

  const res = ref<any>(null);
  const initialComponent = async () => {
    poStore.$reset();
    res.value = await API.get("PurchaseOrder/" + route.params.id);
    poStore.model.cardCode = res.value.data.item.cardCode;
  };

  const handleItemSelectDone = () => {
    poStore.model.itemDetail = res.value.data.item.itemDetail;
    poStore.model.promotion = res.value.data.item.promotion;
    res.value = []
  };

  const getQueryParams = async () => {
    try {
    } catch (error) {
      console.error("Lỗi khi truyền data vào poStore:", error);
    }
  };

  onMounted(async function () {
    await initialComponent();
    getQueryParams();
  });
</script>
