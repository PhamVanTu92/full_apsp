<template>
    <Dialog @hide="onHideAddressDlog" v-model:visible="dialogVisible" header="Nhập địa chỉ" modal style="width: 30rem">
        <div class="mb-3 flex gap-3">
            <span>{{ t('client.delivery_address_label') }}: </span>
            <span>
                <RadioButton v-model="addressType" class="mr-2" :value="true" @change="onChangeAddressType"> </RadioButton>
                <label for="">{{ t('client.domestic') }}</label>
            </span>
            <span>
                <RadioButton v-model="addressType" class="mr-2" :value="false" @change="onChangeAddressType"> </RadioButton>
                <label for="">{{ t('client.international') }}</label>
            </span>
        </div>
        <div class="flex flex-column field" v-if="addressType">
            <label for="input1">{{ t('client.province_city') }}</label>
            <AutoComplete @select="onSelectArea" @complete="onSearchArea" :suggestions="locationData.area" optionLabel="name" v-model="addressModel.area" inputId="input1" :pt:input:class="'w-full'" />
            <small class="text-red-500">{{ errorAddres.area }}</small>
        </div>
        <div class="flex flex-column field" v-if="addressType">
            <label for="input2">{{ t('client.district_ward') }}</label>
            <AutoComplete :disabled="!addressModel.area?.id" @complete="onSearchLocation" :suggestions="locationData.location" optionLabel="name" v-model="addressModel.location" inputId="input2" :pt:input:class="'w-full'" />
            <small class="text-red-500">{{ errorAddres.location }}</small>
        </div>
        <div class="flex flex-column field">
            <label for="input3">{{ t('client.specific_address') }}</label>
            <InputText v-model="addressModel.address" id="input3" class="w-full" />
            <small class="text-red-500">{{ errorAddres.address }}</small>
        </div>
        <template #footer>
            <Button @click="dialogVisible = false" icon="pi pi-times" severity="secondary" :label="t('client.cancel')" />
            <Button @click="onClickConfirmAddress" icon="pi pi-check" :label="t('body.status.XN')" />
        </template>
    </Dialog>
</template>

<script setup>
import { ref, reactive, computed, defineProps, defineEmits } from 'vue';
import { useI18n } from 'vue-i18n';
import API from '@/api/api-main';

const { t } = useI18n();
const props = defineProps({
    visible: Object,
    editMode: Object,
    generalInfoModel: Object,
    addressModel: Object,
});
const emits = defineEmits(['update:visible', 'confirm']);

// Dùng computed thay vì truy cập trực tiếp props.visible.address
// để tránh crash khi visible prop chưa sẵn sàng
const dialogVisible = computed({
    get: () => props.visible?.address ?? false,
    set: (val) => { if (props.visible) props.visible.address = val; }
});

const locationData = reactive({
    area: [],
    location: []
});

const errorAddres = reactive({
    area: null,
    location: null,
    address: null
});

const addressType = ref(true);

const onSearchArea = async () => {
    try {
        const res = await API.get('area/search/' + props.addressModel.area);
        locationData.area = res.data;
    } catch (error) {}
};

const onSearchLocation = async () => {
    if (props.addressModel.area.id) {
        try {
            const res = await API.get(`area/search/${props.addressModel.area.id}/${props.addressModel.location}`);
            locationData.location = res.data;
        } catch (error) {}
    } else {
        locationData.location = [];
    }
};

const onSelectArea = () => {
    props.addressModel.location = {};
};

const onClickConfirmAddress = () => {
    errorAddres.address = null;
    errorAddres.area = null;
    errorAddres.location = null;
    let errCount = 0; 
    if (!props.addressModel.area.id && addressType.value == true) {
        errorAddres.area = 'Vui lòng chọn Thành phố/Tỉnh - Quận/Huyện';
        errCount++;
    }
    if (!props.addressModel.location.id && addressType.value) {
        errorAddres.location = 'Vui lòng chọn Phường/Xã';
        errCount++;
    }
    if (!props.addressModel.address?.trim()) {
        errorAddres.address = 'Vui lòng nhập địa chỉ';
        errCount++;
    } 
    if (errCount > 0) return; 
    const newAddress = {
        areaId: props.addressModel.area.id,
        areaName: props.addressModel.area.name,
        locationId: props.addressModel.location.id,
        locationName: props.addressModel.location.name,
        address: props.addressModel.address,
        _addressLabel: `${props.addressModel.address}, ${props.addressModel.location.name}, ${props.addressModel.area.name}`
    };

    emits('confirm', newAddress);
    if (props.visible) props.visible.address = false;
};

const onChangeAddressType = () => {
    props.addressModel.location = {};
    props.addressModel.area = {};
    props.addressModel.address = null;
    locationData.area = [];
    locationData.location = [];
};

const onHideAddressDlog = () => {
    // Reset model khi đóng dialog
    // Không emit update:visible — PrimeVue tự quản lý qua v-model (dialogVisible computed setter)
    addressType.value = true;
    if (props.addressModel) {
        props.addressModel.location = {};
        props.addressModel.area = {};
        props.addressModel.address = null;
    }
};
</script>