import { useI18n } from 'vue-i18n';

export const useOrderStatusLabels = () => {
    const { t } = useI18n();

    const labels = {
        DONG: {
            label: t('body.status.DONG'),
            class: 'text-white bg-gray-500 border-1 border-gray-500'
        },
        DXL: {
            label: t('body.status.Processing'),
            class: 'text-yellow-700 bg-yellow-200'
        },
        DXN: {
            class: 'text-teal-700 bg-teal-200',
            label: t('body.OrderList.confirmed')
        },
        HUY: {
            class: 'text-red-500 bg-red-100',
            label: t('body.OrderList.cancelled')
        },
        HUY2: {
            class: 'text-red-500 border-red-500 bg-white border-1',
            label: t('body.OrderList.cancelled')
        },
        DGH: {
            class: 'text-blue-700 bg-blue-200',
            label: t('body.OrderList.inDelivery')
        },
        DGHR: {
            class: 'text-blue-700 bg-blue-200 bg-white border-1',
            label: t('body.OrderList.delivered')
        },
        DHT: {
            class: 'text-green-700 bg-green-200',
            label: t('body.OrderList.completed')
        },
        CTT: {
            class: 'text-yellow-50 bg-yellow-700',
            label: t('body.OrderList.waiting_payment')
        },
        TTN: {
            class: 'text-yellow-700 bg-white border-1 border-yellow-700',
            label: t('body.OrderList.waiting_process')
        },
        CXN: {
            class: 'text-orange-700 bg-white border-1 border-orange-700',
            label: t('body.OrderList.pendingConfirmation')
        },
        DTT: {
            class: 'text-green-700 bg-white border-1 border-green-700',
            label: t('body.OrderList.complete_payment')
        }
    };

    const getOrderStatus = (statusCode) => {
        return labels[statusCode] || { severity: '', label: statusCode };
    };

    return {
        labels,
        getOrderStatus
    };
};
