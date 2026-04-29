<template>
    <div>
        <div class="flex justify-content-between align-items-center mb-3">
            <div class="font-bold text-2xl">{{ t('client.documents') }}</div>
            <Button :label="t('client.add_document')" icon="pi pi-plus" @click="onClickOpenAdd" />
        </div>
        <div class="card">
            <DataTable :value="dataTable" showGridlines="" paginator rows="5">
                <Column header="#" class="w-3rem">
                    <template #body="sp">
                        {{ sp.index + 1 }}
                    </template>
                </Column>
                <Column field="fileName" :header="t('client.document_name')">
                    <template #body="{ data }">
                        <Button
                            icon="pi pi-file"
                            :label="data.fileName"
                            @click="downloadFile(data.fileUrl, data.fileName)"
                            text
                        />
                    </template>
                </Column>
                <Column field="note" :header="t('client.note')"></Column>
                <Column field="authorName" :header="t('client.creator')"></Column>
                <Column field="createdDate" :header="t('client.created_time')">
                    <template #body="{ data }">
                        <div v-if="data.createdDate">
                            {{ format(data.createdDate, "dd/MM/yyyy") }}
                        </div>
                    </template>
                </Column>
                <Column field="" :header="t('body.sampleRequest.importPlan.actions')" class="w-9rem">
                    <template #body="sp">
                        <Button
                            @click="onClickEdit(sp.data)"
                            icon="pi pi-pencil"
                            text
                        />
                        <Button
                            @click="onClickDelete(sp.data)"
                            icon="pi pi-trash"
                            text
                            severity="danger"
                        />
                    </template>
                </Column>
                <template #empty>
                    <div class="my-5 py-5 text-center font-italic text-gray-500">
                        {{ t('client.no_data') }}
                    </div>
                </template>
            </DataTable>
        </div>
        <Dialog
            @hide="onClose"
            v-model:visible="visible"
            :header="t('client.add_document')"
            class="w-3"
            modal
        >
            <div class="flex flex-column gap-3">
                <InputGroup @click="onClickOpenFile">
                    <InputText
                        :disabled="model.id"
                        :placeholder="t('body.systemSetting.choose_file')"
                        readonly
                        :value="model.file?.name"
                        class="cursor-pointer"
                    ></InputText>
                    <Button icon="pi pi-file" severity="secondary"/>
                </InputGroup>
                <Textarea
                    v-model="model.note"
                    :placeholder="t('client.note')"
                    class="w-full"
                    rows="3"
                />
            </div>
            <template #footer>
                <Button
                    :label="t('client.cancel')"
                    icon="pi pi-times"
                    @click="visible = false"
                    severity="secondary"
                />
                <Button
                    :loading="loading"
                    :label="t('client.save')"
                    icon="pi pi-save"
                    @click="onClickSave"
                />
            </template>
        </Dialog>
        <ConfirmDialog></ConfirmDialog>
        <input
            ref="inputFile"
            type="file"
            class="hidden"
            size="100"
            @change="onChangeFile"
        />
    </div>
</template>

<script setup>
import { ref, reactive, onBeforeMount } from "vue";
import { useConfirm } from "primevue/useconfirm";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { format } from "date-fns";
import { cloneDeep } from "lodash";
import { useI18n } from "vue-i18n";

const { t} = useI18n();

const toast = useToast();
const confirm = useConfirm();
const me = ref({});
const loading = ref(false);
const inputFile = ref();
const dataTable = ref([{}]);
const visible = ref(false);
const model = ref({
    id: 0,
    file: null,
    note: "",
});

const onClickEdit = (data) => {
    model.value = cloneDeep(data);
    model.value.file = {
        name: data.fileName,
    };
    visible.value = true;
};

const onClickOpenAdd = () => {
    visible.value = true;
};
const onClickOpenFile = () => {
    if (!model.value.id) inputFile.value.click();
};
const minSize = 50 * 1000000;
const onChangeFile = (event) => {
    if (event.target.files.length > 0) {
        if (event.target.files[0].size > minSize) {
            alert("Dung lượng tệp tin quá lớn (tối đa 50MB)");
            return;
        }
        model.value.file = event.target.files[0];
    }
};

const onClickSave = () => {
    if (!model.value.file) {
        alert("Vui lòng chọn tài liệu");
        return;
    }
    loading.value = true;
    const formData = new FormData();
    formData.append("files", model.value.file);
    formData.append("notes", JSON.stringify([model.value.note?.trim()]));
    if (model.value.id) {
        const payload = {
            id: model.value.id,
            note: model.value.note?.trim(),
        };
        API.update(`customer/${me.value.id}/files`, [payload])
            .then((res) => {
                toast.add({
                    severity: "success",
                    summary: t('body.systemSetting.success_label'),
                    detail: t('client.update_success'),
                    life: 3000,
                });
                fetchMe();
                loading.value = false;
                visible.value = false;
            })
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: t('body.report.error_occurred_message'),
                    detail: t('body.report.error_occurred_message'),
                    life: 3000,
                });
                loading.value = false;
            });
    } else {
        API.add(`customer/${me.value.id}/files`, formData)
            .then((res) => {
                toast.add({
                    severity: "success",
                    summary: t('body.systemSetting.success_label'),
                    detail: t('client.update_success'),
                    life: 3000,
                });
                fetchMe();
                loading.value = false;
                visible.value = false;
            })
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: t('body.report.error_occurred_message'),
                    detail: t('body.report.error_occurred_message'),
                    life: 3000,
                });
                loading.value = false;
            });
    }
};

const onClickDelete = (data) => {
    confirm.require({
        message: "Bạn có muốn xoá tài liệu này không",
        header: t('client.confirm'),
        icon: "pi pi-exclamation-triangle",
        rejectClass: "p-button-secondary",
        acceptClass: "p-button-danger",
        rejectLabel: t('client.cancel'),
        acceptLabel: t('body.OrderList.delete'),
        accept: () => {
            deleteRow(data.id);
        },
    });
};

const deleteRow = (id) => {
    API.delete(`customer/${me.value.id}/files`, [id])
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('client.update_success'),
                life: 3000,
            });
            fetchMe();
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
        });
};

const onClose = () => {
    model.value = {
        id: 0,
        file: null,
        note: "",
    };
};
import { useMeStore } from "../../../../../Pinia/me";
const meStore = useMeStore();
const fetchMe = async () => {
    const meData = await meStore.getMe();
    if (meData) {
        me.value = meData.user.bpInfo;
        dataTable.value = me.value.crD6 || [];
    }
};

const downloadFile = (url, name) => {
    var link = document.createElement("a");
    link.download = name;
    link.target = "_blank";
    link.href = url;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

onBeforeMount(() => {
    fetchMe();
});
</script>

<style></style>
