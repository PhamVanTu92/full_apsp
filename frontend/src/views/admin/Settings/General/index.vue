<template>
    <div>
        <div class="flex justify-content-between mb-3">
            <h4 class="font-bold mb-0">{{ t('body.systemSetting.general_settings_title') }}</h4>
            <Button @click="onClickSave" :label="t('body.systemSetting.apply_button')"/>
        </div>
        <div class="card">
            <div class="option-block">
                <div class="text-xl font-bold p-3 surface-200">{{ t('body.systemSetting.2fa_settings_title') }}</div>
                <DataTable :value="settingModel" style="width: 45rem">
                    <Column header="" field="is2FARequired" class="w-3rem">
                        <template #body="{ data, field }">
                            <Checkbox v-model="data[field]" binary></Checkbox>
                        </template>
                    </Column>
                    <Column :header="t('body.systemSetting.account_type_column')" field="userType"></Column>
                    <Column :header="t('body.systemSetting.authentication_method_column')" field="twoFactorType" class="w-15rem">
                        <template #body="{ data, field }">
                            <Dropdown :disabled="!data.is2FARequired" v-model="data[field]" :options="validateMethodOptions" option-label="label" option-value="value" class="w-full"></Dropdown>
                        </template>
                    </Column>
                    <Column :header="t('body.systemSetting.otp_duration_column')" field="timeout" class="w-10rem">
                        <template #body="{ data, field }">
                            <InputGroup>
                                <InputNumber :disabled="!data.is2FARequired" v-model="data[field]" :min="1"></InputNumber>
                                <InputGroupAddon>{{t('body.systemSetting.minute_label')}}</InputGroupAddon>
                            </InputGroup>
                        </template>
                    </Column>
                </DataTable>
            </div>
            <div class="option-block">
                <div class="text-xl font-bold p-3 surface-200">{{ t('body.systemSetting.login_session_settings_title') }}</div>
                <DataTable :value="settingModel" style="width: 45rem">
                    <Column header="" field="isSessionTimeRequired" class="w-3rem">
                        <template #body="{ data, field }">
                            <Checkbox v-model="data[field]" binary></Checkbox>
                        </template>
                    </Column>
                    <Column :header="t('body.systemSetting.account_type_column')" field="userType"></Column>
                    <Column :header="t('body.systemSetting.idle_logout_duration_column')" field="sessionTime" class="w-25rem">
                        <template #body="{ data, field }">
                            <InputGroup>
                                <InputNumber :disabled="!data.isSessionTimeRequired" v-model="data[field]" :min="5" placeholder="Tối thiểu 5 phút"></InputNumber>
                                <InputGroupAddon>{{t('body.systemSetting.minute_label')}}</InputGroupAddon>
                            </InputGroup>
                        </template>
                    </Column>
                </DataTable>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { SettingModel } from './class/model';
import { validateMethodOptions } from './options/dropdownOptions';
import API from '@/api/api-main';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const settingModel = ref<SettingModel[]>([]);
const toast = useToast();
const loading = ref(false);

const onClickSave = () => {
    loading.value = true;
    API.update('appsetting', settingModel.value)
        .then((res) => {
            toast.add({
                severity: 'success',
                summary: t('body.report.apply_success'),
                life: 3000
            });
        })
        .catch((error) => {
            toast.add({
                severity: 'error',
                summary: t('body.report.error_occurred_message'),
                life: 3000
            });
        })
        .finally(() => {
            loading.value = false;
        });
};

const fetchData = () => {
    API.get('appsetting').then((res) => {
        settingModel.value = res.data.item;
    });
};

const initialComponent = () => {
    // code here
    fetchData();
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.option-block {
    /* border-bottom: 1px solid var(--surface-200); */
}
</style>
