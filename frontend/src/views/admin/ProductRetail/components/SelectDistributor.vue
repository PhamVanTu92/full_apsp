<template>
  <InputGroup class="w-full" @click="
    () => {
      if (!props.disabled) visibleList = true;
    }
  ">
    <InputText readonly fluid :value="placeholder" :placeholder="t('body.promotion.applicable_object_placeholder')"
      :class="{
        'cursor-pointer': !props.disabled,
        'cursor-auto': props.disabled,
      }"></InputText>
    <Button :disabled="props.disabled" icon="pi pi-list"/>
  </InputGroup>
  <Dialog @hide="closeDialog" v-model:visible="visible" :header="t('body.promotion.applicable_object_section_title')"
    :closable="true" class="w-9 md:w-7 lg:w-6" modal>
    <TabView v-model:activeIndex="activeIndex" :pt:panelContainer:class="'p-0'">
      <TabPanel :header="t('Custom.customer')">
        <DataTable id="cusdata" :selectAll="false" v-model:selection="selection.users" dataKey="id"
          :value="datastore.users" selectionMode="multiple" :rowsPerPageOptions="[5, 10, 20]" scrollable
          scrollHeight="52vh" style="min-height: 52vh">
          <template #header>
            <div class="flex justify-content-start w-full align-items-center">
              <InputText v-model="keyword" @input="onSearch" :placeholder="t('body.report.search_placeholder_2')" />
            </div>
          </template>
          <Column selectionMode="single" class="w-3rem"></Column> 
          <Column class="w-3rem">
            <template #body>
              <Image src="https://placeholder.co/46x46" width="46"></Image>
            </template>
          </Column>
          <Column>
            <template #body="{ data }">
              <div v-if="loading" class="flex flex-column gap-1 justify-content-center">
                <Skeleton width="60%" height="1.5rem" />
                <Skeleton width="30%" height="0.5rem" />
              </div>
              <div v-else class="flex flex-column gap-1 justify-content-center">
                <div class="font-semibold">{{ data.cardName }}</div>
                <div>{{ data.cardCode }}</div>
              </div>
            </template>
          </Column>
          <template #empty>
            <div class="flex justify-content-center align-items-center py-5" style="height: 52vh">
              <div class="text-base font-normal">{{ t('body.promotion.no_matching_result_message') }}</div>
            </div>
          </template>
        </DataTable>
      </TabPanel>
      <TabPanel :header="t('client.scopeCustomerGroup')">
        <!-- datastore.groups -->
        <DataTable v-model:selection="selection.groups" :value="datastore.groups" dataKey="id" selectionMode="multiple"
          scrollable scrollHeight="52vh" style="min-height: 52vh">
          <template #header>
            <div class="flex justify-content-start align-items-center flex-grow-1">
              <InputText :placeholder="t('body.report.search_placeholder_2')"></InputText>
            </div>
          </template>

          <Column selectionMode="multiple" class="w-3rem"></Column>
          <Column field="groupName"> </Column>
        </DataTable>
      </TabPanel>
    </TabView>
    <template #footer>
      <div class="flex justify-content-between w-full">
        <div class="flex gap-3 align-items-center">
          <Button @click="onClickUnSelected" text severity="danger" outlined>{{ t('Custom.deselect') }}</Button>
          <div>
            {{ placeholder }}
          </div>
        </div>
        <div class="flex gap-3">
          <Button @click="closeDialog" :label="t('client.cancel')" severity="secondary" icon="pi pi-times"/>
          <Button @click="confirmSelect" :label="t('client.confirm')" icon="pi pi-check"/>
          <Button v-if="0" label="log selection" icon="pi pi-bug"/>
        </div>
      </div>
    </template>
  </Dialog>

  <Dialog v-model:visible="visibleList" modal :header="selection?.type == 'G'
    ? t('Custom.selectedCustomerGroupList')
    : t('Custom.selectedCustomerList')
    " :style="{ width: '65rem' }">
    <DataTable :value="selection.users.length ? selection.users : selection.groups" tableStyle="min-width: 50rem">
      <template #empty>
        <div class="flex justify-content-center align-items-center py-5" style="height: 52vh">
          <div class="text-base font-normal">{{ t('Custom.noCustomerSelected') }}</div>
        </div>
      </template>
      <Column header="#">
        <template #body="{ index }">
          <span>{{ index + 1 }}</span>
        </template>
      </Column>
      <Column :field="selection?.type == 'G' ? '' : 'cardCode'" :header="t('client.code')"></Column>
      <Column :field="selection?.type == 'G' ? 'groupName' : 'cardName'" :header="t('body.promotion.name')"></Column>
      <Column :header="t('Custom.action')" headerClass="w-10rem">
        <template #body="{ data }">
          <Button icon="pi pi-trash" text severity="danger" @click="removeUser(data)"/>
        </template>
      </Column>
    </DataTable>
    <template #footer>
      <div class="flex justify-content-between w-full">
        <Button :label="t('Custom.chooseCustomerOrGroup')" outlined @click="onClickOpenDialog()"/>
        <Button type="button" :label="t('client.cancel')" severity="secondary" @click="visibleList = false"/>
      </div>
    </template>
  </Dialog>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from "vue";
import API from "@/api/api-main";
import { useInfiniteScroll } from "@vueuse/core";
import { debounce } from "lodash";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const visible = ref(false);
const vmodelSelection = defineModel("selection", {
  default: {
    type: null,
    items: [],
  },
});
const props = defineProps({
  disabled: {
    default: false,
  },
  selection: {
    default: [],
  },
  customerData: {
    default: [],
  },
  inputCheckBox: {
    default: false,
  },
});
const emits = defineEmits(["update:selection", "change"]);
const placeholder = ref("");
const keyword = ref("");
const loading = ref(false);
const visibleList = ref(false);
const datastore = reactive({
  users: [],
  groups: [],
});
const selection = ref({
  type: null,
  users: [],
  groups: [],
});
const activeIndex = ref(0);
let scrollElement = ref();
var flag = ref(true);

const selectionKeys = ref();

const findGroup = (keys, array) => {
  let result = [];
  array.forEach((group) => {
    if (keys.includes(group.key)) {
      result.push(group.data);
    }
    if (group.children) {
      result = result.concat(findGroup(keys, group.children));
    }
  });

  return result;
};

const onSearch = (event) => {
  db();
};

const doSearch = () => {
  flag.value = true;
  query.skip = 0;
  fetchUsers(keyword.value, true);
  query.skip++;
};

const db = debounce(() => {
  doSearch();
}, 1000);

const onClickUnSelected = () => {
  selection.value.users = [];
  selection.value.groups = [];
};

useInfiniteScroll(
  scrollElement,
  () => {
    if (!loading.value && flag.value) {
      fetchUsers(keyword.value);
      query.skip++;
    }
  },
  { distance: 100 }
);

const onClickOpenDialog = () => {
  if (!props.disabled) {
    visible.value = true;
    setTimeout(() => {
      scrollElement.value = document.querySelector("#cusdata > div.p-datatable-wrapper");
    }, 0); // :D
    query.skip = 0;
    fetchUsers(keyword.value, true);
    query.skip++;
  }
  if (vmodelSelection.value.type == "G") {
    activeIndex.value = 1;
  } else {
    activeIndex.value = 0;
  }
};

const confirmSelect = async () => {
  const data = {
    type: activeIndex.value ? "G" : "C",
    items: activeIndex.value ? selection.value.groups : selection.value.users,
  };
  if (activeIndex.value == 0) {
    data.items = selection.value.users;
    selection.value.groups = [];
    selectionKeys.value = {};
  } else {
    data.items = selection.value.groups;
    selection.value.users = [];
  }
  emits("update:selection", data);
  emits("change", data);
  await setLabel(data);
  visible.value = false;
  visibleList.value = false;
};

const closeDialog = () => {
  visible.value = false;
};

const query = reactive({
  skip: 0,
  limit: 50,
});

const fetchUsers = (keyword = null, isAssgin = false) => {
  let _query = encodeURIComponent(keyword?.trim() || "");
  let search = _query ? "&search=" + _query : "";
  let url = `customer?skip=${query.skip}&filter=(status=A)&limit=${query.limit}${search}`;
  loading.value = true;
  API.get(url)
    .then((response) => {
      loading.value = false;
      if (isAssgin) {
        datastore.users = response.data.items;
        if (
          vmodelSelection.value &&
          selection.value.users.length < vmodelSelection.value.items?.length
        ) {
          let selectedId = vmodelSelection.value.items.map((user) => user.customerId);
          selection.value.users = datastore.users.filter((user) =>
            selectedId.includes(user.id)
          );
        }
        return;
      }
      if (response.data.items?.length) {
        datastore.users.push(...response.data.items);
        if (
          vmodelSelection.value &&
          selection.value.users.length < vmodelSelection.value.items?.length
        ) {
          let selectedId = vmodelSelection.value.items.map((user) => user.customerId);
          let users = datastore.users.filter((user) => selectedId.includes(user.id));
          selection.value.users.push(...users);
        }
      } else flag.value = false;
    })
    .catch((error) => {
      loading.value = false;
    });
};

const mapGroup = (array, fkey = null) => {
  let result = [];
  for (let i = 0; i < array.length; i++) {
    let group = {
      key: fkey ? `${fkey}-${array[i].id}` : `${array[i].id}`,
      children: mapGroup(array[i].child, `${array[i].id}`),
      label: array[i].groupName,
      data: {
        ...array[i],
      },
    };
    result.push(group);
  }
  return result;
};

const fetchGroups = () => {
  //   API.get("customergroup?skip=0&limit=1000000000&filter=(status=A)")
  //     .then((res) => {
  //       datastore.groups = res.data.bpGroup;
  //     })
  //     .catch((error) => {});
  API.get("customergroup?skip=0&limit=1000000000&filter=(isActive=true)")
    .then((res) => {
      datastore.groups = res.data.bpGroup;
    })
    .catch((error) => { });
};

const ConvertCustomerLocal = async (data, type) => {
  if (Array.isArray(data) && data.length > 0) {
    const customer = await Promise.all(
      data.map((el) =>
        type == "C" ? getByIdCustomer(el.id) : getByIdCustomerGroup(el.id)
      )
    );

    return customer;
  }

  return { type: "", items: [] };
};

const getByIdCustomer = async (id) => {
  try {
    const { data } = await API.get(`Customer/${id}`);
    if (data) return data;
  } catch (error) {
    return null;
  }
};

const getByIdCustomerGroup = async (id) => {
  try {
    const { data } = await API.get(`CustomerGroup/${id}`);
    if (data) return data;
  } catch (error) {
    return null;
  }
};

const removeUser = async (item) => {
  if (selection.value?.type == "C") {
    selection.value.users = selection.value.users.filter((e) => e != item);
  } else {
    selection.value.groups = selection.value.users.filter((e) => e != item);
  }

  const data = {
    type: selection.value?.type,
    items: selection.value?.type == "G" ? selection.value.groups : selection.value.users,
  };
  if (selection.value?.type == "C") {
    data.items = selection.value.users;
    selection.value.groups = [];
    selectionKeys.value = {};
  } else {
    data.items = selection.value.groups;
    selection.value.users = [];
  }
  emits("update:selection", data);
  emits("change", data);
  await setLabel(data);
};

const setLabel = async (newVal) => {
  if (newVal.items?.length) {
    let _type = t('Custom.customer');
    if (newVal.type == "G") _type = `${t('Custom.group')} ${_type}`;
    placeholder.value = `${t('body.productManagement.selected_label')} ${newVal.items.length} ${_type}`;
    selection.value.type = newVal.type;
    if (Array.isArray(newVal.items) && newVal.items.length > 0) {
      if (newVal.type == "C") {
        selection.value.users = await ConvertCustomerLocal(newVal.items, "C");
      } else {
        selection.value.groups = await ConvertCustomerLocal(newVal.items, "G");
      }
    }
  } else {
    placeholder.value = "";
  }
};

watch(
  () => vmodelSelection.value,
  async (newVal) => {
    if (newVal) {
      if (newVal?.length) {
        newVal = newVal[0];
        await setLabel(newVal);
      }
    }
  }
);

onMounted(async () => {
  if (vmodelSelection.value.length) await setLabel(vmodelSelection.value[0]);
  fetchGroups();
});
</script>

<style></style>
