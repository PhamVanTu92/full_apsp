<script setup>
import { defineProps, ref } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const props = defineProps({
  label: null,
  icon: null,
  quantity: null,
});
const visibleRight = ref(false);
const visibleCond = ref(false);
</script>
<template>
  <button
    class="btn text-color-secondary border-dashed border-400 border-none bg-white hover:bg-green-50 hover:text-green-500 hover:border-green-500 border-bottom-1 px-2 py-3 cursor-pointer w-full flex justify-content-between"
    @click="visibleRight = true"
  >
    <span class="flex align-items-center">
      <i :class="props.icon"></i>
      <span class="ml-2">{{ props.label }}</span>
    </span>
    <span class="flex align-items-center">
      <span class="mr-2"
        >{{t('body.OrderList.change_button')}}
        <Badge
          v-if="props.quantity"
          :value="props.quantity"
          severity="danger"
          size="small"
        ></Badge
      ></span>
      <i class="pi pi-angle-right"></i>
    </span>
  </button>
  <Sidebar
    v-model:visible="visibleRight"
    :header="t('client.choose_discount_code')"
    position="right"
    style="width: 35rem"
  >
    <div class="card border-round-md bg-gray-100 flex flex-column gap-5">
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">{{t('client.code')}} ASPS50THANG10</strong>
          <span>{{t('client.expiry_date')}}: 28/01/2024</span>
        </div>
        <div>
          <p>{{t('client.discount_description')}}</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            :label="t('client.condition')"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button :label="t('client.apply')" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">{{t('client.code')}} ASPS50THANG10</strong>
          <span>{{t('client.expiry_date')}}: 28/01/2024</span>
        </div>
        <div>
          <p>{{t('client.discount_description')}}</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            :label="t('client.condition')"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">{{t('client.code')}} ASPS50THANG10</strong>
          <span>{{t('client.expiry_date')}}: 28/01/2024</span>
        </div>
        <div>
          <p>{{t('client.discount_description')}}</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
           :label="t('client.condition')"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button :label="t('client.expired')" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
      <div
        class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
      >
        <div class="item-voucher-left bg-gray-100"></div>
        <div class="item-voucher-right bg-gray-100"></div>
        <div class="flex justify-content-between">
          <strong class="text-blue-500">Mã: ASPS50THANG10</strong>
          <span>HSD: 28/01/2024</span>
        </div>
        <div>
          <p>Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</p>
        </div>
        <Divider type="dashed" />
        <div class="flex justify-content-between">
          <Button
            label="Điều kiện"
            @click="visibleCond = true"
            class="px-5 hover:bg-primary"
            outlined
          />
          <Button label="Hết hạn" disabled severity="secondary" class="px-5"/>
        </div>
      </div>
    </div>
  </Sidebar>
  <Dialog
    v-model:visible="visibleCond"
    position="center"
    :draggable="false"
    modal
    header="Điều kiện"
    :style="{ width: '35rem' }"
  >
    <div class="flex flex-column gap-3">
      <strong class="text-blue-500 text-2xl text-center">Mã: ASPS50THANG10</strong>
      <span class="text-gray-400"
        >Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối đa 250K</span
      >
      <div>
        <Divider type="dashed" />
        <ul>
          <li>Đồng giá 2 triệu cho nhóm sản phẩm Set combo</li>
          <li>Đồng giá 2 triệu cho nhóm sản phẩm Set combo</li>
        </ul>
      </div>
    </div>
    <div class="flex justify-content-center gap-5 mt-4">
      <Button
        label="Đóng"
        severity="secondary"
        class="px-8"
        @click="visibleCond = false"
      />
      <Button
        :label="t('body.promotion.status_active')"
        class="px-8"
        @click="
          () => {
            visibleCond = false;
            visibleRight = false;
          }
        "
      />
    </div>
  </Dialog>
</template>
<style scoped>
.btn:focus {
  outline: none;
}
.item-voucher-right {
  right: calc(-1 * 1.6rem / 2);
  content: "";
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 100%;
}
.item-voucher-left {
  left: calc(-1 * 1.6rem / 2);
  content: "";
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 100%;
}
</style>
