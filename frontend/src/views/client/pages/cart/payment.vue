<template>
    <div class="payment-container">
        <div class="payment-card">
            <!-- Tiêu đề và thời gian đếm ngược -->
            <div class="header">
                <h3>{{ t('paymentPage.selectPaymentMethod') }}</h3>
                <div class="timer">
                    <i class="pi pi-clock"></i>
                    <span :class="{ 'text-red-600': remainingTime <= 60 }">
                        {{ formatTime(remainingTime) }}
                    </span>
                </div>
            </div>

            <!-- Danh sách phương thức thanh toán -->
            <div class="payment-options">
                <div
                    v-for="method in paymentMethods"
                    :key="method.id"
                    class="payment-option cursor-pointer"
                    :class="{
                        selected: selectedMethod === method.id,
                        disabled: method.disable
                    }"
                >
                    <div class="flex items-center w-full" :class="{ 'opacity-50': method.disable }" @click="!method.disable && selectMethod(method.id)">
                        <RadioButton v-model="selectedMethod" :inputId="method.id" :value="method.id" :disabled="method.disable" class="mr-3" />

                        <label :for="method.id" class="flex flex-1" style="align-items: center" :class="{ 'cursor-pointer': !method.disable }">
                            <i :class="method.icon" class="mr-3 text-xl"></i>
                            <span>{{ method.label }}</span>

                            <div v-if="method.id === 'INTCARD'" class="flex gap-2 pl-2">
                                <img src="@/assets/images/VISA-logo.png" style="width: 40px" />
                                <img src="@/assets/images/Mastercard-logo.svg.webp" style="width: 30px" />
                                <img src="@/assets/images/American-Express-Color.png" style="width: 40px" />
                                <img src="@/assets/images/JCB_logo.svg.png" style="width: 30px" />
                            </div>
                        </label>
                    </div>
                </div>
            </div>

            <!-- Thông tin đơn hàng -->
            <div class="order-summary">
                <h4>{{ t('paymentPage.orderDetails') }}</h4>
                <div class="info-row">
                    <span>{{ t('paymentPage.paymentAcceptanceUnit') }}</span>
                    <strong>VNPay</strong>
                </div>
                <div class="info-row">
                    <span>{{ t('paymentPage.orderCode') }}</span>
                    <strong>{{ orderDetail?.invoiceCode }}</strong>
                </div>
                <div class="info-row total">
                    <span>{{ t('paymentPage.paymentAmount') }}</span>
                    <strong class="text-primary text-xl">
                        {{ format.FormatCurrency(orderDetail?.totalPaymentAmount) }}
                    </strong>
                </div>
            </div>
            <div class="actions flex gap-2 w-full">
                <Button :label="t('paymentPage.back')" class="whitespace-nowrap w-full" text severity="secondary" @click="cancelPayment" />
                <Button :label="t('paymentPage.confirm')" class="w-full" severity="primary" :disabled="!selectedMethod" @click="confirmPayment" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import API from '@/api/api-main';
import format from '@/helpers/format.helper';
import { useGlobal } from '@/services/useGlobal';
import { useI18n } from 'vue-i18n';

const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();

const VISA_PAYMENT_METHOD = 'PayVisa';
const CARD_PAYMENT_TYPE = 'INTCARD';

const Router = useRoute();
const initialTime = 5 * 60;
const remainingTime = ref(initialTime);
let timer = ref();
const selectedMethod = ref<string | null>(null);

const orderDetail = ref<object | any>({
    invoiceCode: 0,
    totalPaymentAmount: 0
});

const RouteS = useRouter();
const paymentMethods = ref([
    {
        id: CARD_PAYMENT_TYPE,
        label: t('paymentPage.creditDebitCard'),
        icon: 'pi pi-credit-card',
        disable: true
    },
    {
        id: 'VNBANK',
        label: t('paymentPage.atmBankAcc'),
        icon: 'pi pi-money-bill',
        disable: false
    },
    {
        id: 'VNPAYQR',
        label: t('paymentPage.qrPaymentMobileApp'),
        icon: 'pi pi-qrcode',
        disable: false
    }
]);

const applyPaymentMethodAvailability = (isVisaOrder: boolean) => {
    paymentMethods.value = paymentMethods.value.map((method) => ({
        ...method,
        disable: isVisaOrder ? method.id !== CARD_PAYMENT_TYPE : method.id === CARD_PAYMENT_TYPE
    }));

    if (paymentMethods.value.some((method) => method.id === selectedMethod.value && method.disable)) {
        selectedMethod.value = null;
    }
};

const isAllVisaOrder = (order: any) => {
    const items = Array.isArray(order?.itemDetail) ? order.itemDetail : [];
    return items.length > 0 && items.every((item: any) => item.paymentMethodCode === VISA_PAYMENT_METHOD);
};

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

const getTotalPaymentAmount = (order: any) => {
    const paymentInfo = order?.paymentInfo || {};
    const storedTotalAfterVat = toNumber(paymentInfo.totalAfterVat);
    const bonusAmount = toNumber(paymentInfo.bonusAmount);

    if (storedTotalAfterVat) return storedTotalAfterVat - bonusAmount;

    const totalBeforeVat = toNumber(paymentInfo.totalBeforeVat) || (order?.itemDetail || []).reduce((total: number, item: any) => total + getLineBeforeVat(item), 0);
    const bonusCommited = toNumber(paymentInfo.bonusCommited);
    const vatAmount = toNumber(paymentInfo.vatAmount) || (order?.itemDetail || []).reduce((total: number, item: any) => total + getLineVat(item), 0);

    return totalBeforeVat - bonusCommited + vatAmount - bonusAmount;
};

const selectMethod = (id: any) => {
    selectedMethod.value = id;
};

const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
};

const startTimer = () => {
    timer.value = setInterval(() => {
        if (remainingTime.value > 0) {
            remainingTime.value--;
        } else {
            clearInterval(timer.value!);
            timer.value = null;
            cancelPayment();
        }
    }, 1000);
};

const confirmPayment = async () => {
    try {
        if (!selectedMethod.value) return;
        let payload = {
            docId: orderDetail.value.id,
            paymentType: selectedMethod.value
        };
        let res = await API.add(`VnpayPayment`, payload);
        if (res.data) window.location.href = res.data;
    } catch (err) {
        FunctionGlobal.$notify('E', t('paymentPage.errorOccurred'), toast);
        console.error(err);
    }
};

const cancelPayment = () => {
    RouteS.back();
};

const getDetailOrder = async () => {
    try {
        let url: string = '';

        switch (Router.params.type) {
            case 'ORDER':
                url = `PurchaseOrder/${Router.params.id}`;
                break;
            case 'NET':
                url = `PurchaseOrderNet/${Router.params.id}`;
                break;
            case 'VPKM':
                url = `Redeem/${Router.params.id}`;
                break;
            case 'YCLHG':
                url = `PurchaseRequest/${Router.params.id}`;
                break;
            case 'RETURN':
                url = `PurchaseReturn/${Router.params.id}`;
                break;
            default:
                break;
        }

        if (!url) {
            return;
        }

        const res = await API.get(url);
        if (res.data) {
            const order = res.data.item;
            orderDetail.value = {
                totalPaymentAmount: getTotalPaymentAmount(order),
                invoiceCode: order.invoiceCode,
                id: order.id
            };
            applyPaymentMethodAvailability(isAllVisaOrder(order));
        }
    } catch (error) {
        console.error('Error fetching order details:', error);
    }
};

watch(remainingTime, (newValue) => {
    if (newValue === 0) {
        cancelPayment();
    }
});

onMounted(() => {
    startTimer();
    getDetailOrder();
});

onUnmounted(() => {
    if (timer.value) clearInterval(timer.value);
});
</script>

<style>
@import url('@/assets/stylePaymentMethod.css');
</style>
