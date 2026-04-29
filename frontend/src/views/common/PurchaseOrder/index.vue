<template>
  <div>
    <div
      v-if="!poStore.isClient && !props.isClient"
      class="flex justify-content-between mb-3"
    >
      <h4 class="mb-0 font-bold">{{ t("body.OrderList.create_new_order_title") }}</h4>
      <ButtonGoBack />
    </div>
    <div class="grid">
      <div class="col-9">
        <Header ref="getCustomers" v-if="!poStore.isClient && !props.isClient"></Header>
        <TabView
          v-model:activeIndex="poStore.activeIndexTab"
          class="card p-0 overflow-hidden"
          style="min-height: 575px"
        >
          <TabPanel :header="t('body.OrderList.products_tab')">
            <ProductList :isClient="props.isClient" class="mb-3" />
            <PromotionList v-if="poStore.model.docType == ''" />
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
                :currency="poStore.model.currency"
                @update:isExceedDebt="handleExceedDebt"
              />
            </div>
          </div>
          <div class="col-12 py-0">
            <OrderSummary class="mb-3" :isClient="props.isClient" />
            <Committed />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from "vue";
  import { Customer, ItemDetail, PurchaseOrder } from "./types/entities";
  import Header from "./components/Header.vue";
  import ProductList from "./components/ProductList.vue";
  import PromotionList from "./components/PromotionList.vue";
  import OrderSummary from "./components/OrderSummary.vue";
  import CustomerInfo from "./components/CustomerInfo.vue";
  import Committed from "./components/Committed.vue";
  import { isArray } from "lodash";

  import { usePoStore } from "./store/purchaseStore.store";
  import { useCartStore } from "@/Pinia/cart";
  import { useRoute } from "vue-router";
  import { useI18n } from "vue-i18n";
  import API from "@/api/api-main";

  const handleExceedDebt = (val: boolean) => {
    poStore.isExceedDebt = val;
  };
  const { t } = useI18n();
  const getCustomers = ref();
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

  const initialComponent = async () => {
    poStore.$reset();
    await new Promise((result: any) => setTimeout(() => result(), 10));
    if (props.isClient) {
      await cartStore.getCart();
      if (isArray(cartStore.getItems)) {
        for (const item of cartStore.getItems) {
          const newItem = new ItemDetail(
            item["item"],
            item.paymentMethodCode,
            item.quantity
          );
          poStore.model.itemDetail.push(newItem);
        }
      }
    }
  };

  const getQueryParams = async (str: string) => {
    try {
      let data = atob(str);
      let decodedData = decodeURIComponent(data);
      orderData.value = JSON.parse(decodedData);
      orderData.value.deliveryTime = new Date(orderData.value.deliveryTime);
      if (orderData.value) {
        const newModel = new PurchaseOrder();
        poStore.model = newModel;
        let tempItemDetail: ItemDetail[] = [];
        if (orderData.value.itemDetail && Array.isArray(orderData.value.itemDetail)) {
          for (const item of orderData.value.itemDetail) {
            const itemDetail = new ItemDetail();
            Object.assign(itemDetail, item);
            itemDetail.discountType = item.discountType || "P";
            tempItemDetail.push(itemDetail);
          }
        }
        try {
          const res = await API.get(
            `customer/${orderData.value.cardId || route.query.customerId}`
          );
          const customerData = new Customer(res.data);
          if (!props.isClient) {
            getCustomers.value.onSelectCustomer(customerData);
          } else {
            poStore.model.setCustomer(customerData);
          }
          poStore.model.itemDetail = tempItemDetail;
        } catch (error) {
          console.error(error);
        }
      }
    } catch (error) {
      console.error("Lỗi khi truyền data vào poStore:", error);
    }
  };

  onMounted(async function () {
    await initialComponent();
    if (route.query.invoice && typeof route.query.invoice === "string")
      getQueryParams(route.query.invoice);
  });
</script>
