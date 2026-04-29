<template> 
    <div class="grid mb-0">
        <div v-for="card, i in props.data" class="col-12 md:col-6 lg:col-3" :key="i">
            <div class="p-4 border-1 border-200 bg-white flex align-items-center border-round-md">
                <div class="border-circle icon flex justify-content-center align-items-center" :class="card.color">
                    <i :class="card.icon" class="text-2xl"></i>
                </div>
                <div class="flex-grow-1 text-right">
                    <div class="text-xl font-bold mb-2">{{ card.value }}</div>
                    <p class="text-500">{{ card.label }}</p>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
const props = defineProps({
    data: {
        type: Array,
        default: [{}, {}, {}, {}]
    }
});

</script>

<style scoped>
.icon {
    width: 50px;
    min-width: 50px;
    height: 50px;
    min-height: 50px;
}
</style>
<style>
.purple {
    border: 1px solid #6658dd;
    background: #d9d5f6;
    color: #6658dd;
}

.green {
    border: 1px solid #40c5a8;
    background: #c6eee6;
    color: #40c5a8;
}

.yellow {
    border: 1px solid #f8bd4b;
    background: #fdedd2;
    color: #f8bd4b;
}

.blue {
    border: 1px solid #43c9ee;
    background: #d0eff8;
    color: #43c9ee;
}
</style>
