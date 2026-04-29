export const getStatuses = (t: any) => [
    { code: 'DXL', name: t('body.status.DXL') },
    { code: 'DXN', name: t('body.status.DXN') },
    { code: 'HUY2', name: t('body.status.HUY') },
    { code: 'DGH', name: t('body.status.DGH') },
    { code: 'DHT', name: t('body.status.DHT') },
    { code: 'CTT', name: t('body.status.CTT') },
    { code: 'TTN', name: t('body.status.TTN') },
    { code: 'CXN', name: t('body.status.CXN') },
    { code: 'DTT', name: t('body.status.DTT') },
    { code: 'DONG', name: t('body.status.DONG') },
    { code: 'DGHR', name: t('body.status.DGHR') }
];
export const getStatusesPromotion = (t: any) => [
    { code: 'DXL', name: t('body.status.DXL') },
    { code: 'DXN', name: t('body.status.DXN') },
    { code: 'HUY', name: t('body.status.HUY') },
    { code: 'DGH', name: t('body.status.DGH') },
    { code: 'DHT', name: t('body.status.DHT') },
    { code: 'CTT', name: t('body.status.CTT') },
    { code: 'TTN', name: t('body.status.TTN') },
    { code: 'CXN', name: t('body.status.CXN') },
    { code: 'DTT', name: t('body.status.DTT') },
    { code: 'DONG', name: t('body.status.DONG') },
    { code: 'DGHR', name: t('body.status.DGHR') }
];

export const getLabels = (t: any) => ({
    DONG: {
        label: t('body.status.DONG'),
        class: 'text-white  bg-gray-500 border-1 border-gray-500'
    },
    DXL: {
        label: t('body.status.DXL'),
        class: 'text-yellow-700 bg-yellow-200'
    },
    DXN: {
        class: 'text-teal-700 bg-teal-200',
        label: t('body.status.DXN')
    },
    HUY2: {
        class: 'text-red-500 border-red-500 bg-white border-1',
        label: t('body.status.HUY')
    },
   
    DGH: {
        class: 'text-blue-700 bg-blue-200',
        label: t('body.status.DGH')
    },
    DGHR: {
        class: 'text-blue-700 bg-blue-200 bg-white border-1',
        label: t('body.status.DGHR')
    },
    DHT: {
        class: 'text-green-700 bg-green-200',
        label: t('body.status.DHT')
    },
    CTT: {
        class: 'text-yellow-50 bg-yellow-700',
        label: t('body.status.CTT')
    },
    TTN: {
        class: 'text-yellow-700 bg-white border-1 border-yellow-700',
        label: t('body.status.TTN')
    },
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: t('body.status.CXN')
    },
    DTT: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: t('body.status.DTT')
    }
});
export const getLabelsPromotion = (t: any) => ({
    DONG: {
        label: t('body.status.DONG'),
        class: 'text-white  bg-gray-500 border-1 border-gray-500'
    },
    DXL: {
        label: t('body.status.DXL'),
        class: 'text-yellow-700 bg-yellow-200'
    },
    DXN: {
        class: 'text-teal-700 bg-teal-200',
        label: t('body.status.DXN')
    },
    HUY: {
        class: 'text-red-500 border-red-500 bg-white border-1',
        label: t('body.status.HUY')
    },

    DGH: {
        class: 'text-blue-700 bg-blue-200',
        label: t('body.status.DGH')
    },
    DGHR: {
        class: 'text-blue-700 bg-blue-200 bg-white border-1',
        label: t('body.status.DGHR')
    },
    DHT: {
        class: 'text-green-700 bg-green-200',
        label: t('body.status.DHT')
    },
    CTT: {
        class: 'text-yellow-50 bg-yellow-700',
        label: t('body.status.CTT')
    },
    TTN: {
        class: 'text-yellow-700 bg-white border-1 border-yellow-700',
        label: t('body.status.TTN')
    },
    CXN: {
        class: 'text-orange-700 bg-white border-1 border-orange-700',
        label: t('body.status.CXN')
    },
    DTT: {
        class: 'text-green-700 bg-white border-1 border-green-700',
        label: t('body.status.DTT')
    }
});