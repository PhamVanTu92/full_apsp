<script setup>
import { ref, reactive } from "vue";
import AppConfig from "@/layout/AppConfig.vue";
import { useRouter, useRoute } from "vue-router";
const router = useRouter();
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
const disabledBtn = ref(false);

const user = reactive({
    email: null,
});

const messageResponse = ref({
    value: null,
    severity: "info",
});

const errorMessages = reactive({
    email: null,
});
const validate = (data) => {
    errorMessages.email = null;
    if (!emailRegex.test(data.email)) {
        errorMessages.email = t("Login.errorEmail");
        return false;
    }
    return true;
};

import API from "@/api/api-main";
const loading = ref(false);
const timeInterval = ref();
const countDown = ref(30);
const handleSubmit = async () => {
    if (validate(user)) {
        loading.value = true;
        API.add("account/forgot-password", {
            email: user.email?.trim(),
        })
            .then((res) => {
                disabledBtn.value = true;
                timeInterval.value = setInterval(() => {
                    if (countDown.value > 0) {
                        countDown.value--;
                    } else {
                        clearInterval(timeInterval.value);
                        countDown.value = 30;
                        disabledBtn.value = false;
                    }
                }, 1000);
                loading.value = false;

                messageResponse.value.value = res.data;
                messageResponse.value.severity = "info";
            })
            .catch((error) => {
                loading.value = false;
                messageResponse.value.value = error.response.data.error;
                messageResponse.value.severity = "error";
            });
    } else {
    }
};
</script>

<template>
    <div
        class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
        <div class="flex flex-column align-items-center justify-content-center md:w-4">
            <div class="liner">
                <div class="w-full surface-card py-8 px-5 sm:px-8" style="border-radius: 53px">
                    <div class="text-center mb-5 align-items-center flex flex-column">
                        <a href="/">
                            <img src="/image/logo/apoil-logo.png" class="mb-3" height="100" alt="logo" />
                        </a>
                    </div>
                    <div class="field">
                        <div class="font-semibold mb-5">
                            {{ t('Login.titleEmail') }}
                        </div>
                        <label for="email" class="block text-900 text-base font-semibold mb-2">Email</label>
                        <InputText :invalid="errorMessages.email" id="email" type="mail"
                            :placeholder="t('Login.email.placeholder')" class="w-full" style="padding: 1rem"
                            v-model="user.email" />
                        <small class="text-red-500" v-if="errorMessages.email">{{
                            errorMessages.email
                        }}</small>
                        <Message :closable="false" v-if="messageResponse.value" :severity="messageResponse.severity">{{
                            messageResponse.value }}</Message>
                    </div>
                    <Button :disabled="disabledBtn && countDown > 0" :loading="loading" @click="handleSubmit()"
                        :label="disabledBtn ? t('Login.buttons.sendBack') + ` (${countDown}s)` : t('Login.buttons.send')"
                        class="w-full p-3 text-lg mb-3 mt-3"/>
                    <div class="text-center">
                        <Button @click="router.replace({ path: '/login' })" icon="pi pi-arrow-left"
                            :label="t('Login.buttons.backToLogin')" class="" text/>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <AppConfig simple />
    <Toast />
</template>

<style scoped>
.liner {
    border-radius: 56px;
    padding: 0.3rem;
    background: linear-gradient(180deg,
            var(--primary-color) 10%,
            rgba(33, 150, 243, 0) 30%);
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
