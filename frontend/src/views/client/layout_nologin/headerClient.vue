<script setup>
import { onBeforeMount, ref, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
const route = useRoute();
const router = useRouter();
import API from '@/api/api-main';
import Notication from '@/components/Notication.vue';
import { useCartStore } from '../../../Pinia/cart';
import DropdownLanguage from '@/components/DropdownLanguage.vue';
import { useMeStore } from '../../../Pinia/me';
import { useConfirm } from 'primevue/useconfirm';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const cartStore = useCartStore();
const op = ref();
const toggle = (event) => {
    op.value.toggle(event);
};

const cartQuantity = computed(() => cartStore.getCartQuantity);

const menu = ref();
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
const username = ref('');
const meStore = useMeStore();
const user = localStorage.getItem('user');
onBeforeMount(async () => {
    try {
        const udata = await meStore.getMe();
        if (udata) {
            username.value = udata?.user.bpInfo ? udata?.user?.bpInfo?.cardName : udata.user.fullName;
            localStorage.setItem('username', username.value);
        }
    } catch (e) {

    }
});

const confirm = useConfirm();

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
const selectedCountry = ref({ name: 'Tiếng Việt', code: 'VN' });
const countries = ref([
    { name: 'Tiếng Việt', code: 'VN' }
]);
</script>

<template>

    <div class="bg-green-800 py-2">
        <div class="content-container mx-auto text-200 flex justify-content-between gap-3 text-base">
            <div class="my-auto">{{ t('client.welcome_message') }}</div>
            <div class="flex gap-3">
                <div class="flex-grow-1 gap-3 flex align-items-center">
                    <div>{{ t('client.follow_us') }}:</div>
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
                <div class="vertical-divier"></div>
                <div>
                    <DropdownLanguage />
                </div>
            </div>
        </div>
    </div>
    <hr class="m-0 divier-color" />
    <div class="sticky top-0 surface-50 z-5" style="z-index: 1000 !important">
        <div class="bg-green-800 py-2">
            <div class="content-container mx-auto flex justify-content-between">
                <div v-if="user">
                    <router-link :to="`/client`">
                        <img src="/src/assets/images/new-logo-ap.png" class="h-4rem" alt="" /></router-link>
                </div>
                <div v-else>
                    <router-link :to="`/preview-page`">
                        <img src="/src/assets/images/new-logo-ap.png" class="h-4rem" alt="" />
                    </router-link>
                </div>
                <div class="flex gap-4 align-items-center" v-if="user">
                    <span class="text-white text-lg flex align-items-center gap-2">{{ t('client.hello') }},
                        <router-link :to="{ name: 'boardSetup' }" class="font-bold link">
                            <span v-if="username">{{ username }}</span>
                            <Skeleton v-else width="10rem" height="1.4rem"></Skeleton>
                        </router-link>
                    </span>
                    <div class="vertical-divier"></div>
                    <router-link :to="{ name: 'cart' }">
                        <i v-if="cartQuantity > 0" v-badge.danger="cartQuantity >= 100 ? '99+' : cartQuantity"
                            class="fa fa-cart-shopping text-white" style="font-size: 1.8rem" />
                        <i v-else class="fa fa-cart-shopping text-white" style="font-size: 1.8rem"></i>
                    </router-link>
                    <Notication top="130px" />
                    <div class="vertical-divier"></div>
                    <i class="fa-solid fa-right-from-bracket text-white cursor-pointer mx-1" style="font-size: 1.8rem"
                        v-tooltip.bottom="t('Logout.confirm')" @click="confirmLogout" />
                </div>
                <div v-else class="flex gap-4 align-items-center">
                    <router-link :to="{ name: 'login' }">
                        <i class="fa-solid fa-right-from-bracket text-white cursor-pointer mx-1"
                            style="font-size: 1.8rem" v-tooltip.bottom="t('Login.buttons.login')" /> 
                    </router-link>
                </div>
            </div>
        </div>
        <div class="p-2 border-bottom-1 border-200 shadow-1" v-if="user && user?.bpInfo">
            <div class="content-container mx-auto flex justify-content-between">
                <div class="flex gap-2">
                    <router-link :to="{ name: 'cart' }">
                        <Button :label="t('body.OrderList.createOder')" icon="pi pi-shopping-cart" class="p-2"
                            style="width: 13rem"/>
                    </router-link>
                    <router-link :to="{ name: 'client-create-purchasereq' }">
                        <Button :label="t('body.PurchaseRequestList.createPickupRequest')" class="w-15rem"
                            icon="pi pi-calendar-plus" outlined/>
                    </router-link>
                    <router-link to="#">
                        <Button :label="t('Footer.linkProduct.trackOrder')" outlined class="w-13rem"
                            icon="pi pi-search"/>
                    </router-link>
                </div>
                <div class="flex gap-2">
                    <GlobalSearch></GlobalSearch>
                    <Button icon="pi pi-arrow-left" severity="secondary" :label="t('body.home.back_button')"
                        class="w-8rem" v-if="route.path !== '/client'" @click="router.back()"/>
                </div>
            </div>
        </div>
    </div> 
    <Menu ref="menu" style="top: 132px" id="overlay_menu" :model="items" :popup="true" />
    <OverlayPanel ref="op">
        <div class="flex flex-column gap-2">
            <router-link :to="{ name: 'client-create-order' }">
                <Button :disabled="route.path == '/client/order'" text class="w-full" icon="pi pi-file"
                    :label="t('body.OrderList.createOder')" @click="toggle"/>
            </router-link>
            <router-link :to="{ name: 'client-create-forcast' }">
                <Button :disabled="route.path == '/client/forcast'" class="w-full" text icon="pi pi-file"
                    :label="t('body.importPlan.create')" @click="toggle"/>
            </router-link>
            <router-link :to="{ name: 'client-create-purchasereq' }">
                <Button :disabled="route.path == '/client/purchase-request'" class="w-full" text icon="pi pi-file"
                    :label="t('body.PurchaseRequestList.createPickupRequest')" @click="toggle"/>
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
