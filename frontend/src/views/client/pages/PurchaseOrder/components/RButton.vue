<template>
    <button
        class="btn text-color-secondary border-dashed border-400 border-none bg-white hover:bg-green-50 hover:text-green-500 hover:border-green-500 border-bottom-1 px-2 py-3 cursor-pointer w-full flex justify-content-between">
        <span class="flex align-items-center">
            <i :class="props.icon"></i>
            <span class="ml-2">{{ props.label }}</span>
        </span>
        <span class="flex align-items-center">
            <span class="mr-2">Thay đổi</span>
            <i class="pi pi-angle-right"></i>
        </span>
    </button>
</template>

<script setup>
import {defineProps} from 'vue';
const props = defineProps({
    label: null
    , 
    icon: null
});
</script>

<style scoped>
.btn:focus {
    outline: none;
}
</style>
