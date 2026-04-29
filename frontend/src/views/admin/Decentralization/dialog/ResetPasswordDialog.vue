<template>
    <div>
        <Dialog
            v-model:visible="visible"
            modal
            :draggable="false"
            class="w-30rem"
            :header="t('client.confirmResetPassword')"
            @hide="onHide"
        >
            <template v-if="!isConfirmReset">
                <p>{{t('client.confirmResetPasswordAccount')}}</p>
            </template>
            <template v-else>
                <div class="font-semibold mb-2">{{t('client.newPassword')}}</div>
                <!-- style="width: 33rem" -->
                <div
                    class="card bg-gray-200 gap-2 font-medium p-2 flex align-items-center justify-content-between mb-2"
                >
                    <div class="pw p-1">{{ password }}</div>
                    <div>
                        <Button
                            @click="password = generatePw(8)"
                            text
                            icon="pi pi-refresh"
                            severity="primary"
                            :disabled="loadingResetPw"
                        />
                        <Button
                            @click="onClickCopy"
                            text
                            icon="pi pi-copy"
                            severity="secondary"
                        />
                    </div>
                </div>
                <small class="font-italic"
                    ><span class="text-red-500 font-bold mr-2">*</span>{{t('client.pleaseRememberPassword')}}</small
                >
            </template>
            <template #footer>
                <Button
                    @click="visible = false"
                    :label="t('client.cancel')"
                    icon="pi pi-times"
                    severity="secondary"
                />
                <Button
                    v-if="!isConfirmReset"
                    @click="onClickConfirmResetPassword()"
                    :label="t('client.continue')"
                    icon="fa-solid fa-forward"
                />
                <Button
                    v-else
                    @click="handleResetPw"
                    label="Áp dụng"
                    icon="pi pi-check"
                    :loading="loadingResetPw"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { useToast } from "primevue/usetoast";
import { ref, reactive, computed, watch, onMounted } from "vue";
const toast = useToast();
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const visible = ref(false);
const isConfirmReset = ref(false);
const password = ref("");
const loadingResetPw = ref(false);
const isCopy = ref(false);

const user = ref<any>();

const handleResetPw = () => {
    if (!user.value.id) return;
    loadingResetPw.value = true;
    API.update(`account/${user.value.id}`, {
        id: user.value.id,
        email: user.value.email,
        fullName: user.value.fullName,
        password: password.value,
        confirmPassword: password.value,
    })
        .then((res) => {

            visible.value = false;
            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Thay đổi mật khẩu thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            console.error(error);
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Thay đổi mật khẩu thất bại",
                life: 3000,
            });
        })
        .finally(() => {
            loadingResetPw.value = false;
        });
};

const onClickConfirmResetPassword = () => {
    isConfirmReset.value = true;
    // Reset password logic here
    password.value = generatePw(8);
};

const onClickCopy = (): void => {
    navigator.clipboard
        .writeText(password.value)
        .then(() => {
            isCopy.value = true;
            toast.add({
                severity: "info",
                summary: "Tin nhắn",
                detail: "Mật khẩu đã được copy",
                life: 3000,
            });
        })
        .catch(() => {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Trình duyệt của bạn không hỗ trợ chức năng này",
                life: 3000,
            });
        });

    // if (!isCopy.value) {
    //     setTimeout(() => {
    //         isCopy.value = false;
    //     }, 3000);
    // }
};

const lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
const uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const numberChars = "0123456789";
const specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
function generatePw(_length: number) {
    // Các bộ ký tự để sinh mật khẩu

    // Hàm để lấy một ký tự ngẫu nhiên từ một chuỗi cho trước
    const getRandomChar = (charSet: any) =>
        charSet[Math.floor(Math.random() * charSet.length)];

    // Đảm bảo có ít nhất 1 chữ thường
    const lowercase = getRandomChar(lowercaseChars);

    // Đảm bảo có ít nhất 1 chữ hoa
    const uppercase = getRandomChar(uppercaseChars);

    // Đảm bảo có ít nhất 1 số
    const number = getRandomChar(numberChars);

    // Đảm bảo có ít nhất 1 ký tự đặc biệt
    const specialChar = getRandomChar(specialChars);

    // Tạo các ký tự còn lại để đủ 8 ký tự
    const allChars = lowercaseChars + uppercaseChars + numberChars + specialChars;
    const remainingChars = Array.from({ length: _length }, () => getRandomChar(allChars));

    // Kết hợp và trộn ngẫu nhiên các ký tự
    const password = [lowercase, uppercase, number, specialChar, ...remainingChars];

    // Trộn ngẫu nhiên mật khẩu
    for (let i = password.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [password[i], password[j]] = [password[j], password[i]];
    }

    // Chuyển mảng về thành chuỗi
    return password.join("");
}

const open = (_user: any) => {
    user.value = _user;
    visible.value = true;
};

const onHide = () => {
    isConfirmReset.value = false;
    loadingResetPw.value = false;
};

const initialComponent = () => {
    // code here
};

defineExpose({
    open,
});

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.pw {
    letter-spacing: 1px;
    font-size: large;
}
</style>
