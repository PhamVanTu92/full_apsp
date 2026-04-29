<template>
    <div class="bar border-round overflow-hidden">
        <template v-for="(item, i) in props.arguments" :key="i">
            <div
                class="value"
                :style="{
                    background: getColor(i),
                    width: `${
                        getPercentage(item.value) <= 100 ? getPercentage(item.value) : 100
                    }%`,
                }"
                v-if="getPercentage(item.value)"
            >
                <!-- :class="{ full: getPercentage(item.value) == 100 }" -->
                <div class="tooltip">
                    <div style="text-wrap: nowrap">
                        {{ item.label }}:
                        <span class="font-bold">{{ formatNumber(item.value) }}</span>
                        {{ item.unit || "" }}
                    </div>
                    <div>({{ formatNumber(getPercentage(item.value)) }}%)</div>
                </div>
                <div style="overflow-x: hidden">{{ getPercentage(item.value) }}%</div>
            </div>
        </template>
        <div class="value flex-grow-1">
            <div class="tooltip">
                Còn thiếu:
                <span class="font-bold">{{ formatNumber(remainPercent) }}</span>
                {{ props.unit || "" }}
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";

interface Arguments {
    label: string | undefined;
    value: number | 0;
    unit: string | "lít";
}

interface Props {
    value: number | undefined;
    arguments: Array<Arguments> | undefined;
    unit: string | undefined;
}

const props = defineProps<Props>();
const getPercentage = (value: number): number => {
    let val = 0;
    if (props.value && props.value > 0) {
        val = Math.round(((value * 10) / props.value) * 100) / 10;
    }
    return val;
};

// scheme of green
const colors = ["#0d733b", "#17b45e", "#179751", "#00a65a", "#00cd00", "#00ff00"];

const getColor = (index: number): string => {
    return colors[index] || colors[0];
};

const formatNumber = (num: number | any) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat("vi-VI").format(num);
};

const remainPercent = computed((): number => {
    let value = props.value || 0;
    let result = value - remainValue.value;
    return result;
});

const remainValue = computed((): number => {
    let result = 0;
    result = props.arguments?.reduce((acc, curr) => acc + (curr.value || 0), 0) || 0;
    return result;
});
</script>

<style scoped>
.bar {
    background-color: #e2e8f0;
    height: 18px;
    border-radius: 6px;
    display: flex;
    color: #fff;
}

.bar > .value:first-child {
    border-radius: 6px 0 0 6px;
}

.last-bar {
    border-radius: 0 6px 6px 0;
}

.value {
    height: 100%;
    text-align: center;
    position: relative;
    cursor: default;
}

.value:hover > .tooltip {
    display: block;
}
.remain:hover > .tooltip {
    display: block;
}

.value:hover {
    opacity: 0.9;
}
.remain:hover {
    opacity: 0.9;
}

.tooltip {
    position: absolute;
    bottom: 0px;
    left: 50%;
    transform: translate(-50%, -24px);
    background-color: rgb(0, 0, 0);
    color: #fff;
    padding: 5px;
    border-radius: 6px;
    z-index: 1;
    display: none;
    min-width: 10rem;
}
</style>

<style>
.full {
    border-radius: 6px !important;
}
</style>
