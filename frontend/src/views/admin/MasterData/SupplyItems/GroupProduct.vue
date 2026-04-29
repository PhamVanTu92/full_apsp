<script setup>
import { ref, reactive, onMounted, watch } from "vue";
import API from "@/api/api-main";
import { cloneDeep, debounce } from "lodash";
import { useGlobal } from "@/services/useGlobal";
import Dropdown from "primevue/dropdown";

const { toast, FunctionGlobal } = useGlobal();
const visible = ref();
const loading = ref(false);
const comfirmDele = ref();
const DataGroup = ref([]);
const DataProduct = ref([]);
const payLoad = ref();
const submited = ref();
const keySearchCusGrp = ref("");
const condition = ref([
  { code: "brand", name: "Thương hiệu", data: [] },
  { code: "industry", name: "Ngành hàng", data: [] },
  { code: "productGroups", name: "Nhóm hàng", data: [] },
  { code: "productApplications", name: "Ứng dụng", data: [] },
  { code: "itemType", name: "Loại sản phẩm", data: [] },
  { code: "packing", name: "Quy cách bao bì", data: [] },
  { code: "productQualityLevles", name: "Cấp chất lượng", data: [] },
]);

onMounted(async () => {
  await GetAllCustomer();
  DataProduct.value = await getProducts();
  const dataConditon = await getValueCondition();
  condition.value.find((el) => el.code == "brand").data = dataConditon.brand;
  condition.value.find((el) => el.code == "industry").data = dataConditon.industry;
  condition.value.find((el) => el.code == "productGroups").data =
    dataConditon.productGroups;
  condition.value.find((el) => el.code == "productApplications").data =
    dataConditon.productApplications;
  condition.value.find((el) => el.code == "itemType").data = dataConditon.itemType;
  condition.value.find((el) => el.code == "packing").data = dataConditon.packing;
  condition.value.find((el) => el.code == "productQualityLevles").data =
    dataConditon.productQualityLevles;
});

const initData = () => {
  submited.value = false;
  visible.value = false;
  comfirmDele.value = false;
  payLoad.value = {
    id: 0,
    groupName: "",
    groupType: "",
    isSelected: false,
    isOneOfThem: true,
    description: "",
    listUser: [],
    customers: [],
    conditionCusGroups: [
      {
        id: 0,
        groupId: 0,
        isEqual: true,
        typeCondition: "",
        values: [],
      },
    ],
    isActive: true,
  };
};
initData();

const GetAllCustomer = async () => {
  loading.value = true;
  try {
    const res = await API.get("ItemGroup?skip=0&limit=30");
    DataGroup.value = res.data.bpGroup;
  } catch (er) {
    FunctionGlobal.$notify("E", er, toast);
  } finally {
    loading.value = false;
  }
};

const onFilter = async (keySearch) => {
  let uri = "";
  let res = [];
  try {
    keySearch.trim() != "" ? (uri = `ItemGroup/search/${keySearch.trim()}`) : (uri = "");
    uri == "" ? await GetAllCustomer() : (res = await API.get(uri));
    uri != "" ? (DataGroup.value = await res.data) : null;
  } catch (er) {
    FunctionGlobal.$notify("E", er, toast);
  } finally {
    loading.value = false;
  }
};

const debounceF = debounce((keySearch) => {
  onFilter(keySearch);
}, 1000);

const SaveGroup = async () => {
  submited.value = true;
  if (Validate()) return;
  try {
    loading.value = true;
    const data = cloneDeep(payLoad.value);
    data.conditionCusGroups.forEach((el) => {
      el.values = el.values.map(String);
    });
    const dataHeader = {
      id: data.id,
      groupName: data.groupName,
      groupType: data.groupType,
      isActive: data.isActive,
      description: data.description,
      parentId: data.parentId,
      isSelected: data.isSelected,
      isOneOfThem: data.isOneOfThem,
    };
    const FUNAPI = data.id
      ? await API.update(`ItemGroup/${data.id}`, dataHeader)
      : await API.add("ItemGroup/add", dataHeader);
    const response = FUNAPI;

    if (response?.data) {
      if (!data.isSelected) {
        const Line = await SaveLineCondition(data.conditionCusGroups, response.data.id);
        if (!Line) {
          FunctionGlobal.$notify("E", "Lỗi không lưu được điều kiện", toast);
          return;
        }
        if (data.conditionCusGroups.filter((e) => e.statusDB == "D").length) {
          const DataDelete = data.conditionCusGroups.filter((e) => e.statusDB == "D");
          const Delete = await DeleteCondition(
            DataDelete.map((e) => e.id),
            response.data.id
          );
          if (!Delete) {
            FunctionGlobal.$notify("E", "Lỗi không xóa được điều kiện", toast);
            return;
          }
        }
      } else {
        if (data.id) {
          const newCustomer = data.customers
            .filter((e) => e.statusDB == "A")
            .map((e) => e.id);

          if (newCustomer.length) {
            const User = await SaveLineUser(newCustomer, response.data.id);
            if (!User) {
              FunctionGlobal.$notify("E", "Lỗi không lưu được sản phẩm", toast);
              return;
            }
          }
          const DeleteCustomer = data.customers
            .filter((e) => e.statusDB == "D")
            .map((e) => e.id);

          if (DeleteCustomer.length) {
            const User = await DeleteUser(DeleteCustomer, response.data.id);
            if (!User) {
              FunctionGlobal.$notify("E", "Lỗi không lưu được sản phẩm", toast);
              return;
            }
          }
        } else {
          const User = await SaveLineUser(data.listUser, response.data.id);
          if (!User) {
            FunctionGlobal.$notify("E", "Lỗi không lưu được sản phẩm", toast);
            return;
          }
        }
      }
    }
    const mess = data.id ? "Cập nhật thành công" : "Thêm mới thành công";
    FunctionGlobal.$notify("S", mess, toast);
    initData();
    await GetAllCustomer();
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  } finally {
    loading.value = false;
  }
};

const SaveLineCondition = async (data, idGroup) => {
  try {
    const ENDPOINT = `ItemGroup/${idGroup}/conds`;
    // Thêm mới
    const dataNew = data.filter((e) => !e.id);
    if (dataNew.length) {
      await API.add(ENDPOINT, dataNew);
    }
    // Cập nhật
    const dataUpdate = data.filter((e) => e.id && e.statusDB == "U");
    await API.update(ENDPOINT, dataUpdate);
    return true;
  } catch (error) {
    return false;
  }
};

const SaveLineUser = async (data, idGroup) => {
  try {
    const ENDPOINT = `ItemGroup/${idGroup}/customers`;
    if (data.length) {
      await API.add(ENDPOINT, data);
    }
    return true;
  } catch (error) {
    return false;
  }
};

const OpenComfirmDialog = (id) => {
  payLoad.value.id = id;
  comfirmDele.value = true;
};

const acceptDele = async () => {
  try {
    const response = await API.delete(`ItemGroup/${payLoad.value.id}`);
    if (response.data) {
      initData();
      await GetAllCustomer();
      FunctionGlobal.$notify("S", "Đã xóa thành công", toast);
    }
  } catch (er) {
    FunctionGlobal.$notify("E", er, toast);
  }
};

const AddConditon = () => {
  payLoad.value.conditionCusGroups.push({
    id: 0,
    groupId: 0,
    isEqual: true,
    typeCondition: "",
    values: [],
  });
};

const getValueCondition = async () => {
  const endpoints = {
    brand: "Brand/getall",
    industry: "Industry/getall",
    productGroups: "product-groups",
    productApplications: "product-applications",
    itemType: "ItemType/getall",
    packing: "Packing/getall",
    productQualityLevles: "product-quality-levles",
  };

  try {
    const conditionData = {};
    for (const [key, endpoint] of Object.entries(endpoints)) {
      const response = await API.get(endpoint);
      conditionData[key] = response.data?.items || response.data || [];
    }
    return conditionData;
  } catch (error) {
    return Object.keys(endpoints).reduce((acc, key) => {
      acc[key] = [];
      return acc;
    }, {});
  }
};

const getProducts = async () => {
  try {
    const res = await API.get("Item?skip=0&limit=50");
    return res.data.items;
  } catch (error) {
    console.error(error);
  }
};

const OpenDialogGroup = async (data = null) => {
  loading.value = true;
  initData();
  if (data) {
    const res = await API.get("ItemGroup/" + data.id);
    payLoad.value = cloneDeep(res.data);
    payLoad.value.conditionCusGroups.forEach((el) => {
      el.values = el.values.map(Number);
    });
    payLoad.value.listUser = payLoad.value.customers.map((el) => el.id);
    payLoad.value.customers.forEach((el) => (el.statusDB = "U"));
  }
  setIndexCondition();
  visible.value = true;
  if (payLoad.value.isSelected) {
    changUser();
  } else {
    handleCustomerGroup(true);
  }

  loading.value = false;
};

const changeCondition = (e, index, item = null) => {
  const itemSelect = condition.value.find((el) => el.code === e.value);
  if (itemSelect === undefined) return;
  if (item?.id) item.statusDB = "U";
  if (!item?.id) item.statusDB = "A";
  setIndexCondition();
};

const setIndexCondition = () => {
  condition.value.forEach((e) => {
    delete e.index;
  });
  const condi = payLoad.value.conditionCusGroups.filter((e) => e.statusDB !== "D");
  condi.forEach((el, index) => {
    const item = condition.value.find((e) => e.code === el.typeCondition);
    if (item != undefined) item.index = index + 1;
  });
};

const RemoveCondition = (item, index) => {
  setIndexCondition();
  if (item.id) {
    item.statusDB = "D";
  } else {
    payLoad.value.conditionCusGroups.splice(index, 1);
  }
  handleCustomerGroup();
};

const Validate = () => {
  let status = false;
  let { groupName, conditionCusGroups, isSelected, listUser } = payLoad.value;
  conditionCusGroups = conditionCusGroups.filter((e) => e.statusDB != "D");
  if (!groupName) {
    FunctionGlobal.$notify("E", "Vui lòng nhập tên nhóm", toast);
    status = true;
  }
  if (isSelected) {
    if (!listUser.length) {
      FunctionGlobal.$notify("E", "Vui lòng chọn sản phẩm vào nhóm", toast);
      status = true;
    }
  } else {
    conditionCusGroups.forEach((el) => {
      if (!el.typeCondition || !el.values.length) {
        FunctionGlobal.$notify("E", "Vui lòng chọn điều kiện", toast);
        status = true;
      }
    });
  }
  return status;
};

const DeleteCondition = async (data, id) => {
  try {
    const res = API.delete(`ItemGroup/${id}/conds`, data);
    return true;
  } catch (error) {
    return false;
  }
};

const DeleteUser = async (data, id) => {
  try {
    const res = API.delete(`ItemGroup/${id}/customers`, data);
    return true;
  } catch (error) {
    return false;
  }
};

const removeUser = (item) => {
  if (item.id) {
    item.statusDB = "D";
  } else {
    payLoad.value.customers = payLoad.value.customers.filter((el) => el != item);
  }
  payLoad.value.listUser = payLoad.value.customers
    .filter((e) => e.statusDB != "D")
    .map((e) => e.id);
};

const TemporaryCustomerData2 = ref([]);
const changUser = () => {
  const newID = payLoad.value.listUser[payLoad.value.length];
  const { listUser } = payLoad.value;
  if (!listUser.length) {
    payLoad.value.customers = payLoad.value.customers.filter((e) => e.statusDB != "A");
    payLoad.value.customers.forEach((el) => {
      el.statusDB = "D";
    });
    return;
  }
  const setNew = new Set(listUser);
  const setDB = new Set(payLoad.value.customers.map((e) => e.id));
  const uniqueA = [...setNew].filter((item) => !setDB.has(item));
  const uniqueB = [...setDB].filter((item) => !setNew.has(item));
  const unique = [...uniqueA, ...uniqueB];
  if (unique.length) {
    unique.forEach((id) => {
      const user = payLoad.value.customers.find((e) => e.id == id);
      if (user === undefined) {
        const newUser = DataCustomer.value.find((e) => e.id === id);
        newUser.statusDB = "A";
        payLoad.value.customers.push(newUser);
      } else {
        const userUpdate = payLoad.value.customers.find((e) => e.id == newID);
        if (userUpdate != undefined) {
          userUpdate.statusDB = "U";
        }
        if (user.statusDB == "A") {
          payLoad.value.customers = payLoad.value.customers.filter((e) => e.id != id);
        } else {
          user.statusDB = "D";
        }
      }
    });
  }
  TemporaryCustomerData2.value = payLoad.value.customers;
};
const TemporaryCustomerData = ref([]);
const handleCustomerGroup = async (isLoading = false) => {
  const formatData = {
    ...payLoad.value,
    conditionCusGroups: payLoad.value.conditionCusGroups
      .filter((e) => e.values.length > 0 && e.statusDB !== "D")
      .map((item) => ({
        ...item,
        values: item.values.map(String), // Chuyển các giá trị number thành string
      })),
  };

  const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

  try {
    if (!isLoading) await delay(1000);

    const res = await API.add("ItemGroup/GetCusInConds", formatData);
    if (res.data) {
      TemporaryCustomerData.value = res.data;
      payLoad.value.customers = res.data;
    }
  } catch (error) {
    FunctionGlobal.$notify("E", error, toast);
  }
};
watch(
  () => payLoad.value.isSelected,
  (newValue) => {
    if (!newValue) {
      payLoad.value.customers = TemporaryCustomerData.value;
    } else {
      payLoad.value.customers = TemporaryCustomerData2.value;
    }
  }
);

const updateSelectedProduct = (e) => {

};
</script>
<template>
  <div class="flex justify-content-between">
    <h4 class="font-bold m-0">Nhóm sản phẩm</h4>
    <Button
      icon="pi pi-plus"
      label="Thêm mới"
      class="mr-3"
      @click="OpenDialogGroup()"
    />
  </div>
  <div class="card grid mt-3 p-2">
    <div class="col-12">
      <DataTable
        :value="DataGroup"
        :paginator="true"
        :rows="10"
        :rowsPerPageOptions="[10, 20, 30]"
        showGridlines
      >
        <template #empty>
          <div class="my-5 py-5 text-center">Không có dữ liệu phù hợp</div>
        </template>
        <template #header>
          <div class="flex justify-content-between m-0">
            <IconField iconPosition="left">
              <InputText
                @input="debounceF(keySearchCusGrp)"
                v-model="keySearchCusGrp"
                placeholder="Nhập từ khóa tìm kiếm"
              />
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
            </IconField>
            <Button
              type="button"
              icon="pi pi-filter-slash"
              label="Xóa bộ lọc"
              outlined
              @click="clearFilter()"
            />
          </div>
        </template>
        <Column field="groupName" header="Tên Nhóm sản phẩm" class="w-4"></Column>
        <Column field="description" header="Mô tả" class="w-4"></Column>
        <Column field="isActive" header="Trạng thái" class="w-1">
          <template #body="{ data }">
            <Tag v-if="data.isActive" value="Hoạt động"></Tag>
            <Tag v-else value="Không hoạt động" severity="secondary"></Tag>
          </template>
        </Column>
        <Column class="w-1" header="Hành động">
          <template #body="{ data }">
            <Button icon="pi pi-pencil" @click="OpenDialogGroup(data)" text/>
            <Button
              icon="pi pi-trash"
              text
              severity="danger"
              @click="OpenComfirmDialog(data.id)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
  <Dialog
    v-model:visible="visible"
    :header="payLoad.id ? 'Cập nhật nhóm sản phẩm' : 'Thêm mới nhóm sản phẩm'"
    :style="{ width: '85rem' }"
    position="top"
    :modal="true"
    :draggable="false"
  >
    {{ payLoad }}
    <div class="flex flex-column field">
      <label for="GroupName" class="font-semibold"
        >Tên nhóm<sup class="text-red-500">*</sup></label
      >
      <InputText
        id="GroupName"
        v-model="payLoad.groupName"
        class="w-full"
        placeholder="Nhập Tên nhóm"
        :invalid="submited && !payLoad.groupName"
      />
    </div>
    <div class="flex flex-column gap-2 mb-3 mt-4">
      <span class="font-semibold">Thêm sản phẩm vào nhóm</span>
      <small>
        Bạn có thể chọn một trong hai cách bên dưới để thêm sản phẩm vào nhóm
      </small>
      <Divider></Divider>
      <div class="flex flex-column gap-4">
        <div class="flex align-items-center">
          <RadioButton inputId="isSelect" v-model="payLoad.isSelected" :value="true" />
          <label for="isSelect" class="ml-2">Tự chọn sản phẩm</label>
        </div>
        <div class="flex align-items-center">
          <RadioButton
            inputId="isCondititon"
            v-model="payLoad.isSelected"
            :value="false"
          />
          <label for="isCondititon" class="ml-2"
            >Sản phẩm tự động chọn dựa trên những điều kiện</label
          >
        </div>
        <!-- Điều kiện -->
        <div v-if="!payLoad.isSelected" class="flex flex-column gap-3">
          <div class="flex gap-3">
            <span>Các sản phẩm phải phù hợp</span>
            <div class="flex flex-column gap-3">
              <div class="flex align-items-center">
                <RadioButton
                  inputId="all"
                  :value="false"
                  v-model="payLoad.isOneOfThem"
                  @change="handleCustomerGroup"
                />
                <label for="all" class="ml-2">Tất cả các điều kiện</label>
              </div>
              <div class="flex align-items-center">
                <RadioButton
                  inputId="or"
                  :value="true"
                  v-model="payLoad.isOneOfThem"
                  @change="handleCustomerGroup"
                />
                <label for="or" class="ml-2">Bất kỳ điều kiện nào</label>
              </div>
            </div>
          </div>
          <div
            class="flex justify-content-between gap-2"
            v-for="(item, index) in payLoad.conditionCusGroups.filter(
              (e) => e.statusDB != 'D'
            )"
            :key="item"
          >
            <Dropdown
              class="w-4"
              placeholder="Giá trị"
              optionLabel="name"
              optionValue="code"
              :options="condition.filter((el) => !el.index || el.index == index + 1)"
              v-model="item.typeCondition"
              @change="changeCondition($event, index, item)"
              :invalid="submited && !item.typeCondition"
            ></Dropdown>
            <Dropdown
              class="w-3"
              :placeholder="t('client.condition')"
              :options="[
                { name: t('client.equals'), code: true },
                { name: t('client.not_equals'), code: false },
              ]"
              v-model="item.isEqual"
              optionLabel="name"
              optionValue="code"
            ></Dropdown>

            <MultiSelect
              filter
              class="w-4"
              :placeholder="t('client.value')"
              :options="condition.find((el) => el.code == item.typeCondition)?.data || []"
              v-model="item.values"
              optionLabel="name"
              :maxSelectedLabels="3"
              selectedItemsLabel="Đã chọn {0} điều kiện"
              optionValue="id"
              @change="
                () => {
                  (item.statusDB = item.id ? 'U' : 'A'), handleCustomerGroup();
                }
              "
              :invalid="submited && !item.values.length"
            ></MultiSelect>
            <Button
              class="w-1"
              icon="pi pi-trash"
              text
              severity="danger"
              @click="RemoveCondition(item, index)"
            />
          </div>
          <div>
            <Button
              icon="pi pi-plus-circle"
              :label="t('Custom.add_condition')"
              size="small"
              outlined
              :disabled="
                payLoad.conditionCusGroups.filter((e) => e.statusDB != 'D').length > 4
              "
              @click="AddConditon()"
            />
          </div>
        </div>
        <div v-if="payLoad.isSelected">
          <MultiSelect
            class="w-full"
            placeholder="Chọn sản phẩm"
            :emptyMessage="t('body.PurchaseRequestList.no_matching_product_message')"
            filter
            optionLabel="itemName"
            optionValue="id"
            :options="DataProduct"
            :maxSelectedLabels="3"
            selectedItemsLabel="Đã chọn {0} sản phẩm"
            v-model="payLoad.listUser"
            @change="changUser($event)"
          >
          </MultiSelect>
          <ProductSelector
            @confirm="updateSelectedProduct($event)"
            icon="pi pi-plus"
            label="Thêm sản phẩm"
            outlined
          />
        </div>
      </div>
    </div>

    <div class="mt-3 mb-3">
      <DataTable
        :value="payLoad.customers?.filter((e) => e.statusDB != 'D')"
        tableStyle="min-width: 50rem"
        scrollable
        scrollHeight="400px"
        stripedRows
      >
        <Column header="#">
          <template #body="{ index }">{{ index + 1 }}</template>
        </Column>
        <Column field="phone" header="Ảnh sản phẩm"></Column>
        <Column field="cardCode" header="Mã sản phẩm"></Column>
        <Column field="cardName" header="Tên sản phẩm"></Column>
        <Column header="Hành động" v-if="payLoad.isSelected">
          <template #body="{ data }">
            <Button
              icon="pi pi-trash"
              text
              severity="danger"
              @click="removeUser(data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
    <div class="flex flex-column field">
      <label for="Descriptions" class="font-semibold">Mô tả</label>
      <Textarea
        id="Descriptions"
        v-model="payLoad.description"
        rows="5"
        cols="30"
        class="w-full"
      />
    </div>
    <div class="flex align-items-center gap-3 field">
      <label for="Status" class="font-semibold">Trạng thái </label>
      <InputSwitch id="Status" v-model="payLoad.isActive" />
    </div>
    <template #footer>
      <Button
        type="button"
        label="Hủy"
        severity="secondary"
        @click="visible = false"
      />
      <Button type="button" label="Lưu" @click="SaveGroup()"/>
    </template>
  </Dialog>
  <Dialog v-model:visible="comfirmDele" :style="{ width: '20rem' }" header="Xóa">
    <div class="">
      <div class="flex justify-content-center items-center w-full h-full">
        <span>Bạn có muốn chắc chắn xóa không</span>
      </div>
      <div class="flex gap-3 mt-4">
        <Button
          class="w-6"
          label="Hủy"
          @click="comfirmDele = false"
          severity="secondary"
        />
        <Button class="w-6" label="Xóa" @click="acceptDele()" severity="danger" />
      </div>
    </div>
  </Dialog>
  <Loading v-if="loading"></Loading>
</template>
<style></style>
