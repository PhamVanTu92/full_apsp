<template>
    <div class="border-1 border-green-700 p-3 border-round bg-white">
        <template v-for="(item, i) in lists" :key="i">
            <div class="flex justify-content-between my-3">
                <span v-html="item.label"></span>
                <span v-if="item.field != 'bonusAmount'">{{ poStore.model.currency == 'USD' ? fnum(item.value, 2) :
                    fnum(item.value, 0) }}</span>
                <span v-else class="text-green-600">
                    <InputNumber v-model="poStore.orderSummary.bonusAmount" :min="0" :maxFractionDigits="2"
                        class="text-green-600" />
                </span>
            </div>
            <Divider v-if="item.divider"> </Divider>
        </template>
        <div class="mt-5">
            <div class="flex justify-content-between text-lg">
                <span class="font-bold"> {{ t('body.OrderList.total_payment') }}</span>
                <span class="font-bold">{{ fnum(poStore.orderSummary.totalAfterVat - poStore.orderSummary.bonusAmount,
                    2)
                }}</span>
            </div>
            <div class="mb-3">
                <Button @click="paymentDetailDialogRef?.open()" :label="t('body.OrderList.viewDetails')" text
                    severity="info" class="p-0 font-light" size="small" v-if="poStore.model.itemDetail.length"/>
                <div v-else class="mb-5 pb-1"></div>
            </div>
            <div class="flex gap-2 mb-3" v-if="props.isClient">
                <Checkbox v-model="isConfirmed" @change="onChangeConfirm" inputId="confirmTerm" binary></Checkbox>
                <label for="confirmTermx">
                    {{ t('client.generalTransactionPolicy') }}
                    <span @click="onClickTerm" class="text-blue-500 cursor-pointer hover:underline"> {{
                        t('body.OrderList.viewDetails') }} </span>
                </label>
            </div>
        </div> 
        <Button :loading="loading"
            :disabled="!poStore.model.cardId || !poStore.model.itemDetail.length || poStore.disableOrderButton  || (!isConfirmed && props.isClient) || poStore.orderSummary.totalAfterVat - poStore.orderSummary.bonusAmount < 0"
            iconPos="right" :label="t('body.OrderList.place_order_button')" icon="fa-solid fa-cart-shopping"
            class="w-full p-3 text-xl" @click="onCreateOrder"/>
        <PaymentDetailDialog ref="paymentDetailDialogRef"></PaymentDetailDialog>
        <PDFView v-model:visible="visibleTerm" v-if="visibleTerm" :url="termData?.filePath" :header="termData?.name">
        </PDFView>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { fnum, createPurchaseOrder, clearCartItem, createPurchaseOrderNET } from '../script';
import { usePoStore } from '../store/purchaseStore.store';
import { useToast } from 'primevue/usetoast';
import { useRouter } from 'vue-router';
import API from '@/api/api-main';
//Components
import PaymentDetailDialog from '../dialogs/PaymentDetailDialog.vue';
const paymentDetailDialogRef = ref<InstanceType<typeof PaymentDetailDialog>>();
import PDFView from '@/components/PDFViewer/PDFView.vue';

import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const props = defineProps({
    isClient: {
        type: Boolean,
        default: false
    }
});

const toast = useToast();
const router = useRouter();
const poStore = usePoStore();
const isConfirmed = ref(false);

const termData = ref<any>({});
const visibleTerm = ref(false);
const onClickTerm = () => {
    API.get('article?Page=1&PageSize=1&Filter=status=A')
        .then((res) => {
            termData.value = res.data.item?.[0] || {};
            visibleTerm.value = true;
        })
        .catch((error) => { });
};

const onChangeConfirm = () => {
    localStorage.setItem('ACCEPT-PURCHASE-POLICY', JSON.stringify(isConfirmed.value));
};

const loading = ref(false);
const onCreateOrder = () => {
    nextTick(() => {
        let bonus = poStore.orderSummary.bonusPercent * 100;
        let bonusAmount = poStore.orderSummary.bonusAmount;
        let total = poStore.orderSummary.totalAfterVat; //- bonus
        let totalBeforeVatnumber = poStore.orderSummary.totalBeforeVat;
        let vatAmount = poStore.orderSummary.vatAmount;
        const payload = poStore.model.toPayload(bonus, bonusAmount, total, totalBeforeVatnumber, vatAmount);

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

        // loading.value = true; 
        
        createPurchaseOrderNET(payload)
            .then(() => {
                toast.add({
                    severity: 'success',
                    summary: t('body.systemSetting.success_label'),
                    detail: `${t('Custom.order_success')}`,
                    life: 5000
                });
                if (props.isClient) {
                    clearCartItem();
                    router.replace({
                        name: 'hisPur'
                    });
                } else {
                    router.replace({
                        name: 'orderNetList'
                    });
                }
            })
            .catch((error) => {
                console.error(error);
                const errorMessage = error?.response?.data?.message || error?.message || t('Custom.errorOccurred');
                toast.add({
                    severity: 'error',
                    summary: t('Custom.error'),
                    detail: errorMessage,
                    life: 5000
                });
            })
            .finally(() => {
                loading.value = false;
             
            });
    });
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
        field: 'totalDebtGuarantee',
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
};

onMounted(function () {
    initialComponent();
});
</script> 
