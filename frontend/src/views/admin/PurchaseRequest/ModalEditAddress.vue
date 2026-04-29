<template>
    <div>
        <Dialog :header="t('body.PurchaseRequestList.shipping_info_tab')" v-model:visible="visible" modal class="w-30rem" @hide="onHide">
            <div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.contact_person_name_column') }} <span class="p-required">*</span></label>
                    <InputText v-model="model.person" class="w-full" autofocus :invalid="!!errMsg.person"></InputText>
                    <small class="p-required">{{ errMsg.person }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.phone_label') }} <span class="p-required">*</span></label>
                    <InputText v-model="model.phone" class="w-full" :invalid="!!errMsg.phone"></InputText>
                    <small class="p-required">{{ errMsg.phone }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.license_plate_column') }} <span class="p-required">*</span></label>
                    <InputText v-model="model.vehiclePlate" class="w-full" :invalid="!!errMsg.vehiclePlate"></InputText>
                    <small class="p-required">{{ errMsg.vehiclePlate }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.id_card_column') }} <span class="p-required">*</span></label>
                    <InputText v-model="model.cccd" class="w-full" :invalid="!!errMsg.cccd"></InputText>
                    <small class="p-required">{{ errMsg.cccd }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.email_label') }} <span class="p-required">*</span></label>
                    <InputText v-model="model.email" class="w-full" :invalid="!!errMsg.email"></InputText>
                    <small class="p-required">{{ errMsg.email }}</small>
                </div>
                <div class="flex gap-3 align-items-center mb-2">
                    <hr class="flex-grow-1" />
                    <span class="font-semibold">{{ t('body.sampleRequest.customer.address_label') }}</span>
                    <hr class="flex-grow-1" />
                </div>
                <div class="field flex gap-3">
                    <span class="mr-3">{{ t('body.sampleRequest.customer.address_label') }}:</span>
                    <label>
                        <RadioButton v-model="isInternational" :value="false"></RadioButton>
                        <span class="ml-2">{{t('body.sampleRequest.customerGroup.domestic')}}</span>
                    </label>
                    <label>
                        <RadioButton v-model="isInternational" :value="true"></RadioButton>
                        <span class="ml-2">{{t('body.sampleRequest.customerGroup.international')}}</span>
                    </label>
                </div>

                <div class="field" v-if="!isInternational">
                    <label for="">{{ t('body.sampleRequest.customer.area_column') }} <span class="p-required">*</span></label>
                    <AutoComplete v-model="selection.area" class="w-full" pt:input:class="flex-grow-1"
                        optionLabel="name" :suggestions="suggestions.area" @complete="onComplete($event, 'area')"
                        @change="onChange('area')" :invalid="!!errMsg.areaId"></AutoComplete>
                    <small class="p-required">{{ errMsg.areaId }}</small>
                </div>
                <div class="field" v-if="!isInternational">
                    <label for="">{{t('client.ward_commune')}} <span class="p-required">*</span></label>
                    <AutoComplete :disabled="!model.areaId" v-model="selection.location" class="w-full"
                        pt:input:class="flex-grow-1" optionLabel="name" :suggestions="suggestions.location"
                        @complete="onComplete($event, 'location')" @change="onChange('location')"
                        :invalid="!!errMsg.locationId"></AutoComplete>
                    <small class="p-required">{{ errMsg.locationId }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('body.sampleRequest.customer.address_label') }} <span class="p-required">*</span></label>
                    <Textarea v-model="model.address" class="w-full" :invalid="!!errMsg.address"></Textarea>
                    <small class="p-required">{{ errMsg.address }}</small>
                </div>
            </div>
            <template #footer>
                <Button @click="visible = false" severity="secondary" :label="t('body.OrderList.close')"/>
                <Button :label="t('body.systemSetting.save_button')" @click="onClickSave()"/>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from "vue";
import API from "@/api/api-main";
import { Address } from "../../common/Order/types/orderDetail";
import { Validator, ValidateOption, PATTERN } from "@/helpers/validate";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const toast = useToast();
const isInternational = ref(false);
const errMsg = ref<any>({});
const visible = ref(false);
interface Props {
    dataAddress: any,
    visibleModal: boolean,
    paramID: number
}
const props = defineProps<Props>()
const model = ref<Address>({} as Address)
const emits = defineEmits(["change"]);

watch(props, (newVal, oldVal) => { 
    const firstAddress = newVal.dataAddress?.address?.[0] || { } as Address;
    model.value = { ...firstAddress };
    selection.area.id = firstAddress.areaId;
    selection.area.name = firstAddress.areaName;
    selection.location.id = firstAddress.locationId;
    selection.location.name = firstAddress.locationName;
    visible.value = newVal.visibleModal

})
watch(visible, (newVal, oldVal) => {
    if (newVal == false)
        emits('change', newVal)
})

const selection = reactive({
    area: { id: null as any, name: null as any },
    location: { id: null as any, name: null as any },
});

const suggestions = reactive({
    area: [] as any[],
    location: [] as any[],
});

const onHide = () => {
    isInternational.value = false;
    errMsg.value = {};

};

const onComplete = (event: any, path: "area" | "location") => {
    const search = encodeURIComponent(event.query?.trim());
    switch (path) {
        case "area":
            API.get(`Area/search/${search}`).then((res) => {
                suggestions.area = res.data;
            });
            break;
        case "location":
            if (model.value.areaId) {
                API.get(`Area/search/${model.value.areaId}/${search}`).then((res) => {
                    suggestions.location = res.data;
                });
            } else {
                suggestions.location = [];
            }
            break;
        default:
            break;
    }
};

const onChange = (type: "area" | "location") => {
    if (type == "area") {
        model.value.areaId = selection.area.id;
        model.value.areaName = selection.area.name;
        model.value.locationId = 0;
        model.value.locationName = "";
        selection.location = { id: 0, name: "" };
    } else if (type == "location") {
        model.value.locationId = selection.location.id;
        model.value.locationName = selection.location.name;
    }
};
const S_validateOpt: ValidateOption<Address> = {
    person: {
        validators: {
            required: false,
            type: String,
            nullMessage: t('Custom.contact_name_required'),
        },
    },
    phone: {
        validators: {
            required: false,
            type: String,
            nullMessage:  t('Custom.phone_required'),
        },
    },
    cccd: {
        validators: {
            required: false,
            type: String,
            pattern: PATTERN.identityId,
            nullMessage:  t('Custom.id_required'),
        },
    },
    address: {
        validators: {
            required: false,
            type: String,
            nullMessage: t('Custom.address_required'),
        },
    },
    email: {
        validators: {
            type: String,
            pattern: PATTERN.email,
            patternMessage: t('Custom.email_invalid'),
        },
    },
};

const onClickSave = () => {
    const vldOpt = S_validateOpt;
    if (!isInternational.value) {
        vldOpt.areaId = {
            validators: {
                required: true,
                type: Number,
                nullMessage: t('client.select_area'),
            },
        };
        vldOpt.locationId = {
            validators: {
                required: true,
                type: Number,
                nullMessage:  t('client.ward_commune'),
            },
        };
    } else {
        delete vldOpt.areaId;
        delete vldOpt.locationId;
    }
    errMsg.value = {};
    const vresult = Validator(model.value, vldOpt);
    if (!vresult.result) {
        errMsg.value = vresult.errors;
        return;
    }
    let arrB = { ...props.dataAddress.address[1] }
    delete arrB.id;

    const payload = [
        { 
            "address": model.value.address,
            "locationId": model.value.locationId,
            "locationName": model.value.locationName,
            "areaId": model.value.areaId,
            "type": "S",
            "areaName": model.value.areaName,
            "email": model.value.email,
            "phone": model.value.phone,
            "vehiclePlate": model.value.vehiclePlate,
            "cccd": model.value.cccd,
            "person": model.value.person
        }, arrB
    ];
 
    API.update(`PurchaseOrder/${props.paramID}/addresses`, payload)
        .then(() => {
            visible.value = false;
        })
        .catch((error) => {
            console.error(error);
            errMsg.value.global = error.response.data?.error;
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: t('Custom.update_address_failed'),
                life: 3000,
            });
        })
};

</script>
