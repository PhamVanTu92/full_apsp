<script setup>
import { ref, onMounted, watch, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { debounce } from 'lodash';

import ProductSelector from '@/components/ProductSelector.vue';
import DisInfo from './disInfo.vue';
import { useGlobal } from '@/services/useGlobal';
import { useCartStore } from '@/Pinia/cart';
import promotionService from '@/services/promotion.service';
import apiMain from '@/api/api-main';
import format from '@/helpers/format.helper';

// ==================== COMPOSABLES ====================
const { t } = useI18n();
const router = useRouter();
const cartStore = useCartStore();
const { toast, FunctionGlobal } = useGlobal();

// ==================== CONSTANTS ====================
const DEBOUNCE_DELAY = 2000;
const DEFAULT_PAYMENT_METHOD = 'PayNow';
const VISA_PAYMENT_METHOD = 'PayVisa';

const PAYMENT_OPTIONS = [
    { name: t('client.pay_now'), code: 'PayNow' },
    { name: t('client.credit_debt'), code: 'PayCredit' },
    { name: t('client.guarantee_debt'), code: 'PayGuarantee' },
    { name: t('client.visa_credit_card'), code: VISA_PAYMENT_METHOD }
];

const OPERATION_OPTIONS = [
    { name: 'Xóa', type: 'D' },
    { name: 'Phương thức thanh toán', type: 'T' }
];

// ==================== STATE ====================
const dataProduct = ref(null);
const promotion = ref(null);
const isPromotionLoaded = ref(false);

// Cart totals
const totalAmount = ref(0);
const totalUnsecuredDebt = ref(0);
const totalSecuredDebt = ref(0);
const totalNow = ref(0);
const totalVisa = ref(0);
const totalOutput = ref(0);

// UI state
const visible = ref(false);
const visibleOperation = ref(false);
const checked = ref(false);
const tmp = ref();

// Selection state
const selectedProduct = ref([]);
const selectedOperaton = ref();
const paymentTypeChecked = ref('PayNow');

// Operation options
const operationValue = ref(OPERATION_OPTIONS);

// ==================== COMPUTED ====================
const user = computed(() => JSON.parse(localStorage.getItem('user'))?.appUser?.cardId || '');
const cartItems = computed(() => dataProduct.value?.items || []);
const hasVisaPayment = computed(() => cartItems.value.some((item) => item.paymentMethodCode === VISA_PAYMENT_METHOD));
const hasNonVisaPayment = computed(() => cartItems.value.some((item) => (item.paymentMethodCode || DEFAULT_PAYMENT_METHOD) !== VISA_PAYMENT_METHOD));
const hasMixedVisaPayment = computed(() => hasVisaPayment.value && hasNonVisaPayment.value);

const isCheckoutDisabled = computed(() => !totalAmount.value || !isPromotionLoaded.value || hasMixedVisaPayment.value);

// ==================== UTILITY METHODS ====================
const formatDate = (date) => {
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${year}-${month}-${day}`;
};

const formatCurrency = (amount) => {
    return Intl.NumberFormat('vi', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
};

const calculateItemPrice = (item) => {
    if (!item.discount) return item.price;

    if (item.discountType === 'P') {
        return item.price - (item.price * item.discount) / 100;
    }
    return item.price - item.discount;
};

// ==================== CART OPERATIONS ====================
const GetCart = async () => {
    try {
        const response = await cartStore.getCart();
        dataProduct.value = response.data;
        dataProduct.value.items = dataProduct.value.items.map((item) => ({ ...item }));
        debouncePromotion();
    } catch (error) {
        console.error('Error fetching cart:', error);
    }
};

const removeItem = async (id) => {
    try {
        await cartStore.deleteItemFromCart(id);
        GetCart();
        debouncePromotion();
        FunctionGlobal.$notify('S', 'Sản phẩm đã được xóa khỏi giỏ hàng', toast);
    } catch (error) {
        console.error('Error removing item:', error);
        FunctionGlobal.$notify('E', 'Lỗi khi xóa sản phẩm', toast);
    }
};

const onSelectedProduct = async (products) => {
    const data = products.map((item) => ({
        itemId: item.id,
        quantity: 1,
        paymentMethodCode: item.paymentMethodCode || DEFAULT_PAYMENT_METHOD
    }));

    const response = await cartStore.addToCart(data);
    if (response) {
        GetCart();
        debouncePromotion();
    }
};

const changeQuantity = async (e, item) => {
    try {
        item.quantity = e.value;
        changeQuantityApiCall(item);
        debouncePromotion();
    } catch (error) {
        console.error('Error changing quantity:', error);
        FunctionGlobal.$notify('E', 'Lỗi: ' + error.message, toast);
    }
};

const changeQuantityApiCall = debounce(async (item) => {
    try {
        await cartStore.updateQuantity(item);
    } catch (error) {
        console.error('Error updating quantity:', error);
    }
}, 0);

const changePaymentMethod = async (e, data, index) => {
    try {
        await cartStore.updateQuantity(data);
    } catch (error) {
        console.error('Error changing payment method:', error);
    }
};

// ==================== SELECTION OPERATIONS ====================
const SelectAll = () => {
    if (!checked.value) {
        selectedProduct.value = dataProduct.value.items.map((item) => item.id);
    } else {
        selectedProduct.value = [];
    }
};

const operationDialog = async (e) => {
    if (e.value.type === 'T') {
        visibleOperation.value = true;
    } else {
        await cartStore.deleteItemsFromCart(selectedProduct.value);
        GetCart();
        FunctionGlobal.$notify('S', 'Sản phẩm đã được xóa khỏi giỏ hàng', toast);
    }
};

const confirmPaymentType = () => {
    visibleOperation.value = false;

    dataProduct.value.items?.forEach((item) => {
        if (selectedProduct.value.includes(item.id)) {
            item.paymentMethodCode = paymentTypeChecked.value;
        }
    });

    if (selectedProduct.value.length > 0 && paymentTypeChecked.value) {
        apiMain
            .update(`cart/me/update-payments/${paymentTypeChecked.value}`, selectedProduct.value)
            .then(() => {
                GetCart();
                FunctionGlobal.$notify('S', 'Cập nhật phương thức thanh toán thành công', toast);
            })
            .catch(() => {
                FunctionGlobal.$notify('E', 'Cập nhật phương thức thanh toán thất bại', toast);
            });
    }

    checked.value = false;
    selectedProduct.value = [];
};

// ==================== PROMOTION OPERATIONS ====================
const getPromotion = async () => {
    isPromotionLoaded.value = false;

    try {
        // Reset promotion flags
        dataProduct.value.items
            .filter((p) => p.typePromotion)
            .forEach((p) =>
                Object.assign(p, {
                    discount: 0,
                    typePromotion: false,
                    discountType: 'P',
                    priceType: 'P'
                })
            );

        const promotionParamLine = dataProduct.value.items.map((item, index) => ({
            lineId: index,
            itemId: item.item.id,
            quantity: item.quantity
        }));

        const objPromotion = {
            id: 0,
            orderDate: formatDate(new Date()),
            cardId: JSON.stringify(user.value),
            docType: '',
            payMethod: 1,
            promotionParamLine
        };

        const response = await promotionService.getPromotion(objPromotion);
        promotion.value = response;

        applyPromotionToProducts(response.promotionOrderLine);
    } catch (error) {
        console.error('Lỗi khi tải khuyến mãi:', error);
    }

    isPromotionLoaded.value = true;
};

const applyPromotionToProducts = (promotionOrderLine) => {
    promotionOrderLine.forEach((promo) => {
        const subLines = promo.promotionOrderLineSub;
        if (!subLines?.length) return;

        const addedItems = [];

        subLines.forEach((line) => {
            const product = dataProduct.value.items.find((p) => p.item.id === line.itemId);
            if (!product) return;

            if (line.quantityAdd) {
                line.promotionName = promo.promotionName;
                line.promotionDesc = promo.promotionDesc;
                addedItems.push(line);

                Object.assign(product, {
                    lineAdd: addedItems,
                    typePromotion: true,
                    discountType: line.discountType,
                    priceType: line.priceType
                });
            }

            if (line.discount) {
                const currentDiscount = product.discount || 0;
                Object.assign(product, {
                    lineSubSub: line,
                    discount: currentDiscount + line.discount,
                    typePromotion: true,
                    discountType: line.discountType,
                    priceType: line.priceType
                });
            }
        });
    });
};

const debouncePromotion = debounce(getPromotion, DEBOUNCE_DELAY);

const discountItem = (data, index) => {
    tmp.value = data;
    visible.value = true;
};

// ==================== WATCHERS ====================
watch(
    dataProduct,
    () => {
        if (!dataProduct.value?.items) return;

        // Calculate total output
        const outputValue = dataProduct.value.items.reduce((total, item) => {
            return total + item?.item?.packing?.volumn * item.quantity;
        }, 0);
        totalOutput.value = format.FormatCurrency(outputValue);

        // Calculate totals by payment method
        let creditTotal = 0;
        let guaranteeTotal = 0;
        let nowTotal = 0;
        let visaTotal = 0;

        const total = dataProduct.value.items.reduce((sum, item) => {
            let itemTotal;

            if (item.discountType === 'P') {
                const discountedPrice = item.discount ? item.item?.price * (1 - item.discount / 100) : item.item?.price;
                itemTotal = discountedPrice * item.quantity;
            } else {
                itemTotal = item.item?.price * item.quantity - (item?.discount || 0);
            }

            // Distribute by payment method
            switch (item.paymentMethodCode) {
                case 'PayCredit':
                    creditTotal += itemTotal;
                    break;
                case 'PayGuarantee':
                    guaranteeTotal += itemTotal;
                    break;
                case 'PayNow':
                    nowTotal += itemTotal;
                    break;
                case VISA_PAYMENT_METHOD:
                    visaTotal += itemTotal;
                    break;
            }

            return sum + itemTotal;
        }, 0);

        totalAmount.value = total;
        totalSecuredDebt.value = guaranteeTotal;
        totalUnsecuredDebt.value = creditTotal;
        totalNow.value = nowTotal;
        totalVisa.value = visaTotal;
    },
    { deep: true }
);

watch(selectedProduct, (newData) => {
    if (newData.length !== dataProduct.value.items.length) {
        if (newData.length !== 0) {
            checked.value = false;
        }
    }
});

// ==================== LIFECYCLE ====================
onMounted(() => {
    GetCart();
});
</script>

<template>
    <div>
        <div class="card mb-4">
            <div class="grid">
                <!-- Main Cart Section -->
                <div class="col-9">
                    <div class="flex justify-content-between align-content-center mb-3">
                        <h5>{{ t('client.your_cart') }}</h5>
                    </div>

                    <!-- Selection Controls -->
                    <div class="ml-3 mb-3 flex justify-content-between align-items-center">
                        <div>
                            <Checkbox v-model="checked" :binary="true" @click="SelectAll()" />
                            <span class="ml-3">{{ t('body.OrderApproval.selectAll') }}</span>
                        </div>
                        <Dropdown v-if="selectedProduct?.length > 0" v-model="selectedOperaton" :options="operationValue" @change="operationDialog" optionLabel="name" placeholder="Thao tác" class="w-17rem" />
                    </div>

                    <!-- Cart Items -->
                    <div class="card">
                        <div v-for="(item, index) in dataProduct?.items" :key="index" class="grid mb-3">
                            <!-- Product Image & Checkbox -->
                            <div class="col-2 pl-0">
                                <div class="flex gap-3 align-items-center">
                                    <Checkbox v-model="selectedProduct" :inputId="item.id.toString()" :name="item.item.itemName" :value="item.id" />
                                    <div class="w-full relative">
                                        <img class="block" style="width: 100px; height: 100px" :src="item.item?.itM1[0]?.filePath || 'https://placehold.co/800x800'" :alt="item.item.itemName" />
                                    </div>
                                </div>
                            </div>

                            <!-- Product Details -->
                            <div class="col-7 pr-0">
                                <div class="flex flex-column gap-3">
                                    <div>{{ item.item.itemName }}</div>

                                    <!-- Price Display -->
                                    <div class="text-xl font-semibold">
                                        <div v-if="item.discount > 0" class="flex align-items-center gap-3">
                                            <span class="text-red-600">
                                                {{ formatCurrency(calculateItemPrice(item)) }}
                                            </span>
                                            <div class="text-sm">
                                                <span class="line-through text-gray-500">
                                                    {{ formatCurrency(item.price) }}
                                                </span>
                                                <tag class="ml-3">
                                                    {{ format.FormatCurrency(item.discount) + (item.discountType === 'P' ? ' %' : !item.currency ? ' đ' : ' $') }}
                                                </tag>
                                            </div>
                                        </div>
                                        <span v-else class="text-red-600">
                                            {{ formatCurrency(item.price) }}
                                        </span>
                                    </div>

                                    <!-- Quantity Input -->
                                    <div class="w-10rem">
                                        <InputNumber v-model="item.quantity" inputId="horizontal-buttons" showButtons :pt:input:root:style="'width: 6rem; text-align: center'" @input="changeQuantity($event, item)" buttonLayout="horizontal" :min="1">
                                            <template #incrementbuttonicon>
                                                <span class="pi pi-plus" />
                                            </template>
                                            <template #decrementbuttonicon>
                                                <span class="pi pi-minus" />
                                            </template>
                                        </InputNumber>
                                    </div>
                                </div>
                            </div>

                            <!-- Actions Column -->
                            <div class="col-3 flex gap-2 pl-0">
                                <div class="flex flex-column align-items-end gap-3 w-full">
                                    <div>
                                        <span class="text-xs mb-2 block">{{ t('client.choose_payment_method') }}</span>
                                        <Dropdown
                                            v-model="item.paymentMethodCode"
                                            :options="PAYMENT_OPTIONS"
                                            @change="changePaymentMethod($event, item, index)"
                                            optionLabel="name"
                                            optionValue="code"
                                            placeholder="PTTT"
                                            checkmark
                                            :highlightOnSelect="false"
                                            class="w-full"
                                        />
                                    </div>

                                    <Button v-if="item?.lineAdd" @click="discountItem(item, index)" label="Khuyến mãi" icon="pi pi-gift" severity="danger" outlined />
                                </div>
                                <div class="ml-2 flex">
                                    <Button text icon="pi pi-times" severity="danger" @click="removeItem(item.item.id)" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Promotion Dialog -->
                    <Dialog v-model:visible="visible" modal header="Khuyến mãi" :style="{ width: '60%' }">
                        <div class="my-2">
                            <DataTable :value="tmp.lineAdd" showGridlines scrollable>
                                <Column header="STT">
                                    <template #body="{ index }">
                                        <div class="text-center">{{ index + 1 }}</div>
                                    </template>
                                </Column>
                                <Column header="Tên sản phẩm" field="itemName" />
                                <Column header="Đơn vị tính" field="packingName" />
                                <Column header="Số lượng" field="quantityAdd" style="width: 7rem" />
                                <Column header="Chương trình khuyến mãi" field="promotionName" />
                                <Column header="Mô tả" field="promotionDesc" />
                            </DataTable>
                        </div>
                        <div class="flex justify-content-end gap-2">
                            <Button type="button" label="Hủy" severity="secondary" @click="visible = false" />
                        </div>
                    </Dialog>

                    <!-- Add Product & Total Output -->
                    <div class="grid">
                        <div class="col-3">
                            <ProductSelector :label="t('body.sampleRequest.importPlan.add_product_button')" @confirm="onSelectedProduct($event)" />
                        </div>
                        <div class="col-9 flex flex-column align-items-end">
                            <div class="text-red-500 font-bold text-lg">
                                <span class="mr-2"> {{ t('client.total_output') }} {{ totalOutput }} {{ t('client.liter') }} </span>
                                <i class="pi pi-info-circle" v-tooltip="'Giảm ngay 8% trực tiếp trên giá niêm yết (giá trước thuế) đối với đơn hàng đạt sản lượng mua tối thiểu 3.00'" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Sidebar Summary -->
                <div class="col-3">
                    <div class="card p-3">
                        <DisInfo :promotion="promotion" />
                    </div>

                    <div class="card p-3">
                        <span class="text-sm">{{ t('client.tax_and_bonus_notice') }}</span>

                        <h4 class="font-bold text-base">
                            {{ t('client.payment_today') }}
                            {{ totalNow ? Intl.NumberFormat('vi', { currency: 'vnd' }).format(totalNow) : 0 }}đ
                        </h4>

                        <h4 class="font-bold text-base">
                            {{ t('client.payment_installment') }}
                            {{ totalUnsecuredDebt ? Intl.NumberFormat('vi', { currency: 'vnd' }).format(totalUnsecuredDebt) : 0 }}đ
                        </h4>

                        <h4 class="font-bold text-base">
                            {{ t('client.payment_guarantee') }}
                            {{ totalSecuredDebt ? Intl.NumberFormat('vi', { currency: 'vnd' }).format(totalSecuredDebt) : 0 }}đ
                        </h4>

                        <h4 class="font-bold text-base">
                            {{ t('client.visa_credit_card') }}:
                            {{ totalVisa ? Intl.NumberFormat('vi', { currency: 'vnd' }).format(totalVisa) : 0 }}đ
                        </h4>

                        <h2 class="font-bold text-xl">
                            {{ t('client.total_amount') }}
                            {{ totalAmount ? Intl.NumberFormat('vi', { currency: 'vnd' }).format(totalAmount) : 0 }}đ
                        </h2>

                        <small v-if="hasMixedVisaPayment" class="block text-red-500 mb-2">
                            {{ t('client.visa_payment_all_items_required') }}
                        </small>
                        <Button class="w-full p-3 text-xl font-bold" :label="t('client.payment')" :disabled="isCheckoutDisabled" icon="pi pi-angle-right" icon-pos="right" pt:icon:class="text-2xl" @click="router.push('order/new-order')" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Payment Method Dialog -->
    <Dialog modal v-model:visible="visibleOperation" :header="t('client.payment_methods')" :style="{ width: '40%' }">
        <div class="card flex flex-wrap justify-content-around gap-3">
            <div v-for="option in PAYMENT_OPTIONS" :key="option.code" class="flex align-items-center">
                <RadioButton v-model="paymentTypeChecked" :inputId="`payment-${option.code}`" :name="option.code" :value="option.code" />
                <label :for="`payment-${option.code}`" class="ml-2">{{ option.name }}</label>
            </div>
        </div>
        <div class="flex justify-content-end gap-2">
            <Button type="button" label="Hủy" severity="secondary" @click="visibleOperation = false" />
            <Button type="button" label="Xác nhận" @click="confirmPaymentType()" />
        </div>
    </Dialog>
</template>
