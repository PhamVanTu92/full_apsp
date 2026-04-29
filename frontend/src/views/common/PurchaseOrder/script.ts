import { AxiosResponse } from 'axios';
import { PaymentMethodCode, PurchaseOrder } from './types/entities';
import { format } from 'date-fns';
import API from '@/api/api-main';
import { useCartStore } from '@/Pinia/cart';

const cartStore = useCartStore();
export interface PromotionPayload {
    id: 0;
    //**
    // format: 'yyyy-mm-dd';
    // */
    docType: string;
    orderDate: string;
    cardId: string;
    payMethod: 1;
    promotionParamLine: {
        lineId: number;
        itemId: number;
        quantity: number;
        payMethod: PaymentMethodCode;
    }[];
}

import { PromotionOrderLineSub } from './types/promotionOrderLine.type';
export class PromotionInCheckPricePayload implements PromotionOrderLineSub {
    promotionCode = null;
    promotionName = null;
    volumn?: 0 ;
    promotionDesc = null;
    isOtherPromotion = null;
    isOtherDist = null;
    isOtherPay = null;
    hasException = null;
    inGroup: number = 0;
    isOtherPromotionExc = null;
    isOtherDistExc = null;
    isOtherPayExc = null;
    addAccumulate = false;
    type = null;
    promotionId = null;
    itemId: number = 0;
    itemCode: string = '';
    itemName: string = '';
    quantityAdd = 0;
    packingName = '';
    cond = null;
    lineId = 0;
    numInSale = null;
    discount = null;
    discountType = null;
    listLineId = [] as any[];
    constructor(init: any = null) {
        this.discount = null;
        if (init) {
            for (const key in this) {
                this[key] = init[key];
                if (key == 'listLineId' && init['listLineId'] == null) {
                    this.listLineId = [0];
                }
            }
            this.itemId = init.itemId || 0;
        }
    }
}

export interface CheckPricePayload {
    cardId: number;
    cardName: string;
    cardCode: string;
    address: {
        address: string;
        locationId: number;
        locationName: string;
        areaId: number;
        areaName: string;
        email: string;
        phone: string;
        person: string;
        note: string;
        type: string;
    }[];
    itemDetail: {
        lineId: number;
        itemId: number;
        itemCode: string;
        itemName: string;
        quantity: number;
        price: number;
        priceAfterDist: number;
        discount: number;
        distcountAmount: number;
        vat: number;
        vatCode: string;
        vatAmount: number;
        lineTotal: number;
        note: string;
        ouomId: number;
        uomCode: string;
        uomName: string;
        numInSale: number;
        paymentMethodCode: string;
    }[];
    bonus: string;
    bonusAmount: number;
    totalBeforeVat: number;
    total: number;
    promotion: any[];
    quarterlyCommitmentBonus: number;
    yearCommitmentBonus: number;
}

export function fnum(num: number, decimalPlaces: number = 2, suffix: string = ''): string {
    if (isNaN(num) || !num) {
        num = 0;
    }

    return `${Intl.NumberFormat('us-US', {
        minimumFractionDigits: decimalPlaces,
        maximumFractionDigits: decimalPlaces
    }).format(num)}${suffix}`;
}
export function fnumInt(num: number,  suffix: string = ''): string {
    if (isNaN(num) || !num) {
        num = 0;
    }

    return `${Intl.NumberFormat('us-US', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(num)}${suffix}`;
}

export function fdate(date: Date | string): string {
    if (date) {
        const _date = new Date(date);
        return format(_date, 'dd/MM/yyyy');
    }
    return '';
}

export async function fetchPromotion(payload: PromotionPayload): Promise<AxiosResponse> {
    return await API.add('Promotion/getPromotion', payload);
}
export async function fetchCheckPrice(payload: CheckPricePayload): Promise<AxiosResponse> {
    return await API.add('purchaseorder/price-check', payload);
}
export async function createPurchaseOrder(payload: PurchaseOrder): Promise<AxiosResponse> {
    return await API.add('purchaseOrder/add', payload);
}

export async function clearCartItem(): Promise<AxiosResponse> {
    return await cartStore.clearCart();
}
export async function checkPayment(payload: any): Promise<AxiosResponse> {
    return await API.add('payment', payload);
}
