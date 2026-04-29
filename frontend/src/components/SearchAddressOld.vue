<template>
  <div class="flex flex-column gap-3">
    <div class="flex flex-column gap-2">
      <label for="">{{ t('common.dialog_update_delivery_address') }}</label>
      <InputGroup>
        <InputText readonly :value="Address('S')" />
        <Button @click="DeliveryAddressDialog()" :label="t('common.btn_change')" severity="secondary" />
      </InputGroup>
    </div>
    <div class="flex flex-column gap-2">
      <label for="">{{ t('common.dialog_update_billing_address') }}</label>
      <InputGroup>
        <InputText readonly :value="Address('B')" />
        <Button @click="BillingAddressDialog()" :label="t('common.btn_change')" severity="secondary" />
      </InputGroup>
    </div>
  </div>

  <Dialog v-model:visible="DeliveryAddModal" modal
    :header="type == 'S' ? t('common.dialog_update_delivery_address') : t('common.dialog_update_billing_address')" :style="{ width: '700px' }">
    <div>
      <div class="card mb-2 flex align-items-center justify-content-between" v-for="item in getAddress(type)"
        :key="item">
        <div class="flex align-items-center gap-3">
          <div class="flex items-center">
            <RadioButton v-model="item.default" value="Y" @change="UpdateDefault(getAddress(type), item.id)" />
          </div>
          <div class="flex flex-column gap-2">
            <span>{{ t('client.recipient_name') }}: <strong>{{ item.person }}</strong></span>
            <span>{{ t('client.address') }}:
              <strong>{{ item.locationName }} - {{ item.areaName }}</strong>
            </span>
            <span>{{ t('client.phone_number') }}: <strong>{{ item.phone }}</strong></span>
          </div>
        </div>
        <div class="flex gap-2">
          <Button :disabled="item.default != 'Y'" @click="openEditAddr(item)" text icon="pi pi-pencil"/>
          <Button :disabled="item.default != 'Y'" text severity="danger" icon="pi pi-trash"/>
        </div>
      </div>
    </div>

    <div class="flex flex-column gap-3 card" v-if="ortherAddress">
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.recipient_name') }} <sup class="text-red-500">*</sup></label>
        <InputText v-model="person" :invalid="submited && !person" v-if="type == 'S'"></InputText>
        <InputText v-model="person_2" :invalid="submited && !person_2" v-else></InputText>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.phone_number') }} <sup class="text-red-500">*</sup></label>
        <InputText class="input_number" type="number" :invalid="submited && !validatePhoneNumber(phone)"
          v-if="type == 'S'" v-model="phone">
        </InputText>
        <InputText class="input_number" type="number" :invalid="submited && !validatePhoneNumber(phone_2)" v-else
          v-model="phone_2">
        </InputText>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.license_plate') }} <sup class="text-red-500">*</sup></label>
        <InputText :invalid="submited && !Vehicle" v-model="Vehicle" v-if="type == 'S'"></InputText>
        <InputText :invalid="submited && !Vehicle_2" v-model="Vehicle_2" v-else></InputText>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.citizen_id') }}<sup class="text-red-500">*</sup></label>
        <InputText class="input_number" v-model="CCCD" :invalid="submited && !validateCCCD(CCCD)" v-if="type == 'S'">
        </InputText>
        <InputText class="input_number" v-model="CCCD_2" :invalid="submited && !validateCCCD(CCCD_2)" v-else>
        </InputText>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">Email <sup class="text-red-500">*</sup></label>
        <InputText :invalid="submited && !validateEmail(email)" v-if="type == 'S'" v-model="email"></InputText>
        <InputText :invalid="submited && !validateEmail(email_2)" v-else v-model="email_2"></InputText>
      </div>

      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.address') }}</label>
        <InputText :invalid="submited && !address" v-if="type == 'S'" v-model="address"></InputText>
        <InputText :invalid="submited && !address_2" v-else v-model="address_2"></InputText>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.area') }}</label>
        <AutoComplete :placeholder="t('common.placeholder_enter_area')" :invalid="validateKeySearch('A')" v-if="type == 'S'"
          v-model="keySearchArea" :suggestions="Area" @change="onAreaChange($event)" optionLabel="name"
          @complete="fetchAllArea" :pt:input:class="'w-full'">
        </AutoComplete>
        <AutoComplete :placeholder="t('common.placeholder_enter_area')" :invalid="validateKeySearch('A2')" v-else v-model="keySearchArea_2"
          :suggestions="Area" @change="onAreaChange($event)" optionLabel="name" @complete="fetchAllArea"
          :pt:input:class="'w-full'">
        </AutoComplete>
      </div>
      <div class="flex flex-column gap-2">
        <label for="">{{ t('client.ward_commune') }}</label>
        <AutoComplete :placeholder="t('common.placeholder_enter_ward')" :disabled="!keySearchArea" :invalid="validateKeySearch('L')"
          v-if="type == 'S'" v-model="keySearchLocation" :suggestions="Location" @change="onLocationChange($event)"
          optionLabel="name" @complete="fetchAllLoction" :pt:input:class="'w-full'">
        </AutoComplete>
        <AutoComplete :placeholder="t('common.placeholder_enter_ward')" :invalid="validateKeySearch('L2')" v-else
          v-model="keySearchLocation_2" :suggestions="Location" @change="onLocationChange($event)" optionLabel="name"
          @complete="fetchAllLoction" :pt:input:class="'w-full'">
        </AutoComplete>
      </div>
      <Button :label="t('common.btn_confirm')" @click="AddOrderAddress(getAddress(type))"/>
    </div>
    <Button :label="ortherAddress ? t('common.btn_close') : t('common.btn_add_other_address')" text @click="discardForm()"/>
    <template #footer>
      <div class="flex gap-2">
        <Button @click="DeliveryAddModal = false" :label="t('common.btn_cancel')" icon="pi pi-times" outlined/>
        <Button @click="confirmSubmit()" :label="t('common.btn_confirm')" icon="pi pi-check"/>
      </div>
    </template>
  </Dialog>
</template>
<script setup>
import { ref } from "vue";
import API from "../api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const { toast, FunctionGlobal } = useGlobal();

const props = defineProps({
  data: [],
  customer: {},
});
const submited = ref(false);
const id = ref(0);
const id_2 = ref(0);
const address = ref("");
const address_2 = ref("");
const keySearchArea = ref("");
const keySearchArea_2 = ref("");
const keySearchLocation = ref("");
const keySearchLocation_2 = ref("");
const DeliveryAddModal = ref(false);
const Area = ref([]);
const areaId = ref();
const areaName = ref();
const areaId_2 = ref();
const areaName_2 = ref();
const email = ref();
const phone = ref();
const email_2 = ref();
const phone_2 = ref();
const Location = ref([]);
const locationId = ref();
const locationName = ref();
const locationId_2 = ref();
const locationName_2 = ref();
const ortherAddress = ref(false);
const CCCD = ref("");
const CCCD_2 = ref("");
const Vehicle = ref("");
const Vehicle_2 = ref("");
const person = ref("");
const person_2 = ref("");
const DeliveryAddressDialog = () => {
  type.value = "S";
  DeliveryAddModal.value = true;
};

const BillingAddressDialog = () => {
  type.value = "B";
  DeliveryAddModal.value = true;
};
const type = ref("");
const fetchAllArea = async () => {
  try {
    const res = await API.get(
      `Area/search/${type.value == "S" ? keySearchArea.value : keySearchArea_2.value}`
    );
    Area.value = res.data;
  } catch (error) { }
};
const onAreaChange = (e) => {
  if (type.value == "S") {
    areaId.value = e.value.id;
    areaName.value = e.value.name;
    keySearchLocation.value = "";
    locationId.value = null;
    locationName.value = null;
  }
  if (type.value == "B") {
    areaId_2.value = e.value.id;
    areaName_2.value = e.value.name;
    keySearchLocation_2.value = "";
    locationId_2.value = null;
    locationName_2.value = null;
  }
};
const fetchAllLoction = async () => {
  try {
    const res = await API.get(
      `Area/search/${type.value == "S" ? areaId.value : areaId_2.value}/${type.value == "S" ? keySearchLocation.value : keySearchLocation_2.value
      }`
    );
    Location.value = res.data;
  } catch (error) { }
};
const onLocationChange = (e) => {
  if (type.value == "S") {
    locationId.value = e.value.id;
    locationName.value = e.value.name;
  }
  if (type.value == "B") {
    locationId_2.value = e.value.id;
    locationName_2.value = e.value.name;
  }
};
const confirmSubmit = () => {
  DeliveryAddModal.value = false;
};
const validateKeySearch = (type) => {
  switch (type) {
    case "A":
      return submited.value && !keySearchArea.value;
    case "A2":
      return submited.value && !keySearchArea_2.value;
    case "L":
      return submited.value && !keySearchLocation.value;
    case "L2":
      return submited.value && !keySearchLocation_2.value;
  }
};
const validateEmail = (email) => {
  if (!email) return false;
  return email.match(
    /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
  );
};
const validatePhoneNumber = (phone) => {
  if (!phone || typeof phone !== "string") return false;
  const phoneStr = String(phone);
  const normalizedPhone = phoneStr.trim().replace(/^\+84/, "0");
  return /^(0[235789]|01[2689])[0-9]{8}$/.test(normalizedPhone);
};
const validateCCCD = (CCCD) => {
  if (!CCCD) return false;
  return /^[0-9]{1,20}$/.test(CCCD);
};
const ValidateData = () => {
  if (type.value == "S") {
    if (!person.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_name'), toast);
      return false;
    }
    if (!phone.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_phone'), toast);
      return false;
    }
    if (!validatePhoneNumber(phone.value)) {
      FunctionGlobal.$notify("E", t('common.validation_invalid_phone'), toast);
      return false;
    }
    if (!Vehicle.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_plate'), toast);
      return false;
    }
    if (!CCCD.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_id_card'), toast);
      return false;
    }
    if (!validateCCCD(CCCD.value)) {
      FunctionGlobal.$notify("E", t('common.validation_id_card_too_long'), toast);
      return false;
    }
    if (!email.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_email'), toast);
      return false;
    }
    if (!validateEmail(email.value)) {
      FunctionGlobal.$notify("E", t('common.validation_invalid_email'), toast);
      return false;
    }
    if (!keySearchArea.value || !keySearchLocation.value) {
      return false;
    }
    return true;
  }
  if (type.value == "B") {
    if (!person_2.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_name'), toast);
      return false;
    }
    if (!phone_2.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_phone'), toast);
      return false;
    }
    if (!validatePhoneNumber(phone_2.value)) {
      FunctionGlobal.$notify("E", t('common.validation_invalid_phone'), toast);
      return false;
    }
    if (!CCCD_2.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_id_card'), toast);
      return false;
    }
    if (!email_2.value) {
      FunctionGlobal.$notify("E", t('common.validation_enter_email'), toast);
      return false;
    }
    if (!validateEmail(email_2.value)) {
      FunctionGlobal.$notify("E", t('common.validation_invalid_email'), toast);
      return false;
    }
    if (!keySearchArea_2.value || !keySearchLocation_2.value) {
      return false;
    }
    return true;
  }
};

const getAddress = (type) => {
  return props.data.filter((val) => {
    return val.type == type;
  });
};
const UpdateDefault = (data, id) => {
  data.forEach((el) => {
    if (el.id != id) el.default = "N";
  });
  ortherAddress.value = false;
};

const AddOrderAddress = async () => {
  const data = {
    id: type.value == "S" ? id.value : id_2.value,
    email: type.value == "S" ? email.value : email_2.value,
    Vehicle: type.value == "S" ? Vehicle.value : Vehicle_2.value,
    CCCD: type.value == "S" ? CCCD.value : CCCD_2.value,
    phone: type.value == "S" ? phone.value : phone_2.value,
    address: type.value == "S" ? address.value : address_2.value,
    areaId: type.value == "S" ? areaId.value : areaId_2.value,
    areaName: type.value == "S" ? keySearchArea.value.name : keySearchArea_2.value.name,
    locationId: type.value == "S" ? locationId.value : locationId_2.value,
    locationName:
      type.value == "S" ? keySearchLocation.value.name : keySearchLocation_2.value.name,
    type: type.value,
    default: "N",
    person: type.value == "S" ? person.value : person_2.value,
  };
  const payload = {
    id: type.value == "S" ? id.value : id_2.value,
    email: type.value == "S" ? email.value : email_2.value,
    vehiclePlate: type.value == "S" ? Vehicle.value : Vehicle_2.value,
    cccd: type.value == "S" ? CCCD.value : CCCD_2.value,
    phone: type.value == "S" ? phone.value : phone_2.value,
    address: type.value == "S" ? address.value : address_2.value,
    areaId: type.value == "S" ? areaId.value : areaId_2.value,
    areaName: type.value == "S" ? areaName.value : areaName_2.value,
    locationId: type.value == "S" ? locationId.value : locationId_2.value,
    locationName: type.value == "S" ? locationName.value : locationName_2.value,
    type: type.value,
    default: "N",
    person: type.value == "S" ? person.value : person_2.value,
  };
  submited.value = true;
  if (!ValidateData()) return;
  try {
    const API_URL =
      id.value || id_2.value
        ? API.update(`Customer/${props.customer.id}/addresses`, payload)
        : API.add(`Customer/${props.customer.id}/addresses`, payload);
    await API_URL;
    props.data.push(data);
  } catch (error) {
    console.error(error);
  }
  ortherAddress.value = false;
  const checkID = props.data.filter((val) => {
    return val.id == data.id;
  });
  if (checkID.length) return (props.data[findIndexById(0)] = { ...data });
};
const openEditAddr = (data) => { 
  id.value = data.id;
  person.value = data.person;
  address.value = data.address;
  email.value = data.email;
  keySearchArea.value = data.areaName;
  keySearchLocation.value = data.locationName;
  areaId.value = data.areaId;
  areaName.value = data.areaName;
  phone.value = data.phone;
  locationId.value = data.locationId;
  locationName.value = data.locationName;
  CCCD.value = data.cccd;
  Vehicle.value = data.vehiclePlate;
  type.value = data.type;
  ortherAddress.value = true;
};
const Address = (type) => {
  if (props.data == undefined) return "";
  const ad = props.data.filter((val) => {
    return val.type == type && val.default == "Y";
  });
  if (ad.length > 0) {
    return `${ad[0].address} - ${ad[0].locationName} - ${ad[0].areaName}`;
  } else {
    return "";
  }
};

const findIndexById = (id) => {
  let index = -1;
  for (let i = 0; i < props.data.length; i++) {
    if (props.data[i].id === id) {
      index = i;
      break;
    }
  }
  return index;
};
const discardForm = () => {
  ortherAddress.value = !ortherAddress.value;
  submited.value = false;
  address.value = "";
  address_2.value = "";
  keySearchArea.value = "";
  keySearchArea_2.value = "";
  keySearchLocation.value = "";
  keySearchLocation_2.value = "";
  areaId.value = "";
  areaName.value = "";
  areaId_2.value = "";
  areaName_2.value = "";
  phone.value = "";
  phone_2.value = "";
  locationId.value = "";
  locationName.value = "";
  locationId_2.value = "";
  locationName_2.value = "";
  CCCD.value = "";
  CCCD_2.value = "";
  Vehicle.value = "";
  Vehicle_2.value = "";
  person.value = "";
  person_2.value = "";
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
