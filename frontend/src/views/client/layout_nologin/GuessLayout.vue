<script setup>
import HeaderClient from './headerClient.vue';
import FooterClient from './footerClient.vue';
import { RouterView } from 'vue-router';
</script>
<template>
    <div class="w-full relative">
        <header-client></header-client>
        <div class="content-container mx-auto">
            <div class="mt-3">
                <router-view></router-view>
            </div>
        </div>
        <footer-client></footer-client>
    </div>
    <Toast position="bottom-right" />
</template>
<style scoped>
.layout-res {
    padding: 0 13rem;
}

@media only screen and (max-width: 640px) {
    .layout-res {
        padding: 0 2rem;
    }

    .text-res {
        font-size: 12px !important;
    }

    .col-9,
    .col-3 {
        width: 100%;
    }

    .col-3 {
        order: 2;
    }
}

@media only screen and (min-width: 641px) and (max-width: 1023px) {
    .layout-res {
        padding: 0 5rem;
    }
}

@media only screen and (min-width: 1024px) and (max-width: 1279px) {
    .layout-res {
        padding: 0 5rem;
    }

    .col-9 {
        width: 65%;
        /* Điều chỉnh kích thước cột */
    }

    .col-3 {
        width: 35%;
    }
}

@media only screen and (min-width: 1280px) and (max-width: 1535px) {
    .layout-res {
        padding: 0 3rem;
    }
}
</style>
