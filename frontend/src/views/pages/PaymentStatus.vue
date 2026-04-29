<template>
  <div class="p-5">
    <CheckSuccess v-if="status" />
    <div v-else class="text-center p-5">
      <img width="100px" :src="closeIcon"></img>
    </div>
    <div class="">
      <div class="text-center text-5xl mb-3 font-bold">
        {{
          status ? t("paymentPage.success")  : t("paymentPage.failure")
        }}
      </div>
    </div>
    <div class="text-center mb-3 text-xl text-500">
      {{ t("paymentPage.redirect_message_part1") }}
      <span class="font-bold">{{ coudownTimeSet }}</span> {{ t("paymentPage.redirect_message_part2") }}
    </div>
    <div class="flex justify-content-center">
      <div class="flex gap-3 w-30rem">
        <Button
          class="w-30rem border-2 border-noround text-orange-500 border-orange-500 hover:border-orange-500"
          text
          size="large"
          :label="t('paymentPage.home_page')"
          severity="warning"
          @click="routeToHomePage"
        />
        <Button
          class="w-30rem border-2 border-noround text-white bg-orange-500 border-orange-500 hover:bg-orange-400 hover:border-orange-400"
          text
          size="large"
          :label="t('paymentPage.order_details')"
          @click="routeToOrderDetail"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref,  onMounted ,onUnmounted} from "vue";
  import closeIcon from '@/assets/images/close.png'
  import { useRoute, useRouter } from "vue-router";
  import API from "@/api/api-main";
  import { useI18n } from "vue-i18n"; 
  const { t } = useI18n(); 
  const objChangePage = ref();
  const route = useRoute();
  const router = useRouter();
  const status = ref<boolean>();
  var coudownTimeSet = ref(30); 

const timeInterval = setInterval(() => {
    if (coudownTimeSet.value > 0) {
    coudownTimeSet.value--;
    } else {
    clearInterval(timeInterval);
    routeToHomePage();
    }
}, 1000);

  const getOrderId = (orderInfo: any) => {
    return orderInfo?.id || orderInfo?.docId || orderInfo?.docID || orderInfo?.DocId;
  };

  const getOrderDetailRouteName = (docType: any) => {
    const normalizedDocType = String(docType || "").toUpperCase();
    if (normalizedDocType === "NET") return "hisPurNET-detail";
    if (normalizedDocType === "VPKM" || normalizedDocType === "DVP") return "production-commitment-detail";
    return "client-order-detail";
  };

  const routeToOrderDetail = () => { 
    const orderId = getOrderId(objChangePage.value);
    if (orderId) {
      router.replace({
        name: getOrderDetailRouteName(objChangePage.value?.docType),
        params: {
          id: orderId,
        },
      });
    } else {
      routeToHomePage();
    }
  };
  const routeToHomePage = () => {
    router.replace({
      path: "/client",
    });
  };

  onMounted(function () {
    let queryGetStatus = "VnpayPayment?" + (route.fullPath.split("?")[1] || ""); 
    try {
      API.get(queryGetStatus).then((res) => { 
        objChangePage.value = res.data?.obj || null;
        if (res.data && res.data.status == 0) {
          status.value = true;
        } else {
          status.value = false;
        }
      });
    } catch (error) {
      status.value = false;
    }
  });

  onUnmounted(function () {
    clearInterval(timeInterval);
  });
</script>

<style scoped>
  .card-header {
    background-color: #fff;
    position: absolute;
    top: 0px;
    left: 10px;
    padding: 0 8px;
    transform: translateY(-50%);
    font-weight: 600;
    font-size: larger;
  }
</style>
