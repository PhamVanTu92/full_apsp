<template>
    <div>
        <div class="otp-container">
            <input
                v-for="(digit, index) in otp"
                :key="index"
                type="text"
                maxlength="1"
                inputmode="numeric"
                class="otp-input"
                v-model="otp[index]"
                ref="otpRefs"
                @input="handleInput(index)"
                @keydown.backspace="handleBackspace(index, $event)"
                @paste="handlePaste($event)"
                @keypress="onlyNumber($event)"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { isNumber } from "lodash";
import { ref, onMounted, watch } from "vue";

const otp = ref(Array(6).fill(""));
const otpRefs = ref<HTMLElement[]>([]);
const modelValue = defineModel({
    type: String,
    required: true,
});

const emits = defineEmits(["enter"]);

watch(
    () => JSON.stringify(otp.value),
    (value) => {
        const numString = otp.value.filter((x) => x).join("");
        modelValue.value = numString;
    }
);

// Chỉ cho nhập số
const onlyNumber = (event: KeyboardEvent) => {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode === 13) {
        emits("enter", modelValue.value);
    }
    if (charCode < 48 || charCode > 57) {
        event.preventDefault();
    }
};

// Tự động focus sang ô kế tiếp khi nhập xong
const handleInput = (index: number) => {
    if (otp.value[index].length === 1 && index < otp.value.length - 1) {
        otpRefs.value[index + 1].focus();
    }
};

// Tự động lùi khi Backspace ở ô trống
const handleBackspace = (index: number, event: any) => {
    if (event.key === "Backspace" && !otp.value[index] && index > 0) {
        otpRefs.value[index - 1]?.focus();
    }
};

// Xử lý paste dãy số
const handlePaste = (event: any) => {
    event.preventDefault();
    const pasteData = event.clipboardData.getData("Text").replace(/\D/g, ""); // Chỉ lấy số
    for (let i = 0; i < otp.value.length; i++) {
        otp.value[i] = pasteData[i] || "";
    }

    // Focus về ô cuối cùng nếu paste đầy
    const nextFocusIndex = Math.min(pasteData.length, otp.value.length - 1);
    otpRefs.value[nextFocusIndex].focus();
};

onMounted(() => {
    otpRefs.value[0].focus();
});
</script>

<style scoped>
.otp-container {
    display: flex;
    gap: 8px;
    justify-content: center;
}

.otp-input {
    width: 40px;
    height: 40px;
    font-size: 24px;
    text-align: center;
    border: 1px solid var(--surface-200);
    border-radius: 6px;
    outline: none;
    color: var(--surface-700);
}
.otp-input:focus {
    border: 1px solid var(--green-600);
}
</style>
