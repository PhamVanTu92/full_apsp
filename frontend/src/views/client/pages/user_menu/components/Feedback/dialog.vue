<template>
  <Dialog
    :visible="visible"
    @update:visible="$emit('update:visible', $event)"
    :header="t('Feedback.feedback')"
    modal
    :style="{ width: '50vw' }"
    @hide="onClose"
  >
    <div class="flex flex-column gap-3">
      <div class="field">
        <label for="comment" class="font-bold">{{ t("Evaluate.feedbackContent") }}</label>
        <Textarea
          v-model="internalModel.comment"
          :placeholder="t('Feedback.commentTooLong')"
          class="w-full"
          maxLength="1000"
          rows="5"
        />
      </div>
      <div class="image-upload-section">
        <h6>
          {{ t("Evaluate.reviewImages") }} ({{ t("Evaluate.maxImages", { count: 5 }) }})
        </h6>
        <div class="image-upload-container flex flex-wrap gap-2">
          <div
            v-for="(image, index) in internalModel.images"
            :key="index"
            class="uploaded-image relative"
            @click="openImagePreview(index)"
          >
            <img
              :src="image.imageUrl"
              :alt="'Hình ảnh ' + (index + 1)"
              class="w-6rem h-6rem object-cover border-round"
            />
            <button
              @click.stop="removeImage(index)"
              class="remove-image-btn absolute top-0 right-0 -mt-2 -mr-2 bg-red-500 text-white border-circle w-1.5rem h-1.5rem flex align-items-center justify-content-center cursor-pointer"
            >
              <i class="pi pi-times"></i>
            </button>
          </div>
          <div
            v-if="internalModel.images && internalModel.images.length < 5"
            class="upload-button flex flex-column align-items-center justify-content-center border-2 border-dashed border-gray-300 border-round w-6rem h-6rem cursor-pointer"
            @click="triggerFileInput"
          >
            <i class="pi pi-plus text-2xl text-gray-400"></i>
            <span class="text-xs text-gray-500">{{ t("Evaluate.addImage") }}</span>
          </div>
          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            multiple
            @change="handleImageUpload"
            style="display: none"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <Button
        :label="t('Logout.cancel')"
        icon="pi pi-times"
        @click="onClose"
        severity="secondary"
      />
      <Button
        :loading="loading"
        :label="t('client.save')"
        icon="pi pi-save"
        @click="onClickSave"
      />
    </template>

    <Dialog
      v-model:visible="imagePreview.visible"
      modal
      :breakpoints="{ '960px': '75vw', '641px': '100vw' }"
    >
      <template #header>
        <h6>{{ t("Evaluate.previewImage") }}</h6>
      </template>
      <div class="image-preview-container flex justify-content-center">
        <img
          :src="imagePreview.currentImage"
          alt="Preview"
          class="max-w-full max-h-30rem"
        />
      </div>
      <template #footer>
        <Button
          :label="t('Evaluate.close')"
          icon="pi pi-times"
          @click="closeImagePreview"
          class="p-button-text"
        />
      </template>
    </Dialog>
  </Dialog>
</template>

<script setup>
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { cloneDeep } from "lodash";
import API from "@/api/api-main";
import "./style.css";
const { t } = useI18n();
const toast = useToast();

const props = defineProps({
  visible: {
    type: Boolean,
    default: false,
  },
  model: {
    type: Object,
    default: () => ({
      id: 0,
      comment: "",
      createdDate: null,
      images: [],
    }),
  },
  loading: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["update:visible", "save"]);

const internalModel = ref(cloneDeep(props.model));

watch(
  () => props.model,
  (newVal) => {
    internalModel.value = cloneDeep(newVal);
  },
  { deep: true }
);

const fileInput = ref(null);

const imagePreview = ref({
  visible: false,
  currentImage: "",
  currentIndex: 0,
});

const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleImageUpload = (event) => {
  const target = event.target;
  const files = target.files;
  if (!files) return;

  const remainingSlots = 5 - internalModel.value.images.length;
  const filesToProcess = Array.from(files).slice(0, remainingSlots);

  filesToProcess.forEach((file) => {
    if (file.type.startsWith("image/")) {
      const reader = new FileReader();
      reader.onload = (e) => {
        internalModel.value.images.push({
          imageUrl: e.target?.result,
          file: file,
        });
      };
      reader.readAsDataURL(file);
    }
  });
  target.value = "";
};

const removeImage = (index) => {
  internalModel.value.images.splice(index, 1);
};

const openImagePreview = (index) => {
  imagePreview.value.currentImage = internalModel.value.images[index].imageUrl;
  imagePreview.value.currentIndex = index;
  imagePreview.value.visible = true;
};

const closeImagePreview = () => {
  imagePreview.value.visible = false;
  imagePreview.value.currentImage = "";
  imagePreview.value.currentIndex = 0;
};

const onClickSave = () => {
  if (!internalModel.value.comment) {
    toast.add({
      severity: "warn",
      summary: t("Evaluate.warning"),
      detail: t("Evaluate.fill_all_fields"),
      life: 3000,
    });
    return;
  } else if (
    internalModel.value.images.length > 5 ||
    internalModel.value.images.length < 1
  ) {
    toast.add({
      severity: "warn",
      summary: t("Evaluate.warning"),
      detail: t("Evaluate.maxImages"),
      life: 3000,
    });
    return;
  }
  const formData = new FormData();
  if (internalModel.value.id) {
    formData.append("id", internalModel.value.id);
  }
  formData.append("comment", internalModel.value.comment);
  internalModel.value.images.forEach((image) => {
    if (image.file) {
      formData.append("Images", image.file);
    }
  });

  API.add("Rating", formData)
    .then((response) => {
      toast.add({
        severity: "success",
        summary: t("body.systemSetting.success_label"),
        detail: t("Evaluate.success_save"),
        life: 3000,
      });
      emit("saveSuccess", true);
      internalModel.value = cloneDeep(props.model); // Reset model after save
      onClose();
    })
    .catch((error) => {
      toast.add({
        severity: "error",
        summary: t("Custom.error"),
        detail: t("Feedback.submitError"),
        life: 3000,
      });
    });
};

const onClose = () => {
  props.visible = false;
  emit("update:visible", false);
};
</script>
