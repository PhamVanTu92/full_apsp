<template>
    <div class="relative h-full">
        <div class="card p-2" :class="checkHome">
            <PanelMenu class="menu_custom" :model="props.items">
                <template #item="{ item }">
                    <router-link v-if="item.route" :to="item.route" active-class="text-green-800 font-semibold"
                        exact-active-class="text-green-800 font-semibold"
                        class="flex align-items-center cursor-pointer px-3 py-2 no-underline"
                        :class="{ 'text-green-800 font-semibold': isActiveRoute(item.route) }" >
                        <span :class="item.icon" />
                        <span class="ml-2">{{ item.label }}</span>
                    </router-link>
                    <a v-else class="flex align-items-center cursor-pointer hover:text-primary px-3 py-2 no-underline"
                        :class="{ 'text-green-800 font-semibold': isActiveRoute(item.route) }" :href="item.route"
                        :target="item.target">
                        <span :class="item.icon" />
                        <span class="ml-2">{{ item.label }}</span>
                        <span v-if="item.items" class="pi pi-angle-down ml-auto" />
                    </a>
                </template>
            </PanelMenu>
        </div>
    </div>
</template>

<script setup lang="js">
import { computed } from "vue";
import { useRoute } from "vue-router";
const route = useRoute();
const props = defineProps(['items']);
const checkHome = computed(() => {
    return route.name == 'client-home' ? 'h-full' : 'sticky top-cus';
});

const isActiveRoute = (itemRoute) => {
    if (!itemRoute) return false;
    if (typeof itemRoute === 'string')
        return route.path === itemRoute || route.name === itemRoute;
    if (typeof itemRoute === 'object') {
        if (itemRoute.name)
            return route.name === itemRoute.name;
        if (itemRoute.path)
            return route.path === itemRoute.path;
    }
    return false;
};
</script>

<style>
.top-cus {
    top: 135px;
}

.menu_custom a {
    color: rgb(51, 65, 85);
    text-decoration: none;
}

.menu_custom a.no-underline {
    text-decoration: none !important;
}
.menu_custom a.text-green-800 {
    color: rgb(22, 101, 52) !important;
    font-weight: 600 !important;
}

.menu_custom .p-panelmenu-panel {
    border: none;
    margin-bottom: 0;
} 
.menu_custom .router-link-active {
    color: rgb(22, 101, 52) !important;
    font-weight: 600 !important;
}

.menu_custom .router-link-exact-active {
    color: rgb(22, 101, 52) !important;
    font-weight: 600 !important;
}
</style>
