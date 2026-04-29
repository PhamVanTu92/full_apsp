<template>
    <div class="product-detail-container">
        <!-- Breadcrumb & Back Button -->
        <div class="flex justify-content-between align-items-center mb-4 p-3 bg-white border-round-md shadow-1">
            <div class="flex align-items-center gap-2 text-sm">
                <i class="pi pi-home text-primary"></i>
                <span class="text-500">Trang chủ</span>
                <i class="pi pi-angle-right text-400"></i>
                <span class="text-500">Sản phẩm</span>
                <i class="pi pi-angle-right text-400"></i>
                <span class="text-900 font-medium">{{ dataEdit.product?.itemName }}</span>
            </div>
            <Button icon="pi pi-arrow-left" severity="secondary" label="Quay lại" class="w-8rem"
                v-if="route.path !== '/preview-page'" @click="router.back()"/>
        </div>

        <!-- Product Section -->
        <div v-if="dataEdit.product" class="bg-white border-round-md shadow-1 p-4 mb-4">
            <div class="grid nested-grid">
                <!-- Product Images -->
                <div class="lg:col-5 md:col-12 col-12">
                    <div class="product-images-section">
                        <div class="main-image-container bg-gray-50 border-round-md p-3 mb-3">
                            <Image width="100%" height="400px" class="border-round-md"
                                :src="dataEdit.product?.itM1[0]?.filePath || 'https://placehold.co/400x400'" preview
                                style="object-fit: cover" />
                        </div>
                        <div class="flex gap-2 overflow-auto thumbnail-container"
                            v-if="dataEdit.product.product_images?.length">
                            <div v-for="item in dataEdit.product.product_images" :key="item" class="thumbnail-item">
                                <div
                                    class="bg-gray-50 border-round-md p-2 cursor-pointer hover:shadow-2 transition-all transition-duration-200">
                                    <Image width="80px" height="80px" :src="item.image_link" preview
                                        style="object-fit: cover" class="border-round-sm" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Product Info -->
                <div class="lg:col-7 md:col-12 col-12">
                    <div class="product-info-section pl-4">
                        <!-- Product Title -->
                        <h1 class="product-title text-4xl font-bold text-900 mb-3 line-height-3">
                            {{ dataEdit.product?.itemName }}
                        </h1>

                        <!-- Price Section -->
                        <div
                            class="price-section mb-4 p-3 bg-orange-50 border-round-md border-left-3 border-orange-500">
                            <div class="flex align-items-center gap-3">
                                <span class="text-orange-600 text-3xl font-bold">
                                    {{ format.FormatCurrency(dataEdit.product.price) }} {{ dataEdit.product.currency }}
                                </span>
                            </div>
                        </div>

                        <!-- Product Description -->
                        <div class="product-description mb-4">
                            <!-- <h3 class="text-lg font-semibold text-900 mb-2">Mô tả sản phẩm</h3> -->
                            <div class="text-600 line-height-3" v-html="dataEdit.product.description"></div>
                        </div>

                        <!-- Quantity & Actions -->
                        <div class="product-actions mt-4 p-4 bg-gray-50 border-round-md">
                            <div v-if="user" class="flex flex-column gap-4">
                                <div class="flex align-items-center gap-3">
                                    <span class="font-semibold text-900">Số lượng:</span>
                                    <div class="quantity-selector">
                                        <InputGroup class="h-3rem w-10rem">
                                            <Button icon="fa-solid fa-minus" severity="secondary" raised
                                                @click="decrementQuantity" />
                                            <InputNumber v-model="dataEdit.product.quantity" class="text-center-number">
                                            </InputNumber>
                                            <Button icon="fa-solid fa-plus" @click="dataEdit.product.quantity++"
                                                severity="secondary" raised />
                                        </InputGroup>
                                    </div>
                                </div>

                                <div class="flex gap-3">
                                    <router-link :to="{
                                        name: 'client-create-order',
                                        query: { itemId: dataEdit.product?.id, quantity: dataEdit.product?.quantity }
                                    }" class="flex-1">
                                        <Button class="w-full py-3 font-semibold" label="Mua ngay" iconPos="right"
                                            icon="fa-solid fa-cart-shopping" severity="warning" />
                                    </router-link>
                                    <Button class="flex-1 py-3 font-semibold" label="Thêm vào giỏ" iconPos="right"
                                        severity="warning" icon="pi pi-cart-plus" outlined @click="AddCart" />
                                </div>
                            </div>

                            <!-- Login prompt for non-users -->
                            <div v-else class="text-center p-4">
                                <div class="mb-3">
                                    <i class="pi pi-user text-4xl text-500 mb-3"></i>
                                    <h3 class="text-xl font-semibold text-900 mb-2">Đăng nhập để mua hàng</h3>
                                    <p class="text-600 mb-4">Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng và đặt mua
                                    </p>
                                </div>
                                <div class="flex gap-3 justify-content-center">
                                    <router-link to="/login">
                                        <Button label="Đăng nhập" icon="pi pi-sign-in" class="py-3 px-6" />
                                    </router-link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Product Details Tabs -->
        <div class="bg-white border-round-md shadow-1 p-4 mb-4" v-if="dataEdit.product?.note">
            <h3 class="text-xl font-semibold text-900 mb-3">Thông tin chi tiết</h3>
            <div class="product-details-content text-600 line-height-3 mb-5" :class="{ expanded: showMoreDetail }"
                v-html="dataEdit.product.note"></div>
            <div class="action-showmore" @click="showMoreDetail = !showMoreDetail">{{ dataEdit.product.note.length > 100
                ? (showMoreDetail ? 'Ẩn bớt' : 'Xem thêm') : ''}}</div>
        </div>

        <!-- Related Products -->
        <div class="bg-white border-round-md shadow-1 p-4 mb-4">
            <h3 class="text-xl font-semibold text-900 mb-4">Sản phẩm khác</h3>
            <div class="grid">
                <div class="col-3" v-for="product in ProductsOther.slice(0, 4)" :key="product.id">
                    <div
                        class="product-card bg-white border-1 border-gray-200 border-round-md overflow-hidden hover:shadow-3 transition-all transition-duration-200 cursor-pointer">
                        <div class="product-image-container relative">
                            <img class="w-full h-10rem object-cover"
                                :src="product.itM1?.[0]?.filePath || 'https://placehold.co/200x200'"
                                :alt="product.itemName" style="object-fit: cover" />
                            <div class="absolute top-0 right-0 m-2">
                                <div class="bg-orange-500 text-white px-2 py-1 border-round-sm text-xs font-semibold"
                                    v-if="product.discount">-{{ product.discount }}%</div>
                            </div>
                        </div>
                        <div class="p-3">
                            <h4 class="text-900 font-medium mb-2 line-height-3 text-sm"
                                style="height: 2.5rem; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical">
                                {{ product.itemName }}
                            </h4>
                            <div class="flex align-items-center justify-content-between">
                                <div class="flex flex-column">
                                    <span class="text-orange-600 font-bold text-lg">
                                        {{ format.FormatCurrency(product.price) }}
                                    </span>
                                    <span class="text-gray-500 text-xs line-through" v-if="product.old_price">
                                        {{ format.FormatCurrency(product.old_price) }}
                                    </span>
                                </div>
                                <Button icon="pi pi-eye" severity="secondary" text rounded class="w-2rem h-2rem"
                                    @click="viewProduct(product.id)" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- View All Products Button -->
            <div class="text-center mt-4">
                <router-link to="/preview-page">
                    <Button label="Xem tất cả sản phẩm" icon="pi pi-arrow-right" iconPos="right" outlined />
                </router-link>
            </div>
        </div>

        <!-- Promotional Banner -->
        <!-- <div class="mb-4">
            <div class="w-full h-10rem bg-orange-100 border-2 border-dashed border-orange-300 border-round-md flex align-items-center justify-content-center">
                <div class="text-center">
                    <i class="pi pi-megaphone text-4xl text-orange-500 mb-2"></i>
                    <p class="text-orange-700 text-lg m-0 font-semibold">Banner Khuyến Mãi</p>
                    <p class="text-orange-600 text-sm m-0">Ưu đãi đặc biệt cho khách hàng</p>
                </div>
            </div>
        </div> -->

        <!-- Trust Indicators -->
        <!-- <div class="grid mb-4">
            <div class="col-4">
                <div class="bg-white border-round-md shadow-1 p-4 text-center">
                    <div class="w-4rem h-4rem bg-green-100 border-round-full flex align-items-center justify-content-center mx-auto mb-3">
                        <i class="pi pi-verified text-2xl text-green-600"></i>
                    </div>
                    <h4 class="text-900 font-semibold mb-2">Chính hãng 100%</h4>
                    <p class="text-600 text-sm m-0">Cam kết sản phẩm chính hãng</p>
                </div>
            </div>
            <div class="col-4">
                <div class="bg-white border-round-md shadow-1 p-4 text-center">
                    <div class="w-4rem h-4rem bg-blue-100 border-round-full flex align-items-center justify-content-center mx-auto mb-3">
                        <i class="pi pi-truck text-2xl text-blue-600"></i>
                    </div>
                    <h4 class="text-900 font-semibold mb-2">Giao hàng nhanh</h4>
                    <p class="text-600 text-sm m-0">Giao hàng trong 24h</p>
                </div>
            </div>
            <div class="col-4">
                <div class="bg-white border-round-md shadow-1 p-4 text-center">
                    <div class="w-4rem h-4rem bg-purple-100 border-round-full flex align-items-center justify-content-center mx-auto mb-3">
                        <i class="pi pi-shield text-2xl text-purple-600"></i>
                    </div>
                    <h4 class="text-900 font-semibold mb-2">Bảo hành tốt</h4>
                    <p class="text-600 text-sm m-0">Chế độ bảo hành uy tín</p>
                </div>
            </div>
        </div> -->
    </div>
</template>

<script setup>
import { ref, onBeforeMount, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/Pinia/auth';
import API from '@/api/api-main';
import { useGlobal } from '@/services/useGlobal';
import format from '@/helpers/format.helper';
import { useCartStore } from '../../../../Pinia/cart';

// Store & composables
const cartStore = useCartStore();
const authStore = useAuthStore();
const { toast, FunctionGlobal } = useGlobal();
const route = useRoute();
const router = useRouter();

// Reactive data
const dataEdit = ref({});
const ProductsOther = ref([]);
const payload = ref({
    itemId: 0,
    quantity: 0,
    price: 0
});
const showMoreDetail = ref(false);

// Constants
const user = localStorage.getItem('user');
const dataClear = JSON.stringify(payload.value);

// Methods
const decrementQuantity = () => {
    if (dataEdit.value.product.quantity > 0) {
        dataEdit.value.product.quantity--;
    }
};

// Watch route changes to reload product data

const GetByIdItem = async (id) => {
    if (!id) return;

    let url = user ? `Item/${id}` : `Item/${id}/bypass`;
    try {
        const res = await API.get(url);
        dataEdit.value.product = res.data;
        dataEdit.value.product.quantity = 1;
    } catch (error) {
        console.error('Error fetching product:', error);
        FunctionGlobal.$notify('E', 'Không thể tải thông tin sản phẩm', toast);
    }
};

const fetchAllProducts = async () => {
    try {
        const res = await API.get(`Item/bypass?Page=1&PageSize=12`);
        ProductsOther.value = res.data.items;
    } catch (error) {
        console.error('Error fetching products:', error);
    }
};

const AddCart = async () => {
    if (!authStore.isLoggedIn) {
        FunctionGlobal.$notify('W', 'Vui lòng đăng nhập để tiếp tục mua hàng', toast);
        return;
    }

    payload.value.quantity = dataEdit.value.product.quantity;
    payload.value.itemId = dataEdit.value.product.id;
    payload.value.price = dataEdit.value.product.price;

    try {
        const res = await cartStore.addToCart([payload.value]);
        await cartStore.getCart();

        if (res.data) {
            FunctionGlobal.$notify('S', 'Sản phẩm đã được thêm vào giỏ hàng', toast);
            ResetData();
        }
    } catch (error) {
        FunctionGlobal.$notify('E', error, toast);
    }
};

const Validate = () => {
    let status = true;

    if (parseInt(dataEdit.value.product.quantity) < 1) {
        status = false;
        FunctionGlobal.$notify('E', 'Vui lòng nhập số lượng đặt hàng', toast);
    }
    if (dataEdit.value.product.stock < parseInt(dataEdit.value.product.quantity)) {
        status = false;
        FunctionGlobal.$notify('E', 'Số lượng đặt hàng vượt quá số lượng trong kho', toast);
    }

    if (dataEdit.value.product.stock < 1) {
        status = false;
        FunctionGlobal.$notify('E', 'Hiện tại sản phẩm trong kho đang hết hàng', toast);
    }

    return status;
};

const ResetData = () => {
    payload.value = JSON.parse(dataClear);
};

const viewProduct = (productId) => {
    router.push(`/preview-page/detail/${productId}`);
};
watch(
    () => route.params.id,
    (newId, oldId) => {
        if (newId && newId !== oldId) {
            GetByIdItem(newId);
        }
    },
    {
        immediate: true,
        deep: false
    }
);
// Lifecycle
onBeforeMount(() => { 
    // GetByIdItem sẽ được gọi bởi watch với immediate: true
    fetchAllProducts();
});
</script>

<style scoped>
.product-detail-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1rem;
    background: #f8f9fa;
    min-height: 100vh;
}

.product-title {
    color: #2c3e50;
}

.main-image-container {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.thumbnail-container {
    padding: 0.5rem 0;
}

.thumbnail-item {
    flex-shrink: 0;
}

.product-info-section {
    padding-left: 1.5rem;
}

.trust-badges .col-4 {
    padding: 0.25rem;
}

.quantity-selector {
    border-radius: 8px;
    overflow: hidden;
}

.product-actions {
    border: 1px solid #e9ecef;
}

.product-details-content {
    max-height: 20rem;
    overflow-y: hidden;
    word-break: break-word;
    white-space: pre-line;
    transition: max-height 0.3s;
}

.product-details-content.expanded {
    max-height: none;
    overflow-y: visible;
}

/* Product Card Styles */
.action-showmore {
    cursor: pointer;
}

.product-card {
    transition: all 0.3s ease;
}

.product-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.product-image-container {
    position: relative;
    overflow: hidden;
}

.product-image-container img {
    transition: transform 0.3s ease;
}

.product-card:hover .product-image-container img {
    transform: scale(1.05);
}

:deep(.p-inputnumber-input) {
    text-align: center;
    border-radius: 0;
}

:deep(.p-button) {
    transition: all 0.2s ease;
}

:deep(.p-button:hover) {
    transform: translateY(-1px);
}

.p-rating .p-rating-item.p-rating-item-active .p-rating-icon {
    color: #ffd700;
}

@media (max-width: 768px) {
    .product-info-section {
        padding-left: 0;
        margin-top: 1rem;
    }

    .product-detail-container {
        padding: 0.5rem;
    }

    .grid .col-4 {
        width: 100%;
        margin-bottom: 1rem;
    }

    .grid .col-3 {
        width: 50%;
        margin-bottom: 1rem;
    }
}
</style>
