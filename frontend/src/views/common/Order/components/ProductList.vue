<template>
    <DataTable :value="odStore.order?.itemDetail" resizableColumns columnResizeMode="fit" show-gridlines>
        <Column field="" header="#">
            <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column field="itemName" :header="t('client.productName')"></Column>
        <Column field="uomName" :header="t('client.unit')"></Column>
        <Column field="quantity" :header="t('client.quantity')" class="text-right">
            <template #body="{ data, field }">
                {{ fnum(data[field]) }}
            </template>
        </Column>
        <Column field="price" :header="!odStore.order?.isIncoterm ? t('body.OrderList.unit_price_column') : t('body.OrderList.unit_price_column_usd')" class="text-right">
            <template #body="{ data, field }">
                {{ odStore.order?.currency == 'USD' ? fnum(data[field], 2) : fnum(data[field], 0) }}
            </template>
        </Column>
        <Column field="discount" :header="t('client.discount')" class="text-right">
            <template #body="{ data, field }"> {{ odStore.order?.currency == 'VND' && data['discountType'] === 'C' ? fnum(data[field], 0) : fnum(data[field], 2) }} {{ data['discountType'] === 'C' ? odStore.order?.currency : '%' }} </template>
        </Column>
        <Column field="priceAfterDist" :header="!odStore.order?.isIncoterm ? t('body.OrderList.final_unit_price_column') : t('body.OrderList.final_unit_price_column_usd')" class="text-right">
            <template #body="{ data, field }">
                {{ odStore.order?.currency == 'USD' ? fnum(data[field], 2) : fnum(data[field], 0) }}
            </template>
        </Column>
        <Column field="vat" :header="t('client.taxRate')" class="text-right">
            <template #body="{ data, field }">
                {{ odStore.order?.currency == 'USD' ? fnum(data[field], 2) : fnum(data[field], 0) }}
            </template>
        </Column>
        <Column field="" :header="!odStore.order?.isIncoterm ? t('body.OrderList.subtotal_before_tax_column') : t('body.OrderList.subtotal_before_tax_column_usd')" class="text-right">
            <template #body="{ data }">
                {{ odStore.order?.currency == 'USD' ? fnum(data['priceAfterDist'] * data['quantity'], 2) : fnum(data['priceAfterDist'] * data['quantity'], 0) }}
            </template>
        </Column>
        <Column field="paymentMethodCode" :header="t('client.payment_methods')" class="text-right">
            <template #body="{ data, field }">
                {{ getPaymentLabel(data[field]) }}
            </template>
        </Column>
        <template #footer>
            <div class="font-normal">
                {{ t('client.total_output') }}
                <span class="font-bold">{{ fnum(volumn || 0, 0, t('client.liter')) }}</span> <br />
                {{ t('ChangePoint.points_promoion') }}: <b> {{ odStore?.order?.customerPointHistory?.reduce((sum: number, item: any) => sum + (item.pointChange || 0), 0) }} {{ t('PromotionalItems.PromotionalItems.point') }}</b>
            </div>
        </template>
    </DataTable>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { fnum } from '../../PurchaseOrder/script';
import { useOrderDetailStore } from '../store/orderDetail';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const odStore = useOrderDetailStore();
const volumn = computed(() => {
    return odStore.order?.itemDetail.reduce((sum, item) => item.itemVolume * item.quantity + sum, 0);
});
const paymentMethodCode = {
    PayNow: t('client.pay_immediately'),
    PayCredit: t('client.debt_balance'),
    PayGuarantee: t('client.installment_debt'),
    PayVisa: t('client.visa_credit_card')
};

const getPaymentLabel = (key: string) => {
    return paymentMethodCode[key as keyof typeof paymentMethodCode] || '';
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>
