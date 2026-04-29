<template>
  <div>
    <div
      v-if="!poStore.isClient && !props.isClient"
      class="flex justify-content-between mb-3"
    >
      <h4 class="mb-0 font-bold">
        {{ t("PromotionalItems.PromotionalItems.tabAddNew") }}
      </h4>
      <ButtonGoBack />
    </div>
    <div class="grid">
      <div class="col-9">
        <Header
          v-if="!poStore.isClient && !props.isClient"
          @item-select="onSelectCustomer"
        ></Header>
        <TabView
          v-model:activeIndex="poStore.activeIndexTab"
          class="card p-0 overflow-hidden"
          style="min-height: 575px"
        >
          <TabPanel :header="t('body.OrderList.products_tab')">
            <ProductList
              :isClient="props.isClient"
              class="mb-3"
              :disabledRow="disabledRow"
              :changeDiscountValue="100"
              andPointApi="Item/ItemPromotion"
            ></ProductList>
            <PromotionList></PromotionList>
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
            <OrderSummaryVPKM class="mb-3" :isClient="props.isClient"></OrderSummaryVPKM>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from "vue";
  import { ItemDetail } from "../../common/PurchaseOrderVPKM/types/entities";
  import Header from "../../common/PurchaseOrderVPKM/components/Header.vue";
  import ProductList from "../../common/PurchaseOrderVPKM/components/ProductList.vue";
  import PromotionList from "../../common/PurchaseOrderVPKM/components/PromotionList.vue";
  import OrderSummaryVPKM from "../../common/PurchaseOrderVPKM/components/OrderSummaryVPKM.vue";
  import CustomerInfo from "../../common/PurchaseOrderVPKM/components/CustomerInfo.vue";
  import { isArray } from "lodash";
  import { usePoStore } from "../../common/PurchaseOrderVPKM/store/purchaseStore.store";
  import { useCartStore } from "@/Pinia/cart";
  import { useRoute } from "vue-router";
  import { useI18n } from "vue-i18n";
  import API from "@/api/api-main";
  const { t } = useI18n();
  const poStore = usePoStore();
  const cartStore = useCartStore();
  const route = useRoute();
  const orderData = ref();
  const props = defineProps({
    isClient: {
      type: Boolean,
      default: false,
    },
  });

  const disabledRow = ref({
    activeInput: true,
    methodPayment: false,
    taxPrice: false,
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

  let itemDetails = ref<ItemDetail[]>([]);
  const onSelectCustomer = () => {
    poStore.model.itemDetail = itemDetails.value;
  };
  const getDataDetail = async (id: number) => {
    try {
      const response = await API.get(`PurchaseOrder/${id}`);
      poStore.model.cardCode = response?.data?.item?.cardCode;
      itemDetails.value = response?.data?.item?.itemDetail;
      // poStore.model.itemDetail = response?.data?.item?.itemDetail;
    } catch (error) {
      console.error(error);
    }
  };

  onMounted(async function () {
    if (route.query.invoice && typeof route.query.invoice === "string") {
      try {
        let str = route.query.invoice;
        let data = atob(str);
        let decodedData = decodeURIComponent(data);
        orderData.value = JSON.parse(decodedData);
      } catch (error) {
        console.error(error);
      }
    }
    await initialComponent();
    // Nếu có id
    if (route.params.id) await getDataDetail(Number(route.params.id));
  });
</script>
