<template>
  <div class="w-full">
    <div v-for="(el, i) in props.data" :key="i" class="line-row flex gap-2">
      <div
        class="left-side flex flex-column align-items-center"
        :class="{ 'justify-content-start': i == props.data.length - 1 }"
      >
        <span v-if="i == 0" class="dot-active"> </span>
        <span v-else class="dot"></span>
        <div v-if="i < props.data.length - 1" class="line"></div>
      </div>
      <div :class="el.time ? 'title-side p-3 pt-0' : ''">
        {{ el.time }}
      </div>
      <div class="detail-side flex-grow-1 p-3 pt-0">
        <div class="font-bold mb-2 text-lg">{{ el.body.title }}</div>
        <div class="">{{ el.body.description }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  data: {
    type: Array,
    default: {},
  },
});
</script>

<style scoped>
.dot-active::after {
  content: "";
  background: green;
  border-radius: 50%;
  width: 14px;
  height: 14px;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: load 1.5s ease-out infinite;
}

.dot-active {
  position: relative;
  border: 2px solid green;
  border-radius: 50%;
  z-index: 1000;
  width: 24px !important;
  min-height: 24px !important;
  background: white;
}

@keyframes load {
  0% {
    position: absolute;
    border: 0px solid #fff;
  }

  50% {
    border: 5px solid rgba(140, 194, 140, 0.103);
  }

  100% {
    border: 0px solid rgba(214, 231, 214, 0.589);
  }
}

.line-end {
  border-left: 4px solid #7c7878;
}

.line-row {
  min-height: 3rem;
}

.line {
  height: 100%;
  width: 3px;
  background: #7c7878;
  margin-top: -1px;
  margin-bottom: -1px;
}

.left-side {
  min-width: 30px;
  justify-content: center;
}

.dot {
  z-index: 1000;
  background-color: var(--surface-400);
  border-radius: 50%;
  width: 16px;
  height: 16px;
  display: block;
}
</style>
