<template> 
    <DataTable :value="odStore.order?.attDocuments" show-gridlines>
        <template #header>
            <div class="flex justify-content-between">
                <div class="my-2 text-lg">{{ t('client.delivery_documents') }}</div>
            </div>
        </template>
        <Column header="#" class="w-3rem">
            <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column field="fileName" :header="t('client.document_name')"></Column>
        <Column :header="t('client.created_date')" class="w-15rem">
            <template #body="{ data }">
                {{ format(data['uploadFileAt'], 'dd/MM/yyyy') }}
            </template>
        </Column>
        <Column field="authorName" :header="t('client.creator')" class="w-15rem"></Column>
        <Column class="w-1rem" v-if="odStore.order?.status == 'DXN'">
            <template #body="{ index }">
                <Button @click="odStore.order?.attDocuments.splice(index, 1)" icon="pi pi-trash" text
                    severity="danger"/>
            </template>
        </Column>
        <Column class="w-1rem" v-else>
            <template #body="{ data, index }">
                <div class="flex">
                    <Button icon="pi pi-download" text v-tooltip.bottom="'Tải xuống'"
                        @click="onClickDownloadFile(data)"/> 
                    <Button icon="pi pi-trash" v-if="odStore.order?.status != 'DHT'" text v-tooltip.bottom="'Xóa'"
                        severity="danger" @click="() => {
                            odStore.order?.attDocuments.splice(index, 1)
                        }" />
                </div>
            </template>
        </Column>
        <template #empty>
            <div class="py-5 my-5 text-center font-italic text-500">{{ t('client.no_data') }}</div>
        </template>
        <template #footer v-if="odStore.order?.status === 'DGH' || odStore.order?.status === 'DGHR' && !props.isClient">
            <FileSelect icon="pi pi-plus" label="Thêm tài liệu" :multiple="true" @change="onChangeFile"></FileSelect>
        </template>
    </DataTable>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetail';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const odStore = useOrderDetailStore();

const props = defineProps({
    isClient: {
        default: false
    }
});

const onClickDownloadFile = (data: any) => {
    let a = document.createElement('a');
    a.href = data.filePath;
    a.target = "_blank";
    a.download = data.fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};

const onChangeFile = (files: File[]) => {
    for (const file of files) {
        odStore.order?.attDocuments.push({
            uploadFileAt: new Date().toUTCString(),
            _file: file,
            fileName: file.name
        });
    }
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
