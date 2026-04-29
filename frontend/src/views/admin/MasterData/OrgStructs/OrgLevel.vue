<script setup>
import { ref } from "vue";

const selectedProduct = ref();
const products = ref([
  {
    orgLevel: "00asdfasdf1",
    desc:
      '<Column field="category" header="Category" sortable style="width: 20%"></Column>',
    status: "Category 1",
    actions: [
      {
        action: "fa-regular fa-pen-to-square",
      },
      {
        action: "fa-regular fa-trash-can",
      },
    ],
  },
  {
    orgLevel: "002",
    desc:
      '<Column field="category" header="Category" sortable style="width: 20%"></Column> 2',
    status: "Category 2",
    actions: [
      {
        action: "fa-regular fa-pen-to-square",
      },
      {
        action: "fa-regular fa-trash-can",
      },
    ],
  },
  {
    orgLevel: "001",
    desc:
      'Produ<Column field="category" header="Category" sortable style="width: 20%"></Column>ct 1',
    status: "Category 1",
    actions: [
      {
        action: "fa-regular fa-pen-to-square",
      },
      {
        action: "fa-regular fa-trash-can",
      },
    ],
  },
]);

const headerStyle = {
  backgroundColor: "#DEDEDE",
  color: "black",
  fontWeight: "bold",
};
</script>

<template>
  <div class="flex justify-content-between align-items-center mb-4">
    <div class="uppercase font-bold">Danh mục cấp tổ chức</div>
    <Button label="Thêm mới" icon="fa-solid fa-plus" class="py-2 text-sm"/>
  </div>

  <div class="grid mt-3">
    <div class="col-12 h-screen bg-white">
      <DataTable
        v-model:selection="selectedProduct"
        :value="products"
        tableStyle="min-width: 50rem;"
        header="surface-200"
        stripedRows
        selectionMode="single"
      >
        <Column
          selectionMode="multiple"
          :headerStyle="headerStyle"
          :style="{ width: '5%' }"
        ></Column>
        <Column
          field="orgLevel"
          header="Cấp tổ chức"
          :headerStyle="headerStyle"
          :style="{ width: '25%' }"
        ></Column>
        <Column
          field="desc"
          header="Mô tả"
          :headerStyle="headerStyle"
          :style="{ width: '40%' }"
        ></Column>
        <Column
          field="status"
          header="Trạng thái"
          :headerStyle="headerStyle"
          :style="{ width: '20%' }"
        ></Column>
        <Column
          field="actions"
          header="Hành động"
          :headerStyle="headerStyle"
          :style="{ width: '20%' }"
        >
          <template #body="slotProps">
            <div class="ml-3">
              <i :class="slotProps.data.actions[0].action"></i>
              <i class="mx-2"></i>
              <i :class="slotProps.data.actions[1].action"></i>
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
<style scoped></style>
