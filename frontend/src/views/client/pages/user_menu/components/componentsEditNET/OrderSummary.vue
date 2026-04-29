<template>
    <div class="border-1 border-200 p-3 flex flex-column test">
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.order_before_ck') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.totalBeforeVat || 0) }}</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.promotion_discount') }}</div>
            <div>{{ fnum(0) }}</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.sales_incentive') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.bonusCommited || 0) }}</div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.ttn_bonus') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.bonusAmount || 0) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.order_after_ck') }}</div>
            <div>
                {{ fnum((odStore.order?.paymentInfo?.totalBeforeVat || 0) - (odStore.order?.paymentInfo?.bonusCommited
                    || 0)) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.total_tax') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.vatAmount || 0) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.payment_today') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.totalPayNow || 0) }}</div>
        </div>
        <div class="flex justify-content-between mb-3">
            <div>{{ t('client.debt_balance') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.totalDebt || 0) }}</div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('client.installment_debt') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.totalDebtGuarantee || 0) }}</div>
        </div>
        <hr />
        <div class="flex justify-content-between">
            <div>{{ t('client.total_invoice') }}</div>
            <div>{{ fnum(odStore.order?.paymentInfo?.totalAfterVat || 0) }}</div>
        </div>
        <hr />

        <div class="flex justify-content-between my-3 font-semibold text-lg">
            <div>{{ t('client.total_payment_amount') }}</div>
            <div>
                {{ fnum((odStore.order?.paymentInfo?.totalAfterVat || 0) - (odStore.order?.paymentInfo?.bonusAmount ||
                    0)) }}
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetailNET';
import { fnum } from '../types/script';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const odStore = useOrderDetailStore();

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
