<template>
    <div>
        <div class="font-bold text-xl">{{t('client.product_type')}}</div>
        <ul class="p-0 list-none">
            <li
                v-for="(item, i) in itemTypeStore.data
                    .slice(0, limit)
                    .filter((el) => el.id !== 16)"
                :key="i"
                class="mb-2 flex"
            >
                <Checkbox
                    :inputId="`itemtype-${item.id}`"
                    v-model="selectionStore.itemType"
                    :value="item.id"
                    class="mr-2"
                    @change="selectionStore.skip = 0"
                ></Checkbox>
                <label class="cursor-pointer" :for="`itemtype-${item.id}`">{{
                    item.name
                }}</label>
            </li>
            <li v-if="itemTypeStore.data.length > 10">
                <Button
                    @click="onShowMore"
                    class="w-full"
                    :label="
                        limit < itemTypeStore.data.length ? t('client.show_more') : t('client.show_less')
                    "
                    text
                />
            </li>
        </ul>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import API from "@/api/api-main";
import { useItemTypeStore } from "../../../../../Pinia/itemType";
import { useSelectionFilterStore } from "../../../../../Pinia/productFilter";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const selectionStore = useSelectionFilterStore();
const itemTypeStore = useItemTypeStore();
const limit = ref(10);

onMounted(() => {
    if (!itemTypeStore.data.length) itemTypeStore.fetchAll();
});

const onShowMore = () => {
    if (limit.value < itemTypeStore.data.length) {
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
