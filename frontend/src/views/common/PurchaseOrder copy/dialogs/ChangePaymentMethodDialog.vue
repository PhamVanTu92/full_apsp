<template>
    <div>
        <Dialog header="Phương thức thanh toán" v-model:visible="visible" modal>
            <Dropdown
                v-model="payment"
                class="w-20rem"
                :options="paymentOptions"
                option-label="label"
                option-value="value"
            ></Dropdown>
            <template #footer>
                <Button
                    @click="visible = false"
                    label="Đóng"
                    severity="secondary"
                />
                <Button @click="onClickConfirmPaymentType" label="Chọn"/>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, PropType } from "vue";
import { ItemDetail } from "../types/entities";
const visible = ref(false);

const selectedItem = defineModel("selectedItem", {
    type: Array as PropType<ItemDetail[]>,
    default: () => [],
});
type PaymentType = "PayNow" | "PayCredit" | "PayGuarantee";
const payment = ref<PaymentType>("PayNow");

const onClickConfirmPaymentType = () => {
    if (selectedItem.value.length === 0) {
        visible.value = false;
        return;
    }
    selectedItem.value.forEach((item) => {
        item.paymentMethodCode = payment.value;
    });
    visible.value = false;
};

const paymentOptions = ref([
    { label: "Thanh toán ngay", value: "PayNow" },
    { label: "Công nợ tín chấp", value: "PayCredit" },
    { label: "Công nợ bảo lãnh", value: "PayGuarantee" },
]);

const open = () => {
    payment.value = "PayNow";
    visible.value = true;
};

defineExpose({
    open,
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
