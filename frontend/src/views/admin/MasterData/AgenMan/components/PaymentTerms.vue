<template>
    <div id="section4" class="card">
        <div class="flex justify-content-between">
            <div class="py-2 text-green-700 text-xl font-semibold">
                {{ t('body.sampleRequest.customer.credit_limit_title') }}
            </div>
            <div class="flex gap-3">
                <Button v-if="!editMode" :disabled="editMode" @click="editMode = true" icon="pi pi-pencil"
                    :label="t('body.sampleRequest.customer.edit_button')" text/>
                <div v-else class="flex gap-3">
                    <Button @click="onClickCancel()" icon="pi pi-times" :label="t('body.status.HUY2')" text/>
                    <Button :label="t('client.save')" @click="save()" icon="pi pi-pencil"/>
                </div>
            </div>
        </div>
        <hr class="my-2" />
        <DataTable :value="dataEdit">
            <Column header="#">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="paymentMethodName" :header="t('body.sampleRequest.customer.debt_form_column')"></Column>
            <Column field="balance" :header="t('body.sampleRequest.customer.credit_limit_column')">
                <template v-if="editMode" #body="{ data }">
                    <InputNumber type="number" v-model="data.balance" :pt:input:root:class="'w-13rem text-right'"
                        :min="0" />
                </template>
                <template #body="{ data }">
                    <span>{{ data.balance == null ? "Không có hạn mức" : formater.FormatCurrency(data.balance) }}</span>
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.guarantee_bank_column')" field="bankGuarantee">
                <!-- <template v-if="editMode" #body="{ data }">
                    <InputText :disabled="!editMode" v-model="data.bankGuarantee" />
                </template> -->
                <template #body="{ data }">
                    <span>--</span>
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.start_date_column')" field="startLetterOfGuarantee">
                <template #body="{ data }">
                    {{ data.startLetterOfGuarantee ? format(data.startLetterOfGuarantee, "dd/MM/yyyy") : "--" }}
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.end_date_column')" field="letterOfGuarantee">
                <template #body="{ data }">
                    {{ data.letterOfGuarantee ? format(data.letterOfGuarantee, "dd/MM/yyyy") : "--" }}
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.payment_terms_column')" field="payment_term">
                <!-- <template v-if="editMode" #body="{ data }">
                    <InputText :disabled="!editMode" v-model="data.payment_term"></InputText>
                </template> -->
                <template #body="{ data }">
                    --
                </template>
            </Column>
            <template #empty>
                <div class="p-5 text-center">{{ t('body.sampleRequest.customer.no_data_message') }}</div>
            </template>
            <template v-if="editMode && 0" #footer>
                <Button label="Thêm dòng" @click="addRow()" icon="pi pi-plus" outlined/>
            </template>
        </DataTable>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import formater from "../../../../../helpers/format.helper";
import { format } from "date-fns";
import { useRoute } from "vue-router";
import { useI18n } from 'vue-i18n';
import API from "@/api/api-main";
const { t } = useI18n();
const route = useRoute();
const editMode = ref(false);
const dataEdit = ref([]);

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
const onClickCancel = () => {
    editMode.value = false;
};
const addRow = () => {
    let payload = {
        credit_limit: 0,
        bank_guarantee: "",
        start_time: new Date(),
        end_time: new Date(),
        payment_term: "",
    };
    dataEdit.value.push(payload);
};
const fetchData = ref([]);
const save = async () => {
    try {
        fetchData.value = JSON.parse(JSON.stringify(dataEdit.value));
        await API.update(
            `Customer/${props.setup.modelStates.id}/crd3`,
            fetchData.value
        );
        props.setup.toast.add({
            severity: "success",
            summary: t('body.systemSetting.success_label'),
            detail: t('body.systemSetting.update_account_success_message'),
            life: 3000,
        });
        editMode.value = false;
    } catch (error) {
        console.error(error);
    }
};

onMounted(() => {
    if (route.params.id) {
        API.get(`Customer/${route.params.id}/debt`).then((res) => {
            if (res.status === 200) {
                dataEdit.value = res.data;
            }
        });
    }
});
</script> 
