import { defineStore } from 'pinia';
import { PurchaseOrder } from '../types/entities';
import { OrderSummary } from '../types/orderSummary.type';
import { CheckPricePayload, PromotionInCheckPricePayload, PromotionPayload, fetchPromotion, fetchCheckPrice } from '../types/script';
import { PromotionOrderLine, PromotionOrderLineSub, PromotionResponse } from '../types/promotionOrderLine.type';
import { ref, computed, watch } from 'vue';
import { format } from 'date-fns';
import debouncer from 'lodash/debounce';
import { AxiosResponse } from 'axios';
import { useToast } from 'primevue/usetoast';
import cloneDeep from 'lodash/cloneDeep';
import { CommittedShow, CommittedResponse, CommitedData, CommittedLineSub } from '../types/committed.type';
import router from '@/router';

type PromotionToShow = PromotionOrderLine & { _promotionItems?: any; _selection?: any; _discountItems: any[]; _choiceGroup: { [key: string]: PromotionInCheckPricePayload[] }; _giftGroup?: PromotionInCheckPricePayload[] };
type DiscountItem = {
    itemId: number;
    discountValue: number;
};

const time = new Date();
const getCurrentQuarter = () => {
    const month = time.getMonth() + 1; // Months are zero-based
    return Math.ceil(month / 3); // Divide the month by 3 and round up to get the quarter
};
const getQuotaValue = (type: 'Q' | 'Y', subLine: CommittedLineSub): number => {
    let propName = '';
    if (type == 'Q') {
        propName = 'quarter' + getCurrentQuarter();
    } else if (type == 'Y') {
        propName = 'month' + time.getMonth() + 1;
    } else {
        return 0;
    }
    return Number(subLine[propName as keyof typeof subLine] || 0);
};

export const usePoStore = defineStore('PurchaseStore', () => {
    // ---------------------------------------
    const activeIndexTab = ref(0);
    // --------------------------------------- Danh sách sản phẩm khuyến mại
    const promotionItems = ref({
        discounts: [] as PromotionOrderLineSub[],
        gifts: [] as PromotionOrderLineSub[],
        selections: [] as PromotionOrderLineSub[]
    });
    const toast = useToast();
    // ---------------------------------------
    const isClient = ref(false);
    // ---------------------------------------  Model đơn hàng
    const model = ref<PurchaseOrder>(new PurchaseOrder());
    // ---------------------------------------  Trạng thái loading và disable của nút mua hàng
    const loadingOrderButton = ref(false);
    const disableOrderButton = ref(false);
    // ---------------------------------------  Bảng giá tiền - OrderSummary
    const orderSummary = ref(new OrderSummary());
    const getOrderSummary = computed(() => orderSummary.value);
    // ---------------------------------------  Tính toán khuyến mãi
    const discountItems = ref<DiscountItem[]>([]);
    const rawPromotion = ref<PromotionResponse>();
    const rawPromotionOrderLine = ref<PromotionOrderLine[]>([]);
    const getRawPromotionOrderLine = computed(() => rawPromotionOrderLine.value);
    const promotionToShow = ref<PromotionToShow[]>([]);
    const getPromtionToShow = computed<PromotionToShow[]>(() => JSON.parse(JSON.stringify(promotionToShow.value || [])));
    function setPromtionToShow(promotionOrderLine: PromotionOrderLine[]) {
        discountItems.value = [];
        model.value.promotion = [];
        promotionToShow.value = [];
        for (const item of promotionOrderLine) {
            const line: PromotionToShow = {
                ...item,
                _choiceGroup: {} as { [key: string]: PromotionInCheckPricePayload[] },
                _giftGroup: [] as PromotionInCheckPricePayload[],
                _discountItems: [] as PromotionInCheckPricePayload[]
            };

            // item.promotionOrderLineSub[0].inGroup
            const spTangArray: PromotionInCheckPricePayload[] = []; // Danh sách sản phẩm tặng
            const spGiamGiaArray: PromotionInCheckPricePayload[] = []; // Danh sách sản phẩm giảm giá
            for (let _pols of line.promotionOrderLineSub) {
                const picpp = new PromotionInCheckPricePayload({ ...cloneDeep(item), ..._pols });

                if (_pols.quantityAdd) {
                    spTangArray.push(picpp);
                } else if (_pols.discount) {
                    spGiamGiaArray.push(picpp);
                }
            }

            // Áp dụng giảm giá cho bảng chọn sản phẩm
            line._discountItems = spGiamGiaArray;
            // debugger;
            if (spGiamGiaArray.length > 0) {
                for (let spgg of spGiamGiaArray) {
                    let itemInDataTable = model.value.itemDetail.find((p) => p.itemId == spgg.itemId);
                    if (itemInDataTable) {
                        /*  Nếu giá trị giảm giá cuối cùng khác với giá trị khuyến mại thì
                         *  cập nhật lại trường khuyến mại trên sản phẩm.
                         *  -> giữ nguyên được giá trị của người dùng nhập khi thay đổi khi gọi đến khuyến mại
                         */
                        // const di = cloneDeep({
                        //     itemId: spgg.itemId,
                        //     discountValue: spgg.discount || 0
                        // });
                        // Tạm thời bỏ qua logic này vì đang có lỗi không thể debug -> có thể dính tới nested object
                        if (itemInDataTable._lastDiscount != spgg.discount) {
                            itemInDataTable.discount = spgg.discount || 0;
                            itemInDataTable._lastDiscount = spgg.discount || 0;
                            itemInDataTable._isPromotion = true;
                        }
                    }
                }
            }
            // Gán số lượng sản phẩm được tặng vào trường _promotionQuanlity
            for (let _item of model.value?.itemDetail) {
                const spTang = spTangArray.find((sp) => sp.itemId == _item.itemId);
                if (spTang) {
                    _item._promotionQuanlity = spTang.quantityAdd || 0; // Số lượng sản phẩm được tặng
                    // item._volumn = spTang.volumn || 0; // Số lượng quy đổi
                    _item._isPromotion = true; // Đánh dấu là sản phẩm khuyến mãi
                }
            }

            // const groupByinGroup = groupBy(spTangArray, 'inGroup') as { [key: string]: PromotionInCheckPricePayload[] };
            let choiceGroup: { [key: string]: PromotionInCheckPricePayload[] } = {};
            let giftGroup: PromotionInCheckPricePayload[] = [];

            let itemsGrouped = groupByKey<PromotionInCheckPricePayload>(spTangArray, 'inGroup');
            // Sản phẩm tặng
            line._promotionItems = itemsGrouped;

            //Lọc ra nhóm sản phẩm được tặng và nhóm combo chọn
            Object.keys(itemsGrouped).forEach((key) => {
                // const group = groupByinGroup[key];
                const group = itemsGrouped[key];
                // Lọc ra nhóm combo chọn
                if (group.some((itm) => itm.cond == 'OR')) {
                    // choiceGroup[key as keyof typeof choiceGroup] = groupByinGroup[key];
                    choiceGroup[key as keyof typeof choiceGroup] = itemsGrouped[key];
                }
                // Lọc ra nhóm được tặng kèm
                else {
                    giftGroup.push(...group);
                }
            });

            line._choiceGroup = choiceGroup;
            line._selection = Object.keys(choiceGroup)[0]; // Mặc định chọn hàng đầu tiên
            line._giftGroup = giftGroup;

            promotionToShow.value.push(line);
        }
    }
    function assignPromotion() {
        // Gán các sản phẩm khuyến mãi vào trường promotions của model
        model.value.promotion = [];
        for (const item of promotionToShow.value) {
            const _promotions: PromotionInCheckPricePayload[] = [];
            for (let rawP of rawPromotionOrderLine.value) {
                // Bỏ đi, các sản phẩm giảm giá thì không gửi đi
                // for (const sp of item._discountItems) {
                //     const picpp = new PromotionInCheckPricePayload({ ...sp, ...cloneDeep(rawP) });
                //     picpp.itemId = sp.itemId || 0;
                //     picpp.lineId = sp.lineId || 0;
                //     picpp.listLineId = sp.listLineId || [0];
                //     _promotions.push(picpp);
                // }
            }
            const prms: PromotionInCheckPricePayload[] = [];
            if (item._giftGroup?.length) prms.push(...item._giftGroup);
            if (item._choiceGroup[item._selection || 0]?.length) prms.push(...item._choiceGroup[item._selection || 0]);
            if (item._discountItems.length) prms.push(...item._discountItems);
            //    const  rawP =  rawPromotionOrderLine.value
            //...cloneDeep(rawP)
            for (let itm of prms) {
                if (itm) {
                    const picpp = new PromotionInCheckPricePayload({ ...itm });
                    picpp.itemId = itm.itemId || 0;
                    picpp.lineId = itm.lineId || 0;
                    picpp.listLineId = itm.listLineId || [0];
                    _promotions.push(picpp);
                }
            }
            model.value.promotion.push(..._promotions);
        }
    }
    // ---------------------------------------  Cam kết
    const rawCommitted = ref<CommittedResponse | null>();
    const committedToShow = ref<CommittedShow[]>([]);
    const getCommittedToShow = computed(() => committedToShow.value);
    function setCommittedToShow(rawCmt: CommittedResponse) {
        committedToShow.value = [];
        if (rawCmt) {
            rawCmt.committedLine?.forEach((line) => {
                const cmt2Show: CommittedShow = {
                    timeLabel: line.committedType == 'Q' ? 'Quý' : 'Năm',
                    timeValue: line.committedType == 'Q' ? getCurrentQuarter() : new Date().getMonth() + 1,
                    data: []
                };

                line.committedLineSub?.forEach((subLine) => {
                    const cmtData: CommitedData = {
                        industryId: subLine.industryId,
                        industryName: subLine.industryName,
                        currentValue: subLine.currentVolumn,
                        incurredValue: subLine.afterVolumn - subLine.currentVolumn,
                        quotaValue: getQuotaValue(line.committedType, subLine)
                    };
                    cmt2Show.data.push(cmtData);
                });
                committedToShow.value.push(cmt2Show);
            });
        }
    }
    // ---------------------------------------  Các phương thức service methods

    function fetchCheckPriceMethod() {
        // promotionItems.value.discounts = [];
        // promotionItems.value.gifts = [];
        // promotionItems.value.selections = [];
        assignPromotion();
        const checkPricePayload: CheckPricePayload = {
            address: [model.value._customer?.getCrD1Default('S').default as any, model.value._customer?.getCrD1Default('B').default as any].filter((item) => item),
            bonus: 'PayNow',
            bonusAmount: model.value.bonusAmount || 0,
            cardCode: model.value.cardCode,
            cardId: model.value.cardId,
            cardName: model.value.cardName,
            docType :  router.currentRoute.value.fullPath.includes('hisPurNET/new') ? 'NET' : '',
            itemDetail: model.value.itemDetail.map((p) => ({
                ...p,
                quantity: p.quantity || 0,
                price: p.price || 0,
                discount: p.discount || 0
            })) as any,
            promotion: model.value.promotion,
            total: 0,
            quarterlyCommitmentBonus: 0,
            totalBeforeVat: 0,
            yearCommitmentBonus: 0
        }; 
        // Gọi đến endpoint check-price
        disableOrderButton.value = true;
        fetchCheckPrice(checkPricePayload)
            .then((res: AxiosResponse<{ item: OrderSummary; items: any; items1: any }>) => {
                orderSummary.value = res.data['item'];
                rawCommitted.value = res.data['items'];
                setCommittedToShow(res.data['items']);
                disableOrderButton.value = false;
            })
            .catch((error) => {
                toast.add({
                    severity: 'error',
                    summary: 'Lỗi',
                    detail: 'Đã có lỗi xảy ra trong quá trình tạo đơn hàng, vui lòng thử lại sau.',
                    life: 3000
                });
                disableOrderButton.value = true;
            })
            .finally(() => {
                loadingOrderButton.value = false;
            });
    }

    function fetchPromotionMethod(callback?: Function) {
        // hàm lấy sản phẩm khuyến mãi
        // Gọi đến endpoint khuyến mãi
        const prmtPayload: PromotionPayload = {
            id: 0,
            cardId: model.value.cardId?.toString(),
            orderDate: format(new Date(), 'yyyy-MM-dd'),
            payMethod: 1,
            promotionParamLine: model.value.itemDetail.map((item, i) => ({
                lineId: i,
                itemId: item.itemId,
                quantity: item.quantity,
                payMethod: item.paymentMethodCode
            }))
        };

        loadingOrderButton.value = true;
        let response: AxiosResponse<{ item: OrderSummary; items: CommittedResponse; items2: any }>;
        fetchPromotion(prmtPayload)
            .then((res) => {
                rawPromotionOrderLine.value = res.data?.['items']['promotionOrderLine'];
                rawPromotion.value = res.data?.['items'];
                model.value.Promotions = res.data?.['items'];
                try {
                    setPromtionToShow(res.data?.['items']['promotionOrderLine']);
                } catch (err) {
                    console.error('Lỗi: ', err);
                }
                response = res.data;
            })
            .catch((error) => {
                clearPromotion();
            })
            .finally(() => {
                if (callback) callback(response);
            });
    }

    function $reset() {
        activeIndexTab.value = 0;
        model.value = new PurchaseOrder();
        loadingOrderButton.value = false;
        disableOrderButton.value = false;
        orderSummary.value = new OrderSummary();
        rawPromotionOrderLine.value = [];
        promotionToShow.value = [];
        rawCommitted.value = null;
        committedToShow.value = [];
        clearPromotion();
    }

    const clearPromotion = () => {
        rawPromotionOrderLine.value = [];
        promotionToShow.value = [];
    };

    // trigger sẽ được gọi nếu data trong mảng sản phẩm thay đổi
    const itemChangeTrigger = debouncer(() => {
        // fetchPromotionMethod(() => {
        fetchCheckPriceMethod();
        // });
    }, 1000);

    watch(
        () => JSON.stringify([model.value.itemDetail]),
        (value) => {
            if (model.value.cardId) {
                disableOrderButton.value = true;
                itemChangeTrigger();
            } else {
                $reset();
            }
        }
    );

    return {
        activeIndexTab,
        isClient,
        model,
        $reset,
        // --------------------------------
        loadingOrderButton,
        disableOrderButton,
        // --------------------------------
        orderSummary,
        getOrderSummary,
        // --------------------------------
        rawPromotionOrderLine,
        getRawPromotionOrderLine,
        promotionToShow,
        setPromtionToShow,
        getPromtionToShow,
        clearPromotion,
        fetchCheckPriceMethod,
        // --------------------------------
        rawCommitted,
        committedToShow,
        getCommittedToShow,
        //---------------------------------
        promotionItems
    };
});

const groupByKey = <T>(array: T[], key: keyof T): { [key: string]: T[] } => {
    const result = {} as { [key: string]: T[] };
    if (array && array.length > 0) {
        for (const item of array) {
            if (result[`${item[key]}`]) {
                result[`${item[key]}`].push(item);
            } else {
                result[`${item[key]}`] = [item];
            }
        }
        return result;
    } else {
        return {}; // Ensure a return statement for all code paths
    }
};
