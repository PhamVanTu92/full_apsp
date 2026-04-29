<script setup>
import { ref, computed } from "vue";
import API from "../api/api-main";
import { useRoute } from "vue-router";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const { toast, FunctionGlobal } = useGlobal();
const Route = useRoute();
const props = defineProps({
  endpoint: String,
  idCheck: String,
  disabled: Boolean,
});
const isLoading = ref(false);
const emits = defineEmits(["onUpdate"]);
const visible = ref(false);
const submited = ref(false);
const requireAdd = ref({
  id: Route.params.id,
  limitRequire: false,
  limitOverDue: false,
  other: false,
  memo: "",
  attachFile: [
    {
      filename: "",
      note: "",
      memo: "",
      status: "A",
    },
  ],
});
const clearRequireAdd = JSON.stringify(requireAdd.value);

const AddRow = () => {
  requireAdd.value.attachFile.push({
    filename: "",
    note: "",
    memo: "",
    status: "A",
  });
};
const remove = (idx) => {
  requireAdd.value.attachFile.splice(idx, 1);
};

const Submit = async () => {
  submited.value = true;
  if (!validateFormData()) return;
  let formData = new FormData();
  formData.append("document", JSON.stringify(requireAdd.value));
  try {
    isLoading.value = true;
    const res = await API.update(`${props.endpoint}`, formData);
    if (res.data) {
      FunctionGlobal.$notify("S", t("Custom.addnewSuccess"), toast);
      formData.delete("document");
      visible.value = false;
    }
  } catch (error) {
    console.error(error);
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    emits("onUpdate");
    isLoading.value = false;
  }
};
const openAddCauseDialog = () => {
  requireAdd.value = JSON.parse(clearRequireAdd);
  submited.value = false;
  visible.value = true;
};
const validateFormData = () => {
  if (
    !requireAdd.value.limitRequire &&
    !requireAdd.value.limitOverDue &&
    !requireAdd.value.other
  ) {
    FunctionGlobal.$notify("E", t("Custom.reason_required"), toast);
    return false;
  }
  if (requireAdd.value.other && !requireAdd.value.memo) {
    FunctionGlobal.$notify("E", t("Custom.other_reason_required"), toast);
    return false;
  }
  if (requireAdd.value.attachFile.some((el) => !el.note)) {
    FunctionGlobal.$notify("E", t("Custom.document_info_required"), toast);
    return false;
  }
  return true;
};
const checkCause = computed(() => {
  return !(
    requireAdd.value.limitRequire ||
    requireAdd.value.limitOverDue ||
    requireAdd.value.other
  );
});
</script>
<template>
  <Button
    v-if="!idCheck"
    :label="t('Custom.supplementRequest')"
    severity="help"
    :disabled="props.disabled"
    @click="openAddCauseDialog()"
  />
  <Dialog
    v-model:visible="visible"
    modal
    :header="t('Custom.supplementRequest')"
    class="w-5"
  >
    <span class="text-surface-500 dark:text-surface-400 block"
      >{{ t("Custom.request_reason") }} <sup class="text-red-500">*</sup></span
    >
    <div class="flex flex-column gap-3 py-4">
      <div class="flex items-center">
        <Checkbox
          :invalid="submited && checkCause"
          v-model="requireAdd.limitRequire"
          binary
        />
        <label class="ml-2"> {{ t("Custom.debt_limit_exceeded") }} </label>
      </div>
      <div class="flex items-center">
        <Checkbox
          :invalid="submited && checkCause"
          v-model="requireAdd.limitOverDue"
          binary
        />
        <label class="ml-2">{{ t("Custom.debt_overdue") }} </label>
      </div>
      <div class="flex items-center">
        <Checkbox v-model="requireAdd.other" :invalid="submited && checkCause" binary />
        <label class="ml-2"> {{ t("body.systemSetting.other") }} </label>
      </div>
      <div v-if="requireAdd.other">
        <Textarea
          v-model="requireAdd.memo"
          :disabled="!requireAdd.other"
          class="w-full"
          :placeholder="t('Custom.input_content')"
        ></Textarea>
      </div>
    </div>
    <div>
      <h6>{{ t("Custom.document_list") }}</h6>
      <DataTable :value="requireAdd.attachFile" stripedRows>
        <template #empty>
          <div class="text-center p-2">{{ t("Custom.document_list_empty") }}</div>
        </template>
        <Column header="STT" style="width: 3rem">
          <template #body="slotProps">
            <span>{{ slotProps.index + 1 }}</span>
          </template>
        </Column>
        <Column :header="t('body.sampleRequest.customer.document_name_column')">
          <template #body="slotProps">
            <InputText
              :placeholder="t('client.input_document')"
              v-model="slotProps.data.note"
              :invalid="submited && slotProps.data.note == ''"
            ></InputText>
          </template>
        </Column>
        <Column :header="t('client.note')">
          <template #body="slotProps">
            <InputText
              :placeholder="t('client.enter_note')"
              v-model="slotProps.data.memo"
            ></InputText>
          </template>
        </Column>
        <Column style="width: 5rem">
          <template #body="slotProps">
            <div class="flex gap-2">
              <!-- <Button icon="pi pi-pencil" text/> -->
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                @click="remove(slotProps.index)"
              />
            </div>
          </template>
        </Column>
      </DataTable>
      <div class="py-3">
        <Button
          :label="t('client.add_line')"
          icon="pi pi-plus"
          outlined
          @click="AddRow()"
        />
      </div>
    </div>
    <template #footer>
      <div class="flex justify-end gap-2">
        <Button
          type="button"
          :label="t('Logout.cancel')"
          severity="secondary"
          @click="visible = false"
        />
        <Button type="button" :label="t('Login.buttons.send')" @click="Submit" />
      </div>
    </template>
  </Dialog>
  <loading v-if="isLoading"></loading>
</template>
