<script setup lang="ts">
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import Dialog from "primevue/dialog";
import AddApprovalLevelComp from "@/components/AddApprovalLevelComp.vue";
import UserSelector from "@/components/UserSelector/UserSelector.vue";
import type { DocItem, FormData, UserInfo } from "./types";

const { t } = useI18n();

const props = defineProps<{
    visible: boolean;
    formData: FormData;
    Docs: DocItem[];
    submited: boolean;
}>();

const emit = defineEmits<{
    "update:visible": [value: boolean];
    submit: [];
    hide: [];
}>();

const userSelectorComp = ref<InstanceType<typeof UserSelector>>();

const isDialogVisible = computed({
    get: () => props.visible,
    set: (value: boolean) => emit("update:visible", value),
});

const onClickAddUser = (): void => {
    userSelectorComp.value?.open();
};

const onClickDelete = (data: UserInfo, index: number): void => {
    props.formData.rUsers?.splice(index, 1);
    props.formData.approvalSampleMembersLines = (props.formData.rUsers || []).map((user) => user.id);
};

const onHide = (): void => {
    props.formData.rUsers = [];
    props.formData.approvalSampleMembersLines = [];
    emit("hide");
};

const onChangeRUser = (event: UserInfo[]): void => {
    props.formData.rUsers = event;
    props.formData.approvalSampleMembersLines = event.map(user => user.id);
};

const onChangeCheckBox = (item: DocItem, event: Event): void => {
    if (item.selected) {
        if (!props.formData.approvalSampleDocumentsLines.includes(item.transType)) {
            props.formData.approvalSampleDocumentsLines.push(item.transType);
        }
    } else {
        const index = props.formData.approvalSampleDocumentsLines.indexOf(item.transType);
        if (index > -1) {
            props.formData.approvalSampleDocumentsLines.splice(index, 1);
        }
    }
};

const handleSubmit = (): void => {
    emit("submit");
};
</script>

<template>
    <Dialog v-model:visible="isDialogVisible" modal
        :header="formData?.id ? t('body.promotion.update_button') : t('body.systemSetting.add_new_approval_template')"
        :style="{ width: '1000px' }" @hide="onHide">
        <div class="flex flex-column gap-3">
            <div class="flex flex-column">
                <label for="approvalTmpName" class="font-semibold my-2">{{
                    t('body.systemSetting.approval_template_name') }}
                    <sup class="text-red-500">*</sup></label>
                <InputText :invalid="submited && !formData.approvalSampleName" id="approvalTmpName"
                    v-model="formData.approvalSampleName" class="flex-auto" autocomplete="off" required />
            </div>
            <div class="flex flex-column">
                <label for="desc" class="font-semibold my-2">{{ t('body.systemSetting.description') }}</label>
                <Textarea id="desc" rows="3" v-model="formData.description" class="flex-auto"
                    autocomplete="off"></Textarea>
            </div>
            <div class="card p-0 overflow-hidden mb-0">
                <UserSelector v-model="formData.approvalSampleMembersLines"
                    :notIncludedIds="formData.approvalSampleMembersLines" ref="userSelectorComp"
                    @change="onChangeRUser" />
                <TabView>
                    <TabPanel :header="t('body.systemSetting.creator')">
                        <DataTable :value="formData.rUsers" showGridlines>
                            <Column header="#" class="w-3rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column field="fullName" :header="t('body.systemSetting.creator')">
                            </Column>
                            <Column field="role.name" :header="t('body.systemSetting.position')"></Column>
                            <Column header="" class="w-1rem">
                                <template #body="{ data, index }">
                                    <Button @click="onClickDelete(data, index)" icon="pi pi-trash" text
                                        severity="danger" />
                                </template>
                            </Column>
                            <template #footer>
                                <Button @click="onClickAddUser" :label="t('body.systemSetting.add_creator')" />
                            </template>
                            <template #empty>
                                <div class="text-center p-5 m-5">
                                    {{ t('body.systemSetting.no_creator_yet') }}
                                </div>
                            </template>
                        </DataTable>
                    </TabPanel>
                    <!-- Chứng từ - approvalSampleDocumentsLines -->
                    <TabPanel :header="t('body.systemSetting.documents')">
                        <div class="h-7rem flex mt-2 gap-4">
                            <div v-for="(item, index) in Docs" :key="index">
                                <Checkbox v-model="item.selected" binary :trueValue="item.transType" :falseValue="null"
                                    @change="onChangeCheckBox(item, $event)" />
                                <label class="ml-2">{{ item.name }}</label>
                            </div>
                        </div>
                    </TabPanel>
                    <TabPanel :header="t('body.systemSetting.process')">
                        <AddApprovalLevelComp :validate="submited" :type="'ApprovalTemplates'" :data="formData">
                        </AddApprovalLevelComp>
                    </TabPanel>
                    <TabPanel :header="t('body.systemSetting.regulations')">
                        <div class="h-10rem flex flex-column gap-3">
                            <label class="mr-2">{{ t('body.systemSetting.start_approval_process') }}</label>
                            <div class="flex gap-2">
                                <Checkbox v-model="formData.isDebtLimit" binary></Checkbox>
                                <label>{{ t('body.systemSetting.debt_limit') }}</label>
                            </div>
                            <div class="flex gap-2">
                                <Checkbox v-model="formData.isOverdueDebt" binary>
                                </Checkbox>
                                <label>{{ t('body.systemSetting.overdue_debt') }}</label>
                            </div>
                            <div class="flex gap-2">
                                <Checkbox v-model="formData.isOther" binary>
                                </Checkbox>
                                <label>{{ t('body.systemSetting.other') }}</label>
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
            </div>
            <div class="flex align-items-center">
                <label class="mr-4">{{ t('body.systemSetting.status') }}</label>
                <InputSwitch v-model="formData.isActive" />
            </div>
        </div>
        <template #footer>
            <div class="flex justify-content-end gap-2 mt-2">
                <Button class="px-4" type="button" :label="t('body.OrderList.close')" severity="secondary"
                    @click="isDialogVisible = false" />
                <Button class="px-4" type="submit" :label="t('body.sampleRequest.importPlan.save_button')"
                    @click="handleSubmit" />
            </div>
        </template>
    </Dialog>
</template>
