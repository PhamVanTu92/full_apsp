<template>
  <div class="p-2 surface-100 h-full flex flex-column">
    <Dialog v-model:visible="visible" modal header="Chi tiết mã giảm giá" :style="{ width: '40rem' }">
      <div v-if="selectedItem">
        <span>{{ selectedItem.discountLabel }}</span>
        <ul>
          <li v-for="(subLabel, index) in selectedItem.discountSubLabel" :key="index">{{ subLabel }}</li>
        </ul>
        <div class="flex justify-content-between gap-2 pt-3">
          <Button class="w-full" @click="closeDialog" label="Đóng" severity="secondary" rounded/>
          <Button class="w-full" @click="() => copyToClipboard(selectedItem.discountCode)"
            :label="!copiedCodes[selectedItem.discountCode] ? 'Sao chép' : 'Đã sao chép'" rounded/>
        </div>
      </div>
    </Dialog>

    <div v-for="item in items" :key="item.discountCode" class="coupon-card bg-white border-round p-3 mt-2">
      <div class="flex justify-content-between">
        <span class="font-semibold text-blue-600">Mã: {{ item.discountCode }}</span>
        <span>HSD: {{ item.discountTime }}</span>
      </div>

      <div class="my-3">
        <label>{{ item.discountLabel }}</label>
      </div>
      <div class="w-full flex justify-content-between gap-3">
        <Button @click="() => showDetails(item)" class="w-full" label="Điều kiện" outlined rounded />
        <Button v-if="item.discountExpiration" class="w-full" label="Hết hạn" severity="secondary" rounded />
        <Button :disabled="getButtonStatus(item).disabled" v-else class="w-full" 
          @click="onSelectCoupon(item)" :severity="getButtonStatus(item).severity"
          :label="getButtonStatus(item).label" rounded />
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref } from 'vue';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import { useGlobal } from "@/services/useGlobal";
const { toast, FunctionGlobal } = useGlobal();
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const emit = defineEmits(['update:coupon', 'on-select'])

const props = defineProps({
  coupon: null,
  orderValue: 0,
})

const onSelectCoupon = (coupon) => {
  emit('update:coupon', coupon);
  emit('on-select', coupon);
  toast.add({ severity: 'success', summary: 'Thành công', detail: 'Áp dụng thành công', life: 3000 })
}
const getButtonStatus = (coupon) => {
  if (props.orderValue < coupon.discountCondition.minimumAmount) {
    return {
      label : 'Không đủ điều kiện',
      disabled: true,
      severity: 'secondary',
    }
  }
  else if (coupon.id === props.coupon?.id) {
    return {
      label : 'Đã áp dụng',
      disabled: true,
      severity: 'primary',
    }
  } else {
    return {
      label : 'Áp dụng',
      disabled: false,
      severity: 'primary',
    }
  }
}

const visible = ref(false);
const selectedItem = ref(null);
const copiedCodes = ref({});

const showDetails = (item) => {
  selectedItem.value = item;
  visible.value = true;
}

const closeDialog = () => {
  visible.value = false;
  if (selectedItem.value) {
    copiedCodes.value[selectedItem.value.discountCode] = false;
  }
}

const copyToClipboard = async (code) => {
  try {
    await navigator.clipboard.writeText(code);

    copiedCodes.value[code] = true;
    setTimeout(() => copiedCodes.value[code] = false, 2000);
  } catch (err) {
    alert('Lỗi sao chép: ' + err);
  }
}

// Danh sách các mã giảm giá
const items = ref([
  {
    id: 1,
    discountCode: 'EGA50THANG10',
    discountTime: '28/09/2024',
    discountLabel: 'Giảm 15% cho đơn hàng giá trị tối thiểu trên 5 triệu. Mã giảm tối đa 500K',
    discountExpiration: false,
    discountSubLabel: [
      'Đồng giá 2 triệu cho nhóm sản phẩm Set combo',
      'Tổng giá trị đơn hàng từ 5 triệu trở lên'
    ],
    discountCondition: {
      minimumAmount: 5000000,
    },
    discountValue: {
      discountPercent: 15,
      discountAmount: 500000,
    }
  },
  {
    id: 2,
    discountCode: 'EGA50THANG10123',
    discountTime: '28/09/2024',
    discountLabel: 'Chiếu khấu 2% cho đơn hàng giá trị tối thiểu 30 triệu.',
    discountExpiration: false,
    discountSubLabel: [
      'Tổng giá trị đơn hàng từ 30 triệu trở lên'
    ],
    discountCondition: {
      minimumAmount: 30000000,
    },
    discountValue: {
      discountPercent: 2,
      discountAmount: 0,
    }
  }
])
</script>
