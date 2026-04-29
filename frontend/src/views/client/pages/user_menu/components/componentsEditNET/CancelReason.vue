<template>
    <div v-if="odStore.order?.status === 'HUY2'" class="cancel-notification mb-2">
        <div class="cancel-card bg-red-100 flex align-items-center justify-content-between">
            <div>
                <div class="cancel-header">
                    <h3 class="cancel-title">Đơn hàng đã bị hủy</h3>
                </div>
                <div class="cancel-content">
                    <p class="reason-text"><strong>Lý do:</strong> {{ odStore.order.reasonForCancellation }}</p>
                </div>
            </div>
        </div>
    </div>
    <!-- <div class="flex align-content-center justify-content-end my-2">
        <Button @click="openCopyDialog()" label="Nhân bản đơn hàng" icon="pi pi-copy" severity="info"/>
    </div> -->

    <Dialog
        v-model:visible="visible"
        modal
        header="Xác nhận"
        :style="{ width: '28rem' }"
        :pt="{
            root: { class: 'modern-dialog' },
            header: { class: 'modern-dialog-header' },
            content: { class: 'modern-dialog-content' },
            footer: { class: 'modern-dialog-footer' }
        }"
    >
        <div class="confirmation-content">
            <div class="confirmation-icon">
                <i class="pi pi-question-circle text-4xl text-blue-500"></i>
            </div>
            <div class="confirmation-message">
                <h4 class="confirmation-title">Nhân bản đơn hàng</h4>
                <p class="confirmation-text">Bạn có chắc chắn muốn tạo bản sao của đơn hàng này không?</p>
                <p class="confirmation-note">Đơn hàng mới sẽ được tạo với các thông tin giống như đơn hàng hiện tại.</p>
            </div>
        </div>

        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button type="button" label="Hủy" severity="secondary" @click="visible = false"/>
                <Button type="button" label="Xác nhận" @click="handleConfirm"/>
            </div>
        </template>
    </Dialog>
</template>

<script setup>
import { ref } from 'vue';
import { useOrderDetailStore } from '../store/orderDetailNET'; 
import { useRouter } from 'vue-router'; 
const odStore = useOrderDetailStore(); 
const visible = ref(false); 
const props = defineProps({
    isClient: {
        type: Boolean,
        default: null
    }
});
const router = useRouter();
const handleConfirm = async () => {

    let str = JSON.stringify(convertDataPayload(odStore.order));
    let data = btoa(encodeURIComponent(str));
    if (!props.isClient) {
        return router.push(`/purchase-order-new?invoice=${data}&customerId=${odStore.customer.id}`);
    }
    return router.push(`/client/order/new-order?invoice=${data}&customerId=${odStore.customer.id}`);
};
const convertDataPayload = (data) => {
    if (!data) return null;

    const { id, author, ...rest } = data;

    // Xử lý itemDetail nếu tồn tại
    if (rest.itemDetail && Array.isArray(rest.itemDetail)) {
        const newItemDetail = rest.itemDetail.map((el) => {
            const { id: itemId, fatherId: fatherId, ...itemRest } = el;
            return {
                ...itemRest
            };
        });
        const { id: paymentId, fatherId: paymentFatherId, docId: paymentDocId, ...newPaymentInfo } = rest.paymentInfo || {};
        const newPaymentMed =
            rest.paymentMethod?.map((el) => {
                const { id, fatherId, ...restPayment } = el;
                return {
                    ...restPayment
                };
            }) || [];
        const newPromotion =
            rest.promotion?.map((el) => {
                const { id, fatherId, ...restPromotion } = el;
                return {
                    ...restPromotion
                };
            }) || [];
        const newAddress =
            rest.address?.map((el) => {
                const { id, fatherId, ...rest } = el;
                return rest;
            }) || [];
        const newTracking =
            rest.tracking?.map((el) => {
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
</script>

<style scoped>
.cancel-notification {
    margin: 12px 0;
}

.cancel-card {
    background: #fecaca;
    border-radius: 8px;
    padding: 12px;
    border-left: 4px solid #ef4444;
}

.cancel-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
}

.cancel-title {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: #7f1d1d;
}

.cancel-content {
    margin: 0;
}

.reason-text {
    margin: 0;
    font-size: 14px;
    color: #7f1d1d;
    line-height: 1.4;
}

.reason-text strong {
    font-weight: 600;
}

/* Responsive */
@media (max-width: 768px) {
    .cancel-card {
        padding: 16px;
        margin: 12px;
    }

    .cancel-title {
        font-size: 15px;
    }

    .cancel-icon {
        width: 28px;
        height: 28px;
    }

    .reason-text {
        font-size: 13px;
    }
}

/* Modern Dialog Styles */
:deep(.modern-dialog) {
    border-radius: 12px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    backdrop-filter: blur(16px);
    animation: modalSlideIn 0.3s ease-out;
}

:deep(.modern-dialog-header) {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 12px 12px 0 0;
    padding: 1.5rem;
    font-weight: 600;
    font-size: 1.1rem;
}

:deep(.modern-dialog-content) {
    padding: 2rem;
    background: white;
}

:deep(.modern-dialog-footer) {
    padding: 1.5rem 2rem;
    background: #f8fafc;
    border-radius: 0 0 12px 12px;
    border-top: 1px solid #e2e8f0;
}

.confirmation-content {
    display: flex;
    align-items: flex-start;
    gap: 1.5rem;
}

.confirmation-icon {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 60px;
    height: 60px;
    background: linear-gradient(135deg, #3b82f6, #1d4ed8);
    border-radius: 50%;
    color: white;
    box-shadow: 0 10px 25px rgba(59, 130, 246, 0.3);
}

.confirmation-icon i {
    color: white !important;
}

.confirmation-message {
    flex: 1;
}

.confirmation-title {
    margin: 0 0 1rem 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: #1e293b;
    line-height: 1.3;
}

.confirmation-text {
    margin: 0 0 0.75rem 0;
    font-size: 1rem;
    color: #475569;
    line-height: 1.5;
}

.confirmation-note {
    margin: 0;
    font-size: 0.875rem;
    color: #64748b;
    line-height: 1.4;
    font-style: italic;
}

.confirmation-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.cancel-btn {
    transition: all 0.2s ease;
}

.cancel-btn:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.confirm-btn {
    background: linear-gradient(135deg, #10b981, #059669);
    border: none;
    transition: all 0.2s ease;
}

.confirm-btn:hover {
    background: linear-gradient(135deg, #059669, #047857);
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
}

@keyframes modalSlideIn {
    from {
        opacity: 0;
        transform: translateY(-20px) scale(0.95);
    }
    to {
        opacity: 1;
        transform: translateY(0) scale(1);
    }
}

/* Responsive modal */
@media (max-width: 640px) {
    :deep(.modern-dialog) {
        width: 90vw !important;
        margin: 1rem;
    }

    .confirmation-content {
        flex-direction: column;
        text-align: center;
        gap: 1rem;
    }

    .confirmation-actions {
        flex-direction: column-reverse;
    }

    .confirmation-actions .p-button {
        width: 100%;
        justify-content: center;
    }
}
</style>
