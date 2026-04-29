<template>
  <div class="card">
    <div class="flex justify-content-between">
      <div class="py-2 text-green-700 text-xl font-semibold">Sản lượng cam kết</div>
      <Button
        :disabled="editMode"
        @click="editMode = true"
        icon="pi pi-pencil"
        label="Chỉnh sửa"
        text
      />
    </div>
    <hr class="m-0" />
    <DataTable :value="[]">
      <Column header="#"></Column>
      <Column header="Mã sản lượng"></Column>
      <Column header="Tên sản lượng"></Column>
      <Column header="Năm"></Column>
      <template #empty>
        <div class="p-5 text-center">Chưa có dữ liệu để hiển thị</div>
      </template>
    </DataTable>
  </div>
</template>

<script setup>
import { ref } from "vue";
const editMode = ref(false);

const props = defineProps({
  setup: {
    API: {
      type: Object,
      required: true,
    },
    modelStates: {
      type: Object,
      required: true,
    },
  },
});
</script>

<style scoped></style>
