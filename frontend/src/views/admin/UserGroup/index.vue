<script setup>
import { ref, reactive, onMounted, watch, inject } from "vue";
import API from "@/api/api-main";
import { cloneDeep, debounce } from "lodash";
import { useGlobal } from "@/services/useGlobal";
import Dropdown from "primevue/dropdown";
import { FilterStore } from "@/Pinia/Filter/FilterStoreUsergroup";
import { useI18n } from "vue-i18n";
const conditionHandler = inject("conditionHandler");

const { toast, FunctionGlobal } = useGlobal();
const filterStore = FilterStore();
const { t } = useI18n();

const paginator = ref({
    page: 0,
    rows: 10,
    total: 0,
});
const visible = ref();
const loading = ref(false);
const DataCustomer = ref();
const comfirmDele = ref();
const DataGroup = ref([]);
const payLoad = ref();
const submited = ref();
const keySearchCusGrp = ref("");
const condition = ref([
    { code: "region", name: t('client.area'), data: [] },
    { code: "area", name: t('client.province_city'), data: [] },
    { code: "brand", name: t('client.brand'), data: [] },
    { code: "industry", name: t('body.report.table_header_category_4'), data: [] },
    { code: "size", name: t('body.sampleRequest.customer.scale_column'), data: [] },
]);

onMounted(async () => {
    await GetAllCustomer();
    const dataConditon = await getValueCondition();
    condition.value.find((el) => el.code == "region").data = dataConditon.region;
    condition.value.find((el) => el.code == "area").data = dataConditon.area;
    condition.value.find((el) => el.code == "brand").data = dataConditon.brand;
    condition.value.find((el) => el.code == "industry").data = dataConditon.industry;
    condition.value.find((el) => el.code == "size").data = dataConditon.size;
});

const closeDialog = () => {
    visible.value = false;
    initData();
};

const initData = () => {
    submited.value = false;
    visible.value = false;
    comfirmDele.value = false;
    payLoad.value = {
        id: 0,
        groupName: "",
        groupType: "",
        isSelected: true,
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
    const filters = conditionHandler.getQuery(filterStore.filters);
    let url = `CustomerGroup?skip=${paginator.value.page}&limit=${paginator.value.rows}`;
    if (filters) {
        url += `${filters}`;
    }
    loading.value = true;
    try {
        const res = await API.get(url);
        DataGroup.value = res.data.bpGroup;
        paginator.value.total = res.data.total;
    } catch (er) {
        FunctionGlobal.$notify("E", er, toast);
    } finally {
        loading.value = false;
    }
};
const onPageChange = (e) => {
    paginator.value.page = e.page;
    paginator.value.rows = e.rows;
    GetAllCustomer();
};
const clearFilter = () => {
    filterStore.resetFilters();
    GetAllCustomer();
};
const onFilter = async (keySearch) => {
    let uri = "";
    let res = [];
    try {
        keySearch.trim() != ""
            ? (uri = `CustomerGroup/search/${keySearch.trim()}`)
            : (uri = "");
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

// watch(
//   value1,
//   debounce((newValue) => {
//     fetchCustomerGroups(newValue);
//   }, 1000)
// );

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
            ? await API.update(`CustomerGroup/${data.id}`, dataHeader)
            : await API.add("CustomerGroup/add", dataHeader);
        const response = FUNAPI;

        if (response?.data) {
            if (!data.isSelected) {
                const Line = await SaveLineCondition(
                    data.conditionCusGroups,
                    response.data.id
                );
                if (!Line) {
                    FunctionGlobal.$notify("E", "Lỗi không lưu được điều kiện", toast);
                    return;
                }
                if (data.conditionCusGroups.filter((e) => e.statusDB == "D").length) {
                    const DataDelete = data.conditionCusGroups.filter(
                        (e) => e.statusDB == "D"
                    );
                    const Delete = await DeleteCondition(
                        DataDelete.map((e) => e.id),
                        response.data.id
                    );
                    if (!Delete) {
                        FunctionGlobal.$notify(
                            "E",
                            "Lỗi không xóa được điều kiện",
                            toast
                        );
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
                            FunctionGlobal.$notify(
                                "E",
                                "Lỗi không lưu được khách hàng",
                                toast
                            );
                            return;
                        }
                    }
                    const DeleteCustomer = data.customers
                        .filter((e) => e.statusDB == "D")
                        .map((e) => e.id);

                    if (DeleteCustomer.length) {
                        const User = await DeleteUser(DeleteCustomer, response.data.id);
                        if (!User) {
                            FunctionGlobal.$notify(
                                "E",
                                "Lỗi không lưu được khách hàng",
                                toast
                            );
                            return;
                        }
                    }
                } else {
                    const User = await SaveLineUser(data.listUser, response.data.id);
                    if (!User) {
                        FunctionGlobal.$notify(
                            "E",
                            "Lỗi không lưu được khách hàng",
                            toast
                        );
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
        const ENDPOINT = `CustomerGroup/${idGroup}/conds`;
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
        const ENDPOINT = `CustomerGroup/${idGroup}/customers`;
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
        const response = await API.delete(`CustomerGroup/${payLoad.value.id}`);
        if (response.data) {
            initData();
            await GetAllCustomer();
            FunctionGlobal.$notify("S", "Đã xóa thành công", toast);
        }
    } catch (er) {
        FunctionGlobal.$notify("E", er, toast);
    }
};

const dataLoadCustomers = ref({
    skip: 0,
    limit: 10000,
    total: 0,
    loading: false,
});

const getCustomer = async () => {
    try {
        const res = await API.get(`Customer/customegroup?skip=${dataLoadCustomers.value.skip}&limit=${dataLoadCustomers.value.limit}`);
        return res.data.items;
    } catch (error) {
        return [];
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
        region: "regions?skip=0&limit=10000",
        area: "areas?skip=0&limit=10000",
        brand: "Brand/getall",
        industry: "Industry/getall",
        size: "BPSize/getall",
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

const OpenDialogGroup = async (data = null) => { 
    loading.value = true;
    initData();
    if (data) {
        const res = await API.get("CustomerGroup/" + data.id);
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
    DataCustomer.value = await getCustomer();
    // Đưa các khách hàng đã chọn lên đầu danh sách
    if (payLoad.value.listUser && DataCustomer.value && DataCustomer.value.length) {
        const selectedIds = new Set(payLoad.value.listUser);
        const selected = DataCustomer.value.filter(c => selectedIds.has(c.id));
        const unselected = DataCustomer.value.filter(c => !selectedIds.has(c.id));
        DataCustomer.value = [...selected, ...unselected];
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
            FunctionGlobal.$notify("E", "Vui lòng chọn khách hàng vào nhóm", toast);
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
        const res = API.delete(`CustomerGroup/${id}/conds`, data);
        return true;
    } catch (error) {
        return false;
    }
};

const DeleteUser = async (data, id) => {
    try {
        const res = API.delete(`CustomerGroup/${id}/customers`, data);
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
        payLoad.value.customers = payLoad.value.customers.filter(
            (e) => e.statusDB != "A"
        );
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
                    payLoad.value.customers = payLoad.value.customers.filter(
                        (e) => e.id != id
                    );
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


        const res = await API.add("CustomerGroup/GetCusInConds", formatData);
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
const onFilterDB = () => {
    GetAllCustomer();
};
</script>

<template>
    <div class="flex justify-content-between">
        <h4 class="font-bold m-0">{{ t('body.sampleRequest.customerGroup.title') }}</h4>
        <Button
            icon="pi pi-plus"
            :label="t('body.sampleRequest.customerGroup.addNew')"
            class="mr-3"
            @click="OpenDialogGroup()"
        />
    </div>
    <div class="card grid mt-3 p-2">
        <div class="col-12">
            <DataTable :value="DataGroup" :paginator="true" :rows="paginator.rows" :page="paginator.page"
                :totalRecords="paginator.total" :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
                dataKey="id" lazy showGridlines filterDisplay="menu" :filterLocale="'vi'" @filter="onFilterDB()"
                @page="onPageChange" v-model:filters="filterStore.filters">
                <template #empty>
                    <div class="my-5 py-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
                </template>
                <template #header>
                    <div class="flex justify-content-between m-0">
                        <IconField iconPosition="left">
                            <InputText
                                @input="debounceF(keySearchCusGrp)"
                                v-model="keySearchCusGrp"
                                :placeholder="t('body.sampleRequest.customerGroup.enter_keyword_placeholder')"
                            />
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                        </IconField>
                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            v-tooltip.bottom="t('body.OrderApproval.clear')"
                            severity="danger"
                            outlined
                            @click="clearFilter()"
                        />
                    </div>
                </template>
                <Column field="groupName" :header="t('body.sampleRequest.customerGroup.name')"></Column>
                <Column field="description" :header="t('body.sampleRequest.customerGroup.description')" class="w-4"></Column>
                <Column
                    field="isActive"
                    :header="t('body.sampleRequest.customerGroup.status')"
                    class="w-10rem"
                    :showFilterMatchModes="false"
                >
                    <template #body="{ data }">
                        <Tag v-if="data.isActive" :value="t('body.sampleRequest.customer.active_status')"></Tag>
                        <Tag v-else :value="t('body.sampleRequest.customer.inactive_status')" severity="secondary"></Tag>
                    </template>
                    <template #filter="{ filterModel }">
                        <MultiSelect
                            v-model="filterModel.value"
                            :options="[
                                { code: false, name: t('body.sampleRequest.customer.inactive_status') },
                                { code: true, name: t('body.sampleRequest.customer.active_status') },
                            ]"
                            optionLabel="name"
                            optionValue="code"
                            :placeholder="t('body.OrderApproval.clear')"
                            class="p-column-filter"
                            showClear
                        >
                            <template #option="slotProps">
                                <Tag
                                    v-if="slotProps.option.isActive"
                                    :value="
                                        slotProps.option.isActive
                                            ? t('body.sampleRequest.customer.active_status')
                                            : t('body.sampleRequest.customer.inactive_status')
                                    "
                                ></Tag>
                            </template>
                        </MultiSelect>
                    </template>
                </Column>
                <Column class="w-1rem">
                    <template #body="{ data }">
                        <Button icon="pi pi-pencil" @click="OpenDialogGroup(data)" text/>
                        <Button v-if="0" icon="pi pi-trash" text severity="danger"
                            @click="OpenComfirmDialog(data.id)" />
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <Dialog
        v-model:visible="visible"
        :header="payLoad.id ? payLoad.groupName : t('body.sampleRequest.customerGroup.add_new_customer_group')"
        :style="{ width: '85rem' }"
        position="top"
        :modal="true"
        :draggable="false"
    >
        <div class="flex flex-column field">
            <label for="GroupName" class="font-semibold"
                >{{ t('body.sampleRequest.customerGroup.group_name_label') }}<sup class="text-red-500">*</sup></label
            >
            <InputText
                id="GroupName"
                v-model="payLoad.groupName"
                class="w-full"
                :placeholder="t('body.sampleRequest.customerGroup.enter_group_name_placeholder')"
                :invalid="submited && !payLoad.groupName"
            />
        </div>
        <div class="flex flex-column gap-2 mb-3 mt-4">
            <span class="font-semibold">{{ t('body.sampleRequest.customerGroup.add_customers_to_group') }}</span>
            <small>
                {{ t('body.sampleRequest.customerGroup.add_customers_instruction') }}
            </small>
            <Divider></Divider>
            <div class="flex flex-column gap-4">
                <div class="flex align-items-center">
                    <RadioButton
                        inputId="isSelect"
                        v-model="payLoad.isSelected"
                        :value="true"
                    />
                    <label for="isSelect" class="ml-2">{{ t('body.sampleRequest.customerGroup.manual_selection_radio') }}</label>
                </div>
                <div class="flex align-items-center">
                    <RadioButton
                        inputId="isCondititon"
                        v-model="payLoad.isSelected"
                        :value="false"
                    />
                    <label for="isCondititon" class="ml-2"
                        >{{ t('body.sampleRequest.customerGroup.auto_selection_radio') }}</label
                    >
                </div>
                <!-- Điều kiện -->
                <div v-if="!payLoad.isSelected" class="flex flex-column gap-3">
                    <div class="flex gap-3">
                        <span>{{t('client.customers_must_match')}}</span>
                        <div class="flex flex-column gap-3">
                            <div class="flex align-items-center">
                                <RadioButton
                                    inputId="all"
                                    :value="false"
                                    v-model="payLoad.isOneOfThem"
                                    @change="handleCustomerGroup"
                                />
                                <label for="all" class="ml-2">{{t('client.all_conditions')}}</label>
                            </div>
                            <div class="flex align-items-center">
                                <RadioButton
                                    inputId="or"
                                    :value="true"
                                    v-model="payLoad.isOneOfThem"
                                    @change="handleCustomerGroup"
                                />
                                <label for="or" class="ml-2">{{t('client.any_condition')}}</label>
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
                            :placeholder="t('client.value')"
                            optionLabel="name"
                            optionValue="code"
                            :options="
                                condition.filter(
                                    (el) => !el.index || el.index == index + 1
                                )
                            "
                            v-model="item.typeCondition"
                            @change="changeCondition($event, index, item)"
                            :invalid="submited && !item.typeCondition"
                        ></Dropdown>
                        <Dropdown
                            class="w-3"
                            placeholder="Điều kiện"
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
                            :options="
                                condition.find((el) => el.code == item.typeCondition)
                                    ?.data || []
                            "
                            v-model="item.values"
                            optionLabel="name"
                            :maxSelectedLabels="3"
                            selectedItemsLabel="Đã chọn {0} điều kiện"
                            optionValue="id"
                            @change="
                                () => {
                                    (item.statusDB = item.id ? 'U' : 'A'),
                                        handleCustomerGroup();
                                }
                            " :invalid="submited && !item.values.length"></MultiSelect>
                        <Button class="w-1" icon="pi pi-trash" text severity="danger"
                            @click="RemoveCondition(item, index)"/>
                    </div>
                    <div>
                        <Button
                            icon="pi pi-plus-circle"
                            :label="t('body.promotion.addPromotionCondition')"
                            size="small"
                            outlined
                            :disabled="
                                payLoad.conditionCusGroups.filter(
                                    (e) => e.statusDB != 'D'
                                ).length > 4
                            "
                            @click="AddConditon()"
                        />
                    </div>
                </div>
                <div v-if="payLoad.isSelected">
                    <MultiSelect
                        class="w-full"
                        :placeholder="t('body.sampleRequest.customerGroup.select_customer_placeholder')"
                        emptyMessage="Không tìm thấy khách hàng nào"
                        filter
                        optionLabel="cardName"
                        optionValue="id"
                        :options="DataCustomer"
                        :maxSelectedLabels="3"
                        selectedItemsLabel="Đã chọn {0} khách hàng"
                        v-model="payLoad.listUser"
                        @change="changUser($event)"
                    >
                    </MultiSelect>
                </div>
            </div>
        </div>

        <div class="mt-3 mb-3 card p-0 overflow-hidden border-bottom-none border-noround">
            <DataTable :value="payLoad.customers?.filter((e) => e.statusDB != 'D')" tableStyle="min-width: 50rem"
                scrollable scrollHeight="400px" stripedRows>
                <Column header="#">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column field="cardCode" :header="t('body.sampleRequest.customerGroup.customer_code_column')"></Column>
                <Column field="cardName" :header="t('body.sampleRequest.customerGroup.customer_name_column')"></Column>
                <Column field="phone" :header="t('body.sampleRequest.customerGroup.phone_column')"></Column>
                <Column field="email" :header="t('body.sampleRequest.customerGroup.email_column')"></Column>
                <Column class="w-1rem" v-if="payLoad.isSelected">
                    <template #body="{ data }">
                        <Button icon="pi pi-trash" text severity="danger" @click="removeUser(data)"/>
                    </template>
                </Column>
                <template #empty>
                    <div class="py-5 my-5 text-center text-500 font-italic">
                        {{ t('body.sampleRequest.customerGroup.customer_list_title') }}
                    </div>
                </template>
            </DataTable>
        </div>
        <div class="flex flex-column field">
            <label for="Descriptions" class="font-semibold">{{ t('body.sampleRequest.customerGroup.description') }}</label>
            <Textarea
                id="Descriptions"
                v-model="payLoad.description"
                rows="5"
                cols="30"
                class="w-full"
            />
        </div>
        <div class="flex align-items-center gap-3 field">
            <label for="Status" class="font-semibold">{{ t('body.sampleRequest.customerGroup.status') }} </label>
            <InputSwitch id="Status" v-model="payLoad.isActive" />
        </div>
        <template #footer>
            <Button
                type="button"
                :label="t('body.PurchaseRequestList.cancel_button')"
                severity="secondary"
                @click="closeDialog()"
            />
            <Button type="button" :label="t('body.systemSetting.save_button')" @click="SaveGroup()"/>
        </template>
    </Dialog>

    <Dialog v-model:visible="comfirmDele" :style="{ width: '20rem' }" :header="t('body.SaleSchannel.delete')">
        <div class="">
            <div class="flex justify-content-center items-center w-full h-full">
                <span>Bạn có muốn chắc chắn xóa không</span>
            </div>
            <div class="flex gap-3 mt-4">
                <Button
                    class="w-6"
                    :label="t('body.PurchaseRequestList.cancel_button')"
                    @click="comfirmDele = false"
                    severity="secondary"
                />
                <Button class="w-6" :label="t('body.SaleSchannel.delete')" @click="acceptDele()" severity="danger" />
            </div>
        </div>
    </Dialog>
    <Loading v-if="loading"></Loading>
</template>
<style></style>
