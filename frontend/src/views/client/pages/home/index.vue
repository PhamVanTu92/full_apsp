<template>
    <!-- Desktop Layout -->
    <div class="mt-3 lg:block md:block hidden">
        <div class="grid border-round-sm m-0">
            <!-- Menu Section -->
            <div class="col-12 md:col-6 lg:col-3 pl-0 pt-0">
                <MenuUser :items="menuItems" />
            </div>
            <!-- Main Content Section -->
            <div class="col-12 md:col-6 lg:col-9 px-0">
                <div class="grid w-full">
                    <!-- User Info Cards -->
                    <div class="flex justify-content-between w-full">
                        <div class="col-12 p-0">
                            <div class="flex justify-content-between">
                                <div class="col-4 pt-0">
                                    <div class="card p-0 flex flex-column justify-content-between h-full mb-0">
                                        <h5 class="m-0 p-3">{{ t('client.accountInfo') }}</h5>
                                        <div class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3">
                                            <div>
                                                <span class="mr-2 font-semibold">{{ t('client.fullName') }}:</span>
                                                <span>{{ user.fullName }}</span>
                                            </div>
                                            <div>
                                                <span class="mr-2 font-semibold">{{ t('client.email') }}:</span>
                                                <span>{{ user.email }}</span>
                                            </div>
                                            <div>
                                                <span class="mr-2 font-semibold">{{ t('client.phoneNumber') }}:</span>
                                                <span>{{ user?.phone }}</span>
                                            </div>
                                            <div>
                                                <span class="mr-2 font-semibold">{{ t('client.birthday') }}:</span>
                                                <span>{{ formattedBirthday }}</span>
                                            </div>
                                            <div>
                                                <span class="mr-2 font-semibold">{{ t('client.address') }}:</span>
                                                <span>{{ userAddress }}</span>
                                            </div>
                                        </div>
                                        <div class="p-3">
                                            <router-link to="/client/setup/user">
                                                <Button :label="t('client.editInfo')" class="p-3 w-full" outlined />
                                            </router-link>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-4 pt-0">
                                    <div class="card p-0 flex flex-column justify-content-between h-full mb-0">
                                        <h5 class="m-0 p-3">{{ t('client.shippingAddress') }}</h5>
                                        <div class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3">
                                            <template v-if="shippingUser">
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.fullName') }}:</span>
                                                    <span>{{ shippingUser.person }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.citizenId') }}:</span>
                                                    <span>{{ shippingUser.cccd }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.address') }}:</span>
                                                    <span>{{ getAddress(shippingUser) }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.email') }}:</span>
                                                    <span>{{ shippingUser.email }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.phoneNumber') }}:</span>
                                                    <span>{{ shippingUser.phone }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.licensePlate') }}:</span>
                                                    <span>{{ shippingUser.vehiclePlate }}</span>
                                                </div>
                                            </template>
                                            <div v-else class="text-center text-gray-600 h-full">
                                                {{ t('client.no_delivery_address') }}
                                            </div>
                                        </div>
                                        <div class="p-3">
                                            <router-link to="/client/setup/user">
                                                <Button :label="t('client.editInfo')" class="p-3 w-full" outlined />
                                            </router-link>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-4 pt-0">
                                    <div class="card p-0 flex flex-column justify-content-between h-full mb-0">
                                        <h5 class="m-0 p-3">{{ t('client.billingAddress') }}</h5>
                                        <div class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3">
                                            <template v-if="billingUser">
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.fullName') }}:</span>
                                                    <span>{{ billingUser.person }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.address') }}:</span>
                                                    <span>{{ getAddress(billingUser) }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.email') }}:</span>
                                                    <span>{{ billingUser.email }}</span>
                                                </div>
                                                <div>
                                                    <span class="mr-2 font-semibold">{{ t('client.phoneNumber') }}:</span>
                                                    <span>{{ billingUser.phone }}</span>
                                                </div>
                                            </template>
                                            <div v-else class="text-center text-gray-600 h-full">
                                                {{ t('client.no_delivery_address') }}
                                            </div>
                                        </div>
                                        <div class="p-3">
                                            <router-link to="/client/setup/user">
                                                <Button :label="t('client.editInfo')" class="p-3 w-full" outlined />
                                            </router-link>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <OrderStats />
                </div>
            </div>
        </div>
    </div>

    <!-- Charts Section -->
    <div class="grid">
        <template v-if="isShowCongNo">
            <div class="col-12 lg:col-6">
                <div class="card h-full">
                    <div class="font-bold text-xl text-center mb-3">
                        {{ t('client.debt_by_status') }}
                    </div>
                    <PieChart />
                    <div class="text-blue-400 cursor-pointer hover:underline text-center text-lg pt-5" @click="visible = true">
                        {{ t('body.OrderList.viewDetails') }}
                    </div>
                </div>
            </div>
            <div class="col-12 lg:col-6">
                <div class="card">
                    <div class="font-bold text-xl text-center mb-3">
                        {{ t('client.current_debt_vs_limit') }}
                    </div>
                    <StackChart />
                </div>
            </div>
        </template>

        <!-- Order Details Dialog -->
        <div class="col-12">
            <Dialog v-model:visible="visible" modal :header="t('client.order_detail_table')" :style="{ width: '80%' }">
                <div class="card">
                    <OrdersDataTable />
                </div>
                <div class="flex justify-content-end gap-2">
                    <Button type="button" label="Hủy" severity="secondary" @click="visible = false" />
                </div>
            </Dialog>
        </div>
    </div>

    <!-- Products Section -->
    <div class="my-6">
        <!-- Flashsale Section -->
        <div class="lg:flex justify-content-between align-items-center">
            <div class="flex lg:align-items-center">
                <span class="text-3xl font-semibold mr-3">Flashsale</span>
                <span class="mx-2 hidden lg:block">{{ t('client.timeLeft') }}</span>
                <span class="sale-off bg-yellow-300 p-2 font-semibold">{{ countdownlabel }}</span>
            </div>
            <router-link :to="{ name: 'categories' }" class="text-blue-500 flex gap-2 align-items-center font-bold">
                <span class="cursor-pointer hover:underline">{{ t('client.view_all') }}</span>
                <i class="pi pi-arrow-right"></i>
            </router-link>
        </div>

        <div class="mt-5">
            <div class="grid">
                <div class="col-12 md:col-6 lg:col-4 xl:col-3" v-for="item in products" :key="item.id">
                    <ProductsCard :id="item.id" :currency="item.currency" :name="item.itemName" :image="item.itM1[0]?.filePath" :price="item.price" :oldPrice="item.old_price" :status="item.status" class="h-full" />
                </div>
            </div>
        </div>

        <!-- Product Catalog -->
        <div class="my-6">
            <div class="flex justify-content-center">
                <span class="text-5xl font-semibold hover:text-blue-700">
                    {{ t('client.productCatalog') }}
                </span>
            </div>

            <div class="grid justify-content-center">
                <div v-for="(item, index) in cate" :key="index" class="col-2 p-3 flex flex-column justify-content-center border-round-xs border-1" style="border-color: var(--surface-border)">
                    <img :src="item.thumbnail_id ? item.thumbnail_url : '/image/No_Image_Available.jpg'" class="mx-auto" alt="" style="width: 12rem; height: 12rem" />
                    <p class="text-center w-full p-3 pb-0 cursor-pointer hover:underline hidden-product-categories">
                        {{ item.name }}
                    </p>
                </div>
            </div>
        </div>
        <!-- Featured Products -->
        <div class="grid">
            <div class="lg:col-3 col-12 py-0">
                <div class="bg-yellow-300 h-full">
                    <div class="flex flex-column text-center">
                        <span class="text-sx mt-5 font-semibold" style="color: #be4646">SUPERIOR SYNTHETIC MOTOR OIL</span>
                        <h1 class="size-32px">32% DISCOUNT</h1>
                        <span class="size-16px my-3">For all SaiGon Petro products</span>
                        <span class="text-sx my-3">
                            Offers ends in:
                            <span class="bg-white p-1">ENDS OF SUMMER</span>
                        </span>
                        <button class="w-10rem py-3 my-3 border-none mx-auto size-16px text-white cursor-pointer" style="background-color: #fa8232; border-radius: 4px">
                            {{ t('client.buy_now') }}
                            <i class="fa-solid fa-arrow-right"></i>
                        </button>
                    </div>
                    <div class="text-center mx-auto" style="width: 90%">
                        <img class="w-full my-4" src="@/assets/images/image-8.png" alt="" />
                    </div>
                </div>
            </div>
            <div class="lg:col-9 col-12 p-0">
                <div class="flex flex-wrap h-full">
                    <div class="flex flex-column md:flex-column lg:flex-row justify-content-between align-items-center w-full mb-5">
                        <div>
                            <span class="size-24px font-semibold">{{ t('client.featured_product') }}</span>
                        </div>
                        <div class="flex lg:flex-row md:flex-row flex-column">
                            <span class="ml-4 lg:my-0 md:my-2 my-2 inline-block gray-color hover:text-gray-800 cursor-pointer" v-for="item in featuredList" :key="item.name">
                                {{ item.name }}
                            </span>
                            <router-link :to="{ name: 'categories' }">
                                <span class="ml-4 lg:mt-0 md:mt-2 mt-2 orange-color cursor-pointer hover:underline">
                                    {{ t('client.view_all') }}
                                    <i class="fa-solid fa-arrow-right"></i>
                                </span>
                            </router-link>
                        </div>
                    </div>

                    <template v-for="item in products.slice(0, 8)" :key="item.id">
                        <div class="lg:w-3 md:w-12 w-12 pr-2 pt-2">
                            <ProductsCard rating="true" :currency="item.currency" :id="item.id" :name="item.itemName" :image="item.itM1[0]?.filePath" :price="item.price" :oldPrice="item.old_price" :status="item.status" />
                        </div>
                    </template>
                </div>
            </div>
        </div>

        <!-- New Arrivals -->
        <div class="grid mt-2">
            <div class="lg:col-6 md:col-12 col-12">
                <div class="flex" style="background: #f2f4f5; border: 1px solid #191c1f70">
                    <div class="flex flex-column w-6 p-5">
                        <span class="px-3 py-2 bg-blue-300 w-6rem text-center" style="border-radius: 4px">{{ t('client.new_arrival') }}</span>
                        <h1>SP KEPMAX <br />50/CC 20W-50</h1>
                        <span>{{ t('client.engine_oil_4t') }}</span>
                        <span class="bg-orange-400 w-8rem text-center py-3 text-white flex align-items-center justify-content-center mt-3 hover:bg-orange-500 cursor-pointer" style="border-radius: 4px">
                            {{ t('client.buy_now') }}
                            <i class="ml-2 fa-solid fa-cart-shopping"></i>
                        </span>
                    </div>
                    <div class="w-6 flex justify-content-center align-items-center">
                        <img src="/demo/data/images/image-11.png" alt="" />
                    </div>
                </div>
            </div>
            <div class="lg:col-6 md:col-12 col-12">
                <div class="flex" style="background-color: #191c1f">
                    <div class="flex flex-column w-6 p-5 text-white">
                        <span class="px-3 py-2 bg-blue-300 w-6rem text-center text-gray-800 bg-yellow-300" style="border-radius: 4px">Mới về</span>
                        <h1 class="text-white">SP KEPMAX <br />50/CC 20W-50</h1>
                        <span>{{ t('client.engine_oil_4t') }}</span>
                        <span class="bg-orange-400 w-8rem text-center py-3 text-white flex align-items-center justify-content-center mt-3 hover:bg-orange-500 cursor-pointer" style="border-radius: 4px">
                            {{ t('client.buy_now') }}
                            <i class="ml-2 fa-solid fa-cart-shopping"></i>
                        </span>
                    </div>
                    <div class="w-6 flex justify-content-center align-items-center">
                        <img src="/demo/data/images/image-11.png" alt="" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onBeforeMount, onBeforeUnmount, onMounted } from 'vue';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';
import API from '@/api/api-main';
import { useMeStore } from '@/Pinia/me';

// Components
import PieChart from './components/Chart/PieChart.vue';
import StackChart from './components/Chart/StackChart.vue';
import OrdersDataTable from './components/Chart/OrdersDataTable.vue';
import OrderStats from './components/OrderStats.vue';
import MenuUser from '../user_menu/components/menuUser.vue';
import ProductsCard from './components/ProductsCard.vue';

// Composables
const { t } = useI18n();
const meStore = useMeStore();

// State
const user = ref({});
const products = ref([]);
const cate = ref([]);
const visible = ref(false);
const isShowCongNo = ref(false);
const countdownlabel = ref('');
let interval = null;

// Constants
const API_URL = 'Item';
const ADDRESS_FIELDS = ['address', 'locationName', 'areaName'];
const featuredList = ref([{ name: 'SaiGonPetro' }, { name: 'Ap Oil' }, { name: 'Sino' }, { name: 'Singtec Oil' }, { name: 'Polairs' }, { name: 'Export & Toll Bending' }]);
const menuItems = computed(() => [
    {
        label: t('body.sampleRequest.customerGroup.homepage_label'),
        icon: 'fa-solid fa-home',
        route: '/client'
    },
    {
        label: t('body.home.overview'),
        icon: 'fa-solid fa-bars-progress',
        route: '/client/setup/boardSetup'
    },
    {
        label: t('client.orderHistory'),
        icon: 'fa-solid fa-clock-rotate-left',
        items: [
            {
                label: t('Navbar.menu.orders'),
                icon: 'fa-solid fa-file-lines',
                route: '/client/setup/hisPur'
            },
            {
                label: t('body.home.orderNET'),
                icon: 'fa-solid fa-file-lines',
                route: '/client/setup/hisPurNET'
            },
            {
                label: t('Navbar.menu.request_exchange_promotional'),
                icon: 'fa-solid fa-list-alt',
                route: { name: 'promotiondetail-list' }
            }
        ]
    },
    {
        label: t('client.warehouse'),
        icon: 'fa-solid fa-warehouse',
        items: [
            {
                label: t('Navbar.menu.pickupRequest'),
                icon: 'fa-solid fa-truck-ramp-box',
                route: '/client/setup/purchase-request-list'
            },
            {
                label: t('client.shipping_list'),
                icon: 'fa-solid fa-clipboard-list',
                route: '/client/setup/sent-warehouse'
            },
            {
                label: t('Navbar.menu.warehouseFeeRecord'),
                icon: 'fa-solid fa-file-invoice',
                route: '/client/setup/inventory-charge'
            }
        ]
    },
    {
        label: t('client.qualityCommitment'),
        icon: 'fa-solid fa-chart-column',
        route: '/client/setup/production-commitment'
    },
    {
        label: t('client.importPlan'),
        icon: 'fa-regular fa-calendar-days',
        route: '/client/setup/purchase-plan'
    },
    {
        label: t('body.systemSetting.document'),
        icon: 'fa-solid fa-folder-open',
        route: { name: 'user-document' }
    },
    {
        label: t('client.account_settings'),
        icon: 'fa-solid fa-gear',
        route: '/client/setup/user'
    },
    {
        label: t('Feedback.feedback'),
        icon: 'fa-solid fa-comment-dots',
        route: '/client/setup/feedback'
    },
    {
        label: t('client.report'),
        icon: 'fa-solid fa-file-invoice',
        route: '/client/report'
    }
]);
const shippingUser = computed(() => user.value?.bpInfo?.crD1?.find((el) => el.default === 'Y' && el.type === 'S'));
const billingUser = computed(() => user.value?.bpInfo?.crD1?.find((el) => el.default === 'Y' && el.type === 'B'));
const formattedBirthday = computed(() => (user.value?.bpInfo?.dateOfBirth ? format(user.value.bpInfo.dateOfBirth, 'dd/MM/yyyy') : ''));
const userAddress = computed(() => [user.value.bpInfo?.address, user.value.bpInfo?.locationName, user.value.bpInfo?.areaName].filter(Boolean).join(', '));

// Methods
const getAddress = (userData) => {
    if (!userData) return '';
    return ADDRESS_FIELDS.map((key) => userData[key])
        .filter(Boolean)
        .join(', ');
};

const formatNumber = (n) => (n < 10 ? `0${n}` : n);

const doCountdown = () => {
    let times = 7 * 24 * 60 * 60;

    interval = setInterval(() => {
        if (times <= 0) {
            countdownlabel.value = 'Đã kết thúc';
            clearInterval(interval);
            return;
        }

        times--;
        const days = Math.floor(times / (24 * 60 * 60));
        const hours = Math.floor((times % (24 * 60 * 60)) / (60 * 60));
        const minutes = Math.floor((times % (60 * 60)) / 60);
        const seconds = Math.floor(times % 60);

        countdownlabel.value = `${formatNumber(days)} ngày : ${formatNumber(hours)} giờ : ${formatNumber(minutes)} phút : ${formatNumber(seconds)} giây`;
    }, 1000);
};

const fetchProducts = async () => {
    try {
        const res = await API.get(`${API_URL}?Page=1&PageSize=12`);
        products.value = res.data?.items || [];
    } catch (error) {
        console.error('Error fetching products:', error);
    }
};

const initializeUser = async () => {
    const meData = await meStore.getMe();
    user.value = meData?.user || {};

    const balanceLimit = user.value?.bpInfo?.crD3?.[0]?.balanceLimit;
    isShowCongNo.value = balanceLimit > 0 || Boolean(balanceLimit);
};

// Lifecycle Hooks
onBeforeMount(async () => {
    await fetchProducts();
});

onMounted(async () => {
    await initializeUser();
    doCountdown();
});

onBeforeUnmount(() => {
    if (interval) clearInterval(interval);
});
</script>

<style scoped>
@import './components/style.css';
</style>
