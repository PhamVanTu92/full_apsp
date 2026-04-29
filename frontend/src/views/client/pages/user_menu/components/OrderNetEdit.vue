<template>
  <div>
    <div class="flex justify-content-between mb-2 align-items-center">
      <h4 class="mb-0 font-bold">{{ t("client.detail") }}</h4>
      <div class="flex gap-2">
        <Button
          @click="openExportFile"
          :label="t('client.export_files')"
          icon="pi pi-file-export"
        />
        <ButtonGoBack v-if="!isClient" />
      </div>
    </div>

    <ExportFiles :data="isClient" ref="ExportFileRef" :type="'NET'" />

    <template v-if="odStore.error">
      <div class="flex justify-content-center align-items-center flex-column pt-8">
        <Image src="../../../../public/image/order/no-order.png" />
        <div class="font-bold text-2xl mb-2">{{ t("client.notFoundTitle") }}</div>
        <p style="max-width: 40rem; text-align: center">
          {{ t("client.notFoundMessage") }}
        </p>
      </div>
    </template>

    <template v-else>
      <CancelReason :isClient="isClient" />
      <Header />

      <div class="card">
        <div class="flex justify-content-end mb-4" v-if="odStore?.order?.status == 'DHT'">
          <Button
            icon="pi pi-history"
            :label="t('body.ReturnRequestList.FromOrder.buttonLabel')"
            @click="handleReturnRequest"
            class="px-4 py-2 bg-orange-500 border-none hover:bg-orange-600 transition-colors shadow-2"
            iconClass="mr-2"
          />
        </div>

        <ProductList />
        <div class="grid mt-3">
          <div class="col-12 md:col-8 flex flex-column gap-3">
            <Promotion />
            <Notes :is-client="isClient" />
          </div>
          <div class="col-12 md:col-4">
            <OrderSummary />
          </div>
        </div>
        <Buttons :isClient="isClient" />
      </div>

      <div class="card">
        <TaiLieuBoSung :is-client="isClient" />
      </div>
      <div class="card">
        <ChungTuGiaoHang :is-client="isClient" />
      </div>
      <div class="card mb-7">
        <ThongTin />
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";

// Components
import Header from "./componentsEditNET/Header.vue";
import OrderSummary from "./componentsEditNET/OrderSummary.vue";
import ProductList from "./componentsEditNET/ProductList.vue";
import Promotion from "./componentsEditNET/Promotion.vue";
import Notes from "./componentsEditNET/Notes.vue";
import Buttons from "./componentsEditNET/Buttons.vue";
import TaiLieuBoSung from "./componentsEditNET/TaiLieuBoSung.vue";
import ChungTuGiaoHang from "./componentsEditNET/ChungTuGiaoHang.vue";
import ThongTin from "./componentsEditNET/ThongTin.vue";
import ExportFiles from "./dialogs/ExportFiles.vue";
import CancelReason from "./componentsEditNET/CancelReason.vue";

import { useMeStore } from "@/Pinia/me";
import { useOrderDetailStore } from "../components/store/orderDetailNET";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const meData = useMeStore();
const odStore = useOrderDetailStore();
const ExportFileRef = ref();
const route = useRoute();
const router = useRouter();
const isClient = ref(false);

const initialComponent = async () => {
  const orderId = route.params.id;
  if (orderId && typeof orderId === "string" && isInteger(orderId)) {
    odStore.fetchStore(parseInt(orderId));
  } else {
    odStore.error = true;
  }

  const me = await meData.getMe();
  if (me.user?.userType == "NPP") {
    isClient.value = true;
  }
};

function isInteger(value: any) {
  return /^\d+$/.test(value);
}

onMounted(() => {
  initialComponent();
});

const openExportFile = () => {
  ExportFileRef.value?.open();
};

const handleReturnRequest = () => {
  const orderId = odStore.order?.id;
  if (!orderId) return;

  if (isClient.value) {
    router.push({
      name: "client-return-request-from-order",
      params: { id: orderId },
    });
  } else {
    router.push({
      name: "returnRequestFromOrder",
      params: { id: orderId },
    });
  }
};
</script>

<style scoped></style>
