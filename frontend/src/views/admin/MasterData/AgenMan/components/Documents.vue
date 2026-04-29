<template>
    <div id="section5" class="card">
        <div class="flex justify-content-between">
            <div class="py-2 text-green-700 text-xl font-semibold">{{t('client.documents')}}</div>
            <Button
                v-if="!editMode"
                @click="onClickEdit"
                icon="pi pi-pencil"
                :label="t('client.edit')"
                text
            />
            <div class="flex gap-3" v-else>
                <Button
                    @click="onClickCancel"
                    icon="pi pi-times"
                    :label="t('body.status.HUY2')"
                    text
                />
                <Button
                    @click="onClickSaveChange"
                    icon="pi pi-check"
                    :label="t('client.save')"
                />
            </div>
        </div>
        <hr class="my-2" />
        <DataTable :value="dataTable.filter((el) => el.status != 'D')">
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column :header="t('client.document_name')" class="w-20rem">
                <template #body="{ data, index }">
                    <div v-if="data.id">
                        <Button
                            icon="pi pi-file"
                            :label="data.fileName"
                            text
                            @click="onClickDownload(url, data.fileName)"
                        >
                        </Button>
                    </div>
                    <div v-else>
                        <Button
                            icon="pi pi-file"
                            class=""
                            :label="data.file ? data.file.name : t('body.systemSetting.choose_file')"
                            @click="onClickChoseFile(index)"
                            pt:label:class="max-w-10rem"
                        />
                    </div>
                    <input
                        class="hidden"
                        :id="`file${index}`"
                        @change="onFileChanged($event, data)"
                        type="file"
                    />
                </template>
            </Column>
            <Column :header="t('client.notes')" class="w-20rem">
                <template #body="{ data }">
                    <InputText
                        v-if="editMode"
                        v-model="data.note"
                        :placeholder="t('client.enter_note')"
                        class="w-full"
                        @input="onChangeNote(data)"
                    ></InputText>
                    <div v-else>
                        {{ data.note }}
                    </div>
                </template>
            </Column>
            <Column field="authorName" :header="t('client.created_by')"></Column>
            <Column :header="t('client.created_time')">
                <template #body="{ data }">
                    <div v-if="data.id">
                        {{ format(data.createdDate, "dd/MM/yyyy") }}
                    </div>
                </template>
            </Column>
            <Column class="w-3rem" v-if="editMode">
                <template #body="sp">
                    <Button
                        icon="pi pi-trash"
                        text
                        severity="danger"
                        @click="onClickRemove(sp)"
                    />
                </template>
            </Column>
            <template #empty>
                <div class="p-5 text-center">{{ t('client.no_data_to_display') }}</div>
            </template>
            <template #footer v-if="editMode">
                <Button
                    @click="onClickAddRow"
                    icon="pi pi-plus"
                    :label="t('client.add_document')"
                    outlined
                />
            </template>
        </DataTable>
        <!-- {{ dataTable }} -->
    </div>
</template>

<script setup>
import { format } from "date-fns";
import API from "@/api/api-main";
import { onMounted, ref } from "vue";
import { useToast } from "primevue/usetoast";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const editMode = ref(false);
const dataTable = ref([]);
const toast = useToast();
const props = defineProps({
    setup: {
        API: {
            type: Object,
            required: true,
        },
        modelStates: {
            type: Object,
            required: true,
        },
    },
});

const onClickEdit = () => {
    editMode.value = true;
    if (dataTable.value.length < 1) {
        dataTable.value.push({ ...row });
    }
};

const onClickAddRow = () => {
    dataTable.value.push({ ...row });
};

const row = {
    id: 0,
    note: null,
    file: null,
};

const onClickSaveChange = async () => {
    let url = `customer/${props.setup.modelStates.id}/files`;

    let error = 0;
    if (dataTable.value?.length) {
        const dataAdds = dataTable.value.filter((row) => row.id == 0);
        const dataUpdate = dataTable.value.filter((row) => row.status == "U");
        const dataDelete = dataTable.value.filter((row) => row.status == "D");
        if (dataAdds.length > 0) {
            if (dataAdds.filter((el) => !el.file).length >= 1) {
                toast.add({
                    severity: "warn",
                    summary: "Cảnh báo",
                    detail: "Vui lòng chọn ít nhất một file để thêm tài liệu",
                    life: 3000,
                });
                return;
            }
            const formData = new FormData();
            const notes = [];
            dataAdds.forEach((item, index) => {
                formData.append("files", item.file);
                notes.push(item.note?.trim());
            });
            formData.append("notes", JSON.stringify(notes));
            try {
                await API.add(url, formData);
            } catch (e) {
                error++;
            }
        }
        if (dataUpdate.length > 0) {
            const data = [];
            dataUpdate.forEach((item, index) => {
                let { status, author, authorId, authorName, ...row } = item;
                row.note = row.note?.trim() || null;
                data.push(row);
            });
            try {
                await API.update(url, data);
            } catch (e) {
                error++;
            }
        }
        if (dataDelete.length > 0) {
            const data = [];
            dataDelete.forEach((item, index) => {
                data.push(item.id);
            });
            try {
                await API.delete(url, data);
            } catch (e) {
                error++;
            }
        }
    }
    if (error > 0) {
        toast.add({
            severity: "error",
            summary: "Lỗi",
            detail: "Có lỗi xảy ra khi lưu dữ liệu",
            life: 3000,
        });
    } else {
        toast.add({
            severity: "success",
            summary: "Thành công",
            detail: "Cập nhật dữ liệu thành công",
            life: 3000,
        });
    }
    API.get(`customer/${props.setup.modelStates.id}`)
        .then((res) => {
            dataTable.value = unproxy(res.data.crD6);
            editMode.value = false;
        })
        .catch((error) => {
            console.error(error);
        });
};

const onClickCancel = () => {
    dataTable.value = unproxy(props.setup.modelStates?.crD6);
    editMode.value = false;
};

const onClickChoseFile = (i) => {
    let inputFile = document.getElementById(`file${i}`);
    inputFile.click();
};

const onFileChanged = (event, row) => {
    row.file = event.target.files[0];
};

const onClickDownload = (url, name) => {
    var link = document.createElement("a");
    link.download = name;
    link.href = url;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

const onClickRemove = (sp) => {
    if (sp.data.id) {
        sp.data.status = "D";
    } else {
        const dl = dataTable.value.filter((el) => el.status == "D").length;
        dataTable.value.splice(sp.index + dl, 1);
    }
};

const onChangeNote = (row) => {
    if (row.id && !row.status) {
        row.status = "U";
    }
};

onMounted(() => {
    if (props.setup.modelStates.crD6?.length) {
        dataTable.value = unproxy(props.setup.modelStates.crD6);
    }
});

const unproxy = (obj) => {
    return JSON.parse(JSON.stringify(obj));
};
</script>

<style>
.max-w-10rem {
    max-width: 10rem;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
</style>
