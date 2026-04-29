<template>
  <!-- <pre>{{ promotionToShow }}</pre> -->
  <DataTable
    v-model:expandedRows="expandedRows"
    :value="promotionToShow"
    :showGridlines="true"
    class="mb-3"
  >
    <template #header>
      <div class="flex align-items-center justify-content-between">
        <div class="text-lg">{{ t("body.OrderList.promotion_label") }}</div>
        <div class="flex gap-2">
          <ProductSelector
            :label="t('PromotionalItems.SetupPurchases.choose_product_button')"
            @confirm="onSelectProduct"
            :customer="poStore.model._customer"
            icon=""
            page="NET"
          />
          <ProductSelector
            :andPointApi="'Item/ItemPromotions'"
            :disabledUser="true"
            :label="t('PromotionalItems.SetupPurchases.add_product_button')"
            @confirm="onSelectProduct"
            :customer="poStore.model._customer"
            icon=""
          />
        </div>
      </div>
    </template>
    <template #expansion="slotProps">
      <DataTable :value="slotProps.data.promotionOrderLineSub">
        <Column header="#" class="w-5rem text-center">
          <template #body="{ index }">
            {{ index + 1 }}
          </template>
        </Column>
        <Column
          field="itemName"
          :header="t('body.PurchaseRequestList.product_name_column')"
        >
          <template #body="{ data }">
            {{ data.itemName || data.name || "N/A" }}
          </template>
        </Column>
        <Column
          field="quantityAdd"
          class="w-7rem text-right"
          :header="t('body.home.quantity')"
        >
          <template #body="{ data }">
            <InputNumber v-model="data.quantityAdd" :min="1" :max="99999" />
          </template>
        </Column>
        <Column
          field="packingName"
          class="w-15rem"
          :header="t('body.OrderList.unit_column')"
        >
          <template #body="{ data }">
            {{ data.packingName }}
          </template>
        </Column>
        <Column field="itemGroup" class="w-15rem" :header="t('client.product_type')">
          <template #body="{}">
            {{ t("PromotionalItems.SetupPurchases.productFree") }}
          </template>
        </Column>
        <Column header="" class="w-5rem text-center">
          <template #body="{ index }">
            <i
              class="pi pi-trash"
              style="color: red"
              @click="
                () => {
                  slotProps.data.promotionOrderLineSub.splice(index, 1);
                }
              "
            ></i>
          </template>
        </Column>
      </DataTable>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
  import { ref, watch } from "vue";
  import { usePoStore } from "../store/purchaseStore.store";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  const poStore = usePoStore();
  const expandedRows = ref<any>([]);
  const promotionToShow = ref([] as any[]);
  const onSelectProduct = (value: any) => {
    const giftItems = Array.isArray(value) ? value : [value];
    let existingPromotion = promotionToShow.value.find((p) => p.promotionName === "");
    if (existingPromotion) {
      giftItems.forEach((item) => {
        const exists = existingPromotion.promotionOrderLineSub.some(
          (g: any) =>
            (g.id && item.id && g.id === item.id) ||
            g.itemName === (item.itemName || item.name)
        );
        if (!exists) {
          existingPromotion.promotionOrderLineSub.push({
            ...item,
            itemName: item.itemName || item.name || "",
            quantityAdd: item.quantityAdd || item.quantity || 1,
            packingName: item.packingName || item.packing.name || "",
            itemGroup: item.itemGroup || "KH",
            lineId: item.lineId || 0,
          });
        }
      });
      if (!expandedRows.value.includes(existingPromotion)) {
        expandedRows.value.push(existingPromotion);
      }
    } else {
      let dataAddPromotion = {
        promotionName: "",
        promotionDesc: "",
        promotionOrderLineSub: giftItems.map((item) => ({
          ...item,
          itemName: item.itemName || item.name || "",
          quantityAdd: item.quantityAdd || item.quantity || 1,
          packingName: item.packingName || item.packing.name || "",
          itemGroup: item.itemGroup || "KH",
          lineId: item.lineId || 0,
        })),
      };
      promotionToShow.value.push(dataAddPromotion);
      expandedRows.value.push(dataAddPromotion);
    }
  };

  watch(
    promotionToShow,
    (val) => {
      poStore.model.promotion = val[0]?.promotionOrderLineSub;
      poStore.model.Promotions = {
        id: 0,
        orderDate: "",
        cardId: 0,
        promotionOrderLine: val[0]?.promotionOrderLineSub,
      };
    },
    { deep: true }
  );
</script>

<style>
  .collapse-row > *:not(:last-child) {
    padding-bottom: 0.75rem;
    border-bottom: 1px solid var(--surface-border);
    margin-bottom: 0.75rem;
  }

  .w-selection {
    width: 3.5rem !important;
  }

  .no-border-top * {
    border-top: none !important;
    background: none;
  }

  .no-border * {
    border: none !important;
  }
</style>
