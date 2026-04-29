<template>
    <VueDraggableNext v-model="props.items" :list="props.items" item-key="id">
        <template v-for="(item, index) in props.items" :key="index">
            <li class="list-none">
                <slot name="body" :data="item" :index="index"></slot>
            </li>
            <ul v-if="item.children?.length">
                <DraggableTree class="" :items="item.children">
                    <template #body="slotProps">
                        <slot
                            name="body"
                            :data="slotProps.data"
                            :index="slotProps.index"
                        ></slot>
                    </template>
                </DraggableTree>
            </ul>
        </template>
    </VueDraggableNext>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { VueDraggableNext } from "vue-draggable-next";

interface Item {
    id?: number;
    title?: string;
    children?: Array<Item>;
    // other properties...
}

interface Props {
    items: Array<Item>;
}

const props = withDefaults(defineProps<Props>(), {
    items: () => [],
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
