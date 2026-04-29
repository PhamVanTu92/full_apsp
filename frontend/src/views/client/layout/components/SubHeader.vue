<template>
  <div class="flex lg:flex-row md:flex-row flex-column my-2 pr-0">
    <div v-for="item in subHeader" class="h-3rem flex align-items-center px-3 mx-2">
      <router-link :to="item.link">
        <i :class="item.icon"></i>
        <span class="mx-2"> {{ item.name }} </span>
      </router-link>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";

const subHeader = ref([
  {
    link: "#",
    icon: "pi pi-map-marker",
    name: "Tra cứu đơn hàng",
  },
  {
    link: "",
    icon: "pi pi-sync",
    name: "Yêu cầu đổi trả",
  },
  {
    link: "",
    icon: "pi pi-info-circle",
    name: "Phản hồi khiếu nại",
  },
  {
    link: "",
    icon: "pi pi-question-circle",
    name: "Hỗ trợ khách hàng",
  },
]);
</script>
