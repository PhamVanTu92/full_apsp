<template>
  <div class="flex gap-3 justify-content-between align-items-center mb-3">
    <div class="flex gap-3">
      <h3 class="font-bold m-0" style="line-height: 33px">{{ t("Feedback.title") }}</h3>
    </div>
  </div>
  <DataTable
    :value="promotions.data"
    :rows="query.limit"
    :totalRecords="promotions.count"
    :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
    :first="query.skip * query.limit"
    lazy
    paginator
    class="card p-2"
    showGridlines
    @page="onPage($event)"
    :pt="ptTable"
    filterDisplay="menu"
    v-model:filters="filterStore.filters"
  >
    <template #header>
      <div class="flex justify-content-between">
        <IconField iconPosition="left">
          <InputText
            :placeholder="t('body.OrderList.searchPlaceholder')"
            v-model="filterStore.filters['global'].value"
            @input="debouncer"
          />
          <InputIcon>
            <i class="pi pi-search" />
          </InputIcon>
        </IconField>
      </div>
    </template>
    <Column header="#">
      <template #body="slotProps">
        <div>
          {{ slotProps.index + 1 }}
        </div>
      </template>
    </Column>
    <Column
      :header="t('body.report.table_header_customer_code_1')"
      field="cardCode"
      class="w-10rem"
    />
    <Column :header="t('body.report.table_header_customer_name_2')" field="cardName" />
    <Column :header="t('Evaluate.title')">
      <template #body="{ data }">
        <div class="text-justify max-w-30rem">
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
    <Column :header="t('client.image')" field="images">
      <template #body="slotProps">
        <div class="flex gap-2">
          <img
            v-for="(image, index) in slotProps.data.images"
            :key="index"
            :src="image.imageUrl"
            alt=""
            class="w-4rem h-4rem rounded-md"
          />
        </div>
      </template>
    </Column>
    <Column :header="t('client.created_date')" field="createdAt" class="w-5rem">
      <template #body="slotProps">
        <span class="border-1 border-300 py-1">
          <span class="surface-300 px-3 p-1">{{
            format.DateTimePlusUTC(slotProps.data.createdAt).time
          }}</span>
          <span class="px-3 p-1">
            {{ format.DateTimePlusUTC(slotProps.data.createdAt).date }}
          </span>
        </span>
      </template>
    </Column>
    <template #empty>
      <div class="p-5 text-center">{{ t("body.systemSetting.no_data_to_display") }}</div>
    </template>
  </DataTable>
  <ConfirmDialog class="w-30rem"></ConfirmDialog>
  <Loading v-if="loading"></Loading>
</template>
<script setup>
import { ref, onBeforeMount, reactive, watch, inject } from "vue";
import API from "@/api/api-main";
import { useRouter, useRoute } from "vue-router";
import { debounce } from "lodash";
import { FilterStore } from "@/Pinia/Filter/FilterPromotionDataPay";
import { useI18n } from "vue-i18n";
import format from "@/helpers/format.helper";
const conditionHandler = inject("conditionHandler");
const filterStore = FilterStore();
const router = useRouter();
const route = useRoute();
const loading = ref(false);
const expandedComments = ref(new Set());

const promotions = reactive({
  data: [],
  count: 0,
});

const { t } = useI18n();
const query = reactive({
  skip: 0,
  limit: 10,
  search: null,
  status: null,
});

const onSearch = (reset) => {
  let filterQuery = [];
  if (reset) query.skip = 0;
  let search = query.search?.trim();
  if (search) {
    filterQuery.push(`&search=${encodeURIComponent(search)}`);
  }
  fetchAllPromotions(filterQuery.join("&"));
};

const debouncer = debounce(() => onSearch(), 1000);

const toggleCommentExpansion = (commentId) => {
  if (expandedComments.value.has(commentId)) {
    expandedComments.value.delete(commentId);
  } else {
    expandedComments.value.add(commentId);
  }
};
watch(
  () => [query.status],
  () => {
    debouncer();
  }
);

const onPage = async (event) => {
  query.skip = event.page;
  query.limit = event.rows;
  router.push({
    name: route.name,
    query: {
      page: query.skip + 1,
      size: query.limit,
    },
  });
};

watch(route, async () => {
  onSearch(false);
});

const fetchAllPromotions = async (filter) => {
  const filters = conditionHandler.getQuery(filterStore.filters);
  let url = `Rating?Page=${query.skip + 1}&PageSize=${query.limit}&orderBy=id desc`;
  if (filter) url += filter;
  if (filters) url += filters;
  loading.value = true;
  try {
    const response = await API.get(url);
    promotions.data = response.data.result;
    promotions.count = response.data.total;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const initialComponent = async () => {
  if (route.query) {
    let skip = parseInt(route.query.page);
    skip = skip && skip > 0 ? skip - 1 : 0;
    query.skip = skip;
    query.limit = parseInt(route.query.size) || 10;
  }
  await fetchAllPromotions();
};

onBeforeMount(async () => {
  await initialComponent();
});
const ptTable = {
  rowexpansioncell: {
    class: "surface-overlay",
  },
};
</script>
