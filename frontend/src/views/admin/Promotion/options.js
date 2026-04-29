export const getComparisonOptions = (t) => [
    { value: 'AND', label: t('body.promotion.And') },
    { value: 'OR', label: t('body.promotion.Or') }
];

export const defaultPromotionLine = {
    id: 0,
    fatherId: 0,
    cond: 'AND',
    subType: 1,
    hasException: false,
    addAccumulate: false,
    status: '',
    promotionLineSub: []
};

export const defaultPromotionLineSub = {
    id: 0,
    fatherId: 0,
    followBy: 0,
    quantity: 0,
    addType: 'Q',
    addBuy: 0,
    addQty: 0,
    isSameType: false,
    addValueType: 'MIN',
    fromDate: null,
    toDate: null,
    minVolumn: 0,
    discount: 0,
    discountType: 'P',
    priceType: 'P',
    status: null,
    promotionLineSubSub: [],
    promotionItemBuy: [],
    promotionUnit: []
};

export const defaultPromotionLineSubSub = {
    id: 0,
    fatherId: 0,
    cond: 'AND',
    quantity: 0,
    inGroup: 1,
    status: '',
    promotionSubItemAdd: [],
    promotionSubUnit: []
};

export const defaultPayload = {
    id: 0,
    promotionCode: null,
    promotionName: '',
    promotionDescription: '',
    fromDate: '',
    toDate: '',
    promotionMonths: '',
    isBirthday: false,
    beforeDay: 0,
    afterDay: 0,
    isAllCustomer: true,
    promotionStatus: 'A',
    status: '',
    note: '',
    isOtherPromotion: false,
    isOtherDist: false,
    isOtherPay: false,
    promotionType: 2,
    hasException: false,
    isOtherPromotionExc: false,
    isOtherDistExc: false,
    isOtherPayExc: false,
    promotionLine: [],
    promotionCustomer: [],
    promotionBrand: [],
    promotionIndustry: []
};

export const getOptionAddValueType = (t) => [
    {
        code: 'MIN',
        name: t('body.promotion.minValue')
    },
    {
        code: 'MAX',
        name: t('body.promotion.maxValue')
    },
    {
        code: 'OPT',
        name: t('body.promotion.applicable_object_custom')
    }
]

export const getAddType = (t) => [
    {
        code: 'Q',
        name: t('body.home.quantity')
    },
    {
        code: 'R',
        name: t('body.promotion.rate')
    }
]

export const getFollowBy = (t) => [
    {
        code: 1,
        name: t('body.promotion.byQuantity')
    },
    {
        code: 2,
        name: t('body.promotion.byVolume')
    }
]
export const getFollowBy2 = (t) => [
    {
        code: 1,
        name: t('body.promotion.buyGetFree')
    },
    {
        code: 2,
        name: t('body.promotion.buyGetDiscount')
    }
]
export const getSubType = (t) => [
    {
        code: 1,
        name: t('client.buy_giveaway')
    },
    {
        code: 2,
        name: t('client.buy_discount')
    }
]
export const getPromotionType = (t) => [
    { code: 1, name: t('client.invoice') },
    { code: 2, name: t('client.product') },
    { code: 3, name: t('client.general') }
]