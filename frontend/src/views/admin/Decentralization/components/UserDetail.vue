<template>
    <div class="">
        <div class="font-bold mb-3 text-lg">{{ t('body.systemSetting.user_info_section') }}</div>
        <div class="card flex mb-3 p-0">
            <div class="p-3 mx-4">
                <div
                    class="w-8rem h-8rem flex justify-content-center align-items-center surface-200 border-circle mx-auto my-3"
                >
                    <i class="fa fa-solid fa-user text-500 text-7xl"></i>
                </div>
                <Button v-if="editable" icon="pi pi-camera" :label="t('client.select_image')"/>
            </div>
            <div class="flex-grow-1 p-3">
                <div class="flex gap-3 flex-wrap mb-3">
                    <div class="flex flex-column gap-2 w-15rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.username_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <InputText
                                v-if="editable"
                                disabled
                                :value="props.user.userName"
                            ></InputText>
                            <span v-else>{{ props.user?.userName }}</span>
                        </template>
                    </div>
                    <div class="flex flex-column gap-2 w-20rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.full_name_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <template v-if="editable">
                                <InputText
                                    v-model="user.fullName"
                                    :invalid="invalidCheck(errorMessage.fullName)"
                                ></InputText>
                                <small class="text-red-500">{{
                                    errorMessage.fullName
                                }}</small>
                            </template>
                            <span v-else>{{ props.user?.fullName }}</span>
                        </template>
                    </div>
                    <div class="flex flex-column gap-2 w-20rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.phone_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <template v-if="editable">
                                <InputText
                                    v-model="user.phone"
                                    :invalid="invalidCheck(errorMessage.phone)"
                                ></InputText>
                                <small class="text-red-500">{{
                                    errorMessage.phone
                                }}</small>
                            </template>
                            <span v-else>{{ props.user?.phone }}</span>
                        </template>
                    </div>
                    <div class="flex flex-column gap-2 w-25rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.email_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <template v-if="editable">
                                <InputText
                                    v-model="user.email"
                                    :invalid="invalidCheck(errorMessage.email)"
                                ></InputText>
                                <small class="text-red-500">{{
                                    errorMessage.email
                                }}</small>
                            </template>
                            <span v-else>{{ props.user?.email }}</span>
                        </template>
                    </div>
                </div>
                <div class="flex gap-3 flex-wrap">
                    <div class="flex flex-column gap-2 w-15rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.role_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <template v-if="editable">
                                <Dropdown
                                    v-model="user.roleId"
                                    :options="props.roleOption"
                                    optionLabel="name"
                                    optionValue="id"
                                    v-if="editable"
                                    :invalid="invalidCheck(errorMessage.roleId)"
                                ></Dropdown>
                                <small class="text-red-500">{{
                                    errorMessage.roleId
                                }}</small>
                            </template>
                            <span v-else>{{ props.user?.role?.name || "---" }}</span>
                        </template>
                    </div>
                    <div class="flex flex-column gap-2 w-20rem">
                        <label class="font-semibold" for="">{{ t('body.systemSetting.department_label') }}</label>
                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <div
                                v-if="props.user?.orgStrct"
                                class="flex align-items-center gap-2"
                            >
                                <span>{{ props.user?.orgStrct?.name }}</span>
                                <span>-</span>
                                <span
                                    v-if="
                                        props.user?.orgStrct?.managerUser?.id ==
                                        props.user?.id
                                    "
                                >
                                    {{ t('body.systemSetting.direct_manager') }}
                                </span>
                                <span v-else>Nhân viên</span>
                            </div>
                        </template>
                    </div>
                    <div class="flex flex-column w-10rem">
                        <label class="font-semibold mb-2" for="">{{ t('body.systemSetting.status_label') }}</label>

                        <Skeleton
                            width="15rem"
                            height="1.5rem"
                            class="my-auto"
                            v-if="props.loading"
                        ></Skeleton>
                        <template v-else>
                            <Tag
                                :value="getStatusLabel(props.user.status).label"
                                :severity="getStatusLabel(props.user.status).severity"
                                class="p-1 text-sm"
                            ></Tag>
                        </template>
                    </div>
                </div>
            </div>
        </div>
        <div class="flex justify-content-end align-items-center gap-2">
            <template v-if="editable">
                <Button
                    @click="onClickResetPw"
                    icon="pi pi-sync"
                    :label="t('body.systemSetting.reset_password')"
                    severity="info"
                />
                <Button
                    @click="onClickDeactive"
                    :icon="getStatusLabel(props.user.status).btnicon"
                    :label="getStatusLabel(props.user.status).btnLabel"
                    :severity="getStatusLabel(props.user.status).btnseverity"
                />
                <div class="h-2rem border-left-1 border-300"></div>
                <Button
                    @click="onClickCancelEdit"
                    severity="secondary"
                    :label="t('body.PurchaseRequestList.cancel_button')"
                />
                <Button :loading="loading.save" @click="onClickSave" :label="t('body.sampleRequest.importPlan.save_button')"/>
            </template>
            <Button
                :disabled="props.loading"
                @click="onClickEdit"
                v-else
                :label="t('body.OrderList.edit')"
            />
        </div>
        <Dialog
            @hide="onHideResetPwDlg"
            :header="t('client.confirmResetPassword')"
            v-model:visible="visible.resetPw"
            modal
            :draggable="false"
            class="w-30rem"
        >
            <template v-if="!isConfirmReset">
                <p>{{t('client.confirmResetPasswordAccount')}}</p>
            </template>
            <template v-else>
                <div class="font-semibold mb-2">{{t('client.newPassword')}}</div>
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
                    @click="visible.resetPw = false"
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
                    :label="t('client.apply')"
                    icon="pi pi-check"
                    :loading="loadingResetPw"
                />
            </template>
        </Dialog>
        <ConfirmDialog></ConfirmDialog>
    </div>
</template>

<script setup async lang="ts">
import { ref, reactive, onMounted } from "vue";
import API from "@/api/api-main";
import { useRoute, useRouter } from "vue-router";
// import { cloneDeep } from "lodash";
// import cloneDeep from "lodash-es/cloneDeep";

import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
// import { Nullable } from "primevue/ts-helpers";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const emits = defineEmits(["save"]);

const props = defineProps<{
    user: UserResponse;
    roleOption: [];
    loading: boolean;
    orgStrct: [];
}>();

const confirm = useConfirm();
const toast = useToast();
const vueRouter = { router: useRouter(), route: useRoute() };
const editable = ref<boolean>(false);
const loading = reactive({
    save: false,
});

const visible = reactive({
    resetPw: false,
});

const handleDeactive = (status: string): void => {
    API.add(`account/DeActive?id=${props.user.id}&Status=${status}`)
        .then((res) => {
            loading.save = false;
            emits("save", props.user);
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.save_button') || "Thành công",
                detail: t('body.systemSetting.update_success_message') || "Cập nhật tài khoản thành công",
                life: 3000,
            });
            editable.value = false;
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.cancel') || "Lỗi",
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
            loading.save = false;
            console.error(error);
            editable.value = false;
        });
};

const onClickDeactive = (): void => {
    const status = props.user.status == "A" ? "I" : "A";
    if (status == "I") {
        confirm.require({
            message: t('client.confirmDeactivateAccount'),
            header: t('client.confirmDeactivate'),
            acceptLabel: t('client.confirm'),
            rejectLabel: t('body.status.HUY2'),
            acceptIcon: "pi pi-check",
            rejectIcon: "pi pi-times",
            acceptClass: "p-button-danger",
            rejectClass: "p-button-secondary",
            accept() {
                handleDeactive(status);
            },
        });
    } else {
        handleDeactive(status);
    }
};

const onClickEdit = (): void => {
    user.value = new UserUpdate(props.user);
    editable.value = true;
};
const onClickCancelEdit = (): void => {
    errorMessage.value = {};
    editable.value = false;
};
const onClickResetPw = (): void => {
    visible.resetPw = true;
};

const onHideResetPwDlg = (): void => {
    isConfirmReset.value = false;
    isCopy.value = false;
    password.value = "";
    // visible.resetPw = false;
};

const isConfirmReset = ref(false);
const onClickConfirmResetPassword = (): void => {
    // Simulate reset password logic
    isConfirmReset.value = true;
    // Reset password logic here
    password.value = generatePw(8);
    // visible.resetPw = false;
};

const isCopy = ref<boolean>(false);
const onClickCopy = (): void => {
    navigator.clipboard
        .writeText(password.value)
        .then(() => {
            isCopy.value = true;
            toast.add({
                severity: "info",
                summary: t('body.home.details') || "Tin nhắn",
                detail: t('body.systemSetting.password_copied_message') || "Mật khẩu đã được copy",
                life: 3000,
            });
        })
        .catch(() => {
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.clear') || "Lỗi",
                detail: t('body.systemSetting.clipboard_not_supported') || "Trình duyệt của bạn không hỗ trợ chức năng này",
                life: 3000,
            });
        });
};

const password = ref<string>("");
// --- this is old function ----
// const generatePw = (length: number): string => {
//     let result = Math.random().toString(36).slice(2, 10);
//     // const characters =
//     //     "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789`~!@#$%^&*()_+-={}[]:;,.<>";
//     // for (let i = 0; i < length; i++) {
//     //     result += characters.charAt(Math.floor(Math.random() * characters.length));
//     // }
//     return result;
// };

function generatePw(_length: number) {
    // Các bộ ký tự để sinh mật khẩu
    const lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    const uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const numberChars = "0123456789";
    const specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    // Hàm để lấy một ký tự ngẫu nhiên từ một chuỗi cho trước
    const getRandomChar = (charSet) =>
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

const loadingResetPw = ref(false);
const handleResetPw = (): void => {

    loadingResetPw.value = true;
    API.update(`account/${props.user.id}`, {
        id: props.user.id,
        email: props.user.email,
        fullName: props.user.fullName,
        password: password.value,
        confirmPassword: password.value,
    })
        .then((res) => {

            visible.resetPw = false;
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.save_button') || "Thành công",
                detail: t('body.systemSetting.password_change_success') || "Thay đổi mật khẩu thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            console.error(error);
            toast.add({
                severity: "error",
                summary: t('body.OrderApproval.clear') || "Lỗi",
                detail: t('body.systemSetting.password_change_failed') || "Thay đổi mật khẩu thất bại",
                life: 3000,
            });
        })
        .finally(() => {
            loadingResetPw.value = false;
        });
};

interface UserUpdate {
    id: number | null;
    email: string | null;
    fullName: string | null;
    address: string | null;
    locationId: number | null;
    locationName: string | null;
    areaId: number | null;
    areaName: string | null;
    phone: string | null;
    note: string | null;
    dob: string | Date | null;
    isAllBranch: string | null;
    isInforOther: string | null;
    isAllGeneral: string | null;
    password: string | null;
    confirmPassword: string | null;
    roleId: number | null;
    branchId: number | null;
    branchName: string | null;
    status: string | null;
    selectionOrgStrct: any;
    isManager: any;
}

import { Validator, ValidateOption } from "../../../../helpers/validate";

class UserUpdate {
    constructor(userResponse?: UserResponse) {
        if (userResponse) Object.assign(this, userResponse);
    }
    toJSON(): UserUpdate {
        Object.keys(this).forEach((key) => {
            if (this[key] === undefined) {
                delete this[key];
            }
            if (typeof this[key] == "string" || this[key] instanceof String) {
                // trim string
                this[key] = this[key]?.trim();
            }
        });
        return this;
    }
    validate(options: ValidateOption): { result: boolean; errors: object } {
        return Validator(this.toJSON(), options);
    }
}

interface UserResponse extends UserUpdate {
    claims: [];
    role: Role | null;
    userRoles: any;
    fullName: string | null;
    phone: string | null;
    userType: string | null;
    note: string | null;
    dob: string | null;
    status: string | null;
    cardId: number | null;
    bpInfo: any;
    userGroup: any;
    id: number;
    userName: string | null;
    normalizedUserName: string | null;
    email: string | null;
    normalizedEmail: string | null;
    phoneNumber: string | null;
    organizationId: any;
    orgStrct: any;
}

interface Role {
    roleClaims: [];
    privilegeIds: [];
    id: number;
    name: string | null;
    normalizedName: string | null;
    concurrencyStamp: string | null;
}

class UserResponse {}
const user = ref<UserUpdate>(new UserUpdate());

import { phoneRegex } from "../../../../helpers/regex";
const optionValidate = {
    email: {
        validators: {
            required: true,
            name: "email",
        },
    },
    fullName: {
        validators: {
            required: true,
            name: "họ và tên",
        },
    },
    roleId: {
        validators: {
            required: true,
            nullMessage: "Chọn vai trò",
            type: Number,
        },
    },
    phone: {
        validators: {
            required: true,
            name: "số điện thoại",
            pattern: phoneRegex,
            patternMessage: "Số điện thoại không hợp lệ",
        },
    },
};

const errorMessage = ref<any>({});
const invalidCheck = (errorMsg: string | undefined): boolean => {
    return errorMsg ? true : false;
};
const onClickSave = (): void => {
    loading.save = true;
    const validate = user.value.validate(optionValidate);

    // return;
    if (!validate.result) {
        errorMessage.value = validate.errors;

        loading.save = false;
        return;
    }
    const data = user.value.toJSON();
    API.update(`account/${user.value.id}`, data)
        .then((res) => {
            loading.save = false;
            emits("save", res.data);
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.systemSetting.update_account_success_message'),
                life: 3000,
            });
            editable.value = false;
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: err.response.data.errors,
                life: 3000,
            });
            loading.save = false;
            console.error(error);
            // editable.value = false;
        });
};

interface Tag {
    label: string;
    severity: string;
    btnicon: string;
    btnLabel: string;
    btnseverity: string;
}

const statusEnum = {
    A: {
        label: t('body.sampleRequest.customer.active_status'),
        btnLabel: t('body.systemSetting.deactivate_account') || "Ngừng hoạt động",
        btnseverity: "danger",
        severity: "primary",
        btnicon: "pi pi-lock",
    },
    I: {
        label: t('body.sampleRequest.customer.inactive_status'),
        btnLabel: t('body.sampleRequest.customer.active_status_option') || "Kích hoạt",
        btnseverity: "primary",
        severity: "secondary",
        btnicon: "pi pi-unlock",
    },
};
const getStatusLabel = (status: string | null, isReverse = false): Tag => {
    return (
        statusEnum[status || ""] || {
            label: null,
            severity: null,
            icon: null,
            btnLabel: null,
        }
    );
};

onMounted(() => {});
</script>

<style scoped>
.pw {
    letter-spacing: 1px;
    font-size: large;
}
</style>
