<template>
    <div>
        <div class="grid">
            <div class="col-3 relative">
                <div class="sticky" style="top: 135px">
                    <CategoryComponent></CategoryComponent>
                </div>
            </div>
            <div class="col-9">
                <ProductList></ProductList>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import CategoryComponent from "./components/CategoryComponent.vue";
import ProductList from "./components/ProductList.vue";

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
