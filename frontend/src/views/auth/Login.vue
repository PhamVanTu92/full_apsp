<script setup>
import { ref, computed, onMounted } from 'vue';
import AppConfig from '@/layout/AppConfig.vue';
import { useAuthStore } from '@/Pinia/auth';
import { useRouter, useRoute } from 'vue-router';
import { useGlobal } from '@/services/useGlobal';
import footerClient from '../client/layout/footerClient.vue';
const { toast, FunctionGlobal } = useGlobal();
const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const userCredentials = ref({
    username: '',
    password: ''
}); 
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const loading = ref(false);
// onMounted(() => {
//   if (store.state.auth.status.loggedIn) {
//     router.push({ name: "dashboard" });
//   }
// });
const errorMsg = ref('');
const handleSubmit = async () => {
    if (userCredentials.value.username && userCredentials.value.password) {
        try {
            loading.value = true;
            const res = await authStore.login(userCredentials.value);
            loading.value = false;
            if (res.status === false) {
                // FunctionGlobal.$notify("E", res.message, toast);
                if (res._2FA) {
                    router.push({
                        name: 'otp',
                        query: {
                            uid: res.username
                        }
                    });
                    return;
                }
                errorMsg.value = res.message;
                return;
            } else {
                if (route.query.goto) router.push(route.query.goto);
                else {
                    router.push({ name: 'dashboard' });
                }
            }
        } catch (error) {
            // FunctionGlobal.$notify("E", error, toast);
            errorMsg.value = error;
        }
    } else {
        errorMsg.value = t('Login.error');
    }
};
</script>

<template>
    <div
        class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
        <div class="flex flex-column align-items-center justify-content-center">
            <div class="bg-login">
                <div class="w-full surface-card py-8 px-5 sm:px-8" style="border-radius: 53px">
                    <div class="text-center mb-5 align-items-center flex flex-column">
                        <img src="../../../public/image/logo/new-logo-ap.png" class="mb-3" height="100" alt="logo" />
                        <!-- <strong class="text-2xl text-primary mb-3">Saigon Petro OmniChannel</strong> -->
                        <span class="text-600 font-medium">{{ t('Login.title') }}</span>
                    </div>
                    <div class="mb-3">
                        <div v-if="errorMsg" class="p-2 border-1 border-red-100 border-round">
                            <div class="text-red-600 font-semibold p-1 text-sm">
                                {{ errorMsg }}
                            </div>
                        </div>
                    </div>
                    <div>
                        <label for="email1" class="block text-900 text-base font-semibold mb-2">{{ t('Login.username.label') }}</label>
                        <InputText id="email1" type="text" :placeholder="t('Login.username.placeholder')"
                            class="w-full md:w-30rem mb-3" style="padding: 1rem" v-model="userCredentials.username" />
                        <label for="email1" class="block text-900 text-base font-semibold mb-2">{{ t('Login.password.label') }}</label>
                        <Password id="password1" v-model="userCredentials.password" :placeholder="t('Login.password.placeholder')"
                            :toggleMask="true" :feedback="false" class="w-full mb-3" inputClass="w-full"
                            :inputStyle="{ padding: '1rem' }" @keyup.enter="handleSubmit()">
                        </Password>
                        <div class="flex justify-content-end mb-5 gap-5"> 
                            <router-link to="/forgot-password"
                                class="font-medium no-underline ml-2 text-right cursor-pointer"
                                style="color: var(--primary-color)">{{ t('Login.links.forgotPassword') }}</router-link>
                        </div>

                        <Button @click="handleSubmit()" :label="t('Login.buttons.login')" class="w-full p-3 text-lg mb-3"
                            :loading="loading"/>
                        <router-link v-if="false" to="/register">
                            <Button label="Đăng ký" text class="w-full p-3 text-lg"/>
                        </router-link>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <footerClient></footerClient>
    <AppConfig simple />
    <Toast />
</template>

<style scoped>
.bg-login {
    border-radius: 56px;
    padding: 0.3rem;
    background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%);
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
