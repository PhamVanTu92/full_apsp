<template>
  <div class="progress-group my-5 ">
    <div class="wrapper">
      <div class="step" v-for="(item, index) in props.data" :key="index" :style="{ width: percentage + '%' }">
        <progress class="progress" :value="checkValue(index) ? setValue(item) : 0" :max="item.reduce"></progress>
        <div class="progress-circle">
          <i class="label fa-solid fa-gift text-primary text-sm"></i>
          <span class="text-sm label-bottom">{{ item.reduce }}</span>
          <!-- <div class="label-bottom">{{ numeral(item.value).format("0,0") }}</div> -->
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
.progress {
  display: block;
  width: 100%;
  height: 10px;
  position: relative;
  z-index: 5;
  padding-right: 8px;
  padding-top: 2px;
}

@media all and (min--moz-device-pixel-ratio: 0) and (min-resolution: 3e1dpcm) {
  .progress {
    height: 10px;
  }
}

.progress[value] {
  background-color: transparent;
  border: 0;
  appearance: none;
  border-radius: 0;
}

.progress[value]::-ms-fill {
  border: 0;
}

.progress[value]::-moz-progress-bar {
  margin-right: 8px;
}

.progress[value]::-webkit-progress-inner-element {
  background-color: #eee;
}

.progress[value]::-webkit-progress-bar {
  background-color: #eee;
}

.progress-circle {
  width: 20px;
  height: 20px;
  position: absolute;
  right: -2px;
  top: -4px;
  border-radius: 50%;
  background-color: #afac07 !important;
  z-index: 999;
}

.progress-circle:before {
  content: "";
  width: 11px;
  height: 11px;
  background: white;
  border-radius: 50%;
  display: block;
  transform: translate(-50%, -50%);
  position: absolute;
  left: 50%;
  top: 50%;
}

.progress-group {
  margin-top: 10px;
}

@media (max-width: 991px) {
  .progress-group {
    margin-left: -18px;
    margin-right: -18px;
    flex-basis: 100%;
    padding: 18px;
  }
}

@media (max-width: 768px) {
  .progress-group {
    padding: 18px 18px 0;
    margin-bottom: 12px;
  }
}

.progress-group .title {
  margin-bottom: 18px + 6px;
}

.progress-group .wrapper {
  background: white;
  border: 1px solid #eee;
  border-radius: 12px;
  height: 14px;
  display: flex;
  filter: drop-shadow(0 0 1px rgba(0, 0, 0, 0.3));
}

.progress-group .step {
  /* width: 20%; */
  position: relative;
}

.progress-group .step:after {
  content: "";
  height: 20px;
  width: 20px;
  border-radius: 50%;
  display: block;
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
}

.progress-group .step:first-of-type .progress {
  padding-left: 4px;
}

.progress-group .step:first-of-type .progress[value]::-moz-progress-bar {
  border-radius: 5px 0 0 5px;
}

.progress-group .step:first-of-type .progress[value]::-webkit-progress-value {
  border-radius: 5px 0 0 5px;
}

.progress-group .step:not(:first-of-type) .progress[value]::-moz-progress-bar,
.progress-group .step:not(:first-of-type) .progress[value]::-webkit-progress-value {
  border-radius: 0;
}

.label {
  position: absolute;
  border-radius: 100%;
  top: -20px;
  /* left: -5px; */
  width: auto;
}

.label-bottom {
  position: absolute;
  top: 30px;
  right: 0px;
  width: auto;
}

.page-title {
  letter-spacing: -0.05rem;
}
</style>
<script setup>
import { ref, onBeforeMount } from "vue";
import numeral from "numeral";

const props = defineProps({
  data: Object,
  totalValue: Number,
});
const percentage = ref(0);
onBeforeMount(() => {
  percentage.value = 100 / props.data.length;
});

const setValue = (data) => {
  if (props.totalValue >= data.value) {
    data.checkValue = true;
    return data.value;
  }
  if (props.totalValue < data.value) {
    data.checkValue = false;
    return props.totalValue;
  }
};

const checkValue = (index) => {
  if (index < 1) return true;
  const i = index - 1;
  if (props.totalValue < props.data[i].value) return false;
  if (props.totalValue >= props.data[i].value) return true;
};
</script>
