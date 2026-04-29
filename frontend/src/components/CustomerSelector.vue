<template>
  <span v-show="!props.disabled">
    <InputGroup>
      <InputText
        v-if="props.selectionMode == 'multiple'"
        readonly
        :invalid="props.invalid"
        :disabled="props.disabled"
        class="cursor-pointer"
        :placeholder="props.placeholder"
        @click="onClickShow"
        :value="`${selection?.length ? selection?.length + ' khách hàng' : ''}`"
      />
      <AutoComplete
        v-else
        v-model="selection"
        :suggestions="data.customers"
        @complete="onSearchCustomers"
        :placeholder="props.placeholder"
        optionLabel="cardName"
        :completeOnFocus="true"
        @change="onAutoComplete"
        @item-select="onSelectItem"
        :disabled="props.disabled"
        :invalid="props.invalid"
        :style="{ width: props.width }"
        @clear="() => emits('clear')"
      >
        <template #option="{ option }">
          <div class="flex gap-2">
            <Avatar
              size="large"
              :label="option.cardName[0]"
              shape="circle"
              class="font-bold uppercase text-500"
            />
            <span>
              <div class="font-bold mb-1">{{ option.cardCode }}</div>
              <div class="text-overflow-ellipsis w-full overflow-hidden">
                {{ option.cardName }}
              </div>
            </span>
          </div>
        </template>
        <template #empty>
          <div class="p-5 text-center">{{ t("Custom.not_found") }}</div>
        </template>
        <template #chip="sp">
          {{ `${sp.value.cardName}`.slice(0, 20) + "..." }}
        </template>
        <template #content="sp">
          {{ `${sp.items.cardName}`.slice(0, 20) + "..." }}
        </template>
      </AutoComplete>
      <Button
        @click="onClearAll"
        class="border-300 border-left-none text-red-500"
        :class="{
          'surface-200 cursor-auto': props.disabled,
          'border-red-500': props.invalid,
        }"
        icon="pi pi-times"
        severity="secondary"
        outlined
        v-if="visibleClearButton"
      >
      </Button>
      <Button
        @click="onClickShow"
        class="border-300 border-left-none"
        :class="{
          'surface-200 cursor-auto': props.disabled,
          'border-red-500': props.invalid,
        }"
        icon="pi pi-list"
        severity="secondary"
        outlined
      >
      </Button>
    </InputGroup>
    <Dialog
      v-model:visible="visible"
      modal
      style="width: 70rem"
      :header="t('body.sampleRequest.customerGroup.customer_list_title')"
      :closable="true"
      @hide="onClickClose"
      :content-style="{
        height: '580px',
      }"
    >
      <!-- @row-select-all="isSelectAll = true"
                @row-unselect-all="isSelectAll = false" -->
      <!-- v-model:selection="tableSelection" -->
      <DataTable
        id="customeDt"
        v-model:selection="tbSelection"
        :value="data.customers"
        showGridlines
        :selectionMode="props.selectionMode"
        scrollable
        scrollHeight="500px"
      >
        <Column
          :pt="{
            headercontent: {
              class: 'hidden',
            },
          }"
          :selectionMode="props.selectionMode"
          class="w-1rem"
        >
        </Column>
        <!-- <Column class="w-1rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column> -->
        <Column
          :header="t('body.sampleRequest.customerGroup.customer_code_column')"
          class="w-10rem"
        >
          <template #body="{ data }">
            {{ data.cardCode }}
          </template>
        </Column>
        <Column :header="t('body.sampleRequest.customer.tax_code_label')">
          <template #body="{ data }">
            {{ data.licTradNum }}
          </template>
        </Column>
        <Column :header="t('body.sampleRequest.customerGroup.customer_name_column')">
          <template #body="{ data }">
            {{ data.cardName }}
          </template>
        </Column>
        <template #header>
          <div class="flex justify-content-end">
            <InputText
              @input="onSearch"
              v-model="page_query.search"
              :placeholder="t('body.productManagement.search_placeholder')"
            ></InputText>
          </div>
        </template>
        <template #empty>
          <div class="h-full">
            <div class="p-5 my-5 text-center">
              <template v-if="loading">{{ t("body.report.loading_message") }}</template>
              <template v-else>
                <template v-if="data.status == 'success'">
                  <template v-if="data.key">
                    {{ t("body.promotion.no_matching_result_message") }}
                    <span class="font-bold">"{{ data.key }}"</span>
                  </template>
                  <template v-else> {{ t("body.promotion.no_data_message") }} </template>
                </template>
                <template v-if="data.status == 'error'">
                  {{ t("body.report.error_occurred_message") }}
                </template>
              </template>
            </div>
          </div>
        </template>
      </DataTable>
      <template #footer>
        <div class="flex justify-content-end flex-grow-1">
          <div class="flex-grow-1 flex align-items-center">
            <template v-if="props.selectionMode == 'multiple' && vmodel?.length">
              <span
                >{{ t("body.productManagement.selected_label") }}
                {{ vmodel?.length }}</span
              >
              <Button @click="onClearAll" icon="pi pi-times" severity="danger" text />
            </template>
          </div>
          <div class="flex gap-2">
            <Button
              @click="onClickClose"
              icon="pi pi-times"
              :label="t('body.OrderList.close')"
              severity="secondary"
            />
            <Button
              @click="onClickSelect"
              icon="pi pi-check"
              :label="t('body.report.select_label')"
            />
          </div>
        </div>
      </template>
    </Dialog>
  </span>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted, nextTick, computed, watch } from "vue";
  import API from "../api/api-main";
  import { useToast } from "primevue/usetoast";
  import { useInfiniteScroll } from "@vueuse/core";
  import debounce from "lodash/debounce";
  import { cloneDeep, isArray } from "lodash";
  import { useI18n } from "vue-i18n";
  import { useRoute } from "vue-router";
  import { useMeStore } from "@/Pinia/me";
  const meData = useMeStore();
  const { t } = useI18n();
  const tbSelection = ref([]);

  const visibleClearButton = computed(() => {
    return (
      (props.selectionMode == "multiple" && selection.value?.length) ||
      (props.selectionMode == "single" && selection.value !== null)
    );
  });

  interface Props {
    selectionMode?: "single" | "multiple";
    disabled?: boolean;
    placeholder?: string;
    invalid?: boolean;
    poStore?: any;
    optionValue?: string;
    filted?: boolean;
    width: string | number;
  }
  const props = withDefaults(defineProps<Props>(), {
    selectionMode: () => "single",
    placeholder: () => "Chọn khách hàng",
    optionValue: () => "cardCode",
    disabled: () => false,
  });
  const emits = defineEmits(["item-select", "clear"]);
  const visible = ref(false);
  const toast = useToast();
  const selection = ref<null | any[] | any>(null);
  const vmodel = defineModel<any | any[]>();
  const route = useRoute();
  const data = reactive({
    customers: [] as any | any[],
    length: 0,
    status: "success",
    key: "",
  });
  const page_query = reactive({
    search: "",
    skip: 0,
    limit: 50,
  });

  const GetUser = async () => {
    try {
      const me = await meData.getMe();
      if (me.user.userType !== "APSP" && me.user.userName) {
        await searchAndSelectCustomerByName(me.user.userName);
      }
    } catch (error) {
      console.error("Error getting user data:", error);
    }
  };

  // Hàm tìm kiếm và tự động chọn khách hàng theo tên
  const searchAndSelectCustomerByName = async (cardCode: string) => {
    try {
      // Tìm kiếm khách hàng với tên tương ứng
      const searchQuery = {
        search: cardCode,
        skip: 0,
        limit: 50,
      };
      let path = `Customer${getQuery(searchQuery)}`;
      if (props.filted) path += "&filter=(status=A)";
      const response = await API.get(path);
      const customers = response.data?.items || [];
      // Tìm khách hàng có tên khớp chính xác hoặc gần nhất
      const exactMatch = customers.find(
        (customer: any) => customer.cardCode?.toLowerCase() === cardCode.toLowerCase()
      );
      const partialMatch = customers.find(
        (customer: any) =>
          customer.cardCode?.toLowerCase().includes(cardCode.toLowerCase()) ||
          cardCode.toLowerCase().includes(customer.cardCode?.toLowerCase())
      );
      const selectedCustomer = exactMatch || partialMatch;
      if (selectedCustomer) {
        selection.value = selectedCustomer;
        if (props.optionValue) {
          vmodel.value = selectedCustomer[props.optionValue];
        } else {
          vmodel.value = selectedCustomer.id;
        }
        emits("item-select", selectedCustomer);
      }
    } catch (error) {
      console.error("Error searching customer by name:", error);
    }
  };

  const onClearAll = () => {
    selection.value = null;
    vmodel.value = [];
    tbSelection.value = [];
    emits("clear");
  };

  const onClickSelect = () => {
    selection.value = cloneDeep(tbSelection.value);
    if (props.selectionMode == "single") {
      vmodel.value = props.optionValue
        ? tbSelection.value[props.optionValue as keyof typeof tbSelection.value]
        : cloneDeep(tbSelection.value);
    } else if (props.selectionMode == "multiple") {
      if (isArray(tbSelection.value)) {
        const xxx = tbSelection.value?.map((item: any) =>
          props.optionValue ? item[props.optionValue] : item
        );
        vmodel.value = cloneDeep(xxx);
      }
    }
    emits("item-select", tbSelection.value);
    visible.value = false;
  };

  const onSearch = debounce((event: any) => {
    stopFetch = false;
    page_query.skip = 0;
    data.length = 0;
    data.customers = [];
    data.key = "";
    onScrolling();
  }, 200);

  var stopFetch = false;
  const onScrolling = () => {
    // fetch data:
    if (stopFetch) return;
    if (loading.value) return;
    if (data.customers?.length && data.customers?.length >= data?.length) return;
    fetchCustomers(page_query, true);
    page_query.skip++;
  };

  const loading = ref(false);
  const dataTableElement = ref<HTMLElement | null>();
  useInfiniteScroll(
    dataTableElement,
    () => {
      onScrolling();
    },
    {
      distance: 1000,
    }
  );

  const onClickClose = () => {
    visible.value = false;
    page_query.skip = 0;
    stopFetch = false;
    data.length = 0;
    data.customers = [];
    data.status = "success";
    data.key = "";
    tbSelection.value = [];
  };

  const onClickShow = () => {
    if (props.disabled) return;
    visible.value = true;
    stopFetch = false;
    data.customers = [];
    page_query.search = "";
    page_query.skip = 0;
    // selection.value
    tbSelection.value = [];
    nextTick(() => {
      dataTableElement.value = document.querySelector(
        "#customeDt .p-datatable-wrapper"
      ) as HTMLElement;
    });
  };

  const onAutoComplete = () => {
    // if (!props.multiple && typeof selection.value == "string") {
    //     selection.value = null;
    // }
    // emitss("update:selection", selection.value);
    if (typeof selection.value == "string") {
      vmodel.value = null;
    }  
  };

  const onSelectItem = (event: any) => {
    if (props.optionValue) {
      vmodel.value = event.value[props.optionValue];
    } else {
      vmodel.value = event.value.id;
    } 
    emits("item-select", event.value);
    page_query.search = "";
  };

  const fetchCustomers = (query?: any, append: boolean = false) => {
    if (loading.value) return;
    loading.value = true;
    let path = `Customer`;
    if (query) path += getQuery(query);
    if (props.filted) path += "&filter=(status=A)";
    API.get(path)
      .then((res) => {
        let dataResponse = res.data?.items;
        if (dataResponse.length < 1) {
          stopFetch = true;
          data.key = page_query.search;
          return;
        }
        if (append) {
          data.customers = [...data.customers, ...dataResponse];
        } else {
          data.customers = dataResponse;
        }
        // if (isSelectAll.value) {
        //     // tableSelection.value = data.customers;
        // }
        data.length = res.data?.total || 0;
        data.status = "success";
      })
      .catch((error) => {
        toast.add({
          severity: "error",
          summary: t('common.error'),
          detail: t("Custom.fetch_failed"),
          life: 3000,
        });
        data.status = "error";
      })
      .finally(() => {
        loading.value = false;
      });
  };

  const getQuery = (query?: any): string => {
    if (!query) return "";
    let arrayQuery: Array<string> = [];
    for (let key in query) {
      if (!query[key]) continue; // skip empty values
      arrayQuery.push(`${key}=${query[key]}`);
    }
    return "?" + arrayQuery.join("&");
  };

  const onSearchCustomers = (event: any) => {
    page_query.search = event.query;
    page_query.skip = 0;
    fetchCustomers(page_query);
  };

  watch(
    () => props.poStore?.model?.cardCode,
    (newVal, oldVal) => {
      if (
        (route.name == "giftCopy" ||
          route.name == "orderNetCopy" ||
          route.name == "purchase-order-copy-admin" ||
          route.name == "purchase-order-copy") &&
        newVal != oldVal
      )
        searchAndSelectCustomerByName(newVal);
    },
    { deep: true }
  );

  onMounted(async () => {
    // Gọi GetUser để tự động tìm và chọn khách hàng
    await GetUser();

    if (vmodel.value && !selection.value) {
      API.get(`Customer/${vmodel.value}`)
        .then((res) => {
          selection.value = res.data;
        })
        .catch((error) => {
          console.error(error);
        });
    }
  });
</script>
