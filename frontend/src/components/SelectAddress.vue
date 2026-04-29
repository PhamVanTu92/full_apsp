<template>
    <div :class="props.class">
        <label for="address" v-if="props.label">{{ props.label }}</label> 
        <InputGroup>
            <InputText
                :invalid="props.invalid"
                :id="props.inputId"
                v-model="value"
                :placeholder="t('Custom.select_address')"
                readonly
                :disabled="props.disabled"
            />
            <Button
                :disabled="props.disabled"
                @click="onClickShow"
                :label="props.btnLabel"
                severity="secondary"
                :icon="props.icon"
            />
        </InputGroup>
    </div>
    <Dialog v-model:visible="visible" modal :header="t('client.address')" class="w-30rem">
        <div class="flex field flex-column">
            <label for="city">{{t('client.province_city')}} - {{t('client.district')}}</label>
            <AutoComplete
                @item-select="onSelectCity"
                v-model="selectModel.area"
                inputId="city"
                optionLabel="name"
                :pt:input:class="'w-full'"
                @complete="onSearchCity"
                :suggestions="data.suggestionsCity"
                :invalid="errors.area"
            />
            <small class="text-red-500">{{ errors.area }}</small>
        </div>
        <div class="flex field flex-column">
            <label for="ward">{{t('client.ward_commune')}}</label>
            <AutoComplete
                @item-select="onSelectWard"
                v-model="selectModel.location"
                inputId="ward"
                optionLabel="name"
                :pt:input:class="'w-full'"
                @complete="onSearchWard"
                :suggestions="data.suggestionsWard"
                :invalid="errors.location"
            />
            <small class="text-red-500">{{ errors.location }}</small>
        </div>  
        <div class="flex field flex-column mb-0">
            <label for="address_detail">{{t('client.detailed_address')}}</label>
            <InputText
                v-model="payload.address"
                :placeholder="props.placeholder"
                id="address_detail"
                class="w-full"
                :invalid="errors.address"
            />
            <small class="text-red-500">{{ errors.address }}</small>
        </div>
        <template #footer>
            <Button
                @click="visible = false"
                icon="pi pi-times"
                :label="t('client.cancel')"
                severity="secondary"
            />
            <Button @click="onClickConfirm" icon="pi pi-check" :label="t('client.select')"/>
        </template>
    </Dialog>
</template>

<script setup>
import InputText from "primevue/inputtext";
import { ref, reactive, onMounted } from "vue";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
defineOptions({
    inheritAttrs: true,
});

const data = reactive({
    suggestionsCity: [],
    suggestionsWard: [],
});
const emits = defineEmits(["update:modelValue", "select"]);
const selectModel = ref({
    area: null,
    location: null,
});

const payload = reactive({
    address: null,
    areaId: null,
    areaName: null,
    locationId: null,
    locationName: null,
});

const value = ref("");
const visible = ref(false);
const props = defineProps({
    modelValue: {
        type: Object,
        default: {},
    },
    label: {
        type: String,
        required: true,
    },
    btnLabel: {
        type: String,
        default: null,
    },
    placeholder: {
        type: String,
        default: null,
    },
    icon: {
        type: String,
        default: "pi pi-pencil",
    },
    disabled: {
        type: Boolean,
        default: false,
    },
    class: {
        type: String,
        default: "",
    },
    inputId: {
        type: String,
        default: "address",
    },
    invalid: {
        type: Boolean,
        default: false,
    },
});

const onClickShow = () => {
    visible.value = true;
    selectModel.value = {
        area: props.modelValue.areaName,
        location: props.modelValue.locationName,
    };
    payload.address = props.modelValue.address;
    payload.areaId = props.modelValue.areaId;
    payload.areaName = props.modelValue.areaName;
    payload.locationId = props.modelValue.locationId;
    payload.locationName = props.modelValue.locationName;
};
const errors = reactive({
    area: null,
    location: null,
    address: null,
});
const validate = () => {
    errors.area = null;
    errors.location = null;
    errors.address = null;
    if (!payload.areaId) {
        errors.area = t('Custom.city_required');
    }
    if (!payload.locationId) {
        errors.location = t('Custom.district_required');
    }
    if (!payload.address?.trim()) {
        errors.address = t('Custom.address_detail_required');
    }
    return payload.areaId && payload.locationId && payload.address?.trim();
};

const onClickConfirm = () => {
    if (!validate()) return;
    visible.value = false;
    emits("update:modelValue", { ...payload });
    emits("select", payload);
    const fields = [payload.address, payload.locationName, payload.areaName];
    value.value = fields.filter((el) => el).join(", ");
};

const onSearchCity = (event) => {
    if (typeof selectModel.value.area === "object") {
        data.suggestionsCity = null;
        return;
    }
    let query = encodeURIComponent(event.query);
    selectModel.value.location = null;
    API.get(`area/search/${query}`)
        .then((res) => {
            data.suggestionsCity = res.data;
        })
        .catch((error) => {
            console.error(error);
        });
};
const onSearchWard = (event) => {
    if (typeof selectModel.value.location === "object") {
        data.suggestionsWard = null;
        return;
    }
    let query = encodeURIComponent(event.query);
    API.get(`area/search/${payload.areaId}/${query}`)
        .then((res) => {
            data.suggestionsWard = res.data;
        })
        .catch((error) => {
            console.error(error);
        });
};

const onSelectCity = (event) => {
    payload.areaId = event.value.id;
    payload.areaName = event.value.name;
};
const onSelectWard = (event) => {
    payload.locationId = event.value.id;
    payload.locationName = event.value.name;
};

onMounted(() => {
    selectModel.value = {
        area: props.modelValue.areaName,
        location: props.modelValue.locationName,
    };
    payload.address = props.modelValue.address;
    payload.areaId = props.modelValue.areaId;
    payload.areaName = props.modelValue.areaName;
    payload.locationId = props.modelValue.locationId;
    payload.locationName = props.modelValue.locationName;

    const fields = [
        props.modelValue.address,
        props.modelValue.locationName,
        props.modelValue.areaName,
    ];
    value.value = fields.filter((el) => el).join(", ");
});
</script>

<style></style>
