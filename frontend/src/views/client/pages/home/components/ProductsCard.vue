<template>
    <div v-if="0">
        <div class="w-full border-1 border-gray-200 p-3 overflow-hidden bg-white" v-if="!props.isDetail">
            <router-link :to="`/client/detail/${props.id}`">
                <div class="transition-all animation-duration-300 w-full h-full mb-2 relative form_img">
                    <img class="w-full h-full" style="object-fit: contain"
                        :src="props.image ? props.image : 'https://placehold.co/800x800'" alt="" />
                    <div class="absolute top-0 left-0" v-if="props.status">
                        <p class="py-1 px-2 text-sm border-round-sm" :class="colorStatus(props.status)">
                            {{ props.status }}
                        </p>
                    </div>
                    <div
                        class="absolute top-0 right-0 w-full h-13rem layer_hover flex justify-content-center align-items-center">
                        <div
                            class="h-3rem w-3rem border-circle bg-white hover:bg-orange-500 hover:text-white flex justify-content-center align-items-center">
                            <i class="pi pi-eye text-2xl"></i>
                        </div>
                    </div>
                </div>
                <!-- <div class="w-full h-1rem flex gap-1 mb-1" v-if="props.rating">
                    <img
                        style="height: 100%"
                        src="/src/assets/svg/icon_star.png"
                        alt=""
                    />
                    <img
                        style="height: 100%"
                        src="/src/assets/svg/icon_star.png"
                        alt=""
                    />
                    <img
                        style="height: 100%"
                        src="/src/assets/svg/icon_star.png"
                        alt=""
                    />
                    <img
                        style="height: 100%"
                        src="/src/assets/svg/icon_star.png"
                        alt=""
                    />
                    <img
                        style="height: 100%"
                        src="/src/assets/svg/icon_star.png"
                        alt=""
                    />
                    <p class="text-sm text-gray-500" style="text-align: end">(52,677)</p>
                </div> -->
                <p class="truncate-multiline text-base mb-2">{{ props.name }}</p>

                <div class="flex gap-1">
                    <div class="card_old_price text-decoration-line: line-through" v-if="props.oldPrice">
                        {{ format.FormatCurrency(props.oldPrice) }}
                    </div>
                    <div class="card_price font-semibold">
                        {{ format.FormatCurrency(props.price) }}
                    </div>
                </div>
            </router-link>

            <div class="mt-4">
                <Button class="w-full" @pointerdown="addToCart(props, $event)" :label="t('client.addToCart')" severity="primary"
                    outlined />
                <!-- <router-link :to="">
        </router-link> -->
                <router-link :to="{ name: 'client-create-order', query: { itemId: props.id } }">
                    <Button :label="t('client.buyNow')" class="w-full my-2" severity="primary" />
                </router-link>
            </div>
        </div>
        <div class="w-full border-1 border-gray-200 p-3 overflow-hidden bg-white" style="height: 42rem" v-else>
            <router-link :to="`/client/detail/${props.id}`">
                <div class="transition-all animation-duration-300 w-full h-20rem mb-2 relative form_img">
                    <img class="w-full h-full" style="object-fit: contain"
                        :src="props.image ? props.image : 'https://placehold.co/800x800'" alt="" />
                    <div class="absolute top-0 left-0" v-if="props.status">
                        <p class="py-2 px-4 text-base border-round-sm" :class="colorStatus(props.status)">
                            {{ props.status }}
                        </p>
                    </div>
                    <div
                        class="absolute top-0 right-0 w-full h-20rem layer_hover flex justify-content-center align-items-center">
                        <div
                            class="h-4rem w-4rem border-circle bg-white hover:bg-orange-500 hover:text-white flex justify-content-center align-items-center">
                            <i class="pi pi-eye text-3xl"></i>
                        </div>
                    </div>
                </div>
                <!-- <div class="w-full h-2rem flex gap-1">
            <img style="height: 80%;" src="/src/assets/svg/icon_star.png" alt="">
            <img style="height: 80%;" src="/src/assets/svg/icon_star.png" alt="">
            <img style="height: 80%;" src="/src/assets/svg/icon_star.png" alt="">
            <img style="height: 80%;" src="/src/assets/svg/icon_star.png" alt="">
            <img style="height: 80%;" src="/src/assets/svg/icon_star.png" alt="">
            <p class=" text-gray-500" style="text-align: end;"> (52,677)</p>
        </div> -->
                <p class="truncate-multiline text-lg">{{ props.name }}</p>

                <div class="flex gap-1 my-3">
                    <div class="card_old_price text-decoration-line: line-through text-lg" v-if="props.oldPrice">
                        {{ format.FormatCurrency(props.oldPrice) }}
                    </div>
                    <div class="card_price font-semibold text-lg">
                        {{ format.FormatCurrency(props.price) }}
                    </div>
                </div>
                <p style="font-size: 14px" class="truncate-multiline2 h-6rem">
                    {{ props.note }}
                </p>
            </router-link>

            <div class="flex gap-2">
                <Button v-if="0" class="bg-orange-100 border-0 text-black-alpha-90" icon="pi pi-heart"
                    style="height: 48px; min-width: 48px; font-size: 20px" />
                <Button
                    class="w-full bg-orange-500 border-0 text-black-alpha-90 text-white flex justify-content-center align-items-center gap-2"
                    :label="t('client.addToCart')" style="height: 48px; min-width: 48px" @pointerdown="addToCart(props, $event)">
                    <i class="pi pi-shopping-cart"></i> {{ t('client.addToCart') }}</Button>
                <Button class="bg-orange-100 border-0 text-black-alpha-90 text-xl" icon="pi pi-eye"
                    style="height: 48px; min-width: 48px" />
            </div>
        </div>

        <a href="item/12">
            <div class="card">
                <div>Blue T-Shirt</div>
                <button>Add to cart</button>
                <button>Love</button>
            </div>
        </a>
    </div>

    <div class="card card-hover hover:border-primary-400 animation-duration-300 flex flex-column p-3"
        :class="props.class">
        <router-link :to="user ? `/client/detail/${props.id}` : `/preview-page/detail/${props.id}`" class="flex-grow-1">
            <div class="w-full overflow-hidden bg-cover flex-grow-1">
                <div class="flex justify-content-center">
                    <Image :src="props.image ? props.image : 'https://placehold.co/800x800'" imageClass="bg-auto w-full"
                        height="200">
                        <template #indicatoricon> <i class="pi pi-search"></i> </template>
                    </Image>
                </div>
                <div class="my-2 text-lg">
                    {{ props.name }}
                </div>
                <div class="">
                    <div class="text-primary font-semibold text-lg">
                        {{ !props.currency || props.currency == 'VND' ? format.FormatCurrency(props.price, 0) :
                            format.FormatCurrency(props.price, 2) +
                            props.currency }}
                    </div>
                    <div class="card_old_price text-decoration-line: line-through text-lg" v-if="props.oldPrice">
                        {{ !props.currency || props.currency == 'VND' ? format.FormatCurrency(props.oldPrice, 0) :
                            format.FormatCurrency(props.oldPrice, 2)
                            + props.currency }} 
                    </div>
                </div>
                <p v-html="props.note" style="font-size: 14px" class="truncate-multiline2 my-3"></p>
            </div>
        </router-link>
        <div v-if="user" class="flex flex-column gap-2 z-2">
            <Button
                class="w-full bg-white border-orange-500 border-0 text-orange-500 flex justify-content-center align-items-center gap-2 hover:bg-orange-500 hover:text-white"
                @pointerdown="addToCart(props, $event)">
                <i class="fa-solid fa-cart-plus"></i> {{ t('client.addToCart') }}</Button>
            <router-link :to="{ name: 'client-create-order', query: { itemId: props.id } }">
                <Button
                    class="w-full bg-white border-green-500 border-0 text-green-500 flex justify-content-center align-items-center gap-2 hover:bg-green-500 hover:text-white"
                    icon="pi pi-eye" :label="t('client.buyNow')">
                    <i class="fa-solid fa-cart-shopping"></i> {{ t('client.buyNow') }}
                </Button>
            </router-link>
        </div>
    </div>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import format from '@/helpers/format.helper';
import { useCartStore } from '@/Pinia/cart';
import { useGlobal } from '@/services/useGlobal'; 
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const user = localStorage.getItem('user'); 
const { FunctionGlobal, toast } = useGlobal();
const payload = ref({
    itemId: 0,
    quantity: 1,
    price: 0,
    paymentMethodCode: ''
});
const cartStore = useCartStore();
const props = defineProps(['id', 'name', 'image', 'price', 'oldPrice', 'status', 'isDetail', 'note', 'rating', 'class', 'currency']);
const colorStatus = (status: any) => {
    switch (status) {
        case 'HOT':
            return 'bg-green-500 text-white';
        case 'HẾT HÀNG':
            return 'bg-gray-500	text-white';
        default:
            return 'bg-yellow-300 text-black';
    }
};

const addToCart = async (data: any, event: MouseEvent) => {
    event.stopPropagation();
    try {
        payload.value.price = data.price;
        payload.value.itemId = data.id;
        payload.value.paymentMethodCode = 'PayNow';

        const res = await cartStore.addToCart([payload.value]);
        await cartStore.getCart();
        if (res.data) {
            FunctionGlobal.$notify('S', 'Sản phẩm đã được thêm vào giỏ hàng', toast);
        }
    } catch (error) {
        console.error(error);
    }
};
</script>

<style scoped>
.truncate-multiline {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

.truncate-multiline2 {
    display: -webkit-box;
    -webkit-line-clamp: 4;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

.card_price {
    color: rgba(45, 165, 243, 1);
}

.card_old_price {
    font-size: 13px;
    color: rgba(158, 158, 158, 1);
}

.form_img:hover .layer_hover {
    opacity: 1;
}

.layer_hover {
    background: #00000034;
    opacity: 0;
}

a {
    color: #000;
}

.card-hover:hover {
    transform: translateY(-2px);
    box-shadow: rgba(50, 50, 93, 0.25) 0px 13px 27px -5px, rgba(0, 0, 0, 0.3) 0px 8px 16px -8px;
}
</style>
