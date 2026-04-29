<template>
    <div>
        <div>
            <h4 class="font-bold">Thông báo</h4>
        </div>
        <DataTable>
            <Column></Column>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
