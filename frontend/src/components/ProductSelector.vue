<template>
  <div class="flex gap-2">
    <Button
      :disabled="checkDisabled"
      :class="props.btnClass"
      :icon="props.icon"
      :label="props.label"
      :severity="props.severity"
      :outlined="props.outlined"
      :text="props.text"
      @click="onClickOpenPopup"
    />
    <!-- <Button icon="pi pi-download" label="Tải mẫu in" severity="info" outlined @click="onDownloadTemplate" />
        <Button icon="pi pi-upload" label="Nhập Excel" severity="success" outlined @click="onImportExcel" />
        <input ref="fileInput" type="file" accept=".xlsx,.xls" style="display: none" @change="onFileSelected" /> -->
  </div>

  <Dialog
    @hide="onClose"
    v-model:visible="visible"
    :header="t('body.sampleRequest.sampleProposal.choose_product_button')"
    modal
    :class="props.class"
    maximizable
    @maximize="onMaximize"
    @unmaximize="onUnMaximize"
    @keydown.esc.stop.prevent
  >
    <DataTable
      id="product_data"
      v-model:selection="selected"
      :value="products.data"
      :selectionMode="props.selectionMode"
      showGridlines
      stripedRows
      scrollable
      lazy
      :scrollHeight="tableHeight"
      :loading="lazyLoading && false"
      @row-select="onRowSelect"
      @row-select-all="onSelectAll(true, $event)"
      @row-unselect-all="onSelectAll(false, $event)"
      class="mh-30rem"
    >
      <Column :selectionMode="props.selectionMode"></Column>
      <Column
        v-for="(col, i) in props.type == 'PurchaseRequest'
          ? defTablePurchaseReq
          : defaultTableColumns"
        :key="i"
        :field="col.field"
        :id="`td_${col.field}`"
        :pt="{
          bodycell: {
            id: `td_${col.field}`,
          },
        }"
        :header="col.header"
        :style="col.style"
        :class="col.class"
      >
        <template v-if="Object.hasOwn(col, 'command')" #body="sp">
          <template v-if="col.type == 'image'">
            <Image
              :src="col.command(sp) || 'https://placehold.co/60'"
              width="60px"
              height="60px"
              :preview="col.command(sp) ? true : false"
            />
          </template>
          <template v-else-if="col.type == 'inputNumber'">
            {{ col.command(sp) }}
            <InputNumber
              @input="onQuantityChange(sp.data, $event)"
              v-model="sp.data[col.field]"
              :pt="{ input: { root: { style: 'width: 6rem' } } }"
              inputId="horizontal-buttons"
              showButtons
              :max="sp.data.onHand - sp.data.onOrder || undefined"
              :min="0"
            >
            </InputNumber>
            <InputGroup>
              <InputGroupAddon v-if="0" class="p-0 flex flex-column overflow-hidden">
                <button class="ip-addon" @click="onClickIncrease(sp.data)">
                  <i class="pi pi-angle-up"></i>
                </button>
                <button class="ip-addon" @click="onClickDecrease(sp.data)">
                  <i class="pi pi-angle-down"></i>
                </button>
              </InputGroupAddon>
            </InputGroup>
          </template>
          <span v-else :class="col.itemClass" :style="col.itemStyle">{{
            col.command(sp)
          }}</span>
        </template>
      </Column>
      <Column
        v-for="(col, i) in props.cols"
        :key="i"
        :field="col.field"
        :header="col.header"
        :style="col.style"
        :class="col.class"
      >
        <template v-if="Object.hasOwn(col, 'command')" #body="sp">
          <template v-if="col.type == 'image'">
            <Image
              :src="col.command(sp) || 'https://placehold.co/60'"
              width="60px"
              height="60px"
              :preview="col.command(sp) ? true : false"
            />
          </template>

          <span v-else>{{ col.command(sp) }}</span>
        </template>
      </Column>
      <Column
        v-if="props.activeRowPoint == true"
        field="exchangePoint"
        :header="t('PromotionalItems.PointConversion.point1dv')"
        class="text-center w-12rem"
      >
      </Column>
      <slot name="cols-data"></slot>
      <template #header>
        <ProductFilter
          v-if="visible"
          @change="onChangeFilter"
          :clearable="true"
          :isFilterPro="isFilterPro"
          :cardId="props?.customer?.id"
          :andPointApi="props.andPointApi"
          :disabledHeader="props.disabledHeader"
        ></ProductFilter>
      </template>
      <template #empty>
        <div
          v-if="error == null"
          class="flex align-items-center justify-content-center h-30rem"
        >
          {{ emptyLabel }}
        </div>
        <div
          v-else-if="error"
          class="flex flex-column justify-content-center align-items-center gap-3 h-30rem"
        >
          <div>
            <div class="flex flex-column align-items-center gap-3 text-red-500">
              <img class="w-5rem" src="@/assets/svg/server_error.svg" alt="" />
              <div class="font-bold text-xl">
                {{ t("body.productManagement.load_error_message") }}
              </div>
            </div>
            <div class="w-25rem mt-3 border-1 border-300 p-3 border-round surface-0">
              <div class="grid m-0">
                <span class="col-3 p-0 font-bold mb-2">Error:</span>
                <span class="col-9 p-0 mb-2 font-italic">
                  {{ error?.status }}
                </span>
                <span class="col-3 p-0 font-bold">Message:</span>
                <span class="col-9 p-0 font-italic">
                  {{ error?.message }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </template>
    </DataTable>
    <template #footer>
      <div class="flex flex-wrap justify-content-between w-full">
        <div class="flex align-items-center lg:mb-0 lg:flex-grow-0 flex-grow-1">
          {{
            props.selectionMode == "multiple"
              ? t("body.productManagement.selected_label")
              : t("body.productManagement.total_label")
          }}
          <span class="font-semibold mx-1"
            >{{ props.selectionMode == "multiple" ? selected?.length + " /" : "" }}
            {{ products.total }}</span
          >
          {{ t("body.promotion.product_label") }}
          <Button
            @click="selected = []"
            v-if="selected?.length"
            :label="t('body.productManagement.clear_button')"
            severity="danger"
            text
          />
        </div>
        <div class="flex gap-2 lg:flex-grow-0 flex-grow-1">
          <Button
            :label="t('body.OrderApproval.cancel')"
            severity="secondary"
            @click="onClose"
            class="lg:flex-grow-0 flex-grow-1"
          />
          <Button
            @click="onClickConfirmProduct"
            :label="t('body.OrderApproval.confirm')"
            class="lg:flex-grow-0 flex-grow-1"
          />
        </div>
      </div>
    </template>
  </Dialog>
</template>

<script setup>
  import DataTable from "primevue/datatable";
  import { ref, reactive, computed } from "vue";
  import API from "@/api/api-main";
  import { useInfiniteScroll } from "@vueuse/core";
  import { useI18n } from "vue-i18n";
  import format from "@/helpers/format.helper";
  const { t } = useI18n();
  const emit = defineEmits(["confirm"]);

  const onSelectAll = (isSelect, event) => {
    if (isSelect)
      event.data?.forEach((item) => {
        if (isSelect) {
          if (!item.quantity) item.quantity = 1;
        } else {
          item.quantity = 0;
        }
      });
    else
      products.data.forEach((item) => {
        item.quantity = 0;
      });
  };

  const onRowSelect = (event) => {
    if (event.data.quantity == 0) event.data.quantity = 1;
  };

  const lazyLoading = ref(false);
  const products = reactive({
    data: [],
    total: 0,
  });

  const checkDisabled = computed(() => {
    return props.typeModal === "YCLHG" && typeof props.customer !== "object";
  });

  const visible = ref(false);
  const selected = ref([]);
  const emptyLabel = ref("");
  const props = defineProps({
    icon: {
      type: String,
      default: "pi pi-plus",
    },
    label: {
      type: String,
      default: null,
    },
    severity: {
      type: String,
      default: "primary",
    },
    outlined: {
      default: undefined,
    },
    text: {
      default: undefined,
    },
    selectionMode: {
      default: "multiple",
    },
    cols: {
      default: [],
    },
    class: {
      default: "md:w-8 sm:w-12 w-11",
    },
    selection: {
      type: Array,
      default: [],
    },
    type: {
      type: String,
      default: "",
    },
    btnClass: {
      type: String,
      default: "",
    },
    customer: {
      type: Object,
      default: null,
    },
    typeModal: {
      type: String,
      default: null,
    },
    isFilterPro: {
      type: Boolean,
      default: null,
    },
    andPointApi: {
      type: String,
      default: "item",
    },
    itemType: {
      type: String,
      default: "",
    },
    page: {
      type: String,
      default: null,
    },
    activeRowPoint: {
      type: Boolean,
      default: false,
    },
    disabledHeader: {
      type: Boolean,
      default: false,
    },
    disabledUser: {
      type: Boolean,
      default: false,
    },
  });

  const onChangeFilter = (_query) => {
    query.filter = _query;
    error.value = null;
    flag = true;
    fetchProduct(query, false);
    loadCount++;
  };

  var loadCount = 0;
  var scrollElement = ref();
  var flag = true;
  useInfiniteScroll(
    scrollElement,
    () => {
      if (!lazyLoading.value && loadCount > 0) {
        fetchProduct(query);
      }
    },
    { distance: 0 }
  );

  const defaultTableColumns = [
    {
      field: "index",
      header: "#",
      command: (sp) => {
        return sp.index + 1;
      },
    },
    {
      field: "image",
      header: t("client.image"),
      command: (sp) => {
        return sp.data.itM1?.[0]?.filePath;
      },
      type: "image",
      style: "text-align: center;",
    },
    { field: "itemCode", header: t("body.report.table_header_product_code_2") },
    {
      field: "itemName",
      header: t("body.report.table_header_product_name_3"),
      style: "min-width: 28rem;",
    },
    ...(props.andPointApi == "item"
      ? [
          {
            header: t("body.report.table_header_packaging_3"),
            // style: "width: 20rem",
            command: (sp) => {
              return sp.data.packing?.name;
            },
          },
          {
            field: "price",
            header: t("client.unitPrice"),
            command: (sp) => {
              return (
                format.FormatCurrency(sp.data.price || 0) + " " + (sp.data.currency || "")
              );
            },
            itemClass: "text-green-700 font-bold",
          },
          {
            header: t("body.sampleRequest.warehouseFee.table_header_tax"),
            command: (sp) => {
              return sp.data.taxGroups?.name;
            },
          },
        ]
      : []),
  ];
  const defTablePurchaseReq = [
    {
      field: "index",
      header: "#",
      command: (sp) => {
        return sp.index + 1;
      },
    },
    {
      field: "image",
      header: t("client.image"),
      command: (sp) => {
        return sp.data.itM1?.[0]?.filePath;
      },
      type: "image",
      style: "text-align: center;",
    },
    { field: "itemCode", header: t("body.report.table_header_product_code_2") },
    {
      field: "itemName",
      header: t("body.report.table_header_product_name_3"),
      style: "width: 20rem",
    },
    {
      header: t("body.report.table_header_packaging_3"),
      style: "width: 10rem",
      command: (sp) => {
        return sp.data.packing?.name;
      },
    },
    {
      header: t("Custom.pickupQuantity"),
      field: "quantity",
      type: "inputNumber",
      style: "width: 7rem",
      command: (sp) => {
        sp.data.quantity ??= 0;
      },
    },
    {
      field: "onHand",
      header: t("client.warehouse_stock"),
      style: "width: 10rem; text-align: right;",
    },
    {
      field: "onOrder",
      header: t("client.pending_approval"),
      style: "width: 10rem; text-align: right;",
    },
    {
      field: "onLimit",
      header: t("client.ordered_quantity"),
      style: "width: 10rem; text-align: right;",
      command: (sp) => {
        return sp.data.onHand - sp.data.onOrder;
      },
    },
  ];

  const onClickOpenPopup = () => {
    visible.value = true;
    selected.value = [];
    emptyLabel.value = t("Custom.loadingData");
    lazyLoading.value = false;
    flag = true;
    if (!props.isFilterPro) {
      loadCount++;
    }

    setTimeout(() => {
      scrollElement.value = document.querySelector(
        "#product_data > div.p-datatable-wrapper"
      );
    }, 10); // :D
  };

  const onClose = () => {
    visible.value = false;
    selected.value = [];
    products.data = [];
    products.total = 0;
    query.skip = 1;
    loadCount = 0;
    query.limit = 50;
  };

  const onClickConfirmProduct = () => {
    emit("confirm", selected.value);
    selected.value = [];
    visible.value = false;
  };
  const error = ref(null);
  const fetchProduct = (_query, append = true) => {
    if (!flag) return;
    lazyLoading.value = true;
    emptyLabel.value = t("Custom.loadingData");
    error.value = null;
    if (!append) _query.skip = 1;
    let url = `${props.andPointApi + (_query.filter ? _query.filter + "&" : "?")}Page=${
      _query.skip
    }&PageSize=${_query.limit}${props.itemType ? `&itemType=${props.itemType}` : ""}`;
    if (props.customer?.id && props.disabledUser !== true) {
      url += `&cardId=${props.customer.id}`;
    }
    if (props.typeModal === "YCLHG") {
      url += `&typeDoc=${props.typeModal}`;
    }
    API.get(url)
      .then((res) => {
        lazyLoading.value = false;
        let data = res.data.items || [];
        products.total = res.data.total || 0;
        if (append) products.data = products.data.concat(data);
        else products.data = data;
        if (products.data.length < res.data.total) _query.skip += 1;
        else {
          flag = false;
        }
        if (data.length < 1) {
          flag = false;
          emptyLabel.value = t("body.PurchaseRequestList.no_matching_product_message");
        }
      })
      .catch((error) => {
        console.error(error);
        lazyLoading.value = false;
        flag = false;
        error.value = {
          status: err.status,
          message: err.message,
        };
      });
  };

  const query = reactive({
    skip: 1,
    limit: 50,
    filter: null,
  });

  const tableHeight = ref("40rem");

  const onUnMaximize = () => {
    tableHeight.value = "40rem";
  };
  const onMaximize = () => {
    tableHeight.value = "75vh";
  };
  const onQuantityChange = (data, quantity) => {
    // const newQuantity = Math.max(0, Math.min(quantity.value, data.onHand - data.onOrder)); // Ensure quantity is within 0 and onHand
    // const productIndex = selected.value.findIndex((el) => el.id === data.id);
    // if (newQuantity === 0) {
    //     if (productIndex !== -1) {
    //         selected.value.splice(productIndex, 1);
    //     }
    // } else {
    //     if (productIndex === -1) {
    //         if (newQuantity > 0) {
    //             selected.value.push({ ...data, quantity: newQuantity }); // Add to selected if not present
    //         }
    //     } else {
    //         selected.value[productIndex].quantity = newQuantity;
    //     }
    // }
    // data.quantity = newQuantity; // Update data.quantity
  };

  const onClickIncrease = (data) => {
    data.quantity++;
  };

  const onClickDecrease = (data) => {
    if (data.quantity > 0) {
      data.quantity--;
    }
  };
</script>

<style scoped>
  .ip-addon {
    display: flex;
    justify-content: center;
    background-color: transparent;
    border: none;
    border-radius: 0;
    cursor: pointer;
    color: #95999e;
  }

  .ip-addon:hover {
    background-color: #f0f0f0;
    color: #6e7379;
  }

  .ip-addon:active {
    background-color: #e0e0e0;
  }

  .ip-addon:first-child {
    border-bottom: 1px solid #c8c8c8 !important;
  }
</style>

<style>
  .mh-30rem {
    min-height: 40rem;
  }
</style>
