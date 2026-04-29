<template>
  <div>
    <div
      v-if="!poStore.isClient && !props.isClient"
      class="flex justify-content-between mb-3"
    >
      <h4 class="mb-0 font-bold">{{ t("PromotionalItems.OrderNET.tabAddNew") }}</h4>
      <ButtonGoBack />
    </div>
    <div class="grid">
      <div class="col-9">
        <Header
          v-if="!poStore.isClient && !props.isClient"
          :keepProduct="true"
          @item-select="onSelectCustomer"
          :cardId="poStore.model.cardId"
        />
        <TabView
          v-model:activeIndex="poStore.activeIndexTab"
          class="card p-0 overflow-hidden"
          style="min-height: 575px"
        >
          <TabPanel :header="t('body.OrderList.products_tab')">
            <ProductList :isClient="props.isClient" class="mb-3" />
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
            <OrderSummary class="mb-3" :isClient="props.isClient" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { onBeforeMount, watch, ref } from "vue";
  import Header from "./components/Header.vue";
  import ProductList from "./components/ProductList.vue";
  import PromotionList from "./components/PromotionList.vue";
  import OrderSummary from "./components/OrderSummary.vue";
  import CustomerInfo from "./components/CustomerInfo.vue";
  import { useOrderDetailStore } from "./store/orderDetail";
  import { usePoStore } from "./store/purchaseStore.store";
  import { useRoute } from "vue-router";
  import { useI18n } from "vue-i18n";
  import API from "@/api/api-main";
  import { ItemDetail } from "../../common/PurchaseOrderVPKM/types/entities";
  const { t } = useI18n();
  const poStore = usePoStore();
  const route = useRoute();
  const props = defineProps({
    isClient: {
      type: Boolean,
      default: false,
    },
  });
  const odStore = useOrderDetailStore();
  let itemDetails = ref<ItemDetail[]>([]);
  const onSelectCustomer = () => { 
    poStore.model.itemDetail = itemDetails.value;
    itemDetails.value = []; 
  };
  const getDataDetail = async (id: number) => {
    try {
      const response = await API.get(`PurchaseOrder/${id}`);
      poStore.model.cardCode = response?.data?.item?.cardCode;
      itemDetails.value = response?.data?.item?.itemDetail; 
    } catch (error) {
      console.error(error);
    }
  };

  watch(
    () => odStore,
    (newVal) => {
      if (newVal?.order) Object.assign(poStore.model, newVal.order); 
    },
    { deep: true }
  );

  onBeforeMount(async function () {
    poStore.$reset();
    await getDataDetail(Number(route.params.id));
  });
</script>
