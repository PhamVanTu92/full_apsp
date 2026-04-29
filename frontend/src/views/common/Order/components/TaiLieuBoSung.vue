<template>
  <div>
    <DataTable :value="odStore.order?.attachFile" show-gridlines>
      <template #header>
        <div class="flex justify-content-between">
          <div class="my-2 text-lg">{{ t("client.additional_documents") }}</div>
          <!-- {{ odStore.order?.approval?.status }} -->
          <template
            v-if="
              odStore.order?.status == 'DXL' && 
              odStore.order?.approval?.status != 'A'
            "
          >
            <div class="flex gap-2" v-if="editMode">
              <Button
                @click="onClickCancel"
                :label="t('body.status.HUY2')"
                severity="secondary"
              />
              <Button @click="onClickSave" :label="t('client.save')" :loading="loading" />
            </div>
            <Button v-else @click="editMode = true" :label="t('client.edit')" />
          </template>
        </div>
      </template>
      <Column header="#" class="w-3rem">
        <template #body="{ index }">{{ index + 1 }}</template>
      </Column>
      <Column field="note" :header="t('client.document_name')"></Column>
      <Column field="memo" :header="t('client.note')"></Column>
      <Column field="fileName" :header="t('client.attachment')">
        <template #body="{ data, field }">
          <div class="flex gap-2">
            <div
              v-if="data._file?.name || data[field]"
              class="flex gap-3 align-items-center justify-content-between1"
            >
              <div class="">
                <i class="pi pi-file mr-2"></i>
                <span>{{ data._file?.name || data[field] }}</span>
              </div>
              <Button
                v-if="data['filePath']"
                icon="pi pi-download"
                text
                @click="onClickDownloadFile(data)"
                v-tooltip.bottom="'Tải xuống'"
              />
            </div>
            <FileSelect
              v-if="editMode"
              @change="onChangeFile($event, data)"
              label="Cập nhật"
              icon="pi pi-upload"
            />
          </div>
        </template>
      </Column>
      <Column class="w-5rem text-center" v-if="editMode">
        <template #body="{ index }">
          <div class="flex gap-2">
            <Button
              @click="onClickRemove(index)"
              text
              outlined
              icon="pi pi-trash"
              severity="danger"
            />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="py-5 my-5 text-center font-italic text-500">
          {{ t("client.no_data") }}
        </div>
      </template>
    </DataTable>
    <!-- {{ odStore.order?.attachFile }} -->
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, watchEffect } from "vue";
  import { useOrderDetailStore } from "../store/orderDetail";
  import API from "@/api/api-main";
  import { useToast } from "primevue/usetoast";
  import { useI18n } from "vue-i18n";

  // Biến dữ liệu
  const { t } = useI18n();
  const odStore = useOrderDetailStore();
  const toast = useToast();
  const editMode = ref(false);
  const attachFileDefault = ref();
  const loading = ref(false);
  const props = defineProps({
    isClient: {
      default: false,
    },
  });

  //Function
  const onClickDownloadFile = (data: any) => {
    var a = document.createElement("a");
    a.href = data.filePath;
    a.target = "_blank";
    a.download = data.fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  };

  const onChangeFile = (files: File[], data: any) => {
    data._file = files[0];
  };

  const onClickSave = () => {
    const formData = new FormData();
    const payload = {
      id: odStore.order?.id,
      attachFile: [] as any,
    };
    const files = odStore.order?.attachFile.filter((row) => row._file) || [];
    // if (files?.length < 1) {
    //   onClickCancel();
    //   return;
    // }
    files?.forEach((row) => {
      if (row._file) {
        formData.append("attachFile", row._file);
        payload.attachFile.push({
          id: row.id,
          filename: row._file?.name,
          note: row.note,
          memo: row.memo,
          fatherId: row.fatherId,
          status: "U",
        });
      }
    });
    formData.append("document", JSON.stringify(payload));
    loading.value = true;
    API.update(`PurchaseOrder/${payload.id}`, formData)
      .then(() => {
        toast.add({
          severity: "success",
          summary: "Thành công",
          detail: "Cập nhật thành công",
          life: 3000,
        });
        onClickCancel();
        odStore.fetchStore();
      })
      .catch((error) => {
        console.error(error);
        toast.add({
          severity: "error",
          summary: "Lỗi",
          detail: "Đã có lỗi xảy ra",
          life: 3000,
        });
      })
      .finally(() => {
        loading.value = false;
      });
  };

  const onClickCancel = () => {
    editMode.value = false;
    if (odStore.order) odStore.order.attachFile = attachFileDefault.value || [];
  };

  const onClickRemove = (index: number) => {
    odStore.order?.attachFile.splice(index, 1);
  };

  const initialComponent = async () => {
    //code here
  };

  // Life cycle
  watchEffect(() => {
    // khi store khởi tạo thì mới gán giá trị ban đầu
    if (odStore.order && odStore.order?.attachFile?.length > 0) {
      attachFileDefault.value = JSON.parse(
        JSON.stringify(odStore.order?.attachFile || [])
      );
    }
  });

  onMounted(function () {
    initialComponent();
  });
</script>
