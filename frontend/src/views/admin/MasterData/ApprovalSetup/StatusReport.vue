<template>
  <div class="flex justify-content-between align-items-center mb-4">
    <strong class="text-2xl">Báo cáo trạng thái phê duyệt</strong>
    <Button @click="openFilterReport()" icon="pi pi-filter" label="Bộ lọc"/>
  </div>
  <div class="grid mt-3 card p-2">
    <div class="col-12 flex justify-content-end gap-3">
      <InputGroup>
        <InputText
          v-model="keySearchOrders"
          @keyup.enter="fetchPurchaseReq()"
          placeholder="Tìm kiếm..."
        />
        <Button @click="fetchPurchaseReq()" icon="pi pi-search" severity="warning" />
      </InputGroup>
    </div>
    <div class="col-12">
      <DataTable
        stripedRows=""
        class="table-main"
        showGridlines
        :value="PurchaseRequest"
        tableStyle="min-width: 50rem;"
        header="surface-200"
        paginator
        :rows="dataTable.size"
        :page="dataTable.page"
        :totalRecords="dataTable.total_size"
        @page="onPageChange($event)"
        :rowsPerPageOptions="[10, 20, 30]"
        lazy
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} yêu cầu"
      >
        <template #empty>
          <div class="p-2 text-center">Không tìm thấy dữ liệu tương ứng</div>
        </template>
        <Column header="#"></Column>
        <Column header="Mã chứng từ"></Column>
        <Column header="Người tạo"></Column>
        <Column header="Ngày tạo"></Column>
        <Column header="Mẫu phê duyệt"></Column>
        <Column header="Trạng thái"></Column>
        <Column header="Ghi chú"></Column>
      </DataTable>
    </div>
  </div>

  <Dialog
    v-model:visible="filterReportModal"
    modal
    header="Lựa chọn tiêu chí"
    :style="{ width: '700px' }"
  >
    <div class="flex flex-column gap-3">
      <strong>Trạng thái</strong>
      <div class="grid">
        <div class="col-6 flex flex-column gap-4">
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Chờ xử lý</label>
          </div>
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Đã phê duyệt</label>
          </div>
        </div>
        <div class="col-6 flex flex-column gap-4">
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Từ chối</label>
          </div>
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Đã hủy</label>
          </div>
        </div>
      </div>
      <strong>Loại chứng từ</strong>
      <div class="grid">
        <div class="col-6 flex flex-column gap-4">
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Chờ xử lý</label>
          </div>
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Đã phê duyệt</label>
          </div>
        </div>
        <div class="col-6 flex flex-column gap-4">
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Từ chối</label>
          </div>
          <div class="flex gap-2">
            <Checkbox></Checkbox>
            <label for="">Đã hủy</label>
          </div>
        </div>
      </div>
      <strong>Người khởi tạo</strong>
      <div class="grid">
        <div class="col-6 flex align-items-center gap-2">
          <label for="">Từ</label>
          <Dropdown class="w-full"></Dropdown>
        </div>
        <div class="col-6 flex align-items-center gap-2">
          <label for="">Đến</label>
          <Dropdown class="w-full"></Dropdown>
        </div>
      </div>
      <strong>Thời gian</strong>
      <div class="grid">
        <div class="col-6 flex align-items-center gap-2">
          <label for="">Từ</label>
          <Dropdown class="w-full"></Dropdown>
        </div>
        <div class="col-6 flex align-items-center gap-2">
          <label for="">Đến</label>
          <Dropdown class="w-full"></Dropdown>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex gap-2">
        <Button
          @click="filterReportModal = false"
          outlined
          icon="pi pi-times"
          label="Hủy"
        />
        <Button icon="pi pi-check" label="Xác nhận"/>
      </div>
    </template>
  </Dialog>
  <loading v-if="isLoading"></loading>
</template>
<style scoped></style>
<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
const router = useRouter();
const filterReportModal = ref(true);
const dataTable = ref({
  page: 0,
  size: 10,
  total_size: 0,
});
const openFilterReport = () => {
  filterReportModal.value = true;
};
</script>
