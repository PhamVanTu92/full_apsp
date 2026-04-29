<template>
    <div>
        <Button
            class="w-full py-3 px-1 border-noround border-none border-bottom-1 border-dashed border-300 text-gray-500 hover:text-green-700"
            text
            @click="visible = false"
        >
            <div class="flex justify-content-between align-items-center w-full">
                <div class="flex align-items-center gap-3">
                    <i class="pi pi-ticket"></i>
                    <span>Khuyến mại</span>
                </div>
                <div class="flex align-items-center gap-3">
                    <span>Thay đổi</span>
                    <i class="pi pi-angle-right"></i>
                </div>
            </div>
        </Button>
        <Sidebar
            v-model:visible="visible"
            class="w-30rem"
            position="right"
            header="Mã giảm giá"
        >
            <div class="card p-3 h-full overflow-y-auto surface-100">
                <div
                    v-for="(voucher, i) in vouchers"
                    class="overflow-hidden mb-4"
                    :key="i"
                >
                    <div class="card flex p-0 mb-0">
                        <div class="p-3 border-round flex-grow-1 coupon bg-green-700">
                            <div style="color: yellow" class="text-lg font-semibold mb-2">
                                ASPS50THANG10
                            </div>
                            <div class="text-100">
                                Giảm 15% cho đơn hàng giá trị tối thiểu 500k. Mã giảm tối
                                đa 250K
                            </div>
                            <div></div>
                        </div>
                        <div class="p-3 flex flex-column gap-1" style="min-width: 9rem">
                            <div class="text-right mb-2">Còn 19 ngày</div>
                            <div class="flex flex-column gap-1 justify-content-end">
                                <Button
                                    class="flex-grow-1 p-1"
                                    label="Chi tiết"
                                    size="small"
                                    severity="info"
                                    outlined
                                />
                                <Button
                                    class="flex-grow-1 p-1"
                                    label="Áp dụng"
                                    size="small"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </Sidebar>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";

const visible = ref(false);
const vouchers = ref(Array.from({ length: 10 }, () => ({})));

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.l-cb {
}
.coupon {
    position: relative;
    border-right: 2px dashed var(--surface-300);
}
.coupon::before {
    content: " ";
    height: 16px;
    width: 16px;
    background: var(--surface-100);
    display: block;
    position: absolute;
    right: -10px;
    top: -8px;
    border-radius: 50%;
    border: 1px solid var(--surface-200);
}
.coupon::after {
    content: " ";
    height: 16px;
    width: 16px;
    background: var(--surface-100);
    display: block;
    position: absolute;
    right: -10px;
    bottom: -8px;
    border-radius: 50%;
    border: 1px solid var(--surface-200);
}
</style>
