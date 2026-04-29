<template>
  <div>
    <DataTable
      v-model:selection="selection"
      :value="poStore.model.itemDetail"
      resizableColumns
      columnResizeMode="fit"
      showGridlines
      tableStyle="min-width: 50rem"
      scrollable
    >
      <Column selectionMode="multiple" frozen class="z-5 border-right-1"></Column>
      <Column header="#">
        <template #body="{ index }">
          {{ index + 1 }}
        </template>
      </Column>
      <Column
        field="itemName"
        :header="t('body.OrderList.product_name_column')"
        style="min-width: 30rem"
      >
        <template #body="{ data, field }">
          <div class="w-full gap-2 flex">
            <div
              class="flex-grow-1"
              style="
                word-wrap: break-word;
                white-space: pre-wrap;
                overflow-wrap: break-word;
              "
            >
              {{ data[field] }}
            </div>
            <div>
              <Tag v-if="data._lastDiscount || data._promotionQuanlity">{{
                t("body.OrderList.promotion_label")
              }}</Tag>
            </div>
          </div>
        </template>
      </Column>
      <Column :header="t('body.OrderList.quantity_column')" class="w-10rem">
        <template #body="{ data }">
          <InputNumber
            v-model="data.quantity"
            :min="0.1"
            showButtons
            pt:input:root:class="w-10rem"
            :minFractionDigits="3"
            :maxFractionDigits="3"
            placeholder="Số lượng"
          />
        </template>
      </Column>
      <Column :header="t('body.home.packaging_column')">
        <template #body="{ data }">
          <div>{{ data.packing?.code || "--" }}</div>
        </template>
      </Column>

      <template #header v-if="selection.length && false">
        <div class="flex justify-content-end gap-2">
          <Button
            size="small"
            :label="t('body.promotion.copy_button')"
            :disabled="selection.length < 1"
          />
          <Button
            @click="changePaymentMethodDialogRef?.open()"
            size="small"
            :label="t('body.OrderList.payment_method_label')"
            :disabled="selection.length < 1"
          />
          <Button
            @click="onClickRemoveRow"
            size="small"
            :label="t('body.OrderList.delete')"
            severity="danger"
          />
        </div>
      </template>
      <template #empty>
        <div class="my-7 py-7"></div>
      </template>
      <template #footer>
        <div class="flex justify-content-between">
          <div class="flex gap-2">
            <ProductSelector
              v-if="poStore.model.cardId"
              :andPointApi="props.andPointApi"
              :label="t('body.PurchaseRequestList.find_product_button')"
              @confirm="onSelectProduct"
              :customer="poStore.model._customer"
              icon=""
            />
            <template v-if="selection.length">
              <div class="flex justify-content-end gap-2">
                <Divider layout="vertical" class="mx-1"></Divider>
                <Button
                  @click="onClickRemoveRow"
                  size="small"
                  :label="t('body.OrderList.delete')"
                  severity="danger"
                />
              </div>
            </template>
          </div>
          <div class="flex align-items-center gap-2">
            <Button
              outlined
              @click="volumnDialogRef?.open()"
              :label="t('body.OrderList.viewDetails')"
              text
              severity="info"
              v-if="poStore.model.itemDetail.length > 0"
            />
          </div>
        </div>
      </template>
    </DataTable>
    <VolumnDialog ref="volumnDialogRef"></VolumnDialog>
    <ChangePaymentMethodDialog
      v-model:selected-item="selection"
      ref="changePaymentMethodDialogRef"
    />
  </div>
</template>

<script setup lang="ts">
  import { ref, watch } from "vue";
  import { ItemDetail } from "../types/entities";
  import { ItemMasterData } from "../types/item.type";
  import ChangePaymentMethodDialog from "../dialogs/ChangePaymentMethodDialog.vue";
  import { useI18n } from "vue-i18n";
  import VolumnDialog from "../dialogs/VolumnDialog.vue";
  import { usePoStore } from "../store/purchaseStore.store";
  const changePaymentMethodDialogRef = ref<InstanceType<
    typeof ChangePaymentMethodDialog
  > | null>(null);
  const volumnDialogRef = ref<InstanceType<typeof VolumnDialog> | null>(null);
  const props = defineProps({
    isClient: {
      type: Boolean,
      default: false,
    },
    andPointApi: {
      type: String,
      default: "item",
    },
    disabledRow: {
      type: Object,
      default: () => ({
        activeInput: false, //disable all input
        methodPayment: true, //disable payment method
        taxPrice: false, //disable tax price
      }),
    },
    changeDiscountValue: {
      type: Number,
      default: 0,
    },
  });
  const poStore = usePoStore();
  const { t } = useI18n();
  const selection = ref<ItemDetail[]>([]);
  const onClickRemoveRow = () => {
    poStore.model.itemDetail = poStore.model.itemDetail.filter((item) => {
      return !selection.value.map((p) => p.itemId).includes(item.itemId);
    });
    selection.value = [];
  };

  const onSelectProduct = (event: ItemMasterData[]) => {
    for (let item of event) {
      const existItem = poStore.model.itemDetail.find((p) => p.itemId == item.id);
      if (existItem) {
        existItem.quantity++;
        existItem.discount = props.changeDiscountValue;
      } else {
        const newItem = new ItemDetail(item);
        newItem.discount = props.changeDiscountValue;
        poStore.model.itemDetail.push(newItem);
      }
    }
  };

  watch(
    () => poStore.model._customer?.id,
    () => {
      selection.value = [];
    },
    { deep: true }
  );
</script>
