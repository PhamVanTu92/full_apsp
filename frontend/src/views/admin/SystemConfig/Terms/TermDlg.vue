<template>
    <Dialog
        :header="t('body.systemSetting.add_new_term')"
        v-model:visible="visible"
        style="width: 50rem"
        modal
        @hide="onHide"
    >
        <div class="">
            <div class="field">
                <label for="">{{ t('body.systemSetting.term_name') }} <span class="text-red-500">*</span></label>
                <InputText
                    v-model="model.name"
                    :invalid="!!errMsg.name"
                    class="w-full"
                ></InputText>
                <small v-if="!!errMsg.name">{{ errMsg.name }}</small>
            </div>
            <div class="field">
                <label for="">{{ t('body.systemSetting.note') }}</label>
                <Textarea v-model="model.note" class="w-full" :rows="4"></Textarea>
            </div>
            <div class="field">
                <label for="">{{ t('body.systemSetting.document') }} <span class="text-red-500">*</span></label>
                <FileSelect
                    @change="onChangeFile"
                    accept=".pdf,application/pdf"
                    showFileName
                    :invalid="!!errMsg.file"
                ></FileSelect>
                <small v-if="!!errMsg.file">{{ errMsg.file }}</small>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OrderList.close')" @click="visible = false" severity="secondary"/>
            <Button :label="t('body.systemSetting.save_button')" :loading="loading" @click="onSave"/>
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { Validator, ValidateOption } from "../../../../helpers/validate";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const model = reactive({
    name: "",
    note: "",
    file: null as File | null,
});

const emits = defineEmits(["success"]);

const toast = useToast();

const vldOpt: ValidateOption = {
    name: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.term_name_required'),
        },
    },
    file: {
        validators: {
            required: true,
            type: File,
            nullMessage: t('Custom.document_required'),
        },
    },
};

const visible = ref(false);
const errMsg = ref<any>({});
const onSave = () => {
    errMsg.value = {};
    const vresult = Validator(model, vldOpt);
    if (!vresult.result) {
        errMsg.value = vresult.errors;
        return;
    }
    const formData = new FormData();
    for (let key in model) {
        const value = model[key as keyof typeof model];
        if (value) {
            const value = model[key as keyof typeof model];
            if (value !== null) {
                formData.append(key, value as string | Blob);
            }
        }
    }
    loading.value = true;
    API.add("article", formData)
        .then((res) => {
            emits("success");
            visible.value = false;
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: "Đã cập nhật thông tin điều khoản",
                life: 3000,
            });
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                life: 3000,
            });
        })
        .finally(() => {
            loading.value = false;
        });
};

const loading = ref(false);

const onHide = () => {
    errMsg.value = {};
    model.name = "";
    model.note = "";
    model.file = null;
};

const onChangeFile = (files: File[]) => {
    model.file = files[0];
};

const open = () => {
    visible.value = true;
};

defineExpose({
    open,
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
small {
    color: var(--red-500);
}
</style>
