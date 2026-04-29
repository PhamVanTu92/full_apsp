<template>
    <div ref="fileArea">
        <DataTable :value="files" showGridlines="" stripedRows="">
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="name" :header="t('common.document_name')">
                <template #body="{ data }">
                    <div class="flex gap-2 align-items-center">
                        <i class="pi pi-file text-lg"></i>
                        {{ data.name }}
                    </div>
                </template>
            </Column>
            <Column header="" class="w-3rem">
                <template #body="{ data, index }">
                    <div class="text-center">
                        <Button
                            @click="removeDoc(index)"
                            icon="pi pi-trash"
                            text
                            severity="danger"
                        />
                    </div>
                </template>
            </Column>
            <!-- <template #footer>
            </template> -->
            <template #header>
                <div class="flex justify-content-between">
                    <div class="my-auto">{{ props.header }}</div>
                    <Button
                        @click="onClickAppend"
                        icon="pi pi-plus"
                        :label="t('common.btn_add_document')"
                    />
                </div>
            </template>
            <template #empty>
                <div class="text-center py-5 my-5 text-500 font-italic">
                    {{ t('common.no_document') }}
                </div>
            </template>
        </DataTable>
        <input
            @change="onChangeInputFile"
            ref="inputFile"
            type="file"
            style="display: none"
        />
    </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const emits = defineEmits(["change"]);

const props = defineProps({
    header: {
        type: String,
    },
});

const inputFile = ref();
const fileArea = ref();

const files = ref<Array<any>>([]);

const onClickAppend = () => {
    inputFile.value.click();
};

const onChangeInputFile = (event) => {
    if (event.target.files.length > 0) {
        const doc = {
            file: event.target.files[0],
            name: event.target.files[0].name,
        };
        files.value.push(doc);
        emits("change", files.value);
    }
};

const removeDoc = (index) => {
    files.value.splice(index, 1);
    emits("change", files.value);
};
</script>

<style></style>
