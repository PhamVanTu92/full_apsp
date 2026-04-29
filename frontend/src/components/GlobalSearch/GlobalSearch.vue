<template>
  <div>
    <IconField iconPosition="left">
      <InputIcon class="pi pi-search"> </InputIcon>
      <InputText
        @click="onClickSearch"
        :placeholder="t('body.home.search_placeholder')"
        readonly
        class="cursor-pointer"
      />
    </IconField>
    <div class="layout_modal" v-if="visible">
      <div
        ref="searchArea"
        class="content_wrapper w-9 md:w-6"
        @mousedown="onMouseDown"
        @mouseup="onMouseUp"
        @mousemove="onMouseMove"
      >
        <div class="surface-50 p-3 border-1 border-300 border-round">
          <IconField iconPosition="right">
            <IconField iconPosition="left">
              <InputIcon
                :class="loading ? 'fa-solid fa-spinner fa-spin-pulse' : 'pi pi-search'"
              >
              </InputIcon>
              <InputText
                id="search_input"
                v-model="search"
                :placeholder="t('body.home.search_placeholder')"
                class="w-full py-2"
                style="line-height: 26px"
                autofocus
                @input="onInput"
              />
            </IconField>
            <InputIcon>
              <div style="margin-top: -4px !important">
                <button
                  style="font-size: 12px !important"
                  class="p-1 border-1 border-200 border-round select-none cursor-pointer text-500 text-base hover:bg-white hover:shadow-1"
                  @click="onClose"
                >
                  {{ t("client.back") }}
                </button>
              </div>
            </InputIcon>
          </IconField>
          <hr />
          <div class="h-30rem overflow-y-scroll gap-3 flex flex-column">
            <!-- <div v-if="searchResult" class="">
                            Kết quả tìm kiếm cho:
                            <span class="font-bold">"{{ searchResult }}"</span>
                        </div> -->
            <template v-for="(row, i) in searchResult" :key="i">
              <router-link :to="row.link" @click="onClose">
                <div class="p-3 card hover:bg-green-100 flex gap-3 mr-3">
                  <div>
                    <i v-if="row.icon" class="text-3xl" :class="row.icon"></i>
                    <i v-else class="fa-solid fa-minus text-3xl"></i>
                  </div>
                  <div class="flex-grow-1">
                    <div class="texdt-lg mb-2 font-bold">
                      {{ row.name }}
                    </div>
                    <div class="text-500">{{ row.desc }}</div>
                  </div>
                  <div>
                    <i class="fa-solid fa-arrow-up-right-from-square"></i>
                  </div>
                </div>
              </router-link>
            </template>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, nextTick } from "vue";
  import debounce from "lodash/debounce";
  import { onClickOutside } from "@vueuse/core";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  interface MenuItem {
    icon?: string;
    name: string;
    link: string;
    items?: MenuItem[];
    desc?: string;
  }

  const search = ref("");
  const visible = ref(false);
  const searchArea = ref();
  const isUserHoldMouse = ref(false);
  const searchResult = ref<MenuItem[]>([]);
  const loading = ref(false);

  const localDataL: Array<MenuItem> = [
    {
      icon: "fa-solid fa-chart-column",
      name: "Báo cáo",
      desc: t("Custom.report_types"),
      link: "/client/report",
      items: [
        {
          name: t("Custom.report_purchase_by_product"),
          link: "/client/report/buy-by-product",
        },
        {
          name: t("Custom.report_purchase_by_order"),
          link: "/client/report/purchase-by-order",
        },
        {
          name: t("Custom.report_import_plan"),
          link: "/client/report/purchase-plan",
        },
        {
          name: t("Custom.report_production_commitment"),
          link: "/client/report/commited",
        },
        {
          name: t("Custom.report_accounts_payable"),
          link: "/client/report/debt-payment",
        },
        {
          name: t("Custom.report_invoice_statistics"),
          link: "/client/report/bill-summary",
        },
        {
          name: t("Custom.report_consignment_inventory"),
          link: "/client/report/inventory",
        },
        {
          name: t("Custom.list_consignment_confirmation_records"),
          link: "/client/setup/confirmation-minutes",
        },
      ],
    },
    {
      icon: "fa-solid fa-cart-shopping",
      name: t("Custom.shopping_cart"),
      desc: t("Custom.view_cart_products"),
      link: "/client/cart",
    },
    {
      icon: "fa-solid fa-boxes-packing",
      name: t("Custom.purchase_history"),
      desc: t("Custom.view_purchase_history_description"),
      link: "/client/setup/hisPur",
    },
    {
      icon: "fa-solid fa-truck-ramp-box",
      name: t("Custom.consignment_pickup_request"),
      desc: t("Custom.consignment_pickup_request"),
      link: "/client/setup/boardSetup",
    },
  ];

  const onInput = debounce(() => {
    // Search on local
    searchResult.value = [];
    if (search.value === "") {
      searchResult.value = localDataL;
      return;
    }
    localDataL.forEach((item) => {
      if (item.name.toLowerCase().includes(search.value.toLowerCase())) {
        searchResult.value.push(item);
      }
      if (item.items) {
        item.items.forEach((subItem) => {
          if (subItem.name.toLowerCase().includes(search.value.toLowerCase())) {
            searchResult.value.push(subItem);
          }
        });
      }
    });
  }, 500);
  const onMouseMove = (event: MouseEvent) => {
    if (isUserHoldMouse.value) {
      // Drag the mouse to move the element
      // You can use event.clientX and event.clientY to get the mouse position
      // and update the position of the element accordingly
      // For example:
      // searchArea.value.style.position = "absolute";
      // searchArea.value.style.left = event.clientX - event.offsetX + "px";
      // searchArea.value.style.top = event.clientY - event.offsetY + "px";
      // Or you can use a library like interact.js for more advanced dragging
      // functionality.
      // Example of updating the position (uncomment to use):
    }
  };
  const onMouseUp = (event: MouseEvent) => {
    isUserHoldMouse.value = false;
  };
  const onMouseDown = (event: MouseEvent) => {
    isUserHoldMouse.value = true;
  };

  const onClickSearch = () => {
    visible.value = true;
    nextTick(() => {
      document.getElementById("search_input")?.focus();
      searchResult.value = localDataL;
    });
  };

  const onClose = () => {
    visible.value = false;
    search.value = "";
    searchResult.value = [];
  };

  onClickOutside(searchArea, () => {
    onClose();
  });

  const initialComponent = () => {
    // code here
    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        // Handle the Escape key event
        onClose();
      }
    });
  };

  onMounted(function () {
    initialComponent();
  });
</script>

<style scoped>
  #search_input {
    outline: none;
  }
  .content_wrapper {
    max-width: 820px;
  }
  .layout_modal {
    padding: 12vh;
    position: fixed;
    display: flex;
    flex-direction: row;
    justify-content: center;
    top: 0;
    left: 0;
    right: 0;
    height: 100vh;
    background-color: rgba(0, 0, 0, 0.4);
    backdrop-filter: blur(6px);
    z-index: 9999;
  }
</style>
