<template>
  <div>
    <div class="flex justify-content-between align-content-center">
      <h4 class="font-bold m-0">Báo cáo nhập, xuất, tồn hàng gửi</h4>
      <div class="flex gap-2">
        <Button
          label="In báo cáo"
          outlined
          icon="pi pi-print"
          severity="warning"
        />
        <Button
          :label="t('body.home.export_excel_button')"
          outlined
          icon="pi pi-file-export"
          severity="info"
        />
        <Button :label="t('client.filter')" icon="pi pi-filter" @click="visibleFilter = true"/>
      </div>
    </div>
    <div>
      <div class="grid mx-4 my-4">
        <div class="col-6">
          <div class="flex flex-column gap-2">
            <div class="flex gap-8">
              <span class="font-bold w-3">{{t('body.home.category_column')}}</span>
              <span class="font-normal">-</span>
            </div>
            <div class="flex gap-8">
              <span class="font-bold w-3">{{t('body.home.brand_column')}}</span>
              <span class="font-normal">-</span>
            </div>
          </div>
        </div>
        <div class="col-6">
          <div class="flex flex-column gap-2">
            <div class="flex gap-8">
              <span class="font-bold w-3">{{t('body.home.packaging_column')}}</span>
              <span class="font-normal">-</span>
            </div>
            <div class="flex gap-8">
              <span class="font-bold w-3">{{t('body.home.product_type_column')}}</span>
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
        <Column header=" Tên hàng hóa" style="width: 10rem"></Column>
        <Column header=" Quy cách bao bì" style="width: 15rem"></Column>
        <Column header=" Thương hiệu" style="width: 10rem"></Column>
        <Column header=" Ngành hàng" style="width: 10rem"></Column>
        <Column header=" Loại hàng hóa" style="width: 10rem"></Column>
        <Column header=" Số lượng tồn đầu kỳ" style="width: 10rem"></Column>
        <Column header=" Yêu cầu lấy hàng gửi" style="width: 10rem"></Column>
        <Column header=" SL khả dụng " style="width: 10rem"></Column>
        <Column style="width: 5rem">
          <template #body="{ data }">
            <Button icon="pi pi-eye" @click="OpenDetail" text/>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>

  <!-- Chi tiết từng báo cáo  -->
  <Dialog v-model:visible="visibleDetail" header="Chi tiết báo cáo" modal>
    <div class="card">
      <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem">
        <Column header="#" style="width: 3rem">
          <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column header="Mã đơn hàng / yêu cầu lấy hàng gửi" style="width: 20rem"></Column>
        <Column header=" Số lô nhập / xuất" style="width: 12rem"></Column>
        <Column header=" Thời gian" style="width: 10rem"></Column>
        <Column header=" Số lượng tồn đầu kỳ " style="width: 10rem"></Column>
        <Column header=" Số lượng nhập " style="width: 10rem"></Column>
        <Column header=" Số lượng xuất " style="width: 10rem"></Column>
        <Column header=" Số lượng khả dụng" style="width: 10rem"></Column>
      </DataTable>
    </div>
  </Dialog>

  <!--  Bộ lọc -->
  <Dialog v-model:visible="visibleFilter" header="Bộ lọc" style="width: 35%" modal>
    <div class="flex justify-content-between align-content-evenly card">
      <div class="grid w-full">
        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Ngành hàng</label>

          <Dropdown
            optionLabel="name"
            placeholder="Chọn ngành hàng"
            class="w-full md:w-14rem"
          />
        </div>

        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Thương hiệu</label>
          <Dropdown
            optionLabel="name"
            placeholder="Chọn thương hiệu"
            class="w-full md:w-14rem"
          />
        </div>
        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Quy cách bao bì</label>
          <Dropdown
            optionLabel="name"
            placeholder="Chọn QCBB"
            class="w-full md:w-14rem"
          />
        </div>
        <div class="col-6 flex flex-column gap-2">
          <label class="font-bold" for=""> Loại hàng hóa</label>
          <Dropdown
            optionLabel="name"
            placeholder="Chọn loại hàng hóa"
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
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const visibleFilter = ref(true);
const visibleDetail = ref(false);

const OpenDetail = () => {
  visibleDetail.value = true;
};

const goBack = () => {
  window.history.back();
};
</script>
