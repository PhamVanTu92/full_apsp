<template>
  <DataTable showGridlines
    :value="props.type === 'ApprovalTemplates' ? props.data.wtM1.filter(el => { return el.selectedAppLv }) : props.data.wsT1.filter(el => { return el.selectedAppLv })">
    <Column class="w-3rem" header="#">
      <template #body="slotProps">
        <div>
          {{ slotProps.index + 1 }}
        </div>
      </template>
    </Column>
    <Column style="min-width: 200px" :header="t('common.approver')" headerClass="flex justify-content-center">
      <template #body="slotProps">
        <span>{{ slotProps.data.fullName }}</span>
      </template>
    </Column>
    <Column :header="t('common.action')">
      <template #body="slotProps">
        <div>
          <Button @click="removeApprovaler(slotProps.data)" text severity="danger" icon="pi pi-trash"/>
        </div>
      </template>
    </Column>
  </DataTable>
  <div class="flex align-items-center mt-2">
    <Button icon="pi pi-plus-circle" @click="openAddApprovalerDialog()" :label="t('common.btn_add_approver')"
      :severity="props.validate && props.data.wtM1.filter(el => { return el.selectedAppLv }).length < 1 ? 'danger' : 'primary'"
      :outlined="props.validate" text/>
  </div>

  <Dialog v-model:visible="addApprovalerModal" modal :header="t('common.user_list')" :style="{ width: '700px' }">
    <div class="flex flex-column gap-4">
      <InputText class="w-full" :placeholder="t('common.placeholder_search')"></InputText>
      <div class="card p-2 overflow-y-auto bg-gray-100" style="height: 500px">
        <div v-for="(item, index) in Approvalers" :key="index" class="card flex align-items-center gap-4 m-1">
          <Checkbox v-model="item.selectedAppLv" binary></Checkbox>
          <div class="flex flex-column gap-2">
            <div class="flex gap-2">
              <strong>{{ t('common.username_label') }}:</strong>
              <span>{{ item.fullName }}</span>
            </div>
            <div class="flex gap-2">
              <strong>Email:</strong>
              <span>{{ item.email }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex gap-2">
        <Button @click="addApprovalerModal = false" :label="t('common.btn_close')" icon="pi pi-times" severity="secondary"/>
        <Button @click="confirmAddApplr()" :label="t('common.btn_confirm')" icon="pi pi-check"/>
      </div>
    </template>
  </Dialog>
</template>
<script setup>
import { onMounted, reactive, ref, watchEffect } from 'vue';
import { useI18n } from 'vue-i18n';
import API from '../api/api-main';
const { t } = useI18n();


const props = defineProps({
  data: {
    type: Object,
  },
  type: {
    type: String,
    default: ""
  },
  validate: {
    type: Boolean,
    default: false
  },
  dataAPI: {
    type: Array,
    default: []
  }
})
const emits = defineEmits(['GetWsT1Data'])
onMounted(() => {
  fetchAllApprovalers()
})

const isLoading = ref(false)
const Approvalers = ref([])
const addApprovalerModal = ref(false)
const openAddApprovalerDialog = () => {
  addApprovalerModal.value = true;

};

const confirmAddApplr = () => {
  if (props.type === 'ApprovalTemplates') {
    props.data.wtM1 = Approvalers.value.filter(el => {
      return el.selectedAppLv
    })
    addApprovalerModal.value = false;
  } else {
    props.data.wsT1 = Approvalers.value.filter((el) => {
      return el.selectedAppLv;
    });
    addApprovalerModal.value = false;
  }
};
const fetchAllApprovalers = async () => {
  isLoading.value = true;
  try {
    const res = await API.get(`Account/getall?skip=0&limit=200&userType=APSP&Filter=(status=A)`);
    if (res) {
      Approvalers.value = ConvertData(res.data.item);
      isLoading.value = false;
    }
  } catch (error) {
    console.error(error);
  }
};
const ConvertData = (data) => {
  return data.map((el) => {
    return {
      id: el.userId ? el.id : 0,
      fatherId: el.userId ? el.fatherId : 0,
      email: el.email,
      fullName: el.fullName,
      userId: el.userId ? el.userId : el.id,
      status: el.status ? el.status : "",
    };
  });
};

const removeApprovaler = (data) => {
  if (data.id != 0) {
    data.selectedAppLv = false;
    return data.status = "D";
  }
  data.selectedAppLv = false;
};
watchEffect(() => {
  if (props.dataAPI.length) {
    const approvalerMap = new Map();
    Approvalers.value.forEach((e) => {
      approvalerMap.set(e.userId, e);
    });
    props.dataAPI.forEach((el) => {
      const approvaler = approvalerMap.get(el.userId);
      if (approvaler) {
        approvaler.selectedAppLv = true;
        approvaler.id = el.id;
        approvaler.fatherId = el.fatherId;
        el.fullName = approvaler.fullName;
        el.selectedAppLv = true;
      }
    });
    props.dataAPI = Approvalers.value.filter((el) => {
      return el.selectedAppLv;
    });
  }

});
</script>
<style></style>
