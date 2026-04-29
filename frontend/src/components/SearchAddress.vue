<template>
    <div v-if="typeof props.customer === 'object'" class="flex flex-column gap-3 mt-3">
        <div class="flex flex-column gap-2">
            <label for="">{{ t('client.delivery_info') }}</label>
            <div class="card">
                <div class="flex justify-content-between align-items-center gap-3">
                    <div class="flex flex-column gap-2">
                        <span>
                            {{ t('client.recipient_name') }}: <strong>{{ Address("S").person }}</strong>
                        </span>
                        <span>
                            {{ t('client.address') }}:
                            <strong>{{
                                [
                                    Address("S").address,
                                    Address("S").locationName,
                                    Address("S").areaName,
                                ]
                                    .filter((el) => el)
                                    .join(", ")
                            }}</strong>
                        </span>
                        <span>
                            {{ t('client.phone_number') }}:
                            <strong>{{ Address("S").phone }}</strong>
                        </span>
                        <span>
                            {{ t('client.citizen_id') }}: <strong>{{ Address("S").cccd }}</strong>
                        </span>
                        <span>
                            {{ t('client.license_plate') }}:
                            <strong>{{ Address("S").vehiclePlate }}</strong>
                        </span>
                    </div>
                    <div>
                        <Button
                            :disabled="typeof props.customer !== 'object'"
                            @click="openChangeAddress('S')"
                            :label="t('body.OrderList.change_button')"
                            severity="secondary"
                        />
                    </div>
                </div>
            </div>
        </div>
        <div v-if="props.type !== 'YCLHG'" class="flex flex-column gap-2">
            <label for="">{{ t('client.invoice_info') }}</label>
            <div class="card">
                <div class="flex justify-content-between align-items-center gap-3">
                    <div class="flex flex-column gap-2">
                        <span>
                            {{ t('client.contact_person') }}:
                            <strong>{{ Address("B").person }}</strong>
                        </span>
                        <span>
                            {{ t('client.email') }}: <strong>{{ Address("B").email }}</strong>
                        </span>
                        <span>
                            {{ t('client.address') }}:
                            <strong>{{
                                [
                                    Address("B").address,
                                    Address("B").locationName,
                                    Address("B").areaName,
                                ]
                                    .filter((el) => el)
                                    .join(", ")
                            }}</strong>
                        </span>
                        <span v-if="false">
                            {{ t('client.phone_number') }}:
                            <strong>{{ Address("B").phone }}</strong>
                        </span>
                        <span v-if="false">
                            {{ t('client.citizen_id') }}: <strong>{{ Address("B").cccd }}</strong>
                        </span>
                        <span v-if="false">
                            {{ t('client.license_plate') }}:
                            <strong>{{ Address("B").vehiclePlate }}</strong>
                        </span>
                    </div>
                    <div>
                        <Button
                            :disabled="typeof props.customer !== 'object'"
                            @click="openChangeAddress('B')"
                            :label="t('body.OrderList.change_button')"
                            severity="secondary"
                        />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <Dialog
        v-model:visible="DeliveryAddModal"
        modal
        :header="typeModal == 'S' ? t('client.delivery_info') : t('client.invoice_info')"
        :style="{ width: '700px' }"
    >
        <div class="flex flex-column gap-3">
            <div class="flex justify-content-end">
                <Button
                    icon="pi pi-plus-circle"
                    text
                    @click="addNewAddr"
                    :label="t('common.btn_add_address')"
                />
            </div>
            <div
                class="card mb-2 flex align-items-center justify-content-between"
                v-for="item in getAddress(typeModal)"
                :key="item"
            >
                <div class="flex align-items-center gap-3">
                    <div class="flex items-center">
                        <RadioButton
                            v-model="item.default"
                            value="Y"
                            @change="UpdateDefault(getAddress(typeModal), item.id)"
                        />
                    </div>
                    <div class="flex flex-column gap-2">
                        <span>
                            {{ t('client.contact_person') }}: <strong>{{ item.person }}</strong>
                        </span>
                        <span>
                            {{ t('client.address') }}:
                            <strong>{{ item.locationName }} - {{ item.areaName }}</strong>
                        </span>
                        <span>
                            {{ t('client.email') }}: <strong>{{ item.email }}</strong>
                        </span>
                    </div>
                </div>
                <div class="flex gap-2">
                    <Button
                        :disabled="item.default != 'Y'"
                        @click="openEditAddr(item)"
                        text
                        icon="pi pi-pencil"
                    />
                    <Button
                        :disabled="item.default == 'Y'"
                        @click="openRemoveAddr(item)"
                        text
                        severity="danger"
                        icon="pi pi-trash"
                    />
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    @click="DeliveryAddModal = false"
                    :label="t('body.OrderList.close')"
                    icon="pi pi-times"
                    outlined
                />
            </div>
        </template>
    </Dialog>

    <Dialog
        v-model:visible="addNewAddrModal"
        modal
        :header="typeModal == 'S' ? t('client.add_delivery_info') || t('client.delivery_info') : t('client.invoice_info')"
        :style="{ width: '500px' }"
    >
        <div class="flex flex-column gap-3">
            <div class="flex flex-column gap-2">
                <label for="">{{ t('client.contact_person') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.person"
                    :invalid="submited && !payload.person"
                ></InputText>
            </div>
            <div class="flex flex-column gap-2" v-if="typeModal == 'S'">
                <label for="">{{ t('client.phone_number') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.phone"
                    class="input_number"
                    type="number"
                    :invalid="submited && !validatePhoneNumber(payload.phone)"
                >
                </InputText>
            </div>
            <div class="flex flex-column gap-2" v-if="typeModal == 'S'">
                <label for="">{{ t('client.license_plate') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.vehiclePlate"
                    :invalid="submited && !payload.vehiclePlate"
                ></InputText>
            </div>
            <div class="flex flex-column gap-2" v-if="typeModal == 'S'">
                <label for="">{{ t('client.citizen_id') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.cccd"
                    :invalid="submited && !validateCCCD(payload.cccd)"
                ></InputText>
            </div>
            <div class="flex flex-column gap-2">
                <label for="">{{ t('client.email') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.email"
                    :invalid="submited && !validateEmail(payload.email)"
                ></InputText>
            </div>
            <div class="flex flex-column gap-2">
                <label for="">{{ t('client.area') }}<sup class="text-red-500">*</sup></label>
                <AutoComplete
                    :placeholder="t('common.placeholder_enter_area')"
                    v-model="keySearchArea"
                    :suggestions="Area"
                    @change="onAreaChange($event)"
                    optionLabel="name"
                    @complete="fetchAllArea"
                    :pt:input:class="'w-full'"
                >
                </AutoComplete>
            </div>
            <div class="flex flex-column gap-2">
                <label for="">{{ t('client.ward_commune') }}</label>
                <AutoComplete
                    :placeholder="t('common.placeholder_enter_ward')"
                    :disabled="!keySearchArea || !payload.areaId"
                    v-model="keySearchLocation"
                    :suggestions="Location"
                    @change="onLocationChange($event)"
                    optionLabel="name"
                    @complete="fetchAllLoction"
                    :pt:input:class="'w-full'"
                >
                </AutoComplete>
            </div>
            <div class="flex flex-column gap-2">
                <label for="">{{ t('client.detailed_address') }} <sup class="text-red-500">*</sup></label>
                <InputText
                    v-model="payload.address"
                    :invalid="submited && !payload.address"
                ></InputText>
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    @click="addNewAddrModal = false"
                    :label="t('client.cancel')"
                    icon="pi pi-times"
                    outlined
                    v-if="0"
                />
                <Button @click="confirmAddAddr()" :label="t('client.save')" icon="pi pi-check"/>
            </div>
        </template>
    </Dialog>
    <Dialog
        v-model:visible="removeAddrModal"
        modal
        :header="t('client.confirm')"
        :style="{ width: '500px' }"
    >
        <div class="text-center p-2">{{ t('common.msg_confirm_delete_address') }}</div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    @click="removeAddrModal = false"
                    :label="t('client.cancel')"
                    icon="pi pi-times"
                    outlined
                />
                <Button
                    @click="confirmRemoveAddr()"
                    :label="t('body.OrderList.delete')"
                    severity="danger"
                    icon="pi pi-check"
                />
            </div>
        </template>
    </Dialog>
    <CustomerAddress ref="customerAddressComp" @change-default="onChangeFault" />
    <Loading v-if="isLoading"></Loading>
</template>

<script setup>
import { ref } from "vue";
import API from "../api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";

// Tuấn --------------| fix address change
import CustomerAddress from "./CustomeAddress/index.vue";
const customerAddressComp = ref();
const openChangeAddress = (type) => {
    customerAddressComp.value.openDialog(props.customer?.id, type);
};

const onChangeFault = (e) => {
    fetchCustomer();
};
//----------------------------------------
const { toast, FunctionGlobal } = useGlobal();

const { t } = useI18n();
const props = defineProps({
    data: {
        default: [],
    },
    customer: {},
    type: "",
});
const isLoading = ref(false);
const removeAddrModal = ref(false);
const Area = ref([]);
const Location = ref([]);
const keySearchArea = ref();
const keySearchLocation = ref();
const submited = ref(false);
const addNewAddrModal = ref(false);
const typeModal = ref();
const payload = ref({
    id: 0,
    locationId: 0,
    type: "",
    locationName: "",
    areaId: 0,
    areaName: "",
    address: "",
    email: "",
    phone: "",
    vehiclePlate: "",
    cccd: "",
    person: "",
    note: "",
    bpId: 0,
    default: "",
    status: "",
});
const clearPayload = JSON.stringify(payload.value);
const DeliveryAddModal = ref(false);
const Address = (type) => {
    if (props.data == undefined) return "";
    const ad = props.data.filter((val) => {
        return val.type == type && val.default == "Y";
    });
    if (ad.length > 0) {
        return ad[0];
    } else {
        return "";
    }
};
const funcClearPayload = () => {
    payload.value = JSON.parse(clearPayload);
};
const DeliveryAddressDialog = (type) => {
    typeModal.value = type;
    DeliveryAddModal.value = true;
};
const addNewAddr = () => {
    funcClearPayload();
    keySearchArea.value = "";
    keySearchLocation.value = "";
    submited.value = false;
    addNewAddrModal.value = true;
};
const fetchAllArea = async () => {
    if (typeof keySearchArea.value === "object") return;
    try {
        const res = await API.get(`Area/search/${keySearchArea.value}`);
        Area.value = res.data;
    } catch (error) {
        console.error(error);
    }
};
const onAreaChange = (e) => {
    payload.value.areaId = e.value.id;
    payload.value.areaName = e.value.name;
    keySearchLocation.value = "";
    payload.value.locationId = null;
    payload.value.locationName = null;
};
const fetchAllLoction = async () => {
    if (typeof keySearchLocation.value === "object") return;
    try {
        const res = await API.get(
            `Area/search/${payload.value.areaId}/${keySearchLocation.value}`
        );
        Location.value = res.data;
    } catch (error) {
        console.error(error);
    }
};
const onLocationChange = (e) => {
    payload.value.locationId = e.value.id;
    payload.value.locationName = e.value.name;
};
const confirmAddAddr = async () => {
    const data = { ...payload.value, type: typeModal.value, default: "N" };
    const dataUpdate = { ...payload.value, type: typeModal.value };
    submited.value = true;
    if (!ValidateData()) return;
    isLoading.value = true;
    try {
        const res = payload.value.id
            ? API.update(`Customer/${props.customer.id}/addresses`, dataUpdate)
            : API.add(`Customer/${props.customer.id}/addresses`, data);
        if (res) {
            FunctionGlobal.$notify(
                "S",
                payload.value.id
                    ? t('common.msg_update_address_success')
                    : t('common.msg_add_address_success'),
                toast
            );
            fetchCustomer();
        }
    } catch (error) {
        console.error(error);
    } finally {
        getAddress(typeModal.value);
        addNewAddrModal.value = false;
        isLoading.value = false;
    }
};
const fetchCustomer = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`Customer/${props.customer.id}`);
        const newData = res.data.crD1;
        props.data.splice(0, props.data.length, ...newData);
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const validatePhoneNumber = (phone) => {
    if (!phone || typeof phone !== "string") return false;
    const phoneStr = String(phone);
    const normalizedPhone = phoneStr.trim().replace(/^\+84/, "0");
    return /^(0[235789]|01[2689])[0-9]{8}$/.test(normalizedPhone);
};
const validateEmail = (email) => {
    if (!email) return false;
    return email.match(
        /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    );
};
const validateCCCD = (CCCD) => {
    if (!CCCD) return false;
    return /^[0-9]{1,20}$/.test(CCCD);
};
const ValidateData = () => {
    if (typeModal.value == "S") {
        if (!payload.value.person) {
            FunctionGlobal.$notify("E", t('common.validation_enter_name'), toast);
            return false;
        }
        if (!payload.value.phone) {
            FunctionGlobal.$notify("E", t('common.validation_enter_phone'), toast);
            return false;
        }
        if (!validatePhoneNumber(payload.value.phone)) {
            FunctionGlobal.$notify("E", t('common.validation_invalid_phone'), toast);
            return false;
        }
        if (!payload.value.vehiclePlate) {
            FunctionGlobal.$notify("E", t('common.validation_enter_plate'), toast);
            return false;
        }
        if (!payload.value.cccd) {
            FunctionGlobal.$notify("E", t('common.validation_enter_id_card'), toast);
            return false;
        }
        if (!validateCCCD(payload.value.cccd)) {
            FunctionGlobal.$notify("E", t('common.validation_id_card_too_long'), toast);
            return false;
        }
        if (!payload.value.email) {
            FunctionGlobal.$notify("E", t('common.validation_enter_email'), toast);
            return false;
        }
    }
    if (!validateEmail(payload.value.email)) {
        FunctionGlobal.$notify("E", t('common.validation_invalid_email'), toast);
        return false;
    }
    if (!keySearchArea.value) {
        FunctionGlobal.$notify("E", t('common.validation_enter_area'), toast);
        return false;
    }
    if (!keySearchLocation.value) {
        FunctionGlobal.$notify("E", t('common.validation_enter_ward'), toast);
        return false;
    }
    return true;
};
const getAddress = (type) => {
    return props.data.filter((val) => {
        return val.type == type;
    });
};
const UpdateDefault = async (data, id) => {
    await data.forEach((el) => {
        if (el.id != id) el.default = "N";
    });
    confirmSubmit();
};
const confirmSubmit = async () => {
    let dataS = props.data.find((el) => el.default === "Y" && el.type === "S");
    let dataB = props.data.find((el) => el.default === "Y" && el.type === "B");
    isLoading.value = true;
    try {
        const res = await API.update(
            `Customer/${props.customer.id}/addresses`,
            typeModal.value === "S" ? dataS : dataB
        );
        if (res) {
            FunctionGlobal.$notify("S", t('common.success'), toast);
            fetchCustomer();
        }
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const openEditAddr = (data) => {
    payload.value = data;
    keySearchArea.value = data.areaName;
    keySearchLocation.value = data.locationName;
    addNewAddrModal.value = true;
};
const openRemoveAddr = (data) => {
    payload.value = data;
    removeAddrModal.value = true;
};
const confirmRemoveAddr = async () => {
    try {
        const res = await API.delete(`Customer/${props.customer.id}/addresses`, [
            payload.value.id,
        ]);
        FunctionGlobal.$notify("S", t('common.msg_delete_address_success'), toast);
        removeAddrModal.value = false;
    } catch (error) {
        console.error(error);
    } finally {
        fetchCustomer();
    }
};
</script>
<style>
.input_number::-webkit-outer-spin-button,
.input_number::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

/* Firefox */
.input_number[type="number"] {
    -moz-appearance: textfield;
}
</style>
