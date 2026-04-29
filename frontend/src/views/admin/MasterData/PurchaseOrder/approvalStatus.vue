<script setup>
import { ref } from "vue";
const products = ref([
  {
    id: "001",
    docName: "Yêu cầu đặt hàng 001",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
  {
    id: "002",
    docName: "Yêu cầu đặt hàng 002",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
  {
    id: "003",
    docName: "Yêu cầu đặt hàng 003",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
  {
    id: "003",
    docName: "Yêu cầu đặt hàng 003",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
  {
    id: "003",
    docName: "Yêu cầu đặt hàng 003",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
  {
    id: "003",
    docName: "Yêu cầu đặt hàng 003",
    docCode: "YCDHxxx",
    createdBy: "Đại lý Trường an",
    startDate: "21312",
    tmpForm: "Phê duyệt 3 cấp",
    status: "Chờ phê duyệt",
    note: "Đơn đặt hàng 04/2024 - Đợt 2",
  },
]);

const data = ref([
  {
    process: "Phê duyệt cấp 1",
    createdBy: "hoanggggf",
    result: "Phê duyệt",
    approvalDate: "16/04-2024",
  },
  {
    process: "Phê duyệt cấp 1",
    createdBy: "hoanggggf",
    result: "Phê duyệt",
    approvalDate: "16/04-2024",
  },
  {
    process: "Phê duyệt cấp 1",
    createdBy: "hoanggggf",
    result: "Phê duyệt",
    approvalDate: "16/04-2024",
  },
]);
</script>
<template>
  <div class="">
    <div class="flex justify-content-between align-items-center mb-4">
      <div class="text-2xl font-semibold uppercase">Báo cáo trạng thái phê duyệt</div>
      <Button label="Cập nhật" icon="fa-solid fa-rotate-right" class="bg-green-700"/>
    </div>

    <DataTable :value="products" tableStyle="min-width: 50rem;" header="surface-200" showGridlines>
      <Column field="id" header="#" :style="{ width: '3%' }"></Column>
      <Column field="docName" header="Tên chứng từ" :style="{ width: '15%' }"></Column>
      <Column field="docCode" header="Mã chứng từ" :style="{ width: '10%' }"></Column>
      <Column field="createdBy" header="Người tạo" :style="{ width: '10%' }"></Column>
      <Column field="startDate" header="Ngày tạo" :style="{ width: '8%' }"></Column>
      <Column field="tmpForm" header="Mẫu phê duyệt" :style="{ width: '13%' }"></Column>
      <Column field="status" header="Trạng thái" :style="{ width: '13%' }"></Column>
      <Column field="note" header="Ghi chú" :style="{ width: '20%' }"></Column>
    </DataTable>
    <Paginator class="" :rows="10" :totalRecords="120"></Paginator>
  </div>

  <div class="mt-3">
    <DataTable :value="data" tableStyle="min-width: 50rem;" header="surface-200" showGridlines>
      <Column field="process" header="Quy trình"></Column>
      <Column field="createdBy" header="Người duyệt"></Column>
      <Column field="result" header="Kết quả"></Column>
      <Column field="approvalDate" header="Người duyệt"></Column>
    </DataTable>
  </div>
</template>

<style scoped></style>
