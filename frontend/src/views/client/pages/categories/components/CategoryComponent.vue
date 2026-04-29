<template>
    <div>
        <!-- <h4 class="font-bold uppercase mt-1 mb-3">Danh mục sản phẩm</h4> -->
        <div>
            <Brand></Brand>
        </div>
        <div>
            <ItemType></ItemType>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import ItemType from "./ItemType.vue";
import Brand from "./Brand.vue";

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
