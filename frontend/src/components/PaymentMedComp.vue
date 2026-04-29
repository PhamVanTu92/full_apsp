<template>
    <div v-if="!props.isClient" class="flex align-items-center justify-content-between">
        <strong>Phương thức thanh toán:</strong>
    </div>
    <div v-if="!props.isClient" class="flex flex-column gap-3">
        <div v-for="(item, index) in PaymentMed" :key="index" class="flex gap-2">
            <div v-if="!props.isClient" class="flex align-items-center">
                <RadioButton
                    v-model="data.paymentMethodCode"
                    :inputId="item.paymentMethodCode"
                    @change="onPaymentChange"
                    :value="item.paymentMethodCode"
                    class="mr-2"
                ></RadioButton>
                <label class="cursor-pointer" :for="item.paymentMethodCode"
                    >{{ item.paymentMethodName }}
                </label>
            </div>
            <div class="flex-grow-1 flex justify-content-between" v-else>
                <span>{{ item.paymentMethodName }}</span>
                <span>0 đ</span>
            </div>
        </div>
    </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import API from "../api/api-main";
import Button from "primevue/button";

const props = defineProps({
    isClient: {
        type: Boolean,
        required: false,
    },
});

const ModelP = defineModel("PaymentMed", {
    type: Array,
    default: [],
});
const PaymentMed = ref([]);
const data = ref({
    paymentMethodCode: "PayNow",
    paymentMethodName: "Thanh toán ngay",
});
onMounted(() => {
    fetchAllPaymentMed();
});
const fetchAllPaymentMed = async () => {
    try {
        const res = await API.get(`PaymentMethod/getall`);
        PaymentMed.value = res.data.map((el) => ({
            paymentMethodID: el.id,
            paymentMethodCode: el.paymentMethodCode,
            paymentMethodName: el.paymentMethodName,
        }));
        // PaymentMed.value.push({
        //     paymentMethodID: 3,
        //     paymentMethodCode: "Incoterm2020",
        //     paymentMethodName: "Incoterm 2020",
        // });
    } catch (error) {}
};

const onPaymentChange = () => {
    ModelP.value = PaymentMed.value.find(
        (el) => el.paymentMethodCode == data.value.paymentMethodCode
    );
};
</script>
<style></style>
