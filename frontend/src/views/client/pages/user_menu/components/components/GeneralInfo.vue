<template>
  <div class="card p-0">
    <div class="flex justify-content-between p-3">
      <div class="py-2 text-primary text-xl font-semibold">
        {{ t("client.customer_info") }}
      </div>
      <!-- <Button
                v-if="!enableEdit"
                :disabled="enableEdit"
                @click="onClickEdit"
                icon="pi pi-pencil"
                :label="t('client.edit')"
            /> -->
      <div v-if="enableEdit" class="flex gap-3">
        <Button
          @click="onClickCancelEdit"
          icon="pi pi-times"
          :label="t('client.cancel')"
          severity="secondary"
        />
        <Button
          :loading="loading"
          @click="onClickSave"
          icon="pi pi-save"
          :label="t('client.save')"
        />
      </div>
    </div>
    <hr class="m-0" />
    <!-- {{ props.data }} -->
    <div class="grid m-0">
      <div class="col-3 flex flex-column align-items-center gap-3">
        <Image
          class="mt-2"
          :src="imageUrl || 'https://placehold.co/200x200'"
          width="200"
          height="200"
        />
        <Button
          v-if="enableEdit"
          icon="pi pi-image"
          :label="t('body.systemSetting.choose_file')"
          style="width: 200px"
          @click="onClickOpenFile"
        />
        <input type="file" @change="onSelectImage" id="imageInput" accept="image/*" />
      </div>
      <div class="col-9">
        <div class="grid align-items-center m-0">
          <div class="col-3 pt-0">
            <div class="font-bold" for="tdn">{{ t("client.business_name") }}</div>
            <sup v-if="enableEdit" class="ml-1 font-bold text-red-500">*</sup>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.cardName || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText id="tdn" v-model="modelEdit.cardName" class="w-full" />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="tk">{{ t("client.contact_name") }}</div>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.frgnName || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText id="tk" v-model="modelEdit.frgnName" class="w-full" />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="mst">{{ t("client.tax_code") }}</div>
            <sup v-if="enableEdit" class="ml-1 font-bold text-red-500">*</sup>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.licTradNum || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputMask mask="9999999999?-999" id="mst" v-model="modelEdit.licTradNum" />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="tnd">{{ t("client.account_name") }}</div>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.userName || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText id="tnd" :value="props.data.userName" class="w-full" disabled />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="tk">{{ t("client.representative") }}</div>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.person || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText id="tk" v-model="modelEdit.person" class="w-20rem" />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="tk">{{ t("client.dob") }}</div>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            <!-- {{ props.data.bpInfo?.dateOfBirth }} -->
            {{
              props.data.bpInfo?.dateOfBirth
                ? format(new Date(props.data.bpInfo?.dateOfBirth), "dd/MM/yyyy")
                : null || "-"
            }}
          </div>
          <div v-else class="col-9 pt-0">
            <Calendar
              disabled
              id="tk"
              v-model="modelEdit.dateOfBirth"
              class="w-10rem"
              dateFormat="dd/mm/yy"
            />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="email">{{ t("client.email") }}</div>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.email || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText id="email" v-model="modelEdit.email" class="w-20rem" />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="phone">{{ t("client.phoneNumber") }}</div>
            <sup v-if="enableEdit" class="ml-1 font-bold text-red-500">*</sup>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ props.data.bpInfo?.phone || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <InputText
              id="phone"
              type="number"
              v-model="modelEdit.phone"
              class="input_number"
            />
          </div>
          <div class="col-3 pt-0">
            <div class="font-bold" for="dc">{{ t("client.address") }}</div>
            <sup v-if="enableEdit" class="ml-1 font-bold text-red-500">*</sup>
          </div>
          <div v-if="!enableEdit" class="col-9 pt-0">
            {{ addressLabel || "-" }}
          </div>
          <div v-else class="col-9 pt-0">
            <SelectAddress inputId="dc" v-model="modelEdit.address" class="w-full" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, reactive, computed } from "vue";
  import API from "@/api/api-main";
  import { useToast } from "primevue/usetoast";
  import { format } from "date-fns";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();
  const toast = useToast();
  const emits = defineEmits(["on-submit"]);
  const loading = ref(false);
  const props = defineProps({
    data: {
      type: Object,
      default: {
        bpInfo: {},
      },
    },
  });

  const modelEdit = reactive({
    id: null,
    cardName: null,
    frgnName: null,
    licTradNum: null,
    cardName: null,
    phone: null,
    email: null,
    cardName: null,
    address: {},
  });

  const enableEdit = ref(false);

  const imageUrl = ref(null);
  const image = ref(null);
  const onSelectImage = () => {
    image.value = document.getElementById("imageInput").files[0];
    const blob = new Blob([image.value]);
    imageUrl.value = URL.createObjectURL(blob);
  };
  const onClickOpenFile = () => {
    document.getElementById("imageInput").click();
  };

  const onClickEdit = () => {
    assignData();
    enableEdit.value = true;
  };

  const assignData = () => {
    modelEdit.id = props.data.cardId;
    modelEdit.cardName = props.data.bpInfo?.cardName;
    modelEdit.frgnName = props.data.bpInfo?.frgnName;
    modelEdit.licTradNum = props.data.bpInfo?.licTradNum;
    modelEdit.cardName = props.data.userName;
    modelEdit.phone = props.data.bpInfo?.phone;
    modelEdit.email = props.data.bpInfo?.email;
    imageUrl.value = props.data.avatar;
    modelEdit.cardName = props.data.bpInfo?.cardName;
    modelEdit.person = props.data.bpInfo?.person;
    modelEdit.dateOfBirth = props.data.bpInfo?.dateOfBirth
      ? new Date(props.data.bpInfo?.dateOfBirth)
      : null;
    modelEdit.address = {
      address: props.data.bpInfo?.address,
      locationId: props.data.bpInfo?.locationId,
      locationName: props.data.bpInfo?.locationName,
      areaId: props.data.bpInfo?.areaId,
      areaName: props.data.bpInfo?.areaName,
    };
  };

  const addressLabel = computed(() => {
    let address = [
      props.data.bpInfo?.address,
      props.data.bpInfo?.locationName,
      props.data.bpInfo?.areaName,
    ];
    return address.filter((el) => el).join(", ");
  });

  const onClickSave = () => {
    let { address, ...payload } = modelEdit;
    Object.assign(payload, address);
    const formData = new FormData();
    if (image.value) {
      payload.avatar = image.value.name;
      formData.append("image", image.value);
    }
    formData.append("item", JSON.stringify(payload));
    loading.value = true;
    API.update("customer/me", formData)
      .then((res) => {
        if (res.status == 200) {
          loading.value = false;
          enableEdit.value = false;
          emits("on-submit", res.data);
          toast.add({
            severity: "success",
            summary: "Thông báo",
            detail: "Cập nhật thành công.",
            life: 3000,
          });
        }
      })
      .catch((error) => {
        toast.add({
          severity: "error",
          summary: "Thông báo",
          detail: "Cập nhật thất bại. Vui lòng thử lại sau.",
          life: 3000,
        });
        loading.value = false;
      });
  };

  const onClickCancelEdit = () => {
    imageUrl.value = props.data.avatar;
    image.value = null;
    enableEdit.value = false;
  };

  onMounted(() => {
    assignData();
  });
</script>

<style scoped>
  label {
    line-height: 33px;
  }

  #imageInput {
    display: none;
  }

  .input_number::-webkit-outer-spin-button,
  .input_number::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
  }

  /* Firefox */
  .input_number[type="number"] {
    -moz-appearance: textfield;
  }

  .sticky_class {
    position: sticky;
    top: 160px;
    /* background-color: white; */
  }
</style>
