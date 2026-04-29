<script setup>
import { ref, watchEffect, watch, onBeforeUnmount, nextTick } from "vue";
import promotionService from "../../services/promotion.service";
import { cloneDeep, groupBy, merge, debounce } from "lodash";
import DetailOutputComp from "../DetailOutputComp.vue";
import { FilterStore } from "../../Pinia/FilterPromotion";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();


const user = JSON.parse(localStorage.getItem("user"))?.appUser;
const role = merge([], user.personRole?.roleClaims, user.role?.roleClaims);
const filterStore = FilterStore();
onBeforeUnmount(() => {
    filterStore.resetFilters();
});
// =================================================================
const visiblePaymentMethod = ref(false);
const selectionMethod = ref();
const onConfirmChangePayMethod = () => {
    selectionProduct.value.forEach((el) => {
        el.paymentMethodCode = selectionMethod.value;
    });
    visiblePaymentMethod.value = false;
    selectionProduct.value = [];
};

const openChangeMethod = () => {
    visiblePaymentMethod.value = true;
    selectionMethod.value = paymentMethodOptions[0].value;
};

const removeRow = () => {
    // selectedProduct.value = selectedProduct.value.filter(
    //     (el) => !selectionProduct.value.map((x) => x.id).includes(el.id)
    // );

    selectedProduct.value = selectedProduct.value.filter((_, index) => {
        return !selectionProductIndex.value.includes(index);
    });
    if (selectedProduct.value.length < 1) {
        purchaseOrderStore.resetStats();
    } else {
        getPromotion();
    }
    selectionProduct.value = [];
    selectionProductIndex.value = [];
};

const selectionProduct = ref([]);
const paymentMethodOptions = [
    { label: t('body.OrderList.cash_payment_option'), value: "PayNow" },
    { label: t('body.OrderList.credit_debt_option'), value: "PayCredit" },
    { label: t('body.OrderList.guaranteed_debt_option'), value: "PayGuarantee" },
];
const onHidePaymentMethod = () => {
    visiblePaymentMethod.value = false;
    selectionMethod.value = null;
};

// =================================================================
const modal = defineModel("selectedProduct", {
    type: Array,
    default: [],
});
const props = defineProps({
    Customer: {
        default: [],
    },
    paymentMethodCode: {
        required: false,
        default: null,
    },
    payload: {
        type: Object,
        default: null,
    },
    isIncoterm: {
        type: Boolean,
        default: false,
    },
    isClient: {
        type: Boolean,
        default: false,
    },
    currency: {
        type: String,
        default: "VND",
    },
});
const note = ref();
const loading = ref(false);
const selectedProduct = ref([]);
const expandedRowsProduct = ref({ 0: true });

import { usePurchaseOrderStore } from "../../Pinia/PurchaseOrder";
const purchaseOrderStore = usePurchaseOrderStore();
const isPromotionLoaded = ref(false);
const getPromotion = async () => {
    emits("onProductListChange", selectedProduct.value);
    isPromotionLoaded.value = false;
    selectedProduct.value
        .filter((p) => p.typePromotion)
        .forEach((p) =>
            Object.assign(p, {
                discount: 0,
                typePromotion: false,
                discountType: "P",
                priceType: "P",
            })
        );

    if (!props.Customer?.id || !selectedProduct.value.length) {
        props.payload.Promotions = [];
        return;
    }
    loading.value = true;
    const objPromotion = {
        id: 0,
        orderDate: formatData(new Date()),
        cardId: JSON.stringify(props.Customer?.id),
        payMethod: props.payload.paymentMethod[0].paymentMethodID,
        docType: "",
        promotionParamLine: selectedProduct.value.map((el, index) => ({
            lineId: index,
            itemId: el.id,
            quantity: el.quantity,
            payMethod: el.paymentMethodCode,
            discount: el.discount,
            discountType: el.discountType,
        })),
    };
    props.payload.Promotions = await promotionService.getPromotion(objPromotion);

    // Gọi hàm xử lý lấy dữ liệu tiền thanh toán
    const { promotionOrderLine } = props.payload.Promotions;
    const dataEmit = {
        promotion: props.payload.Promotions,
        items: selectedProduct.value.filter((el) => !el.hide),
    };
    emits("checkprice", dataEmit);
    promotionOrderLine.forEach((el) => {
        let subLines = el.promotionOrderLineSub;
        if (subLines?.length) {
            subLines.forEach((line) => {
                if (line.discount) {
                    const product = selectedProduct.value.find(
                        (p) => p.id === line.itemId
                    );
                    if (product) {
                        const productDiscount = product.discount || 0;
                        Object.assign(product, {
                            discount: line.discount + productDiscount,
                            typePromotion: true,
                            discountType: line.discountType,
                            priceType: line.priceType,
                        });
                    }
                }
            });
        }
    });

    setStore();
    loading.value = false;
    isPromotionLoaded.value = true;
};

// defineExpose({
//     isPromotionLoaded: ,
// });

// Nhóm các hàm xử lý sản phẩm
const updateSelectedProduct = async (event) => {
    event.forEach((data) => {
        const existingItem = selectedProduct.value.find(
            (item) => item.itemCode === data.itemCode
        );

        if (existingItem) {
            existingItem.quantity += 1;
        } else {
            const product = {
                ...data,
                paymentMethodCode: "PayNow",
                quantity: 1,
                discount: 0,
                discountType: data.discountType || "P",
                priceType: data.priceType || "P",
                typePromotion: data.typePromotion || false,
                price: data.price || 0,
                itemName: data.itemName || "",
                ougp: data.ougp || { ouom: { uomName: "" } }
            };

            selectedProduct.value.push(product);
        }
    });
    debounceF();
};

const remove = (itemcode) => {
    selectedProduct.value = selectedProduct.value.filter(
        (val) => val.itemCode !== itemcode
    );
    getPromotion();
};

// Nhóm các hàm tính toán
const CalculatorLine = (data) => {
    let discount = 0;
    const taxRate = (data.taxGroups?.rate || 0) / 100;

    // Tính toán giảm giá
    if (data.discountType === "P") {
        discount = data.price * (data.discount / 100);
    } else {
        discount = data.discount;
    }

    data.priceAfterDist = data.price - discount;
    data.distcountAmount = discount * data.quantity;
    data.vatAmount = data.priceAfterDist * taxRate * data.quantity;
    data.lineTotal = data.priceAfterDist * data.quantity;
};

// Định dạng kiểu số
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};
const Reset = () => {
    selectedProduct.value = [];
};

// const SetLabel = (key) => {
//     return key === "KH" ? t('body.sampleRequest.paymentSettings.tax') : t('body.home.item');
// };

defineExpose({
    getPromotion,
    Reset,
    isPromotionLoaded,
});

const debounceF = debounce(getPromotion, 1000);

watchEffect(() => {
    modal.value = selectedProduct.value;
});

watch(
    () => props.Customer,
    () => {
        debounceF();
    }
);

watch(
    () => props.payload.paymentMethod[0],
    () => {
        debounceF();
    }
);

const getVolumn = (data) => {
    if (!data.length) return 0;
    const total = data.reduce((sum, item) => {
        return sum + item.quantity * item.packing?.volumn;
    }, 0);

    return total;
};

const formatData = (d) => {
    const date = new Date(d);

    // Lấy các thành phần ngày, tháng, năm
    const day = String(date.getDate()).padStart(2, "0"); // Lấy ngày (dd)
    const month = String(date.getMonth() + 1).padStart(2, "0"); // Lấy tháng (mm) (lưu ý: getMonth trả về từ 0-11)
    const year = date.getFullYear(); // Lấy năm (yyyy)

    // Kết hợp thành định dạng dd/mm/yyyy
    const formattedDate = `${year}-${month}-${day}`;

    return formattedDate;
};

const emits = defineEmits(["attachFile", "onProductListChange", "checkprice"]);
const onFileChange = (data) => {
    emits("attachFile", data);
};

const getLabelPayMethod = (str) => {
    switch (str) {
        case "PayNow":
            return t('body.OrderList.cash_payment_option');
        case "PayCredit":
            return t('body.OrderList.credit_debt_option');
        case "PayGuarantee":
            return t('body.OrderList.guaranteed_debt_option');
        default:
            return "";
    }
};

// import {cloneDeep} from 'lodash';
const onClickDoubleItemRow = () => {
    const selectedItems = cloneDeep(selectionProduct.value);
    // const index = selectedProduct.value.findIndex((el) => el.id === selected.id);
    // const clone = cloneDeep(selected);
    // clone.id = Date.now();
    // clone.quantity = 1;
    // selectedProduct.value.splice(index + 1, 0, clone);
    // debounceF();  
    selectedProduct.value.push(...selectedItems);
};

const selectionProductIndex = ref([]);
const onRowSelect = (event) => {
    selectionProductIndex.value.push(event.index);
};

const onRowUnselect = (event) => {

    selectionProductIndex.value = selectionProductIndex.value.filter(
        (el) => el !== event.index
    );
};

const setStore = () => {
    purchaseOrderStore.items = selectedProduct.value;
    purchaseOrderStore.customer = props.Customer;
    purchaseOrderStore.setData(props.payload.Promotions, "KM");
};

const onChangeDiscount = (data, value) => {
    nextTick(() => {
        setStore();
    });
};

const onRowSelectAll = (event) => {
    selectionProductIndex.value = Array.from({ length: event.data.length }, (_, i) => i);
};
const onRowUnselectAll = (event) => {
    selectionProductIndex.value = [];
};
const getLaterDiscount = (data) => {
    let discount = 0;
    if (data.discountType === "P")
        discount = data.price * ((data.discount || 0) / 100);
    else
        discount = data.discount || 0;
    return formatNumber((data.price - discount) * (data.quantity || 1));
}

watch(
    () => props.Customer,
    (value) => {
        if (typeof value == "string") {
            selectedProduct.value = [];
        }
    }
);
</script>
<template>
    <div class="flex flex-column gap-3 justify-content-between" style="height: 100%">
        <DataTable v-model:selection="selectionProduct" showGridlines stripedRows
            :value="selectedProduct.filter((el) => !el.hide)" scrollable scrollHeight="450px" resizableColumns
            columnResizeMode="fit" @row-select="onRowSelect" @row-unselect="onRowUnselect"
            @row-select-all="onRowSelectAll" @row-unselect-all="onRowUnselectAll">
            <template v-if="selectionProduct.length" #header>
                <div class="flex justify-content-end gap-2">
                    <Button @click="onClickDoubleItemRow" :label="t('common.btn_duplicate')" size="small" :disabled="!role.find((e) => e.privilegeCode === 'Order.ChangeDiscount')
                        " />
                    <Button @click="openChangeMethod" :label="t('common.btn_change_payment_method')" size="small" />
                    <Button :label="t('common.btn_delete')" size="small" severity="danger" @click="removeRow" />
                </div>
            </template>
            <template #empty>
                <div class="p-5 my-5 text-center"></div>
            </template>
            <Column v-if="!props.isClient" selectionMode="multiple" style="min-width: 3rem; z-index: 1"
                class="border-right-1" frozen>
            </Column>
            <Column header="STT" :pt="{
                headerCell: {
                    class: 'border-left-none',
                },
            }" class="text-right">
                <template #body="sp">
                    <span>{{ sp.index + 1 }}</span>
                </template>
            </Column>
            <Column field="itemName" :header="t('client.product_name')" style="min-width: 200px">
                <template #body="{ data }">
                    <div class="product-name flex gap-2">
                        <span class="flex-grow-1 w-full overflow-hidden text-overflow-ellipsis">{{ data.itemName
                            }}</span>
                        <Tag v-if="data.typePromotion" value="Giảm giá"></Tag>
                        <Tag v-if="!data.price" value="KM"></Tag>
                    </div>
                </template>
            </Column>
            <Column :header="t('client.quantity')" style="min-width: 100px; text-align: end">
                <template #body="sp">
                    <InputNumber class="w-6rem" showButtons @update:modelValue="debounceF()" :min="1"
                        v-model="sp.data.quantity">
                    </InputNumber>
                    <!-- <span v-else>{{ sp.data.quantity }}</span> -->
                </template>
            </Column>
            <Column field="ougp.ouom.uomName" :header="t('client.unit_price')" style="min-width: 120px">
            </Column>
            <Column :header="`${t('client.unitPrice')} (${props.currency})`" style="min-width: 150px; text-align: end">
                <template #body="sp">
                    <span class="hidden"> {{ CalculatorLine(sp.data) }}</span>
                    <InputNumber v-if="
                        role.find((e) => e.privilegeCode === 'Order.ChangeDiscount')
                    " class="w-10rem" showButtons :min="0" v-model="sp.data.price" @update:modelValue="debounceF()">
                    </InputNumber>

                    <span v-else>{{ formatNumber(sp.data.price) }}</span>
                </template>
            </Column>
            <Column field="discount" :header="t('body.OrderList.discount_column')"
                style="min-width: 120px; text-align: end">
                <template #body="sp">
                    <span v-if="
                        role.find(
                            (e) => e.privilegeCode === 'Order.ChangeDiscount'
                        ) == undefined
                    ">{{ formatNumber(sp.data.discount) + (sp.data.discountType == 'C' ? props.currency : '%')
                    }}</span>
                    <InputNumber v-if="
                        role.find((e) => e.privilegeCode === 'Order.ChangeDiscount')
                    " class="w-6rem" showButtons :min="0" :max="100"
                        @update:modelValue="onChangeDiscount(sp.data, $event)" v-model="sp.data.discount" />
                </template>
            </Column>
            <Column :header="`${t('client.discountedPrice')} (${props.currency})`"
                style="min-width: 200px; text-align: end;">
                <template #body="sp">
                    <span> {{ getLaterDiscount(sp.data) }}</span>
                </template>
            </Column>
            <Column :header="t('client.taxRate')" style="min-width: 130px; text-align: end;">
                <template #body="sp">
                    <InputNumber v-if="role.find((e) => e.privilegeCode === 'Order.ChangeDiscount')" class="w-6rem"
                        showButtons :min="0" @update:modelValue="debounceF()" v-model="sp.data.taxGroups.rate">
                    </InputNumber>
                    <span v-else>{{ formatNumber(sp.data.taxGroups?.rate || 0) }}</span>
                </template>
            </Column>
            <Column :header="`${t('client.subtotal')} (${props.currency})`" style="min-width: 230px; text-align: end">
                <template #body="sp">
                    <span>{{
                        getLaterDiscount(sp.data)
                        }}
                    </span>
                </template>
            </Column>
            <Column :header="t('client.paymentMethod')" style="min-width: 200px">
                <template #body="sp">
                    <Dropdown v-model="sp.data.paymentMethodCode" :options="paymentMethodOptions" optionLabel="label"
                        optionValue="value" :disabled="props.isClient && 0" @change="debounceF()"></Dropdown>
                </template>
            </Column>
            <template #footer>
                <div class="flex justify-content-between align-items-center flex-column md:flex-row gap-2">
                    <div>
                        <template v-if="typeof props.Customer == 'object'">
                            <ProductSelector v-if="!props.isClient" icon="pi pi-plus" :label="t('common.btn_add_product')" outlined
                                :customer="props.Customer" @confirm="updateSelectedProduct($event)" />
                        </template>
                    </div>
                    <div class="flex align-items-center gap-2">
                        <div class="text-yellow-800">
                            {{ t('client.total_output') }}
                            {{
                                formatNumber(
                                    getVolumn(selectedProduct.filter((el) => !el.hide))
                                )
                            }}
                            {{ t('client.liter') }}
                        </div>
                        <DetailOutputComp :data="selectedProduct.filter((el) => !el.hide)"></DetailOutputComp>
                    </div>
                </div>
            </template>
        </DataTable>
        <DataTable showGridlines="" stripedRows :value="props?.payload?.Promotions?.promotionOrderLine || []"
            tableStyle="min-width: 50rem" scrollable scrollHeight="450px" class="mt-3"
            v-model:expandedRows="expandedRowsProduct" :loading="loading" dataKey="lineId">
            <template #empty>
                <div class="py-5 my-5"></div>
            </template>
            <template #header>
                <div class="text-lg font-bold">{{ t('body.systemSetting.promotions') }}</div>
            </template>
            <Column expander style="width: 5rem" />
            <Column field="promotionName" style="width: 25%" :header="t('body.systemSetting.promotion_programs')">
            </Column>
            <Column field="promotionDesc" :header="t('body.systemSetting.description_column')"></Column>
            <template #expansion="{ data }">
                <div v-if="data.promotionOrderLineSub?.filter((e) => !e.discount).length">
                    <div class="border-1 border-200 flex" style="border-collapse: collapse">
                        <div class="w-6 font-bold border-200 p-3">{{ t('client.productName') }}</div>
                        <div class="w-2 font-bold p-3 border-left-1 border-200 ml-0">
                            ĐVT
                        </div>
                        <div class="w-1 font-bold p-3 border-left-1 border-200 ml-0 text-right">
                            {{ t('client.quantity') }}
                        </div>
                        <div class="w-2 font-bold p-3 border-left-1 border-200 ml-0 text-center">
                            {{ t('client.product_type') }}
                        </div>
                        <div class="w-1 font-bold p-3 border-left-1 border-200 ml-0 text-center">
                            Dòng
                        </div>
                    </div>
                    <div v-for="(value, key) in groupBy(
                        data.promotionOrderLineSub?.filter((e) => !e.discount),
                        'lineId'
                    )" :key="key">
                        <!-- <h6 class="mt-3">Dòng sản phẩm {{ parseInt(key) + 1 }}</h6> -->

                        <div v-for="(_value, groupKey) in groupBy(value, 'inGroup')" :key="groupKey">
                            <div>
                                <span class="hidden" v-if="_value[0].cond == 'OR' && !value[0].selected">
                                    <template>{{
                                        (value[0].selected = key + "_" + groupKey)
                                        }}</template>
                                </span>
                                <div class="flex justify-content-between mb-0" v-for="(_item, idx) in _value"
                                    :key="idx">
                                    <span class="w-6 font-normal flex gap-2 p-3 border-left-1 border-200">
                                        <div class="flex justify-content-start" v-if="_value[0].cond == 'OR' && !idx">
                                            <div class="flex gap-2">
                                                <RadioButton v-model="value[0].selected" :value="key + '_' + groupKey"
                                                    :name="key + '_inGroup'">
                                                </RadioButton>
                                                {{ groupKey }}
                                                <!-- @change="debounceF()" -->
                                            </div>
                                        </div>
                                        <span :class="{
                                            'ml-4': _value[0].cond == 'OR' && idx,
                                        }">{{ _item.itemName }}</span>
                                    </span>
                                    <span class="w-2 font-normal p-3 border-left-1 border-200">{{ _item.packingName
                                        }}</span>
                                    <span class="w-1 font-normal text-right p-3 border-left-1 border-200">{{
                                        _item.quantityAdd }}</span>
                                    <span class="w-2 font-normal p-3 border-left-1 border-200">{{
                                        t('body.home.item') }}
                                    </span>
                                    <span class="w-1 font-normal text-center p-3 border-x-1 border-200">{{ _item.lineId
                                        + 1 }}</span>
                                </div>
                                <hr class="m-0" />
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </DataTable>
        <!-- <DocumentsComp></DocumentsComp> -->
        <FilesAttachment v-if="props.isIncoterm" class="my-3" @change="onFileChange" />
        <div class="flex flex-column gap-2">
            <label for="note" class="font-semibold">{{ t('client.note') }}</label>
            <Textarea id="note" class="w-full" v-model="note" @change="props.payload.note = note" rows="4"></Textarea>
        </div>
    </div>
    <Dialog v-model:visible="visiblePaymentMethod" :header="t('common.dialog_change_payment_method')" modal class="w-25rem"
        @hide="onHidePaymentMethod">
        <Dropdown v-model="selectionMethod" class="w-full" :placeholder="t('common.placeholder_select_payment_method')"
            :options="paymentMethodOptions" optionLabel="label" optionValue="value"></Dropdown>
        <template #footer>
            <Button @click="selectionMethod = false" :label="t('common.btn_close')" severity="secondary" />
            <Button @click="onConfirmChangePayMethod" :label="t('common.btn_select')" />
        </template>
    </Dialog>
</template>

<style scoped>
.product-name {
    width: 30rem;
    overflow: hidden;
}
</style>
