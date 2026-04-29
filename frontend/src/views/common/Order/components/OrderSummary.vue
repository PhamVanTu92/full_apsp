<template>
    <div class="border-1 border-200 p-3 flex flex-column test">
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.order_before_ck') }}</div>
            <div>{{ formatCurrency(totalBeforeVat) }}</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.promotion_discount') }}</div>
            <div>0</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.sales_incentive') }}</div>
            <div>{{ formatCurrency(bonusCommited) }}</div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.ttn_bonus') }}</div>
            <div>{{ formatCurrency(bonusAmount) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.order_after_ck') }}</div>
            <div>{{ formatCurrency(totalAfterDiscount) }}</div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.total_tax') }}</div>
            <div>{{ formatCurrency(vatAmount) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.payment_today') }}</div>
            <div>{{ formatCurrency(totalPayNow) }}</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.debt_balance') }}</div>
            <div>{{ formatCurrency(totalDebt) }}</div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.installment_debt') }}</div>
            <div>{{ formatCurrency(totalDebtGuarantee) }}</div>
        </div>
        <div v-if="hasVisaPayment" class="flex justify-content-between mt-3">
            <div>{{ t('client.visa_credit_card') }}</div>
            <div>{{ formatCurrency(totalVisa) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between">
            <div>{{ t('client.total_invoice') }}</div>
            <div>{{ formatCurrency(totalAfterVat) }}</div>
        </div>
        <hr /> 
        <div class="flex justify-content-between my-3 font-semibold text-lg">
            <div>{{ t('client.total_payment_amount') }}</div>
            <div>{{ formatCurrency(totalPayment) }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetail';
import { fnum } from '../../PurchaseOrder/script';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const odStore = useOrderDetailStore();
const VISA_PAYMENT_METHOD = 'PayVisa';

const decimalPlaces = computed(() => (odStore.order?.currency === 'USD' ? 2 : 0));
const paymentInfo = computed(() => odStore.order?.paymentInfo || {});
const itemDetails = computed(() => odStore.order?.itemDetail || []);

const toNumber = (value: unknown) => {
    const parsedValue = Number(value);
    return Number.isFinite(parsedValue) ? parsedValue : 0;
};

const getLineBeforeVat = (item: any) => {
    const quantity = toNumber(item?.quantity);
    const price = toNumber(item?.price);
    const discount = toNumber(item?.discount);
    const priceAfterDist = toNumber(item?.priceAfterDist);

    if (priceAfterDist) return priceAfterDist * quantity;

    const priceAfterDiscount = item?.discountType === 'C'
        ? price - discount
        : price - (price * discount) / 100;
    return priceAfterDiscount * quantity;
};

const getLineVat = (item: any) => {
    const storedVatAmount = toNumber(item?.vatAmount);
    if (storedVatAmount) return storedVatAmount;
    return getLineBeforeVat(item) * (toNumber(item?.vat) / 100);
};

const getPaymentTotal = (paymentMethodCode: string) => {
    return itemDetails.value
        .filter((item: any) => item?.paymentMethodCode === paymentMethodCode)
        .reduce((total: number, item: any) => total + getLineBeforeVat(item) + getLineVat(item), 0);
};

const sumItemBeforeVat = computed(() => itemDetails.value.reduce((total: number, item: any) => total + getLineBeforeVat(item), 0));
const sumItemVat = computed(() => itemDetails.value.reduce((total: number, item: any) => total + getLineVat(item), 0));
const totalBeforeVat = computed(() => toNumber((paymentInfo.value as any)?.totalBeforeVat) || sumItemBeforeVat.value);
const bonusCommited = computed(() => toNumber((paymentInfo.value as any)?.bonusCommited));
const bonusAmount = computed(() => toNumber((paymentInfo.value as any)?.bonusAmount));
const totalAfterDiscount = computed(() => totalBeforeVat.value - bonusCommited.value);
const vatAmount = computed(() => toNumber((paymentInfo.value as any)?.vatAmount) || sumItemVat.value);
const totalAfterVat = computed(() => toNumber((paymentInfo.value as any)?.totalAfterVat) || totalAfterDiscount.value + vatAmount.value);
const totalPayment = computed(() => totalAfterVat.value - bonusAmount.value);
const hasVisaPayment = computed(() => itemDetails.value.some((item: any) => item?.paymentMethodCode === VISA_PAYMENT_METHOD));
const totalVisa = computed(() => getPaymentTotal(VISA_PAYMENT_METHOD) || (hasVisaPayment.value ? totalPayment.value : 0));
const totalPayNow = computed(() => toNumber((paymentInfo.value as any)?.totalPayNow));
const totalDebt = computed(() => toNumber((paymentInfo.value as any)?.totalDebt));
const totalDebtGuarantee = computed(() => toNumber((paymentInfo.value as any)?.totalDebtGuarantee));

const formatCurrency = (value: number) => {
    return fnum(value, decimalPlaces.value);
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.test {
    /* border-radius: 8px; */
    box-shadow: 0 8px 8px rgba(0, 0, 0, 0.1);
    background-color: #fff;
}
</style>
