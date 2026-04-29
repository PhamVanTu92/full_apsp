<template>
  <h3 class="text-2xl font-bold mb-3">Nhóm đơn vị tính</h3>
  
  <DataTable :value="[{}]" tableStyle="min-width: 50rem">
      <Column field="code" header="Code"></Column>
      <Column field="name" header="Name"></Column>
      <Column field="category" header="Category"></Column>
      <Column field="quantity" header="Quantity"></Column>
  </DataTable>
  
</template>

<script setup>
import { ref, onBeforeMount } from 'vue';
import API from '@/api/api-main';

const initialComponent = () => {
    
}

onBeforeMount(() => {
    initialComponent();
})

</script>

<style>

</style>
