<template>
    <div class="w-full">
        <div class="flex mx-3 justify-content-between align-items-center">
            <div class="text-2xl font-bold">{{t('body.OrderApproval.order')}}</div>
            <router-link
                :to="{ name: 'hisPur' }"
                class="flex gap-2 text-right text-blue-500 align-items-center font-bold"
            >
                <div class="hover:underline cursor-pointer">{{ t('client.view_all') }}</div>
                <i class="pi pi-arrow-right"></i>
            </router-link>
        </div>
        <div class="flex flex-row justify-content-between w-full pr-0">
            <div class="col-3" v-for="(item, i) in status" :key="i">
                <router-link :to="item.routerTo">
                    <div
                        @mouseenter="addAnimation(item)"
                        @mouseleave="removeAnimation(item)"
                        class="border-round-sm p-3"
                        :class="item.class"
                    >
                        <div class="flex">
                            <div
                                class="p-3 bg-white border-round-sm text-3xl text-center mr-3 h-4rem w-4rem"
                            >
                                <i
                                    :class="{
                                        [item.iclass]: true,
                                        [item.animation]: item.isHover,
                                    }"
                                ></i>
                            </div>
                            <div class="flex flex-column justify-content-around">
                                <div class="text-2xl font-bold">
                                    {{ item.count }}
                                </div>
                                <div class="font-bold text-600">{{ item.name }}</div>
                            </div>
                        </div>
                    </div>
                </router-link>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import API from "@/api/api-main";
import { useRouter, RouteLocationAsRelativeGeneric } from "vue-router";

import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const router = useRouter();
const getRouter = (status: string): RouteLocationAsRelativeGeneric => {
    // router.push({

    // })
    return {
        name: "hisPur",
        query: {
            status: status,
        },
    } as RouteLocationAsRelativeGeneric;
};

function addAnimation(item: any) {
    item.isHover = true;
}

function removeAnimation(item: any) {
    item.isHover = false;
}

const status = ref<Array<any>>([
    {
        propName: "complete",
        name: t('client.completed'),
        routerTo: getRouter("DHT"),
        class: "bg-green-200 hover:bg-green-300",
        count: "00",
        iclass: "fa-solid fa-check-to-slot text-green-300",
        isHover: false,
        animation: "fa-bounce",
    },
    {
        propName: "onDelivery",
        name: t('client.delivering'),
        routerTo: getRouter("DGH"),
        class: "bg-blue-200 hover:bg-blue-300",
        count: "00",
        iclass: "fas fa-shipping-fast text-blue-300",
        animation: "fa-shake",
    },
    {
        propName: "confirmed",
        name: t('client.confirmed'),
        routerTo: getRouter("DXN"),
        class: "bg-teal-200 hover:bg-teal-300",
        count: "00",
        iclass: "fas fa-shopping-cart text-teal-300",
        animation: "fa-beat",
    },
    {
        propName: "processing",
        name: t('client.processing'),
        routerTo: getRouter("DXL"),
        class: "bg-yellow-200 hover:bg-yellow-300",
        count: 0,
        iclass: "fa-solid fa-spinner text-yellow-300",
        animation: "fa-spin fa-spin-pulse",
    },
]);

const formatNumber = (num: number) => {
    if (num < 10) return "0" + num;
    return new Intl.NumberFormat("en-EN", {
        style: "decimal",
        maximumFractionDigits: 1,
    }).format(num);
};

const initialComponent = () => {
    // code here
    API.get("Report/order-state")
        .then((res) => {
            status.value = status.value.map((item) => {
                return {
                    ...item,
                    count: formatNumber(res.data.order[item.propName] || 0),
                };
            });
        })
        .catch((error) => {})
        .finally(() => {});
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
