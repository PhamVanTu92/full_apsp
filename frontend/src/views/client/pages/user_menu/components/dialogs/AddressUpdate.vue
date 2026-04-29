<template>
    <div>
        <Dialog
            :header="type == 'S' ? t('client.delivery_info') : t('client.invoice_info')"
            v-model:visible="visible"
            modal
            class="w-30rem"
            @hide="onHide"
        >
            <div>
                <!-- <Message
                    v-if="errMsg.global"
                    :sticky="false"
                    severity="error"
                    :life="3000"
                    >{{ errMsg.global }}</Message
                > -->

                <div class="field">
                    <label for="">{{ t('client.contact_person') }} <span class="p-required">*</span></label>
                    <InputText
                        v-model="model.person"
                        class="w-full"
                        autofocus
                        :invalid="!!errMsg.person"
                    ></InputText>
                    <small class="p-required">{{ errMsg.person }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('client.phoneNumber') }} <span class="p-required">*</span></label>
                    <InputText
                        v-model="model.phone"
                        class="w-full"
                        :invalid="!!errMsg.phone"
                    ></InputText>
                    <small class="p-required">{{ errMsg.phone }}</small>
                </div>
                <div class="field" v-if="type != 'B'">
                    <label for="">{{ t('client.license_plate') }} <span class="p-required">*</span></label>
                    <InputText
                        v-model="model.vehiclePlate"
                        class="w-full"
                        :invalid="!!errMsg.vehiclePlate"
                    ></InputText>
                    <small class="p-required">{{ errMsg.vehiclePlate }}</small>
                </div>
                <div class="field" v-if="type != 'B'">
                    <label for="">
                        {{ t('client.citizenId') }}
                        <span class="p-required">*</span>
                    </label>
                    <InputText
                        v-model="model.cccd"
                        class="w-full"
                        :invalid="!!errMsg.cccd"
                    ></InputText>
                    <small class="p-required">{{ errMsg.cccd }}</small>
                </div>
                <div class="field">
                    <label for="">
                        {{ t('client.email') }}
                        <span class="p-required" v-if="type == 'B'">*</span>
                    </label>
                    <InputText
                        v-model="model.email"
                        class="w-full"
                        :invalid="!!errMsg.email"
                    ></InputText>
                    <small class="p-required">{{ errMsg.email }}</small>
                </div>
                <div class="flex gap-3 align-items-center mb-2">
                    <hr class="flex-grow-1" />
                    <span class="font-semibold">{{ t('client.address') }}</span>
                    <hr class="flex-grow-1" />
                </div>
                <div class="field flex gap-3">
                    <span class="mr-3">{{ t('client.address_type') || t('client.address') }}:</span>
                    <label>
                        <RadioButton
                            v-model="isInternational"
                            :value="false"
                        ></RadioButton>
                        <span class="ml-2">{{ t('client.domestic') }}</span>
                    </label>
                    <label>
                        <RadioButton
                            v-model="isInternational"
                            :value="true"
                        ></RadioButton>
                        <span class="ml-2">{{ t('client.international') }}</span>
                    </label>
                </div>

                <div class="field" v-if="!isInternational">
                    <label for="">{{ t('client.area') }} <span class="p-required">*</span></label>
                    <AutoComplete
                        v-model="selection.area"
                        class="w-full"
                        pt:input:class="flex-grow-1"
                        optionLabel="name"
                        :suggestions="suggestions.area"
                        @complete="onComplete($event, 'area')"
                        @change="onChange('area')"
                        :invalid="!!errMsg.areaId"
                    ></AutoComplete>
                    <small class="p-required">{{ errMsg.areaId }}</small>
                </div>
                <div class="field" v-if="!isInternational">
                    <label for="">{{ t('client.ward_commune') }} <span class="p-required">*</span></label>
                    <AutoComplete
                        :disabled="!model.areaId"
                        v-model="selection.location"
                        class="w-full"
                        pt:input:class="flex-grow-1"
                        optionLabel="name"
                        :suggestions="suggestions.location"
                        @complete="onComplete($event, 'location')"
                        @change="onChange('location')"
                        :invalid="!!errMsg.locationId"
                    ></AutoComplete>
                    <small class="p-required">{{ errMsg.locationId }}</small>
                </div>
                <div class="field">
                    <label for="">{{ t('client.detailed_address') }}<span class="p-required">*</span></label>
                    <Textarea
                        v-model="model.address"
                        class="w-full"
                        :invalid="!!errMsg.address"
                    ></Textarea>
                    <small class="p-required">{{ errMsg.address }}</small>
                </div>
            </div>
            <template #footer>
                <Button
                    @click="visible = false"
                    severity="secondary"
                    :label="t('client.cancel')"
                />
                <Button :loading="loading" @click="onClickSave" :label="t('client.save')"/>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useOrderDetailStore } from "../store/orderDetail";
import { Address } from "../types/orderDetail";
import API from "@/api/api-main";
import { Validator, ValidateOption, PATTERN } from "@/helpers/validate";
import { useToast } from "primevue/usetoast";

import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const visible = ref(false);
const odStore = useOrderDetailStore();

const type = ref<"S" | "B">();
const model = ref<Address>({} as Address);
const isInternational = ref(false); // quốc tế : true, trong nước: fasle

const selection = reactive({
    area: { id: null as any, name: null as any },
    location: { id: null as any, name: null as any },
});

const suggestions = reactive({
    area: [] as any[],
    location: [] as any[],
});

const S_validateOpt: ValidateOption<Address> = {
    person: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.contact_name_required'),
        },
    },
    phone: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.phone_required'),
        },
    },
    cccd: {
        validators: {
            required: true,
            type: String,
            pattern: PATTERN.identityId,
            nullMessage: t('Custom.id_required'),
        },
    },
    address: {
        validators: {
            required: true,
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
const B_validateOpt: ValidateOption<Address> = {
    person: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.contact_name_required'),
        },
    },
    phone: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.phone_required'),
        },
    },
    email: {
        validators: {
            required: true,
            type: String,
            pattern: PATTERN.email,
            nullMessage: t('Custom.email_required'),
            patternMessage: t('Custom.email_invalid'),
        },
    },
    address: {
        validators: {
            required: true,
            type: String,
            nullMessage: t('Custom.address_required'),
        },
    },
};

const toast = useToast();
const errMsg = ref<any>({});
const loading = ref(false);
const onClickSave = () => {
    const vldOpt = type.value === "S" ? S_validateOpt : B_validateOpt;
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
                nullMessage: t('client.ward_commune'),
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

    const anotherAddress =
        odStore.order?.address?.filter((adrss) => adrss.type != type.value) || [];
    const payload = [model.value];
    if (anotherAddress.length > 0) {
        payload.push(...anotherAddress);
    }
    loading.value = true;
 
    API.update(`PurchaseOrder/${odStore.order?.id}/addresses`, payload)
        .then(() => {
            odStore.fetchStore();
            visible.value = false;
        })
        .catch((error) => {
            console.error(error);
            errMsg.value.global = err.response.data?.error;
            toast.add({
                severity: "error",
                summary: t('Custom.error'),
                detail: t('Custom.update_address_failed'),
                life: 3000,
            });
        })
        .finally(() => {
            loading.value = false;
        });
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

const onHide = () => {
    isInternational.value = false;
    errMsg.value = {};
};

const open = (dialogType: "S" | "B") => {
    const address = odStore.order?.address?.find((item) => item.type == dialogType);
    Object.assign(model.value, address);
    if (address?.areaId || address?.locationId) {
        selection.area.id = address?.areaId;
        selection.area.name = address?.areaName;
        selection.location.id = address?.locationId;
        selection.location.name = address?.locationName;
        isInternational.value = false;
    } else {
        isInternational.value = true;
    }
    type.value = dialogType;
    visible.value = true;
};

defineExpose({
    open,
});

watch(
    () => isInternational.value,
    () => {
        model.value.areaId = null as any;
        model.value.areaName = null as any;
        model.value.locationId = null as any;
        model.value.locationName = null as any;
        selection.area.id = null;
        selection.area.name = "";
        selection.location.id = null;
        selection.location.name = "";
    }
);

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.p-required {
    color: var(--red-500);
}
</style>
