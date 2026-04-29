<template>
    <div class="card p-0">
        <div class="flex justify-content-between p-3 border-bottom-1 border-200">
            <span class="text-primary text-xl font-semibold my-auto"
                >{{ t('client.account_info') }}</span
            >
            <!-- <Button label="Chỉnh sửa" icon="pi pi-pencil"/> -->
        </div>
        <div class="p-3">
            <div class="grid m-0 align-items-center">
                <div class="col-3 pt-0">
                    <div class="font-semibold my-auto">{{ t('client.user_name') }}</div>
                </div>
                <div class="col-9 pt-0">
                    <span>{{ accountInfo?.userName || "-" }}</span>
                </div>
                <div class="col-3 pt-0">
                    <div class="font-semibold my-auto">{{ t('client.full_name') }}</div>
                </div>
                <div class="col-9 pt-0">
                    <span>{{ accountInfo?.fullName || "-" }}</span>
                </div>
                <div class="col-3 pt-0">
                    <div class="font-semibold my-auto">{{ t('client.email') }}</div>
                </div>
                <div class="col-9 pt-0">
                    <span>{{ accountInfo?.email || "-" }}</span>
                </div>
                <div class="col-3 pt-0">
                    <div class="font-semibold my-auto">{{ t('client.phoneNumber') }}</div>
                </div>
                <div class="col-9 pt-0">
                    <span>{{ accountInfo?.phone || "-" }}</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { useMeStore } from "../../../../../../Pinia/me";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
interface Account {
    claims: [];
    isSupperUser: false;
    personRoleId: any;
    personRole: any;
    role: any;
    userRoles: any;
    fullName: string;
    phone: any;
    userType: string;
    note: any;
    dob: any;
    status: string;
    cardId: 24;
    bpInfo: any;
    userGroup: any;
    lastLogin: string;
    organizationId: any;
    id: 14;
    userName: string;
    normalizedUserName: string;
    email: string;
    normalizedEmail: string;
    phoneNumber: any;
}

const meStore = useMeStore();
const accountInfo = ref<Account>();
onMounted(async function () {
    const data = await meStore.getMe();
    if (data) {
        const acc = data as { user: Account };
        accountInfo.value = acc.user;
    }
});
</script>

<style scoped>
.col-3 {
    line-height: 33px;
}
</style>
