<template>
    <h4 class="font-bold mb-4">{{ t('client.account_info') }}</h4>
    <div class="grid m-0 p-1">
        <div class="lg:col-3 col-12">
            <div class="text-center">
                <Image :src="link" width="140px" height="140px" class="border-circle overflow-hidden"
                    imageClass="border-circle" preview />
                <!-- <Button icon="pi pi-camera"/> -->
                <FileUpload class="mt-3" mode="basic" name="demo[]" url="/api/upload" accept="image/*"
                    :maxFileSize="1000000" @upload="onUpload" :auto="true" :chooseLabel="t('client.select_image')"
                    uploadIcon="pi pi-camera">
                </FileUpload>
            </div>
        </div>
        <div class="lg:col-9 col-12">
            <div class="card p-0">
                <div class="p-3 flex justify-content-between align-items-center">
                    <span class="font-bold text-xl">{{ t('Custom.Info') }}</span>
                    <Button v-if="mode1 == 'view'" :label="t('client.edit')" :disabled="mode1 == 'edit'"
                        icon="pi pi-pencil" @click="editDataClick1" />
                    <div v-else-if="mode1 == 'edit'" class="">
                        <Button class="mr-3" @click="cancelEditClick1" :label="t('Logout.cancel')" icon="pi pi-times"
                            severity="secondary" />
                        <Button :label="t('client.save')" icon="pi pi-save" />
                    </div>
                </div>
                <hr class="m-0" />
                <div class="py-3 px-5">
                    <div class="grid m-0 align-items-center">
                        <div class="col-2 py-1">{{ t('body.systemSetting.full_name_label') }}:</div>
                        <div v-if="mode1 == 'view'" class="col-10">
                            {{ `${userData.fullName}` }}
                        </div>
                        <div v-else-if="mode1 == 'edit'" class="col-10 py-1">
                            <InputText v-model="userData.fullName" placeholder="Họ và tên" class="w-10rem mr-3" />
                        </div>
                    </div>
                    <div class="grid m-0 align-items-center">
                        <div class="col-2">{{ t('body.systemSetting.username_label') }}:</div>
                        <div class="col">
                            {{ userData.userName }}
                        </div>
                    </div>
                    <div class="grid m-0 align-items-center">
                        <div class="col-2">{{ t('Navbar.menu.roles') }}</div>
                        <div class="col">{{ userData?.role?.name }}</div>
                    </div>
                    <div class="grid m-0 align-items-center">
                        <div class="col-2">{{ t('body.systemSetting.phone_label') }}:</div>
                        <div class="col">{{ userData.phone }}</div>
                    </div>
                </div>
            </div>
            <div class="card p-0">
                <div class="p-3 flex justify-content-between align-items-center">
                    <span class="font-bold text-xl">{{ t('client.change_password') }}</span>
                    <Button v-if="mode2 == 'view'" :label="t('Custom.change')" :disabled="mode2 == 'edit'"
                        icon="pi pi-pencil" @click="editDataClick2" />
                    <div v-else-if="mode2 == 'edit'" class="">
                        <Button class="mr-3" @click="cancelEditClick2" :label="t('Logout.cancel')" icon="pi pi-times"
                            severity="secondary" />
                        <Button :loading="loading1" @click="onClickChangePassword" :label="t('client.save')"
                            icon="pi pi-save" />
                    </div>
                </div>
                <hr class="m-0" />
                <div class="py-3 px-5">
                    <template v-if="mode2 == 'view'">
                        <div>
                            <i class="pi pi-lock mr-3"></i>{{ t('body.systemSetting.password_requirements_note') }}
                        </div>
                        <div class="mt-2">
                            <div>
                                <i class="fa-solid fa-triangle-exclamation mr-3"></i>{{
                                    t('body.systemSetting.password_requirements_heading') }}
                            </div>
                            <ul class="mt-2">
                                <li class="mb-2">{{ t('body.systemSetting.password_requirement_8_chars') }}</li>
                                <li class="mb-2">
                                    {{ t('body.systemSetting.password_requirement_upper_lower') }}
                                </li>
                                <li class="mb-2"> {{ t('body.systemSetting.password_requirement_number') }}</li>
                                <li class="mb-2">{{ t('body.systemSetting.password_requirement_special_char') }}</li>
                            </ul>
                        </div>
                    </template>
                    <template v-else-if="mode2 == 'edit'">
                        <div class="grid">
                            <div class="col-6">
                                <div class="grid m-0 align-items-center justify-content-end">
                                    <div class="col-5 p-0">{{ t('Custom.current_password') }}</div>
                                    <div class="col p-0">
                                        <div>
                                            <Password :invalid="!!errorMessagePw.currentPassword
                                                " :feedback="false" v-model="modelPassword.currentPassword" toggleMask>
                                            </Password>
                                        </div>
                                        <small class="text-red-500">{{
                                            errorMessagePw.currentPassword
                                            }}</small>
                                    </div>
                                    <!-- <div class="col-7 p-0 mt-2">
                                        <a href="#" class="text-blue-700"
                                            >Quên mật khẩu?</a
                                        >
                                    </div> -->
                                </div>
                                <hr />
                                <div class="grid m-0 align-items-center mb-3">
                                    <div class="col-5 p-0">{{ t('client.newPassword') }}</div>
                                    <div class="col p-0">
                                        <div>
                                            <Password :invalid="!!errorMessagePw.newPassword" :feedback="false"
                                                v-model="modelPassword.newPassword" toggleMask></Password>
                                        </div>
                                        <small class="text-red-500">{{
                                            errorMessagePw.newPassword
                                            }}</small>
                                    </div>
                                </div>
                                <div class="grid m-0 align-items-center mb-3">
                                    <div class="col-5 p-0">{{ t('client.confirm_new_password') }}</div>
                                    <div class="col p-0">
                                        <div>
                                            <Password :invalid="!!errorMessagePw.confirmPassword
                                                " :feedback="false" v-model="modelPassword.confirmPassword" toggleMask>
                                            </Password>
                                        </div>
                                        <small class="text-red-500">{{
                                            errorMessagePw.confirmPassword
                                            }}</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-6">
                                <div>
                                    <i class="pi pi-lock mr-3"></i>{{ t('body.systemSetting.password_requirements_note')
                                    }}
                                </div>
                                <div class="mt-2">
                                    <div>
                                        <i class="fa-solid fa-triangle-exclamation mr-3"></i>{{
                                            t('body.systemSetting.password_requirements_heading') }}
                                    </div>
                                    <ul class="mt-2">
                                        <li class="mb-2">
                                            {{ t('body.systemSetting.password_requirement_8_chars') }}
                                            <i v-if="
                                                regexLength.test(
                                                    modelPassword.newPassword
                                                )
                                            " class="pi pi-check-circle ml-1 text-green-700 font-bold"></i>
                                        </li>
                                        <li class="mb-2">
                                            {{ t('body.systemSetting.password_requirement_upper_lower') }}
                                            <i v-if="
                                                regexUpnLow.test(
                                                    modelPassword.newPassword
                                                )
                                            " class="pi pi-check-circle ml-1 text-green-700 font-bold"></i>
                                        </li>
                                        <li class="mb-2">
                                            {{ t('body.systemSetting.password_requirement_number') }}
                                            <i v-if="
                                                regexNum.test(
                                                    modelPassword.newPassword
                                                )
                                            " class="pi pi-check-circle ml-1 text-green-700 font-bold"></i>
                                        </li>
                                        <li class="mb-2">
                                            {{ t('body.systemSetting.password_requirement_special_char') }}
                                            <i v-if="
                                                regexSpecialChar.test(
                                                    modelPassword.newPassword
                                                )
                                            " class="pi pi-check-circle ml-1 text-green-700 font-bold"></i>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </template>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onBeforeMount, reactive } from "vue";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const toast = useToast();
const regexLength = /^.{8,}$/;
const regexUpnLow = /^(?=.*[a-z])(?=.*[A-Z]).+$/;
const regexNum = /^(?=.*\d).+$/;
const regexSpecialChar = /^(?=.*[@$!#%*?&]).+$/;

const regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!#%*?&])[A-Za-z\d@$!#%*?&]{8,}$/;

const errorMessagePw = reactive({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});
const checkValidPassword = () => {
    Object.keys(errorMessagePw).forEach((key) => {
        errorMessagePw[key] = null;
    });
    if (!modelPassword.currentPassword || !modelPassword.currentPassword.trim()) {
        errorMessagePw.currentPassword = t('Login.password.placeholder');
    }
    if (!modelPassword.newPassword) {
        errorMessagePw.newPassword = t('Custom.new_password');
    } else {
        if (!regexPassword.test(modelPassword.newPassword)) {
            errorMessagePw.newPassword = t('body.systemSetting.invalid_password_message');
        }
    }
    if (!modelPassword.confirmPassword) {
        errorMessagePw.confirmPassword = t('client.confirm_new_password');
    }
    if (modelPassword.newPassword && modelPassword.confirmPassword) {
        if (modelPassword.newPassword !== modelPassword.confirmPassword) {
            errorMessagePw.confirmPassword = "Mật khẩu mới không trùng khớp";
        }
    }
    return (
        !errorMessagePw.currentPassword &&
        !errorMessagePw.confirmPassword &&
        !errorMessagePw.newPassword
    );
};

const loading1 = ref(false);
const onClickChangePassword = async () => {
    const isValid = checkValidPassword();
    if (isValid) {
        loading1.value = true;

        try {
            await API.update("account/me/change-password", {
                newPassword: modelPassword.newPassword,
                oldPassword: modelPassword.currentPassword,
            });
            cancelEditClick2();
            toast.add({
                severity: "success",
                summary: "Thay đổi mật khẩu thành công",
                detail: "Mật khẩu của bạn đã được thay đổi thành công",
                life: 3000,
            });
        } catch (error) {
            if (error.response.data.status == 404) {
                errorMessagePw.currentPassword = "Mật khẩu sai";
            } else {
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Đổi mật khẩu không thành công",
                    life: 3000,
                });
            }
        } finally {
            loading1.value = false;
        }
    }
};

const mode1 = ref("view");
const mode2 = ref("view");

const userData = ref({});

const onUpload = () => {
    alert(1);
};

const modelPassword = reactive({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

const modelUserEdit = reactive({
    first_name: "",
    last_name: "",
});

const cancelEditClick1 = () => {
    mode1.value = "view";
    modelUserEdit.first_name = null;
    modelUserEdit.last_name = null;
};

const cancelEditClick2 = () => {
    mode2.value = "view";
    modelPassword.currentPassword = null;
    modelPassword.newPassword = null;
    modelPassword.confirmPassword = null;
    Object.keys(errorMessagePw).forEach((key) => {
        errorMessagePw[key] = null;
    });
};

const editDataClick2 = () => {
    mode2.value = "edit";
};

const editDataClick1 = () => {
    mode1.value = "edit";
    modelUserEdit.first_name = userData.value.first_name;
    modelUserEdit.last_name = userData.value.last_name;
};

const link =
    "https://i0.wp.com/www.repol.copl.ulaval.ca/wp-content/uploads/2019/01/default-user-icon.jpg?ssl=1";

onBeforeMount(async () => {
    try {
        const res = await API.get("Account/me");
        userData.value = res.data.user;
    } catch (error) {
        console.error(error);
    }
});
</script>

<style></style>
