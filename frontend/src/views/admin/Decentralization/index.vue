<script setup>
import { ref, onBeforeMount, reactive } from "vue";
import API from "@/api/api-main";
import UserDetail from "./components/UserDetail.vue";
import Permission from "./components/Permission.vue";
import CustomerInCharge from "./components/CustomerInCharge.vue";
import NPP from "./components/NPP.vue";
import CustomerData from "./components/CustomerData.vue";
import { useToast } from "primevue/usetoast";
import { getOrgStruct } from "../OrganizationalStructure/script";
import { useConfirm } from "primevue/useconfirm";
import { useFilterSystemUserStore } from "../../../Pinia/Filter/FilterSystemUser";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const toast = useToast();
const confirm = useConfirm();

const { filters, getQuery } = useFilterSystemUserStore();

const filterOptions = reactive({
    role: [],
    status: [
        { label: t('body.sampleRequest.customer.active_status'), value: "A" },
        { label: t('body.sampleRequest.customer.inactive_status'), value: "I" },
    ],
});

import { GridifyQueryBuilder, ConditionalOperator as op } from "gridify-client";
var queryFilter = "";
import { toQueryString } from "../../../helpers/gridify-client";
const onFilter = (e) => {
    const gqb = new GridifyQueryBuilder();
    e.filters["role.name"].value?.forEach((item, idx) => {
        if (idx == 0) {
            gqb.startGroup();
        }
        gqb.addCondition("role.id", op.Equal, item);
        if (idx < e.filters["role.name"].value.length - 1) {
            gqb.or();
        }
        if (idx == e.filters["role.name"].value.length - 1) {
            gqb.endGroup();
        }
    });
    if (e.filters["status"].value) {
        if (e.filters["role.name"].value?.length > 0) {
            gqb.and();
        }
        gqb.addCondition("status", op.Equal, e.filters["status"].value);
    }
    const result = gqb.build();
    queryFilter = result.filter;
    getAllUser(keyword.value, queryFilter);
};

const User = ref({
    skip: 0,
    limit: 10,
    item: [],
});
const loading = ref(false);

onBeforeMount(async () => {
    getAllUser();
    fetchRoles();
    OrgStructData.value = await getOrgStruct();
});

const getAllUser = async (text = "", filters = "") => {
    try {
        loading.value = true;
        const { skip, limit } = User.value;
        let uri = `Account/getall?userType=APSP&skip=${skip || 0}&limit=${
            limit || 30
        }&OrderBy=id desc`;
        if (text) {
            uri += `&search=${text}`;
        }
        if (filters) {
            uri += `&filter=${filters}`;
        }
        const { data } = await API.get(uri);
        User.value = data;
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const roles = ref([]);
const fetchRoles = () => {
    API.get("role/getrole").then((res) => {
        roles.value = res.data;
        filterOptions.role = res.data.map((item) => {
            return {
                label: item.name,
                value: item.id,
            };
        });
    });
};

const onPage = async (event) => {
    User.value.skip = event.page;
    User.value.limit = event.rows;
    await getAllUser(keyword.value, queryFilter);
};

const expandedRows = ref({});
const selectedRow = ref({});
const onRowClicked = (event) => {
    if (expandedRows.value[`${event.data.id}`]) {
        expandedRows.value = {};
        return;
    }
    userResponse.value = null;
    const row = {};
    row[`${event.data.id}`] = true;
    expandedRows.value = row;
    fetchUser(event.data.id, async (user) => {
        // row[`${event.data.id}`] = true;
        const orgStrctId = user.organizationId;
        if (orgStrctId) {
            const orgStrct = await getOrgStructById(orgStrctId);
            userResponse.value = user;
            userResponse.value.orgStrct = orgStrct;
        } else {
            userResponse.value = user;
        }
        // if (user.organizationId) {
        //     userResponse.value.selectionOrgStrct = {
        //         [`${user.organizationId}`]: true,
        //     };
        // }
    });
};

const getOrgStructById = async (id) => {
    let result = {};
    try {
        const { data } = await API.get(`OrganizationUnit/${id}`);
        result = data;
    } catch (err) {
        console.error(error);
    }
    return result;
};

const userResponse = ref(null);

const fetchUser = (uid, callback) => {
    if (uid) {
        API.get(`account/${uid}`)
            .then((res) => {
                callback(res.data.item);
            })
            .catch((error) => {});
    }
};

const onSaveInfoUser = (user) => {
    userResponse.value = null;
    fetchUser(user.id, (resuser) => {
        userResponse.value = resuser;
    });
    getAllUser(keyword.value, queryFilter);
};

const onChangeTab = ({ index }) => {};

const visible = ref(false);
const onClickOpenDlg = (event) => {
    createUser.value = new CreateUser();
    errorMsg.value = {};
    visible.value = true;
};

import { Validator, PATTERN } from "../../../helpers/validate";
class CreateUser {
    email = "";
    userName = "";
    fullName = "";
    // address = "";
    // locationId = 0;
    // locationName = "";
    // areaId = 0;
    // areaName = "";
    phone = "";
    // note = "";
    // dob = "";
    // isAllBranch = "";
    // isInforOther = "";
    // isAllGeneral = "";
    organizationId = null;
    selectionOrgStrct = null;
    isManager = false;
    password = "";
    confirmPassword = "";
    roleId = 0;
    status = "A";

    toJSON() {
        const json = {};
        Object.keys(this).forEach((key) => {
            if (typeof this[key] != "function") {
                if (typeof this[key] == "string") {
                    json[key] = this[key].trim();
                } else {
                    json[key] = this[key];
                }
            }
        });
        json.confirmPassword = json.password;
        return json;
    }

    validate(opts) {
        const obj = this.toJSON();
        return Validator(obj, opts);
    }
}

const createUser = ref(new CreateUser());
const fieldOptions = {
    email: {
        validators: {
            required: true,
            name: "email",
            type: String,
            pattern: PATTERN.email,
            patternMessage: "Email không hợp lệ",
        },
    },
    fullName: {
        validators: {
            required: true,
            name: "họ và tên",
        },
    },
    phone: {
        validators: {
            required: true,
            name: "số điện thoại",
            pattern: PATTERN.phone,
            patternMessage: "Số điện thoại không hợp lệ",
        },
    },
    roleId: {
        validators: {
            required: true,
            name: "vai trò",
            nullMessage: "Chọn vai trò",
        },
    },
    organizationId: {
        validators: {
            required: true,
            name: "phòng ban",
            nullMessage: "Chọn phòng ban",
        },
    },
    password: {
        validators: {
            required: true,
            name: "mật khẩu",
            pattern: PATTERN.password,
            patternMessage: "Mật không không đúng định dạng",
        },
    },
    userName: {
        validators: {
            required: true,
            name: "tên đăng nhập",
        },
    },
};

const loadingBtn = ref(false);
const errorMsg = ref({});
const onClickCreateUser = () => {
    const validate = createUser.value.validate(fieldOptions);
    errorMsg.value = validate.errors;
    if (validate.result) {
        const payload = createUser.value.toJSON();
        delete payload.selectionOrgStrct;
        // delete payload.isManager;

        loadingBtn.value = true;
        API.add("account/register", payload)
            .then((res) => {
                getAllUser();
                toast.add({
                    severity: "success",
                    summary: t('body.systemSetting.save_button') || "Thành công",
                    detail: t('body.systemSetting.create_account_button') + " " + t('body.OrderList.orderList') || "Thêm mới người dùng thành công",
                    life: 3000,
                });
                if (payload.isManager) {
                    if (res.data.id)
                        API.add(`OrganizationUnit/${payload.organizationId}/employees`, [
                            res.data.id,
                        ])
                            .then((subRes) => {
                                API.update(
                                    `OrganizationUnit/${payload.organizationId}/set-manager/${res.data.id}`
                                )
                                    .then((subRes2) => {})
                                    .catch((error) => {
                                        toast.add({
                                            severity: "error",
                                            summary: t('body.OrderApproval.clear') || "Lỗi",
                                            detail: t('body.report.error_occurred_message') || "Đã có lỗi trong khi thay đổi quản lý",
                                            life: 3000,
                                        });
                                    });
                            })
                            .catch((error) => {
                                toast.add({
                                    severity: "error",
                                    summary: t('body.OrderApproval.clear') || "Lỗi",
                                    detail: t('body.report.error_occurred_message') || "Đã có lỗi trong khi thay đổi quản lý",
                                    life: 3000,
                                });
                            })
                            .catch((error) => {
                                toast.add({
                                    severity: "error",
                                    summary: t('body.OrderApproval.clear') || "Lỗi",
                                    detail: t('body.report.error_occurred_message') || "Đã có lỗi trong khi thay đổi quản lý",
                                    life: 3000,
                                });
                            });
                }
                visible.value = false;
            })
            .catch((error) => {
                if (err.status == 400) {
                    if (err.response.data.errors.includes("Username")) {
                        errorMsg.value.userName = t('body.sampleRequest.customerGroup.account_column') + ` '${createUser.value.userName}' ` + t('body.systemSetting.already_exists_label');
                    } else if (err.response.data.errors.includes("Passwords")) {
                        errorMsg.value.password = t('body.systemSetting.invalid_password_message');
                    } else if (err.response.data.errors.includes("Email")) {
                        errorMsg.value.email = t('body.systemSetting.email_already_exists_message');
                    } else if (err.response.data.errors.includes("Số điện thoại")) {
                        errorMsg.value.phone = t('body.systemSetting.phone_already_exists_message');
                    } else {
                        toast.add({
                            severity: "error",
                            summary: t('body.OrderApproval.clear') || "Lỗi",
                            detail: t('body.report.error_occurred_message') || "Đã có lỗi xảy ra",
                            life: 3000,
                        });
                    }
                } else {
                    toast.add({
                        severity: "error",
                        summary: t('body.OrderApproval.clear') || "Lỗi",
                        detail: t('body.report.error_occurred_message') || "Đã có lỗi xảy ra",
                        life: 3000,
                    });
                }
            })
            .finally(() => {
                loadingBtn.value = false;
            });
    }
};

const isInvld = (errorMassage) => {
    return errorMassage ? true : false;
};
const OrgStructData = ref([]);

var isExistManager = false;
const onSelectOrgStruct = (e) => {
    isExistManager = false;
    createUser.value.isManager = false;
    createUser.value.organizationId = e.id;
    if (e.managerUserId) {
        isExistManager = true;
    }
};

const onChangeMngCheckBox = () => {
    if (isExistManager && createUser.value.isManager) {
        confirm.require({
            message:
                "Phòng ban này đã có quản lý, bạn có muốn thay thế quản lý mới không?",
            header: "Xác nhận",
            rejectClass: "p-button-secondary",
            rejectLabel: "Hủy",
            acceptLabel: "Xác nhận",
            accept: () => {},
            reject: () => {
                createUser.value.isManager = false;
            },
        });
    }
};
import { debounce } from "lodash";
const keyword = ref("");
const dbSearch = debounce(() => {
    User.value.skip = 0;
    getAllUser(keyword.value);
}, 500);

const clearFilter = () => {
    keyword.value = "";
    User.value.skip = 0;
    getAllUser();
};
</script>

<template>
    <div>
        <div class="mb-4">
            <h3 class="font-bold m-0" style="line-height: 33px">{{ t('body.systemSetting.user_management_title') }}</h3>
        </div>
        <!-- scrollable
            scrollHeight="70vh"
            v-model:filters="filters"
            :globalFilterFields="['userName', 'fullName', 'email', 'phone']"
            filterDisplay="menu" -->
        <TabView class="card p-0 overflow-hidden">
            <TabPanel :header="t('body.systemSetting.system_account_tab')">
                <DataTable
                    v-model:filters="filters"
                    v-model:selection="selectedRow"
                    v-model:expandedRows="expandedRows"
                    :globalFilterFields="['status', 'role.name']"
                    :value="User?.item"
                    :rows="User?.limit"
                    :totalRecords="User?.total"
                    :rowsPerPageOptions="[10, 30, 50]"
                    :first="User.limit * User.skip"
                    filterDisplay="menu"
                    dataKey="id"
                    paginator
                    lazy
                    showGridlines
                    stripedRows
                    selectionMode="single"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                    @page="onPage($event)"
                    @row-click="onRowClicked"
                    @filter="onFilter($event)"
                >
                    <template #header>
                        <div class="flex justify-content-between">
                            <IconField iconPosition="left">
                                <InputText
                                    v-model="keyword"
                                    :placeholder="t('body.systemSetting.search_placeholder')"
                                    @input="dbSearch()"
                                />
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                            </IconField>
                            <div class="flex gap-2">
                                <Button
                                    type="button"
                                    icon="pi pi-filter-slash"
                                    outlined
                                    v-tooltip.bottom="t('body.OrderApproval.clear')"
                                    severity="danger"
                                    @click="clearFilter()"
                                />
                                <Button
                                    @click="onClickOpenDlg"
                                    :label="t('body.systemSetting.create_account_button')"
                                />
                            </div>
                        </div>
                    </template>
                    <template #empty>
                        <div class="py-5 my-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
                    </template>
                    <Column field="userName" :header="t('body.systemSetting.username_column')"> </Column>
                    <Column field="fullName" :header="t('body.systemSetting.full_name_column')"></Column>
                    <Column field="email" :header="t('body.systemSetting.email_column')"></Column>
                    <Column
                        field="role.name"
                        filterField="role.name"
                        :header="t('body.systemSetting.role_column')"
                        :showFilterMatchModes="false"
                    >
                        <template #filter="{ filterModel }">
                            <MultiSelect
                                v-model="filterModel.value"
                                :options="filterOptions.role"
                                optionLabel="label"
                                optionValue="value"
                                :maxSelectedLabels="1"
                                selectedItemsLabel="{0} vai trò"
                                :placeholder="t('body.OrderApproval.searchPlaceholder')"
                                showClear
                            ></MultiSelect>
                        </template>
                    </Column>
                    <Column
                        field="status"
                        filterField="status"
                        :header="t('body.systemSetting.status_column')"
                        :showFilterMatchModes="false"
                    >
                        <template #body="{ data }">
                            <span>
                                {{
                                    data.status === "A" ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status')
                                }}
                            </span>
                        </template>
                        <template #filter="{ filterModel }">
                            <Dropdown
                                v-model="filterModel.value"
                                :options="filterOptions.status"
                                optionLabel="label"
                                optionValue="value"
                                :placeholder="t('body.systemSetting.status_column')"
                                showClear
                            ></Dropdown>
                        </template>
                    </Column>
                    <template #expansion>
                        <TabView
                            @tab-change="onChangeTab"
                            class="card p-0 overflow-hidden"
                        >
                            <TabPanel :header="t('body.systemSetting.user_info_section')">
                                <UserDetail
                                    :user="userResponse || {}"
                                    @save="onSaveInfoUser"
                                    :roleOption="roles"
                                    :loading="userResponse != null ? false : true"
                                    :orgStrct="OrgStructData"
                                />
                                <!-- userResponse ? true : false -->
                            </TabPanel>
                            <TabPanel :header="t('body.systemSetting.user_management_title')">
                                <Permission
                                    :user="userResponse || {}"
                                    :loading="userResponse != null ? false : true"
                                />
                            </TabPanel>
                            <TabPanel v-if="0" :header="t('body.systemSetting.customers')">
                                <CustomerInCharge></CustomerInCharge>
                            </TabPanel>
                        </TabView>
                    </template>
                </DataTable>
            </TabPanel>
            <TabPanel v-if="1" :header="t('body.systemSetting.customer_account_tab')">
                <CustomerData></CustomerData>
            </TabPanel>
        </TabView>

        <Loading v-if="loading"></Loading>
        <Dialog
            v-model:visible="visible"
            class="w-10 md:w-6"
            :header="t('body.systemSetting.dialog_title_add_user')"
            modal
        >
            <div>
                <div class="font-bold mb-2">{{ t('body.systemSetting.user_info_section') }}</div>
                <div class="card p-3 mb-3">
                    <div class="grid mb-3">
                        <div class="col-12 md:col-5 flex flex-column gap-2 pb-0">
                            <label for="" class="font-semibold"
                                >{{ t('body.systemSetting.full_name_label') }} <sup class="text-red-500">*</sup></label
                            >
                            <InputText :invalid="isInvld(errorMsg.fullName)" v-model="createUser.fullName" />
                            <small class="text-red-500" v-if="errorMsg.fullName">{{ errorMsg.fullName }}</small>
                        </div>
                        <div class="col-12 md:col-3 flex flex-column gap-2 pb-0">
                            <label for="" class="font-semibold"
                                >{{ t('body.systemSetting.phone_label') }} <sup class="text-red-500">*</sup></label
                            >
                            <InputText :invalid="isInvld(errorMsg.phone)" v-model="createUser.phone" />
                            <small class="text-red-500" v-if="errorMsg.phone">{{ errorMsg.phone }}</small>
                        </div>
                        <div class="col-12 md:col-4 flex flex-column gap-2 pb-0">
                            <label for="" class="font-semibold"
                                >{{ t('body.systemSetting.email_label') }} <sup class="text-red-500">*</sup></label
                            >
                            <InputText :invalid="isInvld(errorMsg.email)" v-model="createUser.email" />
                            <small class="text-red-500" v-if="errorMsg.email">{{ errorMsg.email }}</small>
                        </div>
                    </div>
                    <div class="grid mb-3">
                        <div class="col-12 md:col-4 flex flex-column gap-2 pb-0">
                            <label for="" class="font-semibold"
                                >{{ t('body.systemSetting.role_label') }}<sup class="text-red-500">*</sup></label
                            >
                            <Dropdown v-model="createUser.roleId" :options="roles" optionLabel="name" optionValue="id" :invalid="isInvld(errorMsg.roleId)"></Dropdown>
                            <small class="text-red-500" v-if="errorMsg.roleId">{{ errorMsg.roleId }}</small>
                        </div>
                        <div class="col-12 md:col-4 flex flex-column gap-2 pb-0">
                            <label for="" class="font-semibold"
                                >{{ t('body.systemSetting.department_label') }} <sup class="text-red-500">*</sup></label
                            >
                            <TreeSelect v-model="createUser.selectionOrgStrct" :options="OrgStructData" @node-select="onSelectOrgStruct" :invalid="errorMsg.organizationId ? true : false"></TreeSelect>
                            <small class="text-red-500" v-if="errorMsg.organizationId">{{ errorMsg.organizationId }}</small>
                        </div>
                        <div
                            v-if="createUser.organizationId"
                            class="col-12 md:col-4 flex flex-column gap-2 pb-0"
                        >
                            <label for="" class="font-semibold select-none mt-1"
                                >&nbsp;</label
                            >
                            <div style="height: 31px" class="flex align-items-center">
                                <Checkbox
                                    v-model="createUser.isManager"
                                    inputId="isManager"
                                    binary
                                    @change="onChangeMngCheckBox"
                                ></Checkbox>
                                <label for="isManager" class="ml-2">{{ t('body.systemSetting.direct_manager') }}</label>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="font-bold mb-2">{{ t('body.systemSetting.account_info_section') }}</div>
                <div class="card p-3 mb-3">
                    <div class="grid">
                        <div class="col-5">
                            <div class="grid mb-3">
                                <div class="col-12 flex flex-column gap-2 pb-0">
                                    <label for="" class="font-semibold">{{ t('body.systemSetting.username_label') }}
                                        <sup class="text-red-500">*</sup></label
                                    >
                                    <InputText :invalid="isInvld(errorMsg.userName)" v-model="createUser.userName" />
                                    <small class="text-red-500" v-if="errorMsg.userName">{{ errorMsg.userName }}</small>
                                </div>
                                <div class="col-12 flex flex-column gap-2 pb-0">
                                    <label for="" class="font-semibold">{{ t('body.systemSetting.password_label') }}<sup class="text-red-500">*</sup></label
                                    >
                                    <Password id="password" :invalid="isInvld(errorMsg.password)" v-model="createUser.password" toggleMask class="w-full" />
                                    <small class="text-red-500" v-if="errorMsg.password">{{ errorMsg.password }}</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-7">
                            <div>
                                <div>
                                    <i class="pi pi-lock mr-3"></i>{{ t('body.systemSetting.password_requirements_note') }}
                                </div>
                                <div class="mt-2">
                                    <div>
                                        <i class="fa-solid fa-triangle-exclamation mr-3"></i>{{ t('body.systemSetting.password_requirements_heading') }}
                                    </div>
                                    <ul class="mt-2">
                                        <li class="mb-2">{{ t('body.systemSetting.password_requirement_8_chars') }}</li>
                                        <li class="mb-2">{{ t('body.systemSetting.password_requirement_upper_lower') }}</li>
                                        <li class="mb-2">{{ t('body.systemSetting.password_requirement_number') }}</li>
                                        <li class="mb-2">{{ t('body.systemSetting.password_requirement_special_char') }}</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="flex flex-column gap-2 w-10rem">
                    <label for="" class="font-semibold">{{ t('body.systemSetting.status_label') }} </label>
                    <div class="flex-grow-1 flex align-items-center">
                        <InputSwitch v-model="createUser.status" class="my-auto" falseValue="I" trueValue="A"></InputSwitch>
                    </div>
                </div>
            </div>
            <template #footer>
                <Button icon="pi pi-times" :label="t('body.OrderList.close')" severity="secondary" @click="visible = false"/>
                <Button :loading="loadingBtn" @click="onClickCreateUser" icon="pi pi-save" :label="t('body.promotion.save_button')"/>
            </template>
        </Dialog>
    </div>
</template>

<style>
#password > input {
    width: 100%;
}
</style>
