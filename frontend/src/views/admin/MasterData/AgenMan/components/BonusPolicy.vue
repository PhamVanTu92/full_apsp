<template>
  <div class="card">
    <div class="flex justify-content-between mb-3">
      <div class="py-2 text-green-700 text-xl font-semibold">Chính sách thưởng</div>
      <div class="flex gap-3">
        <Button
          v-if="!editMode"
          icon="pi pi-pencil"
          label="Chỉnh sửa"
          @click="onClickEdit()"
          text
        />
        <template v-else>
          <Button
            icon="pi pi-times"
            label="Huỷ"
            @click="onClickCancel()"
            severity="secondary"
            text
          />
          <Button @click="save()" icon="pi pi-save" label="Lưu" />
        </template>
      </div>
    </div>
    <DataTable :value="editMode == true ? data : fetchData">
      <template #empty>
        <div class="p-5 text-center">Chưa có dữ liệu để hiển thị</div>
      </template>
      <Column header="#">
        <template #body="{ index }">{{ index + 1 }}</template>
      </Column>
      <Column header="Loại thưởng" field="bonusType">
        <template #body="{ data }">
          <Dropdown
            v-model="data.bonusType"
            :disabled="!editMode"
            :options="['Thanh toán ngay', 'Thanh toán đúng hẹn']"
            placeholder="Chọn loại thưởng"
          ></Dropdown>
        </template>
      </Column>
      <Column header="Giá trị" field="value">
        <template #body="{ data }">
          <InputNumber :disabled="!editMode" v-model="data.value"></InputNumber>
        </template>
      </Column>
      <Column>
        <template #body="{ index }">
          <div>
            <Button
              v-if="editMode"
              @click="removeRow(index)"
              icon="pi pi-trash"
              severity="danger"
            />
          </div>
        </template>
      </Column>
      <template v-if="editMode == true" #footer>
        <Button label="Thêm dòng" @click="addRow()" icon="pi pi-plus" outlined/>
      </template>
    </DataTable>
  </div>
</template>

<script setup>
import { ref } from "vue";

const editMode = ref(false);
const data = ref([]);
const fetchData = ref([]);
let originalData = ref([]);

const onClickEdit = () => {
  originalData.value = JSON.parse(JSON.stringify(fetchData.value));
  data.value = JSON.parse(JSON.stringify(fetchData.value));
  editMode.value = true;
};

const onClickCancel = () => {
  data.value = JSON.parse(JSON.stringify(originalData.value));
  editMode.value = false;
};

const addRow = () => {
  const payload = {
    bonusType: "",
    value: 0,
  };
  data.value.push(payload);
};

const removeRow = (index) => {
  data.value.splice(index, 1);
};

const save = () => {
  fetchData.value = JSON.parse(JSON.stringify(data.value));
  editMode.value = false;
};
</script>
