<template>
  <div class="">
    <div class="grid">
      <div class="lg:col-3 md:col-12 col-12 relative">
        <MenuUser :items="items" />
      </div>
      <div class="lg:col-9 md:col-12 col-12">
        <router-view />
      </div>
    </div>
  </div>
</template>

<script setup>
  import { computed } from "vue";
  import MenuUser from "./components/menuUser.vue";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  const items = computed(() => [
    {
      label: t("body.sampleRequest.customerGroup.homepage_label"),
      icon: "fa-solid fa-home",
      route: "/client",
    },
    {
      label: t("body.home.overview"),
      icon: "fa-solid fa-bars-progress",
      route: "/client/setup/boardSetup",
    },
    {
      label: t("client.orderHistory"),
      icon: "fa-solid fa-clock-rotate-left",
      items: [
        {
          label: t("Navbar.menu.orders"),
          icon: "fa-solid fa-file-lines",
          route: "/client/setup/hisPur",
        },
        {
          label: t("body.home.orderNET"),
          icon: "fa-solid fa-file-lines",
          route: "/client/setup/hisPurNET",
        },
        {
          label: t("Navbar.menu.request_exchange_promotional"),
          icon: "fa-solid fa-list-alt",
          route: { name: "promotiondetail-list" },
        },
        {
          label: t("Navbar.menu.returnRequest"),
          icon: "fa-solid fa-file-lines",
          route: "/client/setup/return-request",
        },
      ],
    },

    {
      label: t("client.warehouse"),
      icon: "fa-solid fa-warehouse",
      items: [
        {
          label: t("Navbar.menu.pickupRequest"),
          icon: "fa-solid fa-truck-ramp-box",
          route: "/client/setup/purchase-request-list",
        },
        {
          label: t("client.shipping_list"),
          icon: "fa-solid fa-clipboard-list",
          route: "/client/setup/sent-warehouse",
        },
        {
          label: t("Navbar.menu.warehouseFeeRecord"),
          icon: "fa-solid fa-file-invoice",
          route: "/client/setup/inventory-charge",
        },
        {
          label: t("client.confirmation_minutes"),
          icon: "fa-solid fa-file-signature",
          route: "/client/setup/confirmation-minutes",
        },
      ],
    },
    {
      label: t("client.qualityCommitment"),
      icon: "fa-solid fa-chart-column",
      route: "/client/setup/production-commitment",
    },

    {
      label: t("client.importPlan"),
      icon: "fa-regular fa-calendar-days",
      route: "/client/setup/purchase-plan",
    },
    {
      label: t("body.systemSetting.document"),
      icon: "fa-solid fa-folder-open",
      route: {
        name: "user-document",
      },
    }, 
    {
      label: t("client.account_settings"),
      icon: "fa-solid fa-gear",
      route: "/client/setup/user",
    },
    {
      label: t("Feedback.feedback"),
      icon: "fa-solid fa-comment-dots",
      route: "/client/setup/feedback",
    },
    {
        label: t('client.report'),
        icon: 'fa-solid fa-file-invoice',
        route: '/client/report'
    }
  ]);
</script>
