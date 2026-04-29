<template>
  <DataTable :reorderableColumns="true" @rowReorder="onRowReorder" showGridlines
    :value="props.data.approvalSampleProcessesLines.filter((el) => el.status != 'D' && el.selectedAppLv)">
    <template #empty>
      <div class="text-center p-5 m-5">{{ t('Custom.noBrowsers') }}</div>
    </template>
    <Column rowReorder headerStyle="width: 3rem" :reorderableColumn="false" />
    <Column class="w-3rem" header="#">
      <template #body="slotProps">
        <div>
          {{ slotProps.index + 1 }}
        </div>
      </template>
    </Column>
    <Column style="min-width: 200px" :header="t('Navbar.menu.approvalLevel')" headerClass="flex justify-content-center">
      <template #body="slotProps">
        <span>{{ slotProps.data.name }}</span>
      </template>
    </Column>
    <Column field="description" :header="t('body.OrderApproval.description')"></Column>
    <Column :header="t('common.action')">
      <template #body="slotProps">
        <div>
          <Button @click="removeApproval(slotProps.data)" text severity="danger" icon="pi pi-trash" />
        </div>
      </template>
    </Column>
    <template #footer>
      <Button icon="pi pi-plus-circle" @click="openAddApprovalsDialog()"
        :label="t('body.systemSetting.add_approval_level')" text />

    </template>
  </DataTable>

  <Dialog v-model:visible="addApprovalModal" modal :header="t('Custom.listOfApproval')" :style="{ width: '700px' }">
    <div class="flex flex-column gap-4">
      <!-- <InputText class="w-full" :placeholder="t('body.home.search_placeholder')" @input="debouncedFetchAllApprovalers"
        v-model="keyInput" /> -->
      <div class="card p-2 overflow-y-auto bg-gray-100" style="height: 500px">
        <div v-for="(item, index) in Approvals" :key="index" class="card flex align-items-center gap-4 m-1">
          <Checkbox v-model="item.isCheck" binary></Checkbox>
          <div class="flex flex-column gap-2">
            <div class="flex gap-2">
              <strong>{{ t('body.sampleRequest.customerGroup.approval_level_name_column') }}:</strong>
              <span>{{ item.name }}</span>
            </div>
            <div class="flex gap-2">
              <strong>{{ t('common.description') }}:</strong>
              <span>{{ item.description }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex gap-2">
        <Button @click="closeApprovalModal()" :label="t('common.btn_close')" icon="pi pi-times" severity="secondary" />
        <Button @click="confirmAddApplr()" :label="t('common.btn_confirm')" icon="pi pi-check" />
      </div>
    </template>
  </Dialog>
</template>
<script setup>
import { onMounted, ref } from "vue";
import API from "../api/api-main";
import { useI18n } from 'vue-i18n';
 
const debounce = (func, delay) => {
  let timeoutId;
  return (...args) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func.apply(null, args), delay);
  };
};
const { t } = useI18n();
const props = defineProps({
  data: {
    type: Object,
  },
  type: {
    type: String,
    default: "",
  },
  validate: {
    type: Boolean,
    default: false,
  },
});
const emits = defineEmits(["GetWsT1Data"]);
onMounted(() => {
  fetchAllApprovalers();
});

const keyInput = ref('')
const isLoading = ref(false);
const Approvals = ref([]);
const addApprovalModal = ref(false);
const openAddApprovalsDialog = () => {
  addApprovalModal.value = true;
}; 
const confirmAddApplr = () => {
  if (props.type === "ApprovalTemplates") {
    props.data.approvalSampleProcessesLines = Approvals.value.filter((el) => {
      el.selectedAppLv = el.isCheck;
      if (el.isCheck && el.id) el.status = "U";
      if (!el.isCheck && el.id) el.status = "D";
      return el.isCheck || el.status == "D";
    });
    SortArr(props.data.approvalSampleProcessesLines);
    addApprovalModal.value = false;
  }
};
const fetchAllApprovalers = async () => {
  isLoading.value = true;
  try {
    const res = await API.get(`ApprovalLevel?search=${keyInput.value}`);
    if (res) {
      Approvals.value = ConvertData(res.data.result);
      if (props.data.approvalSampleProcessesLines.length > 0) {
        const selectedIds = new Map();
        props.data.approvalSampleProcessesLines.forEach((el) => {
          selectedIds.set(el.wtsId, el);
        });
        Approvals.value.forEach((val) => {
          let data = selectedIds.get(val.wtsId);
          if (data != undefined) {
            val.id = data.id;
            val.sort = data.sort;
            val.selectedAppLv = true;
            val.isCheck = true;
            data.selectedAppLv = true;
            data.approvalLevelName = val.approvalLevelName;
          }
        });
      }
      props.data.approvalSampleProcessesLines = Approvals.value
        .filter((el) => {
          return el.isCheck || el.status == "D";
        })
        .sort((a, b) => a.sort - b.sort);
      isLoading.value = false;
    }
  } catch (error) {
    console.error(error);
  }
};
const debouncedFetchAllApprovalers = debounce(fetchAllApprovalers, 500);
const ConvertData = (data) => {
  return data.map((el) => {
    return {
      id: 0,
      wtsId: el.id,
      name: el.approvalLevelName,
      description: el.description,
      status: "",
      isCheck: "",
    };
  });
};

const removeApproval = (item) => {
  item.isCheck = false;
  item.selectedAppLv = false;
  if (item.id) item.status = "D";
  SortArr(props.data.approvalSampleProcessesLines.filter((el) => el.status != "D"));
};

const findIndexById = (id) => {
  let index = -1;
  for (let i = 0; i < props.data.approvalSampleProcessesLines.length; i++) {
    if (props.data.approvalSampleProcessesLines[i].id === id) {
      index = i;
      break;
    }
  }
  return index;
};
const closeApprovalModal = () => {
  Approvals.value.filter((el) => {
    el.isCheck = el.selectedAppLv;
  });
  addApprovalModal.value = false;
};
const onRowReorder = (event) => {
  event.value.forEach((el, index) => {
    el.sort = index + 1;
    el.status = !el.id ? "" : "U";
  });
  SortArr(props.data.approvalSampleProcessesLines);
};

const SortArr = (data) => {
  data.sort((a, b) => {
    if (a.sort === undefined) return 1;
    if (b.sort === undefined) return -1;
    return a.sort - b.sort;
  });
  data.forEach((item, index) => {
    item.sort = index + 1;
  });
};
</script>
<style></style>
