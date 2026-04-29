<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.report.zalo_list') }}</h4>
            <div class="flex gap-2">
                <Button :label="t('Custom.change')" @click="actionEdit = true" v-if="actionEdit == false" />
                <Button :label="t('Logout.cancel')" @click="actionEdit = false" v-if="actionEdit == true" outlined
                    size="small" severity="danger" />
                <Button :label="t('body.promotion.save_button')" @click="handleSave" v-if="actionEdit == true"></Button>
                <ButtonGoBack />
            </div>
        </div>

        <Card class="shadow-2xl">
            <template #content>
                <div class="grid">
                    <div class="col-12 flex gap-2  align-items-center">
                        <label for="id" class="font-semibold text-gray-700 flex items-center gap-2">
                            <i class="pi pi-hashtag text-blue-600" />
                            ID
                        </label>
                        <InputNumber id="id" v-model="dataZalo.id" v-if="actionEdit == true" />
                        <span v-else> {{ dataZalo.id }}</span>
                    </div>

                    <div class="col-6">
                        <label for="access_token" class="font-semibold text-gray-700 flex items-center gap-2  mb-2">
                            <i class="pi pi-lock text-green-600"></i>
                            Access Token
                        </label>
                        <Textarea id="access_token" v-model="dataZalo.access_token" :rows="10" class="w-full  text-sm"
                            placeholder="Nhập access token..." v-if="actionEdit == true" />
                        <span v-else class="block overflow-x-auto whitespace-nowrap max-w-xs ">
                            {{ dataZalo.refresh_token }}
                        </span>

                    </div>

                    <div class="col-6">
                        <label for="refresh_token" class="font-semibold text-gray-700 flex items-center gap-2 mb-2">
                            <i class="pi pi-refresh text-purple-600"></i>
                            Refresh Token
                        </label>
                        <Textarea id="refresh_token" v-model="dataZalo.refresh_token" :rows="10" class="w-full  text-sm"
                            placeholder="Nhập refresh token..." v-if="actionEdit == true" />
                        <span v-else class="block overflow-x-auto whitespace-nowrap max-w-xs"> {{ dataZalo.refresh_token
                            }}</span>
                    </div>

                    <div class="col-4 flex gap-2 align-items-center">
                        <i class="pi pi-clock text-orange-600"></i>
                        <span>{{ t('body.report.expires_in') }} </span>
                        <InputNumber id="expires_in" v-model="dataZalo.expires_in" class="w-20rem" :step="1000"
                            v-if="actionEdit == true" />
                        <span v-else>{{ dataZalo.expires_in }} </span>
                        {{ t('body.report.second') }}
                    </div>

                    <div class="col-4 flex gap-2 align-items-center">
                        <label for="templateConfirmed" class="font-semibold text-gray-700 flex items-center gap-2">
                            <i class="pi pi-check-circle text-green-600"></i>
                            {{ t('body.report.template_confirmed') }}
                        </label>
                        <InputText id="templateConfirmed" v-model="dataZalo.templateConfirmed" v-if="actionEdit == true"
                            placeholder="ID template..." />
                        <span v-else>{{ dataZalo.templateConfirmed }}</span>
                    </div>

                    <div class="col-4 flex gap-2 align-items-center">
                        <label for="templateCompleted" class="font-semibold text-gray-700 flex items-center gap-2">
                            <i class="pi pi-check-square text-blue-600"></i>
                            {{ t('body.report.template_completed') }}
                        </label>
                        <InputText id="templateCompleted" v-model="dataZalo.templateCompleted" v-if="actionEdit == true"
                            placeholder="ID template..." />
                        <span v-else>{{ dataZalo.templateCompleted }}</span>
                    </div>
                </div>
            </template>
        </Card>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import API from '@/api/api-main';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import { zaloTokenData } from './type/type';
import ButtonGoBack from '@/components/ButtonGoBack.vue';
const actionEdit = ref(false)
const toast = useToast();
const { t } = useI18n();
const dataZalo = ref<typeof zaloTokenData>({ ...zaloTokenData });

// Fetch Zalo configuration data
const fetchData = async () => {
    try {
        const res = await API.get('Zalo');
        dataZalo.value = res.data || { ...zaloTokenData };
    } catch (error) {
        console.error('Lỗi tải dữ liệu Zalo:', error);
        toast.add({ severity: 'error', summary: t('Custom.error'), detail: t('client.no_data'), life: 3000 });
    }
};

// Save Zalo configuration
const handleSave = async () => {
    try {
        await API.update('Zalo', dataZalo.value);
        actionEdit.value = false
        toast.add({ severity: 'success', summary: t('body.systemSetting.success_label'), detail: t('Notification.save_success'), life: 3000 });
    } catch (error) {
        console.error('Lỗi lưu dữ liệu Zalo:', error);
        toast.add({ severity: 'error', summary: t('Custom.error'), detail: t('client.update_failed'), life: 3000 });
    }
};

// Load data on component mount
onMounted(() => {
    fetchData();
});
</script>

<style scoped lang="css">
small {
    color: var(--orange-500);
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

form {
    animation: fadeIn 0.3s ease-in;
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }

    to {
        opacity: 1;
    }
}
</style>
