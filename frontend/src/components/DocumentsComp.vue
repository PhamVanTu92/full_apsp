<template>
  <div>
    <div class="card p-0 border-noround">
      <div class="flex p-3 justify-content-between align-items-center">
        <div class="text-xl font-bold mb-0">
          {{ t("body.sampleRequest.customer.document_title") }}
        </div>
        <template v-if="['DXL'].includes(props.data.status)">
          <div
            v-if="props.data.attachFile?.length > 0 && !props.idCheck"
            class="flex justify-content-end gap-2"
          >
            <Button
              @click="acctiveEdit = true"
              v-if="!acctiveEdit"
              :label="t('client.edit')"
            />
            <Button
              v-if="acctiveEdit"
              @click="removeValueInFormData"
              severity="secondary"
              :label="t('body.PurchaseRequestList.cancel_button')"
            />
            <Button
              v-if="acctiveEdit"
              :label="t('body.systemSetting.save_button')"
              @click="confirmUpload()"
            />
          </div>
        </template>
      </div>
      <hr class="m-0" />
      <DataTable
        showGridlines
        :value="props.data?.attachFile?.filter((el) => el.status != 'D')"
        tableStyle="min-width: 50rem"
        class="p-3"
      >
        <template #empty>
          <div class="py-5 my-5 text-center">
            {{ t("body.systemSetting.no_data_to_display") }}
          </div>
        </template>
        <Column style="width: 3%" header="#">
          <template #body="slotProps">
            {{ slotProps.index + 1 }}
          </template>
        </Column>
        <Column
          field="note"
          :header="t('body.sampleRequest.customer.document_name_column')"
        ></Column>
        <Column
          field="memo"
          :header="t('body.sampleRequest.customer.note_column')"
        ></Column>
        <Column
          :header="t('body.sampleRequest.customer.document_title')"
          style="width: 35%"
        >
          <template #body="slotProps">
            <div class="flex align-items-center">
              <a target="_blank" :href="slotProps.data.filePath">
                <i v-if="slotProps.data.fileName" class="pi pi-file" /><span
                  class="p-2"
                  >{{ slotProps.data.fileName }}</span
                >
                <input
                  type="file"
                  class="hidden click-file"
                  @change="UploadFileLocal(slotProps.data, $event, slotProps.index)"
                />
              </a>
              <!-- <Button
                v-if="!props.idCheck && props.data.status != 'HUY'"
                :label="
                  slotProps.data.filePath
                    ? t('body.report.update_button')
                    : t('body.systemSetting.choose_file')
                "
                :icon="slotProps.data.filePath ? 'pi pi-upload' : 'pi pi-paperclip'"
                :text="slotProps.data.filePath ? false : true"
                class="w-9rem btn-up-file"
                raised 
              /> -->
              <i
                :class="
                  slotProps.data.fileName
                    ? 'pi pi-upload cursor-pointer text-green-500'
                    : 'pi pi-paperclip cursor-pointer'
                "
                @click="Openfile(slotProps.index)"
                v-tooltip.top="
                  slotProps.data.fileName
                    ? t('body.report.update_button')
                    : t('body.report.import_file_button')
                "
                v-if="!props.idCheck && props.data.status != 'HUY'"
              />
            </div>
          </template>
        </Column>
        <Column
          :header="t('body.sampleRequest.customer.creator_column')"
          style="width: 15%"
        ></Column>
        <Column
          header=""
          class="w-5rem"
          v-if="authStore.userType == 'APSP'"
        >
          <template #body="sp">
            <Button
              icon="pi pi-trash"
              severity="danger"
              text
              @click="removeItem(sp.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
  <loading v-if="isLoading"></loading>
</template>
<script setup>
  import { ref } from "vue";
  import API from "../api/api-main";
  import { useGlobal } from "@/services/useGlobal";
  import { useAuthStore } from "@/Pinia/auth";
  import { useI18n } from "vue-i18n";

  const authStore = useAuthStore();
  const { toast, FunctionGlobal } = useGlobal();
  const { t } = useI18n();
  const acctiveEdit = ref(false);
  const props = defineProps({
    data: Array,
    idCheck: String,
  });

  const emits = defineEmits(["callAPI"]);
  const formData = new FormData();
  const data = ref({
    id: 0,
    attachFile: [],
  });
  const isLoading = ref(false);

  const Openfile = (index) => {
    document.querySelectorAll(".click-file")[index].click();
  };
  const UploadFileLocal = async (el, event, index) => {
    if (event.target.files.length < 1) return;
    const file = event.target.files[0];
    const fileExtention = file.name.split(".").pop().toLowerCase();
    const fileName = file.name;
    // const allowFiles = ["image/jpeg", "image/png"];
    // const allowExt = ["pdf", "txt", "xlsx"];
    const fileAllow = ["jpg", "jpeg", "png", "pdf", "doc", "docx", "xls", "xlsx", "txt"];
    if (!fileAllow.includes(fileExtention)) {
      return FunctionGlobal.$notify("E", t("Custom.file_format_error"), toast);
    }
    // const fileExtensions = fileName.split(".").pop().toLowerCase();
    // if (!allowFiles.includes(file) && !allowExt.includes(fileExtensions)) {
    //   return FunctionGlobal.$notify(
    //     "E",
    //     "File đã chọn không đúng định dạng, vui lòng chọn file khác!",
    //     toast
    //   );
    // }
    if (fileName) {
      props.data.attachFile[index].fileName = fileName;
      data.value.id = props.data.id;
      const newDoc = {
        id: el.id,
        filename: fileName,
        note: el.note,
        memo: el.memo,
        fatherId: props.data.id,
        status: "U",
      };
      const existingDocIndex = data.value.attachFile.findIndex(
        (doc) => doc.note === newDoc.note
      );
      if (existingDocIndex !== -1) {
        data.value.attachFile[existingDocIndex] = newDoc;
      } else {
        data.value.attachFile.push(newDoc);
      }
      Array.from(event.target.files).forEach((file) =>
        formData.append("attachFile", file)
      );
      document.querySelectorAll(".click-file")[index].value = "";
    }
  };

  const confirmUpload = async () => {
    isLoading.value = true;
    formData.set("document", JSON.stringify(data.value));
    try {
      const res = await API.update(`PurchaseOrder/${props.data.id}`, formData);
      if (res.data) {
        FunctionGlobal.$notify(
          "S",
          t("body.systemSetting.success_label") || "Success",
          toast
        );
        formData.delete("document");
        formData.delete("attachFile");
        data.value = {
          id: 0,
          attachFile: [],
        };
      }
    } catch (error) {
      formData.delete("document");
      formData.delete("attachFile");
      switch (error.response?.status) {
        case 400:
          return FunctionGlobal.$notify(
            "E",
            error.response.data.errors.document[0],
            toast
          );
        case 500:
          return FunctionGlobal.$notify("E", error.response, toast);
        default:
          return FunctionGlobal.$notify(
            "E",
            t("body.report.error_occurred_message") || "An error occurred.",
            toast
          );
      }
    } finally {
      emits("callAPI", null);
      isLoading.value = false;
    }
  };
  const removeValueInFormData = () => {
    acctiveEdit.value = false;
    formData.delete("document");
    formData.delete("attachFile");
  };
  const removeItem = (event) => {
    event.status = "D";
    data.value.id = event.fatherId;
    const newDoc = {
      id: event.id,
      filename: event.fileName,
      note: event.note,
      memo: event.memo,
      fatherId: event.fatherId,
      status: event.status,
    };
    data.value.attachFile.push(newDoc);
  };
</script>
<style scoped>
  a {
    color: black;
    text-decoration: none;
  }
</style>
