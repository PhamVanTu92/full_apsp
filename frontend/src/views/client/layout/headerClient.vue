<script setup>
import { onBeforeMount, ref, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Notication from '@/components/Notication.vue';
import { useCartStore } from '../../../Pinia/cart';
import { useMeStore } from '../../../Pinia/me';
import { useConfirm } from 'primevue/useconfirm';
import { useI18n } from 'vue-i18n';
import { backPage } from '@/helpers/backPage.helper';
const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const cartStore = useCartStore();
const op = ref();
const menu = ref();
const username = ref('');
const meStore = useMeStore();
const user = localStorage.getItem('user');
const confirm = useConfirm();

const toggle = (event) => {
    op.value.toggle(event);
}; 
const cartQuantity = computed(() => cartStore.getCartQuantity);
const items = ref([
    {
        label: t('client.accountInfo'),
        icon: 'fa-solid fa-user',
        command: () => {
            router.push({ name: 'boardSetup' });
        }
    },
    {
        label: t('Logout.confirm'),
        icon: 'fa-solid fa-right-from-bracket',
        command: () => {
            localStorage.clear();
            router.push({ name: 'login' });
        }
    }
]);
onBeforeMount(async () => {
    try {
        const udata = await meStore.getMe();
        username.value = udata?.user?.bpInfo?.cardName;
        localStorage.setItem('username', username.value);
    } catch (e) {

    }
});

const confirmLogout = () => {
    confirm.require({
        message: t('Logout.message'),
        header: t('Logout.confirm'),
        rejectClass: 'p-button-secondary p-button-outlined',
        acceptClass: 'p-button-danger',
        rejectClass: 'p-button-secondary',
        rejectLabel: t('Logout.cancel'),
        acceptLabel: t('Logout.confirm'),
        accept: () => {
            localStorage.clear();
            meStore.$dispose();
            router.replace(`/login`);
        }
    });
};
 
</script>

<template>
    <div class="bg-green-800 pt-5 hidden lg:block mb-2">
        <div class="content-container mx-auto flex justify-content-between grid text-white">
            <div class="lg:col-6 col-12 p-0">
                {{ t('client.welcome_message') }}
            </div>
            <div class="lg:col-6 col-12 flex justify-content-between grid text-white p-0 ">
                <div
                    class="flex-grow-1 gap-3 flex align-items-center lg:justify-content-end justify-content-center lg:col-6 col-12">
                    <div class="text-right">{{ t('client.follow_us') }}:</div>
                    <a class="link" href="#">
                        <i class="fa-brands fa-facebook"></i>
                    </a>
                    <a class="link" href="#">
                        <i class="fa-brands fa-youtube"></i>
                    </a>
                    <a class="link" href="#">
                        <i class="fa-brands fa-tiktok"></i>
                    </a>
                    <a class="link" href="#">
                        <i class="fa-brands fa-linkedin"></i>
                    </a>
                    <a class="link" href="#">
                        <i class="fa-brands fa-x-twitter"></i>
                    </a> 
                </div>
            </div>
        </div>
    </div>
    <hr class="m-0 divier-color"/>
    <div class="sticky top-0 surface-50 z-5" style="z-index: 1000 !important">
        <div class="bg-green-800 ">
            <div class="content-container mx-auto flex justify-content-between grid lg:p-0 py-3">
                <div class="col-12 lg:col-6 p-0 text-center lg:text-left">
                    <router-link :to="user ? `/client` : `/preview-page`">
                        <img src="/src/assets/images/new-logo-ap.png" class="h-4rem" alt="" />
                    </router-link>
                </div>
                <div class="lg:flex gap-4  col-12 lg:col-6 p-0 justify-content-end">
                    <span class="text-white text-lg flex align-items-center gap-2 text-center">{{ t('client.hello') }},
                        <router-link :to="{ name: 'boardSetup' }" class="font-bold link">
                            <span v-if="username">{{ username }}</span>
                            <Skeleton v-else width="10rem" height="1.4rem"></Skeleton>
                        </router-link>
                        <div class="vertical-divier hidden lg:block"></div>
                    </span>
                    <div class="flex align-items-center gap-2 lg:mt-0 mt-2">
                        <router-link :to="{ name: 'cart' }">
                            <i v-if="cartQuantity > 0" v-badge.danger="cartQuantity >= 100 ? '99+' : cartQuantity"
                                class="fa fa-cart-shopping text-white" style="font-size: 1.8rem" />
                            <i v-else class="fa fa-cart-shopping text-white" style="font-size: 1.8rem"></i>
                        </router-link>
                        <Notication />
                        <div class="vertical-divier"></div>
                        <i class="fa-solid fa-right-from-bracket text-white cursor-pointer mx-1"
                            style="font-size: 1.8rem" v-tooltip.bottom="t('Logout.confirm')" @click="confirmLogout" />
                    </div>
                </div>

            </div>
        </div>
        <div class="p-2 border-bottom-1 border-200 shadow-1">
            <div class="content-container mx-auto flex justify-content-between grid mt-0">
                <div class="flex gap-2 lg:col-6 col-12 p-0">
                    <router-link :to="{ name: 'cart' }">
                        <Button :label="t('body.OrderList.createOder')" icon="pi pi-shopping-cart" class="p-2"
                            style="width: 13rem" />
                    </router-link>
                    <router-link :to="{ name: 'client-create-purchasereq' }">
                        <Button :label="t('body.PurchaseRequestList.createPickupRequest')" class="w-15rem"
                            icon="pi pi-calendar-plus" outlined />
                    </router-link>
                    <!-- <router-link to="#">
                        <Button :label="t('Footer.linkProduct.trackOrder')" outlined class="w-13rem"
                            icon="pi pi-search"/>
                    </router-link> -->
                </div>
                <div class="flex gap-2 lg:col-6 col-12 lg:justify-content-end lg:p-0 p-0 pt-2 ">
                    <GlobalSearch class="hidden lg:block"></GlobalSearch>
                    <Button icon="pi pi-arrow-left" severity="secondary" :label="t('body.home.back_button')"
                        class="w-8rem" v-if="route.path !== '/client'" @click="backPage(router, route)" />
                </div>
            </div>
        </div>
    </div>
    <Menu ref="menu" style="top: 132px" id="overlay_menu" :model="items" :popup="true" />
    <OverlayPanel ref="op">
        <div class="flex flex-column gap-2">
            <router-link :to="{ name: 'client-create-order' }">
                <Button :disabled="route.path == '/client/order'" text class="w-full" icon="pi pi-file"
                    :label="t('body.OrderList.createOder')" @click="toggle" />
            </router-link>
            <router-link :to="{ name: 'client-create-forcast' }">
                <Button :disabled="route.path == '/client/forcast'" class="w-full" text icon="pi pi-file"
                    :label="t('body.importPlan.create')" @click="toggle" />
            </router-link>
            <router-link :to="{ name: 'client-create-purchasereq' }">
                <Button :disabled="route.path == '/client/purchase-request'" class="w-full" text icon="pi pi-file"
                    :label="t('body.PurchaseRequestList.createPickupRequest')" @click="toggle" />
            </router-link>
        </div>
    </OverlayPanel>
    <ConfirmDialog :draggable="false" />
</template>

<style scoped>
.vertical-divier {
    width: 1px;
    margin: auto 0 auto 0;
    height: 24px;
    background-color: rgba(255, 255, 255, 0.836);
}

.divier-color {
    border-color: rgb(38 90 30 / 74%) !important;
}
</style>