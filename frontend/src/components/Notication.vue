<script setup>
  import { ref, onBeforeMount, computed, watch, onBeforeUnmount } from "vue";
  import { onClickOutside } from "@vueuse/core";
  import { formatRelative, format } from "date-fns";
  import { vi } from "date-fns/locale";
  import { useRouter } from "vue-router";
  import { useAuthStore } from "@/Pinia/auth";
  import { useI18n } from "vue-i18n";
  import API from "@/api/api-main";
  import DropdownLanguage from "./DropdownLanguage.vue";
  import formatNoti from "@/helpers/formatStatusNotification.helper";
  const props = defineProps({
    top: {
      type: String,
      default: "80px",
    },
  });

  const router = useRouter();
  const authStore = useAuthStore();
  const { t } = useI18n();
  const noti = ref(null);
  const menu = ref(null);
  const visible = ref(false);
  const visibleDetail = ref(false);
  const onlyUnread = ref(true);
  const notifications = ref([]);
  const notiSelector = ref(null);
  const userType = ref("");
  let wsConnection = null;
  const STORAGE_KEY = "CONFIG-NOTI-UNREADONLY";
  const NOTIFICATION_LIMIT = 30;

  const itemsMenu = computed(() => [
    {
      label: "Chi tiết",
      icon: "pi pi-info-circle",
      command: () => {
        visibleDetail.value = true;
      },
    },
    {
      label: "Đánh dấu là chưa đọc",
      icon: "fa-regular fa-circle-check",
      command: async () => {
        if (notiSelector.value) {
          notiSelector.value.isRead = false;
          await markAsUnread(notiSelector.value.id);
        }
      },
    },
  ]);
  const unreadCount = computed(() => notifications.value.filter((n) => !n.isRead).length);
  const filteredNotifications = computed(() =>
    onlyUnread.value ? notifications.value.filter((n) => !n.isRead) : notifications.value
  );
  const badgeText = computed(() => (unreadCount.value > 99 ? "99+" : unreadCount.value));
  const upperCaseFirstCharacter = (str) => {
    if (!str) return "";
    return str.charAt(0).toUpperCase() + str.slice(1);
  };

  const formatTimeAgo = (date) => {
    return upperCaseFirstCharacter(
      formatRelative(new Date(date), new Date(), { locale: vi })
    );
  };

  const ROUTE_MAP = {
    APSP: {
      order: (id) => ({ name: "order-detail", params: { id } }),
      order_request: (id) => ({ name: "DetailPurchaseRequest", params: { id } }),
      approval: (id) => ({ name: "orderApproval", params: { id } }),
      feebycustomer: (id) => ({ name: "warehouse-storage-fee", query: { id } }),
      sale_forecast: (id) => ({ name: "orderPlanningDetail", params: { id } }),
      commited: (id) => ({ name: "commited-outputing", query: { id } }),
      exchange: (id) => ({ name: "change_point_edit", params: { id } }),
      confirmation_minutes: (id) => ({
        name: "admin-confirmation-minutes-detail",
        params: { id },
      }),
    },
    NPP: {
      order: (id) => ({ name: "client-order-detail", params: { id } }),
      order_request: (id) => ({ name: "detail-purchase-request", params: { id } }),
      sale_forecast: (id) => ({ name: "purchase-plan-detail-client", params: { id } }),
      commited: (id) => ({ name: "production-commitment", query: { id } }),
      feebycustomer: (id) => ({ name: "inventory-charge", query: { id } }),
      debt_reconciliation: (id) => ({
        name: "debt-reconciliation-client",
        query: { id },
      }),
      exchange: (id) => ({ name: "production-commitment-detail", params: { id } }),
      confirmation_minutes: (id) => ({
        name: "client-confirmation-minutes-detail",
        params: { id },
      }),
    },
  };

  const getRouterUrl = (item) => {
    const objType = item.notification.object.objType;
    const objId = item.notification.object.objId;
    const routeMap = ROUTE_MAP[userType.value];

    if (!routeMap || !routeMap[objType]) {
      return "#";
    }

    return routeMap[objType](objId);
  };

  const fetchNotifications = async () => {
    try {
      const res = await API.get(`Notification?limit=${NOTIFICATION_LIMIT}&skip=0`);
      notifications.value = res.data.data || [];
    } catch (error) {
      console.error("Error fetching notifications:", error);
    }
  };

  const markAsRead = async (id) => {
    try {
      await API.add(`Notification/${id}/read`);
    } catch (error) {
      console.error("Error marking notification as read:", error);
    }
  };

  const markAsUnread = async (id) => {
    try {
      await API.add(`Notification/${id}/unread`);
    } catch (error) {
      console.error("Error marking notification as unread:", error);
    }
  };

  // ===== WebSocket =====
  const initWebSocket = () => {
    const envHost = import.meta.env.VITE_APP_WS_HOST;
    // Khi không có VITE_APP_WS_HOST (dev với Vite proxy), tự lấy từ window.location
    const host = envHost || `${window.location.protocol === 'https:' ? 'wss' : 'ws'}://${window.location.host}`;
    const token = authStore.token;
    if (!token) return;
    wsConnection = new WebSocket(`${host}/api/notifications?token=${token}`);
    wsConnection.onopen = () => {
      console.log("WebSocket connected");
      wsConnection.send("Hello Server!");
    };

    wsConnection.onmessage = async (event) => {
      try {
        if (event.data) {
          try {
            JSON.parse(event.data);
          } catch (parseError) {
            console.debug("WebSocket notification message is not JSON:", event.data);
          }
        }
        await fetchNotifications();
      } catch (error) {
        console.error("WebSocket message error:", error);
      }
    };

    wsConnection.onerror = (error) => {
      console.error("WebSocket error:", error);
    };

    wsConnection.onclose = () => {
      console.log("WebSocket disconnected");
    };
  };

  const closeWebSocket = () => {
    if (wsConnection) {
      wsConnection.close();
      wsConnection = null;
    }
  };

  // ===== Event Handlers =====
  const showNotification = async () => {
    visible.value = !visible.value;
    if (visible.value) {
      await fetchNotifications();
    }
  };

  const onClickNoti = async (item) => {
    const route = getRouterUrl(item);
    if (route !== "#") {
      router.push(route);
    }
    if (!item.isRead) {
      item.isRead = true;
      await markAsRead(item.id);
    }
    visible.value = false;
  };

  const toggleMenu = (event, item) => {
    menu.value.toggle(event);
    notiSelector.value = item;
  };

  const onChangeFilter = () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(onlyUnread.value));
  };

  // ===== Click Outside =====
  onClickOutside(noti, () => {
    visible.value = false;
  });

  // ===== Watch =====
  watch(visibleDetail, (newVal) => {
    if (!newVal) {
      visible.value = false;
    }
  });

  // ===== Lifecycle =====
  onBeforeMount(async () => {
    const config = localStorage.getItem(STORAGE_KEY);
    if (config) {
      onlyUnread.value = JSON.parse(config);
    }
    const user = JSON.parse(localStorage.getItem("user") || "{}");
    userType.value = user.appUser?.userType || "";
    await fetchNotifications();
    initWebSocket();
  });

  onBeforeUnmount(() => {
    closeWebSocket();
  });
</script>

<template>
  <div ref="noti" class="flex align-items-center mr-2">
    <!-- Notification Bell -->
    <div @click="showNotification" class="cursor-pointer">
      <i
        v-if="unreadCount"
        v-badge.danger="badgeText"
        class="fa-solid fa-bell text-white"
        style="font-size: 1.8rem"
      />
      <i v-else class="fa-solid fa-bell text-white" style="font-size: 1.8rem" />
    </div>

    <!-- Language Dropdown -->
    <div class="ml-3">
      <DropdownLanguage />
    </div>

    <!-- Notification Panel -->
    <div
      class="notification-panel shadow-5"
      :class="{ show: visible }"
      :style="{ top: props.top }"
    >
      <!-- Header -->
      <div
        class="p-3 border-bottom-1 border-300 flex justify-content-between align-items-center"
      >
        <span class="font-bold text-lg">
          Thông báo
          <span v-if="unreadCount">({{ badgeText }})</span>
        </span>
        <div class="flex align-items-center gap-2">
          <label for="unread-switch" class="text-sm select-none">
            Chỉ hiển thị chưa đọc
          </label>
          <InputSwitch id="unread-switch" v-model="onlyUnread" @change="onChangeFilter" />
        </div>
      </div>

      <!-- Notification List -->
      <div class="notification-list p-2">
        <template v-if="filteredNotifications.length > 0">
          <div
            v-for="item in filteredNotifications"
            :key="item.id"
            class="relative notification-item"
          >
            <div
              class="card p-2 mb-2 cursor-pointer flex hover:surface-200"
              :class="{ 'border-primary-200 bg-primary-50': !item.isRead }"
              @click="onClickNoti(item)"
            > 
              <span class="p-2">
                <i class="fa-solid fa-info-circle text-xl text-blue-500" />
              </span>

              <div v-if="!item.isRead" class="unread-dot" />

              <div class="flex flex-column justify-content-between p-1 flex-grow-1">
                <strong class="text-gray-700 mb-1">
                  {{ item.notification.title }}
                </strong>

                <div class="notification-message">
                  {{ item.notification.message }}
                </div>

                <div class="flex justify-content-between align-items-center mt-2">
                  <Tag
                    class="text-xs"
                    :severity="
                      formatNoti.formatStatus(item.notification.object.objType, t).class
                    "
                    :value="
                      formatNoti.formatStatus(item.notification.object.objType, t).label
                    "
                  />

                  <div class="text-gray-500 text-xs">
                    {{ formatTimeAgo(item.notification.createdAt) }}
                  </div>
                </div>
              </div>
            </div>
            <span class="more-options-btn">
              <Button
                severity="secondary"
                icon="fa-solid fa-ellipsis-vertical"
                class="p-button-rounded"
                @click="toggleMenu($event, item)"
              />
            </span>
          </div>
        </template>
        <template v-else>
          <div class="py-6 text-center">
            <i class="fa-regular fa-bell-slash text-5xl block mb-3" />
            <span>Không có thông báo nào!</span>
          </div>
        </template>
      </div>

      <!-- Footer -->
      <div class="notification-footer border-top-1 border-300 p-2 text-center"  @click="visible= false">
        <router-link to="/notifications" class="text-color hover:text-primary">
          Xem tất cả thông báo
        </router-link>
      </div>
    </div>
  </div>

  <!-- Context Menu -->
  <Menu ref="menu" :model="itemsMenu" :popup="true" />
  <Dialog
    v-if="notiSelector"
    v-model:visible="visibleDetail"
    modal
    dismissableMask
    :draggable="false"
    style="width: 35rem; right: 28.5rem; top: 5.6rem; position: absolute"
  >
    <template #header>
      <div class="flex flex-grow-1 gap-3 align-items-center">
        <div class="text-xl font-bold">
          {{ notiSelector.notification.title }}
        </div>
        <Tag v-if="!notiSelector.isRead">Chưa đọc</Tag>
      </div>
    </template>

    <div class="flex mb-3">
      <div class="font-semibold" style="min-width: 7rem">Nội dung:</div>
      <div>{{ notiSelector.notification.message }}</div>
    </div>

    <div class="flex align-items-center mb-3">
      <label class="font-semibold w-7rem">Thời gian:</label>
      <span>
        {{ format(new Date(notiSelector.notification.createdAt), "HH:mm dd/MM/yyyy") }}
      </span>
    </div>

    <div class="flex align-items-center">
      <label class="font-semibold w-7rem">Danh mục:</label>
      <Tag
        class="text-xs"
        :severity="
          formatNoti.formatStatus(notiSelector.notification.object.objType, t).class
        "
        :value="
          formatNoti.formatStatus(notiSelector.notification.object.objType, t).label
        "
      />
    </div>

    <template #footer>
      <Button icon="pi pi-trash" severity="danger" v-tooltip.top="'Xoá thông báo'" />
      <Button
        icon="pi pi-check-square"
        severity="secondary"
        v-tooltip.top="'Đánh dấu là chưa đọc'"
      />
      <Button
        icon="pi pi-arrow-circle-right"
        severity="info"
        v-tooltip.top="'Đi tới thông báo'"
      />
    </template>
  </Dialog>
</template>

<style scoped>
  @import url("../assets/styleNoti.css");
</style>
