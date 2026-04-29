<template>
    <div>
        <div
            class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden"
        >
            <div
                class="flex flex-column align-items-center justify-content-center md:w-4"
            >
                <div class="liner">
                    <div
                        class="w-full surface-card py-8 px-5 sm:px-8"
                        style="border-radius: 53px; max-width: 60rem"
                    >
                        <div class="text-center mb-5 align-items-center flex flex-column">
                            <a href="/">
                                <img
                                    src="/image/logo/apoil-logo.png"
                                    class="mb-3"
                                    height="100"
                                    alt="logo"
                                />
                            </a>
                        </div>
                        <div class="field">
                            <label
                                for="otp"
                                class="block text-900 text-base font-semibold mb-3"
                                >Nhập mã OTP</label
                            >
                            <InputOtp
                                v-model="otp"
                                class="mb-2"
                                :otpLenght="OTP_LENGTH"
                                @enter="onClickSubmit()"
                            />

                            <small class="text-red-500 ml-5">{{ errorMsg }}</small>
                        </div>
                        <Button
                            @click="onClickSubmit"
                            label="Gửi"
                            class="w-full p-3 text-lg"
                            :loading="loading"
                        />
                        <div class="text-center mt-2">
                            <Button
                                @click="router.replace({ name: 'login' })"
                                icon="pi pi-arrow-left"
                                label="Quay trở lại đăng nhập"
                                text
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import API from "@/api/api-main";
import { useRoute, useRouter } from "vue-router";

const OTP_LENGTH = 6;
const route = useRoute();
const router = useRouter();
const otp = ref<string>("");
const loading = ref(false);

const errorMsg = ref("");
const onClickSubmit = () => {
    errorMsg.value = "";
    if (otp.value.length < OTP_LENGTH) {
        errorMsg.value = "Mã OTP không đúng định dạng";
        return;
    }
    loading.value = true;
    API.add(`account/verify-otp?otp=${otp.value}&email=${route.query.uid}`)
        .then((res) => {
            const userData = res.data;
            if (userData) {
                const currentDate = new Date();
                const expireTime = userData["access-exp"]
                    ? userData["access-exp"] * 60 * 1000
                    : 0;
                const expireTokenTime = new Date(
                    currentDate.getTime() + expireTime
                ).toString();
                const jToken = { ExpireToken: expireTokenTime, ...userData };
                localStorage.setItem("user", JSON.stringify(jToken));
                router.replace("/");
            }
        })
        .catch((error) => {
            errorMsg.value = err.response?.data?.errors || "Đã có lỗi xảy ra";
        })
        .finally(() => {
            loading.value = false;
        });
};

const initialComponent = () => {
    // code here
    if (!route.query.uid) {
        router.push({
            name: "login",
        });
    }
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
