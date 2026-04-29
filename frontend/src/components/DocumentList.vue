<template>
  <div class="pb-5">
    <DataTable :value="Data" class="card p-0 overflow-hidden" stripedRows>
      <Column header="STT"></Column>
      <Column header="Tên tài liệu"></Column>
      <Column header="Ghi chú "></Column>
      <Column header="Tệp đính kèm"></Column>
    </DataTable>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue';
import API from '../api/api-main';


const Data = ref([])
const props = defineProps({
  url_endpoint: String
})
onMounted(() => {
  fetchData()
})
const fetchData = async () => {
  try {
    await API.get(`${props.url_endpoint}`)
  } catch (error) {
    console.error(error);
  }
}
</script>
<style></style>
