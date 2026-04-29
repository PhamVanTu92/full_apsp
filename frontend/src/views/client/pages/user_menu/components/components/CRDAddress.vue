<template>
    <div class="card p-0">
        <div class="flex justify-content-between p-3">
            <div class="text-primary text-xl font-semibold my-auto">
                {{ props.header }}
            </div>
            <Button v-if="!enableEdit" :disabled="enableEdit" @click="onClickEdit" icon="pi pi-pencil"
                :label="t('client.edit')"/>
            <div v-if="enableEdit" class="flex gap-3">
                <Button @click="onClickCancelEdit" icon="pi pi-times" :label="t('client.cancel')"
                    severity="secondary" />
                <Button :loading="loading" @click="onClickSave" icon="pi pi-save" :label="t('client.save')" />
            </div>
        </div>
        <hr class="m-0" />
        <div class="p-3"> 
            <DataTable :value="enableEdit ? dataTable.filter((el) => el.status != 'D') : props.data
                " showGridlines scrollable scrollHeight="300px">
                <Column v-for="(column, i) in props.columns" :key="i" :field="column.field" :header="column.header">
                    <template #body="sp">
                        <span v-if="!enableEdit">
                            {{ sp.data[column.field] }}
                        </span>
                        <SelectAddress v-model="sp.data" @select="onSelectAddress($event, sp.index)"
                            v-else-if="column.field == 'full_address'" :invalid="sp.data.error?.[column.field]" />
                        <InputText v-else v-model="sp.data[column.field]" class="w-full"
                            :placeholder="column.placeholder" :invalid="sp.data.error?.[column.field]" />
                        <small v-if="sp.data.error?.[column.field]" class="text-red-500">{{
                            sp.data.error?.[column.field] }}</small>
                    </template>
                </Column>
                <Column :header="t('body.sampleRequest.customer.default_column')"
                    style="width: 3rem; text-align: center">
                    <template #body="sp">
                        <RadioButton v-model="sp.data.default" :disabled="!enableEdit || !sp.data.id" :value="'Y'"
                            @change="onChangeDefault(sp.data.id)" />
                    </template>
                </Column>
                <Column style="width: 3rem" v-if="enableEdit">
                    <template #body="sp">
                        <Button @click="removeRow(sp)" icon="pi pi-trash" severity="danger" text />
                    </template>
                </Column>
                <template v-if="enableEdit" #footer>
                    <Button @click="appendRow" icon="pi pi-plus-circle"
                        :label="t('body.systemSetting.add_new_approval_button')" outlined />
                </template>
                <template #empty>
                    <div class="p-6 m-6 text-center">{{ t('client.no_data') }}</div>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<script setup>
import { computed, ref } from "vue";
import { useToast } from "primevue/usetoast";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const toast = useToast();
const loading = ref(false);
const emits = defineEmits(["on-submit"]);
const props = defineProps({
    header: "",
    crd: null,
    data: {
        default: [],
    },
    columns: {
        default: [],
    },
    id: null,
    type: {
        required: true,
        default: "B", // B: Địa chỉ xuất hoá đơn, S: Địa chỉ giao hành
    },
});

const onSelectAddress = (event, index) => {
    const filteredData = dataTable.value.filter((el) => el.status != 'D');
    const targetRow = filteredData[index]; 
    if (targetRow) {
        Object.assign(targetRow, {
            areaId: event.areaId,
            areaName: event.areaName,
            locationId: event.locationId,
            locationName: event.locationName,
            address: event.address,
            full_address: [event.address, event.locationName, event.areaName]
                .filter(Boolean)
                .join(", ")
        });
    }
};

const validate = () => {
    let errorCount = 0;
    dataTable.value.forEach((row) => {
        row.error = {};
        for (const col of props.columns) {
            if (col.required) {
                if (!row[col.field]?.trim()) {
                    row.error[col.field] = `${col.header}` + t('Notification.required_mess');
                    errorCount++;
                } else {
                    if (col.regex) {
                        if (!row[col.field]?.trim().match(col.regex)) {
                            row.error[col.field] = `${col.header}` + t('Notification.required_mess_err');
                            errorCount++;
                        }
                    }
                }
            }
        }
    });
    return errorCount < 1;
};

const onClickSave = () => {
    if (!validate()) return;
    try {
        const formData = new FormData();
        const payload = {};
        payload.id = props.id;  
        payload[props.crd] = dataTable.value.map(item => ({
            ...item,
            type: props.type
        })); 
        formData.append("item", JSON.stringify(payload));
        loading.value = true;
        API.update("customer/me", formData)
            .then((res) => {
                if (res.status == 200) {
                    toast.add({
                        severity: "success",
                        summary: t('body.systemSetting.success_label'),
                        detail: t('client.update_success'),
                        life: 3000,
                    });
                    loading.value = false;
                    enableEdit.value = false;
                    emits("on-submit", res.data);
                }
            })
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: t('Custom.titleMessageInfo'),
                    detail: t('client.update_failed'),
                    life: 3000,
                });
                loading.value = false;
                console.error(error);
            });
    } catch (error) {
        toast.add({
            severity: "error",
            summary: t('Custom.titleMessageInfo'),
            detail: t('client.update_failed'),
            life: 3000,
        });
        loading.value = false;
        console.error(error);
    }
};

const onChangeDefault = (id) => {
    dataTable.value = dataTable.value.map((el) => ({ ...el, default: "N" }));
    const row = dataTable.value.find((el) => el.id == id);
    if (row) {
        row.default = "Y";
    }
};

const appendRow = () => {
    dataTable.value.push({
        status: "A",
    });
};

const removeRow = (sp) => {
    if (sp.data.id) {
        sp.data.status = "D";
    } else {
        dataTable.value.splice(sp.index, 1);
    }
};

const dataTable = ref([]);
const enableEdit = ref(false);
const onClickEdit = () => {
    dataTable.value = JSON.parse(JSON.stringify(props.data));
    dataTable.value = dataTable.value.map((el) => ({ ...el, status: "U" }));
    if (dataTable.value.length < 1) {
        appendRow();
    }
    enableEdit.value = true;
};
const onClickCancelEdit = () => {
    dataTable.value = [];
    enableEdit.value = false;
};
</script>

<style></style>
