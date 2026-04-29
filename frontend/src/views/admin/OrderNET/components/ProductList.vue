<template>
    <div>
        <!-- <pre>{{ poStore.model.itemDetail }}</pre> -->
        <DataTable v-model:selection="selection" :value="poStore.model.itemDetail" resizableColumns columnResizeMode="fit" showGridlines tableStyle="min-width: 50rem" scrollable>
            <Column selectionMode="multiple" frozen class="z-5 border-right-1"></Column>
            <Column header="#">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="itemName" :header="t('body.OrderList.product_name_column')" style="min-width: 30rem">
                <template #body="{ data, field }">
                    <div class="w-full gap-2 flex">
                        <div class="flex-grow-1" style="word-wrap: break-word; white-space: pre-wrap; overflow-wrap: break-word">
                            {{ data[field] }}
                        </div>
                        <div>
                            <Tag v-if="data._lastDiscount || data._promotionQuanlity">{{ t('body.OrderList.promotion_label') }}</Tag>
                        </div>
                    </div>
                </template>
            </Column>
            <Column :header="t('body.OrderList.quantity_column')" class="w-10rem">
                <template #body="{ data }">
                    <InputNumber v-model="data.quantity" :min="0.1" showButtons pt:input:root:class="w-10rem" :minFractionDigits="3" :maxFractionDigits="3" placeholder="Số lượng" />
                </template>
            </Column>
            <Column field="uomName" :header="t('body.OrderList.unit_column')"></Column>
            <Column field="price" :header="!poStore.model.isIncoterm ? t('body.OrderList.unit_price_column') : t('body.OrderList.unit_price_column_usd')">
                <template #body="{ data, field }">
                    <InputNumber v-if="!props.isClient" v-model="data[field]" :min="0" showButtons :minFractionDigits="2" :maxFractionDigits="2" :disabled="props.disabledRow.activeInput" pt:input:root:class="w-12rem" placeholder="0" />
                    <div v-else class="text-right">{{ fnum(data[field]) }}</div>
                </template>
            </Column>
            <Column field="discount" :header="t('body.OrderList.discount_column')">
                <template #body="{ data, field }">
                    <div v-if="!props.isClient">
                        <InputGroup>
                            <InputNumber
                                v-model="data[field]"
                                :min="0"
                                :max="data['discountType'] == 'P' ? 100 : data.price"
                                placeholder="0"
                                pt:input:root:class="w-20remd"
                                class="w-12rem"
                                :minFractionDigits="2"
                                :maxFractionDigits="2"
                                @update:modelValue="
                                    (val) => {
                                        if (data['discountType'] === 'P') {
                                            data.priceAfterDist = data.price - (data.price * val) / 100;
                                        } else {
                                            data.priceAfterDist = data.price - val;
                                        }
                                    }
                                "
                            />
                            <Dropdown
                                v-model="data['discountType']"
                                class="w-4rem surface-50 hover:bg-green-50"
                                :options="discountTypeOptions"
                                option-label="label"
                                option-value="value"
                                pt:trigger:class="hidden"
                                style="margin-left: -1px"
                                @change="data[field] = 0"
                            >
                            </Dropdown>
                        </InputGroup>
                    </div>
                    <div v-else class="text-right">{{ fnum(data[field], 2) }}</div>
                </template>
            </Column>
            <Column :header="!poStore.model.isIncoterm ? t('body.OrderList.final_unit_price_column') : t('body.OrderList.final_unit_price_column_usd')" class="text-right" field="priceAfterDist">
                <template #body="{ data, field }">
                <div class="grid">
                    <InputNumber
                        v-model="data[field]"
                        placeholder="0" 
                        :min="0"
                        :minFractionDigits="2"
                        :maxFractionDigits="2"
                        @input="(e) => handlePriceAfterDistChange(e.value, data, field)"
  
                        :invalid="data.price < data.priceAfterDist"
                    /> 
                    </div>
                </template>
            </Column>
            <Column field="vat" :header="t('body.OrderList.tax_rate_column')">
                <template #body="{ data, field }">
                    <InputNumber v-if="!props.isClient" v-model="data[field]" :min="0" :max="100" suffix=" %" showButtons pt:input:root:class="w-7rem" :minFractionDigits="2" :maxFractionDigits="2" />
                    <div v-else class="text-right">{{ fnum(data[field], 2) }}</div>
                </template>
            </Column>
            <Column :header="!poStore.model.isIncoterm ? t('body.OrderList.subtotal_before_tax_column') : t('body.OrderList.subtotal_before_tax_column_usd')" class="text-right">
                <template #body="{ data }">
                    {{ fnum(data.priceAfterDist * data.quantity, 2) }}
                </template>
            </Column>
            <Column :header="t('PromotionalItems.PromotionalItems.taxPrice')">
                <template #body="{ data, field }">
                    {{ fnum((data.priceAfterDist * data.quantity * data.vat) / 100, 2) }}
                </template>
            </Column>
            <Column field="paymentMethodCode" :header="t('body.OrderList.payment_method_label')" v-if="props.disabledRow.methodPayment">
                <template #body="{ data, field }">
                    <Dropdown v-model="data[field]" optionValue="value" optionLabel="label" :options="paymentOptions" class="w-full"></Dropdown>
                </template>
            </Column>

            <template #header v-if="selection.length && false">
                <div class="flex justify-content-end gap-2">
                    <Button size="small" :label="t('body.promotion.copy_button')" :disabled="selection.length < 1" />
                    <Button @click="changePaymentMethodDialogRef?.open()" size="small" :label="t('body.OrderList.payment_method_label')" :disabled="selection.length < 1" />
                    <Button @click="onClickRemoveRow" size="small" :label="t('body.OrderList.delete')" severity="danger" />
                </div>
            </template>
            <template #empty>
                <div class="my-7 py-7"></div>
            </template>
            <template #footer>
                <div class="flex justify-content-between">
                    <div class="flex gap-2">
                        <ProductSelector v-if="poStore.model.cardId" :andPointApi="props.andPointApi" :label="t('body.PurchaseRequestList.find_product_button')" @confirm="onSelectProduct" :customer="poStore.model._customer" icon="" />
                        <template v-if="selection.length">
                            <div class="flex justify-content-end gap-2">
                                <Divider layout="vertical" class="mx-1" />
                                <Button @click="onClickRemoveRow" size="small" :label="t('body.OrderList.delete')" severity="danger" />
                            </div>
                        </template>
                    </div>
                    <div class="flex align-items-center gap-2">
                        <Button outlined @click="volumnDialogRef?.open()" :label="t('body.OrderList.viewDetails')" text severity="info" v-if="poStore.model.itemDetail.length > 0" />
                    </div>
                </div>
            </template>
        </DataTable>
        <VolumnDialog ref="volumnDialogRef"></VolumnDialog>
        <ChangePaymentMethodDialog v-model:selected-item="selection" ref="changePaymentMethodDialogRef" />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { ItemDetail } from '../types/entities';
import { ItemMasterData } from '../types/item.type';
import { fnum } from '../script';
import ChangePaymentMethodDialog from '../dialogs/ChangePaymentMethodDialog.vue';
import { useI18n } from 'vue-i18n';
import VolumnDialog from '../dialogs/VolumnDialog.vue';
import { usePoStore } from '../store/purchaseStore.store';
import { useToast } from 'primevue/usetoast';
const toast = useToast();
const changePaymentMethodDialogRef = ref<InstanceType<typeof ChangePaymentMethodDialog> | null>(null);
const volumnDialogRef = ref<InstanceType<typeof VolumnDialog> | null>(null);
const props = defineProps({
    isClient: {
        type: Boolean,
        default: false
    },
    andPointApi: {
        type: String,
        default: 'item'
    },
    disabledRow: {
        type: Object,
        default: () => ({
            activeInput: false,
            methodPayment: true,
            taxPrice: false
        })
    },
    changeDiscountValue: {
        type: Number,
        default: 0
    }
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
            newItem.priceAfterDist = newItem.price - (newItem.discountType == 'P' ? (newItem.price * (newItem.discount || 0)) / 100 : newItem.discount || 0);
        }
    }
};
const discountTypeOptions = [
    { label: '%', value: 'P' },
    { label: 'Tiền', value: 'C' }
];

const paymentOptions = [
    { label: t('body.OrderList.cash_payment_option'), value: 'PayNow' },
    { label: t('body.OrderList.credit_debt_option'), value: 'PayCredit' },
    { label: t('body.OrderList.guaranteed_debt_option'), value: 'PayGuarantee' }
]; 
const handlePriceAfterDistChange = (val, data, field) => {
    const priceAfter = Number(val) || 0;
    const price = Number(data.price) || 0;
    if (!price) {
        data.discount = 0;
    } else if (data.discountType === 'P') {
        data.discount = Math.max(0, (1 - priceAfter / price) * 100);
    } else {
        data.discount = Math.max(0, price - priceAfter);
    }
    data.priceAfterDist = priceAfter;
    if(priceAfter > price) {
        toast.add({ severity: 'error', summary: 'Giá sau chiết khấu không được lớn hơn giá gốc', life: 2000 });
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
