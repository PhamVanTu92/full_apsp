<template>
    <div style="margin-bottom: 20rem">
        <OrderDetail :is-client="true"></OrderDetail>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import OrderDetail from "../../../../common/Order/index.vue";

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
