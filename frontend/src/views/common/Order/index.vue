<template>
    <div>
        <div class="flex justify-content-between mb-2 align-items-center">
            <h4 class="mb-0 font-bold">
                {{ !odStore?.order?.docType ? t('client.detail') : odStore?.order?.docType == 'NET' ? t('client.detailNET') : t('client.detailVPKM') }}
            </h4>
            <div class="flex gap-2">
                <Button :label="t('CancelOrder.copyOrder')" icon="pi pi-copy" severity="warn" @click="visible = true" />
                <Button @click="openExportFile" :label="t('client.export_files')" icon="pi pi-file-export" />
                <Button :label="t('Evaluate.title')" icon="pi pi-star" severity="success" @click="ratingDialogVisible = true" v-if="odStore?.order?.status == 'DHT' && odStore?.order?.ratings?.length == 0 && props.isClient" />
                <ButtonGoBack v-if="!props.isClient" />
            </div>
        </div>
        <ExportFiles :data="props.isClient" ref="ExportFileRef" :type="'ORDER'" />

        <template v-if="odStore.error">
            <div class="flex justify-content-center align-items-center flex-column pt-8">
                <Image src="../../../../public/image/order/no-order.png" />
                <div class="font-bold text-2xl mb-2">{{ t('client.notFoundTitle') }}</div>
                <p style="max-width: 40rem; text-align: center">
                    {{ t('client.notFoundMessage') }}
                </p>
            </div>
        </template>
        <template v-else>
            <CancelReason :isClient="props.isClient" />
            <Header />
            
            <div class="card">
                <!-- <div class="flex justify-content-end mb-4" v-if="odStore?.order?.status == 'DHT'">
                    <Button 
                        icon="pi pi-history" 
                        :label="t('body.ReturnRequestList.FromOrder.buttonLabel')" 
                        @click="handleReturnRequest" 
                        class="px-4 py-2 bg-orange-500 border-none hover:bg-orange-600 transition-colors shadow-2"
                        iconClass="mr-2"
                    />
                </div> -->
                <ProductList />
                <div class="grid mt-3">
                    <div class="col-12 md:col-8 flex flex-column gap-3">
                        <Promotion />
                        <Notes :is-client="props.isClient" />
                    </div>
                    <div class="col-12 md:col-4">
                        <OrderSummary />
                    </div>
                </div>
                <Buttons :is-client="props.isClient" />
            </div>
            <div class="card">
                <TaiLieuBoSung :is-client="props.isClient" />
            </div>
            <div class="card">
                <ChungTuGiaoHang :is-client="props.isClient" />
            </div>
            <div class="card" v-if="odStore?.order?.status == 'DHT' && odStore?.order?.ratings?.length > 0">
                <DanhGiaDonHang :type="'preview'" :is-client="props.isClient" :initialData="odStore?.order?.ratings" />
            </div>
            <div class="card">
                <ThongTin />
            </div>
        </template>
        <Dialog v-model:visible="visible" modal :header="t('CancelOrder.confirmHeader')" :style="{ width: '28rem' }">
            <div class="confirmation-content align-items-center gap-3">
                <div class="flex align-items-center gap-3">
                    <i class="pi pi-question-circle text-4xl text-blue-500"></i>
                    <h4 class="confirmation-title">{{ t('CancelOrder.copyOrder') }}</h4>
                </div>
                <p class="confirmation-text">{{ t('CancelOrder.confirmText') }}</p>
            </div>
            <template #footer>
                <div class="flex justify-content-end gap-2">
                    <Button type="button" :label="t('Notification.cancel')" severity="secondary" @click="visible = false" />
                    <!-- <Button type="button" :label="t('Notification.confirm')" @click="handleConfirm" /> -->
                    <Button type="button" :label="t('Notification.confirm')" @click="handleConfirmCopy" />
                </div>
            </template>
        </Dialog>
        <Dialog v-model:visible="ratingDialogVisible" modal :header="t('Evaluate.evaluateOrder')" :style="{ width: '50rem' }">
            <DanhGiaDonHang :type="'vote'" :orderId="odStore?.order?.id" :docType="odStore?.order?.docType || ''" />
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
//Components
import Header from './components/Header.vue';
import OrderSummary from './components/OrderSummary.vue';
import ProductList from './components/ProductList.vue';
import Promotion from './components/Promotion.vue';
import Notes from './components/Notes.vue';
import Buttons from './components/Buttons.vue';
import TaiLieuBoSung from './components/TaiLieuBoSung.vue';
import ChungTuGiaoHang from './components/ChungTuGiaoHang.vue';
import DanhGiaDonHang from './components/DanhGiaDonHang.vue';
import ThongTin from './components/ThongTin.vue';
import ExportFiles from './dialogs/ExportFiles.vue';
import CancelReason from './components/CancelReason.vue';
import { useRouter } from 'vue-router';
import { useOrderDetailStore } from './store/orderDetail';
import { useI18n } from 'vue-i18n'; 
const { t } = useI18n();
const visible = ref(false);
const ratingDialogVisible = ref(false);
const odStore = useOrderDetailStore();
const ExportFileRef = ref();
const router = useRouter(); 
const props = defineProps({
    isClient: {
        type: Boolean,
        required: false,
        default: false
    }
});

const route = useRoute();

const initialComponent = () => {
    const orderId = route.params.id;
    if (orderId && typeof orderId === 'string' && isInteger(orderId)) {
        // fetchOrderById(parseInt(orderId));
        odStore.fetchStore(parseInt(orderId));
    } else {
        odStore.error = true;
    }
};

function isInteger(value: any) {
    return /^\d+$/.test(value);
}
const handleConfirm = async () => {
    if (!odStore?.order?.docType) {
        let str = JSON.stringify(convertDataPayload(odStore.order));
        let data = btoa(encodeURIComponent(str));
        if (!props.isClient) return router.push(`/purchase-order-new?invoice=${data}&customerId=${odStore.customer.id}`);
        return router.push(`/client/order/new-order?invoice=${data}&customerId=${odStore.customer.id}`);
    } else if (odStore?.order?.docType == 'VPKM') {
        if (!props.isClient) return router.push(`/gift-request/detail/${odStore.order.id}`);
        // return router.push(`/client/order/new-order?invoice=${data}&customerId=${odStore.customer.id}`);
    } else {
        if (!props.isClient) {
            return router.push(`/order-net/copy/${odStore.order.id}`);
        }
    }
};

const handleConfirmCopy = () => {
    let docType = odStore?.order?.docType || '';
    let id = odStore?.order?.id || 0; 
    if (!props.isClient) {
        if (docType == '') return router.push(`/purchase-order/purchase-order-copy/${id}`);
        if (docType == 'NET') return router.push(`/order-net/copy/${id}`);
        if (docType == 'VPKM') return router.push(`/gift-request/detail/${id}`);
    } else if (docType == '') {
        return router.push(`/client/order/purchase-order-copy/${id}`);
    }
};

const handleReturnRequest = () => {
    const orderId = odStore.order?.id;
    if (!orderId) return;
    
    if (props.isClient) {
        router.push({ 
            name: 'client-return-request-from-order', 
            params: { id: orderId } 
        });
    } else {
        router.push({ 
            name: 'returnRequestFromOrder', 
            params: { id: orderId } 
        });
    }
};

const convertDataPayload = (data: any) => {
    if (!data) return null;
    const { id, author, ...rest } = data;
    // Xử lý itemDetail nếu tồn tại
    if (rest.itemDetail && Array.isArray(rest.itemDetail)) {
        const newItemDetail = rest.itemDetail.map((el: any) => {
            const { id: itemId, fatherId: fatherId, ...itemRest } = el;
            return {
                ...itemRest
            };
        });
        const { id: paymentId, fatherId: paymentFatherId, docId: paymentDocId, ...newPaymentInfo } = rest.paymentInfo || {};
        const newPaymentMed =
            rest.paymentMethod?.map((el: any) => {
                const { id, fatherId, ...restPayment } = el;
                return {
                    ...restPayment
                };
            }) || [];
        const newPromotion =
            rest.promotion?.map((el: any) => {
                const { id, fatherId, ...restPromotion } = el;
                return {
                    ...restPromotion
                };
            }) || [];
        const newAddress =
            rest.address?.map((el: any) => {
                const { id, fatherId, ...rest } = el;
                return rest;
            }) || [];
        const newTracking =
            rest.tracking?.map((el: any) => {
                const { id, fatherId, ...rest } = el;
                return rest;
            }) || [];
        return {
            ...rest,
            itemDetail: newItemDetail,
            paymentInfo: newPaymentInfo,
            paymentMethod: newPaymentMed,
            promotion: newPromotion,
            address: newAddress,
            tracking: newTracking
        };
    }

    return rest;
};

onMounted(function () {
    initialComponent();
});
const openExportFile = () => {
    ExportFileRef.value?.open();
};
</script>
