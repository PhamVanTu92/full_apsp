<template>
    <div>
        <div class="font-bold text-xl">{{ t('client.brand') }}</div>
        <ul class="p-0 list-none">
            <li
                v-for="(item, i) in brandStore.data.slice(0, limit)"
                :key="i"
                class="mb-2 flex"
            >
                <Checkbox
                    :inputId="`itemtype-${item.id}`"
                    v-model="selectionStore.brand"
                    :value="item.id"
                    class="mr-2"
                    @change="selectionStore.skip = 0"
                ></Checkbox>
                <label class="cursor-pointer" :for="`itemtype-${item.id}`">{{
                    item.name
                }}</label>
            </li>
            <li v-if="brandStore.data.length > 10">
                <Button
                    @click="onShowMore"
                    class="w-full"
                    :label="limit < brandStore.data.length ? t('common.btn_show_more') : t('common.btn_show_less')"
                    text
                />
            </li>
        </ul>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted } from "vue";
import { useBrandStore } from "../../../../../Pinia/brand";
import { useSelectionFilterStore } from "../../../../../Pinia/productFilter";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();


const selectionStore = useSelectionFilterStore();
const brandStore = useBrandStore();
const limit = ref(10);

onMounted(() => {
    if (!brandStore.data.length) brandStore.fetchAll();
});

const btnLabel = ref("Hiển thị thêm");
const onShowMore = () => {
    if (limit.value < brandStore.data.length) {
        limit.value += 10;
    } else {
        limit.value = 10;
    }
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});

onUnmounted(() => {
    selectionStore.$dispose();
});
</script>

<style scoped></style>
