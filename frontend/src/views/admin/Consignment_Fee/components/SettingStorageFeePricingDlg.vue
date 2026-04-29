<script setup>
import { ref, reactive, onBeforeMount, watchEffect } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";

const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();

const dataFeeLevel = ref([]);
const editingRows = ref([]);
const loading = ref(false);
const submited = ref(false);
const emits = defineEmits(["updateData"]);
const visible = defineModel("visible", {
    default: false,
});

const onClickAppendRow = () => {
    dataFeeLevel.value.push({
        id: 0,
        index: dataFeeLevel.value.length,
        name: "",
        fromDays: 0,
        toDays: 0,
        status: "A",
        init: true,
    });
};
const onClickDeleteRow = (sp) => {
    if (sp.data.id) {
        sp.data.status = "D";
    } else {
        dataFeeLevel.value.splice(sp.index, 1);
    }
};
const confirmSave = async () => {
    submited.value = true;
    if (validate()) {
        return FunctionGlobal.$notify("E", "Vui lòng nhập đủ thông tin", toast);
    }
    loading.value = true;
    try {
        const res = await API.add(`FeeLevel/add`, dataFeeLevel.value);
        dataFeeLevel.value = res.data;
        FunctionGlobal.$notify("S", "Cập nhật thành công", toast);
        visible.value = false;
        emits("updateData");
    } catch (error) {
        FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
        loading.value = false;
        submited.value = false;
    }
};
const getFeeLevel = async () => {
    try {
        loading.value = true;
        const { data } = await API.get(`FeeLevel`);
        dataFeeLevel.value = data.items;
        dataFeeLevel.value.forEach((el, index) => {
            el.index = index;
        });
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};
const CloseDialog = () => {
    visible.value = false;
};

const validate = () => {
    for (let i = 0; i < dataFeeLevel.value.filter((el) => el.status != "D").length; i++) {
        const el = dataFeeLevel.value[i];
        if (!el.name) return true;
    }
    return false;
};
const onRowEditSave = (event) => {
    let { newData, index } = event;
    if (newData.id) newData.status = "U";
    if (!newData.id) newData.status = "A";
    dataFeeLevel.value[index] = newData;
};

const onRowEditInit = (event) => {
    event.data.init = false;
};
watchEffect(() => {
    if (visible.value) getFeeLevel();
});
</script>

<template>
    <Dialog
        v-model:visible="visible"
        modal
        :header="t('body.sampleRequest.warehouseFee.table_header_fee_milestone')"
        style="width: 80rem"
    >
        <div class="card p-1">
            <DataTable
                v-model:editingRows="editingRows"
                :value="dataFeeLevel.filter((el) => el.status != 'D')"
                tableStyle="min-width: 50rem"
                showGridlines
                dataKey="index"
                editMode="row"
                @row-edit-save="onRowEditSave"
                @row-edit-init="onRowEditInit"
            >
                <Column header="#">
                    <template #body="{ index }">
                        <span>{{ index + 1 }}</span>
                    </template>
                </Column>
                <Column field="name" :header="t('body.sampleRequest.warehouseFee.table_header_fee_milestone')">
                    <template #body="sp">
                        <span>{{ sp.data.name }}</span>
                        <span v-if="sp.data.init">{{ sp.editorInitCallback() }}</span>
                    </template>
                    <template #editor="{ data, field }">
                        <InputText
                            v-model="data[field]"
                            :invalid="submited && !data[field]"
                        />
                    </template>
                </Column>
                <Column field="fromDays" :header="t('body.sampleRequest.warehouseFee.from_date_label')">
                    <template #body="{ data }">
                        <span v-if="data.fromDays">{{ t('body.sampleRequest.warehouseFee.from_date_label') }} {{ data.fromDays }}</span>
                    </template>
                    <template #editor="{ data, field }">
                        <InputNumber v-model="data[field]" />
                    </template>
                </Column>
                <Column field="toDays" :header="t('body.sampleRequest.warehouseFee.to_date_label')">
                    <template #body="{ data }">
                        <span v-if="data.toDays">{{ t('body.sampleRequest.warehouseFee.to_date_label') }} {{ data.toDays }}</span>
                    </template>
                    <template #editor="{ data, field }">
                        <InputNumber v-model="data[field]" />
                    </template>
                </Column>
                <Column
                    :rowEditor="true"
                    style="width: 10%; min-width: 8rem"
                    :header="t('body.sampleRequest.warehouseFee.actions')"
                >
                    <template #body="sp">
                        <Button
                            @click="sp.editorInitCallback()"
                            icon="pi pi-pencil"
                            text
                            severity="secondary"
                        />
                        <Button
                            icon="pi pi-trash"
                            @click="onClickDeleteRow(sp)"
                            text
                            severity="danger"
                        />
                    </template>
                    <template #editor="sp">
                        <Button
                            @click="sp.editorSaveCallback()"
                            icon="pi pi-check"
                            text
                            severity="secondary"
                        />
                        <Button
                            @click="sp.editorCancelCallback()"
                            icon="pi pi-times"
                            text
                            severity="secondary"
                        />
                    </template>
                </Column>
            </DataTable>
        </div>
        <template #footer>
            <div class="flex justify-content-between w-full">
                <Button @click="onClickAppendRow" :label="t('body.systemSetting.add_new_button')"/>
                <div>
                    <Button
                        :label="t('body.sampleRequest.importPlan.cancel_button')"
                        severity="secondary"
                        @click="CloseDialog()"
                    />
                    <Button
                        @click="confirmSave()"
                        :label="t('body.systemSetting.save_button')"
                        class="ml-2"
                        :loading="loading"
                    />
                </div>
            </div>
        </template>
    </Dialog>
    <Loading v-if="loading"></Loading>
</template>
<style scoped></style>
