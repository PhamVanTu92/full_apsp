<template>
    <div class="border-1 border-green-700 p-3 border-round bg-white">
        <template v-for="(item, i) in lists" :key="i">
            <div class="flex justify-content-between my-3">
                <span v-html="item.label"></span>
                <span>{{ fnum(getListItemValue(item), poStore.model.currency == 'USD' ? 2 : 0) }}</span>
            </div>
            <Divider v-if="item.divider" />
        </template>
        <div class="flex justify-content-between my-3">
            <span class="">{{ t('body.OrderList.total_order_value') }}</span>
            <span class="">{{ fnum(totalAfterVat, poStore.model.currency == 'USD' ? 2 : 0) }}</span>
        </div>
        <div class="mt-5">
            <div class="flex justify-content-between text-lg">
                <span class="font-bold">{{ t('body.OrderList.total_payment') }}</span>
                <span class="font-bold">{{ fnum(totalPayment, poStore.model.currency == 'USD' ? 2 : 0) }}</span>
            </div>
            <div class="mb-3">
                <Button @click="paymentDetailDialogRef?.open()" :label="t('body.OrderList.viewDetails')" text severity="info" class="p-0 font-light" size="small" v-if="poStore.model.itemDetail.length" />
                <div v-else class="mb-5 pb-1"></div>
            </div>
            <small v-if="hasMixedVisaPayment" class="block text-red-500 mb-3">
                {{ t('client.visa_payment_all_items_required') }}
            </small>
            <div class="flex gap-2 mb-3" v-if="props.isClient">
                <Checkbox v-model="isConfirmed" @change="onChangeConfirm" inputId="confirmTerm" binary />
                <label for="confirmTermx">
                    {{ t('client.generalTransactionPolicy') }}
                    <span @click="onClickTerm" class="text-blue-500 cursor-pointer hover:underline">
                        {{ t('body.OrderList.viewDetails') }}
                    </span>
                </label>
            </div>
        </div>
        <Button
            :loading="loading"
            v-if="activeBuyProduct"
            :disabled="!poStore.model.cardId || !poStore.model.itemDetail.length || poStore.disableOrderButton || hasMixedVisaPayment || totalPayment <= 0 || (!isConfirmed && props.isClient)"
            iconPos="right"
            :label="t('body.OrderList.place_order_button')"
            icon="fa-solid fa-cart-shopping"
            class="w-full p-3 text-xl"
            @click="onCreateOrder"
        />

        <PaymentDetailDialog ref="paymentDetailDialogRef" />
        <PDFView v-model:visible="visibleTerm" v-if="visibleTerm" :url="termData?.filePath" :header="termData?.name" /> 
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { fnum, createPurchaseOrder, clearCartItem } from '../script';
import { usePoStore } from '../store/purchaseStore.store';
import { useToast } from 'primevue/usetoast';
import { useRouter, useRoute } from 'vue-router';
import API from '@/api/api-main';
import PaymentDetailDialog from '../dialogs/PaymentDetailDialog.vue';
import PDFView from '@/components/PDFViewer/PDFView.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const paymentDetailDialogRef = ref<InstanceType<typeof PaymentDetailDialog>>();
const activeBuyProduct = ref<boolean>(false);
const props = defineProps({
    isClient: {
        type: Boolean,
        default: false
    }
});

const toast = useToast();
const router = useRouter();
const route = useRoute();
const poStore = usePoStore();
const isConfirmed = ref(false);
const termData = ref<any>({});
const visibleTerm = ref(false);
const VISA_PAYMENT_METHOD = 'PayVisa';
const hasMixedVisaPayment = computed(() => {
    const items = poStore.model.itemDetail || [];
    const hasVisaPayment = items.some((item: any) => item.paymentMethodCode === VISA_PAYMENT_METHOD);
    const hasNonVisaPayment = items.some((item: any) => item.paymentMethodCode !== VISA_PAYMENT_METHOD);
    return hasVisaPayment && hasNonVisaPayment;
});
const totalAfterVat = computed(() => {
    if (poStore.orderSummary.totalAfterVat) return poStore.orderSummary.totalAfterVat;
    return (poStore.orderSummary.totalBeforeVat || 0) - (poStore.orderSummary.bonusCommited || 0) + (poStore.orderSummary.vatAmount || 0);
});
const totalPayment = computed(() => totalAfterVat.value - (poStore.orderSummary.bonusAmount || 0));
const isVisaOrder = computed(() => {
    const items = poStore.model.itemDetail || [];
    return items.length > 0 && items.every((item: any) => item.paymentMethodCode === VISA_PAYMENT_METHOD);
});
const totalVisaPayment = computed(() => {
    return (poStore.model.itemDetail || []).reduce((total, item: any) => {
        if (item.paymentMethodCode !== VISA_PAYMENT_METHOD) return total;

        const price = item.price || 0;
        const quantity = item.quantity || 0;
        const discount = item.discount || 0;
        const vatRate = (item.vat || 0) / 100;
        const priceAfterDiscount = item.discountType === 'C' ? price - discount : price - (price * discount) / 100;

        return total + priceAfterDiscount * quantity * (1 + vatRate);
    }, 0);
});
const getListItemValue = (item: any) => {
    return item.field === 'totalVisa' ? totalVisaPayment.value : item.value;
};
const onClickTerm = () => {
    API.get('article?Page=1&PageSize=1&Filter=status=A')
        .then((res) => {
            termData.value = res.data.item?.[0] || {};
            visibleTerm.value = true;
        })
        .catch((error) => {
            console.error(error);
        });
};

const onChangeConfirm = () => {
    localStorage.setItem('ACCEPT-PURCHASE-POLICY', JSON.stringify(isConfirmed.value));
};

const loading = ref(false);
const isSuccessResponse = (res: any) => res?.status >= 200 && res?.status < 300;
const fallbackPaymentMethods = () => ({
    PayNow: {
        paymentMethodID: 1,
        paymentMethodCode: 'PayNow',
        paymentMethodName: t('body.OrderList.cash_payment_option')
    },
    PayCredit: {
        paymentMethodID: 2,
        paymentMethodCode: 'PayCredit',
        paymentMethodName: t('body.OrderList.credit_debt_option')
    },
    PayGuarantee: {
        paymentMethodID: 3,
        paymentMethodCode: 'PayGuarantee',
        paymentMethodName: t('body.OrderList.guaranteed_debt_option')
    },
    PayVisa: {
        paymentMethodID: 4,
        paymentMethodCode: 'PayVisa',
        paymentMethodName: t('body.OrderList.visa_credit_card_option')
    }
});
const getOrderPaymentMethodCode = () => {
    const paymentCodes = Array.from(new Set((poStore.model.itemDetail || []).map((item: any) => item.paymentMethodCode).filter(Boolean)));
    if (paymentCodes.length === 1) return paymentCodes[0];
    if (paymentCodes.includes(VISA_PAYMENT_METHOD)) return VISA_PAYMENT_METHOD;
    return poStore.model.paymentMethod?.[0]?.paymentMethodCode || 'PayNow';
};
const getPaymentMethod = async (paymentMethodCode: string) => {
    try {
        const res = await API.get('PaymentMethod/getall');
        const paymentMethod = res.data?.find((item: any) => item.paymentMethodCode === paymentMethodCode);
        if (paymentMethod) {
            return {
                paymentMethodID: paymentMethod.id || paymentMethod.paymentMethodID,
                paymentMethodCode: paymentMethod.paymentMethodCode,
                paymentMethodName: paymentMethod.paymentMethodName
            };
        }
    } catch (error) {
        console.error('Error fetching payment method:', error);
    }
    const paymentMethods = fallbackPaymentMethods();
    return paymentMethods[paymentMethodCode as keyof ReturnType<typeof fallbackPaymentMethods>] || paymentMethods.PayNow;
};
const syncPaymentMethodForOrderCreation = async (payload: any) => {
    const paymentMethodCode = getOrderPaymentMethodCode();
    payload.paymentMethod = [await getPaymentMethod(paymentMethodCode)];
};

const handleOrderCreated = async (payload: any) => {
    toast.add({
        severity: 'success',
        summary: t('body.systemSetting.success_label'),
        detail: `${t('Custom.order_success')}`,
        life: 5000
    });
    clearCartItem().catch((error) => console.error('Error clearing cart:', error));
    try {
        if (payload.docType === '') {
            if (route.name === 'purchase-order-new' || route.name === 'purchase-order-copy-admin')
                await router.replace({
                    name: 'purchaseList'
                });
            else
                await router.replace({
                    name: 'hisPur'
                });
        } else if (payload.docType === 'NET') {
            if (route.name === 'hisPurNET-add')
                await router.replace({
                    name: 'hisPurNET'
                });
            else
                await router.replace({
                    name: 'orderNetList'
                });
        } else
            await router.replace({
                name: 'giftlist'
            });
    } catch (error) {
        console.error('Error redirecting after order creation:', error);
    }
};

const onCreateOrder = async () => {
    await nextTick();
    let payload: any = null;
    try {
        let bonus = poStore.orderSummary.bonusPercent * 100;
        let bonusAmount = poStore.orderSummary.bonusAmount;
        let total = poStore.orderSummary.totalAfterVat; //- bonus
        let totalBeforeVatnumber = poStore.orderSummary.totalBeforeVat;
        let vatAmount = poStore.orderSummary.vatAmount;
        payload = poStore.model.toPayload(bonus, bonusAmount, total, totalBeforeVatnumber, vatAmount);
        if (payload.address.length < 2) {
            poStore.activeIndexTab = 1;
            toast.add({
                severity: 'warn',
                detail: t('Custom.order_warning'),
                summary: t('Custom.warning'),
                life: 3000
            });
            return;
        }
        if (payload.docType !== '') payload.promotion = [];
        loading.value = true;
        await syncPaymentMethodForOrderCreation(payload);
        const res = await createPurchaseOrder(payload);
        if (!isSuccessResponse(res)) {
            throw res;
        }
        await handleOrderCreated(payload);
    } catch (error) {
        if (payload && isSuccessResponse((error as any)?.response)) {
            await handleOrderCreated(payload);
            return;
        }
        console.error(error);
        toast.add({
            severity: 'error',
            detail: (error as any)?.response?.data?.errors || t('Custom.complete_order_error'),
            summary: t('Custom.complete_order_error'),
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

const lists = computed(() => [
    {
        label: t('body.OrderList.order_value_before_discount1'),
        value: 0,
        field: 'totalBeforeVat'
    },
    {
        label: t('body.OrderList.other_promotions_value'),
        value: 0,
        field: 'TotalDiscount'
    },
    {
        label: t('body.OrderList.commitment_bonus'),
        value: 0,
        field: 'bonusCommited'
    },
    {
        label: t('body.OrderList.payment_bonus'),
        value: 0,
        field: 'bonusAmount',
        divider: true
    },
    {
        label: t('body.OrderList.order_value_after_discount'),
        value: 0,
        field: 'totalBeforeVat_bonusCommited'
    },
    {
        label: t('body.OrderList.total_tax_amount'),
        value: 0,
        field: 'vatAmount',
        divider: true
    },
    {
        label: t('body.OrderList.cash_payment_option'),
        value: 0,
        field: 'totalPayNow'
    },
    {
        label: t('body.OrderList.credit_debt_option'),
        value: 0,
        field: 'totalDebt'
    },
    {
        label: t('body.OrderList.guaranteed_debt_option'),
        value: 0,
        field: 'totalDebtGuarantee'
    },
    {
        label: t('client.visa_credit_card'),
        value: 0,
        field: 'totalVisa',
        divider: true
    }
]);

const roundNumber = (num: number) => {
    return Math.round(num * 100) / 100;
};

watch(
    () => JSON.stringify(poStore.orderSummary),
    () => {
        lists.value.map((item) => {
            if (item.field == 'totalBeforeVat_bonusCommited') {
                item.value = roundNumber(poStore.orderSummary['totalBeforeVat'] - poStore.orderSummary['bonusCommited']);
            } else if (item.field == 'totalVisa') {
                item.value = roundNumber(totalVisaPayment.value);
            } else {
                item.value = roundNumber(poStore.orderSummary[item.field as keyof typeof poStore.orderSummary] || 0);
            }
            return item;
        });
    }
);

const initialComponent = () => {
    // code here
    localStorage.getItem('ACCEPT-PURCHASE-POLICY') && (isConfirmed.value = JSON.parse(localStorage.getItem('ACCEPT-PURCHASE-POLICY') || 'false'));
    setTimeout(() => {
        activeBuyProduct.value = true;
    }, 2000);
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
