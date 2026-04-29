<template>
    <div class="layout-wrapper" :class="containerClass">
        <app-topbar></app-topbar>
        <div class="layout-sidebar h-full">
            <app-sidebar></app-sidebar>
        </div>
        <div class="layout-main-container">
            <div class="layout-main">
                <router-view></router-view>
            </div>
            <app-footer></app-footer>
        </div> 
        <div class="layout-mask"></div>
    </div>
    <Toast position="bottom-right" /> 
    <IdleTimeoutDialog v-model:visible="showWarning" :remaining-time="remainingTime" @extend-session="extendSession" @logout="performLogout" />
</template>

<script setup>
import { computed, watch, ref } from 'vue';
import AppTopbar from './AppTopbar.vue';
import AppFooter from './AppFooter.vue';
import AppSidebar from './AppSidebar.vue';
import { useLayout } from '@/layout/composables/layout';
import IdleTimeoutDialog from '@/components/IdleTimeoutDialog.vue';
import { useIdleTimeout } from '@/composables/useIdleTimeout';
import { useAuthStore } from '@/Pinia/auth';

const authStore = useAuthStore();

const { layoutConfig, layoutState, isSidebarActive } = useLayout();
const { showWarning, remainingTime, extendSession, clearAllTimers } = useIdleTimeout();
const performLogout = () => {
    clearAllTimers();
    authStore.logout();
    window.location.href = '/login';
};

const outsideClickListener = ref(null);
watch(isSidebarActive, (newVal) => {
    if (newVal) {
        bindOutsideClickListener();
    } else {
        unbindOutsideClickListener();
    }
});

const containerClass = computed(() => {
    return {
        'layout-theme-light': layoutConfig.darkTheme.value === 'light',
        'layout-theme-dark': layoutConfig.darkTheme.value === 'dark',
        'layout-overlay': layoutConfig.menuMode.value === 'overlay',
        'layout-static': layoutConfig.menuMode.value === 'static',
        'layout-static-inactive': layoutState.staticMenuDesktopInactive.value && layoutConfig.menuMode.value === 'static',
        'layout-overlay-active': layoutState.overlayMenuActive.value,
        'layout-mobile-active': layoutState.staticMenuMobileActive.value,
        'p-ripple-disabled': layoutConfig.ripple.value === false
    };
});
const bindOutsideClickListener = () => {
    if (!outsideClickListener.value) {
        outsideClickListener.value = (event) => {
            if (isOutsideClicked(event)) {
                layoutState.overlayMenuActive.value = false;
                layoutState.staticMenuMobileActive.value = false;
                layoutState.menuHoverActive.value = false;
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
    const sidebarEl = document.querySelector('.layout-sidebar');
    const topbarEl = document.querySelector('.layout-menu-button');

    return !(sidebarEl.isSameNode(event.target) || sidebarEl.contains(event.target) || topbarEl.isSameNode(event.target) || topbarEl.contains(event.target));
};
</script>

<style lang="scss">
::-webkit-scrollbar-track {
    -webkit-box-shadow: inset 0 0 4px rgba(107, 106, 106, 0.3);
    background-color: #f5f5f5;
}

::-webkit-scrollbar-thumb {
    background-color: rgba(107, 106, 106, 0.3);
}

::-webkit-scrollbar {
    width: 7px !important;
    height: 7px !important;
}
</style>
