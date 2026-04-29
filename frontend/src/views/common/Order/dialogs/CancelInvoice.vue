<template>
    <div>
        <Dialog v-model:visible="visible" class="w-30rem" modal @hide="onHide" :closable="true" :draggable="false">
            <template #header>
                <div class="dialog-header">
                    <div class="header-icon">
                        <i class="pi pi-exclamation-triangle text-orange-500"></i>
                    </div>
                    <div class="header-content">
                        <h3 class="header-title">{{ t('Custom.confirm_cancel_oder') }}</h3>
                        <p class="header-subtitle">{{ t('Custom.enter_reason') }}</p>
                    </div>
                </div>
            </template>

            <div class="dialog-content">
                <div class="form-group">
                    <label for="cancelReason" class="form-label">
                        <i class="pi pi-comment mr-2"></i>
                        {{ t('Custom.reason') }}
                        <span class="required-star">*</span>
                    </label>
                    <Textarea id="cancelReason" v-model="cancelReason" class="cancel-reason-input"
                        placeholder="Nhập lý do hủy đơn hàng..." rows="4" :autoResize="false" :maxlength="1000"
                        :class="{ 'p-invalid': showError && !cancelReason }" />
                    <small class="character-count">{{ cancelReason.length }}/1000 {{ t('Custom.characters') }}</small>
                    <small v-if="showError && !cancelReason" class="error-message">
                        <i class="pi pi-exclamation-circle mr-1"></i>
                        {{ t('Custom.reason_required') }}
                    </small>
                </div>
            </div>

            <template #footer>
                <div class="flex justify-content-end gap-2 w-full">
                    <Button :label="t('Logout.cancel')" @click="visible = false" icon="pi pi-times"
                        severity="secondary"/>
                    <Button :label="t('client.confirm')" icon="pi pi-check" @click="ConfirmCancel"/>
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetail';
import API from '@/api/api-main';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const odStore = useOrderDetailStore();
const toast = useToast();
const visible = ref(false);
const cancelReason = ref('');
const showError = ref(false);
const loading = ref(false);

const onHide = () => {
    cancelReason.value = '';
    showError.value = false;
    loading.value = false;
};

const open = () => {
    visible.value = true;
    cancelReason.value = '';
    showError.value = false;
    loading.value = false;
};

defineExpose({ open });

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});

const ConfirmCancel = async () => {
    if (!cancelReason.value.trim()) {
        showError.value = true;
        return;
    }
    try {
        loading.value = true;
        try {
            await API.update(`${odStore?.order?.docType == 'NET'? "PurchaseOrderNet/" : 'PurchaseOrder/'}${odStore?.order?.id}/change-status/HUY2?reasonForCancellation=${cancelReason.value}`);
            toastService('success', t('body.systemSetting.success_label'), t('Custom.success_cancel_oder'));
            visible.value = false;
            odStore.fetchStore();
            window.location.reload();
        } catch (error) {
            console.error(error);
        }
    } catch (error) {
        toastService('error', t('Custom.error'), t('Custom.error_cancel_oder'));
    } finally {
        loading.value = false;
    }
};

const toastService = (severity: 'error' | 'secondary' | 'success' | 'info' | 'contrast' | 'warn', smr: string, detail: string, life: number = 3000) => {
    toast.add({
        severity: severity,
        detail: detail,
        summary: smr,
        life: life
    });
};
</script>

<style scoped>
.cancel-invoice-dialog {
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

.dialog-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 0.5rem 0;
}

.header-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 3rem;
    height: 3rem;
    background: linear-gradient(135deg, #fef3cd, #fff3cd);
    border-radius: 50%;
    border: 2px solid #fbbf24;
}

.header-icon i {
    font-size: 1.25rem;
}

.header-content {
    flex: 1;
}

.header-title {
    margin: 0 0 0.25rem 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--text-color);
}

.header-subtitle {
    margin: 0;
    font-size: 0.875rem;
    color: var(--text-color-secondary);
    line-height: 1.4;
}

.dialog-content {
    padding: 1.5rem 0;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}

.form-label {
    display: flex;
    align-items: center;
    font-weight: 500;
    color: var(--text-color);
    font-size: 0.9rem;
}

.required-star {
    color: #ef4444;
    margin-left: 0.25rem;
}

.cancel-reason-input {
    border-radius: 8px;
    border: 2px solid var(--surface-border);
    transition: all 0.2s ease;
    font-size: 0.9rem;
    padding: 0.75rem;
    resize: none;
}

.cancel-reason-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb), 0.1);
}

.cancel-reason-input.p-invalid {
    border-color: #ef4444;
    background-color: #fef2f2;
}

.cancel-reason-input.p-invalid:focus {
    border-color: #ef4444;
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.character-count {
    text-align: right;
    color: var(--text-color-secondary);
    font-size: 0.75rem;
    margin-top: -0.5rem;
}

.error-message {
    color: #ef4444;
    font-size: 0.8rem;
    display: flex;
    align-items: center;
    margin-top: -0.5rem;
}

.dialog-footer {
    display: flex;
    gap: 0.75rem;
    padding: 0.5rem 0 0 0;
}

.cancel-button {
    flex: 1;
    border-radius: 8px;
    padding: 0.75rem 1.5rem;
    font-weight: 500;
    transition: all 0.2s ease;
}

.cancel-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.confirm-button {
    flex: 1;
    border-radius: 8px;
    padding: 0.75rem 1.5rem;
    font-weight: 500;
    background: linear-gradient(135deg, #ef4444, #dc2626);
    border: none;
    color: white;
    transition: all 0.2s ease;
}

.confirm-button:hover:not(:disabled) {
    background: linear-gradient(135deg, #dc2626, #b91c1c);
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
}

.confirm-button:disabled {
    background: var(--surface-300);
    color: var(--text-color-secondary);
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

/* Animation cho dialog */
:deep(.p-dialog) {
    animation: dialogShow 0.3s ease-out;
}

@keyframes dialogShow {
    from {
        opacity: 0;
        transform: scale(0.9) translateY(-20px);
    }

    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}

/* Responsive */
@media (max-width: 768px) {
    .cancel-invoice-dialog {
        width: 95vw !important;
        margin: 1rem;
    }

    .dialog-header {
        flex-direction: column;
        text-align: center;
        gap: 0.75rem;
    }

    .header-icon {
        align-self: center;
    }

    .dialog-footer {
        flex-direction: column;
    }
}
</style>
