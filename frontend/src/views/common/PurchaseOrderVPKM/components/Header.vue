<template>
  <div class="grid">
    <div class="col-2 pr-0 hidden">
      <div class="flex flex-column formgrid field">
        <label for="">{{ t("body.OrderList.orderCode") }}</label>
        <InputText disabled></InputText>
      </div>
    </div>
    <div class="col-12 md:col-7 pr-0">
      <div class="flex flex-column formgrid field">
        <label for="">{{ t("body.OrderList.customer_label") }}</label>
        <CustomerSelector
          @item-select="onSelectCustomer"
          @clear="onClearCustomer"
          :poStore="poStore"
          :filted="true"
          :placeholder="t('body.OrderList.select_customer_placeholder')"
        ></CustomerSelector>
      </div>
    </div>
    <div class="col-12 md:col-5">
      <div class="grid">
        <div class="col-12 pb-0 field">
          <label class="" for="">{{ t("body.OrderList.order_date_label") }}</label>
        </div>
        <div class="col-5 py-0">
          <div class="flex flex-column formgrid field">
            <Calendar
              v-model="poStore.model.deliveryTime"
              showTime
              hourFormat="24"
            ></Calendar>
          </div>
        </div>
        <div class="col py-0 pl-0">
          <div class="flex flex-column formgrid field mb-0">
            <div class="flex align-items-center gap-3">
              <div class="flex gap-2">
                <Checkbox v-model="poStore.model.isIncoterm" binary readonly></Checkbox>
                <div>{{ t("body.sampleRequest.customer.incoterm") }} 2020</div>
              </div>
              <div class="flex-grow-1">
                <Dropdown
                  v-model="poStore.model.currency"
                  :options="currencyOption"
                  class="w-full"
                >
                </Dropdown>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, watch, onMounted } from "vue";
  import { PurchaseOrder, Customer } from "../types/entities";

  import { usePoStore } from "../store/purchaseStore.store";
  import { useI18n } from "vue-i18n";
  import { useRoute } from "vue-router";

  const { t } = useI18n();
  const route = useRoute();
  const poStore = usePoStore();

  const currencyOption = ["VND", "USD"];

  const onClearCustomer = () => {
    poStore.$reset();
  };
  const emits = defineEmits(["item-select"]);
  const onSelectCustomer = (event: Customer) => {
    poStore.model = new PurchaseOrder(new Customer(event));
    emits("item-select", true);
    // poStore.fetchCheckPriceMethod();
  };

  const initialComponent = () => {
    // code here
  };

  onMounted(function () {
    initialComponent();
  });
  defineExpose({
    onSelectCustomer,
  });
</script>

<style scoped>
  .field {
    margin-bottom: 0;
  }
</style>
