<template>
    <div v-if="dataEdit.product" class="grid nested-grid mt-5">
        <div class="lg:col-4 md:col-12 col-12">
            <div class="text-center bg-white border-round-md">
                <Image width="100%" class="p-3" height="auto" :src="dataEdit.product?.itM1[0]?.filePath || 'https://placehold.co/200x200'" preview />
            </div>
            <div class="flex mt-2 gap-2 overflow-auto">
                <div v-for="item in dataEdit.product.product_images" :key="item">
                    <div class="bg-white py-3 text-center border-round-md">
                        <Image width="100px" height="100px" :src="item.image_link" preview />
                    </div>
                </div>
            </div>
        </div>
        <div class="lg:col-8 md:col-12 col-12">
            <div class="grid m-0">
                <div class="" style="word-wrap: break-word">
                    <div>
                        <h3 class="font-bold text-3xl">{{ dataEdit.product?.itemName }}</h3>
                        <div class="m-5">
                            <div class="text-green-700 text-2xl font-medium my-3">
                                {{ format.FormatCurrency(dataEdit.product.price) }}
                            </div> 
                        </div>
                    </div>
                    <div class="w-full bg-gray-400 my-3" style="height: 1px"></div>
                    <div v-if="user" class="flex gap-3 align-items-center my-5">
                        <span class="mr-5">{{ t('client.quantity') }}:</span>
                        <div class="w-10rem">
                            <InputGroup class="h-3rem">
                                <Button icon="fa-solid fa-minus" severity="secondary" raised @click="decrementQuantity" />
                                <InputNumber v-model="dataEdit.product.quantity" class="text-center-number"></InputNumber>
                                <Button icon="fa-solid fa-plus" @click="dataEdit.product.quantity++" severity="secondary" raised />
                            </InputGroup>
                        </div>
                        <!-- <span><b>{{ numeral(dataEdit.product.stock).format("0,0") }}</b> sản phẩm có sẵn</span> -->
                    </div>
                    <div>
                        <h3 class="font-bold text-3xl">Mô tả</h3>
                        <div class="mt-2">{{ dataEdit.product.description }}</div>
                    </div>
                    <div v-if="user" class="my-5">
                        <router-link
                            :to="{
                                name: 'client-create-order',
                                query: { itemId: dataEdit.product?.id, quantity: dataEdit.product?.quantity }
                            }"
                        >
                            <Button class="py-3 px-6 mr-3" :label="t('client.buy_now')" iconPos="right" icon="fa-solid fa-cart-shopping" severity="warning" />
                        </router-link>
                        <!-- <router-link :to="{ name: 'cart' }"> -->
                        <Button class="py-3 px-6" :label="t('client.addToCart')" iconPos="right" severity="warning" icon="pi pi-cart-plus" outlined @click="AddCart" />
                        <!-- </router-link> -->
                    </div>
                </div>
                <!--  <div class="col-12 p-0">
                    <TabView>
                        <TabPanel header="Thông tin sản phẩm">
                            <div v-html="dataEdit.product.note"></div>
                        </TabPanel>
                       <TabPanel header="Tính năng">
                            <div v-html="dataEdit.product.feature"></div>
                        </TabPanel>
                        <TabPanel header="Thông số">
                            <div v-html="dataEdit.product.specifications"></div>
                        </TabPanel>
                        <TabPanel header="Tệp đính kèm">
                            <DataTable :value="dataEdit.product.product_attachments" tableStyle="min-width: 50rem">
                                <Column field="file_name" header="Tên file"></Column>
                                <Column header="Link">
                                    <template #body="data">
                                        <a :href="data.data.file_link" target="_blank">Xem thử</a>
                                    </template>
                                </Column>
                            </DataTable>
                        </TabPanel> 
                    </TabView>
                </div>-->
            </div>
        </div>

        <div class="lg:col-12 md:col-12 col-12 bg-white border-round-md shadow-1 p-12 mb-5" v-if="dataEdit.product?.note">
            <h3 class="text-xl font-semibold text-900 mb-12">{{ t('client.details') }}</h3>
            <div class="product-details-content text-600 line-height-3" :class="{ expanded: showMoreDetail }" v-html="dataEdit.product.note"></div>
            <div class="action-showmore" @click="showMoreDetail = !showMoreDetail">{{ dataEdit.product.note.length > 100 ? (showMoreDetail ? 'Ẩn bớt' : 'Xem thêm') : '' }}</div>
        </div>
    </div>
</template>

<style scoped>
.p-rating .p-rating-item.p-rating-item-active .p-rating-icon {
    color: yellow;
}
.action-showmore {
    cursor: pointer;
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
:deep(.p-inputnumber-input) {
    text-align: center;
}
</style>
<script setup>
import { ref, onBeforeMount } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from '@/Pinia/auth';
import API from '@/api/api-main';
import { useGlobal } from '@/services/useGlobal';
import format from '@/helpers/format.helper';
import { useCartStore } from '../../../../Pinia/cart';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const cartStore = useCartStore();
const authStore = useAuthStore();
const { toast, FunctionGlobal } = useGlobal();
const route = useRoute();
const dataEdit = ref({});
const user = localStorage.getItem('user');
const showMoreDetail = ref(false);
const payload = ref({
    itemId: 0,
    quantity: 0,
    price: 0
});

const dataClear = JSON.stringify(payload.value);

const decrementQuantity = () => {
    if (dataEdit.value.product.quantity > 0) {
        dataEdit.value.product.quantity--;
    }
};

onBeforeMount(() => {
    GetByIdItem(route.params.id);
});

const GetByIdItem = async (id) => {
    let url = user ? `Item/${id}` : `Item/${id}/bypass`;
    try {
        const res = await API.get(url);
        dataEdit.value.product = res.data; 
        dataEdit.value.product.description = await generateDescriptionAI(dataEdit.value.product.itemName);
        dataEdit.value.product.quantity = 1;
    } catch (error) {}
};

const AddCart = async () => {
    if (!authStore.isLoggedIn) {
        FunctionGlobal.$notify('W', 'Vui lòng đăng nhập để tiếp tục mua hàng', toast);
        return;
    }
    payload.value.quantity = dataEdit.value.product.quantity;
    payload.value.itemId = dataEdit.value.product.id;
    payload.value.price = dataEdit.value.product.price;

    // if (dataEdit.value.product.product_units.length)
    //   payload.value.unit_id = dataEdit.value.product.product_units[0].id;

    // if (!Validate()) return;
    try {
        const res = await cartStore.addToCart([payload.value]);
        await cartStore.getCart();

        // const res = await API.add("Cart/me/items", [payload.value]);
        if (res.data) {
            FunctionGlobal.$notify('S', 'Sản phẩm đã được thêm vào giỏ hàng', toast);
            ResetData();
        }
    } catch (error) {
        FunctionGlobal.$notify('S', error, toast);
    }
};
const Validate = () => {
    let status = true;

    if (parseInt(dataEdit.value.product.quantity) < 1) {
        status = false;
        FunctionGlobal.$notify('E', 'Vui lòng nhập số lượng dặt hàng', toast);
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

async function generateDescriptionAI(productName) {
    const res = await fetch('https://api.openai.com/v1/chat/completions', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${import.meta.env.VITE_APP_OPENAI_KEY}`
        },
        body: JSON.stringify({
            model: 'gpt-4.1-mini',
            messages: [
                {
                    role: 'user',
                    content: `Mở rộng mô tả chi tiết, bổ sung lợi ích cho người dùng.Tên sản phẩm: "${productName}",Thêm emoji, không quá 500 ký tự, chuẩn SEO, dễ hiểu, phù hợp cho trang web bán hàng`
                }
            ]
        })
    });

    const data = await res.json();
    return data.choices[0].message.content;
}
</script>
