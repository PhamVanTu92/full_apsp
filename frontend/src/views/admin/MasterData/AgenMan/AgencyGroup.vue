<template>
  <div class="flex justify-content-between align-items-center mb-4">
    <h4 class="font-bold m-0">Nhóm nhà phân phối</h4>
    <div class="flex gap-2">
      <Button label="Thêm mới" icon="fa-solid fa-plus" @click="NewItem()"/>
      <Button icon="fa-solid fa-rotate-right" disabled/>
    </div>
  </div>

  <div class="grid mt-3 card p-2">
    <div class="col-12 flex gap-3">
      <!-- <Dropdown class="w-3" placeholder="Thêm điều kiện lọc" /> -->
      <InputGroup class="w-30rem">
        <InputText
          placeholder="Tìm kiếm nhóm nhà phân phối"
          v-model="dataEdit.keysearch"
        />
        <Button
          icon="pi pi-search"
          severity="warning"
          @click="GetItem(dataEdit.keysearch)"
        />
      </InputGroup>
    </div>
    <div class="col-12 h-screen bg-white">
      <DataTable
        class="table-main"
        showGridlines
        :value="dataEdit.distributor_groups"
        v-model:selection="dataEdit.selectedProduct"
        dataKey="id"
        stripedRows
        selectionMode="single"
        tableStyle="min-width: 50rem;"
        header="surface-200"
        paginator
        :rows="dataTable.size"
        :page="dataTable.page"
        :totalRecords="dataTable.total_count"
        @page="onPageChange($event)"
        :rowsPerPageOptions="[10, 20, 30]"
        lazy
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} nhóm nhà phân phối"
      >
        <Column selectionMode="multiple"></Column>
        <Column header="#" :style="{ width: '5%' }">
          <template #body="slotProps">
            {{ slotProps.index + 1 }}
          </template>
        </Column>
        <Column field="code" header="Mã NPP" :style="{ width: '15%' }"></Column>
        <Column field="name" header="Tên nhóm nhà phân phối" :style="{ width: '20%' }">
        </Column>
        <Column field="description" header="Mô tả " :style="{ width: '20%' }">
          <template #body="slotProps">
            <div v-html="slotProps.data.description"></div>
          </template>
        </Column>
        <Column field="created_at" header="Ngày tạo" :style="{ width: '20%' }">
          <template #body="slotProps">
            <div>{{ format(slotProps.data.created_at, "dd/MM/yyyy") }}</div>
          </template>
        </Column>

        <Column header="Hành động" :style="{ width: '20%' }">
          <template #body="slotProps">
            <div>
              <Button
                class="ml-1"
                icon="fa-regular fa-pen-to-square"
                text
                @click="UpdateData(slotProps.data)"
              />
              <Button
                @click="deleteItem(slotProps.data)"
                severity="danger"
                icon="fa-solid fa-trash"
                text
              />
              <Button
                class="ml-1"
                icon="fa-solid fa-ellipsis-vertical"
                text
                @click="ActionCopy($event, slotProps.data)"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>

  <Dialog
    v-model:visible="dialogItem"
    modal
    position="top"
    :draggable="false"
    :header="
      payload.id
        ? 'Cập nhật nhóm nhà phân phối'
        : payload.copy == true
        ? 'Sao chép nhóm nhà phân phối'
        : 'Thêm mới nhóm nhà phân phối'
    "
    :style="{ width: '65%' }"
    class="p-fluid"
  >
    <div class="card">
      <h6 class="mb-3 uppercase font-bold">Thông tin chung</h6>
      <div class="grid mt-3">
        <div class="col-6">
          <div class="field">
            <label for="code">Mã nhóm nhà phân phối</label>
            <span
              style="
                color: #eb5757;
                font-size: 16px;
                font-family: Open Sans;
                font-weight: 400;
                word-wrap: break-word;
                margin-left: 3px;
              "
              >*</span
            >
            <InputText v-model="payload.code" />
          </div>
        </div>
        <div class="col-6">
          <div class="field">
            <label for="code">Tên nhóm nhà phân phối</label>
            <span
              style="
                color: #eb5757;
                font-size: 16px;
                font-family: Open Sans;
                font-weight: 400;
                word-wrap: break-word;
                margin-left: 3px;
              "
              >*</span
            >
            <InputText v-model="payload.name" />
          </div>
        </div>

        <div class="col-12">
          <div class="field">
            <label for="name">Mô tả nhóm nhà phân phối</label>
            <Textarea rows="5" v-model="payload.description" />
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex justify-end gap-2">
        <Button
          type="button"
          label="Huỷ"
          severity="secondary"
          @click="dialogItem = false"
        />
        <Button type="button" label="Lưu" @click="SaveItem()"/>
      </div>
    </template>
  </Dialog>
  <Dialog
    v-model:visible="dialogDeleteItem"
    modal
    position="top"
    :draggable="false"
    header="Xác nhận"
    :style="{ width: '35%' }"
    class="p-fluid"
  >
    <div
      class="flex flex-column align-items-center w-full gap-3 border-bottom-1 surface-border pb-0"
    >
      <i class="pi pi-info-circle text-6xl text-red-500"></i>
      <p>
        Bạn có chắc chắn xoá nhóm nhà phân phối
        {{ dataEdit.dataDelete.name }} không?
      </p>
    </div>
    <template #footer>
      <div class="flex gap-2">
        <Button label="Huỷ" outlined severity="secondary" @click="confirmDelete(false)" />
        <Button label="Xoá" @click="confirmDelete(true)" severity="danger" />
      </div>
    </template>
  </Dialog>

  <OverlayPanel ref="opcopy" appendTo="body">
    <div class="flex flex-column gap-3">
      <Button icon="fa-solid fa-copy" text @click="CopyGroup()"/>
    </div>
  </OverlayPanel>
  <Loading v-if="loading" />
</template>

<script setup>
// eslint-disable-next-line no-unused-vars
import { ref, onBeforeMount, watch } from "vue";
import API from "@/api/api-main";
import { format } from "date-fns";
import merge from "lodash/merge";
import { useGlobal } from "@/services/useGlobal";
import { useRouter, useRoute } from "vue-router";

const API_URL = "distributor-groups";
const loading = ref(false);
const router = useRouter();
const route = useRoute();
const { toast, FunctionGlobal } = useGlobal();
const dataTable = ref({
  page: route.query.page ? route.query.page : 1,
  size: route.query.size ? route.query.size : 10,
});
const payload = ref({
  description: "",
  code: "",
  name: "",
  id: 0,
});
const dataClear = JSON.stringify(payload.value);
const dialogItem = ref(false);
const dialogDeleteItem = ref(false);
const dataEdit = ref({ submited: false });
const opcopy = ref("");
const idCopy = ref("");

onBeforeMount(() => {
  GetItem();
});

const GetItem = async (key = "") => {
  loading.value = true;
  try {
    const res = await API.get(
      `${API_URL}?size=${dataTable.value.size}&page=${dataTable.value.page}&q=${key}`
    );
    if (res.data) {
      dataEdit.value.distributor_groups = res.data.distributor_groups;
      dataTable.value = res.data;
    }
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
    router.push(`?size=${dataTable.value.size}&page=${dataTable.value.page}&q=${key}`);
  }
};

const onPageChange = (event) => {
  dataTable.value.page = event.page;
  dataTable.value.size = event.rows;
  GetItem();
};
const NewItem = () => {
  ClearData();
  dialogItem.value = true;
};
const UpdateData = async (data) => {
  try {
    loading.value = true;
    const res = await API.get(`${API_URL}/${data.id}`);
    if (res.data) payload.value = merge({}, payload.value, removeNullValues(res.data));
    dialogItem.value = true;
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
  }
};

const SaveItem = async () => {
  if (!ValidateData()) return;
  loading.value = true;
  const FUNAPI = payload.value.id
    ? API.update(`${API_URL}/${payload.value.id}`, payload.value)
    : API.add(`${API_URL}`, payload.value);
  try {
    const res = await FUNAPI;
    if (res.data) {
      FunctionGlobal.$notify("S", res.data.message, toast);
    }
  } catch (error) {
    FunctionGlobal.$notify("E", error.response.data.error.message, toast);
  } finally {
    loading.value = false;
    dialogItem.value = false;
    ClearData();
  }
};
const ClearData = () => {
  payload.value = JSON.parse(dataClear);
  GetItem();
};

const deleteItem = (data) => {
  dataEdit.value.dataDelete = data;
  dialogDeleteItem.value = true;
};

const confirmDelete = async (status) => {
  if (status) {
    try {
      loading.value = true;
      const res = await API.delete(`${API_URL}/${dataEdit.value.dataDelete.id}`);
      if (res.data) FunctionGlobal.$notify("S", "Đã xoá thành công", toast);
    } catch (error) {
      FunctionGlobal.$notify("E", error, toast);
    } finally {
      loading.value = false;
      dialogDeleteItem.value = false;
      GetItem();
    }
  } else {
    dialogDeleteItem.value = false;
    dataEdit.value.dataDelete = {};
  }
};

const ValidateData = () => {
  let status = true;
  const fields = [
    {
      value: payload.value.code,
      message: "Vui lòng nhập mã nhóm nhà phân phối",
    },
    {
      value: payload.value.name,
      message: "Vui lòng nhập tên nhóm nhà phân phối",
    },
  ];

  fields.forEach((field) => {
    if (!field.value) {
      FunctionGlobal.$notify("E", field.message, toast);
      status = false;
    } else if (field.validate && !field.validate(field.value)) {
      FunctionGlobal.$notify("E", field.invalidMsg, toast);
      status = false;
    }
  });

  return status;
};

const removeNullValues = (obj) => {
  const result = {};
  Object.keys(obj).forEach((key) => {
    if (obj[key] !== null) {
      result[key] = obj[key];
    }
  });
  return result;
};

const ActionCopy = (event, data) => {
  opcopy.value.toggle(event);
  idCopy.value = data.id;
};
const CopyGroup = async () => {
  try {
    loading.value = true;
    const res = await API.get(`${API_URL}/${idCopy.value}`);
    if (res.data) payload.value = merge({}, payload.value, removeNullValues(res.data));
    payload.value.id = 0;
    payload.value.copy = true;
    payload.value.name = "Nhân bản từ " + payload.value.name;
    payload.value.sku = "copy_" + payload.value.code;
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
    dialogItem.value = true;
  }
};
</script>
<style scoped>
.btn-delete-img {
  display: none;
}
.box-op-img:hover {
  background-color: rgba(0, 0, 0, 0.315);
}
.box-img:hover .btn-delete-img {
  display: block;
  z-index: 9999;
}
:deep(.file_uploads .p-fileupload-file-thumbnail) {
  display: none;
}
</style>
