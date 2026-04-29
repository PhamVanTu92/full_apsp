<template>
    <div> 
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
                            <Tag v-if="data._lastDiscount || data._promotionQuanlity">
                                {{ t('body.OrderList.promotion_label') }}
                            </Tag>
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
            <Column field="price" :header="priceHeader">
                <template #body="{ data, field }">
                    <InputNumber v-if="!props.isClient" v-model="data[field]" :min="0" showButtons :disabled="props.disabledRow.activeInput" pt:input:root:class="w-12rem" placeholder="0" :minFractionDigits="2" :maxFractionDigits="2" />
                    <div v-else class="text-right">
                        {{ formatPrice(data[field]) }}
                    </div>
                </template>
            </Column>
            <Column field="discount" :header="t('body.OrderList.discount_column')">
                <template #body="{ data, field }">
                    <template v-if="!props.isClient">
                        <InputGroup>
                            <InputNumber
                                v-model="data[field]"
                                :min="0"
                                :disabled="props.disabledRow.activeInput"
                                :max="data.discountType === 'P' ? 100 : data.price"
                                placeholder="0"
                                pt:input:root:class="w-20remd"
                                class="w-12rem"
                                :minFractionDigits="2"
                                :maxFractionDigits="2"
                                @update:modelValue="updateDiscount(data)"
                            />
                            <Dropdown
                                v-model="data.discountType"
                                @change="data.discount = 0"
                                class="w-4rem surface-50 hover:bg-green-50"
                                :options="discountTypeOptions"
                                option-label="label"
                                option-value="value"
                                pt:trigger:class="hidden"
                                style="margin-left: -1px"
                            />
                        </InputGroup>
                    </template>
                    <div v-else class="text-right">
                        {{ formatDiscount(data) }}
                    </div>
                </template>
            </Column>
            <Column :header="finalPriceHeader" class="text-right">
                <template #body="{ data }">
                    {{ calculateFinalPrice(data) }}
                </template>
            </Column>
            <Column field="vat" :header="t('body.OrderList.tax_rate_column')">
                <template #body="{ data, field }">
                    <InputNumber v-if="!props.isClient" v-model="data[field]" :min="0" :max="100" suffix=" %" :disabled="props.disabledRow.activeInput" showButtons pt:input:root:class="w-7rem" :minFractionDigits="2" :maxFractionDigits="2" />
                    <div v-else class="text-right">{{ fnum(data[field], 2) }}</div>
                </template>
            </Column>
            <Column :header="subtotalHeader" class="text-right">
                <template #body="{ data }">
                    {{ calculateSubtotal(data) }}
                </template>
            </Column>
            <Column v-if="props.disabledRow.taxPrice" field="paymentMethodCode" :header="t('PromotionalItems.PromotionalItems.taxPrice')">
                <template #body="">0</template>
            </Column>
            <Column v-if="props.disabledRow.methodPayment" field="paymentMethodCode" :header="t('body.OrderList.payment_method_label')">
                <template #body="{ data, field }">
                    <Dropdown v-model="data[field]" optionValue="value" optionLabel="label" :options="paymentOptions" class="w-full" />
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
                        <ProductSelector
                            v-if="poStore.model.cardId"
                            :andPointApi="props.andPointApi"
                            :label="t('body.PurchaseRequestList.find_product_button')"
                            @confirm="onSelectProduct"
                            :customer="poStore.model._customer"
                            icon=""
                            :disabledHeader="false"
                        />
                        <template v-if="selection.length">
                            <div class="flex justify-content-end gap-2">
                                <Divider layout="vertical" class="mx-1"></Divider>
                                <Button @click="onClickRemoveRow" size="small" :label="t('body.OrderList.delete')" severity="danger" />
                            </div>
                        </template>
                    </div>
                    <div v-if="hasItems" class="flex align-items-center gap-2">
                        <Button @click="showPointDetails" outlined severity="success" :label="pointsLabel" :title="t('ChangePoint.clickToViewDetails')" />
                        <Button outlined @click="volumnDialogRef?.open()" :label="t('ChangePoint.productionDetails')" text severity="info" />
                    </div>
                </div>
            </template>
        </DataTable>
        <VolumnDialog ref="volumnDialogRef" />
        <ChangePaymentMethodDialog v-model:selected-item="selection" ref="changePaymentMethodDialogRef" />
        <PointDetailsDialog ref="pointDetailsDialogRef" />
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { ItemDetail } from '../types/entities';
import { ItemMasterData } from '../types/item.type';
import { fnum } from '../script';
import ChangePaymentMethodDialog from '../dialogs/ChangePaymentMethodDialog.vue';
import { useI18n } from 'vue-i18n';
import VolumnDialog from '../dialogs/VolumnDialog.vue';
import PointDetailsDialog from '../dialogs/PointDetailsDialog.vue';
import { usePoStore } from '../store/purchaseStore.store';
import API from '@/api/api-main';

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
const changePaymentMethodDialogRef = ref<InstanceType<typeof ChangePaymentMethodDialog> | null>(null);
const volumnDialogRef = ref<InstanceType<typeof VolumnDialog> | null>(null);
const pointDetailsDialogRef = ref<InstanceType<typeof PointDetailsDialog> | null>(null);
const receivedPoints = ref(0);
const selection = ref<ItemDetail[]>([]);
const detailPointsPromotion = ref<any[]>([]);

// Computed properties
const isUSD = computed(() => poStore.model.currency === 'USD');
const isIncoterm = computed(() => poStore.model.isIncoterm);
const hasItems = computed(() => poStore.model.itemDetail.length > 0);
const priceHeader = computed(() => (isIncoterm.value ? t('body.OrderList.unit_price_column_usd') : t('body.OrderList.unit_price_column')));
const finalPriceHeader = computed(() => (isIncoterm.value ? t('body.OrderList.final_unit_price_column_usd') : t('body.OrderList.final_unit_price_column')));
const subtotalHeader = computed(() => (isIncoterm.value ? t('body.OrderList.subtotal_before_tax_column_usd') : t('body.OrderList.subtotal_before_tax_column')));
const pointsLabel = computed(() => `${t('ChangePoint.receivedPoints')} : ${receivedPoints.value}${t('PromotionalItems.PromotionalItems.point')}`);

const discountTypeOptions = [
    { label: '%', value: 'P' },
    { label: 'Tiền', value: 'C' }
];

const paymentOptions = computed(() => [
    { label: t('body.OrderList.cash_payment_option'), value: 'PayNow' },
    { label: t('body.OrderList.credit_debt_option'), value: 'PayCredit' },
    { label: t('body.OrderList.guaranteed_debt_option'), value: 'PayGuarantee' },
    ...(props.isClient ? [{ label: t('body.OrderList.visa_credit_card_option'), value: 'PayVisa' }] : [])
]);

// Helper functions
const getDecimalPlaces = () => (isIncoterm.value ? 2 : 0);

const formatPrice = (price: number) => {
    const decimals = isUSD.value ? 2 : 0;
    return fnum(price, decimals);
};

const formatDiscount = (data: ItemDetail) => {
    if (data.discountType === 'P' || !data.discountType) {
        return `${data.discount} %`;
    }
    const decimals = isUSD.value ? 2 : 0;
    return `${fnum(data.discount ? data.discount : 0, decimals)} ${poStore.model.currency}`;
};

const calculateDiscountedPrice = (data: ItemDetail): number => {
    const discount = data.discount || 0;
    if (data.discountType === 'P') {
        return data.price - (data.price * discount) / 100;
    }
    return data.price - discount;
};

const calculateFinalPrice = (data: ItemDetail) => {
    const price = calculateDiscountedPrice(data);
    return fnum(price, getDecimalPlaces());
};

const calculateSubtotal = (data: ItemDetail) => {
    const price = calculateDiscountedPrice(data);
    const total = price * data.quantity;
    return fnum(total, getDecimalPlaces());
};

const updateDiscount = (data: ItemDetail) => {
    if (!data.discount) data.discount = 0;
};

// Event handlers
const onClickRemoveRow = () => {
    const selectedIds = selection.value.map((p) => p.itemId);
    poStore.model.itemDetail = poStore.model.itemDetail.filter((item) => !selectedIds.includes(item.itemId));
    selection.value = [];
};

const onSelectProduct = (event: ItemMasterData[]) => {
    event.forEach((item) => {
        const existItem = poStore.model.itemDetail.find((p) => p.itemId === item.id);
        if (existItem) {
            existItem.quantity++;
            existItem.discount = props.changeDiscountValue;
            existItem._volumn = existItem.volumn || 0;
        } else { 
            const newItem = new ItemDetail(item);
            newItem.discount = props.changeDiscountValue; 
            newItem.volumn = item.packing.volumn; 
            poStore.model.itemDetail.push(newItem);
        }
    });
    getPointPromion();
};

const getPointPromion = async () => {
    if (!poStore.model.cardId || !poStore.model.itemDetail.length) {
        receivedPoints.value = 0;
        detailPointsPromotion.value = [];
        return;
    }
    try {
        const data = {
            cardId: poStore.model.cardId,
            calculatorPointLine: poStore.model.itemDetail.map((item) => ({
                itemId: item.itemId,
                quantity: item.quantity
            }))
        };
        const res = await API.add('PointSetup/getPoints', data);
        if (res.data.items) {
            detailPointsPromotion.value = res.data.items
                .filter((item: any) => item.totalPoint > 0)
                .map((detail: any) => {
                    const item = poStore.model.itemDetail.find((i) => i.itemId === detail.itemId);
                    return {
                        ...detail,
                        itemName: item?.itemName || ''
                    };
                });

            receivedPoints.value = res.data.items.reduce((total: number, item: { totalPoint: number }) => total + (item.totalPoint || 0), 0);
        }
    } catch (error) {
        console.error('Error fetching point promotion:', error);
        receivedPoints.value = 0;
        detailPointsPromotion.value = [];
    }
};

const showPointDetails = () => {
    if (detailPointsPromotion.value.length > 0) {
        pointDetailsDialogRef.value?.open(detailPointsPromotion.value);
    }
};

// Watchers
watch(
    () => poStore.model._customer?.id,
    () => {
        selection.value = [];
    },
    { deep: true }
);

watch(
    () => poStore.model.itemDetail,
    () => {
        getPointPromion();
    },
    { deep: true }
);
</script>
