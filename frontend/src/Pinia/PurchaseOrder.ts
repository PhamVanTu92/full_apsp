import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import API from '../api/api-main';

interface OrderStat {
    id: 0;
    docId: number;
    vatAmount: number;
    paynowVATAmount: number;
    debtVATAmount: number;
    debtGuaranteeVATAmount: number;
    bonusCommited: number;
    totalBeforeVat: number;
    totalPayNowBeforeVat: number;
    totalDebtBeforeVat: number;
    totalDebtGuaranteeBeforeVat: number;
    totalAfterVat: number;
    bonusPercent: number;
    bonusAmount: number;
    totalPayNow: number;
    totalDebt: number;
    totalDebtGuarantee: number;
}

class PromotionPayload {
    promotionCode = null;
    promotionName = null;
    promotionDesc = null;
    isOtherPromotion = null;
    isOtherDist = null;
    isOtherPay = null;
    hasException = null;
    isOtherPromotionExc = null;
    isOtherDistExc = null;
    isOtherPayExc = null;
    addAccumulate = null;
    type = null;
    promotionId = null;
    itemId = null;
    itemCode = null;
    itemName = null;
    quantityAdd = null;
    packingName = null;
    cond = null;
    lineId = null;
    numInSale = null;
    discount = null;
    discountType = null;
    listLineId = [] as any[];
    constructor(init = null) {
        if (init) {
            for (const key in this) {
                this[key] = init[key];
                if (key == 'listLineId' && init['listLineId'] == null) {
                    this.listLineId = [0];
                }
            }
        }
    }
}

interface ChietKhau {
    quy: number;
    nam: number;
}
interface CommittedLineSub {
    id: number;
    fatherId: number;
    industryId: number;
    brandIds: any[];
    industryName: string;
    brandId: null;
    itemTypeIds: any[];
    itemTypes: any[];
    quarter1: number;
    bonusQuarter1: number;
    totalQuarter1: number;
    quarter2: number;
    bonusQuarter2: number;
    totalQuarter2: number;
    quarter3: number;
    bonusQuarter3: number;
    totalQuarter3: number;
    quarter4: number;
    bonusQuarter4: number;
    totalQuarter4: number;
    package: number;
    month1: null;
    bonusMonth1: number;
    totalMonth1: number;
    month2: null;
    bonusMonth2: number;
    totalMonth2: number;
    month3: null;
    bonusMonth3: number;
    totalMonth3: number;
    month4: null;
    bonusMonth4: number;
    totalMonth4: number;
    month5: null;
    bonusMonth5: number;
    totalMonth5: number;
    month6: null;
    bonusMonth6: number;
    totalMonth6: number;
    month7: null;
    bonusMonth7: number;
    totalMonth7: number;
    month8: null;
    bonusMonth8: number;
    totalMonth8: number;
    month9: null;
    bonusMonth9: number;
    totalMonth9: number;
    month10: null;
    bonusMonth10: number;
    totalMonth10: number;
    month11: null;
    bonusMonth11: number;
    totalMonth11: number;
    month12: null;
    bonusMonth12: number;
    totalMonth12: number;
    total: number;
    discount: number;
    discountYear: number;
    isCvYear: null;
    isConvert: boolean;
    sixMonthDiscount: number;
    isCvSixMonth: boolean;
    sixMonthBonus: null;
    threeMonthDiscount: null;
    threeMonthBonus: null;
    isCvThreeMonth: boolean;
    discountMonth: null;
    isCvMonth: boolean;
    nineMonthDiscount: 1;
    nineMonthBonus: null;
    yearBonus: null;
    isCvNineMonth: boolean;
    currentVolumn: number;
    total12M: number;
    afterVolumn: number;
    isAchieved: boolean;
    bonusPercentage: number;
    totalBonus: number;
    industry: any;
    status: '';
    brand: any[];
    committedLineSubSub: any[];
}

interface CommittedLine {
    id: number;
    fatherId: number;
    committedType: string;
    status: string | null;
    committedLineSub: CommittedLineSub[];
}

interface Committed {
    id: number;
    committedCode: string | null;
    committedName: string | null;
    committedDescription: string | null;
    cardId: number;
    cardCode: string | null;
    cardName: string | null;
    committedYear: string | null;
    userId: number;
    creator: string | null;
    docStatus: string | null;
    rejectReason: string | null;
    committedLine: CommittedLine[];
}
const cmtType = {
    Q: 'Quý',
    Y: 'Năm'
};
const currentQYNum = {
    Q: Math.floor(new Date().getMonth() / 3) + 1,
    Y: new Date().getMonth() + 1
};
const getProVal = (type: string): string => {
    if (type == 'Q') return `bonusQuarter${currentQYNum[type]}`;
    else if (type == 'Y') return `bonusMonth${currentQYNum[type]}`;
    else return '';
};

export const usePurchaseOrderStore = defineStore('PurchaseOrder', () => {
    const chietKhau = ref<ChietKhau>();
    const KM = ref();
    const items = ref([]);
    const customer = ref(null);
    const promotion = ref();
    const orderStat = ref<OrderStat>({
        id: 0,
        docId: 0,
        vatAmount: 0,
        paynowVATAmount: 0,
        debtVATAmount: 0,
        debtGuaranteeVATAmount: 0,
        bonusCommited: 0,
        totalBeforeVat: 0,
        totalPayNowBeforeVat: 0,
        totalDebtBeforeVat: 0,
        totalDebtGuaranteeBeforeVat: 0,
        totalAfterVat: 0,
        bonusPercent: 0,
        bonusAmount: 0,
        totalPayNow: 0,
        totalDebt: 0,
        totalDebtGuarantee: 0
    });

    const tienGiamSanLuong = ref({
        name: '',
        label: 'Thưởng sản lượng',
        total: 0,
        yearTotal: 0,
        quarterTotal: 0,
        visible: false,
        lines: [] as any[]
    });

    const orderError = ref(false);
    const getOrderStats = computed(() => orderStat.value);

    const committed = ref<Committed | null>(null);
    const getCommitted = computed(() => committed.value);

    const setData = (data: any | ChietKhau, type: 'KM' | 'CK') => {
        if (type === 'CK') {
            chietKhau.value = data;
        } else if (type == 'KM') {
            KM.value = data || { id: 0, orderDate: new Date(), cardId: 0, promotionOrderLine: [] };
        }
        setTimeout(() => {
            getTotal();
        }, 100)
        // if (chietKhau.value && KM.value) {
        //     getTotal();
        // }
    };

    function getTotal() {
        const { id, cardName, cardCode, crD1 } = customer.value || ({} as any);
        const dataPlayload = {} as any;

        const defaultAddress = crD1?.filter((el: any) => el.default === 'Y') || {};
        dataPlayload.cardId = id || 0;
        dataPlayload.cardName = cardName || '';
        dataPlayload.cardCode = cardCode || '';
        dataPlayload.address =
            defaultAddress.map((el: any) => ({
                address: el.address,
                locationId: el.locationId,
                locationName: el.locationName,
                areaId: el.areaId,
                areaName: el.areaName,
                email: el.email,
                phone: el.phone,
                person: el.person,
                note: el.note,
                type: el.type
            })) || [];
        dataPlayload.itemDetail = items.value?.map((el: any, index) => ({
            lineId: index,
            itemId: el.id,
            itemCode: el.itemCode,
            itemName: el.itemName,
            quantity: el.quantity,
            price: el.price,
            priceAfterDist: el.priceAfterDist || 0,
            discount: el.discount,
            discountType: el.discountType,
            distcountAmount: el.distcountAmount,
            vat: el.taxGroups?.rate,
            vatCode: el.taxGroups?.code,
            vatAmount: el.vatAmount,
            lineTotal: el.lineTotal,
            note: el.note,
            ouomId: el.ougp.baseUom,
            uomCode: el.ougp.ouom.uomCode,
            uomName: el.ougp.ouom.uomName,
            numInSale: el.packing.volumn * el.quantity,
            paymentMethodCode: el.paymentMethodCode || null
        }));
        dataPlayload.bonus = 'PayNow';
        dataPlayload.bonusAmount = 0;
        dataPlayload.totalBeforeVat = 0;
        dataPlayload.total = 0;

        let product = [] as Array<any>;
        //------------------- Khuyen mai --------------------------------------
        const promotions: PromotionPayload[] = [];
        if (KM.value.promotionOrderLine?.length) {
            for (const line of KM.value.promotionOrderLine) {
                if (line.promotionOrderLineSub) {
                    const orLineSubs = line.promotionOrderLineSub?.filter((item: any) => item.cond == 'OR')
                    const andLineSubs = line.promotionOrderLineSub?.filter((item: any) => item.cond == 'AND');
                    const selectedInGroupKeys = Array(orLineSubs).filter((item: any) => item.selected).map((item: string) => {
                        let arr = item.split('_');
                        return {
                            lineId: arr[0],
                            inGroup: arr[1]
                        }
                    }) as {
                        lineId: string,
                        inGroup: string
                    }[];
                    if (orLineSubs.length) {
                        for (const orLineSub of orLineSubs) {
                            for (const sigKey of selectedInGroupKeys) {
                                const isTrue = (sigKey.inGroup == orLineSub.inGroup && sigKey.lineId == orLineSub.lineId)
                                if (isTrue) {
                                    promotions.push(new PromotionPayload({ ...KM.value.items, ...line, ...orLineSub }))
                                }
                            }
                        }
                        for (const andLineSub of andLineSubs) {
                            promotions.push(new PromotionPayload({ ...KM.value.items, ...line, ...andLineSub }))
                        }
                    }
                    else {
                        for (const lineSub of line.promotionOrderLineSub) {
                            promotions.push(new PromotionPayload({ ...KM.value.items, ...line, ...lineSub }))
                        }
                    }
                }
                // else
                // promotions.push(new PromotionPayload({...KM.value.items,...line}))
            }
        }
        dataPlayload.promotion = promotions;
        promotion.value = promotions;
        //----------------------------------------------------------------------
        if (product.length) {
            product = product.map((el) => {
                const line = dataPlayload.promotion.find((e: any) => e.promotionId == el.promotionId);
                return {
                    promotionCode: line?.promotionCode || '',
                    promotionName: line?.promotionName || '',
                    promotionDesc: line?.promotionDesc || '',
                    isOtherPromotion: line?.isOtherPromotion,
                    isOtherDist: line?.isOtherDist,
                    isOtherPay: line?.isOtherPay,
                    hasException: line?.hasException,
                    isOtherPromotionExc: line?.isOtherPromotionExc,
                    isOtherDistExc: line?.isOtherDistExc,
                    isOtherPayExc: line?.isOtherPayExc,
                    addAccumulate: line?.addAccumulate,
                    type: el.itemGroup,
                    promotionId: el.promotionId,
                    itemId: el.itemId,
                    itemCode: el.itemCode,
                    itemName: el.itemName,
                    quantityAdd: el.quantityAdd,
                    packingName: el.packingName,
                    cond: el.cond,
                    lineId: el.lineId,
                    numInSale: el.volumn * el.quantityAdd || 0,
                    discount: el.discount,
                    discountType: el.discountType,
                    listLineId: el.listLineId || [el.lineId]
                };
            });
            dataPlayload.promotion = product;
            // payload.itsemDetail = payload.itemDetail.concat(product);
        }

        dataPlayload.quarterlyCommitmentBonus = chietKhau.value?.quy || 0;
        dataPlayload.yearCommitmentBonus = chietKhau.value?.nam || 0;

        API.add('purchaseorder/price-check', dataPlayload)
            .then((res) => {
                if (items.value.length < 0) {
                    resetStats();
                    return;
                }
                Object.assign(orderStat.value, res.data['item']);
                committed.value = res.data['items'] as Committed;
                const info = {
                    name: '',
                    label: 'Thưởng sản lượng',
                    total: 0,
                    yearTotal: 0,
                    quarterTotal: 0,
                    visible: false,
                    lines: [] as any[]
                };
                if (committed.value?.committedLine.length) {
                    // debugger
                    for (let line of committed.value?.committedLine) {
                        const _line = {
                            label: cmtType[line?.committedType as keyof typeof cmtType] ? `Thưởng sản lượng ${cmtType[line.committedType as keyof typeof cmtType]} ${currentQYNum[line.committedType as keyof typeof currentQYNum]}` : 'Unknown',
                            time_num: currentQYNum[line.committedType as keyof typeof currentQYNum],
                            value: 0
                        };

                        const line3M = {
                            label: 'Thưởng 3 tháng',
                            value: 0
                        };
                        const line6M = {
                            label: 'Thưởng 6 tháng',
                            value: 0
                        };
                        const line9M = {
                            label: 'Thưởng 9 tháng',
                            value: 0
                        };
                        const line1Y = {
                            label: 'Thưởng năm',
                            value: 0
                        };
                        const lineOver = {
                            label: 'Thưởng sản lượng vượt',
                            value: 0
                        };

                        for (let subLine of line?.committedLineSub) {
                            if (subLine[getProVal(line.committedType) as keyof CommittedLineSub]) {
                                _line.value += subLine[getProVal(line.committedType) as keyof CommittedLineSub];
                                info.total += subLine[getProVal(line.committedType) as keyof CommittedLineSub];
                                if (line.committedType == 'Y') {
                                    info.yearTotal += subLine[getProVal(line.committedType) as keyof CommittedLineSub];
                                }
                                if (line.committedType == 'Q') {
                                    info.quarterTotal += subLine[getProVal(line.committedType) as keyof CommittedLineSub];
                                }
                            }
                            if (subLine.threeMonthBonus) {
                                line3M.value += subLine.threeMonthBonus;
                                info.total += subLine.threeMonthBonus;
                                if (line.committedType == 'Y') {
                                    info.yearTotal += subLine.nineMonthBonus || 0;
                                }
                                if (line.committedType == 'Q') {
                                    info.quarterTotal += subLine.nineMonthBonus || 0;
                                }
                            }
                            if (subLine.sixMonthBonus) {
                                line6M.value += subLine.sixMonthBonus;
                                info.total += subLine.sixMonthBonus;
                                if (line.committedType == 'Y') {
                                    info.yearTotal += subLine.sixMonthBonus;
                                }
                                if (line.committedType == 'Q') {
                                    info.quarterTotal += subLine.sixMonthBonus;
                                }
                            }
                            if (subLine.nineMonthBonus) {
                                line9M.value += subLine.nineMonthBonus;
                                info.total += subLine.nineMonthBonus;
                                if (line.committedType == 'Y') {
                                    info.yearTotal += subLine.nineMonthBonus;
                                }
                                if (line.committedType == 'Q') {
                                    info.quarterTotal += subLine.nineMonthBonus;
                                }
                            }
                            if (subLine.afterVolumn >= subLine.total && subLine.yearBonus) {
                                line1Y.value += subLine.yearBonus;
                                info.total += subLine.yearBonus;
                                if (line.committedType == 'Y') {
                                    info.yearTotal += subLine.nineMonthBonus || 0;
                                }
                                if (line.committedType == 'Q') {
                                    info.quarterTotal += subLine.nineMonthBonus || 0;
                                }
                            }
                            // Thưởng sản lượng vượt
                            if (subLine.committedLineSubSub.length) {
                                for (let subSubLine of subLine.committedLineSubSub) {
                                    if (subLine.afterVolumn >= subSubLine.outPut) {
                                        lineOver.value += subSubLine.bonusTotal;
                                        info.total += subSubLine.bonusTotal;
                                        if (line.committedType == 'Y') {
                                            info.yearTotal += subSubLine.bonusTotal;
                                        }
                                        if (line.committedType == 'Q') {
                                            info.quarterTotal += subSubLine.bonusTotal;
                                        }
                                    }
                                }
                            }
                        }

                        info.lines.push(...[_line, line3M, line6M, line9M, line1Y, lineOver]);
                    }

                    // set to purchase order store
                    chietKhau.value = {
                        quy: info.quarterTotal,
                        nam: info.yearTotal
                    };
                }
                tienGiamSanLuong.value = info;
            })
            .catch((error) => {
                resetStats();
                orderError.value = false;
                console.error(error);
            });
    }

    function resetStats() {
        orderError.value = false;
        items.value = [];
        for (let key in orderStat.value) {
            orderStat.value[key as keyof typeof orderStat.value] = 0;
        }
    }

    const getPromotion = computed(() => promotion);

    const getTienGiamSanLuong = computed(() => tienGiamSanLuong);
    return { sanLuong: chietKhau, KM, setData, orderStat, customer, items, getOrderStats, resetStats, getCommitted, orderError, promotion, getPromotion, tienGiamSanLuong, getTienGiamSanLuong };
});
