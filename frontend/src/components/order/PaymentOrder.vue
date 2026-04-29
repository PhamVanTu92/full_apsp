<script setup>
import { ref, computed, reactive } from "vue";
import { useGlobal } from "@/services/useGlobal";
import API from "@/api/api-main";
import RButton from "./RButton.vue";
import { cloneDeep } from "lodash";
import { useRoute, useRouter } from "vue-router";
const { toast, FunctionGlobal } = useGlobal();
const loading = ref(false);
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const router = useRouter();

let totalUnsecuredDebt = ref(0);
let totalSecuredDebt = ref(0);
let totalNow = ref(0);

const expandedRows_1 = ref();

const props = defineProps({
    selectedProduct: {
        type: Array,
        default: [],
    },
    Customer: {
        default: [],
    },
    payload: {
        type: Object,
        default: {},
    },
    attachFile: {
        type: Array,
        default: [],
    },
    isClient: {
        type: Boolean,
        default: false,
    },
    type: {
        type: String,
    },
    isUserInfoLoaded: {
        type: Boolean,
        default: false,
    },
    isPromotionLoaded: {
        type: Boolean,
        default: false,
    },
    tienGiamSanLuong: {
        default: null,
    },
    currency: {
        type: String,
        default: "VND",
    },
});

const currencySymbol = reactive({
    VND: "đ",
    USD: "$",
});

const emit = defineEmits(["OrderSuccessful", "clear-cart", "AddOrder"]);
const isSuccessResponse = (res) => res?.status >= 200 && res?.status < 300;

const CalculatorTotal = () => {
    const dis = 0;
    let T = 0; //Tin chap
    let B = 0; //Bao lanh
    let N = 0; // Thanh toan ngay
    let { TotalGoods, TotalDiscount, TotalVatAmount } = props.selectedProduct.reduce(
        (totals, el) => {
            if (!el.hide) {
                const discount = el.lineTotal * (dis / 100);
                const vat = (el.lineTotal - discount) * (el.taxGroups?.rate / 100);
                totals.TotalGoods += el.lineTotal;
                totals.TotalDiscount += discount;
                totals.TotalVatAmount += vat;

                // const itemTotal = el.discount
                //   ? item.item?.price * (1 - el.discount / 100) * item.quantity // Áp dụng giảm giá
                //   : item.item?.price * el.quantity; // Không có giảm giá, tính theo giá * số lượng

                const itemTotal = el.discount
                    ? el.price * (1 - el.discount / 100) * el.quantity
                    : el.price * el.quantity;

                if (el.paymentMethodCode == "PayCredit") {
                    T += itemTotal;
                } else if (el.paymentMethodCode == "PayGuarantee") {
                    B += itemTotal;
                } else if (el.paymentMethodCode == "PayNow") {
                    N += itemTotal;
                }
            }
            return totals;
        },
        { TotalGoods: 0, TotalDiscount: 0, TotalVatAmount: 0 }
    );
    totalSecuredDebt.value = B;
    totalUnsecuredDebt.value = T;
    totalNow.value = N;
    // Tính gift một cách độc lập
    const gift = paymentMethodCode.value === "PayNow" ? payNowGift.value : 0;
    const TotalPayment = Math.round(TotalVatAmount + TotalGoods - TotalDiscount - gift);

    props.payload.distcountAmount = Math.round(TotalDiscount);
    props.payload.vatAmount = Math.round(TotalVatAmount);

    return {
        TotalVatAmount: Math.round(TotalVatAmount),
        TotalDiscount: Math.round(TotalDiscount),
        TotalPayment,
        TotalGoods: Math.round(TotalGoods),
    };
};

// Định dạng kiểu số
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(Math.round(num));
};
import { usePurchaseOrderStore } from "../../Pinia/PurchaseOrder";
const {
    getOrderStats,
    orderStat,
    orderError,
    tienGiamSanLuong,
    sanLuong,
    getTienGiamSanLuong,
    getPromotion,
} = usePurchaseOrderStore();
// Nhóm các hàm xử lý đơn hàng 

const SaveOrder = async () => {
    const { id, cardName, cardCode, crD1 } = props.Customer || {};
    const { selectedProduct, payload } = props;
    if (!id) return FunctionGlobal.$notify("E", "Vui lòng chọn khách hàng", toast);
    if (!selectedProduct.length)
        return FunctionGlobal.$notify("E", "Vui lòng chọn sản phẩm", toast);

    const defaultAddress = crD1?.filter((el) => el.default === "Y") || {};
    payload.cardId = id || 0;
    payload.cardName = cardName || "";
    payload.cardCode = cardCode || "";
    payload.address =
        defaultAddress.map((el) => ({
            address: el.address,
            locationId: el.locationId,
            locationName: el.locationName,
            areaId: el.areaId,
            areaName: el.areaName,
            email: el.email,
            phone: el.phone,
            person: el.person,
            note: el.note,
            type: el.type,
        })) || [];
    payload.itemDetail = selectedProduct.map((el) => ({
        itemId: el.id,
        itemCode: el.itemCode,
        itemName: el.itemName,
        quantity: el.quantity || 0,
        price: el.price || 0,
        priceAfterDist: el.priceAfterDist || 0,
        discount: el.discount || 0,
        distcountAmount: el.distcountAmount || 0,
        vatCode: el.taxGroups?.code,
        vat: el.taxGroups?.rate,
        vatAmount: el.vatAmount,
        lineTotal: el.lineTotal,
        note: el.note,
        ouomId: el.ougp.baseUom,
        uomCode: el.ougp.ouom.uomCode,
        uomName: el.ougp.ouom.uomName,
        numInSale: el.packing.volumn * el.quantity,
        paymentMethodCode: el.paymentMethodCode || null,
    }));
    payload.bonus =
        paymentMethodCode.value === "PayNow" ? getOrderStats.bonusPercent * 100 : 0;
    payload.bonusAmount = paymentMethodCode.value === "PayNow" ? payNowGift.value : 0;
    payload.totalBeforeVat = totalBeforeVat.value;
    payload.total = CalculatorTotal().TotalPayment;
    payload.currency = "VND";

    const Promotions = cloneDeep(payload.Promotions);
    let product = [];
    let errorPromtion = false;
    if (Promotions.promotionOrderLine?.length && false) {
        payload.promotion = Promotions.promotionOrderLine.map((e) => ({
            promotionId: e.promotionId,
            promotionCode: e.promotionCode,
            promotionName: e.promotionName,
            promotionDesc: e.promotionDesc,
            lineId: e.lineId,
        }));
        Promotions.promotionOrderLine.forEach((el) => {
            el.promotionOrderLineSub.forEach((item) => {
                item.promotionId = el.promotionId;
            });

            if (
                el.promotionOrderLineSub.filter((e) => e.cond === "OR").length &&
                !el.promotionOrderLineSub.filter((e) => e.selected).length
            ) {
                errorPromtion = true;
            }

            // Lấy sản phẩm có dk AND
            el.promotionOrderLineSub
                .filter((e) => e.cond === "AND" && !e.discount)
                .forEach((e) => {
                    product.push(e);
                });

            // Lấy sp có dk OR
            el.promotionOrderLineSub
                .map((e) => e.selected)
                .filter((e) => e != undefined && e.cond === "OR")
                .forEach((e) => {
                    const item = el.promotionOrderLineSub.filter(
                        (es) =>
                            es.lineId === parseInt(e.split("_")[0]) &&
                            es.inGroup === parseInt(e.split("_")[1])
                    );
                    if (item.length) {
                        item.forEach((es) => {
                            product.push(es);
                        });
                    }
                });
        });
    }
    payload.promotion = getPromotion.value;
    if (errorPromtion) {
        FunctionGlobal.$notify("E", "Vui lòng chọn sản phẩm tặng", toast);
        return;
    }
    if (product.length) {
        product = product.map((el) => ({
            promotionCode:
                payload.promotion.find((e) => e.promotionId == el.promotionId)
                    ?.promotionCode || "",
            promotionName:
                payload.promotion.find((e) => e.promotionId == el.promotionId)
                    ?.promotionName || "",
            promotionDesc:
                payload.promotion.find((e) => e.promotionId == el.promotionId)
                    ?.promotionDesc || "",
            type: el.itemGroup,
            promotionId: el.promotionId,
            itemId: el.itemId,
            itemCode: el.itemCode,
            itemName: el.itemName,
            itemId: payload.itemDetail[el.lineId].itemId,
            quantityAdd: el.quantityAdd,
            packingName: el.packingName,
            cond: el.cond,
            lineId: el.lineId,
            numInSale: el.volumn * el.quantityAdd || 0,
        }));
        payload.promotion = product;
    }
    if (payload?.address.length < 1) {
        FunctionGlobal.$notify(
            "E",
            "Vui lòng thêm thông tin nhận hàng & xuất hóa đơn",
            toast
        );
        return;
    }

    try {
        loading.value = true;
        payload.quarterlyCommitmentBonus = props.tienGiamSanLuong?.quarterTotal || 0;
        payload.yearCommitmentBonus = props.tienGiamSanLuong?.yearTotal || 0;
        // if (props.tienGiamSanLuong.length > 0) {
        //     props.tienGiamSanLuong.forEach((row) => {
        //         if (row.label.includes("Quý")) {
        //             payload.quarterlyCommitmentBonus = row.value;
        //         }
        //         if (row.label.includes("Năm")) {
        //             payload.yearCommitmentBonus = row.value;
        //         }
        //     });
        // } 
        // return;
        // console.log(JSON.stringify(payload).length);
        // return
        const res = await API.add(`PurchaseOrder/add`, payload);
        const data = res?.data;
        if (!isSuccessResponse(res)) {
            throw res;
        }
        if (props.isClient) {
            emit("clear-cart", true);
        } else {
            router.back();
        }
        // if (paymentMethodTotal.PayNowFinal > 0 && props.isClient) {
        //     // route to màn hình nhập số tìn trả 
        //     const payData = {
        //         docId: data.id,
        //         paymentAmount: paymentMethodTotal.PayNowFinal + "00",
        //         paymentMethodId: 0,
        //     };
        //     const payRes = await API.add(`payment`, payData);
        //     const redirectLink = payRes.data.redirectLink;
        //     if (redirectLink) {
        //         // Chuyển hướng one payment
        //         // window.open(redirectLink, "_self");
        //         router.push({
        //             name: "payment-status",
        //             query: {
        //                 vpc_TxnResponseCode: 0,
        //                 vpc_OrderInfo: data.id,
        //                 type: "order",
        //             },
        //         });
        //     } 
        // } else {
        //     emit("AddOrder", data);
        //     FunctionGlobal.$notify("S", "Thêm thành công!", toast);
        // }
        FunctionGlobal.$notify("S", t('Custom.addnewSuccess'), toast);
        if (data) {
            emit("AddOrder", data);
            if (data.id) {
                sentAttachFiles(data.id);
            }
        } else if (props.isClient) {
            router.replace({
                name: "hisPur",
            });
        }
    } catch (error) {
        if (isSuccessResponse(error?.response)) {
            if (props.isClient) {
                emit("clear-cart", true);
                router.replace({
                    name: "hisPur",
                });
            } else {
                router.back();
            }
            FunctionGlobal.$notify("S", t('Custom.addnewSuccess'), toast);
            return;
        }
        console.error(error)

        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

const sentAttachFiles = async (targetId) => {
    if (props.attachFile.length == 0) return;
    try {
        const formdata = new FormData();
        for (let { file } of props.attachFile) {
            formdata.append("files", file);
        }
        await API.add(`PurchaseOrder/${targetId}/AttFiles`, formdata);
    } catch (err) { }
};

// Nhóm computed
const showDebtCheck = computed(() => !!props.Customer?.crD4);

const totalPayment = computed(() => {
    return paymentMethodCode.value !== "PayNow"
        ? CalculatorTotal().TotalPayment
        : paymentMethodCode.value
            ? CalculatorTotal().TotalPayment - payNowGift.value
            : 0;
});
const itemDebt = computed(() =>
    props.payload.paymentMethod[0].paymentMethodCode !== "PayNow"
        ? {
            paymentMethodName: props.payload.paymentMethod[0].paymentMethodName,
            amountOverdue: CalculatorTotal().TotalPayment,
            type: "draft",
        }
        : null
);

const paymentMethodCode = computed(
    () => props.payload.paymentMethod[0].paymentMethodCode
);
const payNowGift = computed(() => {
    const totalGoods = props.selectedProduct.reduce((sum, el) => {
        return !el.hide
            ? sum +
            (el.quantity * el.price +
                el.quantity * el.price * (el.taxGroups?.rate / 100))
            : sum;
    }, 0);
    return (totalGoods * 2) / 100;
});
const totalBeforeVat = computed(() => {
    const totalGoods = props.selectedProduct.reduce((sum, el) => {
        return !el.hide ? sum + el.quantity * el.price : sum;
    }, 0);
    return totalGoods;
});
const totalAfterVat = computed(() => {
    const totalGoods = props.selectedProduct.reduce((sum, el) => {
        return !el.hide
            ? sum +
            (el.quantity * el.price -
                (el.quantity * el.price * el.taxGroups.rate) / 100)
            : sum;
    }, 0);
    return totalGoods;
});

const getThanhTienSauThue = (methodType) => {
    const paynowProducts = props.selectedProduct.filter(
        (el) => el.paymentMethodCode === methodType
    );
    const amount = paynowProducts.reduce((sum, el) => {
        let afterDiscount = el.price - (el.price * el.discount) / 100;
        let taxValue = (afterDiscount * el.taxGroups.rate) / 100;
        let afterTax = (afterDiscount + taxValue) * el.quantity;
        return afterTax + sum;
    }, 0);
    return roundNumber(amount);
};

const paymentMethodTotal = reactive({
    PriceBeforeTAX: computed(() => {
        const value = props.selectedProduct.reduce((sum, el) => {
            return sum + el.totalLine;
        }, 0);
        return roundNumber(value);
    }),
    GiftPayNow: computed(() => {
        const paynowProducts = props.selectedProduct.filter(
            (el) => el.paymentMethodCode === "PayNow"
        );
        const value = paynowProducts.reduce((sum, el) => {
            // let result = ((el.totalLine * el.taxGroups.rate) / 100 + el.totalLine) * 0.02;
            const taxRate = (el.taxGroups.rate + 100) / 100;
            let result = el.price * el.quantity * 0.02 * taxRate;
            return result + sum;
        }, 0);
        return roundNumber(value);
    }),
    TAXs: computed(() => {
        const result = props.selectedProduct.reduce((sum, el) => {
            return sum + el.totalLine * (el.taxGroups.rate / 100);
        }, 0);
        return roundNumber(result);
    }),
    PayNowAfterTax: computed(() => getThanhTienSauThue("PayNow")),
    PayNowFinal: computed(() => {
        return paymentMethodTotal.PayNowAfterTax - paymentMethodTotal.GiftPayNow;
    }),
    PayCredit: computed(() => getThanhTienSauThue("PayCredit")),
    PayGuarantee: computed(() => getThanhTienSauThue("PayGuarantee")),
    totalPay: computed(() => {
        return (
            paymentMethodTotal.PayNowFinal +
            paymentMethodTotal.PayCredit +
            paymentMethodTotal.PayGuarantee
        );
    }),
});

const roundNumber = (number) => {
    if (number) {
        return Math.round(number * 100) / 100;
    }
    return 0;
};

const visibleTCK = ref(false);
const onClickThuongChietKhau = () => {
    visibleTCK.value = true;
};

const visibleDetail = ref(false);
const header = ref("");
const dataTableInfo = ref([]);
const sumDataTableInfo = computed(() => {
    return dataTableInfo.value?.reduce((a, b) => {
        return a + b.value;
    }, 0);
});
const types = {
    // Giá trị đơn hàng sau CK
    1: [
        {
            value: "totalPayNowBeforeVat", // "TotalPayNowBeforeVat",
            label: "Thanh toán ngay",
        },
        {
            value: "totalDebtBeforeVat", //"totalDebtBeforeVat",
            label: "Tín chấp",
        },
        {
            value: "totalDebtGuaranteeBeforeVat", //"totalDebtGuaranteeBeforeVat",
            label: "Bảo lãnh",
        },
    ],
    // Tổng tiền thuế
    2: [
        {
            value: "paynowVATAmount", // "TotalPayNowBeforeVat",
            label: "Thanh toán ngay",
        },
        {
            value: "debtVATAmount", //"totalDebtBeforeVat",
            label: "Tín chấp",
        },
        {
            value: "debtGuaranteeVATAmount", //"totalDebtGuaranteeBeforeVat",
            label: "Bảo lãnh",
        },
    ],
};
const onClickOpenDetail = (_type, _header) => {
    // dataTableInfo.value = types[_type];
    dataTableInfo.value = types[_type].map((item) => {
        return {
            label: item.label,
            value:
                (item.value == "totalPayNowBeforeVat" ? getOrderStats[item.value] - getOrderStats.bonusCommited : getOrderStats[item.value]) || 0,
        };
    });
    header.value = _header;
    visibleDetail.value = true;
};

const isShowTermConfirm = computed(() => {
    const isOk = props.selectedProduct.some((item) => item.paymentMethodCode == "PayNow");
    if (isOk) isConfirmTerm.value = false;
    return isOk;
});
const isConfirmTerm = ref(false);

import PDFView from "../../components/PDFViewer/PDFView.vue";
const visibleTerm = ref(false);
const termData = ref({});
const onClickThongBao = () => {
    API.get("article?Page=1&PageSize=1&Filter=status=A")
        .then((res) => {
            termData.value = res.data.item?.[0] || {};
            visibleTerm.value = true;
        })
        .catch((error) => { });
};
</script>
<template>
    <PDFView v-model:visible="visibleTerm" v-if="visibleTerm" :url="termData?.filePath" :header="termData?.name">
    </PDFView>
    <Dialog modal :header="header" class="w-30rem" v-model:visible="visibleDetail">
        <!-- <div>{{ props.tienGiamSanLuong }}</div> -->
        <DataTable :value="dataTableInfo" showGridlines>
            <Column>
                <template #body="slotProps">
                    <span>{{ slotProps.data.label }}</span>
                </template>
            </Column>
            <Column header="Giá trị (VND)" class="text-right">
                <template #body="slotProps">
                    {{ Intl.NumberFormat().format(slotProps.data.value) }}</template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :footer="t('body.home.total')" />
                    <Column class="text-right" :footer="Intl.NumberFormat().format(sumDataTableInfo)" />
                </Row>
            </ColumnGroup>
        </DataTable>
    </Dialog>
    <Dialog modal header="Thưởng sản lượng" class="w-30rem" v-model:visible="visibleTCK">
        <DataTable :value="getTienGiamSanLuong?.lines?.filter((el) => el.value > 0) || []" showGridlines>
            <Column header="Loại thưởng">
                <template #body="slotProps">
                    <span>{{ slotProps.data?.label }}</span>
                </template>
            </Column>
            <Column header="Giá trị (VND)" class="text-right">
                <template #body="slotProps">
                    <span>{{
                        Intl.NumberFormat().format(slotProps.data?.value || 0)
                    }}</span>
                </template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :footer="t('body.home.total')" />
                    <Column class="text-right" :footer="Intl.NumberFormat().format(getTienGiamSanLuong?.total)" />
                </Row>
            </ColumnGroup>
        </DataTable>
    </Dialog>
    <!-- <DebtCheck
        v-if="showDebtCheck && 0"
        :DebtData="Customer.crD4"
        :TotalDebtData="Customer.crD3"
        :TotalPayment="totalPayment"
        :ItemDebt="itemDebt"
        :paymentMethod="paymentMethodCode"
    /> -->
    <KiemTraCongNo :bpId="Customer?.id" :bpName="Customer?.cardName" :payCredit="paymentMethodTotal.PayCredit"
        :payGuarantee="paymentMethodTotal.PayGuarantee" />
    <div class="card p-4 mb-0 border-primary flex flex-column gap-3">
        <RButton icon="pi pi-ticket" :label="t('client.promotion_for_you')"></RButton>
        <div class="flex flex-column gap-3">
            <div class="flex flex-wrap align-items-center justify-content-between">
                <span>{{ t('body.OrderList.order_value_before_discount') }}:</span>
                <span>
                    {{ formatNumber(orderStat.totalBeforeVat) }}
                </span>
            </div>
            <div class="flex flex-wrap align-items-center justify-content-between">
                <span>{{ t('body.OrderList.other_promotions_value') }} </span>
                <span>{{ formatNumber(CalculatorTotal().TotalDiscount) }} </span>
            </div>
            <div v-if="!props.payload.isIncoterm" class="flex flex-wrap align-items-center justify-content-between">
                <span class="flex gap-2 align-items-center">{{ t('body.OrderList.commitment_bonus') }}:
                    <i v-if="getOrderStats.bonusCommited" @click="onClickThuongChietKhau"
                        class="pi pi-info-circle text-primary cursor-pointer"></i></span>
                <span>{{ Intl.NumberFormat().format(getOrderStats.bonusCommited || 0) }}
                </span>
            </div>
            <div class="flex flex-wrap align-items-center justify-content-between">
                <div class="flex gap-2 align-items-center">
                    <span>{{ t('body.OrderList.payment_bonus') }}: </span>
                    <Button size="" class="p-0 m-0" style="width: 16px; height: 16px" v-if="
                        props.payload.paymentMethod[0].paymentMethodCode ===
                        'PayNow' && getOrderStats.bonusPercent
                    " v-tooltip="{
                        value: `Giảm ngay ${getOrderStats.bonusPercent * 100
                            }% trên tổng giá niêm yết`,
                        showDelay: 100,
                        hideDelay: 300,
                    }" text icon="pi pi-info-circle" />
                </div>
                <span>
                    {{ formatNumber(getOrderStats.bonusAmount) }}
                </span>
            </div>
            <hr class="border-300 my-0" />
            <div class="flex flex-wrap align-items-center justify-content-between">
                <span>{{ t('body.OrderList.order_value_after_discount') }}:
                    <i @click="onClickOpenDetail(1, 'Giá trị đơn hàng sau CK')"
                        class="pi pi-info-circle text-primary cursor-pointer"></i></span>
                <span>
                    {{
                        formatNumber(
                            (getOrderStats.totalBeforeVat || 0) -
                            (getOrderStats.bonusCommited || 0)
                        )
                    }}
                </span>
            </div>
            <div class="flex flex-wrap align-items-center justify-content-between">
                <span>{{ t('body.OrderList.total_tax_amount') }}:
                    <i @click="onClickOpenDetail(2, t('client.total_tax'))"
                        class="pi pi-info-circle text-primary cursor-pointer"></i></span>
                <span>
                    {{ formatNumber(getOrderStats.vatAmount) }}
                </span>
            </div>
            <hr class="border-300 my-0" />
            <div class="flex flex-wrap justify-content-between">
                <span>{{ t('client.pay_now') }}:</span>
                <!-- {{ formatNumber(paymentMethodTotal.PayNowFinal) }} -->
                <span>{{ formatNumber(getOrderStats.totalPayNow) }} </span>
            </div>
            <div class="flex flex-wrap justify-content-between">
                <span>{{ t('client.credit_debt') }}:</span>
                <!-- {{ formatNumber(paymentMethodTotal.PayCredit) }} -->
                <span>
                    {{ formatNumber(getOrderStats.totalDebt) }}
                </span>
            </div>
            <div class="flex flex-wrap justify-content-between">
                <span>{{ t('client.guarantee_debt') }}:</span>
                <!-- {{ formatNumber(paymentMethodTotal.PayGuarantee) }} -->
                <span>
                    {{ formatNumber(getOrderStats.totalDebtGuarantee) }}
                </span>
            </div>
            <hr class="my-0" />
            <div class="flex flex-wrap align-items-center justify-content-between">
                <span>{{ t('body.OrderList.total_order_value') }}:</span>
                <span>{{ formatNumber(getOrderStats.totalAfterVat) }}</span>
            </div>
            <hr class="my-0" />
            <div class="my-2 flex flex-wrap align-items-center text-lg justify-content-between">
                <strong>{{ t('body.OrderList.total_payment') }}:</strong>
                <span>
                    {{
                        formatNumber(
                            getOrderStats.totalAfterVat - getOrderStats.bonusAmount
                        )
                    }}
                </span>
            </div>
            <div v-if="isShowTermConfirm && props.isClient" class="flex gap-2 my-2">
                <Checkbox v-model="isConfirmTerm" binary></Checkbox>
                <label for="">{{ t('client.agree_read_and_accept') }}
                    <span @click="onClickThongBao" class="text-blue-500 cursor-pointer hover:underline">{{
                        t('client.personal_data_notice') }}</span>
                    {{ t('client.company_suffix') }}</label>
            </div>
            <Button icon="fa-solid fa-cart-shopping" class="p-3 text-xl" :label="t('body.OrderList.place_order_button')"
                icon-pos="right" @click="SaveOrder" :loading="loading" :disabled="(props.isClient &&
                    !props.isUserInfoLoaded &&
                    !props.isPromotionLoaded) ||
                    orderError ||
                    (isShowTermConfirm && !isConfirmTerm && props.isClient)
                    " />
        </div>
    </div>
    <Loading v-if="loading"></Loading>
</template>
