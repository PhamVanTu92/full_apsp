<template>
    <div class="card p-0">
        <div class="p-3 flex justify-content-between">
            <div class="font-semibold text-xl my-auto text-primary">{{ t('client.change_password') }}</div>
            <div v-if="enableEdit" class="flex gap-2">
                <Button
                    icon="pi pi-times"
                    :label="t('client.cancel')"
                    severity="secondary"
                    @click="onClickCancelEdit"
                />
                <Button @click="onClickSave" icon="pi pi-save" :label="t('client.save')" />
            </div>
            <Button v-else icon="pi pi-pencil" :label="t('client.edit')" @click="onClickEdit" />
        </div>
        <hr class="m-0" />
        <div class="p-3">
            <div class="p-3 pb-0 border-1 border-200">
                <div class="grid">
                    <div class="col-6" :class="{ hidden: !enableEdit }">
                        <div class="grid m-0 align-items-center justify-content-end">
                            <div class="col-5 p-0 font-semibold">{{ t('body.systemSetting.password_label') }}</div>
                            <div class="col p-0">
                                <div>
                                    <Password
                                        :invalid="!!errorMessagePw.currentPassword"
                                        :feedback="false"
                                        v-model="modelPassword.currentPassword"
                                        toggleMask
                                    ></Password>
                                </div>
                                <small class="text-red-500">{{
                                    errorMessagePw.currentPassword
                                }}</small>
                            </div>
                            <div class="col-7 p-0 mt-2">
                                <a href="/forgot-password" class="text-blue-700">{{ t('Login.links.forgotPassword') }}</a>
                            </div>
                        </div>
                        <hr />
                        <div class="grid m-0 align-items-center mb-3">
                            <div class="col-5 p-0 font-semibold">{{ t('body.systemSetting.password_label') }}</div>
                            <div class="col p-0">
                                <div>
                                    <Password
                                        :invalid="!!errorMessagePw.newPassword"
                                        :feedback="false"
                                        v-model="modelPassword.newPassword"
                                        toggleMask
                                    ></Password>
                                </div>
                                <small class="text-red-500">{{
                                    errorMessagePw.newPassword
                                }}</small>
                            </div>
                        </div>
                        <div class="grid m-0 align-items-center mb-3">
                            <div class="col-5 p-0 font-semibold">
                                {{t('client.confirm_new_password')}}
                            </div>
                            <div class="col p-0">
                                <div>
                                    <Password
                                        :invalid="!!errorMessagePw.confirmPassword"
                                        :feedback="false"
                                        v-model="modelPassword.confirmPassword"
                                        toggleMask
                                    ></Password>
                                </div>
                                <small class="text-red-500">{{
                                    errorMessagePw.confirmPassword
                                }}</small>
                            </div>
                        </div>
                    </div>
                    <div
                        class="flex-grow-1 p-3"
                        :class="{ 'col-6': enableEdit, col: !enableEdit }"
                    >
                        <div class="flex gap-3">
                            <i class="pi pi-lock"></i>
                            <div>
                                {{ t('body.systemSetting.password_requirements_note') }}
                            </div>
                        </div>
                        <div class="mt-2 flex gap-3">
                            <div>
                                <i class="fa-solid fa-triangle-exclamation"></i>
                            </div>
                            <div class="">
                               {{ t('body.systemSetting.password_requirements_heading') }}
                                <ul class="pl-3">
                                    <li class="mb-2">
                                         {{ t('body.systemSetting.password_requirement_8_chars') }}
                                        <i
                                            v-if="
                                                regexLength.test(
                                                    modelPassword.newPassword
                                                )
                                            "
                                            class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                        ></i>
                                    </li>
                                    <li class="mb-2">
                                          {{ t('body.systemSetting.password_requirement_upper_lower') }}
                                        <i
                                            v-if="
                                                regexUpnLow.test(
                                                    modelPassword.newPassword
                                                )
                                            "
                                            class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                        ></i>
                                    </li>
                                    <li class="mb-2">
                                       {{ t('body.systemSetting.password_requirement_number') }}
                                        <i
                                            v-if="
                                                regexNum.test(modelPassword.newPassword)
                                            "
                                            class="pi pi-check-circle ml-1 text-green-700 font-bold"
                                        ></i>
                                    </li>
                                    <li class="mb-2">
                                        {{ t('body.systemSetting.password_requirement_special_char') }}
                                        <i
                                            v-if="
                                                regexSpecialChar.test(
                                                    modelPassword.newPassword
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
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { useToast } from "primevue/usetoast";
import API from "@/api/api-main";
import { Validator, PATTERN } from "../../../../../../helpers/validate";
import cloneDeep from "lodash/cloneDeep";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const toast = useToast();
const regexLength = /^.{8,}$/;
const regexUpnLow = /^(?=.*[a-z])(?=.*[A-Z]).+$/;
const regexNum = /^(?=.*\d).+$/;
const regexSpecialChar = /^(?=.*[@$!#%*?&]).+$/;

const validateOption = {
    newPassword: {
        validators: {
            type: String,
            name: "mật khẩu",
            required: true,
            pattern: PATTERN.password,
            patternMessage: "Mật khẩu không đúng định dạng",
        },
    },
    currentPassword: {
        validators: {
            type: String,
            name: "mật khẩu",
            required: true,
        },
    },
};
const onClickSave = () => {
    // Your code to save new password goes here
    // alert(1);
    const cloneModel = cloneDeep(modelPassword.value);
    // debugger;
    const validate = Validator(cloneModel, validateOption);
    errorMessagePw.value = validate.errors;
    if (validate.result) {
        if (modelPassword.value.newPassword != modelPassword.value.confirmPassword) {
            errorMessagePw.value.confirmPassword = "Mật khẩu không khớp";
            return;
        }
        const payload = {
            oldPassword: modelPassword.value.currentPassword,
            newPassword: modelPassword.value.newPassword,
        };
        API.update("account/me/change-password", payload)
            .then((res) => {
                enableEdit.value = false;
                toast.add({
                    severity: "success",
                    summary: "Thay đổi mật khẩu thành công",
                    detail: "Mật khẩu của bạn đã được thay đổi thành công",
                    life: 3000,
                });
                modelPassword.value.currentPassword = "";
                modelPassword.value.newPassword = "";
                modelPassword.value.confirmPassword = "";
                errorMessagePw.value = {};
            })
            .catch((error) => {
                if (error.response.data.status == 404) {
                    errorMessagePw.value.currentPassword = "Mật khẩu sai";
                } else {
                    toast.add({
                        severity: "error",
                        summary: "Lỗi",
                        detail: "Đổi mật khẩu không thành công",
                        life: 3000,
                    });
                }
            });
    }
};

const modelPassword = ref({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

const errorMessagePw = ref({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

const enableEdit = ref(false);
const onClickEdit = () => {
    enableEdit.value = true;
};
const onClickCancelEdit = () => {
    Object.keys(modelPassword.value).forEach((key) => {
        modelPassword.value[key] = "";
    });
    enableEdit.value = false;
};
</script>

<style></style>
