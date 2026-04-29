<template>
    <div>
        <hr v-if="buttons.some((item) => item.visible)" />
        <div class="flex gap-2 justify-content-end">
            <template v-for="(btn, i) in buttons.filter((btn) => btn.visible)" :key="i">
                <Button :icon="btn.icon" :label="btn.label" :severity="btn.severity" :disabled="btn.disabled" @click="
                    async () => {
                        await onButtonClick(i, btn.onClick);
                    }
                " :loading="loading[i]" />
            </template>
        </div>
        <YCBSDialog ref="YCBSDialogRef" />
        <HinhThucThanhToanDialog ref="htttDialogRef" type="VPKM" />
        <CancelInvoice ref="CancelInvoiceRef" />
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetail';
import API from '@/api/api-main';
import { useToast } from 'primevue/usetoast';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
import YCBSDialog from '../dialogs/YCBS.vue';
const YCBSDialogRef = ref<InstanceType<typeof YCBSDialog>>();
import HinhThucThanhToanDialog from '../dialogs/HinhThucThanhToan.vue';
const htttDialogRef = ref<InstanceType<typeof HinhThucThanhToanDialog>>();
import CancelInvoice from '../dialogs/CancelInvoice.vue';
const CancelInvoiceRef = ref<InstanceType<typeof CancelInvoice>>();
const odStore = useOrderDetailStore();
const toast = useToast();
const props = defineProps({
    isClient: {
        default: false
    }
});
const router = useRouter();
const loading = ref<{ [key: string]: boolean }>({});
type ButtonSeverity = 'danger' | 'secondary' | 'success' | 'info' | 'warning' | 'help' | 'contrast';
interface Button {
    label: string;
    severity?: ButtonSeverity;
    onClick?: Function;
    visible?: boolean;
    disabled?: boolean;
    icon?: string;
}

// Button click handlers 
const xacNhanDonHang = async () => {
    try {
        await API.add(`PurchaseOrder/${odStore.order?.id}/send-payment`);
        toastService('success', t('body.systemSetting.success_label'), t('Custom.OrderConfirmed'));
        odStore.fetchStore();
    } catch (err) {
        toastService('error', t('Custom.error'), t('Custom.errorOccurred'));
    }
};

const yeuCauBoSung = () => {
    YCBSDialogRef.value?.open();
};

const pheDuyet = async () => {
    if (odStore.order?.approval == null || odStore.order?.approval.status == 'R') {
        try {
            const res = await API.add(`Approval/action-purchase/${odStore.order?.id}`);
            router.push(`/approval-setup/order-approval/${res.data.id}`);
        } catch (err) {
            toastService('error', t('Custom.error'), t('Custom.errorOccurred'));
        }
    } else {
        router.push(`/approval-setup/order-approval/${odStore.order?.approval.id}`);
    }
};

const xacNhan = async () => {
    try {
        await API.update(`PurchaseOrder/${odStore.order?.id}/change-status/DXN`);
        toastService('success', t('body.systemSetting.success_label'), t('Custom.OrderConfirmed'));
        odStore.fetchStore();
    } catch (err) {
        toastService('error', t('Custom.error'), t('Custom.errorOccurred'));
    }
};

const giaoHang = async () => {
    try {
        const body = new FormData();
        odStore.order?.attDocuments.forEach((el) => {
            if (el._file) body.append('AttachFile', el._file);
        }); 
        await API.update(`PurchaseOrder/${odStore.order?.id}/change-status/DHT`, body);
        toastService('success', t('body.systemSetting.success_label'), t('Custom.deliveryConfirmed'));
        odStore.fetchStore();
    } catch (err) {
        toastService('error', t('Custom.error'), t('Custom.errorOccurred'));
    }
};
const daNhanHang = async () => {
    try {
        await API.update(`PurchaseOrder/${odStore.order?.id}/change-status/DHT`);
        toastService('success', t('body.systemSetting.success_label'), t('Custom.OrderConfirmed'));
        odStore.fetchStore();
    } catch (err) {
        toastService('error', t('Custom.error'), t('Custom.errorOccurred'));
    }
}; 
const thanhToan = async () => {
    htttDialogRef.value?.open();
    return;
};
const cancelInvoice = async () => {
    CancelInvoiceRef.value?.open();
};
const buttons = computed<Button[]>(() => {
    return [
        {
            label: t('Custom.cancelOrder'),
            severity: 'danger',
            visible: isVisible(['TTN', 'DXL'], !props.isClient) || (isVisible(['CXN']) && !props.isClient),
            onClick: cancelInvoice
        },
        {
            label: t('Custom.confirmOrder'),
            visible: isVisible(['TTN'], !props.isClient),
            onClick: xacNhanDonHang
        },
        {
            label: t('Custom.supplementRequest'),
            severity: 'help',
            visible: isVisible(['DXL'], odStore.isExceedDebt && !props.isClient),
            disabled: odStore.order?.approval?.status == 'A',
            onClick: yeuCauBoSung
        },
        {
            label: t('Custom.approve'),
            visible: isVisible(['DXL'], odStore.isExceedDebt && !props.isClient),
            disabled: odStore.order?.attachFile.some((row) => !row.filePath) || (odStore.order?.attachFile.length || 0) < 1,
            onClick: pheDuyet
        },
        {
            label: t('Custom.confirmToSAP'),
            visible: isVisible(['DTT']) || (isVisible(['CXN']) && (odStore.order?.approval?.status == 'A' || !odStore.isExceedDebt) && !props.isClient),
            disabled: odStore.order?.attachFile.some((row) => !row.filePath),
            onClick: xacNhan
        },
         {
            label: t('Custom.complete'), 
            // severity: 'success',
            // icon: 'fa-solid fa-truck-fast',
            visible: isVisible(['DGH'], !props.isClient),
            onClick: giaoHang,
            disabled: (odStore.order?.attDocuments.length || 0) < 1
        },
        {
            label: t('Custom.payment'),
            visible: isVisible(['CTT'], props.isClient),
            icon: 'fa-solid fa-money-check-dollar',
            onClick: thanhToan
        },

        {
            label: t('Custom.complete'),
            visible: isVisible(['DGHR']),
            onClick: daNhanHang
        }
    ] as Button[];
});

const onButtonClick = async (index: number, callback: Function | undefined) => {
    Object.keys(loading.value).forEach((key) => {
        loading.value[key] = false;
    });
    if (callback) {
        loading.value[index] = true;
        await callback();
        loading.value[index] = false;
    }
};

// stt: Trạng thái đơn hàng mà nút sẽ được hiện ra
// except: trạng thái bổ sung
const isVisible = (stt: string[], except = true): boolean => {
    return stt.includes(odStore.order?.status || '') && except;
};

const toastService = (severity: 'error' | 'secondary' | 'success' | 'info' | 'contrast' | 'warn', smr: string, detail: string, life: number = 3000) => {
    toast.add({
        severity: severity,
        detail: detail,
        summary: smr,
        life: life
    });
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
