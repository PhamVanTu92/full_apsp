<template>
    <div class="mx-auto flex justify-content-center align-items-center mt-5" style="width: 1325px; height: 300px">
        <div class="flex justify-content-center flex-column">
            <CheckSuccess />
            <!-- <CheckEror /> -->
            <div class="flex flex-column justify-content-center align-items-center">
                <span class="lg:text-3xl md:text-3xl text-2xl text-bold my-3">
                    {{ lableTitleSuccess }}
                    
                </span>
                <span class="text-center text-gray-500">Bạn sẽ được chuyển hướng về trang chủ sau <strong>{{ cd
                        }}</strong> giây!</span>
            </div>

            <div class="grid mt-3">
                <div class="lg:col-6 md:col-6 col-12">
                    <router-link :to="routerPageHistory(route.query.type)">
                        <button
                            class="w-full h-3rem border-2 border-round-xs border-solid border-orange-400 bg-white text-orange-400 font-bold p-2 cursor-pointer">{{
                                lableButtonHistory }}</button></router-link>
                </div>
                <div class="lg:col-6 md:col-6 col-12">
                    <router-link :to="routerPageDetail(route.query.type)">
                        <button
                            class="w-full h-3rem border-none border-round-xs cursor-pointer p-2 bg-orange-400 font-bold text-white">
                            {{ lableButtonDetail }}</button>
                    </router-link>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed, onBeforeMount, onMounted, ref } from 'vue';
import CheckSuccess from './CheckSuccess.vue';
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router';
const route = useRoute()
const router = useRouter()
const cd = ref(10)
const interval = ref()
onBeforeMount(() => {
    clearInterval(interval.value)
})
onBeforeRouteLeave((to, from) => {
    if (interval.value) {
        clearInterval(interval.value)
        interval.value = null
    }
})
onMounted(() => {
    // Khởi tạo interval mới
    if (!interval.value) {
        interval.value = setInterval(() => {
            if (cd.value > 0) {
                cd.value--
            } else {
                clearInterval(interval.value)
                interval.value = null
                router.push('/client')
            }
        }, 1000)
    }
})

const routerPageHistory = (route) => {
    if (route == 'forcast') {
        return `/client/setup/purchase-plan`
    }
    else {
        return route == 'order' ? `/client/setup/hisPur` : `/client/setup/purchase-request-list`
    }
}

const lableButtonDetail = computed(() => {
    if (route.query.type == 'forcast') {
        return `Xem chi tiết kế hoạch nhập hàng`
    }
    else {
        return route.query.type == 'order' ? `Xem chi tiết đơn hàng` : `Xem chi tiết yêu cầu`
    }
})

const lableButtonHistory = computed(() => {
    if (route.query.type == 'forcast') {
        return `Danh sách kế hoạch nhập hàng`
    }
    else {
        return route.query.type == 'order' ? `Lịch sử đơn hàng` : `Danh sách yêu cầu`
    }
})
const lableTitleSuccess = computed(() => {
    if (route.query.type == 'forcast') {
        return `Tạo kế hoạch nhập hàng thành công!`
    }
    else {
        return route.query.type == 'order' ? `Đơn hàng đã được đặt thành công!` : `Yêu cầu lấy hàng gửi đã tạo thành công!`
    }
    
})
const routerPageDetail = () => {
    if (route.query.type == 'forcast') {
        return `/client/setup/purchase-plan/${route.query.id}`
    }
    else {
        return route.query.type == 'order' ? `/client/order/${route.query.id}` : `/client/setup/purchase-request-client/${route.query.id}`
    }
}
</script>
