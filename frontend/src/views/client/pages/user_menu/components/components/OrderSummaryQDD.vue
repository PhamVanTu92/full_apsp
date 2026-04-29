<template>
    <div class="border-1 border-green-700 p-3 border-round bg-white">
        <div class="col-12 pb-0 formgrid text-center mb-2">
            <span>{{ t('PromotionalItems.PointConversion.currentPoints') }}</span><br>
            <b style="font-size: 30px; color: red;"> {{
                totalPoint + " " +
                t('PromotionalItems.PromotionalItems.point') }}</b>
        </div>
        <div class="flex gap-2 mb-3" v-if="props.isClient">
            <Checkbox v-model="isConfirmed" @change="onChangeConfirm" inputId="confirmTerm" binary />
            <label for="confirmTermx">
                {{ t('client.generalTransactionPolicy') }}
                <span @click="onClickTerm" class="text-blue-500 cursor-pointer hover:underline"> {{
                    t('body.OrderList.viewDetails') }} </span>
            </label>
        </div>
        <Button :loading="loading"
            :disabled="!poStore.model.cardId || !poStore.model.itemDetail.length || poStore.disableOrderButton || (!isConfirmed && props.isClient) || totalPoint < getPointPromion"
            iconPos="right" :label="t('body.OrderList.place_order_button')" icon="fa-solid fa-cart-shopping"
            class="w-full p-3 text-xl" @click="onCreateOrder" />
        <PaymentDetailDialog ref="paymentDetailDialogRef" />
        <PDFView v-model:visible="visibleTerm" v-if="visibleTerm" :url="termData?.filePath" :header="termData?.name" />
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { createPurchaseOrderQDD } from '../types/script';
import { usePoStore } from '../store/purchaseStore.store';
import { useToast } from 'primevue/usetoast';
import { useRouter } from 'vue-router';
import API from '@/api/api-main';
import PaymentDetailDialog from '../dialogs/PaymentDetailDialog.vue';
import PDFView from '@/components/PDFViewer/PDFView.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const paymentDetailDialogRef = ref<InstanceType<typeof PaymentDetailDialog>>();
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

let totalPoint = ref(0);


const getPointPromion = computed(() => {
    return poStore.model.itemDetail.reduce((sum, item) => sum + (item.exchangePoint || 0) *
        (item.quantity ||
            0), 0);
});
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
const onCreateOrder = () => {
    nextTick(() => {
        let bonus = poStore.orderSummary.bonusPercent * 100;
        let bonusAmount = poStore.orderSummary.bonusAmount;
        let total = poStore.orderSummary.totalAfterVat; //- bonus
        let totalBeforeVatnumber = poStore.orderSummary.totalBeforeVat;
        let vatAmount = poStore.orderSummary.vatAmount;
        const payload = poStore.model.toPayload(bonus, bonusAmount, total, totalBeforeVatnumber, vatAmount);
        payload.docType = 'DVP'
        payload.itemDetail.forEach(item => {
            item.priceAfterDist = 0;
            item.lineTotal = item.exchangePoint * item.quantity;
            item.price = item.exchangePoint
        });
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

        loading.value = true;
        createPurchaseOrderQDD(payload)
            .then(() => {
                toast.add({
                    severity: 'success',
                    summary: t('body.systemSetting.success_label'),
                    detail: `${t('Custom.order_success')}`,
                    life: 5000
                });
                router.push({ name: 'promotiondetail-list' });
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

watch(
    () => poStore.model?._customer?.customerPoints,
    () => { 
        getPointUser()
    }
)

const getPointUser = () => {
    let date = new Date();
    poStore.model?._customer?.customerPoints?.forEach(point => { 
        if (point.expiryDate && new Date(point.expiryDate) > date) {
            totalPoint.value += point.remainingPoint
        } else if (new Date(point.endDate) > date) {
            totalPoint.value += point.remainingPoint
        }
    }) 
    return totalPoint.value
};
const initialComponent = () => {
    // code here
    localStorage.getItem('ACCEPT-PURCHASE-POLICY') && (isConfirmed.value = JSON.parse(localStorage.getItem('ACCEPT-PURCHASE-POLICY') || 'false'));

    // setTimeout(() => { 
    //     getPointUser()
    // }, 1000);
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
:deep .p-inputnumber-input {
    width: 6rem;
}
</style>
