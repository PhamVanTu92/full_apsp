<template>
    <div>
        <div
            v-if="poStore.getCommittedToShow.length"
            class="card p-0 border-green-700 collapse-border"
        >
            <div v-for="(item, i) in poStore.getCommittedToShow" :key="i" class="p-3">
                <div
                    class="flex justify-content-between align-items-center border-bottom-1 border-dashed border-300 border-x-none border-top-none"
                >
                    <div class="font-bold text-lg text-500">
                        {{ item.timeLabel }} {{ item.timeValue }}
                    </div>
                    <Button
                        @click="committedReachRef?.open()"
                        icon="pi pi-eye"
                        text
                    />
                </div>
                <template v-for="(subItem, j) in item.data" :key="j">
                    <div class="my-2">
                        <span class="font-bold mr-2">{{ subItem.industryName }}: </span>
                        <span
                            >{{ fnum(subItem.currentValue + subItem.incurredValue) }} /
                            {{ fnum(subItem.quotaValue) }}</span
                        >
                    </div>
                    <PercentBar
                        :arguments="[
                            {
                                label: 'Sản lượng đã tích lũy',
                                value: subItem.currentValue,
                                unit: 'Lít',
                            },
                            {
                                label: 'Sản lượng cộng thêm',
                                value: subItem.incurredValue,
                                unit: 'Lít',
                            },
                        ]"
                        :value="subItem.quotaValue"
                        unit="Lít"
                    ></PercentBar>
                </template>
            </div>
        </div>
        <CommittedReach ref="committedReachRef"></CommittedReach>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { usePoStore } from "../store/purchaseStore.store";
import { fnum } from "../script";
import CommittedReach from "../dialogs/CommittedReach.vue";

const committedReachRef = ref<InstanceType<typeof CommittedReach> | null>(null);
const poStore = usePoStore();

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.collapse-border > :not(:last-child) {
    border-bottom: 1px solid var(--surface-300);
}
</style>
