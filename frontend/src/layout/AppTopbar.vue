<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useLayout } from '@/layout/composables/layout';
import { useAuthStore } from '@/Pinia/auth';
import { useRouter } from 'vue-router';
import Notication from '@/components/Notication.vue';
// import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const { onMenuToggle } = useLayout();

const outsideClickListener = ref(null);
const topbarMenuActive = ref(false);
// const router = useRouter();

onMounted(() => {
    bindOutsideClickListener();
});

onBeforeUnmount(() => {
    unbindOutsideClickListener();
});

const onTopBarMenuButton = () => {
    topbarMenuActive.value = !topbarMenuActive.value;
};
// const onSettingsClick = () => {
//     topbarMenuActive.value = false;
//     router.push('/documentation');
// };
const topbarMenuClasses = computed(() => {
    return {
        'layout-topbar-menu-mobile-active': topbarMenuActive.value
    };
});

const bindOutsideClickListener = () => {
    if (!outsideClickListener.value) {
        outsideClickListener.value = (event) => {
            if (isOutsideClicked(event)) {
                topbarMenuActive.value = false;
            }
        };
        document.addEventListener('click', outsideClickListener.value);
    }
};
const unbindOutsideClickListener = () => {
    if (outsideClickListener.value) {
        document.removeEventListener('click', outsideClickListener);
        outsideClickListener.value = null;
    }
};
const isOutsideClicked = (event) => {
    if (!topbarMenuActive.value) return;

    const sidebarEl = document.querySelector('.layout-topbar-menu');
    const topbarEl = document.querySelector('.layout-topbar-menu-button');

    return !(sidebarEl.isSameNode(event.target) || sidebarEl.contains(event.target) || topbarEl.isSameNode(event.target) || topbarEl.contains(event.target));
};

const authStore = useAuthStore();
const router = useRouter();
const menu = ref();
const items = ref([
    {
        label: t('Logout.infoDetail'),
        icon: 'pi pi-user',
        command: () => {
            router.push({
                name: 'profile'
            });
        }
    },
    {
        label: t('Logout.confirm'),
        icon: 'fa-solid fa-arrow-right-from-bracket',
        command: () => {
            authStore.logout();
            router.push({ name: 'login' });
            location.reload();
        }
    }
]);

const onLogout = () => {
    const languagePos = localStorage.getItem('language-pos');
    authStore.logout();
    localStorage.clear();
    if (languagePos) {
        localStorage.setItem('language-pos', languagePos);
    }
    location.reload();
    location.replace('/login');
};

const toggle = (event) => {
    menu.value.toggle(event);
};

const user = JSON.parse(localStorage.getItem('user'))?.appUser;

const visibleConfirmLogout = ref(false);
const onClickLogout = () => {
    visibleConfirmLogout.value = true;
};
 
</script>

<template>
    <div class="layout-topbar bg-green-700">
        <button class="p-link layout-menu-button layout-topbar-button" @click="onMenuToggle()">
            <i class="pi pi-bars text-white"></i>
        </button>
        <router-link to="/" class="layout-topbar-logo justify-content-center">
            <img src="/src/assets/images/new-logo-ap.png" class="h-3rem" alt="logo" />
        </router-link> 
        <button class="p-link layout-topbar-menu-button layout-topbar-button " @click="onTopBarMenuButton()">
            <i class="pi pi-ellipsis-v icon-topbar"></i>
        </button>

        <div class="layout-topbar-menu" :class="topbarMenuClasses">
            <!-- Notication -->
            <div class="flex gap-2 align-items-center">
                <!-- <div class="border-1 p-2 border-300 border-round mr-3" >
                    <i class="pi pi-user mr-2 text-white text-lg"></i>
                </div> -->
                <span class="text-lg mr-2 lg:text-white text-black">{{ t('client.hello') }},&nbsp;
                    <router-link :to="{ name: 'profile' }"
                        class="font-semibold hover:underline cursor-pointer lg:text-white text-black">{{ user?.fullName || 'Unknown'
                        }}</router-link>
                </span>
                <div class="border-right-1 border-300 h-2rem"></div>
                <div class="mx-3">
                    <Notication />
                </div>
                <div class="flex align-items-end">
                    <Button size="large" class="p-3" icon="fa-solid fa-right-from-bracket lg:text-white text-black text-2xl" text
                        rounded v-tooltip.bottom="t('Logout.confirm')" @click="onClickLogout"/>
                </div>
            </div>

            <div v-if="0" class="bg-white ml-2" style="width: 0.5px"></div>
            <button class="p-link flex align-items-center ml-3" @click="toggle"
                v-if="authStore.isLoggedIn && 0">
                <Button class="w-3rem" type="button" icon="fa-regular fa-user" aria-haspopup="true"
                    aria-controls="overlay_tmenu" :pt="{
                        root: {
                            style: 'border-radius: 50%; width: 40px; height: 40px;'
                        },
                        label: {
                            style: 'background-color: red'
                        }
                    }" />
                <TieredMenu ref="menu" id="overlay_tmenu" :model="items" popup />
                <p class="text-lg text-white ml-3">
                    {{ user?.userName || 'Unknown' }}
                    <i class="fa-solid fa-chevron-down ml-1 text-base"></i>
                </p>
            </button>
        </div>
    </div>

    <Dialog v-model:visible="visibleConfirmLogout" :header="t('Logout.confirm')" modal class="w-28rem"
        :closable="false">
        <span>{{ t('Logout.message') }}</span>
        <template #footer>
            <Button :label="t('Logout.cancel')" icon="pi pi-times" severity="secondary"
                @click="visibleConfirmLogout = false"/>
            <Button :label="t('Logout.confirm')" severity="danger" icon="fa-solid fa-right-from-bracket"
                @click="onLogout"/>
        </template>
    </Dialog>
</template>

<style lang="scss" scoped>
.icon-topbar {
    font-size: 1.5rem;
    color: white;
}

.icon-topbar:hover {
    color: black;
}
</style>
