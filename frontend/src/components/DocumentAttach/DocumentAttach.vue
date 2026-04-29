<template>
    <div :class="{ 'border-1 border-red-400': props.invalid }">
        <DataTable :value="modelValue" :showGridlines="props.showGridlines">
            <Column header="#" class="w-1rem">
                <template #body="{ index }">
                    <div>
                        {{ index + 1 }}
                    </div>
                </template>
            </Column>
            <Column :header="t('common.document_name')">
                <template #body="{ data }">
                    <div class="flex gap-2 align-items-center">
                        <i class="pi pi-file text-xl"></i>
                        <div>{{ data[props.fileName] }}</div>
                        <Tag v-if="!data.id" value="mới"></Tag>
                    </div>
                </template>
            </Column>
            <Column class="w-1rem">
                <template #body="sp">
                    <div class="flex">
                        <a :href="sp.data.fileUrl">
                            <Button
                                :disabled="!sp.data.id"
                                icon="pi pi-download"
                                text
                                @click="onClickDownloadFile(sp.data)"
                            />
                        </a>

                        <Button
                            v-if="!props.readonly"
                            @click="onClickDelete(sp)"
                            icon="pi pi-trash"
                            severity="danger"
                            text
                        />
                    </div>
                </template>
            </Column>
            <template #header>
                <div class="flex">
                    <div
                        class="flex-grow-1 flex align-items-center text-lg font-bold"
                        style="height: 33px"
                    >
                        {{ props.header }}
                    </div>
                    <Button
                        v-if="!props.readonly"
                        @click="onClickOpenSelectFile"
                        :label="t('common.document_list')"
                        icon="pi pi-plus"
                    />
                </div>
            </template>
            <template #empty>
                <div class="text-center text-500 my-5 py-5 font-italic">
                    {{ t('common.no_data') }}
                </div>
            </template>
        </DataTable>
        <input
            @change="onChangeInputFile"
            ref="inputFile"
            type="file"
            :multiple="props.multiple"
            class="hidden"
        />
        <ConfirmDialog></ConfirmDialog>
    </div>
</template>

<script setup lang="ts">
import ConfirmDialog from "primevue/confirmdialog";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { ref, reactive, computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const toast = useToast();
// import API from "../../"

const onClickDownloadFile = async (data: any): Promise<void> => {
    return;

    // const a = document.createElement("a");
    // a.href = data.fileUrl;
    // a.download = data.fileName;
    // document.body.appendChild(a);
    // a.click();
    // document.body.removeChild(a);
    const fileUrl = data.fileUrl; // Thay URL file thật vào đây
    const fileName = data.fileName; // Tên file khi tải về

    try {
        const response = await fetch(fileUrl);
        if (!response.ok) {
            toast.add({
                severity: "error",
                summary: t('common.error'),
                detail: t('common.msg_cannot_download'),
                life: 3000,
            });
            throw new Error(t('common.msg_cannot_download'));
        }

        const blob = await response.blob();
        const blobUrl = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = blobUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        // Giải phóng bộ nhớ
        URL.revokeObjectURL(blobUrl);
    } catch (error) {
        toast.add({
            severity: "error",
            summary: t('common.error'),
            detail: t('common.msg_cannot_download'),
            life: 3000,
        });
        console.error(t('common.msg_error_occurred'), error);
    }
};

const onClickDelete = (row: any): void => {
    if (row.id && props.deleteConfirm) {
        confirm.require({
            message: t('common.msg_confirm_delete_document'),
            header: t('common.dialog_confirm_delete'),
            acceptIcon: "pi pi-trash",
            rejectIcon: "pi pi-times",
            rejectClass: "p-button-secondary",
            acceptClass: "p-button-danger",
            rejectLabel: t('common.btn_cancel'),
            acceptLabel: t('common.btn_delete'),
            accept: () => {
                emits("delete", row);
            },
            reject: () => {},
        });
    } else {
        emits("delete", row);
    }
};

const onChangeInputFile = (e: any): void => {
    const files = Array.from(e.target.files);
    const fileList = files.map((file: any) => {
        const row = {
            id: null,
            file: file,
            url: null,
        };
        row[`${props.fileName}`] = file.name;
        return row;
    });
    modelValue.value = [...modelValue.value, ...fileList];
};

const onClickOpenSelectFile = (): void => {
    inputFile.value.click();
};

//------------------------------
const emits = defineEmits(["delete"]);

interface Props {
    header?: string;
    showGridlines?: boolean | undefined;
    multiple?: boolean | undefined;
    fileName?: string;
    deleteConfirm?: boolean | undefined;
    invalid?: boolean | undefined;
    readonly: boolean | undefined;
}
const props = withDefaults(defineProps<Props>(), {
    fileName: () => "name",
    readonly: () => false,
});

const confirm = useConfirm();

const modelValue = defineModel("data", {
    default: () => [] as Array<any>,
});

const inputFile = ref();
onMounted(function () {});
</script>

<style scoped></style>
