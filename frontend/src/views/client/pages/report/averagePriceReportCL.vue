<template>
  <div>
    <div class="flex justify-content-between align-content-center">
      <h4 class="font-bold m-0">Báo cáo giá trung bình sản phẩm</h4>
      <div class="flex gap-2">
        <Button
          label="Quay lại"
          @click="goBack()"
          icon="pi pi-arrow-circle-left"
          severity="secondary"
        />
        <Button
          label="In báo cáo"
          outlined
          icon="pi pi-print"
          severity="warning"
        />
        <Button
          label="Xuất excel"
          outlined
          icon="pi pi-file-export"
          severity="info"
        />
        <Button label="Bộ lọc" icon="pi pi-filter" @click="visibleFilter = true"/>
      </div>
    </div>
    <div>
      <div class="grid mx-4 my-4">
        <div class="col-6">
          <div class="flex flex-column gap-2">
            <div class="flex gap-8">
              <span class="font-bold w-3">Thời gian mua hàng</span>
              <span class="font-normal">-</span>
            </div>
            <div class="flex gap-8">
              <span class="font-bold w-3">Sản phẩm</span>
              <span class="font-normal">-</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <hr />
    <div class="card">
      <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem">
        <Column header="#" style="width: 3rem">
          <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column header="Mã sản phẩm" style="width: 8rem"></Column>
        <Column header="Tên sản phẩm" style="width: 10rem"></Column>
        <Column header="Thương hiệu" style="width: 10rem"></Column>
        <Column header="Ngành hàng" style="width: 10rem"></Column>
        <Column header="Loại sản" style="width: 10rem"></Column>
        <Column header="Quy cách bao bì" style="width: 10rem"></Column>
        <Column header="Giá trung bình sản phẩm" style="width: 10rem"></Column>
        <Column style="width: 5rem">
          <template #body="{ data }">
            <Button icon="pi pi-eye" @click="OpenDetail" text/>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>

  <!-- Chi tiết từng báo cáo  -->
  <Dialog
    v-model:visible="visibleDetail"
    header="Chi tiết báo cáo giá trung bình sản phẩm"
    modal
  >
    <div class="card">
      <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem">
        <Column header="#" style="width: 3rem">
          <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column header=" Tháng" style="width: 10rem"></Column>
        <Column header=" Đơn giá" style="width: 12rem"></Column>
        <Column header=" Mã đơn hàng" style="width: 10rem"></Column>
      </DataTable>
    </div>
  </Dialog>

  <!--  Bộ lọc -->
  <Dialog v-model:visible="visibleFilter" header="Bộ lọc" style="width: 35%" modal>
    <div class="flex justify-content-between align-content-evenly card">
      <div class="grid w-full">
        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Thời gian mua hàng</label>
          <Calendar
            v-model="dates"
            selectionMode="range"
            :manualInput="false"
            placeholder="Từ ngày - đến ngày"
          />
        </div>

        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Sản phẩm</label>
          <Dropdown
            optionLabel="name"
            placeholder="Chọn sản phẩm "
            class="w-full md:w-14rem"
          />
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex justify-content-end gap-2">
        <Button
          type="button"
          label="Bỏ qua"
          severity="secondary"
          @click="visibleFilter = false"
        />
        <Button type="button" label="Xác nhận" @click="visibleFilter = false"/>
      </div>
    </template>
  </Dialog>
</template>

<script setup>
import { ref } from "vue";

const visibleFilter = ref(true);
const visibleDetail = ref(false);

const goBack = () => {
  window.history.back();
};
const OpenDetail = () => {
  visibleDetail.value = true;
};
</script>
