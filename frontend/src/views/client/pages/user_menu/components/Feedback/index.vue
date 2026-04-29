<template>
  <div>
    <div class="flex justify-content-between align-items-center mb-3">
      <div class="font-bold text-2xl">{{ t("Feedback.title") }}</div>
      <Button :label="t('Feedback.feedback')" icon="pi pi-plus" @click="onClickOpenAdd" />
    </div>
    <div class="card">
      <DataTable
        :value="ratings"
        showGridlines
        stripedRows
        paginator
        :rows="rows"
        :totalRecords="totalRecords"
        :rowsPerPageOptions="[5, 10, 20, 50]"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :currentPageReportTemplate="`${t(
          'body.productManagement.display'
        )} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t(
          'Feedback.feedback'
        )}`"
        @page="onPage($event)"
      >
        <Column header="#" class="w-3rem">
          <template #body="sp">
            {{ sp.index + 1 + page * rows }}
          </template>
        </Column>
        <Column :header="t('Evaluate.title')">
          <template #body="{ data }">
            <div v-if="data.comment" class="text-justify">
              <span v-if="data.comment.length <= 200 || expandedComments.has(data.id)">
                {{ data.comment }}
              </span>
              <span v-else> {{ data.comment.substring(0, 200) }}... </span>
              <i
                @click="toggleCommentExpansion(data.id)"
                class="text-green-500"
                v-if="data.comment.length > 200"
              >
                {{
                  expandedComments.has(data.id)
                    ? t("Feedback.show_less")
                    : t("Feedback.read_more")
                }}
              </i>
            </div>
          </template>
        </Column>
        <Column field="reviewImages" :header="t('Feedback.reviewImages')">
          <template #body="{ data }">
            <div v-if="data.images && data.images.length > 0" class="flex">
              <img
                v-for="(image, index) in data.images"
                :key="index"
                :src="image.imageUrl"
                alt="Hình ảnh"
                class="w-4rem h-4rem object-cover border-round mr-2 cursor-pointer"
                @click="openImagePreview(image)"
              />
            </div>
          </template>
        </Column>
        <Column field="createdAt" style="width: 5rem" :header="t('client.created_date')">
          <template #body="{ data }">
            <span class="border-1 border-300 py-1">
              <span class="surface-300 px-3 p-1">{{
                format.DateTimePlusUTC(data.createdAt).time
              }}</span>
              <span class="px-3 p-1">
                {{ format.DateTimePlusUTC(data.createdAt).date }}
              </span>
            </span>
          </template>
        </Column>
        <template #empty>
          <div class="my-5 py-5 text-center font-italic text-gray-500">
            {{ t("client.no_data") }}
          </div>
        </template>
      </DataTable>
    </div>
    <FeedbackDialog
      v-model:visible="visible"
      :model="model"
      :loading="loading"
      @saveSuccess="fetchRatings"
      @save="fetchRatings"
    />

    <Dialog
      v-model:visible="imagePreview.visible" 
      modal
      :style="{ width: '50vw' }"
      :breakpoints="{ '960px': '75vw', '641px': '100vw' }"
    >
      <template #header>
        <h6>{{ t("Evaluate.previewImage") }}</h6>
      </template>
      <div class="image-preview-container flex justify-content-center">
        <img
          :src="imagePreview.currentImage.imageUrl"
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
  </div>
</template>

<script setup>
  import { ref, onBeforeMount } from "vue";
  import API from "@/api/api-main";
  import { useToast } from "primevue/usetoast";
  import format from "@/helpers/format.helper";
  import { useI18n } from "vue-i18n";
  import FeedbackDialog from "./dialog.vue";
  import { cloneDeep } from "lodash";

  const { t } = useI18n();
  const toast = useToast();
  const loading = ref(false);
  const ratings = ref([]);
  const visible = ref(false);

  const page = ref(0);
  const rows = ref(10);
  const totalRecords = ref(0);

  const defaultModel = {
    id: 0,
    userId: null,
    userName: "",
    comment: "",
    createdDate: null,
    images: [],
  };
  const model = ref(cloneDeep(defaultModel));

  const imagePreview = ref({
    visible: false,
    currentImage: "",
  });

  const expandedComments = ref(new Set());

  const openImagePreview = (imageUrl) => {
    imagePreview.value.currentImage = imageUrl;
    imagePreview.value.visible = true;
  };

  const closeImagePreview = () => {
    imagePreview.value.visible = false;
    imagePreview.value.currentImage = "";
  };

  // New function to toggle comment expansion
  const toggleCommentExpansion = (commentId) => {
    if (expandedComments.value.has(commentId)) {
      expandedComments.value.delete(commentId);
    } else {
      expandedComments.value.add(commentId);
    }
  };

  const fetchRatings = async () => {
    loading.value = true;
    try {
      let param = {
        Page: page.value + 1,
        PageSize: rows.value,
        OrderBy: "id asc",
      };
      const response = await API.get(
        `Rating?Page=${param.Page}&PageSize=${param.PageSize}&OrderBy=${param.OrderBy}`
      );
      ratings.value = response.data.result;
      totalRecords.value = response.data.total; // Giả sử API trả về tổng số bản ghi
    } catch (error) {
      toast.add({
        severity: "error",
        summary: t("body.report.error_occurred_message"),
        detail: t("Feedback.errorLoadingReviews"),
        life: 3000,
      });
      console.error("Lỗi khi tải đánh giá:", error);
    } finally {
      loading.value = false;
    }
  };

  const onPage = (event) => {
    page.value = event.page;
    rows.value = event.rows;
    fetchRatings();
  };

  const onClickOpenAdd = () => {
    model.value = cloneDeep(defaultModel);
    visible.value = true;
  };

  onBeforeMount(() => {
    fetchRatings();
  });
</script>

<style scoped>
  /* Bạn có thể thêm các style tùy chỉnh tại đây */
</style>
