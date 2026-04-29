<template>
    <Dialog
        @hide="onHide"
        v-model:visible="visible"
        :header="model.id ? t('body.systemSetting.unit_info_update') : t('body.systemSetting.add_new_unit')"
        modal
        class="w-4"
    >
        <div class="grid mx-0">
            <div class="col-12 md:col-4 pb-0">
                <div class="flex flex-column field mb-0">
                    <label class="font-semibold" for="">{{ t('body.systemSetting.unit_code_label') }}</label>
                    <InputText
                        placeholder="Mã đơn vị tự sinh"
                        :invalid="errorMessage.code ? true : false"
                        v-model="model.code"
                    ></InputText>
                    <small class="text-red-500" v-if="errorMessage.code">{{
                        errorMessage.code
                    }}</small>
                </div>
            </div>
            <div class="col-12 md:col-8 pb-0">
                <div class="flex flex-column field mb-0">
                    <label class="font-semibold" for="">{{ t('body.systemSetting.unit_name') }}</label>
                    <InputText
                        :invalid="errorMessage.name ? true : false"
                        v-model="model.name"
                    ></InputText>
                    <small class="text-red-500" v-if="errorMessage.name">{{
                        errorMessage.name
                    }}</small>
                </div>
            </div>
            <div class="col-12 pb-0">
                <div class="flex flex-column field mb-0">
                    <label class="font-semibold" for="">{{ t('body.systemSetting.parent_unit') }}</label>
                    <!-- {{ model.fatherId }} -->
                    <TreeSelect
                        :invalid="errorMessage.parentId ? true : false"
                        v-model="model.parentId"
                        :options="[rootNode, ...props.options]"
                    >
                    </TreeSelect>
                    <small class="text-red-500" v-if="errorMessage.parentId">{{
                        errorMessage.parentId
                    }}</small>
                </div>
            </div>
            <div class="col-12 pb-0">
                <div class="flex flex-column field mb-0">
                    <label class="font-semibold" for="">{{ t('body.systemSetting.note') }}</label>
                    <Textarea
                        v-model="model.description"
                        class="w-full"
                        rows="3"
                    ></Textarea>
                </div>
            </div>
            <div class="col-12 pb-0" v-if="0">
                <div class="flex field gap-3 mb-0">
                    <label class="font-semibold my-auto" for="">{{ t('body.systemSetting.status_label') }}</label>
                    <InputSwitch v-model="model.isActive"></InputSwitch>
                </div>
            </div>
        </div>
        <template #footer>
            <Button
                @click="visible = false"
                icon="pi pi-times"
                :label="t('body.OrderList.close')"
                severity="secondary"
            />
            <Button
                @click="onClickSave"
                icon="pi pi-save"
                :label="t('body.sampleRequest.importPlan.save_button')"
                :loading="props.loading"
            />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch, PropType } from "vue";
import { OrgStruct } from "../script";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const emits = defineEmits(["save"]);

const model = ref<OrgStruct>(new OrgStruct());

const rootNode = {
    key: "null",
    id: null,
    label: "--Không thuộc đơn vị nào--",
    name: "--Không thuộc đơn vị nào--",
    parentId: "null",
};

interface ErrorMessage {
    code?: string;
    name?: string;
    parentId?: string;
}

const props = defineProps({
    data: {
        type: OrgStruct,
        default: () => new OrgStruct(),
    },
    options: {
        type: Array<OrgStruct>,
        default: () => [],
    },
    loading: {
        type: Boolean,
        default: false,
    },
});

const visible = defineModel("visible", {
    type: Boolean,
    default: false,
});

const errorMessage = ref<ErrorMessage>({});

const onClickSave = () => {
    const _payload = new OrgStruct(model.value);
    const validate = _payload.validate();
    if (!validate.result) {
        errorMessage.value = validate.errors;
        return;
    }
    emits("save", model.value);
};

onMounted(function () {});

watch(
    () => visible.value,
    (val) => {
        if (val){
            Object.assign(model.value, props.data);
            const fatherId = {};
            fatherId[`${props.data.parentId}`] = true;
            model.value.parentId = fatherId
        }
    }
);

const onHide = () => {
    model.value = new OrgStruct();
    errorMessage.value = {};
};
</script>

<style scoped></style>
