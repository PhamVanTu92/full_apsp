<template>
  <div>
    <h4 class="font-bold" v-if="props.type == 'preview'">
      {{ t("Evaluate.evaluateOrder") }}
    </h4>
    <div class="rating-section">
      <div class="rating-item">
        <span class="rating-label">{{ t("Evaluate.productQuality") }}</span>
        <Rating
          v-model="evaluate.qualityScore"
          :stars="5"
          :cancel="type === 'vote'"
          :disabled="isReadOnly"
        />
      </div>
      <div class="rating-item">
        <span class="rating-label">{{ t("Evaluate.sellerService") }}</span>
        <Rating
          v-model="evaluate.serviceScore"
          :stars="5"
          :cancel="type === 'vote'"
          :disabled="isReadOnly"
        />
      </div>
      <div class="rating-item">
        <span class="rating-label">{{ t("Evaluate.shipService") }}</span>
        <Rating
          v-model="evaluate.shipScore"
          :stars="5"
          :cancel="type === 'vote'"
          :disabled="isReadOnly"
        />
      </div>
    </div>
    <div>
      <h6>{{ t("Evaluate.reviewContent") }}</h6>
      <textarea
        v-model="evaluate.comment"
        class="review-textarea"
        :placeholder="t('Evaluate.shareExperience')"
        rows="2"
        maxlength="1000"
        :readonly="type === 'preview'"
      />
      <div class="character-count">
        {{ evaluate.comment.length }}/1000 {{ t("Evaluate.characters") }}
      </div>
    </div>
    <!-- <div class="pl-5" v-if="type === 'preview'">
            <h6>{{ t('Evaluate.feedbackContent') }}</h6>
            <textarea v-model="evaluate.comment" class="review-textarea" :placeholder="t('Evaluate.sendFeedback')"
                rows="2" maxlength="1000" :readonly="type === 'preview'" />
            <div class="character-count flex gap-2 justify-content-end  align-items-center">
                {{ evaluate.comment.length }}/1000 {{ t('Evaluate.characters') }} <Button label="Gửi"
                    icon="pi pi-send" />
            </div>
        </div> -->
    <div v-if="type === 'vote'" class="image-upload-section">
      <h6>{{ t("Evaluate.reviewImages") }} ({{ t("Evaluate.maxImages") }})</h6>
      <div class="image-upload-container">
        <div
          v-for="(image, index) in evaluate.images"
          :key="index"
          class="uploaded-image"
          @click="openImagePreview(index)"
        >
          <img :src="image.imageUrl" :alt="'Hình ảnh ' + (index + 1)" />
          <button @click.stop="removeImage(index)" class="remove-image-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div
          v-if="evaluate.images.length < 5"
          class="upload-button"
          @click="triggerFileInput"
        >
          <i class="fas fa-plus"></i>
          <span>{{ t("Evaluate.addImage") }}</span>
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
    <div
      v-else-if="type === 'preview' && evaluate.images.length > 0"
      class="image-preview-section"
    >
      <h6>{{ t("Evaluate.reviewImages") }}</h6>
      <div class="image-preview-grid flex gap-2">
        <div
          v-for="(image, index) in evaluate.images"
          :key="index"
          class="preview-image-item"
          @click="openImagePreview(index)"
        >
          <img
            :src="image?.imageUrl || ''"
            :alt="'Hình ảnh ' + (index + 1)"
            style="width: 100px; height: 100px"
          />
        </div>
      </div>
    </div>
    <div v-if="type === 'vote'" class="submit-section">
      <Button
        @click="submitReview"
        :label="t('Evaluate.submitReview')"
        class="submit-button"
        :disabled="!canSubmit || isSubmitting"
        :loading="isSubmitting"
      />
    </div>
    <div v-if="submitStatus" class="status-message" :class="submitStatus.type">
      <i :class="submitStatus.icon"></i>
      {{ submitStatus.message }}
    </div>
    <Dialog
      v-model:visible="imagePreview.visible"
      modal
      :style="{ width: '50vw' }"
      :breakpoints="{ '960px': '75vw', '641px': '100vw' }"
    >
      <template #header>
        <h6>{{ t("Evaluate.previewImage") }}</h6>
      </template>
      <div class="image-preview-container">
        <img :src="imagePreview.currentImage" alt="Preview" class="preview-image" />
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import Rating from "../components/Rating.vue";
import "../style/danhgiadon.css";
import API from "@/api/api-main";
import { Rating as RatingType } from "@/views/common/Order/types/orderDetail";
const { t } = useI18n();

const props = defineProps<{
  type?: "vote" | "preview";
  orderId?: number | string;
  docType?: string;
  initialData?: RatingType[];
}>();

const evaluate = ref({
  qualityScore: props.initialData?.[0]?.qualityScore ?? 5,
  serviceScore: props.initialData?.[0]?.serviceScore ?? 5,
  shipScore: props.initialData?.[0]?.shipScore ?? 5,
  comment: props.initialData?.[0]?.comment ?? "",
  createdAt: props.initialData?.[0]?.createdAt ?? "",
  images: (props.initialData?.[0]?.images ?? []) as Array<{
    imageUrl: string;
    file?: File;
  }>,
});

const fileInput = ref<HTMLInputElement>();
const isSubmitting = ref(false);
const submitStatus = ref<{
  type: "success" | "error";
  message: string;
  icon: string;
} | null>(null);

const imagePreview = ref({
  visible: false,
  currentImage: "",
  currentIndex: 0,
});

const canSubmit = computed(() => {
  return (
    evaluate.value.qualityScore > 0 &&
    evaluate.value.serviceScore > 0 &&
    evaluate.value.comment.trim().length > 0
  );
});

const isReadOnly = computed(() => {
  return props.type === "preview" || !!props.initialData;
});

const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleImageUpload = (event: Event) => {
  const target = event.target as HTMLInputElement;
  const files = target.files;
  if (!files) return;
  const remainingSlots = 5 - evaluate.value.images.length;
  const filesToProcess = Array.from(files).slice(0, remainingSlots);
  filesToProcess.forEach((file) => {
    if (file.type.startsWith("image/")) {
      const reader = new FileReader();
      reader.onload = (e) => {
        evaluate.value.images.push({
          imageUrl: e.target?.result as string,
          file: file,
        });
      };
      reader.readAsDataURL(file);
    }
  });
  target.value = "";
};

const removeImage = (index: number) => {
  evaluate.value.images.splice(index, 1);
};
const openImagePreview = (index: number) => {
  imagePreview.value.currentImage = evaluate.value.images[index].imageUrl;
  imagePreview.value.currentIndex = index;
  imagePreview.value.visible = true;
};
const closeImagePreview = () => {
  imagePreview.value.visible = false;
  imagePreview.value.currentImage = "";
  imagePreview.value.currentIndex = 0;
};
const submitReview = async () => {
  if (!canSubmit.value) return;
  isSubmitting.value = true;
  submitStatus.value = null;
  try {
    const formData = new FormData();
    formData.append("orderId", props.orderId as string);
    formData.append("qualityScore", evaluate.value.qualityScore.toString());
    formData.append("serviceScore", evaluate.value.serviceScore.toString());
    formData.append("shipScore", evaluate.value.shipScore.toString());
    formData.append("comment", evaluate.value.comment);

    evaluate.value.images.forEach((image) => {
      if (image.file) {
        formData.append("Images", image.file);
      }
    });

    type DocType = "VPKM" | "NET" | "YCLHG";
    const endpointMap: Record<DocType, string> = {
      VPKM: "PurchaseOrderVPKM/rating",
      NET: "PurchaseOrderNet/rating",
      YCLHG: "PurchaseRequest/rating",
    };

    const endpoint = endpointMap[props.docType as DocType] || "PurchaseOrder/rating";
    await API.add(endpoint, formData);

    submitStatus.value = {
      type: "success",
      message: t("Evaluate.successMessage"),
      icon: "fas fa-check-circle",
    };

    setTimeout(() => {
      window.location.reload();
    }, 3000);
  } catch (error) {
    submitStatus.value = {
      type: "error",
      message: t("Evaluate.errorMessage"),
      icon: "fas fa-exclamation-circle",
    };
  } finally {
    isSubmitting.value = false;
  }
};
</script>
