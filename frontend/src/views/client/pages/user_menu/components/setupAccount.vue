<template>
    <GeneralInfo :data="user" @on-submit="onSubmitGeneralInfo" />
    <AccountInfo />
    <CRDAddress
        :id="user?.cardId"
        :header="t('client.contact_person')"
        :columns="contactorColumn"
        :data="user?.bpInfo?.crD5"
        :crd="'crD5'"
        :type="null"
        @on-submit="onSubmitCRD5"
    />
    <CRDAddress
        :id="user?.cardId"
        :header="t('client.delivery_address')"
        :columns="deliveryAddressColumn"
        :data="deliveryAddress.filter((el) => el.type == 'S')"
        :crd="'crD1'"
        :type="'S'"
        @on-submit="onSubmitCRD1"
    />
    <CRDAddress
        :id="user?.cardId"
        :header="t('client.billingAddress')"
        :columns="billingAddressColumn"
        :data="deliveryAddress.filter((el) => el.type == 'B')"
        :crd="'crD1'"
        :type="'B'"
        @on-submit="onSubmitCRD1"
    />
    <ChangePassword />
</template>

<script setup>
import { ref, reactive, onBeforeMount } from "vue";
import API from "@/api/api-main";
import GeneralInfo from "./components/GeneralInfo.vue";
import CRDAddress from "./components/CRDAddress.vue";
import AccountInfo from "./components/AccountInfo.vue";
import ChangePassword from "./components/ChangePassword.vue";

import {
    phoneRegex,
    vehiclePlateRegex,
    emailRegex,
    cccdRegex,
} from "../../../../../helpers/regex";
import { useMeStore } from "../../../../../Pinia/me";
// add i18n
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const user = ref({});

const onSubmitGeneralInfo = (data) => {
    Object.assign(user.value.bpInfo, data);
};
const onSubmitCRD1 = (data) => {
    deliveryAddress.value =
        data?.crD1?.map((el) => {
            let addressArr = [el.address, el.locationName, el.areaName];
            return {
                ...el,
                full_address: addressArr.filter((x) => x).join(", "),
            };
        }) || [];
};
const onSubmitCRD5 = (data) => {
    Object.assign(user.value.bpInfo, data);
};

const contactorColumn = [
    {
        field: "person",
        header: t('client.recipient_name'),
        regex: null,
        required: true,
        placeholder: "Nguyễn Văn A",
    },
    {
        field: "phone",
        header: t('client.phone_1'),
        regex: phoneRegex,
        placeholder: "0123456789",
        required: true,
    },
    {
        field: "phone1",
        header: t('client.phone_2'),
        regex: phoneRegex,
        placeholder: "0123456789",
        required: true,
    },
    {
        field: "email",
        header: t('client.email'),
        regex: emailRegex,
        placeholder: "example@email.com",
        required: true,
    },
];

const deliveryAddressColumn = [
    {
        field: "person",
        header: t('client.recipient_name'),
        regex: null,
        required: true,
        placeholder: "Nguyễn Văn A",
    },
    {
        field: "phone",
        header: t('client.phone_contact'),
        regex: phoneRegex,
        placeholder: "0123456789",
        required: true,
    },
    {
        field: "vehiclePlate",
        header: t('client.vehicle_plate'),
        regex: vehiclePlateRegex,
        placeholder: "30A-12345",
        required: true,
    },
    {
        field: "cccd",
        header: t('client.cccd_number'),
        regex: cccdRegex,
        placeholder: "012345678912",
        required: true,
    },
    {
        field: "email",
        header: t('client.email'),
        regex: emailRegex,
        placeholder: "example@email.com",
        required: true,
    },
    { field: "full_address", header: t('client.address'), required: true, regex: null },
];

const billingAddressColumn = [
    {
        field: "person",
        header: t('client.recipient_name'),
        regex: null,
        required: true,
        placeholder: "Nguyễn Văn A",
    },
    {
        field: "phone",
        header: t('client.phone_contact'),
        regex: phoneRegex,
        placeholder: "0123456789",
        required: true,
    },
    { field: "full_address", header: t('client.address'), required: true, regex: null },
];

const deliveryAddress = ref([]);
const meStore = useMeStore();
const initialComponents = async () => {
    const meData = await meStore.getMe();
    if (meData) {
        user.value = meData.user;
        deliveryAddress.value =
            user.value?.bpInfo?.crD1?.map((el) => {
                let addressArr = [el.address, el.locationName, el.areaName];
                return {
                    ...el,
                    full_address: addressArr.filter((x) => x).join(", "),
                };
            }) || [];
    }
};

onBeforeMount(() => {
    initialComponents();
});
</script>
