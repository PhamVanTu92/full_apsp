<script setup>
import { ref, computed, onMounted, reactive } from "vue";
// import AppConfig from "@/layout/AppConfig.vue";
import { useRouter, useRoute } from "vue-router";
import { Validator, PATTERN } from "../../helpers/validate";
import { useToast } from "primevue/usetoast";

const router = useRouter();
const route = useRoute();
const toast = useToast();

const validateOption = {
    password: {
        validators: {
            required: true,
            type: String,
            name: "Mật khẩu",
            pattern: PATTERN.password,
        },
    },
};

const validate = () => {
    const vr = Validator(passwordModel.value, validateOption);
    if (vr.result && passwordModel.value.password != passwordModel.value.repeatPassword) {
        vr.result = false;
        vr.errors.repeatPassword = "Nhập khẩu không khớp";
    }
    return vr;
};

const errorMessages = ref({});
const passwordModel = ref({
    password: null,
    repeatPassword: null,
});

import API from "@/api/api-main";
const loading = ref(false);
const countDown = ref(0);
const handleSubmit = async () => {
    let validateResult = validate();
    errorMessages.value = validateResult.errors;
    if (validateResult.result) {
        const payload = {
            email: route.query.email,
            token: route.query.token,
            newPassword: passwordModel.value.password,
        };
        loading.value = true;
        API.add("account/reset-password", payload)
            .then((res) => {
                loading.value = false;
                toast.add({
                    severity: "success",
                    summary: "Đổi mật khẩu thành công",
                    detail: "Bạn đang được điều hướng sang trang đăng nhập.",
                    life: 3000,
                });
                countDown.value = 3;
                const ti = setInterval(() => {
                    if (countDown.value > 0) {
                        countDown.value--;
                    }
                    if (countDown.value === 0) {
                        clearInterval(ti);
                        router.replace({
                            path: "/login",
                        });
                    }
                }, 1000);
            })
            .catch((error) => {
                loading.value = false;
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Đã có lỗi xảy ra, vui lòng thử lại sau.",
                    life: 3000,
                });
            });
        return;
    }
};

const passthrought = {
    input: { root: { class: "w-full p-3" } },
    root: { class: "w-full" },
};

const routeToLogin = () => {
    router.replace({
        path: "/login",
    });
};

onMounted(() => {
    if (!route.query.token && !route.query.email) {
        routeToLogin();
    }
});
const regexLength = /^.{8,}$/;
const regexUpnLow = /^(?=.*[a-z])(?=.*[A-Z]).+$/;
const regexNum = /^(?=.*\d).+$/;
const regexSpecialChar = /^(?=.*[@$!#%*?&]).+$/;
</script>

<template>
    <div
        class="surface-ground flex align-items-center justify-content-center overflow-hidden min-h-screen min-w-screen"
    >
        <div class="flex flex-column align-items-center justify-content-center w-6">
            <div class="liner w-full">
                <div
                    class="w-full surface-card py-8 px-5 sm:px-8"
                    style="border-radius: 53px"
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
                    <div class="grid">
                        <div class="col-7">
                            <div class="h-full border-right-1 border-300 p-4">
                                <div class="flex gap-3">
                                    <i class="pi pi-lock"></i>
                                    <div>
                                        Bạn nên sử dụng mật khẩu mạnh mà mình chưa sử dụng
                                        ở đâu khác
                                    </div>
                                </div>
                                <div class="mt-2 flex gap-3">
                                    <div>
                                        <i class="fa-solid fa-triangle-exclamation"></i>
                                    </div>
                                    <div class="">
                                        Mật khẩu tài khoản của bạn phải đáp ứng các yêu
                                        cầu sau:
                                        <ul class="pl-3">
                                            <li class="mb-2">
                                                Có ít nhất 8 ký tự
                                                <i
                                                    v-if="
                                                        regexLength.test(
                                                            passwordModel.password
                                                        )
                                                    "
                                                    class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                                ></i>
                                            </li>
                                            <li class="mb-2">
                                                Có ít nhất một chữ hoa và một chữ thường
                                                <i
                                                    v-if="
                                                        regexUpnLow.test(
                                                            passwordModel.password
                                                        )
                                                    "
                                                    class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                                ></i>
                                            </li>
                                            <li class="mb-2">
                                                Có ít nhất một chữ số
                                                <i
                                                    v-if="
                                                        regexNum.test(
                                                            passwordModel.password
                                                        )
                                                    "
                                                    class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                                ></i>
                                            </li>
                                            <li class="mb-2">
                                                Có ít nhất một ký tự đặc biệt
                                                <i
                                                    v-if="
                                                        regexSpecialChar.test(
                                                            passwordModel.password
                                                        )
                                                    "
                                                    class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                                ></i>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-5">
                            <div class="field">
                                <label
                                    for="password"
                                    class="block text-900 text-base font-semibold mb-2"
                                    >Nhập mật khẩu mới</label
                                >
                                <Password
                                    id="password"
                                    :invalid="errorMessages.password"
                                    placeholder="Nhập mật khẩu"
                                    :pt="passthrought"
                                    v-model="passwordModel.password"
                                    toggleMask
                                    :feedback="false"
                                />
                                <small
                                    class="text-red-500"
                                    v-if="errorMessages.password"
                                    >{{ errorMessages.password }}</small
                                >
                            </div>
                            <div class="field">
                                <label
                                    for="repeatPassword"
                                    class="block text-900 text-base font-semibold mb-2"
                                    >Nhập lại mật khẩu</label
                                >
                                <Password
                                    id="repeatPassword"
                                    :invalid="errorMessages.repeatPassword"
                                    placeholder="Nhập mật khẩu"
                                    :pt="passthrought"
                                    v-model="passwordModel.repeatPassword"
                                    toggleMask
                                    :feedback="false"
                                />
                                <small
                                    class="text-red-500"
                                    v-if="errorMessages.repeatPassword"
                                    >{{ errorMessages.repeatPassword }}</small
                                >
                            </div>
                            <div>
                                <Button
                                    :disabled="countDown > 0"
                                    :loading="loading"
                                    @click="handleSubmit()"
                                    label="Lưu"
                                    class="w-full p-3 text-lg mb-3 mt-3"
                                />
                                <Button
                                    @click="routeToLogin"
                                    icon="pi pi-arrow-left"
                                    severity="secondary"
                                    :label="`Quay lại trang đăng nhập ${
                                        countDown ? `(${countDown}s)` : ''
                                    }`"
                                    class="w-full p-3 text-lg"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- <AppConfig simple /> -->
    <Toast />
</template>

<style scoped>
.liner {
    border-radius: 56px;
    padding: 0.3rem;
    background: linear-gradient(
        180deg,
        var(--primary-color) 10%,
        rgba(33, 150, 243, 0) 30%
    );
}

.pi-eye {
    transform: scale(1.6);
    margin-right: 1rem;
}

.pi-eye-slash {
    transform: scale(1.6);
    margin-right: 1rem;
}
</style>
