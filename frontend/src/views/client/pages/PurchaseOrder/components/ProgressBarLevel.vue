<template>
    <div class="p-3 w-full">
        <div id="process-bar" class="flex justify-content-between relative">
            <div v-for="(el, i) in levels" :key="i">
                {{ el.content }}
            </div>
        </div>
    </div>
</template>

<script setup>

const levels = [
    {
        opposite: "0",
        content: "0",
        value: 0
    },
    {
        opposite: "1",
        content: "10",
        value: 10
    },
    {
        opposite: "2",
        content: "30",
        value: 30
    },
    {
        opposite: "3",
        content: "50",
        value: 50
    },
    {
        opposite: "4",
        content: "100",
        value: 100
    },
]

</script>

<style scoped>

</style>
