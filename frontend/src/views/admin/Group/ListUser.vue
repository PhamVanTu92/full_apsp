<template>
  <div class="border-1 border-solid border-200 p-3">
    <div class="flex justify-content-between align-items-center mb-3">
      <div><label class="uppercase font-bold">{{ t('client.brand') }}</label></div>
      <div>
        <Button
          label="Thêm người dùng"
          icon="pi pi-plus"
          @click="dialogAddUserToGroup = true"
          outlined
        />
      </div>
    </div>
    <DataTable
      :value="modelValue"
      showGridlines
      stripedRows
      scrollHeight="300px"
      scrollable
      size="small"
    >
      <Column header="#" :style="{ width: '5%' }">
        <template #body="slotProps">
          <div class="text-center">
            {{ slotProps.index + 1 }}
          </div>
        </template>
      </Column>
      <Column header="Mã người dùng" field="userName" :style="{ width: '30%' }"></Column>
      <Column header="Tên người dùng" field="fullName" :style="{ width: '50%' }">
      </Column>
      <Column :style="{ width: '15%' }">
        <template #body="slotProps">
          <div class="flex justify-content-center">
            <Button
              icon="pi pi-trash"
              text
              severity="danger"
              @click="removeUser(slotProps.index)"
            />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="my-8 text-center text-600 font-italic">Danh sách trống</div>
      </template>
    </DataTable>
    <div class="Numberclass border-1">
      <p>Số lượng: {{ modelValue?.length }}</p>
    </div>
    <Dialog
      v-model:visible="dialogAddUserToGroup"
      position="center"
      :draggable="false"
      modal
      header="Thêm mới người dùng vào nhóm"
      :style="{ width: '50rem' }"
    >
      <div class="flex mb-3 w-full">
        <IconField iconPosition="left" class="w-full">
          <InputText
            v-model="userSearchKeyword"
            placeholder="Tìm kiếm theo tên"
            class="w-full"
          />
          <InputIcon>
            <i class="pi pi-search" />
          </InputIcon>
        </IconField>
      </div>
      <DataTable
        :value="filteredUsers"
        v-model:selection="selectedUsers"
        selectionMode="multiple"
        showGridlines
        stripedRows
        scrollHeight="300px"
        scrollable
      >
        <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
        <Column field="userName" header="Mã người dùng"></Column>
        <Column field="fullName" header="Tên người dùng"></Column>
        <Column field="email" header="Email"></Column>
      </DataTable>
      <template #footer>
        <div class="flex justify-content-end gap-2">
          <Button label="Huỷ" severity="secondary" @click="closeAddUserDialog" />
          <Button label="Chọn" @click="addSelectedUsers" />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, watch } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  modelValue: {
    type: Array,
    default: [],
  },
  idGroup: {
    type: Intl,
    default: 0,
  },
});

const emit = defineEmits(["update:modelValue"]);

const { toast, FunctionGlobal } = useGlobal();
const dialogAddUserToGroup = ref(false);
const userSearchKeyword = ref("");
const selectedUsers = ref([]);
const dListUser = ref([]);
const filteredUsers = ref([]);
const loading = ref(false);

const fetchUsers = async () => {
  loading.value = true;
  try {
    const res = await API.get("Account/getall?skip=0&limit=30");
    dListUser.value = res.data.item;
    filteredUsers.value = dListUser.value;
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
  }
};

const closeAddUserDialog = () => {
  dialogAddUserToGroup.value = false;
  selectedUsers.value = [];
};

const addSelectedUsers = () => {
  const updatedUsers = [...props.modelValue];
  selectedUsers.value.forEach((user) => {
    if (!updatedUsers.some((u) => u.id === user.id)) {
      updatedUsers.push(user);
    }
  });
  emit("update:modelValue", updatedUsers);
  closeAddUserDialog();
};

const removeUser = async (index) => {
  const updatedUsers = [...props.modelValue];
  if (props.idGroup == 0 || !updatedUsers[index].userGroup) {
    updatedUsers.splice(index, 1);
    emit("update:modelValue", updatedUsers);
    return;
  }
  try {
    const res = await API.delete(`UserGroup/${props.idGroup}/users`, [
      updatedUsers[index].id,
    ]);
    if (res.status === 200) {
      updatedUsers.splice(index, 1);
      emit("update:modelValue", updatedUsers);
    }
  } catch (error) {}
};

const filterUsers = () => {
  if (!userSearchKeyword.value) {
    filteredUsers.value = dListUser.value;
  } else {
    filteredUsers.value = dListUser.value.filter((user) =>
      user.userName.toLowerCase().includes(userSearchKeyword.value.toLowerCase())
    );
  }
};

watch(userSearchKeyword, () => {
  filterUsers();
});

fetchUsers();
</script>

<style scoped>
.Numberclass {
  background: #f9fafb;
  color: #374151;
  border: 1px solid #e5e7eb;
  border-width: 0 0 1px 0;
  padding: 1rem 1rem;
  font-weight: 700;
}
</style>
