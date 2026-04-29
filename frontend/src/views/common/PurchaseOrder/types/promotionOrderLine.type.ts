export interface PromotionOrderLineSub {
    id?: number;
    fatherId?: number;
    cond: "AND" | "OR" | null;
    inGroup: number;
    lineId: number;
    listLineId: number[] | null;
    itemGroup?: string;
    itemId: number;
    itemCode: string;
    addAccumulate: boolean;
    itemName: string;
    packingId?: number;
    packingName: string;
    volumn?: number | null;
    quantityAdd: number;
    note?: string | null;
    discount: number | null;
    discountType: string | null;
    priceType?: string | null;
}

export interface PromotionOrderLine {
    id: number;
    fatherId: number;
    lineId: number;
    itemId: number;
    quantity: number;
    promotionId: number;
    promotionCode: string;
    promotionName: string;
    promotionDesc: string;
    isOtherPromotion: boolean;
    isOtherDist: boolean;
    isOtherPay: boolean;
    hasException: boolean;
    isOtherPromotionExc: boolean;
    isOtherDistExc: boolean;
    isOtherPayExc: boolean;
    promotionOrderLineSub: PromotionOrderLineSub[];
}

export interface PromotionResponse {
    id: number;
    orderDate: string;
    cardId: number;
    promotionOrderLine: PromotionOrderLine[];
}

