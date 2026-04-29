import { defineStore } from 'pinia';
import { PurchaseOrder } from '../types/entities';
import { OrderSummary } from '../types/orderSummary.type';
import { CheckPricePayload, PromotionInCheckPricePayload, PromotionPayload, fetchPromotion, fetchCheckPrice } from '../script';
import { PromotionOrderLine, PromotionOrderLineSub, PromotionResponse } from '../types/promotionOrderLine.type';
import { ref, computed, watch } from 'vue';
import { format } from 'date-fns';
import debouncer from 'lodash/debounce';
import { AxiosResponse } from 'axios';
import { useToast } from 'primevue/usetoast';
import cloneDeep from 'lodash/cloneDeep';
import { CommittedShow, CommittedResponse, CommitedData, CommittedLineSub } from '../types/committed.type';

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

export const usePoStore = defineStore('PurchaseStoreNET', () => {
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

    function check (itemDetail: any[]) {
        if (itemDetail.some((item) => item.priceAfterDist > item.price)) {
            disableOrderButton.value = true;
        }        
    }

    function fetchCheckPriceMethod() {
        const checkPricePayload: CheckPricePayload = {
            address: [model.value._customer.getCrD1Default('S').default as any, model.value._customer.getCrD1Default('B').default as any].filter((item) => item),
            bonus: 'PayNow',
            docType: 'NET',
            bonusAmount: 0,
            cardCode: model.value.cardCode,
            cardId: model.value.cardId,
            cardName: model.value.cardName,
            itemDetail: model.value.itemDetail.map((p) => ({
                ...p,
                quantity: p.quantity || 0,
                price: p.price || 0,
                discount: p.discount || 0
            })) as any,
            // promotion: model.value.promotion,
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
                check(checkPricePayload.itemDetail);
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
        () => {
            if (!model.value.cardId) {
                $reset();
                return;
            }  
            disableOrderButton.value = true;
            itemChangeTrigger();
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
