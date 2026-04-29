<template>
  <div class="layout-menu-container">
    <!-- Component tìm kiếm -->
    <MenuSearch :menu-items="model" @search="handleSearch" @select="handleSelectItem" />

    <!-- Menu items -->
    <ul class="layout-menu">
      <template v-for="(item, i) in model" :key="item">
        <app-menu-item :item="item" :index="i" :search-query="searchQuery" />
      </template>
    </ul>
  </div>
</template>

<script setup>
  import { computed, ref } from "vue";
  import { useRouter } from "vue-router";
  import AppMenuItem from "./AppMenuItem.vue";
  import MenuSearch from "./MenuSearch.vue";
  import { useI18n } from "vue-i18n";

  const { t } = useI18n();
  const router = useRouter();
  const searchQuery = ref("");

  const model = computed(() => [
    {
      label: t("Navbar.menu.system"),
      items: [
        {
          label: t("Navbar.menu.overview"),
          icon: "fa-solid fa-house",
          to: { name: "dashboard" },
        },
        {
          label: t("Navbar.menu.transactionManagement"),
          icon: "fa-solid fa-receipt",
          items: [
            {
              label: t("Navbar.menu.orders"),
              icon: "fa-solid fa-file-lines",
              items: [
                {
                  label: t("Navbar.menu.orders"),
                  icon: "fa-solid fa-file-lines",
                  to: { name: "purchaseList" },
                },
                {
                  label: t("body.home.orderNET"),
                  icon: "fa-solid fa-globe",
                  to: { name: "orderNetList" },
                },
                {
                  label: t("body.home.orderPromotion"),
                  icon: "fa-solid fa-gift",
                  to: { name: "giftlist" },
                },
              ],
            },
            {
              label: t("Navbar.menu.pickupRequest"),
              icon: "fa-solid fa-inbox",
              to: { name: "PurchaseRequest" },
            },
            {
              label: t("Navbar.menu.request_exchange_promotional"),
              icon: "fa-solid fa-repeat",
              to: { name: "promotiondetail" },
            },
            {
              label: t("Navbar.menu.returnRequest"),
              icon: "fa-solid fa-arrow-rotate-left",
              to: { name: "returnRequestList" },
            },
            {
              label: t("Navbar.menu.approval"),
              icon: "fa-solid fa-clipboard-check",
              to: { name: "orderApproval" },
            },
            {
              label: t("Navbar.menu.sampleRequest"),
              icon: "fa-solid fa-box",
              to: { name: "requestSample" },
            },
          ],
        },
        {
          label: t("Navbar.menu.customerManagement"),
          icon: "fa-solid fa-users",
          items: [
            {
              label: t("Navbar.menu.customerCategory"),
              icon: "fa-solid fa-bookmark",
              to: { name: "agencyCategory" },
            },
            {
              label: t("Navbar.menu.customerGroup"),
              icon: "fa-solid fa-user-group",
              to: { name: "agencyGroup" },
            },
            {
              label: t("Navbar.menu.importPlan"),
              icon: "fa-solid fa-calendar-plus",
              to: { name: "orderPlanning" },
            },
            {
              label: t("Navbar.menu.outputCommitment"),
              icon: "fa-solid fa-list-check",
              to: { name: "commited-outputing" },
            },
            {
              label: t("Navbar.menu.warehouseManagement"),
              icon: "fa-solid fa-warehouse",
              items: [
                {
                  label: t("Navbar.menu.warehouseFeeRecord"),
                  icon: "fa-solid fa-file-invoice-dollar",
                  to: { name: "warehouse-storage-fee" },
                },
                {
                  label: t("Navbar.menu.confirmationMinutes"),
                  icon: "fa-solid fa-file-signature",
                  to: { name: "admin-confirmation-minutes" },
                },
                {
                  label: t("Navbar.menu.warehouseFeePricing"),
                  icon: "fa-solid fa-dollar-sign",
                  to: { name: "storage-fee-pricing-list" },
                },
              ],
            },
            {
              label: t("Navbar.menu.paymentRule"),
              icon: "fa-solid fa-credit-card",
              to: { name: "payment-rule" },
            },
          ],
        },
        {
          label: t("Navbar.menu.productManagement"),
          icon: "fa-solid fa-box-archive",
          items: [
            {
              label: t("Navbar.menu.productList"),
              icon: "fa-solid fa-boxes-stacked",
              to: { name: "ListProduct" },
            },
            {
              label: t("Navbar.menu.productListretail"),
              icon: "fa-solid fa-store",
              to: { name: "ListProductRetail" },
            },
            {
              label: t("Navbar.menu.productGroup"),
              icon: "fa-solid fa-layer-group",
              to: { name: "product-group" },
            },
            {
              label: t("Navbar.menu.productType"),
              icon: "fa-solid fa-tags",
              to: { name: "itemType" },
            },
            {
              label: t("Navbar.menu.packaging"),
              icon: "fa-solid fa-box-open",
              to: { name: "unitOfMeasurement" },
            },
            {
              label: t("Navbar.menu.uomGroup"),
              icon: "fa-solid fa-cubes",
              to: { name: "uomGroup" },
            },
          ],
        },
        {
          label: t("Navbar.menu.setting_vpkm"),
          icon: "fa-solid fa-ticket",
          items: [
            {
              label: t("Navbar.menu.setting_point_buy"),
              icon: "fa-solid fa-cogs",
              to: { name: "predeempromotionals-data" },
            },
            {
              label: t("Navbar.menu.setting_point_vpkm"),
              icon: "fa-solid fa-wrench",
              to: { name: "promotionalPoints" },
            },
          ],
        },
        {
          label: t("Navbar.menu.promotion"),
          icon: "fa-solid fa-percent",
          items: [
            {
              label: t("Navbar.menu.promotionProgram"),
              icon: "fa-solid fa-bullhorn",
              to: { name: "promotion-data" },
            },
            {
              label: t("Navbar.menu.voucher"),
              icon: "fa-solid fa-ticket",
              to: { name: "voucher" },
            },
            {
              label: t("Navbar.menu.coupon"),
              icon: "fa-solid fa-gift",
              to: { name: "Coupon" },
            },
          ],
        },
        {
          label: t("Navbar.menu.systemSetting"),
          icon: "fa-solid fa-gear",
          items: [
            {
              label: t("Navbar.menu.userManagement"),
              icon: "fa-solid fa-users-gear",
              to: { name: "Decentralization-user" },
            },
            {
              label: t("Navbar.menu.organizationStructure"),
              icon: "fa-solid fa-sitemap",
              to: { name: "organizational-structure" },
            },
            {
              label: t("Navbar.menu.roles"),
              icon: "fa-solid fa-id-card",
              to: { name: "roles" },
            },
            {
              label: t("Navbar.menu.approvalProcess"),
              icon: "fa-solid fa-tasks",
              items: [
                {
                  label: t("Navbar.menu.approvalLevel"),
                  icon: "fa-solid fa-user-check",
                  to: { name: "approvalLevel" },
                },
                {
                  label: t("Navbar.menu.approvalTemplate"),
                  icon: "fa-solid fa-file-circle-check",
                  to: { name: "approvalTemplate" },
                },
              ],
            },
            {
              label: t("Navbar.menu.terms"),
              icon: "fa-solid fa-feather-pointed",
              to: { name: "terms" },
            },
            {
              label: t("Navbar.menu.generalSetting"),
              icon: "fa-solid fa-sliders",
              to: { name: "settings" },
            },
            {
              label: "Zalo",
              icon: "fa-solid fa-mobile-screen-button",
              to: { name: "Zalo" },
            },
          ],
        }, 
        {
          label: t("Navbar.menu.report"),
          icon: "fa-solid fa-chart-column",
          to: { name: "report" },
        },
        {
          label: t("Navbar.menu.feedback"),
          icon: "fa-solid fa-comment-dots",
          to: { name: "feedback-admin" },
        },
        {
          label: t("Navbar.menu.notification"),
          icon: "fa-solid fa-bell",
          to: { name: "notifications" },
        },
      ],
    },
  ]);

  const handleSearch = (query) => {
    searchQuery.value = query;
  };

  const handleSelectItem = (item) => {
    // Điều hướng đến route nếu có
    if (item.to) {
      router.push(item.to);
    } else if (item.url) {
      window.open(item.url, item.target || "_self");
    }

    // Thực thi command nếu có
    if (item.command) {
      item.command({ item });
    }
  };
</script>

<style lang="scss" scoped>
  .layout-menu-container {
    display: flex;
    flex-direction: column;
    height: 100%;
  }

  .layout-menu {
    flex: 1;
    overflow-y: auto;
    list-style: none;
    margin: 0;
    padding: 0;
  }
</style>
