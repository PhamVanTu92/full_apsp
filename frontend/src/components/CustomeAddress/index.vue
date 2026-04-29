<template>
  <div>
    <Dialog
      @hide="onHide"
      v-model:visible="visible"
      :header="header"
      style="width: 50rem"
      modal
    >
      <!-- {{ selected }} -->
      <DataTable
        v-model:selection="selected"
        :value="dataTable"
        selectionMode="single"
        class="card p-0 overflow-hidden"
        :pt="{
          headerrow: {
            class: 'hidden',
          },
        }"
        scrollable
        scrollHeight="443px"
        :loading="loading"
        dataKey="id"
        @row-unselect="onRowUnselect"
        v-model:filters="filters"
        :globalFilterFields="[
          'person',
          'email',
          'phone',
          'status',
          'address',
          'locationName',
          'areaName',
        ]"
      >
        <Column selectionMode="single" class="w-1rem"></Column>
        <Column>
          <template #body="{ data }">
            <div class="flex flex-column gap-2">
              <div>
                <span>
                  {{ t("client.contact_person") }}:
                  <span class="font-semibold">{{ data.person }}</span>
                </span>
              </div>
              <div>
                <span>
                  {{ t("client.citizen_id") }}:
                  <span class="font-semibold">{{ data.cccd }}</span>
                </span>
              </div>
              <div>
                <span>
                  {{ t("client.address") }}:
                  <span class="font-semibold">{{ toStringAddress(data) }}</span>
                </span>
              </div>
              <div>
                <span>
                  {{ t("client.email") }}:
                  <span class="font-semibold">{{ data.email }}</span>
                </span>
              </div>
              <div>
                <span>
                  {{ t("client.phone_number") }}:
                  <span class="font-semibold">{{ data.phone }}</span>
                </span>
              </div>
            </div>
          </template>
        </Column>
        <Column class="w-1rem">
          <template #body="{ data }">
            <div class="flex">
              <Button icon="pi pi-pencil" text @click="openAddressDlg(data.id)" />
              <Button
                icon="pi pi-trash"
                text
                severity="danger"
                :disabled="data.default == 'Y'"
                @click="onDelete(data.id)"
              />
            </div>
          </template>
        </Column>
        <template #header>
          <div class="flex justify-content-between">
            <IconField iconPosition="left">
              <InputIcon class="pi pi-search"> </InputIcon>
              <InputText
                :placeholder="t('body.home.search_placeholder')"
                v-model="filters['global'].value"
              />
            </IconField>
            <Button
              :label="t('client.add_contact')"
              outlined
              icon="pi pi-plus-circle"
              @click="openAddressDlg()"
            />
          </div>
        </template>
        <template #empty>
          <div class="my-5 py-5 text-center font-italic text-500">
            <span v-if="loading">{{ t("report.loading_message") }}</span>
            <span v-else>{{ t("client.no_data") }}</span>
          </div>
        </template>
      </DataTable>
      <template #footer>
        <Button
          :label="t('body.OrderList.close')"
          severity="secondary"
          @click="visible = false"
        />
        <Button
          :label="t('client.set_as_default')"
          @click="onChoseDefault()"
          :loading="loading2"
        />
      </template>
    </Dialog>
    <Dialog
      v-model:visible="visible1"
      :header="payload.id ? t('client.edit_contact') : t('client.add_contact')"
      modal
      class="w-30rem"
      @hide="onHideAI"
    >
      <div class="flex flex-column gap-3">
        <div class="flex flex-column gap-2">
          <label for=""
            >{{ t("client.contact_person") }} <sup class="text-red-500">*</sup></label
          >
          <InputText
            :invalid="errMsg.person ? true : false"
            v-model="payload.person"
          ></InputText>
          <small v-if="errMsg.person" class="text-red-500">{{ errMsg.person }}</small>
        </div>
        <div class="flex flex-column gap-2">
          <label for=""
            >{{ t("client.phone_number") }} <sup class="text-red-500">*</sup></label
          >
          <InputText
            :invalid="errMsg.phone ? true : false"
            v-model="payload.phone"
            class="input_number"
            type="number"
          >
          </InputText>
          <small v-if="errMsg.phone" class="text-red-500">{{ errMsg.phone }}</small>
        </div>
        <div class="flex flex-column gap-2" v-if="type == 'S'">
          <label for=""
            >{{ t("client.license_plate") }} <sup class="text-red-500">*</sup></label
          >
          <InputText
            :invalid="errMsg.vehiclePlate ? true : false"
            v-model="payload.vehiclePlate"
          ></InputText>
          <small v-if="errMsg.vehiclePlate" class="text-red-500">{{
            errMsg.vehiclePlate
          }}</small>
        </div>
        <div class="flex flex-column gap-2" v-if="type == 'S'">
          <label for=""
            >{{ t("client.citizen_id") }} <sup class="text-red-500">*</sup></label
          >
          <InputText
            :invalid="errMsg.cccd ? true : false"
            v-model="payload.cccd"
          ></InputText>
          <small v-if="errMsg.cccd" class="text-red-500">{{ errMsg.cccd }}</small>
        </div>
        <div class="flex flex-column gap-2">
          <label for=""
            >{{ t("client.email") }}
            <sup v-if="type == 'B'" class="text-red-500">*</sup></label
          >
          <InputText
            :invalid="errMsg.email ? true : false"
            v-model="payload.email"
          ></InputText>
          <small v-if="errMsg.email" class="text-red-500">{{ errMsg.email }}</small>
        </div>
        <div class="flex align-items-center gap-2">
          <hr class="m-0 flex-grow-1" />
          <span class="font-semibold">{{ t("client.address") }}</span>
          <hr class="m-0 flex-grow-1" />
        </div>
        <div class="flex gap-3">
          <span class="mr-3"
            >{{ t("client.address_type") || t("client.address") }}:
          </span>
          <span>
            <RadioButton
              @change="
                () => {
                  onClear('location');
                  onClear('area');
                }
              "
              inputId="DM"
              v-model="payload.locationType"
              class="mr-2"
              value="DM"
            ></RadioButton>
            <label for="DM">{{ t("client.domestic") }}</label>
          </span>
          <span>
            <RadioButton
              @change="
                () => {
                  onClear('location');
                  onClear('area');
                }
              "
              inputId="INT"
              v-model="payload.locationType"
              class="mr-2"
              value="INT"
            ></RadioButton>
            <label for="INT">{{ t("client.international") }}</label>
          </span>
        </div>
        <div class="flex flex-column gap-2" v-if="['DM'].includes(payload.locationType)">
          <label for="">{{ t("client.area") }} <sup class="text-red-500">*</sup></label>
          <AutoComplete
            v-model="selections.area"
            :invalid="errMsg.areaId ? true : false"
            :placeholder="t('client.enter_area')"
            :suggestions="suggestions.area"
            optionLabel="name"
            :pt:input:class="'w-full'"
            @complete="onComplete($event as any, 'area')"
            @item-select="onSelectAddress($event as any, 'area')"
            @change="onClear('area')"
          >
          </AutoComplete>
          <small v-if="errMsg.areaId" class="text-red-500">{{ errMsg.areaId }}</small>
        </div>
        <div class="flex flex-column gap-2" v-if="['DM'].includes(payload.locationType)">
          <label for="">{{ t("client.ward_commune") }} </label>
          <AutoComplete
            v-model="selections.location"
            :invalid="errMsg.locationId ? true : false"
            :placeholder="t('client.enter_ward')"
            optionLabel="name"
            :suggestions="suggestions.location"
            :pt:input:class="'w-full'"
            @complete="onComplete($event as any, 'location')"
            @change="onClear('location')"
            @item-select="onSelectAddress($event as any, 'location')"
            :disabled="!payload.areaId"
          >
          </AutoComplete>
          <small v-if="errMsg.locationId" class="text-red-500">{{
            errMsg.locationId
          }}</small>
        </div>
        <div class="flex flex-column gap-2">
          <label for=""
            >{{ t("client.detailed_address") }} <sup class="text-red-500">*</sup></label
          >
          <Textarea
            class="w-full"
            :rows="2"
            :invalid="errMsg.address ? true : false"
            v-model="payload.address"
          ></Textarea>
          <small v-if="errMsg.address" class="text-red-500">{{ errMsg.address }}</small>
        </div>
      </div>
      <template #footer>
        <Button
          :label="t('body.OrderList.close')"
          severity="secondary"
          @click="visible1 = false"
        />
        <Button :label="t('client.save')" @click="onClickSave" :loading="loading" />
      </template>
    </Dialog>
    <!-- <ConfirmDialog /> -->
  </div>
</template>

<script setup lang="ts">
  import { useToast } from "primevue/usetoast";
  import { ref, reactive, computed, onMounted } from "vue";
  import API from "../../api/api-main";
  import { AddressInfo, IAddressInfo, ResponeLocation } from "./entities/AddressInfo";
  import { useConfirm } from "primevue/useconfirm";
  import { useI18n } from "vue-i18n";

  const { t } = useI18n();

  // /Customer/{id}/addresses : POST
  const toast = useToast();
  const payload = ref<IAddressInfo>(new AddressInfo());
  const confirm = useConfirm();

  const suggestions = reactive({
    area: [],
    location: [],
  });
  const selections = reactive<{
    area: any | null;
    location: any | null;
  }>({
    area: null,
    location: null,
  });
  const cid = ref<number>();
  const selected = ref<IAddressInfo>(new AddressInfo());
  const type = ref<"B" | "S">();
  const visible = ref(false);
  const visible1 = ref(false);
  const header = ref("");
  const dataTable = ref([]);
  const loading = ref(false);
  const loading2 = ref(false);

  const onRowUnselect = (e: any) => {
    selected.value = e.data;
  };

  const emits = defineEmits(["change-default"]);
  const onChoseDefault = () => {
    selected.value.default = "Y";
    loading2.value = true;
    API.update(`Customer/${cid.value}/addresses`, selected.value)
      .then((res) => {
        toast.add({
          severity: "success",
          summary: t("body.systemSetting.success_label"),
          detail: t("Custom.default_address"),
          life: 3000,
        });
        emits("change-default", selected.value);
        visible.value = false;
        visible1.value = false;
      })
      .catch((error) => {
        toast.add({
          severity: "error",
          summary: t("body.report.error_occurred_message"),
          detail: t("Custom.error_occurred"),
          life: 3000,
        });
        console.error(error);
      })
      .finally(() => {
        loading2.value = false;
      });
  };

  import { FilterMatchMode } from "primevue/api";
  const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  });

  const onDelete = (id: number) => {
    confirm.require({
      message: t("Custom.confirm_delete"),
      header: t("client.confirm"),
      icon: "pi pi-exclamation-triangle",
      rejectClass: "p-button-secondary",
      acceptClass: "p-button-danger",
      rejectLabel: t("client.cancel"),
      acceptLabel: t("body.OrderList.delete"),
      accept: () => {
        API.delete(`Customer/${cid.value}/addresses`, [id])
          .then((res) => {
            toast.add({
              severity: "success",
              summary: t("body.systemSetting.success_label"),
              detail: "t('Custom.delete_success')",
              life: 3000,
            });
            if (cid.value) {
              fetchCustomerById(cid.value);
            }
          })
          .catch((error) => {
            toast.add({
              severity: "error",
              summary: t("body.report.error_occurred_message"),
              detail: t("Custom.error_occurred"),
              life: 3000,
            });
          });
      },
    });
  };

  const errMsg = ref<any | IAddressInfo>({});
  const onClickSave = () => {
    visible1.value = true;
    const vresult = payload.value.validate(payload.value.type);
    if (!vresult.result) {
      errMsg.value = vresult.errors;
      return;
    }
    if (payload.value.id) {
      API.update(`Customer/${cid.value}/addresses`, payload.value)
        .then(() => {
          toast.add({
            severity: "success",
            summary: t("body.systemSetting.success_label"),
            detail: t("client.update_success"),
            life: 3000,
          });
          if (cid.value) {
            fetchCustomerById(cid.value);
          }
        })
        .catch((error) => {
          toast.add({
            severity: "error",
            summary: t("body.report.error_occurred_message"),
            detail: t("Custom.update_failed"),
            life: 3000,
          });
          console.error(error);
        })
        .finally(() => {
          visible1.value = false;
        });
    } else {
      payload.value.type = type.value || "S";
      API.add(`Customer/${cid.value}/addresses`, payload.value)
        .then(() => {
          toast.add({
            severity: "success",
            summary: t("body.systemSetting.success_label"),
            detail: t("client.update_success"),
            life: 3000,
          });
          if (cid.value) {
            fetchCustomerById(cid.value);
          }
        })
        .catch((error) => {
          toast.add({
            severity: "error",
            summary: t("body.report.error_occurred_message"),
            detail: t("Custom.update_failed"),
            life: 3000,
          });
          console.error(error);
        })
        .finally(() => {
          visible1.value = false;
        });
    }
  };

  const onClear = (path: "area" | "location") => {
    if (path == "area") {
      payload.value.areaId = null;
      payload.value.areaName = null;
      payload.value.locationId = null;
      payload.value.locationName = null;
      selections.location = null;
    }
    if (path == "location") {
      payload.value.locationId = null;
      payload.value.locationName = null;
    }
  };

  const onSelectAddress = (
    event: { originalEvent: InputEvent; value: ResponeLocation },
    path: "area" | "location"
  ) => {
    if (path == "area") {
      payload.value.areaId = event.value.id;
      payload.value.areaName = event.value.name;
    }
    if (path == "location") {
      payload.value.locationId = event.value.id;
      payload.value.locationName = event.value.name;
    }
  };

  const onComplete = (
    event: { originalEvent: InputEvent; query: string },
    path: "area" | "location"
  ) => {
    const search = encodeURIComponent(event.query?.trim());
    switch (path) {
      case "area":
        API.get(`Area/search/${search}`).then((res) => {
          suggestions.area = res.data;
        });
        break;
      case "location":
        if (payload.value.areaId) {
          API.get(`Area/search/${payload.value.areaId}/${search}`).then((res) => {
            suggestions.location = res.data;
          });
        }
        break;
      default:
        break;
    }
  };

  const openAddressDlg = (id?: number) => {
    if (id) {
      const ai = dataTable.value.find((el: IAddressInfo) => el.id == id);
      payload.value = new AddressInfo(ai);
      selections.area = {
        name: payload.value.areaName,
        id: payload.value.areaId,
      };
      selections.location = {
        name: payload.value.locationName,
        id: payload.value.locationId,
      };
    } else {
      payload.value = new AddressInfo();
      payload.value.type = type.value || "S";
    }
    visible1.value = true;
  };

  const fetchCustomerById = (id: number) => {
    loading.value = true;
    API.get(`customer/${id}`)
      .then((res) => {
        dataTable.value = res.data?.crD1?.filter((cus: any) => {
          return cus.type == type.value;
        });
        selected.value = (dataTable.value?.find(
          (el: IAddressInfo) => el.default == "Y"
        ) || {}) as IAddressInfo;
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        // Close loading
        loading.value = false;
      });
  };
  const openDialog = (id: number, _type: "B" | "S") => {
    visible.value = true;
    type.value = _type;
    cid.value = id;
    fetchCustomerById(id);
    if (_type == "S") {
      header.value = t("client.update_delivery_info");
    } else if (_type == "B") {
      header.value = t("client.update_invoice_address");
    }
  };

  const toStringAddress = (data: any): string => {
    let result = "";
    if (data) {
      result = [data.address, data.locationName, data.areaName]
        .filter((el) => el)
        .join(", ");
    }
    return result;
  };

  const onHideAI = () => {
    payload.value = new AddressInfo();
    errMsg.value = {};
    selections.area = null;
    selections.location = null;
  };

  const onHide = () => {
    visible.value = false;
    selected.value = {} as IAddressInfo;
    errMsg.value = {};
    dataTable.value = [];
    header.value = "";
  };

  defineExpose({
    openDialog,
    header,
  });

  onMounted(function () {});
</script>

<style scoped></style>
