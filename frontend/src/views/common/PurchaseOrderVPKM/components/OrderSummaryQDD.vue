<template>
    <div class="border-1 border-green-700 p-3 border-round bg-white"> 
        <Button :loading="loading" :disabled="checkPointExceed" iconPos="right"
            :label="t('body.OrderList.place_order_button')" icon="fa-solid fa-cart-shopping" class="w-full p-3 text-xl"
            @click="onCreateOrder" />
        <PaymentDetailDialog ref="paymentDetailDialogRef" />
        <PDFView v-model:visible="visibleTerm" v-if="visibleTerm" :url="termData?.filePath" :header="termData?.name">
        </PDFView>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { createPurchaseOrderQDD, clearCartItem } from '../script';
import { usePoStore } from '../store/purchaseStore.store';
import { useToast } from 'primevue/usetoast';
import { useRouter } from 'vue-router';
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
                clearCartItem();
                router.push({ name: 'promotiondetail' });

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

const checkPointExceed = computed(() => {
    let totalPoint = poStore.model?._customer?.customerPoints?.reduce((sum, item) => {
        const dateToCheck = item.expiryDate ? new Date(item.expiryDate) : new Date(item.endDate);
        const isValid = dateToCheck > new Date();
        const pointsToAdd = isValid ? item.remainingPoint : 0;
        return sum + pointsToAdd;
    }, 0);
    let requiredPoint = poStore.model.itemDetail.reduce((acc, item) => acc + item.exchangePoint * item.quantity, 0) || 0; 
    return requiredPoint > totalPoint ? true : false;
});

const initialComponent = () => {
    localStorage.getItem('ACCEPT-PURCHASE-POLICY') && (isConfirmed.value = JSON.parse(localStorage.getItem('ACCEPT-PURCHASE-POLICY') || 'false'));
};

onMounted(function () {
    initialComponent();
});
</script>
