<template>
    <div v-if="odStore.order?.status === 'HUY2'" class="cancel-notification mb-2">
        <div class="cancel-card bg-red-100 flex align-items-center justify-content-between">
            <div>
                <div class="cancel-header">
                    <h3 class="cancel-title">{{ t('CancelOrder.title') }}</h3>
                </div>
                <div class="cancel-content">
                    <p class="reason-text"><strong>{{ t('ChangePoint.reason') }}:</strong> {{
                        odStore.order.reasonForCancellation }}</p>
                </div>
            </div>
        </div>
    </div> 
</template>

<script setup>
import { useOrderDetailStore } from '../store/orderDetail';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const odStore = useOrderDetailStore();
const props = defineProps({
    isClient: {
        type: Boolean,
        default: null
    }
}); 
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
